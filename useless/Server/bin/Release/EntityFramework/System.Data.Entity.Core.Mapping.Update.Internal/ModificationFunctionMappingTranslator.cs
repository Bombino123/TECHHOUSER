using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal abstract class ModificationFunctionMappingTranslator
{
	private sealed class EntitySetTranslator : ModificationFunctionMappingTranslator
	{
		private readonly Dictionary<EntityType, EntityTypeModificationFunctionMapping> m_typeMappings;

		internal EntitySetTranslator(EntitySetMapping setMapping)
		{
			m_typeMappings = new Dictionary<EntityType, EntityTypeModificationFunctionMapping>();
			foreach (EntityTypeModificationFunctionMapping modificationFunctionMapping in setMapping.ModificationFunctionMappings)
			{
				m_typeMappings.Add(modificationFunctionMapping.EntityType, modificationFunctionMapping);
			}
		}

		internal override FunctionUpdateCommand Translate(UpdateTranslator translator, ExtractedStateEntry stateEntry)
		{
			ModificationFunctionMapping item = GetFunctionMapping(stateEntry).Item2;
			EntityKey entityKey = stateEntry.Source.EntityKey;
			HashSet<IEntityStateEntry> hashSet = new HashSet<IEntityStateEntry> { stateEntry.Source };
			IEnumerable<Tuple<AssociationEndMember, IEntityStateEntry>> enumerable = from end in item.CollocatedAssociationSetEnds
				join candidateEntry in translator.GetRelationships(entityKey) on end.CorrespondingAssociationEndMember.DeclaringType equals candidateEntry.EntitySet.ElementType
				select Tuple.Create(end.CorrespondingAssociationEndMember, candidateEntry);
			Dictionary<AssociationEndMember, IEntityStateEntry> dictionary = new Dictionary<AssociationEndMember, IEntityStateEntry>();
			Dictionary<AssociationEndMember, IEntityStateEntry> dictionary2 = new Dictionary<AssociationEndMember, IEntityStateEntry>();
			foreach (Tuple<AssociationEndMember, IEntityStateEntry> item2 in enumerable)
			{
				ProcessReferenceCandidate(entityKey, hashSet, dictionary, dictionary2, item2.Item1, item2.Item2);
			}
			FunctionUpdateCommand functionUpdateCommand;
			if (hashSet.All((IEntityStateEntry e) => e.State == EntityState.Unchanged))
			{
				functionUpdateCommand = null;
			}
			else
			{
				functionUpdateCommand = new FunctionUpdateCommand(item, translator, new ReadOnlyCollection<IEntityStateEntry>(hashSet.ToList()), stateEntry);
				BindFunctionParameters(translator, stateEntry, item, functionUpdateCommand, dictionary, dictionary2);
				if (item.ResultBindings != null)
				{
					foreach (ModificationFunctionResultBinding resultBinding in item.ResultBindings)
					{
						PropagatorResult memberValue = stateEntry.Current.GetMemberValue(resultBinding.Property);
						functionUpdateCommand.AddResultColumn(translator, resultBinding.ColumnName, memberValue);
					}
				}
			}
			return functionUpdateCommand;
		}

		private static void ProcessReferenceCandidate(EntityKey source, HashSet<IEntityStateEntry> stateEntries, Dictionary<AssociationEndMember, IEntityStateEntry> currentReferenceEnd, Dictionary<AssociationEndMember, IEntityStateEntry> originalReferenceEnd, AssociationEndMember endMember, IEntityStateEntry candidateEntry)
		{
			Func<DbDataRecord, int, EntityKey> getEntityKey = (DbDataRecord record, int ordinal) => (EntityKey)record[ordinal];
			Action<DbDataRecord, Action<IEntityStateEntry>> action = delegate(DbDataRecord record, Action<IEntityStateEntry> registerTarget)
			{
				int arg = ((record.GetOrdinal(endMember.Name) == 0) ? 1 : 0);
				if (getEntityKey(record, arg) == source)
				{
					stateEntries.Add(candidateEntry);
					registerTarget(candidateEntry);
				}
			};
			switch (candidateEntry.State)
			{
			case EntityState.Unchanged:
				action(candidateEntry.CurrentValues, delegate(IEntityStateEntry target)
				{
					currentReferenceEnd.Add(endMember, target);
					originalReferenceEnd.Add(endMember, target);
				});
				break;
			case EntityState.Added:
				action(candidateEntry.CurrentValues, delegate(IEntityStateEntry target)
				{
					currentReferenceEnd.Add(endMember, target);
				});
				break;
			case EntityState.Deleted:
				action(candidateEntry.OriginalValues, delegate(IEntityStateEntry target)
				{
					originalReferenceEnd.Add(endMember, target);
				});
				break;
			}
		}

		private Tuple<EntityTypeModificationFunctionMapping, ModificationFunctionMapping> GetFunctionMapping(ExtractedStateEntry stateEntry)
		{
			EntityType entityType = ((stateEntry.Current == null) ? ((EntityType)stateEntry.Original.StructuralType) : ((EntityType)stateEntry.Current.StructuralType));
			EntityTypeModificationFunctionMapping entityTypeModificationFunctionMapping = m_typeMappings[entityType];
			ModificationFunctionMapping modificationFunctionMapping;
			switch (stateEntry.State)
			{
			case EntityState.Added:
				modificationFunctionMapping = entityTypeModificationFunctionMapping.InsertFunctionMapping;
				EntityUtil.ValidateNecessaryModificationFunctionMapping(modificationFunctionMapping, "Insert", stateEntry.Source, "EntityType", entityType.Name);
				break;
			case EntityState.Deleted:
				modificationFunctionMapping = entityTypeModificationFunctionMapping.DeleteFunctionMapping;
				EntityUtil.ValidateNecessaryModificationFunctionMapping(modificationFunctionMapping, "Delete", stateEntry.Source, "EntityType", entityType.Name);
				break;
			case EntityState.Unchanged:
			case EntityState.Modified:
				modificationFunctionMapping = entityTypeModificationFunctionMapping.UpdateFunctionMapping;
				EntityUtil.ValidateNecessaryModificationFunctionMapping(modificationFunctionMapping, "Update", stateEntry.Source, "EntityType", entityType.Name);
				break;
			default:
				modificationFunctionMapping = null;
				break;
			}
			return Tuple.Create(entityTypeModificationFunctionMapping, modificationFunctionMapping);
		}

		private static void BindFunctionParameters(UpdateTranslator translator, ExtractedStateEntry stateEntry, ModificationFunctionMapping functionMapping, FunctionUpdateCommand command, Dictionary<AssociationEndMember, IEntityStateEntry> currentReferenceEnds, Dictionary<AssociationEndMember, IEntityStateEntry> originalReferenceEnds)
		{
			foreach (ModificationFunctionParameterBinding parameterBinding in functionMapping.ParameterBindings)
			{
				PropagatorResult propagatorResult;
				if (parameterBinding.MemberPath.AssociationSetEnd != null)
				{
					AssociationEndMember correspondingAssociationEndMember = parameterBinding.MemberPath.AssociationSetEnd.CorrespondingAssociationEndMember;
					if (!(parameterBinding.IsCurrent ? currentReferenceEnds.TryGetValue(correspondingAssociationEndMember, out var value) : originalReferenceEnds.TryGetValue(correspondingAssociationEndMember, out value)))
					{
						if (correspondingAssociationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
						{
							string name = stateEntry.Source.EntitySet.Name;
							string name2 = parameterBinding.MemberPath.AssociationSetEnd.ParentAssociationSet.Name;
							throw new UpdateException(Strings.Update_MissingRequiredRelationshipValue(name, name2), null, command.GetStateEntries(translator).Cast<ObjectStateEntry>().Distinct());
						}
						propagatorResult = PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, null);
					}
					else
					{
						PropagatorResult memberValue = (parameterBinding.IsCurrent ? translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(value, ModifiedPropertiesBehavior.AllModified) : translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(value, ModifiedPropertiesBehavior.AllModified)).GetMemberValue(correspondingAssociationEndMember);
						EdmProperty member = (EdmProperty)parameterBinding.MemberPath.Members[0];
						propagatorResult = memberValue.GetMemberValue(member);
					}
				}
				else
				{
					propagatorResult = (parameterBinding.IsCurrent ? stateEntry.Current : stateEntry.Original);
					int num = parameterBinding.MemberPath.Members.Count;
					while (num > 0)
					{
						num--;
						EdmMember member2 = parameterBinding.MemberPath.Members[num];
						propagatorResult = propagatorResult.GetMemberValue(member2);
					}
				}
				command.SetParameterValue(propagatorResult, parameterBinding, translator);
			}
			command.RegisterRowsAffectedParameter(functionMapping.RowsAffectedParameter);
		}
	}

	private sealed class AssociationSetTranslator : ModificationFunctionMappingTranslator
	{
		private readonly AssociationSetModificationFunctionMapping m_mapping;

		internal AssociationSetTranslator(AssociationSetMapping setMapping)
		{
			if (setMapping != null)
			{
				m_mapping = setMapping.ModificationFunctionMapping;
			}
		}

		internal override FunctionUpdateCommand Translate(UpdateTranslator translator, ExtractedStateEntry stateEntry)
		{
			if (m_mapping == null)
			{
				return null;
			}
			bool flag = EntityState.Added == stateEntry.State;
			EntityUtil.ValidateNecessaryModificationFunctionMapping(flag ? m_mapping.InsertFunctionMapping : m_mapping.DeleteFunctionMapping, flag ? "Insert" : "Delete", stateEntry.Source, "AssociationSet", m_mapping.AssociationSet.Name);
			ModificationFunctionMapping modificationFunctionMapping = (flag ? m_mapping.InsertFunctionMapping : m_mapping.DeleteFunctionMapping);
			FunctionUpdateCommand functionUpdateCommand = new FunctionUpdateCommand(modificationFunctionMapping, translator, new ReadOnlyCollection<IEntityStateEntry>(new IEntityStateEntry[1] { stateEntry.Source }.ToList()), stateEntry);
			PropagatorResult propagatorResult = ((!flag) ? stateEntry.Original : stateEntry.Current);
			foreach (ModificationFunctionParameterBinding parameterBinding in modificationFunctionMapping.ParameterBindings)
			{
				EdmProperty member = (EdmProperty)parameterBinding.MemberPath.Members[0];
				AssociationEndMember member2 = (AssociationEndMember)parameterBinding.MemberPath.Members[1];
				PropagatorResult memberValue = propagatorResult.GetMemberValue(member2).GetMemberValue(member);
				functionUpdateCommand.SetParameterValue(memberValue, parameterBinding, translator);
			}
			functionUpdateCommand.RegisterRowsAffectedParameter(modificationFunctionMapping.RowsAffectedParameter);
			return functionUpdateCommand;
		}
	}

	internal abstract FunctionUpdateCommand Translate(UpdateTranslator translator, ExtractedStateEntry stateEntry);

	internal static ModificationFunctionMappingTranslator CreateEntitySetTranslator(EntitySetMapping setMapping)
	{
		return new EntitySetTranslator(setMapping);
	}

	internal static ModificationFunctionMappingTranslator CreateAssociationSetTranslator(AssociationSetMapping setMapping)
	{
		return new AssociationSetTranslator(setMapping);
	}
}
