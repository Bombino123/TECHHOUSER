using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading;

namespace System.Data.Entity.Core.Mapping.Update.Internal;

internal class ViewLoader
{
	private readonly StorageMappingItemCollection m_mappingCollection;

	private readonly Dictionary<AssociationSet, AssociationSetMetadata> m_associationSetMetadata = new Dictionary<AssociationSet, AssociationSetMetadata>();

	private readonly Dictionary<EntitySetBase, Set<EntitySet>> m_affectedTables = new Dictionary<EntitySetBase, Set<EntitySet>>();

	private readonly Set<EdmMember> m_serverGenProperties = new Set<EdmMember>();

	private readonly Set<EdmMember> m_isNullConditionProperties = new Set<EdmMember>();

	private readonly Dictionary<EntitySetBase, ModificationFunctionMappingTranslator> m_functionMappingTranslators = new Dictionary<EntitySetBase, ModificationFunctionMappingTranslator>(EqualityComparer<EntitySetBase>.Default);

	private readonly ReaderWriterLockSlim m_readerWriterLock = new ReaderWriterLockSlim();

	internal ViewLoader(StorageMappingItemCollection mappingCollection)
	{
		m_mappingCollection = mappingCollection;
	}

	internal ModificationFunctionMappingTranslator GetFunctionMappingTranslator(EntitySetBase extent, MetadataWorkspace workspace)
	{
		return SyncGetValue(extent, workspace, m_functionMappingTranslators, extent);
	}

	internal Set<EntitySet> GetAffectedTables(EntitySetBase extent, MetadataWorkspace workspace)
	{
		return SyncGetValue(extent, workspace, m_affectedTables, extent);
	}

	internal AssociationSetMetadata GetAssociationSetMetadata(AssociationSet associationSet, MetadataWorkspace workspace)
	{
		return SyncGetValue(associationSet, workspace, m_associationSetMetadata, associationSet);
	}

	internal bool IsServerGen(EntitySetBase entitySetBase, MetadataWorkspace workspace, EdmMember member)
	{
		return SyncContains(entitySetBase, workspace, m_serverGenProperties, member);
	}

	internal bool IsNullConditionMember(EntitySetBase entitySetBase, MetadataWorkspace workspace, EdmMember member)
	{
		return SyncContains(entitySetBase, workspace, m_isNullConditionProperties, member);
	}

	private T_Value SyncGetValue<T_Key, T_Value>(EntitySetBase entitySetBase, MetadataWorkspace workspace, Dictionary<T_Key, T_Value> dictionary, T_Key key)
	{
		return SyncInitializeEntitySet(entitySetBase, workspace, (T_Key k) => dictionary[k], key);
	}

	private bool SyncContains<T_Element>(EntitySetBase entitySetBase, MetadataWorkspace workspace, Set<T_Element> set, T_Element element)
	{
		return SyncInitializeEntitySet(entitySetBase, workspace, set.Contains, element);
	}

	private TResult SyncInitializeEntitySet<TArg, TResult>(EntitySetBase entitySetBase, MetadataWorkspace workspace, Func<TArg, TResult> evaluate, TArg arg)
	{
		m_readerWriterLock.EnterReadLock();
		try
		{
			if (m_affectedTables.ContainsKey(entitySetBase))
			{
				return evaluate(arg);
			}
		}
		finally
		{
			m_readerWriterLock.ExitReadLock();
		}
		m_readerWriterLock.EnterWriteLock();
		try
		{
			if (m_affectedTables.ContainsKey(entitySetBase))
			{
				return evaluate(arg);
			}
			InitializeEntitySet(entitySetBase, workspace);
			return evaluate(arg);
		}
		finally
		{
			m_readerWriterLock.ExitWriteLock();
		}
	}

