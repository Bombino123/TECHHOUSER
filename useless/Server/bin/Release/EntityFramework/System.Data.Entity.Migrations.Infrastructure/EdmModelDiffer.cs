using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Xml.Linq;

namespace System.Data.Entity.Migrations.Infrastructure;

internal class EdmModelDiffer
{
	private class ModelMetadata
	{
		public EdmItemCollection EdmItemCollection { get; set; }

		public StoreItemCollection StoreItemCollection { get; set; }

		public EntityContainerMapping EntityContainerMapping { get; set; }

		public EntityContainer StoreEntityContainer { get; set; }

		public DbProviderManifest ProviderManifest { get; set; }

		public DbProviderInfo ProviderInfo { get; set; }
	}

	private static readonly PrimitiveTypeKind[] _validIdentityTypes = new PrimitiveTypeKind[6]
	{
		PrimitiveTypeKind.Byte,
		PrimitiveTypeKind.Decimal,
		PrimitiveTypeKind.Guid,
		PrimitiveTypeKind.Int16,
		PrimitiveTypeKind.Int32,
		PrimitiveTypeKind.Int64
	};

	private static readonly DynamicEqualityComparer<ForeignKeyOperation> _foreignKeyEqualityComparer = new DynamicEqualityComparer<ForeignKeyOperation>((ForeignKeyOperation fk1, ForeignKeyOperation fk2) => fk1.Name.EqualsOrdinal(fk2.Name));

	private static readonly DynamicEqualityComparer<IndexOperation> _indexEqualityComparer = new DynamicEqualityComparer<IndexOperation>((IndexOperation i1, IndexOperation i2) => i1.Name.EqualsOrdinal(i2.Name) && i1.Table.EqualsOrdinal(i2.Table));

	private ModelMetadata _source;

	private ModelMetadata _target;

	public ICollection<MigrationOperation> Diff(XDocument sourceModel, XDocument targetModel, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator = null, MigrationSqlGenerator migrationSqlGenerator = null, string sourceModelVersion = null, string targetModelVersion = null)
	{
		if (sourceModel == targetModel || XNode.DeepEquals((XNode)(object)sourceModel, (XNode)(object)targetModel))
		{
			return new MigrationOperation[0];
		}
		StorageMappingItemCollection storageMappingItemCollection = sourceModel.GetStorageMappingItemCollection(out var providerInfo);
		ModelMetadata source = new ModelMetadata
		{
			EdmItemCollection = storageMappingItemCollection.EdmItemCollection,
			StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
			StoreEntityContainer = storageMappingItemCollection.StoreItemCollection.GetItems<EntityContainer>().Single(),
			EntityContainerMapping = storageMappingItemCollection.GetItems<EntityContainerMapping>().Single(),
			ProviderManifest = GetProviderManifest(providerInfo),
			ProviderInfo = providerInfo
		};
		storageMappingItemCollection = targetModel.GetStorageMappingItemCollection(out providerInfo);
		ModelMetadata target = new ModelMetadata
		{
			EdmItemCollection = storageMappingItemCollection.EdmItemCollection,
			StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
			StoreEntityContainer = storageMappingItemCollection.StoreItemCollection.GetItems<EntityContainer>().Single(),
			EntityContainerMapping = storageMappingItemCollection.GetItems<EntityContainerMapping>().Single(),
			ProviderManifest = GetProviderManifest(providerInfo),
			ProviderInfo = providerInfo
		};
		return Diff(source, target, modificationCommandTreeGenerator, migrationSqlGenerator, sourceModelVersion, targetModelVersion);
	}

	private ICollection<MigrationOperation> Diff(ModelMetadata source, ModelMetadata target, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator, string sourceModelVersion = null, string targetModelVersion = null)
	{
		_source = source;
		_target = target;
		List<Tuple<EntityType, EntityType>> entityTypePairs = FindEntityTypePairs().ToList();
		List<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs = FindMappingFragmentPairs(entityTypePairs).ToList();
		List<Tuple<AssociationType, AssociationType>> list = FindAssociationTypePairs(entityTypePairs).ToList();
		List<Tuple<EntitySet, EntitySet>> tablePairs = FindTablePairs(mappingFragmentPairs, list).ToList();
		list.AddRange(FindStoreOnlyAssociationTypePairs(list, tablePairs));
		List<RenameTableOperation> renameTableOperations = FindRenamedTables(tablePairs).ToList();
		List<RenameColumnOperation> list2 = FindRenamedColumns(mappingFragmentPairs, list).ToList();
		List<AddColumnOperation> second = FindAddedColumns(tablePairs, list2).ToList();
		List<DropColumnOperation> second2 = FindDroppedColumns(tablePairs, list2).ToList();
		List<AlterColumnOperation> list3 = FindAlteredColumns(tablePairs, list2).ToList();
		List<DropColumnOperation> second3 = FindOrphanedColumns(tablePairs, list2).ToList();
		List<MoveTableOperation> second4 = FindMovedTables(tablePairs).ToList();
		List<CreateTableOperation> second5 = FindAddedTables(tablePairs).ToList();
		List<DropTableOperation> second6 = FindDroppedTables(tablePairs).ToList();
		List<AlterTableOperation> second7 = FindAlteredTables(tablePairs).ToList();
		List<MigrationOperation> source2 = FindAlteredPrimaryKeys(tablePairs, list2, list3).ToList();
		List<AddForeignKeyOperation> source3 = FindAddedForeignKeys(list, list2).Concat(source2.OfType<AddForeignKeyOperation>()).ToList();
		List<DropForeignKeyOperation> source4 = FindDroppedForeignKeys(list, list2).Concat(source2.OfType<DropForeignKeyOperation>()).ToList();
		List<CreateProcedureOperation> second8 = FindAddedModificationFunctions(modificationCommandTreeGenerator, migrationSqlGenerator).ToList();
		List<AlterProcedureOperation> second9 = FindAlteredModificationFunctions(modificationCommandTreeGenerator, migrationSqlGenerator).ToList();
		List<DropProcedureOperation> second10 = FindDroppedModificationFunctions().ToList();
		List<RenameProcedureOperation> second11 = FindRenamedModificationFunctions().ToList();
		List<MoveProcedureOperation> second12 = FindMovedModificationFunctions().ToList();
		List<ConsolidatedIndex> sourceIndexes = ((string.IsNullOrWhiteSpace(sourceModelVersion) || string.Compare(sourceModelVersion.Substring(0, 3), "6.1", StringComparison.Ordinal) >= 0) ? FindSourceIndexes(tablePairs) : BuildLegacyIndexes(source)).ToList();
		List<ConsolidatedIndex> targetIndexes = ((string.IsNullOrWhiteSpace(targetModelVersion) || string.Compare(targetModelVersion.Substring(0, 3), "6.1", StringComparison.Ordinal) >= 0) ? FindTargetIndexes() : BuildLegacyIndexes(target)).ToList();
		List<CreateIndexOperation> list4 = FindAddedIndexes(sourceIndexes, targetIndexes, list3, list2).ToList();
		List<DropIndexOperation> list5 = FindDroppedIndexes(sourceIndexes, targetIndexes, list3, list2).ToList();
		return Enumerable.Concat(second: HandleTransitiveRenameDependencies(FindRenamedIndexes(list4, list5, list3, list2).ToList()), first: ((IEnumerable<MigrationOperation>)HandleTransitiveRenameDependencies(renameTableOperations)).Concat((IEnumerable<MigrationOperation>)second4).Concat(source4.Distinct(_foreignKeyEqualityComparer)).Concat(list5.Distinct(_indexEqualityComparer))
			.Concat(second3)
			.Concat(HandleTransitiveRenameDependencies(list2))).Concat(source2.OfType<DropPrimaryKeyOperation>()).Concat(second5)
			.Concat(second7)
			.Concat(second)
			.Concat(list3)
			.Concat(source2.OfType<AddPrimaryKeyOperation>())
			.Concat(list4.Distinct(_indexEqualityComparer))
			.Concat(source3.Distinct(_foreignKeyEqualityComparer))
			.Concat(second2)
			.Concat(second6)
			.Concat(second8)
			.Concat(second12)
			.Concat(second11)
			.Concat(second9)
			.Concat(second10)
			.ToList();
	}

