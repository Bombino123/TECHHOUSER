using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class UpdateTranslator
{
	private class RelationshipConstraintValidator
	{
		private class DirectionalRelationship : IEquatable<DirectionalRelationship>
		{
			internal readonly EntityKey ToEntityKey;

			internal readonly AssociationEndMember FromEnd;

			internal readonly AssociationEndMember ToEnd;

			internal readonly IEntityStateEntry StateEntry;

			internal readonly AssociationSet AssociationSet;

			private DirectionalRelationship _equivalenceSetLinkedListNext;

			private readonly int _hashCode;

			internal DirectionalRelationship(EntityKey toEntityKey, AssociationEndMember fromEnd, AssociationEndMember toEnd, AssociationSet associationSet, IEntityStateEntry stateEntry)
			{
				ToEntityKey = toEntityKey;
				FromEnd = fromEnd;
				ToEnd = toEnd;
				AssociationSet = associationSet;
				StateEntry = stateEntry;
				_equivalenceSetLinkedListNext = this;
				_hashCode = toEntityKey.GetHashCode() ^ fromEnd.GetHashCode() ^ toEnd.GetHashCode() ^ associationSet.GetHashCode();
			}

			internal void AddToEquivalenceSet(DirectionalRelationship other)
			{
				DirectionalRelationship equivalenceSetLinkedListNext = _equivalenceSetLinkedListNext;
				_equivalenceSetLinkedListNext = other;
				other._equivalenceSetLinkedListNext = equivalenceSetLinkedListNext;
			}

			internal IEnumerable<DirectionalRelationship> GetEquivalenceSet()
			{
				DirectionalRelationship current = this;
				do
				{
					yield return current;
					current = current._equivalenceSetLinkedListNext;
				}
				while (current != this);
			}

			internal void GetCountsInEquivalenceSet(out int addedCount, out int deletedCount)
			{
				addedCount = 0;
				deletedCount = 0;
				DirectionalRelationship directionalRelationship = this;
				do
				{
					if (directionalRelationship.StateEntry.State == EntityState.Added)
					{
						addedCount++;
					}
					else if (directionalRelationship.StateEntry.State == EntityState.Deleted)
					{
						deletedCount++;
					}
					directionalRelationship = directionalRelationship._equivalenceSetLinkedListNext;
				}
				while (directionalRelationship != this);
			}

			public override int GetHashCode()
			{
				return _hashCode;
			}

			public bool Equals(DirectionalRelationship other)
			{
				if (this == other)
				{
					return true;
				}
				if (other == null)
				{
					return false;
				}
				if (ToEntityKey != other.ToEntityKey)
				{
					return false;
				}
				if (AssociationSet != other.AssociationSet)
				{
					return false;
				}
				if (ToEnd != other.ToEnd)
				{
					return false;
				}
				if (FromEnd != other.FromEnd)
				{
					return false;
				}
				return true;
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as DirectionalRelationship);
			}

			public override string ToString()
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}.{1}-->{2}: {3}", AssociationSet.Name, FromEnd.Name, ToEnd.Name, StringUtil.BuildDelimitedList(ToEntityKey.EntityKeyValues, null, null));
			}
		}

		private readonly Dictionary<DirectionalRelationship, DirectionalRelationship> m_existingRelationships;

		private readonly Dictionary<DirectionalRelationship, IEntityStateEntry> m_impliedRelationships;

		private readonly Dictionary<EntitySet, List<AssociationSet>> m_referencingRelationshipSets;

		internal RelationshipConstraintValidator()
		{
			m_existingRelationships = new Dictionary<DirectionalRelationship, DirectionalRelationship>(EqualityComparer<DirectionalRelationship>.Default);
			m_impliedRelationships = new Dictionary<DirectionalRelationship, IEntityStateEntry>(EqualityComparer<DirectionalRelationship>.Default);
			m_referencingRelationshipSets = new Dictionary<EntitySet, List<AssociationSet>>(EqualityComparer<EntitySet>.Default);
		}

		internal void RegisterEntity(IEntityStateEntry stateEntry)
		{
			if (EntityState.Added != stateEntry.State && EntityState.Deleted != stateEntry.State)
			{
				return;
			}
			EntityKey entityKey = stateEntry.EntityKey;
			EntitySet entitySet = (EntitySet)stateEntry.EntitySet;
			EntityType otherType = ((EntityState.Added == stateEntry.State) ? GetEntityType(stateEntry.CurrentValues) : GetEntityType(stateEntry.OriginalValues));
			foreach (AssociationSet referencingAssociationSet in GetReferencingAssociationSets(entitySet))
			{
				ReadOnlyMetadataCollection<AssociationSetEnd> associationSetEnds = referencingAssociationSet.AssociationSetEnds;
				foreach (AssociationSetEnd item in associationSetEnds)
				{
					foreach (AssociationSetEnd item2 in associationSetEnds)
					{
						if (item2.CorrespondingAssociationEndMember != item.CorrespondingAssociationEndMember && item2.EntitySet.EdmEquals(entitySet) && MetadataHelper.GetLowerBoundOfMultiplicity(item.CorrespondingAssociationEndMember.RelationshipMultiplicity) != 0 && MetadataHelper.GetEntityTypeForEnd(item2.CorrespondingAssociationEndMember).IsAssignableFrom(otherType))
						{
							DirectionalRelationship key = new DirectionalRelationship(entityKey, item.CorrespondingAssociationEndMember, item2.CorrespondingAssociationEndMember, referencingAssociationSet, stateEntry);
							m_impliedRelationships.Add(key, stateEntry);
						}
					}
				}
			}
		}

		private static EntityType GetEntityType(DbDataRecord dbDataRecord)
		{
			return (EntityType)(dbDataRecord as IExtendedDataRecord).DataRecordInfo.RecordType.EdmType;
		}

		internal void RegisterAssociation(AssociationSet associationSet, IExtendedDataRecord record, IEntityStateEntry stateEntry)
		{
			Dictionary<string, EntityKey> dictionary = new Dictionary<string, EntityKey>(StringComparer.Ordinal);
			foreach (FieldMetadata fieldMetadatum in record.DataRecordInfo.FieldMetadata)
			{
				string name = fieldMetadatum.FieldType.Name;
				EntityKey value = (EntityKey)record.GetValue(fieldMetadatum.Ordinal);
				dictionary.Add(name, value);
			}
			ReadOnlyMetadataCollection<AssociationSetEnd> associationSetEnds = associationSet.AssociationSetEnds;
			foreach (AssociationSetEnd item in associationSetEnds)
			{
				foreach (AssociationSetEnd item2 in associationSetEnds)
				{
					if (item2.CorrespondingAssociationEndMember != item.CorrespondingAssociationEndMember)
					{
						DirectionalRelationship relationship = new DirectionalRelationship(dictionary[item2.CorrespondingAssociationEndMember.Name], item.CorrespondingAssociationEndMember, item2.CorrespondingAssociationEndMember, associationSet, stateEntry);
						AddExistingRelationship(relationship);
					}
				}
			}
		}

		internal void ValidateConstraints()
		{
			foreach (KeyValuePair<DirectionalRelationship, IEntityStateEntry> impliedRelationship in m_impliedRelationships)
			{
				DirectionalRelationship key = impliedRelationship.Key;
				IEntityStateEntry value = impliedRelationship.Value;
				int num = GetDirectionalRelationshipCountDelta(key);
				if (EntityState.Deleted == value.State)
				{
					num = -num;
				}
				int lowerBoundOfMultiplicity = MetadataHelper.GetLowerBoundOfMultiplicity(key.FromEnd.RelationshipMultiplicity);
				int? upperBoundOfMultiplicity = MetadataHelper.GetUpperBoundOfMultiplicity(key.FromEnd.RelationshipMultiplicity);
				int num2 = (upperBoundOfMultiplicity.HasValue ? upperBoundOfMultiplicity.Value : num);
				if (num < lowerBoundOfMultiplicity || num > num2)
				{
					throw EntityUtil.UpdateRelationshipCardinalityConstraintViolation(key.AssociationSet.Name, lowerBoundOfMultiplicity, upperBoundOfMultiplicity, TypeHelpers.GetFullName(key.ToEntityKey.EntityContainerName, key.ToEntityKey.EntitySetName), num, key.FromEnd.Name, value);
				}
			}
			foreach (DirectionalRelationship key2 in m_existingRelationships.Keys)
			{
				key2.GetCountsInEquivalenceSet(out var addedCount, out var deletedCount);
				int num3 = Math.Abs(addedCount - deletedCount);
				int lowerBoundOfMultiplicity2 = MetadataHelper.GetLowerBoundOfMultiplicity(key2.FromEnd.RelationshipMultiplicity);
				int? upperBoundOfMultiplicity2 = MetadataHelper.GetUpperBoundOfMultiplicity(key2.FromEnd.RelationshipMultiplicity);
				if (upperBoundOfMultiplicity2.HasValue)
				{
					EntityState? entityState = null;
					int? num4 = null;
					if (addedCount > upperBoundOfMultiplicity2.Value)
					{
						entityState = EntityState.Added;
						num4 = addedCount;
					}
					else if (deletedCount > upperBoundOfMultiplicity2.Value)
					{
						entityState = EntityState.Deleted;
						num4 = deletedCount;
					}
					if (entityState.HasValue)
					{
						throw new UpdateException(Strings.Update_RelationshipCardinalityViolation(upperBoundOfMultiplicity2.Value, entityState.Value, key2.AssociationSet.ElementType.FullName, key2.FromEnd.Name, key2.ToEnd.Name, num4.Value), null, (from reln in key2.GetEquivalenceSet()
							select reln.StateEntry).Cast<ObjectStateEntry>().Distinct());
					}
				}
				if (1 == num3 && 1 == lowerBoundOfMultiplicity2 && 1 == upperBoundOfMultiplicity2)
				{
					bool flag = addedCount > deletedCount;
					if (!m_impliedRelationships.TryGetValue(key2, out var value2) || (flag && EntityState.Added != value2.State) || (!flag && EntityState.Deleted != value2.State))
					{
						throw EntityUtil.Update(Strings.Update_MissingRequiredEntity(key2.AssociationSet.Name, key2.StateEntry.State, key2.ToEnd.Name), null, key2.StateEntry);
					}
				}
			}
		}

		private int GetDirectionalRelationshipCountDelta(DirectionalRelationship expectedRelationship)
		{
			if (m_existingRelationships.TryGetValue(expectedRelationship, out var value))
			{
				value.GetCountsInEquivalenceSet(out var addedCount, out var deletedCount);
				return addedCount - deletedCount;
			}
			return 0;
		}

		private void AddExistingRelationship(DirectionalRelationship relationship)
		{
			if (m_existingRelationships.TryGetValue(relationship, out var value))
			{
				value.AddToEquivalenceSet(relationship);
			}
			else
			{
				m_existingRelationships.Add(relationship, relationship);
			}
		}

		private IEnumerable<AssociationSet> GetReferencingAssociationSets(EntitySet entitySet)
		{
			if (!m_referencingRelationshipSets.TryGetValue(entitySet, out var value))
			{
				value = new List<AssociationSet>();
				foreach (EntitySetBase baseEntitySet in entitySet.EntityContainer.BaseEntitySets)
				{
					if (!(baseEntitySet is AssociationSet associationSet) || associationSet.ElementType.IsForeignKey)
					{
						continue;
					}
					foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
					{
						if (associationSetEnd.EntitySet.Equals(entitySet))
						{
							value.Add(associationSet);
							break;
						}
					}
				}
				m_referencingRelationshipSets.Add(entitySet, value);
			}
			return value;
		}
	}

	private readonly EntityAdapter _adapter;

	private readonly Dictionary<EntitySetBase, ChangeNode> _changes;

	private readonly Dictionary<EntitySetBase, List<ExtractedStateEntry>> _functionChanges;

	private readonly List<IEntityStateEntry> _stateEntries;

	private readonly Set<EntityKey> _knownEntityKeys;

	private readonly Dictionary<EntityKey, AssociationSet> _requiredEntities;

	private readonly Set<EntityKey> _optionalEntities;

	private readonly Set<EntityKey> _includedValueEntities;

	private readonly IEntityStateManager _stateManager;

	private readonly DbInterceptionContext _interceptionContext;

	private readonly RecordConverter _recordConverter;

	private readonly RelationshipConstraintValidator _constraintValidator;

	private readonly DbProviderServices _providerServices;

	private Dictionary<ModificationFunctionMapping, DbCommandDefinition> _modificationFunctionCommandDefinitions;

	private readonly Dictionary<Tuple<EntitySetBase, StructuralType>, ExtractorMetadata> _extractorMetadata;

	internal readonly IEqualityComparer<CompositeKey> KeyComparer;

	internal MetadataWorkspace MetadataWorkspace => Connection.GetMetadataWorkspace();

	internal virtual KeyManager KeyManager { get; private set; }

	internal ViewLoader ViewLoader => MetadataWorkspace.GetUpdateViewLoader();

	internal RecordConverter RecordConverter => _recordConverter;

	internal virtual EntityConnection Connection => _adapter.Connection;

	internal virtual int? CommandTimeout => _adapter.CommandTimeout;

	public virtual DbInterceptionContext InterceptionContext => _interceptionContext;

	public UpdateTranslator(EntityAdapter adapter)
		: this()
	{
		_stateManager = adapter.Context.ObjectStateManager;
		_interceptionContext = adapter.Context.InterceptionContext;
		_adapter = adapter;
		_providerServices = adapter.Connection.StoreProviderFactory.GetProviderServices();
	}

	protected UpdateTranslator()
	{
		_changes = new Dictionary<EntitySetBase, ChangeNode>();
		_functionChanges = new Dictionary<EntitySetBase, List<ExtractedStateEntry>>();
		_stateEntries = new List<IEntityStateEntry>();
		_knownEntityKeys = new Set<EntityKey>();
		_requiredEntities = new Dictionary<EntityKey, AssociationSet>();
		_optionalEntities = new Set<EntityKey>();
		_includedValueEntities = new Set<EntityKey>();
		_interceptionContext = new DbInterceptionContext();
		_recordConverter = new RecordConverter(this);
		_constraintValidator = new RelationshipConstraintValidator();
		_extractorMetadata = new Dictionary<Tuple<EntitySetBase, StructuralType>, ExtractorMetadata>();
		KeyComparer = CompositeKey.CreateComparer(KeyManager = new KeyManager());
	}

	internal void RegisterReferentialConstraints(IEntityStateEntry stateEntry)
	{
		if (stateEntry.IsRelationship)
		{
			AssociationSet associationSet = (AssociationSet)stateEntry.EntitySet;
			if (0 >= associationSet.ElementType.ReferentialConstraints.Count)
			{
				return;
			}
			DbDataRecord dbDataRecord = ((stateEntry.State == EntityState.Added) ? stateEntry.CurrentValues : stateEntry.OriginalValues);
			{
				foreach (ReferentialConstraint referentialConstraint in associationSet.ElementType.ReferentialConstraints)
				{
					EntityKey entityKey = (EntityKey)dbDataRecord[referentialConstraint.FromRole.Name];
					EntityKey entityKey2 = (EntityKey)dbDataRecord[referentialConstraint.ToRole.Name];
					using ReadOnlyMetadataCollection<EdmProperty>.Enumerator enumerator2 = referentialConstraint.FromProperties.GetEnumerator();
					using ReadOnlyMetadataCollection<EdmProperty>.Enumerator enumerator3 = referentialConstraint.ToProperties.GetEnumerator();
					while (enumerator2.MoveNext() && enumerator3.MoveNext())
					{
						int keyMemberCount;
						int keyMemberOffset = GetKeyMemberOffset(referentialConstraint.FromRole, enumerator2.Current, out keyMemberCount);
						int keyMemberCount2;
						int keyMemberOffset2 = GetKeyMemberOffset(referentialConstraint.ToRole, enumerator3.Current, out keyMemberCount2);
						int keyIdentifierForMemberOffset = KeyManager.GetKeyIdentifierForMemberOffset(entityKey, keyMemberOffset, keyMemberCount);
						int keyIdentifierForMemberOffset2 = KeyManager.GetKeyIdentifierForMemberOffset(entityKey2, keyMemberOffset2, keyMemberCount2);
						KeyManager.AddReferentialConstraint(stateEntry, keyIdentifierForMemberOffset2, keyIdentifierForMemberOffset);
					}
				}
				return;
			}
		}
		if (!stateEntry.IsKeyEntry)
		{
			if (stateEntry.State == EntityState.Added || stateEntry.State == EntityState.Modified)
			{
				RegisterEntityReferentialConstraints(stateEntry, currentValues: true);
			}
			if (stateEntry.State == EntityState.Deleted || stateEntry.State == EntityState.Modified)
			{
				RegisterEntityReferentialConstraints(stateEntry, currentValues: false);
			}
		}
	}

	private void RegisterEntityReferentialConstraints(IEntityStateEntry stateEntry, bool currentValues)
	{
		object obj;
		if (!currentValues)
		{
			obj = (IExtendedDataRecord)stateEntry.OriginalValues;
		}
		else
		{
			IExtendedDataRecord currentValues2 = stateEntry.CurrentValues;
			obj = currentValues2;
		}
		IExtendedDataRecord extendedDataRecord = (IExtendedDataRecord)obj;
		EntitySet entitySet = (EntitySet)stateEntry.EntitySet;
		EntityKey entityKey = stateEntry.EntityKey;
		foreach (Tuple<AssociationSet, ReferentialConstraint> foreignKeyDependent in entitySet.ForeignKeyDependents)
		{
			AssociationSet item = foreignKeyDependent.Item1;
			ReferentialConstraint item2 = foreignKeyDependent.Item2;
			if (!MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)item2.ToRole).IsAssignableFrom(extendedDataRecord.DataRecordInfo.RecordType.EdmType))
			{
				continue;
			}
			EntityKey principalKey = null;
			if (!currentValues || !_stateManager.TryGetReferenceKey(entityKey, (AssociationEndMember)item2.FromRole, out principalKey))
			{
				EntityType entityTypeForEnd = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)item2.FromRole);
				bool flag = false;
				object[] array = new object[entityTypeForEnd.KeyMembers.Count];
				int i = 0;
				for (int num = array.Length; i < num; i++)
				{
					EdmProperty value = (EdmProperty)entityTypeForEnd.KeyMembers[i];
					int index = item2.FromProperties.IndexOf(value);
					int ordinal = extendedDataRecord.GetOrdinal(item2.ToProperties[index].Name);
					if (extendedDataRecord.IsDBNull(ordinal))
					{
						flag = true;
						break;
					}
					array[i] = extendedDataRecord.GetValue(ordinal);
				}
				if (!flag)
				{
					EntitySet entitySet2 = item.AssociationSetEnds[item2.FromRole.Name].EntitySet;
					principalKey = ((1 != array.Length) ? new EntityKey(entitySet2, array) : new EntityKey(entitySet2, array[0]));
				}
			}
			if (!(null != principalKey))
			{
				continue;
			}
			if (!_stateManager.TryGetEntityStateEntry(principalKey, out var stateEntry2) && currentValues && KeyManager.TryGetTempKey(principalKey, out var tempKey))
			{
				if (null == tempKey)
				{
					throw EntityUtil.Update(Strings.Update_AmbiguousForeignKey(item2.ToRole.DeclaringType.FullName), null, stateEntry);
				}
				principalKey = tempKey;
			}
			AddValidAncillaryKey(principalKey, _optionalEntities);
			int j = 0;
			for (int count = item2.FromProperties.Count; j < count; j++)
			{
				EdmProperty property = item2.FromProperties[j];
				EdmProperty edmProperty = item2.ToProperties[j];
				int keyMemberCount;
				int keyMemberOffset = GetKeyMemberOffset(item2.FromRole, property, out keyMemberCount);
				int keyIdentifierForMemberOffset = KeyManager.GetKeyIdentifierForMemberOffset(principalKey, keyMemberOffset, keyMemberCount);
				int dependentIdentifier;
				if (entitySet.ElementType.KeyMembers.Contains(edmProperty))
				{
					int keyMemberCount2;
					int keyMemberOffset2 = GetKeyMemberOffset(item2.ToRole, edmProperty, out keyMemberCount2);
					dependentIdentifier = KeyManager.GetKeyIdentifierForMemberOffset(entityKey, keyMemberOffset2, keyMemberCount2);
				}
				else
				{
					dependentIdentifier = KeyManager.GetKeyIdentifierForMember(entityKey, edmProperty.Name, currentValues);
				}
				if (currentValues && stateEntry2 != null && stateEntry2.State == EntityState.Deleted && (stateEntry.State == EntityState.Added || stateEntry.State == EntityState.Modified))
				{
					throw EntityUtil.Update(Strings.Update_InsertingOrUpdatingReferenceToDeletedEntity(item.ElementType.FullName), null, stateEntry, stateEntry2);
				}
				KeyManager.AddReferentialConstraint(stateEntry, dependentIdentifier, keyIdentifierForMemberOffset);
			}
		}
	}

	private static int GetKeyMemberOffset(RelationshipEndMember role, EdmProperty property, out int keyMemberCount)
	{
		EntityType entityType = (EntityType)((RefType)role.TypeUsage.EdmType).ElementType;
		keyMemberCount = entityType.KeyMembers.Count;
		return entityType.KeyMembers.IndexOf(property);
	}

	internal IEnumerable<IEntityStateEntry> GetRelationships(EntityKey entityKey)
	{
		return _stateManager.FindRelationshipsByKey(entityKey);
	}

	internal virtual int Update()
	{
		Dictionary<int, object> identifierValues = new Dictionary<int, object>();
		List<KeyValuePair<PropagatorResult, object>> generatedValues = new List<KeyValuePair<PropagatorResult, object>>();
		IEnumerable<UpdateCommand> enumerable = ProduceCommands();
		UpdateCommand source = null;
		try
		{
			foreach (UpdateCommand item in enumerable)
			{
				long rowsAffected = (source = item).Execute(identifierValues, generatedValues);
				ValidateRowsAffected(rowsAffected, source);
			}
		}
		catch (Exception ex)
		{
			if (ex.RequiresContext())
			{
				throw new UpdateException(Strings.Update_GeneralExecutionException, ex, DetermineStateEntriesFromSource(source).Cast<ObjectStateEntry>().Distinct());
			}
			throw;
		}
		BackPropagateServerGen(generatedValues);
		return AcceptChanges();
	}

	internal virtual async Task<int> UpdateAsync(CancellationToken cancellationToken)
	{
		Dictionary<int, object> identifierValues = new Dictionary<int, object>();
		List<KeyValuePair<PropagatorResult, object>> generatedValues = new List<KeyValuePair<PropagatorResult, object>>();
		IEnumerable<UpdateCommand> enumerable = ProduceCommands();
		UpdateCommand source = null;
		try
		{
			foreach (UpdateCommand item in enumerable)
			{
				source = item;
				ValidateRowsAffected(await item.ExecuteAsync(identifierValues, generatedValues, cancellationToken).WithCurrentCulture(), source);
			}
		}
		catch (Exception ex)
		{
			if (ex.RequiresContext())
			{
				throw new UpdateException(Strings.Update_GeneralExecutionException, ex, DetermineStateEntriesFromSource(source).Cast<ObjectStateEntry>().Distinct());
			}
			throw;
		}
		BackPropagateServerGen(generatedValues);
		return AcceptChanges();
	}

	protected virtual IEnumerable<UpdateCommand> ProduceCommands()
	{
		PullModifiedEntriesFromStateManager();
		PullUnchangedEntriesFromStateManager();
		_constraintValidator.ValidateConstraints();
		KeyManager.ValidateReferentialIntegrityGraphAcyclic();
		IEnumerable<UpdateCommand> first = ProduceDynamicCommands();
		IEnumerable<UpdateCommand> second = ProduceFunctionCommands();
		if (!new UpdateCommandOrderer(first.Concat(second), this).TryTopologicalSort(out var orderedVertices, out var remainder))
		{
			throw DependencyOrderingError(remainder);
		}
		return orderedVertices;
	}

	private void ValidateRowsAffected(long rowsAffected, UpdateCommand source)
	{
		if (rowsAffected == 0L)
		{
			IEnumerable<IEntityStateEntry> source2 = DetermineStateEntriesFromSource(source);
			throw new OptimisticConcurrencyException(Strings.Update_ConcurrencyError(rowsAffected), null, source2.Cast<ObjectStateEntry>().Distinct());
		}
	}

	private IEnumerable<IEntityStateEntry> DetermineStateEntriesFromSource(UpdateCommand source)
	{
		if (source == null)
		{
			return Enumerable.Empty<IEntityStateEntry>();
		}
		return source.GetStateEntries(this);
	}

	private void BackPropagateServerGen(List<KeyValuePair<PropagatorResult, object>> generatedValues)
	{
		foreach (KeyValuePair<PropagatorResult, object> generatedValue in generatedValues)
		{
			if (-1 == generatedValue.Key.Identifier || !KeyManager.TryGetIdentifierOwner(generatedValue.Key.Identifier, out var owner))
			{
				owner = generatedValue.Key;
			}
			object value = generatedValue.Value;
			if (owner.Identifier == -1)
			{
				owner.SetServerGenValue(value);
				continue;
			}
			foreach (int dependent in KeyManager.GetDependents(owner.Identifier))
			{
				if (KeyManager.TryGetIdentifierOwner(dependent, out owner))
				{
					owner.SetServerGenValue(value);
				}
			}
		}
	}

	private int AcceptChanges()
	{
		int num = 0;
		foreach (IEntityStateEntry stateEntry in _stateEntries)
		{
			if (EntityState.Unchanged != stateEntry.State)
			{
				if (_adapter.AcceptChangesDuringUpdate)
				{
					stateEntry.AcceptChanges();
				}
				num++;
			}
		}
		return num;
	}

	private IEnumerable<EntitySetBase> GetDynamicModifiedExtents()
	{
		return _changes.Keys;
	}

	private IEnumerable<EntitySetBase> GetFunctionModifiedExtents()
	{
		return _functionChanges.Keys;
	}

	private IEnumerable<UpdateCommand> ProduceDynamicCommands()
	{
		UpdateCompiler updateCompiler = new UpdateCompiler(this);
		Set<EntitySet> set = new Set<EntitySet>();
		foreach (EntitySetBase dynamicModifiedExtent in GetDynamicModifiedExtents())
		{
			Set<EntitySet> affectedTables = ViewLoader.GetAffectedTables(dynamicModifiedExtent, MetadataWorkspace);
			if (affectedTables.Count == 0)
			{
				throw EntityUtil.Update(Strings.Update_MappingNotFound(dynamicModifiedExtent.Name), null);
			}
			foreach (EntitySet item in affectedTables)
			{
				set.Add(item);
			}
		}
		foreach (EntitySet item2 in set)
		{
			DbQueryCommandTree cqtView = Connection.GetMetadataWorkspace().GetCqtView(item2);
			ChangeNode changeNode = Propagator.Propagate(this, item2, cqtView);
			TableChangeProcessor tableChangeProcessor = new TableChangeProcessor(item2);
			foreach (UpdateCommand item3 in tableChangeProcessor.CompileCommands(changeNode, updateCompiler))
			{
				yield return item3;
			}
		}
	}

	internal DbCommandDefinition GenerateCommandDefinition(ModificationFunctionMapping functionMapping)
	{
		if (_modificationFunctionCommandDefinitions == null)
		{
			_modificationFunctionCommandDefinitions = new Dictionary<ModificationFunctionMapping, DbCommandDefinition>();
		}
		if (!_modificationFunctionCommandDefinitions.TryGetValue(functionMapping, out var value))
		{
			TypeUsage resultType = null;
			if (functionMapping.ResultBindings != null && functionMapping.ResultBindings.Count > 0)
			{
				List<EdmProperty> list = new List<EdmProperty>(functionMapping.ResultBindings.Count);
				foreach (ModificationFunctionResultBinding resultBinding in functionMapping.ResultBindings)
				{
					list.Add(new EdmProperty(resultBinding.ColumnName, resultBinding.Property.TypeUsage));
				}
				resultType = TypeUsage.Create(new CollectionType(new RowType(list)));
			}
			IEnumerable<KeyValuePair<string, TypeUsage>> parameters = functionMapping.Function.Parameters.Select((FunctionParameter paramInfo) => new KeyValuePair<string, TypeUsage>(paramInfo.Name, paramInfo.TypeUsage));
			DbFunctionCommandTree commandTree = new DbFunctionCommandTree(MetadataWorkspace, DataSpace.SSpace, functionMapping.Function, resultType, parameters);
			value = _providerServices.CreateCommandDefinition(commandTree, _interceptionContext);
			_modificationFunctionCommandDefinitions.Add(functionMapping, value);
		}
		return value;
	}

	private IEnumerable<UpdateCommand> ProduceFunctionCommands()
	{
		foreach (EntitySetBase functionModifiedExtent in GetFunctionModifiedExtents())
		{
			ModificationFunctionMappingTranslator translator = ViewLoader.GetFunctionMappingTranslator(functionModifiedExtent, MetadataWorkspace);
			if (translator == null)
			{
				continue;
			}
			foreach (ExtractedStateEntry extentFunctionModification in GetExtentFunctionModifications(functionModifiedExtent))
			{
				FunctionUpdateCommand functionUpdateCommand = translator.Translate(this, extentFunctionModification);
				if (functionUpdateCommand != null)
				{
					yield return functionUpdateCommand;
				}
			}
		}
	}

	internal ExtractorMetadata GetExtractorMetadata(EntitySetBase entitySetBase, StructuralType type)
	{
		Tuple<EntitySetBase, StructuralType> key = Tuple.Create(entitySetBase, type);
		if (!_extractorMetadata.TryGetValue(key, out var value))
		{
			value = new ExtractorMetadata(entitySetBase, type, this);
			_extractorMetadata.Add(key, value);
		}
		return value;
	}

	private UpdateException DependencyOrderingError(IEnumerable<UpdateCommand> remainder)
	{
		HashSet<IEntityStateEntry> hashSet = new HashSet<IEntityStateEntry>();
		foreach (UpdateCommand item in remainder)
		{
			hashSet.UnionWith(item.GetStateEntries(this));
		}
		throw new UpdateException(Strings.Update_ConstraintCycle, null, hashSet.Cast<ObjectStateEntry>().Distinct());
	}

	internal DbCommand CreateCommand(DbModificationCommandTree commandTree)
	{
		try
		{
			return new InterceptableDbCommand(_providerServices.CreateCommand(commandTree, _interceptionContext), _interceptionContext);
		}
		catch (Exception ex)
		{
			if (ex.RequiresContext())
			{
				throw new EntityCommandCompilationException(Strings.EntityClient_CommandDefinitionPreparationFailed, ex);
			}
			throw;
		}
	}

	internal void SetParameterValue(DbParameter parameter, TypeUsage typeUsage, object value)
	{
		_providerServices.SetParameterValue(parameter, typeUsage, value);
	}

	private void PullModifiedEntriesFromStateManager()
	{
		foreach (IEntityStateEntry entityStateEntry in _stateManager.GetEntityStateEntries(EntityState.Added))
		{
			if (!entityStateEntry.IsRelationship && !entityStateEntry.IsKeyEntry)
			{
				KeyManager.RegisterKeyValueForAddedEntity(entityStateEntry);
			}
		}
		foreach (IEntityStateEntry entityStateEntry2 in _stateManager.GetEntityStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified))
		{
			RegisterReferentialConstraints(entityStateEntry2);
		}
		foreach (IEntityStateEntry entityStateEntry3 in _stateManager.GetEntityStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified))
		{
			LoadStateEntry(entityStateEntry3);
		}
	}

	private void PullUnchangedEntriesFromStateManager()
	{
		foreach (KeyValuePair<EntityKey, AssociationSet> requiredEntity in _requiredEntities)
		{
			EntityKey key = requiredEntity.Key;
			if (!_knownEntityKeys.Contains(key))
			{
				if (!_stateManager.TryGetEntityStateEntry(key, out var stateEntry) || stateEntry.IsKeyEntry)
				{
					throw EntityUtil.Update(Strings.Update_MissingEntity(requiredEntity.Value.Name, TypeHelpers.GetFullName(key.EntityContainerName, key.EntitySetName)), null);
				}
				LoadStateEntry(stateEntry);
			}
		}
		foreach (EntityKey optionalEntity in _optionalEntities)
		{
			if (!_knownEntityKeys.Contains(optionalEntity) && _stateManager.TryGetEntityStateEntry(optionalEntity, out var stateEntry2) && !stateEntry2.IsKeyEntry)
			{
				LoadStateEntry(stateEntry2);
			}
		}
		foreach (EntityKey includedValueEntity in _includedValueEntities)
		{
			if (!_knownEntityKeys.Contains(includedValueEntity) && _stateManager.TryGetEntityStateEntry(includedValueEntity, out var stateEntry3))
			{
				_recordConverter.ConvertCurrentValuesToPropagatorResult(stateEntry3, ModifiedPropertiesBehavior.NoneModified);
			}
		}
	}

	private void ValidateAndRegisterStateEntry(IEntityStateEntry stateEntry)
	{
		EntitySetBase entitySet = stateEntry.EntitySet;
		if (entitySet == null)
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidStateEntry, 1, null);
		}
		EntityKey entityKey = stateEntry.EntityKey;
		IExtendedDataRecord extendedDataRecord = null;
		if (((EntityState.Unchanged | EntityState.Added | EntityState.Modified) & stateEntry.State) != 0)
		{
			extendedDataRecord = stateEntry.CurrentValues;
			ValidateRecord(entitySet, extendedDataRecord);
		}
		if (((EntityState.Unchanged | EntityState.Deleted | EntityState.Modified) & stateEntry.State) != 0)
		{
			extendedDataRecord = (IExtendedDataRecord)stateEntry.OriginalValues;
			ValidateRecord(entitySet, extendedDataRecord);
		}
		if (entitySet is AssociationSet associationSet)
		{
			AssociationSetMetadata associationSetMetadata = ViewLoader.GetAssociationSetMetadata(associationSet, MetadataWorkspace);
			if (associationSetMetadata.HasEnds)
			{
				foreach (FieldMetadata fieldMetadatum in extendedDataRecord.DataRecordInfo.FieldMetadata)
				{
					EntityKey key = (EntityKey)extendedDataRecord.GetValue(fieldMetadatum.Ordinal);
					AssociationEndMember element = (AssociationEndMember)fieldMetadatum.FieldType;
					if (associationSetMetadata.RequiredEnds.Contains(element))
					{
						if (!_requiredEntities.ContainsKey(key))
						{
							_requiredEntities.Add(key, associationSet);
						}
					}
					else if (associationSetMetadata.OptionalEnds.Contains(element))
					{
						AddValidAncillaryKey(key, _optionalEntities);
					}
					else if (associationSetMetadata.IncludedValueEnds.Contains(element))
					{
						AddValidAncillaryKey(key, _includedValueEntities);
					}
				}
			}
			_constraintValidator.RegisterAssociation(associationSet, extendedDataRecord, stateEntry);
		}
		else
		{
			_constraintValidator.RegisterEntity(stateEntry);
		}
		_stateEntries.Add(stateEntry);
		if ((object)entityKey != null)
		{
			_knownEntityKeys.Add(entityKey);
		}
	}

	private void AddValidAncillaryKey(EntityKey key, Set<EntityKey> keySet)
	{
		if (_stateManager.TryGetEntityStateEntry(key, out var stateEntry) && !stateEntry.IsKeyEntry && stateEntry.State == EntityState.Unchanged)
		{
			keySet.Add(key);
		}
	}

	private void ValidateRecord(EntitySetBase extent, IExtendedDataRecord record)
	{
		DataRecordInfo dataRecordInfo;
		if (record == null || (dataRecordInfo = record.DataRecordInfo) == null || dataRecordInfo.RecordType == null)
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidStateEntry, 2, null);
		}
		VerifyExtent(MetadataWorkspace, extent);
	}

	private static void VerifyExtent(MetadataWorkspace workspace, EntitySetBase extent)
	{
		EntityContainer entityContainer = extent.EntityContainer;
		EntityContainer entityContainer2 = null;
		if (entityContainer != null)
		{
			workspace.TryGetEntityContainer(entityContainer.Name, entityContainer.DataSpace, out entityContainer2);
		}
		if (entityContainer == null || entityContainer2 == null || entityContainer != entityContainer2)
		{
			throw EntityUtil.Update(Strings.Update_WorkspaceMismatch, null);
		}
	}

	private void LoadStateEntry(IEntityStateEntry stateEntry)
	{
		ValidateAndRegisterStateEntry(stateEntry);
		ExtractedStateEntry item = new ExtractedStateEntry(this, stateEntry);
		EntitySetBase entitySet = stateEntry.EntitySet;
		if (ViewLoader.GetFunctionMappingTranslator(entitySet, MetadataWorkspace) == null)
		{
			ChangeNode extentModifications = GetExtentModifications(entitySet);
			if (item.Original != null)
			{
				extentModifications.Deleted.Add(item.Original);
			}
			if (item.Current != null)
			{
				extentModifications.Inserted.Add(item.Current);
			}
		}
		else
		{
			GetExtentFunctionModifications(entitySet).Add(item);
		}
	}

	internal ChangeNode GetExtentModifications(EntitySetBase extent)
	{
		if (!_changes.TryGetValue(extent, out var value))
		{
			value = new ChangeNode(TypeUsage.Create(extent.ElementType));
			_changes.Add(extent, value);
		}
		return value;
	}

	internal List<ExtractedStateEntry> GetExtentFunctionModifications(EntitySetBase extent)
	{
		if (!_functionChanges.TryGetValue(extent, out var value))
		{
			value = new List<ExtractedStateEntry>();
			_functionChanges.Add(extent, value);
		}
		return value;
	}
}