	private void InitializeEntitySet(EntitySetBase entitySetBase, MetadataWorkspace workspace)
	{
		EntityContainerMapping entityContainerMapping = (EntityContainerMapping)m_mappingCollection.GetMap(entitySetBase.EntityContainer);
		if (entityContainerMapping.HasViews)
		{
			m_mappingCollection.GetGeneratedView(entitySetBase, workspace);
		}
		Set<EntitySet> set = new Set<EntitySet>();
		if (entityContainerMapping != null)
		{
			Set<EdmMember> set2 = new Set<EdmMember>();
			EntitySetBaseMapping entitySetBaseMapping;
			if (entitySetBase.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
			{
				entitySetBaseMapping = entityContainerMapping.GetEntitySetMapping(entitySetBase.Name);
				m_serverGenProperties.Unite(GetMembersWithResultBinding((EntitySetMapping)entitySetBaseMapping));
			}
			else
			{
				if (entitySetBase.BuiltInTypeKind != BuiltInTypeKind.AssociationSet)
				{
					throw new NotSupportedException();
				}
				entitySetBaseMapping = entityContainerMapping.GetAssociationSetMapping(entitySetBase.Name);
			}
			foreach (MappingFragment mappingFragment in GetMappingFragments(entitySetBaseMapping))
			{
				set.Add(mappingFragment.TableSet);
				m_serverGenProperties.AddRange(FindServerGenMembers(mappingFragment));
				set2.AddRange(FindIsNullConditionColumns(mappingFragment));
			}
			if (0 < set2.Count)
			{
				foreach (MappingFragment mappingFragment2 in GetMappingFragments(entitySetBaseMapping))
				{
					m_isNullConditionProperties.AddRange(FindPropertiesMappedToColumns(set2, mappingFragment2));
				}
			}
		}
		m_affectedTables.Add(entitySetBase, set.MakeReadOnly());
		InitializeFunctionMappingTranslators(entitySetBase, entityContainerMapping);
		if (entitySetBase.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
		{
			AssociationSet associationSet = (AssociationSet)entitySetBase;
			if (!m_associationSetMetadata.ContainsKey(associationSet))
			{
				m_associationSetMetadata.Add(associationSet, new AssociationSetMetadata(m_affectedTables[associationSet], associationSet, workspace));
			}
		}
	}

	private static IEnumerable<EdmMember> GetMembersWithResultBinding(EntitySetMapping entitySetMapping)
	{
		foreach (EntityTypeModificationFunctionMapping typeFunctionMapping in entitySetMapping.ModificationFunctionMappings)
		{
			if (typeFunctionMapping.InsertFunctionMapping != null && typeFunctionMapping.InsertFunctionMapping.ResultBindings != null)
			{
				foreach (ModificationFunctionResultBinding resultBinding in typeFunctionMapping.InsertFunctionMapping.ResultBindings)
				{
					yield return resultBinding.Property;
				}
			}
			if (typeFunctionMapping.UpdateFunctionMapping == null || typeFunctionMapping.UpdateFunctionMapping.ResultBindings == null)
			{
				continue;
			}
			foreach (ModificationFunctionResultBinding resultBinding2 in typeFunctionMapping.UpdateFunctionMapping.ResultBindings)
			{
				yield return resultBinding2.Property;
			}
		}
	}

	private void InitializeFunctionMappingTranslators(EntitySetBase entitySetBase, EntityContainerMapping mapping)
	{
		KeyToListMap<AssociationSet, AssociationEndMember> keyToListMap = new KeyToListMap<AssociationSet, AssociationEndMember>(EqualityComparer<AssociationSet>.Default);
		if (!m_functionMappingTranslators.ContainsKey(entitySetBase))
		{
			foreach (EntitySetMapping entitySetMap in mapping.EntitySetMaps)
			{
				if (0 < entitySetMap.ModificationFunctionMappings.Count)
				{
					m_functionMappingTranslators.Add(entitySetMap.Set, ModificationFunctionMappingTranslator.CreateEntitySetTranslator(entitySetMap));
					foreach (AssociationSetEnd implicitlyMappedAssociationSetEnd in entitySetMap.ImplicitlyMappedAssociationSetEnds)
					{
						AssociationSet parentAssociationSet = implicitlyMappedAssociationSetEnd.ParentAssociationSet;
						if (!m_functionMappingTranslators.ContainsKey(parentAssociationSet))
						{
							m_functionMappingTranslators.Add(parentAssociationSet, ModificationFunctionMappingTranslator.CreateAssociationSetTranslator(null));
						}
						AssociationSetEnd oppositeEnd = MetadataHelper.GetOppositeEnd(implicitlyMappedAssociationSetEnd);
						keyToListMap.Add(parentAssociationSet, oppositeEnd.CorrespondingAssociationEndMember);
					}
				}
				else
				{
					m_functionMappingTranslators.Add(entitySetMap.Set, null);
				}
			}
			foreach (AssociationSetMapping relationshipSetMap in mapping.RelationshipSetMaps)
			{
				if (relationshipSetMap.ModificationFunctionMapping != null)
				{
					AssociationSet key = (AssociationSet)relationshipSetMap.Set;
					m_functionMappingTranslators.Add(key, ModificationFunctionMappingTranslator.CreateAssociationSetTranslator(relationshipSetMap));
					keyToListMap.AddRange(key, Enumerable.Empty<AssociationEndMember>());
				}
				else if (!m_functionMappingTranslators.ContainsKey(relationshipSetMap.Set))
				{
					m_functionMappingTranslators.Add(relationshipSetMap.Set, null);
				}
			}
		}
		foreach (AssociationSet key2 in keyToListMap.Keys)
		{
			m_associationSetMetadata.Add(key2, new AssociationSetMetadata(keyToListMap.EnumerateValues(key2)));
		}
	}

	private static IEnumerable<EdmMember> FindServerGenMembers(MappingFragment mappingFragment)
	{
		foreach (ScalarPropertyMapping item in FlattenPropertyMappings(mappingFragment.AllProperties).OfType<ScalarPropertyMapping>())
		{
			if (MetadataHelper.GetStoreGeneratedPattern(item.Column) != 0)
			{
				yield return item.Property;
			}
		}
	}

	private static IEnumerable<EdmMember> FindIsNullConditionColumns(MappingFragment mappingFragment)
	{
		foreach (ConditionPropertyMapping item in FlattenPropertyMappings(mappingFragment.AllProperties).OfType<ConditionPropertyMapping>())
		{
			if (item.Column != null && item.IsNull.HasValue)
			{
				yield return item.Column;
			}
		}
	}

	private static IEnumerable<EdmMember> FindPropertiesMappedToColumns(Set<EdmMember> columns, MappingFragment mappingFragment)
	{
		foreach (ScalarPropertyMapping item in FlattenPropertyMappings(mappingFragment.AllProperties).OfType<ScalarPropertyMapping>())
		{
			if (columns.Contains(item.Column))
			{
				yield return item.Property;
			}
		}
	}

	private static IEnumerable<MappingFragment> GetMappingFragments(EntitySetBaseMapping setMapping)
	{
		foreach (TypeMapping typeMapping in setMapping.TypeMappings)
		{
			foreach (MappingFragment mappingFragment in typeMapping.MappingFragments)
			{
				yield return mappingFragment;
			}
		}
	}

	private static IEnumerable<PropertyMapping> FlattenPropertyMappings(ReadOnlyCollection<PropertyMapping> propertyMappings)
	{
		foreach (PropertyMapping propertyMapping in propertyMappings)
		{
			if (propertyMapping is ComplexPropertyMapping complexPropertyMapping)
			{
				foreach (ComplexTypeMapping typeMapping in complexPropertyMapping.TypeMappings)
				{
					foreach (PropertyMapping item in FlattenPropertyMappings(typeMapping.AllProperties))
					{
						yield return item;
					}
				}
			}
			else
			{
				yield return propertyMapping;
			}
		}
	}
}