	private static IEnumerable<ConsolidatedIndex> BuildLegacyIndexes(ModelMetadata modelMetadata)
	{
		foreach (AssociationType associationType in modelMetadata.StoreItemCollection.GetItems<AssociationType>())
		{
			string name = IndexOperation.BuildDefaultName(associationType.Constraint.ToProperties.Select((EdmProperty p) => p.Name));
			string schemaQualifiedName = GetSchemaQualifiedName(modelMetadata.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == associationType.Constraint.DependentEnd.GetEntityType()));
			ReadOnlyMetadataCollection<EdmProperty> toProperties = associationType.Constraint.ToProperties;
			ConsolidatedIndex consolidatedIndex;
			if (toProperties.Count > 0)
			{
				consolidatedIndex = new ConsolidatedIndex(schemaQualifiedName, toProperties[0].Name, new IndexAttribute(name, 0));
				for (int i = 1; i < toProperties.Count; i++)
				{
					consolidatedIndex.Add(toProperties[i].Name, new IndexAttribute(name, i));
				}
			}
			else
			{
				consolidatedIndex = new ConsolidatedIndex(schemaQualifiedName, new IndexAttribute(name));
			}
			yield return consolidatedIndex;
		}
	}

	private IEnumerable<Tuple<EntityType, EntityType>> FindEntityTypePairs()
	{
		List<Tuple<EntityType, EntityType>> list = (from et1 in _source.EdmItemCollection.GetItems<EntityType>()
			from et2 in _target.EdmItemCollection.GetItems<EntityType>()
			where et1.Name.EqualsOrdinal(et2.Name)
			select Tuple.Create(et1, et2)).ToList();
		List<EntityType> source = Enumerable.Except(second: list.Select((Tuple<EntityType, EntityType> t) => t.Item1).ToList(), first: _source.EdmItemCollection.GetItems<EntityType>()).ToList();
		List<EntityType> targetRemainingEntities = Enumerable.Except(second: list.Select((Tuple<EntityType, EntityType> t) => t.Item2).ToList(), first: _target.EdmItemCollection.GetItems<EntityType>()).ToList();
		return list.Concat(from et1 in source
			from et2 in targetRemainingEntities
			where FuzzyMatchEntities(et1, et2)
			select Tuple.Create(et1, et2));
	}

	private static bool FuzzyMatchEntities(EntityType entityType1, EntityType entityType2)
	{
		if (!entityType1.KeyMembers.SequenceEqual(entityType2.KeyMembers, new DynamicEqualityComparer<EdmMember>((EdmMember m1, EdmMember m2) => m1.EdmEquals(m2))))
		{
			return false;
		}
		if ((entityType1.BaseType != null && entityType2.BaseType == null) || (entityType1.BaseType == null && entityType2.BaseType != null))
		{
			return false;
		}
		return (double)((float)(from m1 in entityType1.DeclaredMembers
			from m2 in entityType2.DeclaredMembers
			where m1.EdmEquals(m2)
			select 1).Count() * 2f / (float)(entityType1.DeclaredMembers.Count + entityType2.DeclaredMembers.Count)) > 0.8;
	}

	private static bool SourceAndTargetMatch(EntityType sourceEntityType, EntityTypeMapping sourceEntityTypeMapping, EntityType targetEntityType, EntityTypeMapping targetEntityTypeMapping)
	{
		if (sourceEntityTypeMapping.EntityType != null && targetEntityTypeMapping.EntityType != null)
		{
			if (sourceEntityType == sourceEntityTypeMapping.EntityType && targetEntityType == targetEntityTypeMapping.EntityType)
			{
				return true;
			}
		}
		else
		{
			ReadOnlyCollection<EntityTypeBase> isOfTypes = sourceEntityTypeMapping.IsOfTypes;
			if (isOfTypes.Contains(sourceEntityType))
			{
				ReadOnlyCollection<EntityTypeBase> isOfTypes2 = targetEntityTypeMapping.IsOfTypes;
				if (isOfTypes2.Contains(targetEntityType))
				{
					IEnumerable<string> first = from et in isOfTypes.Except(new EntityType[1] { sourceEntityType })
						select et.Name;
					IEnumerable<string> second = from et in isOfTypes2.Except(new EntityType[1] { targetEntityType })
						select et.Name;
					if (first.SequenceEqual(second))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool MappingTypesAreIdentical(EntityTypeMapping sourceEntityTypeMapping, EntityTypeMapping targetEntityTypeMapping)
	{
		EntityTypeBase obj = sourceEntityTypeMapping.EntityType ?? sourceEntityTypeMapping.IsOfTypes.First();
		EntityTypeBase entityTypeBase = targetEntityTypeMapping.EntityType ?? targetEntityTypeMapping.IsOfTypes.First();
		return obj.FullName == entityTypeBase.FullName;
	}

	private IEnumerable<Tuple<MappingFragment, MappingFragment>> FindMappingFragmentPairs(ICollection<Tuple<EntityType, EntityType>> entityTypePairs)
	{
		List<EntityTypeMapping> targetEntityTypeMappings = _target.EntityContainerMapping.EntitySetMappings.SelectMany((EntitySetMapping esm) => esm.EntityTypeMappings).ToList();
		IEnumerable<EntityTypeMapping> enumerable = _source.EntityContainerMapping.EntitySetMappings.SelectMany((EntitySetMapping esm) => esm.EntityTypeMappings);
		List<EntityTypeMapping> matchedTargets = new List<EntityTypeMapping>();
		foreach (EntityTypeMapping etm1 in enumerable)
		{
			foreach (EntityTypeMapping etm2 in targetEntityTypeMappings)
			{
				if (matchedTargets.Contains(etm2))
				{
					continue;
				}
				bool flag = entityTypePairs.Any((Tuple<EntityType, EntityType> t) => SourceAndTargetMatch(t.Item1, etm1, t.Item2, etm2));
				if (!flag)
				{
					flag = MappingTypesAreIdentical(etm1, etm2);
				}
				if (!flag)
				{
					continue;
				}
				matchedTargets.Add(etm2);
				foreach (Tuple<MappingFragment, MappingFragment> item in etm1.MappingFragments.Zip(etm2.MappingFragments, Tuple.Create))
				{
					yield return item;
				}
				break;
			}
		}
	}

	private IEnumerable<Tuple<AssociationType, AssociationType>> FindAssociationTypePairs(ICollection<Tuple<EntityType, EntityType>> entityTypePairs)
	{
		List<Tuple<AssociationType, AssociationType>> list = (from ets in entityTypePairs
			from np1 in ets.Item1.NavigationProperties
			from np2 in ets.Item2.NavigationProperties
			where np1.Name.EqualsIgnoreCase(np2.Name)
			from t in GetStoreAssociationTypePairs(np1.Association, np2.Association, entityTypePairs)
			select t).Distinct().ToList();
		List<AssociationType> source = _source.StoreItemCollection.GetItems<AssociationType>().Except(list.Select((Tuple<AssociationType, AssociationType> t) => t.Item1)).ToList();
		List<AssociationType> targetRemainingAssociationTypes = _target.StoreItemCollection.GetItems<AssociationType>().Except(list.Select((Tuple<AssociationType, AssociationType> t) => t.Item2)).ToList();
		return list.Concat(from at1 in source
			from at2 in targetRemainingAssociationTypes
			where at1.Name.EqualsIgnoreCase(at2.Name) || (at1.Constraint != null && at2.Constraint != null && at1.Constraint.PrincipalEnd.GetEntityType().EdmEquals(at2.Constraint.PrincipalEnd.GetEntityType()) && at1.Constraint.DependentEnd.GetEntityType().EdmEquals(at2.Constraint.DependentEnd.GetEntityType()) && at1.Constraint.ToProperties.SequenceEqual(at2.Constraint.ToProperties, new DynamicEqualityComparer<EdmMember>((EdmMember p1, EdmMember p2) => p1.EdmEquals(p2))))
			select Tuple.Create(at1, at2));
	}

	private IEnumerable<Tuple<AssociationType, AssociationType>> GetStoreAssociationTypePairs(AssociationType conceptualAssociationType1, AssociationType conceptualAssociationType2, ICollection<Tuple<EntityType, EntityType>> entityTypePairs)
	{
		if (_source.StoreItemCollection.TryGetItem<AssociationType>(GetStoreAssociationIdentity(conceptualAssociationType1.Name), out var item) && _target.StoreItemCollection.TryGetItem<AssociationType>(GetStoreAssociationIdentity(conceptualAssociationType2.Name), out var item2))
		{
			yield return Tuple.Create(item, item2);
			yield break;
		}
		AssociationEndMember sourceEnd1 = conceptualAssociationType1.SourceEnd;
		Tuple<EntityType, EntityType> tuple = entityTypePairs.Single((Tuple<EntityType, EntityType> t) => t.Item1 == sourceEnd1.GetEntityType());
		AssociationEndMember sourceEnd2 = ((conceptualAssociationType2.SourceEnd.GetEntityType() == tuple.Item2) ? conceptualAssociationType2.SourceEnd : conceptualAssociationType2.TargetEnd);
		if (_source.StoreItemCollection.TryGetItem<AssociationType>(GetStoreAssociationIdentity(sourceEnd1.Name), out item) && _target.StoreItemCollection.TryGetItem<AssociationType>(GetStoreAssociationIdentity(sourceEnd2.Name), out item2))
		{
			yield return Tuple.Create(item, item2);
		}
		AssociationEndMember otherEnd = conceptualAssociationType1.GetOtherEnd(sourceEnd1);
		AssociationEndMember otherEnd2 = conceptualAssociationType2.GetOtherEnd(sourceEnd2);
		if (_source.StoreItemCollection.TryGetItem<AssociationType>(GetStoreAssociationIdentity(otherEnd.Name), out item) && _target.StoreItemCollection.TryGetItem<AssociationType>(GetStoreAssociationIdentity(otherEnd2.Name), out item2))
		{
			yield return Tuple.Create(item, item2);
		}
	}

	private IEnumerable<Tuple<AssociationType, AssociationType>> FindStoreOnlyAssociationTypePairs(ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs, ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		List<AssociationType> list = _source.StoreItemCollection.GetItems<AssociationType>().Except(associationTypePairs.Select((Tuple<AssociationType, AssociationType> t) => t.Item1)).ToList();
		List<AssociationType> list2 = _target.StoreItemCollection.GetItems<AssociationType>().Except(associationTypePairs.Select((Tuple<AssociationType, AssociationType> t) => t.Item2)).ToList();
		List<Tuple<AssociationType, AssociationType>> list3 = new List<Tuple<AssociationType, AssociationType>>();
		while (list.Any())
		{
			AssociationType associationType1 = list[0];
			for (int i = 0; i < list2.Count; i++)
			{
				AssociationType associationType2 = list2[i];
				if (tablePairs.Any((Tuple<EntitySet, EntitySet> t) => t.Item1.ElementType == associationType1.Constraint.PrincipalEnd.GetEntityType() && t.Item2.ElementType == associationType2.Constraint.PrincipalEnd.GetEntityType()) && tablePairs.Any((Tuple<EntitySet, EntitySet> t) => t.Item1.ElementType == associationType1.Constraint.DependentEnd.GetEntityType() && t.Item2.ElementType == associationType2.Constraint.DependentEnd.GetEntityType()))
				{
					list3.Add(Tuple.Create(associationType1, associationType2));
					list2.RemoveAt(i);
					break;
				}
			}
			list.RemoveAt(0);
		}
		return list3;
	}

	private static string GetStoreAssociationIdentity(string associationName)
	{
		return "CodeFirstDatabaseSchema." + associationName;
	}

	private IEnumerable<Tuple<EntitySet, EntitySet>> FindTablePairs(ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs, ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs)
	{
		HashSet<EntitySet> sourceTables = new HashSet<EntitySet>();
		HashSet<EntitySet> targetTables = new HashSet<EntitySet>();
		foreach (Tuple<MappingFragment, MappingFragment> mappingFragmentPair in mappingFragmentPairs)
		{
			EntitySet tableSet = mappingFragmentPair.Item1.TableSet;
			EntitySet tableSet2 = mappingFragmentPair.Item2.TableSet;
			if (!sourceTables.Contains(tableSet) && !targetTables.Contains(tableSet2))
			{
				sourceTables.Add(tableSet);
				targetTables.Add(tableSet2);
				yield return Tuple.Create(tableSet, tableSet2);
			}
		}
		foreach (Tuple<AssociationType, AssociationType> associationTypePair in associationTypePairs)
		{
			EntitySet entitySet = _source.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == associationTypePair.Item1.Constraint.DependentEnd.GetEntityType());
			EntitySet entitySet2 = _target.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == associationTypePair.Item2.Constraint.DependentEnd.GetEntityType());
			if (!sourceTables.Contains(entitySet) && !targetTables.Contains(entitySet2))
			{
				sourceTables.Add(entitySet);
				targetTables.Add(entitySet2);
				yield return Tuple.Create(entitySet, entitySet2);
			}
		}
	}

	private static IEnumerable<RenameTableOperation> HandleTransitiveRenameDependencies(IList<RenameTableOperation> renameTableOperations)
	{
		return HandleTransitiveRenameDependencies(renameTableOperations, delegate(RenameTableOperation rt1, RenameTableOperation rt2)
		{
			DatabaseName databaseName = DatabaseName.Parse(rt1.Name);
			DatabaseName databaseName2 = DatabaseName.Parse(rt2.Name);
			return databaseName.Name.EqualsIgnoreCase(rt2.NewName) && databaseName.Schema.EqualsIgnoreCase(databaseName2.Schema);
		}, (string t, RenameTableOperation rt) => new RenameTableOperation(t, rt.NewName), delegate(RenameTableOperation rt, string t)
		{
			rt.NewName = t;
		});
	}

	private static IEnumerable<RenameColumnOperation> HandleTransitiveRenameDependencies(IList<RenameColumnOperation> renameColumnOperations)
	{
		return HandleTransitiveRenameDependencies(renameColumnOperations, (RenameColumnOperation rc1, RenameColumnOperation rc2) => rc1.Table.EqualsIgnoreCase(rc2.Table) && rc1.Name.EqualsIgnoreCase(rc2.NewName), (string c, RenameColumnOperation rc) => new RenameColumnOperation(rc.Table, c, rc.NewName), delegate(RenameColumnOperation rc, string c)
		{
			rc.NewName = c;
		});
	}

	private static IEnumerable<RenameIndexOperation> HandleTransitiveRenameDependencies(IList<RenameIndexOperation> renameIndexOperations)
	{
		return HandleTransitiveRenameDependencies(renameIndexOperations, (RenameIndexOperation ri1, RenameIndexOperation ri2) => ri1.Table.EqualsIgnoreCase(ri2.Table) && ri1.Name.EqualsIgnoreCase(ri2.NewName), (string i, RenameIndexOperation rc) => new RenameIndexOperation(rc.Table, i, rc.NewName), delegate(RenameIndexOperation rc, string i)
		{
			rc.NewName = i;
		});
	}

	private static IEnumerable<T> HandleTransitiveRenameDependencies<T>(IList<T> renameOperations, Func<T, T, bool> dependencyFinder, Func<string, T, T> renameCreator, Action<T, string> setNewName) where T : class
	{
		int tempCounter = 0;
		List<T> tempRenames = new List<T>();
		for (int i = 0; i < renameOperations.Count; i++)
		{
			T renameOperation = renameOperations[i];
			if (renameOperations.Skip(i + 1).SingleOrDefault((T rt) => dependencyFinder(renameOperation, rt)) != null)
			{
				string text = "__mig_tmp__" + tempCounter++;
				tempRenames.Add(renameCreator(text, renameOperation));
				setNewName(renameOperation, text);
			}
			yield return renameOperation;
		}
		foreach (T item in tempRenames)
		{
			yield return item;
		}
	}

	private IEnumerable<MoveProcedureOperation> FindMovedModificationFunctions()
	{
		return (from esm1 in _source.EntityContainerMapping.EntitySetMappings
			from mfm1 in esm1.ModificationFunctionMappings
			from esm2 in _target.EntityContainerMapping.EntitySetMappings
			from mfm2 in esm2.ModificationFunctionMappings
			where mfm1.EntityType.Identity == mfm2.EntityType.Identity
			from o in DiffModificationFunctionSchemas(mfm1, mfm2)
			select o).Concat(from asm1 in _source.EntityContainerMapping.AssociationSetMappings
			where asm1.ModificationFunctionMapping != null
			from asm2 in _target.EntityContainerMapping.AssociationSetMappings
			where asm2.ModificationFunctionMapping != null && asm1.ModificationFunctionMapping.AssociationSet.Identity == asm2.ModificationFunctionMapping.AssociationSet.Identity
			from o in DiffModificationFunctionSchemas(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping)
			select o);
	}

	private static IEnumerable<MoveProcedureOperation> DiffModificationFunctionSchemas(EntityTypeModificationFunctionMapping sourceModificationFunctionMapping, EntityTypeModificationFunctionMapping targetModificationFunctionMapping)
	{
		if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema.EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema))
		{
			yield return new MoveProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.InsertFunctionMapping.Function), targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema);
		}
		if (!sourceModificationFunctionMapping.UpdateFunctionMapping.Function.Schema.EqualsOrdinal(targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema))
		{
			yield return new MoveProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.UpdateFunctionMapping.Function), targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema);
		}
		if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema.EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
		{
			yield return new MoveProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.DeleteFunctionMapping.Function), targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema);
		}
	}

	private static IEnumerable<MoveProcedureOperation> DiffModificationFunctionSchemas(AssociationSetModificationFunctionMapping sourceModificationFunctionMapping, AssociationSetModificationFunctionMapping targetModificationFunctionMapping)
	{
		if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema.EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema))
		{
			yield return new MoveProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.InsertFunctionMapping.Function), targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema);
		}
		if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema.EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
		{
			yield return new MoveProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.DeleteFunctionMapping.Function), targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema);
		}
	}

	private IEnumerable<CreateProcedureOperation> FindAddedModificationFunctions(Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return (from esm1 in _target.EntityContainerMapping.EntitySetMappings
			from mfm1 in esm1.ModificationFunctionMappings
			where !(from esm2 in _source.EntityContainerMapping.EntitySetMappings
				from mfm2 in esm2.ModificationFunctionMappings
				where mfm1.EntityType.Identity == mfm2.EntityType.Identity
				select mfm2).Any()
			from o in BuildCreateProcedureOperations(mfm1, modificationCommandTreeGenerator, migrationSqlGenerator)
			select o).Concat(from asm1 in _target.EntityContainerMapping.AssociationSetMappings
			where asm1.ModificationFunctionMapping != null
			where !(from asm2 in _source.EntityContainerMapping.AssociationSetMappings
				where asm2.ModificationFunctionMapping != null && asm1.ModificationFunctionMapping.AssociationSet.Identity == asm2.ModificationFunctionMapping.AssociationSet.Identity
				select asm2.ModificationFunctionMapping).Any()
			from o in BuildCreateProcedureOperations(asm1.ModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator)
			select o);
	}

	private IEnumerable<RenameProcedureOperation> FindRenamedModificationFunctions()
	{
		return (from esm1 in _source.EntityContainerMapping.EntitySetMappings
			from mfm1 in esm1.ModificationFunctionMappings
			from esm2 in _target.EntityContainerMapping.EntitySetMappings
			from mfm2 in esm2.ModificationFunctionMappings
			where mfm1.EntityType.Identity == mfm2.EntityType.Identity
			from o in DiffModificationFunctionNames(mfm1, mfm2)
			select o).Concat(from asm1 in _source.EntityContainerMapping.AssociationSetMappings
			where asm1.ModificationFunctionMapping != null
			from asm2 in _target.EntityContainerMapping.AssociationSetMappings
			where asm2.ModificationFunctionMapping != null && asm1.ModificationFunctionMapping.AssociationSet.Identity == asm2.ModificationFunctionMapping.AssociationSet.Identity
			from o in DiffModificationFunctionNames(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping)
			select o);
	}

	private static IEnumerable<RenameProcedureOperation> DiffModificationFunctionNames(AssociationSetModificationFunctionMapping sourceModificationFunctionMapping, AssociationSetModificationFunctionMapping targetModificationFunctionMapping)
	{
		if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName.EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName))
		{
			yield return new RenameProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName, targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema), targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName);
		}
		if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName.EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName))
		{
			yield return new RenameProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName, targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema), targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName);
		}
	}

	private static IEnumerable<RenameProcedureOperation> DiffModificationFunctionNames(EntityTypeModificationFunctionMapping sourceModificationFunctionMapping, EntityTypeModificationFunctionMapping targetModificationFunctionMapping)
	{
		if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName.EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName))
		{
			yield return new RenameProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName, targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema), targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName);
		}
		if (!sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName.EqualsOrdinal(targetModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName))
		{
			yield return new RenameProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName, targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema), targetModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName);
		}
		if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName.EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName))
		{
			yield return new RenameProcedureOperation(GetSchemaQualifiedName(sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName, targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema), targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName);
		}
	}

	private static string GetSchemaQualifiedName(string table, string schema)
	{
		return new DatabaseName(table, schema).ToString();
	}

	private IEnumerable<AlterProcedureOperation> FindAlteredModificationFunctions(Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return (from esm1 in _source.EntityContainerMapping.EntitySetMappings
			from mfm1 in esm1.ModificationFunctionMappings
			from esm2 in _target.EntityContainerMapping.EntitySetMappings
			from mfm2 in esm2.ModificationFunctionMappings
			where mfm1.EntityType.Identity == mfm2.EntityType.Identity
			from o in DiffModificationFunctions(mfm1, mfm2, modificationCommandTreeGenerator, migrationSqlGenerator)
			select o).Concat(from asm1 in _source.EntityContainerMapping.AssociationSetMappings
			where asm1.ModificationFunctionMapping != null
			from asm2 in _target.EntityContainerMapping.AssociationSetMappings
			where asm2.ModificationFunctionMapping != null && asm1.ModificationFunctionMapping.AssociationSet.Identity == asm2.ModificationFunctionMapping.AssociationSet.Identity
			from o in DiffModificationFunctions(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator)
			select o);
	}

	private IEnumerable<AlterProcedureOperation> DiffModificationFunctions(AssociationSetModificationFunctionMapping sourceModificationFunctionMapping, AssociationSetModificationFunctionMapping targetModificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		if (!DiffModificationFunction(sourceModificationFunctionMapping.InsertFunctionMapping, targetModificationFunctionMapping.InsertFunctionMapping))
		{
			yield return BuildAlterProcedureOperation(targetModificationFunctionMapping.InsertFunctionMapping.Function, GenerateInsertFunctionBody(targetModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		}
		if (!DiffModificationFunction(sourceModificationFunctionMapping.DeleteFunctionMapping, targetModificationFunctionMapping.DeleteFunctionMapping))
		{
			yield return BuildAlterProcedureOperation(targetModificationFunctionMapping.DeleteFunctionMapping.Function, GenerateDeleteFunctionBody(targetModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		}
	}

	private IEnumerable<AlterProcedureOperation> DiffModificationFunctions(EntityTypeModificationFunctionMapping sourceModificationFunctionMapping, EntityTypeModificationFunctionMapping targetModificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		if (!DiffModificationFunction(sourceModificationFunctionMapping.InsertFunctionMapping, targetModificationFunctionMapping.InsertFunctionMapping))
		{
			yield return BuildAlterProcedureOperation(targetModificationFunctionMapping.InsertFunctionMapping.Function, GenerateInsertFunctionBody(targetModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		}
		if (!DiffModificationFunction(sourceModificationFunctionMapping.UpdateFunctionMapping, targetModificationFunctionMapping.UpdateFunctionMapping))
		{
			yield return BuildAlterProcedureOperation(targetModificationFunctionMapping.UpdateFunctionMapping.Function, GenerateUpdateFunctionBody(targetModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		}
		if (!DiffModificationFunction(sourceModificationFunctionMapping.DeleteFunctionMapping, targetModificationFunctionMapping.DeleteFunctionMapping))
		{
			yield return BuildAlterProcedureOperation(targetModificationFunctionMapping.DeleteFunctionMapping.Function, GenerateDeleteFunctionBody(targetModificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		}
	}

	private string GenerateInsertFunctionBody(EntityTypeModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return GenerateFunctionBody(modificationFunctionMapping, (ModificationCommandTreeGenerator m, string s) => m.GenerateInsert(s), modificationCommandTreeGenerator, migrationSqlGenerator, modificationFunctionMapping.InsertFunctionMapping.Function.FunctionName, null);
	}

	private string GenerateInsertFunctionBody(AssociationSetModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return GenerateFunctionBody(modificationFunctionMapping, (ModificationCommandTreeGenerator m, string s) => m.GenerateAssociationInsert(s), modificationCommandTreeGenerator, migrationSqlGenerator, null);
	}

	private string GenerateUpdateFunctionBody(EntityTypeModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return GenerateFunctionBody(modificationFunctionMapping, (ModificationCommandTreeGenerator m, string s) => m.GenerateUpdate(s), modificationCommandTreeGenerator, migrationSqlGenerator, modificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName, modificationFunctionMapping.UpdateFunctionMapping.RowsAffectedParameterName);
	}

	private string GenerateDeleteFunctionBody(EntityTypeModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return GenerateFunctionBody(modificationFunctionMapping, (ModificationCommandTreeGenerator m, string s) => m.GenerateDelete(s), modificationCommandTreeGenerator, migrationSqlGenerator, modificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName, modificationFunctionMapping.DeleteFunctionMapping.RowsAffectedParameterName);
	}

	private string GenerateDeleteFunctionBody(AssociationSetModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		return GenerateFunctionBody(modificationFunctionMapping, (ModificationCommandTreeGenerator m, string s) => m.GenerateAssociationDelete(s), modificationCommandTreeGenerator, migrationSqlGenerator, modificationFunctionMapping.DeleteFunctionMapping.RowsAffectedParameterName);
	}

	private string GenerateFunctionBody<TCommandTree>(EntityTypeModificationFunctionMapping modificationFunctionMapping, Func<ModificationCommandTreeGenerator, string, IEnumerable<TCommandTree>> treeGenerator, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator, string functionName, string rowsAffectedParameterName) where TCommandTree : DbModificationCommandTree
	{
		TCommandTree[] commandTrees = new TCommandTree[0];
		if (modificationCommandTreeGenerator != null)
		{
			DynamicToFunctionModificationCommandConverter dynamicToFunctionModificationCommandConverter = new DynamicToFunctionModificationCommandConverter(modificationFunctionMapping, _target.EntityContainerMapping);
			try
			{
				commandTrees = dynamicToFunctionModificationCommandConverter.Convert(treeGenerator(modificationCommandTreeGenerator.Value, modificationFunctionMapping.EntityType.Identity)).ToArray();
			}
			catch (UpdateException innerException)
			{
				throw new InvalidOperationException(Strings.ErrorGeneratingCommandTree(functionName, modificationFunctionMapping.EntityType.Name), innerException);
			}
		}
		return GenerateFunctionBody(migrationSqlGenerator, rowsAffectedParameterName, commandTrees);
	}

	private string GenerateFunctionBody<TCommandTree>(AssociationSetModificationFunctionMapping modificationFunctionMapping, Func<ModificationCommandTreeGenerator, string, IEnumerable<TCommandTree>> treeGenerator, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator, string rowsAffectedParameterName) where TCommandTree : DbModificationCommandTree
	{
		TCommandTree[] commandTrees = new TCommandTree[0];
		if (modificationCommandTreeGenerator != null)
		{
			commandTrees = new DynamicToFunctionModificationCommandConverter(modificationFunctionMapping, _target.EntityContainerMapping).Convert(treeGenerator(modificationCommandTreeGenerator.Value, modificationFunctionMapping.AssociationSet.ElementType.Identity)).ToArray();
		}
		return GenerateFunctionBody(migrationSqlGenerator, rowsAffectedParameterName, commandTrees);
	}

	private string GenerateFunctionBody<TCommandTree>(MigrationSqlGenerator migrationSqlGenerator, string rowsAffectedParameterName, TCommandTree[] commandTrees) where TCommandTree : DbModificationCommandTree
	{
		if (migrationSqlGenerator == null)
		{
			return null;
		}
		string providerManifestToken = _target.ProviderInfo.ProviderManifestToken;
		return migrationSqlGenerator.GenerateProcedureBody(commandTrees, rowsAffectedParameterName, providerManifestToken);
	}

	private bool DiffModificationFunction(ModificationFunctionMapping functionMapping1, ModificationFunctionMapping functionMapping2)
	{
		if (!functionMapping1.RowsAffectedParameterName.EqualsOrdinal(functionMapping2.RowsAffectedParameterName))
		{
			return false;
		}
		if (!functionMapping1.ParameterBindings.SequenceEqual(functionMapping2.ParameterBindings, DiffParameterBinding))
		{
			return false;
		}
		IEnumerable<ModificationFunctionResultBinding> enumerable = Enumerable.Empty<ModificationFunctionResultBinding>();
		IEnumerable<ModificationFunctionResultBinding> resultBindings = functionMapping1.ResultBindings;
		IEnumerable<ModificationFunctionResultBinding> source = resultBindings ?? enumerable;
		resultBindings = functionMapping2.ResultBindings;
		if (!source.SequenceEqual(resultBindings ?? enumerable, DiffResultBinding))
		{
			return false;
		}
		return true;
	}

	private bool DiffParameterBinding(ModificationFunctionParameterBinding parameterBinding1, ModificationFunctionParameterBinding parameterBinding2)
	{
		FunctionParameter parameter = parameterBinding1.Parameter;
		FunctionParameter parameter2 = parameterBinding2.Parameter;
		if (!parameter.Name.EqualsOrdinal(parameter2.Name))
		{
			return false;
		}
		if (parameter.Mode != parameter2.Mode)
		{
			return false;
		}
		if (parameterBinding1.IsCurrent != parameterBinding2.IsCurrent)
		{
			return false;
		}
		if (!parameterBinding1.MemberPath.Members.SequenceEqual(parameterBinding2.MemberPath.Members, (EdmMember m1, EdmMember m2) => m1.Identity.EqualsOrdinal(m2.Identity)))
		{
			return false;
		}
		if (_source.ProviderInfo.Equals(_target.ProviderInfo))
		{
			if (parameter.TypeName.EqualsIgnoreCase(parameter2.TypeName))
			{
				return parameter.TypeUsage.EdmEquals(parameter2.TypeUsage);
			}
			return false;
		}
		if (parameter.Precision == parameter2.Precision)
		{
			return parameter.Scale == parameter2.Scale;
		}
		return false;
	}

	private static bool DiffResultBinding(ModificationFunctionResultBinding resultBinding1, ModificationFunctionResultBinding resultBinding2)
	{
		if (!resultBinding1.ColumnName.EqualsOrdinal(resultBinding2.ColumnName))
		{
			return false;
		}
		if (!resultBinding1.Property.Identity.EqualsOrdinal(resultBinding2.Property.Identity))
		{
			return false;
		}
		return true;
	}

	private IEnumerable<CreateProcedureOperation> BuildCreateProcedureOperations(EntityTypeModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		yield return BuildCreateProcedureOperation(modificationFunctionMapping.InsertFunctionMapping.Function, GenerateInsertFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		yield return BuildCreateProcedureOperation(modificationFunctionMapping.UpdateFunctionMapping.Function, GenerateUpdateFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		yield return BuildCreateProcedureOperation(modificationFunctionMapping.DeleteFunctionMapping.Function, GenerateDeleteFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
	}

	private IEnumerable<CreateProcedureOperation> BuildCreateProcedureOperations(AssociationSetModificationFunctionMapping modificationFunctionMapping, Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
	{
		yield return BuildCreateProcedureOperation(modificationFunctionMapping.InsertFunctionMapping.Function, GenerateInsertFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
		yield return BuildCreateProcedureOperation(modificationFunctionMapping.DeleteFunctionMapping.Function, GenerateDeleteFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
	}

	private CreateProcedureOperation BuildCreateProcedureOperation(EdmFunction function, string bodySql)
	{
		CreateProcedureOperation createProcedureOperation = new CreateProcedureOperation(GetSchemaQualifiedName(function), bodySql);
		function.Parameters.Each(delegate(FunctionParameter p)
		{
			createProcedureOperation.Parameters.Add(BuildParameterModel(p, _target));
		});
		return createProcedureOperation;
	}

	private AlterProcedureOperation BuildAlterProcedureOperation(EdmFunction function, string bodySql)
	{
		AlterProcedureOperation alterProcedureOperation = new AlterProcedureOperation(GetSchemaQualifiedName(function), bodySql);
		function.Parameters.Each(delegate(FunctionParameter p)
		{
			alterProcedureOperation.Parameters.Add(BuildParameterModel(p, _target));
		});
		return alterProcedureOperation;
	}

	private static ParameterModel BuildParameterModel(FunctionParameter functionParameter, ModelMetadata modelMetadata)
	{
		TypeUsage modelTypeUsage = functionParameter.TypeUsage.ModelTypeUsage;
		string name = modelMetadata.ProviderManifest.GetStoreType(modelTypeUsage).EdmType.Name;
		ParameterModel parameterModel = new ParameterModel(((PrimitiveType)modelTypeUsage.EdmType).PrimitiveTypeKind, modelTypeUsage)
		{
			Name = functionParameter.Name,
			IsOutParameter = (functionParameter.Mode == ParameterMode.Out),
			StoreType = ((!functionParameter.TypeName.EqualsIgnoreCase(name)) ? functionParameter.TypeName : null)
		};
		if (modelTypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: true, out var item) && item.Value != null)
		{
			parameterModel.MaxLength = item.Value as int?;
		}
		if (modelTypeUsage.Facets.TryGetValue("Precision", ignoreCase: true, out item) && item.Value != null)
		{
			parameterModel.Precision = (byte?)item.Value;
		}
		if (modelTypeUsage.Facets.TryGetValue("Scale", ignoreCase: true, out item) && item.Value != null)
		{
			parameterModel.Scale = (byte?)item.Value;
		}
		if (modelTypeUsage.Facets.TryGetValue("FixedLength", ignoreCase: true, out item) && item.Value != null && (bool)item.Value)
		{
			parameterModel.IsFixedLength = true;
		}
		if (modelTypeUsage.Facets.TryGetValue("Unicode", ignoreCase: true, out item) && item.Value != null && !(bool)item.Value)
		{
			parameterModel.IsUnicode = false;
		}
		return parameterModel;
	}

	private IEnumerable<DropProcedureOperation> FindDroppedModificationFunctions()
	{
		return (from esm1 in _source.EntityContainerMapping.EntitySetMappings
			from mfm1 in esm1.ModificationFunctionMappings
			where !(from esm2 in _target.EntityContainerMapping.EntitySetMappings
				from mfm2 in esm2.ModificationFunctionMappings
				where mfm1.EntityType.Identity == mfm2.EntityType.Identity
				select mfm2).Any()
			from o in new DropProcedureOperation[3]
			{
				new DropProcedureOperation(GetSchemaQualifiedName(mfm1.InsertFunctionMapping.Function)),
				new DropProcedureOperation(GetSchemaQualifiedName(mfm1.UpdateFunctionMapping.Function)),
				new DropProcedureOperation(GetSchemaQualifiedName(mfm1.DeleteFunctionMapping.Function))
			}
			select o).Concat(from asm1 in _source.EntityContainerMapping.AssociationSetMappings
			where asm1.ModificationFunctionMapping != null
			where !(from asm2 in _target.EntityContainerMapping.AssociationSetMappings
				where asm2.ModificationFunctionMapping != null && asm1.ModificationFunctionMapping.AssociationSet.Identity == asm2.ModificationFunctionMapping.AssociationSet.Identity
				select asm2.ModificationFunctionMapping).Any()
			from o in new DropProcedureOperation[2]
			{
				new DropProcedureOperation(GetSchemaQualifiedName(asm1.ModificationFunctionMapping.InsertFunctionMapping.Function)),
				new DropProcedureOperation(GetSchemaQualifiedName(asm1.ModificationFunctionMapping.DeleteFunctionMapping.Function))
			}
			select o);
	}

	private static IEnumerable<RenameTableOperation> FindRenamedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		return from p in tablePairs
			where !p.Item1.Table.EqualsIgnoreCase(p.Item2.Table)
			select new RenameTableOperation(GetSchemaQualifiedName(p.Item1), p.Item2.Table);
	}

	private IEnumerable<CreateTableOperation> FindAddedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		return from es in _target.StoreEntityContainer.EntitySets.Except(tablePairs.Select((Tuple<EntitySet, EntitySet> p) => p.Item2))
			select BuildCreateTableOperation(es, _target);
	}

	private IEnumerable<MoveTableOperation> FindMovedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		return from p in tablePairs
			where !p.Item1.Schema.EqualsIgnoreCase(p.Item2.Schema)
			select new MoveTableOperation(new DatabaseName(p.Item2.Table, p.Item1.Schema).ToString(), p.Item2.Schema)
			{
				CreateTableOperation = BuildCreateTableOperation(p.Item2, _target)
			};
	}

	private IEnumerable<DropTableOperation> FindDroppedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		return from es in _source.StoreEntityContainer.EntitySets.Except(tablePairs.Select((Tuple<EntitySet, EntitySet> p) => p.Item1))
			select new DropTableOperation(GetSchemaQualifiedName(es), GetAnnotations(es.ElementType), es.ElementType.Properties.Where((EdmProperty p) => GetAnnotations(p).Count > 0).ToDictionary((Func<EdmProperty, string>)((EdmProperty p) => p.Name), (Func<EdmProperty, IDictionary<string, object>>)((EdmProperty p) => GetAnnotations(p))), BuildCreateTableOperation(es, _source));
	}

	private IEnumerable<AlterTableOperation> FindAlteredTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		return from p in tablePairs
			where !GetAnnotations(p.Item1.ElementType).SequenceEqual(GetAnnotations(p.Item2.ElementType))
			select BuildAlterTableAnnotationsOperation(p.Item1, p.Item2);
	}

	private AlterTableOperation BuildAlterTableAnnotationsOperation(EntitySet sourceTable, EntitySet destinationTable)
	{
		AlterTableOperation operation = new AlterTableOperation(GetSchemaQualifiedName(destinationTable), BuildAnnotationPairs(GetAnnotations(sourceTable.ElementType), GetAnnotations(destinationTable.ElementType)));
		destinationTable.ElementType.Properties.Each(delegate(EdmProperty p)
		{
			operation.Columns.Add(BuildColumnModel(p, _target, GetAnnotations(p).ToDictionary((KeyValuePair<string, object> a) => a.Key, (KeyValuePair<string, object> a) => new AnnotationValues(a.Value, a.Value))));
		});
		return operation;
	}

	internal static Dictionary<string, object> GetAnnotations(MetadataItem item)
	{
		return item.Annotations.Where((MetadataProperty a) => a.Name.StartsWith("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:", StringComparison.Ordinal) && !a.Name.EndsWith("Index", StringComparison.Ordinal)).ToDictionary((MetadataProperty a) => a.Name.Substring("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:".Length), (MetadataProperty a) => a.Value);
	}

	internal static IndexAttribute GetPrimaryKeyIndexAttribute(EntityType entityType)
	{
		return (from a in entityType.Annotations
			where a.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:Index"
			select a.Value).OfType<IndexAnnotation>().SelectMany((IndexAnnotation ia) => ia.Indexes).SingleOrDefault();
	}

	private IEnumerable<MigrationOperation> FindAlteredPrimaryKeys(ICollection<Tuple<EntitySet, EntitySet>> tablePairs, ICollection<RenameColumnOperation> renamedColumns, ICollection<AlterColumnOperation> alteredColumns)
	{
		return from ts in tablePairs
			let t2 = GetSchemaQualifiedName(ts.Item2)
			let pk1 = GetPrimaryKeyIndexAttribute(ts.Item1.ElementType) ?? new IndexAttribute()
			let pk2 = GetPrimaryKeyIndexAttribute(ts.Item2.ElementType) ?? new IndexAttribute()
			where !ts.Item1.ElementType.KeyProperties.SequenceEqual(ts.Item2.ElementType.KeyProperties, (EdmProperty p1, EdmProperty p2) => p1.Name.EqualsIgnoreCase(p2.Name) || renamedColumns.Any((RenameColumnOperation rc) => rc.Table.EqualsIgnoreCase(t2) && rc.Name.EqualsIgnoreCase(p1.Name) && rc.NewName.EqualsIgnoreCase(p2.Name))) || ts.Item2.ElementType.KeyProperties.Any((EdmProperty p) => alteredColumns.Any((AlterColumnOperation ac) => ac.Table.EqualsIgnoreCase(t2) && ac.Column.Name.EqualsIgnoreCase(p.Name))) || pk1.Name != pk2.Name || pk1.IsClusteredConfigured != pk2.IsClusteredConfigured || pk1.IsClustered != pk2.IsClustered
			from o in BuildChangePrimaryKeyOperations(ts)
			select o;
	}

	private IEnumerable<MigrationOperation> BuildChangePrimaryKeyOperations(Tuple<EntitySet, EntitySet> tablePair)
	{
		List<ReferentialConstraint> list = (from at in _source.StoreItemCollection.GetItems<AssociationType>()
			select at.Constraint into c
			where c.FromProperties.SequenceEqual(tablePair.Item1.ElementType.KeyProperties)
			select c).ToList();
		foreach (ReferentialConstraint item in list)
		{
			yield return BuildDropForeignKeyOperation(item, _source);
		}
		DropPrimaryKeyOperation dropPrimaryKeyOperation = new DropPrimaryKeyOperation
		{
			Table = GetSchemaQualifiedName(tablePair.Item2)
		};
		tablePair.Item1.ElementType.KeyProperties.Each(delegate(EdmProperty pr)
		{
			dropPrimaryKeyOperation.Columns.Add(pr.Name);
		});
		IndexAttribute primaryKeyIndexAttribute = GetPrimaryKeyIndexAttribute(tablePair.Item1.ElementType);
		if (primaryKeyIndexAttribute != null)
		{
			dropPrimaryKeyOperation.Name = primaryKeyIndexAttribute.Name;
			if (primaryKeyIndexAttribute.IsClusteredConfigured)
			{
				dropPrimaryKeyOperation.IsClustered = primaryKeyIndexAttribute.IsClustered;
			}
		}
		yield return dropPrimaryKeyOperation;
		AddPrimaryKeyOperation addPrimaryKeyOperation = new AddPrimaryKeyOperation
		{
			Table = GetSchemaQualifiedName(tablePair.Item2)
		};
		tablePair.Item2.ElementType.KeyProperties.Each(delegate(EdmProperty pr)
		{
			addPrimaryKeyOperation.Columns.Add(pr.Name);
		});
		IndexAttribute primaryKeyIndexAttribute2 = GetPrimaryKeyIndexAttribute(tablePair.Item2.ElementType);
		if (primaryKeyIndexAttribute2 != null)
		{
			addPrimaryKeyOperation.Name = primaryKeyIndexAttribute2.Name;
			if (primaryKeyIndexAttribute2.IsClusteredConfigured)
			{
				addPrimaryKeyOperation.IsClustered = primaryKeyIndexAttribute2.IsClustered;
			}
		}
		yield return addPrimaryKeyOperation;
		List<ReferentialConstraint> list2 = (from at in _target.StoreItemCollection.GetItems<AssociationType>()
			select at.Constraint into c
			where c.FromProperties.SequenceEqual(tablePair.Item2.ElementType.KeyProperties)
			select c).ToList();
		foreach (ReferentialConstraint item2 in list2)
		{
			yield return BuildAddForeignKeyOperation(item2, _target);
		}
	}

	private IEnumerable<AddForeignKeyOperation> FindAddedForeignKeys(ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from at in _target.StoreItemCollection.GetItems<AssociationType>().Except(associationTypePairs.Select((Tuple<AssociationType, AssociationType> p) => p.Item2)).Concat(from at in associationTypePairs
				where !DiffAssociations(at.Item1.Constraint, at.Item2.Constraint, renamedColumns)
				select at.Item2)
			select BuildAddForeignKeyOperation(at.Constraint, _target);
	}

	private IEnumerable<DropForeignKeyOperation> FindDroppedForeignKeys(ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from at in _source.StoreItemCollection.GetItems<AssociationType>().Except(associationTypePairs.Select((Tuple<AssociationType, AssociationType> p) => p.Item1)).Concat(from at in associationTypePairs
				where !DiffAssociations(at.Item1.Constraint, at.Item2.Constraint, renamedColumns)
				select at.Item1)
			select BuildDropForeignKeyOperation(at.Constraint, _source);
	}

	private bool DiffAssociations(ReferentialConstraint referentialConstraint1, ReferentialConstraint referentialConstraint2, ICollection<RenameColumnOperation> renamedColumns)
	{
		string targetTable = GetSchemaQualifiedName(_target.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == referentialConstraint2.DependentEnd.GetEntityType()));
		if (referentialConstraint1.ToProperties.SequenceEqual(referentialConstraint2.ToProperties, (EdmProperty p1, EdmProperty p2) => p1.Name.EqualsIgnoreCase(p2.Name) || renamedColumns.Any((RenameColumnOperation rc) => rc.Table.EqualsIgnoreCase(targetTable) && rc.Name.EqualsIgnoreCase(p1.Name) && rc.NewName.EqualsIgnoreCase(p2.Name))))
		{
			return referentialConstraint1.PrincipalEnd.DeleteBehavior == referentialConstraint2.PrincipalEnd.DeleteBehavior;
		}
		return false;
	}

	private static AddForeignKeyOperation BuildAddForeignKeyOperation(ReferentialConstraint referentialConstraint, ModelMetadata modelMetadata)
	{
		AddForeignKeyOperation addForeignKeyOperation = new AddForeignKeyOperation();
		BuildForeignKeyOperation(referentialConstraint, addForeignKeyOperation, modelMetadata);
		referentialConstraint.FromProperties.Each(delegate(EdmProperty pr)
		{
			addForeignKeyOperation.PrincipalColumns.Add(pr.Name);
		});
		addForeignKeyOperation.CascadeDelete = referentialConstraint.PrincipalEnd.DeleteBehavior == OperationAction.Cascade;
		return addForeignKeyOperation;
	}

	private static DropForeignKeyOperation BuildDropForeignKeyOperation(ReferentialConstraint referentialConstraint, ModelMetadata modelMetadata)
	{
		DropForeignKeyOperation dropForeignKeyOperation = new DropForeignKeyOperation(BuildAddForeignKeyOperation(referentialConstraint, modelMetadata));
		BuildForeignKeyOperation(referentialConstraint, dropForeignKeyOperation, modelMetadata);
		return dropForeignKeyOperation;
	}

	private static void BuildForeignKeyOperation(ReferentialConstraint referentialConstraint, ForeignKeyOperation foreignKeyOperation, ModelMetadata modelMetadata)
	{
		foreignKeyOperation.PrincipalTable = GetSchemaQualifiedName(modelMetadata.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == referentialConstraint.PrincipalEnd.GetEntityType()));
		foreignKeyOperation.DependentTable = GetSchemaQualifiedName(modelMetadata.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == referentialConstraint.DependentEnd.GetEntityType()));
		referentialConstraint.ToProperties.Each(delegate(EdmProperty pr)
		{
			foreignKeyOperation.DependentColumns.Add(pr.Name);
		});
	}

	private IEnumerable<AddColumnOperation> FindAddedColumns(ICollection<Tuple<EntitySet, EntitySet>> tablePairs, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from p in tablePairs
			let t = GetSchemaQualifiedName(p.Item2)
			from c in p.Item2.ElementType.Properties.Except(p.Item1.ElementType.Properties, (EdmProperty c1, EdmProperty c2) => c1.Name.EqualsIgnoreCase(c2.Name))
			where !renamedColumns.Any((RenameColumnOperation cr) => cr.Table.EqualsIgnoreCase(t) && cr.NewName.EqualsIgnoreCase(c.Name))
			select new AddColumnOperation(t, BuildColumnModel(c, _target, GetAnnotations(c).ToDictionary((KeyValuePair<string, object> a) => a.Key, (KeyValuePair<string, object> a) => new AnnotationValues(null, a.Value))));
	}

	private IEnumerable<DropColumnOperation> FindDroppedColumns(ICollection<Tuple<EntitySet, EntitySet>> tablePairs, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from p in tablePairs
			let t = GetSchemaQualifiedName(p.Item2)
			from c in p.Item1.ElementType.Properties.Except(p.Item2.ElementType.Properties, (EdmProperty c1, EdmProperty c2) => c1.Name.EqualsIgnoreCase(c2.Name))
			where !renamedColumns.Any((RenameColumnOperation rc) => rc.Table.EqualsIgnoreCase(t) && rc.Name.EqualsIgnoreCase(c.Name))
			select new DropColumnOperation(t, c.Name, GetAnnotations(c), new AddColumnOperation(t, BuildColumnModel(c, _source, GetAnnotations(c).ToDictionary((KeyValuePair<string, object> a) => a.Key, (KeyValuePair<string, object> a) => new AnnotationValues(null, a.Value)))));
	}

	private IEnumerable<DropColumnOperation> FindOrphanedColumns(ICollection<Tuple<EntitySet, EntitySet>> tablePairs, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from p in tablePairs
			let t = GetSchemaQualifiedName(p.Item2)
			from rc1 in renamedColumns
			where rc1.Table.EqualsIgnoreCase(t)
			from c in p.Item1.ElementType.Properties
			where c.Name.EqualsIgnoreCase(rc1.NewName) && !renamedColumns.Any((RenameColumnOperation rc2) => rc2 != rc1 && rc2.Table.EqualsIgnoreCase(rc1.Table) && rc2.Name.EqualsIgnoreCase(rc1.NewName))
			select new DropColumnOperation(t, c.Name, GetAnnotations(c), new AddColumnOperation(t, BuildColumnModel(c, _source, GetAnnotations(c).ToDictionary((KeyValuePair<string, object> a) => a.Key, (KeyValuePair<string, object> a) => new AnnotationValues(null, a.Value)))));
	}

	private IEnumerable<AlterColumnOperation> FindAlteredColumns(ICollection<Tuple<EntitySet, EntitySet>> tablePairs, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from p in tablePairs
			let t = GetSchemaQualifiedName(p.Item2)
			from p1 in p.Item1.ElementType.Properties
			let p2 = p.Item2.ElementType.Properties.SingleOrDefault((EdmProperty c) => (p1.Name.EqualsIgnoreCase(c.Name) || renamedColumns.Any((RenameColumnOperation rc) => rc.Table.EqualsIgnoreCase(t) && rc.Name.EqualsIgnoreCase(p1.Name) && rc.NewName.EqualsIgnoreCase(c.Name))) && !DiffColumns(p1, c))
			where p2 != null
			select BuildAlterColumnOperation(t, p2, _target, p1, _source);
	}

	private IEnumerable<ConsolidatedIndex> FindSourceIndexes(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
	{
		return from es in _source.StoreEntityContainer.EntitySets
			let p = tablePairs.SingleOrDefault((Tuple<EntitySet, EntitySet> p) => p.Item1 == es)
			let t = GetSchemaQualifiedName((p != null) ? p.Item2 : es)
			from i in ConsolidatedIndex.BuildIndexes(t, es.ElementType.Properties.Select((EdmProperty c) => Tuple.Create(c.Name, c)))
			select i;
	}

	private IEnumerable<ConsolidatedIndex> FindTargetIndexes()
	{
		return from es in _target.StoreEntityContainer.EntitySets
			from i in ConsolidatedIndex.BuildIndexes(GetSchemaQualifiedName(es), es.ElementType.Properties.Select((EdmProperty p) => Tuple.Create(p.Name, p)))
			select i;
	}

	private static IEnumerable<CreateIndexOperation> FindAddedIndexes(ICollection<ConsolidatedIndex> sourceIndexes, ICollection<ConsolidatedIndex> targetIndexes, ICollection<AlterColumnOperation> alteredColumns, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from i in targetIndexes.Except(sourceIndexes, (ConsolidatedIndex i1, ConsolidatedIndex i2) => IndexesEqual(i1, i2, renamedColumns) && !alteredColumns.Any((AlterColumnOperation ac) => ac.Table.EqualsIgnoreCase(i2.Table) && i2.Columns.Contains<string>(ac.Column.Name, StringComparer.OrdinalIgnoreCase)))
			select i.CreateCreateIndexOperation();
	}

	private static IEnumerable<DropIndexOperation> FindDroppedIndexes(ICollection<ConsolidatedIndex> sourceIndexes, ICollection<ConsolidatedIndex> targetIndexes, ICollection<AlterColumnOperation> alteredColumns, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from i in sourceIndexes.Except(targetIndexes, (ConsolidatedIndex i2, ConsolidatedIndex i1) => IndexesEqual(i1, i2, renamedColumns) && !alteredColumns.Any((AlterColumnOperation ac) => ac.Table.EqualsIgnoreCase(i2.Table) && i2.Columns.Contains<string>(ac.Column.Name, StringComparer.OrdinalIgnoreCase)))
			select i.CreateDropIndexOperation();
	}

	private static bool IndexesEqual(ConsolidatedIndex consolidatedIndex1, ConsolidatedIndex consolidatedIndex2, ICollection<RenameColumnOperation> renamedColumns)
	{
		if (!consolidatedIndex1.Table.EqualsIgnoreCase(consolidatedIndex2.Table))
		{
			return false;
		}
		if (!consolidatedIndex1.Index.Equals(consolidatedIndex2.Index))
		{
			return false;
		}
		return consolidatedIndex1.Columns.Select((string c) => (from rc in renamedColumns
			where rc.Table.EqualsIgnoreCase(consolidatedIndex1.Table) && rc.Name.EqualsIgnoreCase(c)
			select rc.NewName).SingleOrDefault() ?? c).SequenceEqual<string>(consolidatedIndex2.Columns, StringComparer.OrdinalIgnoreCase);
	}

	private static IEnumerable<RenameIndexOperation> FindRenamedIndexes(ICollection<CreateIndexOperation> addedIndexes, ICollection<DropIndexOperation> droppedIndexes, ICollection<AlterColumnOperation> alteredColumns, ICollection<RenameColumnOperation> renamedColumns)
	{
		return from ci1 in addedIndexes.ToList()
			from di in droppedIndexes.ToList()
			let ci2 = (CreateIndexOperation)di.Inverse
			where ci1.Table.EqualsIgnoreCase(ci2.Table) && !ci1.Name.EqualsIgnoreCase(ci2.Name) && ci1.Columns.SequenceEqual<string>(ci2.Columns.Select((string c) => (from rc in renamedColumns
				where rc.Table.EqualsIgnoreCase(ci2.Table) && rc.Name.EqualsIgnoreCase(c)
				select rc.NewName).SingleOrDefault() ?? c), StringComparer.OrdinalIgnoreCase) && ci1.IsClustered == ci2.IsClustered && ci1.IsUnique == ci2.IsUnique && !alteredColumns.Any((AlterColumnOperation ac) => ac.Table.EqualsIgnoreCase(ci1.Table) && ci1.Columns.Contains<string>(ac.Column.Name, StringComparer.OrdinalIgnoreCase)) && addedIndexes.Remove(ci1) && droppedIndexes.Remove(di)
			select new RenameIndexOperation(ci1.Table, di.Name, ci1.Name);
	}

	private bool DiffColumns(EdmProperty column1, EdmProperty column2)
	{
		if (column1.Nullable != column2.Nullable)
		{
			return false;
		}
		if (column1.PrimitiveType.PrimitiveTypeKind != column2.PrimitiveType.PrimitiveTypeKind)
		{
			return false;
		}
		if (column1.StoreGeneratedPattern != column2.StoreGeneratedPattern)
		{
			return false;
		}
		if (!(from a in GetAnnotations(column1)
			orderby a.Key
			select a).SequenceEqual(from a in GetAnnotations(column2)
			orderby a.Key
			select a))
		{
			return false;
		}
		if (_source.ProviderInfo.Equals(_target.ProviderInfo))
		{
			if (column1.TypeName.EqualsIgnoreCase(column2.TypeName))
			{
				return column1.TypeUsage.EdmEquals(column2.TypeUsage);
			}
			return false;
		}
		if (column1.Precision == column2.Precision && column1.Scale == column2.Scale && column1.IsUnicode == column2.IsUnicode)
		{
			return column1.IsFixedLength == column2.IsFixedLength;
		}
		return false;
	}

	private AlterColumnOperation BuildAlterColumnOperation(string table, EdmProperty targetProperty, ModelMetadata targetModelMetadata, EdmProperty sourceProperty, ModelMetadata sourceModelMetadata)
	{
		IDictionary<string, AnnotationValues> dictionary = BuildAnnotationPairs(GetAnnotations(sourceProperty), GetAnnotations(targetProperty));
		Dictionary<string, AnnotationValues> annotations = dictionary.ToDictionary((KeyValuePair<string, AnnotationValues> a) => a.Key, (KeyValuePair<string, AnnotationValues> a) => new AnnotationValues(a.Value.NewValue, a.Value.OldValue));
		ColumnModel columnModel = BuildColumnModel(targetProperty, targetModelMetadata, dictionary);
		ColumnModel columnModel2 = BuildColumnModel(sourceProperty, sourceModelMetadata, annotations);
		columnModel2.Name = columnModel.Name;
		return new AlterColumnOperation(table, columnModel, columnModel.IsNarrowerThan(columnModel2, _target.ProviderManifest), new AlterColumnOperation(table, columnModel2, columnModel2.IsNarrowerThan(columnModel, _target.ProviderManifest)));
	}

	private static IDictionary<string, AnnotationValues> BuildAnnotationPairs(IDictionary<string, object> rawSourceAnnotations, IDictionary<string, object> rawTargetAnnotations)
	{
		Dictionary<string, AnnotationValues> dictionary = new Dictionary<string, AnnotationValues>();
		foreach (string item in rawTargetAnnotations.Keys.Concat(rawSourceAnnotations.Keys).Distinct())
		{
			if (!rawSourceAnnotations.ContainsKey(item))
			{
				dictionary[item] = new AnnotationValues(null, rawTargetAnnotations[item]);
			}
			else if (!rawTargetAnnotations.ContainsKey(item))
			{
				dictionary[item] = new AnnotationValues(rawSourceAnnotations[item], null);
			}
			else if (!object.Equals(rawSourceAnnotations[item], rawTargetAnnotations[item]))
			{
				dictionary[item] = new AnnotationValues(rawSourceAnnotations[item], rawTargetAnnotations[item]);
			}
		}
		return dictionary;
	}

	private IEnumerable<RenameColumnOperation> FindRenamedColumns(ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs, ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs)
	{
		return FindRenamedMappedColumns(mappingFragmentPairs).Concat(FindRenamedForeignKeyColumns(associationTypePairs)).Concat(FindRenamedDiscriminatorColumns(mappingFragmentPairs)).Distinct(new DynamicEqualityComparer<RenameColumnOperation>((RenameColumnOperation c1, RenameColumnOperation c2) => c1.Table.EqualsIgnoreCase(c2.Table) && c1.Name.EqualsIgnoreCase(c2.Name) && c1.NewName.EqualsIgnoreCase(c2.NewName)));
	}

	private static IEnumerable<RenameColumnOperation> FindRenamedMappedColumns(ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs)
	{
		return from mfs in mappingFragmentPairs
			let t = GetSchemaQualifiedName(mfs.Item2.StoreEntitySet)
			from cr in FindRenamedMappedColumns(mfs.Item1, mfs.Item2, t)
			select cr;
	}

	private static IEnumerable<RenameColumnOperation> FindRenamedMappedColumns(MappingFragment mappingFragment1, MappingFragment mappingFragment2, string table)
	{
		return from cmb1 in mappingFragment1.FlattenedProperties
			from cmb2 in mappingFragment2.FlattenedProperties
			where cmb1.PropertyPath.SequenceEqual(cmb2.PropertyPath, new DynamicEqualityComparer<EdmProperty>((EdmProperty p1, EdmProperty p2) => p1.EdmEquals(p2))) && !cmb1.ColumnProperty.Name.EqualsIgnoreCase(cmb2.ColumnProperty.Name)
			select new RenameColumnOperation(table, cmb1.ColumnProperty.Name, cmb2.ColumnProperty.Name);
	}

	private IEnumerable<RenameColumnOperation> FindRenamedForeignKeyColumns(ICollection<Tuple<AssociationType, AssociationType>> associationTypePairs)
	{
		return from ats in associationTypePairs
			let rc1 = ats.Item1.Constraint
			let rc2 = ats.Item2.Constraint
			from ps in EntityUtil.Zip(rc1.ToProperties, rc2.ToProperties)
			where !ps.Key.Name.EqualsIgnoreCase(ps.Value.Name) && (!rc2.DependentEnd.GetEntityType().Properties.Any((EdmProperty p) => p.Name.EqualsIgnoreCase(ps.Key.Name)) || rc1.DependentEnd.GetEntityType().Properties.Any((EdmProperty p) => p.Name.EqualsIgnoreCase(ps.Value.Name)))
			select new RenameColumnOperation(GetSchemaQualifiedName(_target.StoreEntityContainer.EntitySets.Single((EntitySet es) => es.ElementType == rc2.DependentEnd.GetEntityType())), ps.Key.Name, ps.Value.Name);
	}

	private static IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns(ICollection<Tuple<MappingFragment, MappingFragment>> mappingFragmentPairs)
	{
		return from mfs in mappingFragmentPairs
			let t = GetSchemaQualifiedName(mfs.Item2.StoreEntitySet)
			from cr in FindRenamedDiscriminatorColumns(mfs.Item1, mfs.Item2, t)
			select cr;
	}

	private static IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns(MappingFragment mappingFragment1, MappingFragment mappingFragment2, string table)
	{
		return from c1 in mappingFragment1.Conditions
			from c2 in mappingFragment2.Conditions
			where object.Equals(c1.Value, c2.Value)
			where !c1.Column.Name.EqualsIgnoreCase(c2.Column.Name)
			select new RenameColumnOperation(table, c1.Column.Name, c2.Column.Name);
	}

	private static CreateTableOperation BuildCreateTableOperation(EntitySet entitySet, ModelMetadata modelMetadata)
	{
		CreateTableOperation createTableOperation = new CreateTableOperation(GetSchemaQualifiedName(entitySet), GetAnnotations(entitySet.ElementType));
		entitySet.ElementType.Properties.Each(delegate(EdmProperty p)
		{
			createTableOperation.Columns.Add(BuildColumnModel(p, modelMetadata, GetAnnotations(p).ToDictionary((KeyValuePair<string, object> a) => a.Key, (KeyValuePair<string, object> a) => new AnnotationValues(null, a.Value))));
		});
		AddPrimaryKeyOperation addPrimaryKeyOperation = new AddPrimaryKeyOperation();
		entitySet.ElementType.KeyProperties.Each(delegate(EdmProperty p)
		{
			addPrimaryKeyOperation.Columns.Add(p.Name);
		});
		IndexAttribute primaryKeyIndexAttribute = GetPrimaryKeyIndexAttribute(entitySet.ElementType);
		if (primaryKeyIndexAttribute != null)
		{
			addPrimaryKeyOperation.Name = primaryKeyIndexAttribute.Name;
			if (primaryKeyIndexAttribute.IsClusteredConfigured)
			{
				addPrimaryKeyOperation.IsClustered = primaryKeyIndexAttribute.IsClustered;
			}
		}
		createTableOperation.PrimaryKey = addPrimaryKeyOperation;
		return createTableOperation;
	}

	private static ColumnModel BuildColumnModel(EdmProperty property, ModelMetadata modelMetadata, IDictionary<string, AnnotationValues> annotations)
	{
		TypeUsage edmType = modelMetadata.ProviderManifest.GetEdmType(property.TypeUsage);
		TypeUsage storeType = modelMetadata.ProviderManifest.GetStoreType(edmType);
		return BuildColumnModel(property, edmType, storeType, annotations);
	}

	public static ColumnModel BuildColumnModel(EdmProperty property, TypeUsage conceptualTypeUsage, TypeUsage defaultStoreTypeUsage, IDictionary<string, AnnotationValues> annotations)
	{
		ColumnModel columnModel = new ColumnModel(property.PrimitiveType.PrimitiveTypeKind, conceptualTypeUsage)
		{
			Name = property.Name,
			IsNullable = ((!property.Nullable) ? new bool?(false) : null),
			StoreType = ((!property.TypeName.EqualsIgnoreCase(defaultStoreTypeUsage.EdmType.Name)) ? property.TypeName : null),
			IsIdentity = (property.IsStoreGeneratedIdentity && _validIdentityTypes.Contains(property.PrimitiveType.PrimitiveTypeKind)),
			IsTimestamp = (property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary && property.MaxLength == 8 && property.IsStoreGeneratedComputed),
			IsUnicode = ((property.IsUnicode == false) ? new bool?(false) : null),
			IsFixedLength = ((property.IsFixedLength == true) ? new bool?(true) : null),
			Annotations = annotations
		};
		if (property.TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: true, out var item) && !item.IsUnbounded && !item.Description.IsConstant)
		{
			columnModel.MaxLength = (int?)item.Value;
		}
		if (property.TypeUsage.Facets.TryGetValue("Precision", ignoreCase: true, out item) && !item.IsUnbounded && !item.Description.IsConstant)
		{
			columnModel.Precision = (byte?)item.Value;
		}
		if (property.TypeUsage.Facets.TryGetValue("Scale", ignoreCase: true, out item) && !item.IsUnbounded && !item.Description.IsConstant)
		{
			columnModel.Scale = (byte?)item.Value;
		}
		return columnModel;
	}

	private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
	{
		return DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(providerInfo.ProviderInvariantName).GetProviderServices().GetProviderManifest(providerInfo.ProviderManifestToken);
	}

	private static string GetSchemaQualifiedName(EntitySet entitySet)
	{
		return new DatabaseName(entitySet.Table, entitySet.Schema).ToString();
	}

	private static string GetSchemaQualifiedName(EdmFunction function)
	{
		return new DatabaseName(function.FunctionName, function.Schema).ToString();
	}
}
