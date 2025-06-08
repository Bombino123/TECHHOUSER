using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ColumnMapProcessor
{
	private readonly IEnumerator<Var> m_varList;

	private readonly VarInfo m_varInfo;

	private readonly VarRefColumnMap m_columnMap;

	private readonly StructuredTypeInfo m_typeInfo;

	private const string c_TypeIdColumnName = "__TypeId";

	private const string c_EntitySetIdColumnName = "__EntitySetId";

	private const string c_NullSentinelColumnName = "__NullSentinel";

	internal ColumnMap ExpandColumnMap()
	{
		if (m_varInfo.Kind == VarInfoKind.CollectionVarInfo)
		{
			return new VarRefColumnMap(m_columnMap.Var.Type, m_columnMap.Name, ((CollectionVarInfo)m_varInfo).NewVar);
		}
		if (m_varInfo.Kind == VarInfoKind.PrimitiveTypeVarInfo)
		{
			return new VarRefColumnMap(m_columnMap.Var.Type, m_columnMap.Name, ((PrimitiveTypeVarInfo)m_varInfo).NewVar);
		}
		return CreateColumnMap(m_columnMap.Var.Type, m_columnMap.Name);
	}

	internal ColumnMapProcessor(VarRefColumnMap columnMap, VarInfo varInfo, StructuredTypeInfo typeInfo)
	{
		m_columnMap = columnMap;
		m_varInfo = varInfo;
		PlanCompiler.Assert(varInfo.NewVars != null && varInfo.NewVars.Count > 0, "No new Vars specified");
		m_varList = varInfo.NewVars.GetEnumerator();
		m_typeInfo = typeInfo;
	}

	private Var GetNextVar()
	{
		if (m_varList.MoveNext())
		{
			return m_varList.Current;
		}
		PlanCompiler.Assert(condition: false, "Could not GetNextVar");
		return null;
	}

	private ColumnMap CreateColumnMap(TypeUsage type, string name)
	{
		if (!TypeUtils.IsStructuredType(type))
		{
			return CreateSimpleColumnMap(type, name);
		}
		return CreateStructuralColumnMap(type, name);
	}

	private ComplexTypeColumnMap CreateComplexTypeColumnMap(TypeInfo typeInfo, string name, ComplexTypeColumnMap superTypeColumnMap, Dictionary<object, TypedColumnMap> discriminatorMap, List<TypedColumnMap> allMaps)
	{
		List<ColumnMap> list = new List<ColumnMap>();
		IEnumerable enumerable = null;
		SimpleColumnMap nullSentinel = null;
		if (typeInfo.HasNullSentinelProperty)
		{
			nullSentinel = CreateSimpleColumnMap(Helper.GetModelTypeUsage(typeInfo.NullSentinelProperty), "__NullSentinel");
		}
		if (superTypeColumnMap != null)
		{
			ColumnMap[] properties = superTypeColumnMap.Properties;
			foreach (ColumnMap item in properties)
			{
				list.Add(item);
			}
			enumerable = TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type);
		}
		else
		{
			enumerable = TypeHelpers.GetAllStructuralMembers(typeInfo.Type);
		}
		foreach (EdmMember item3 in enumerable)
		{
			ColumnMap item2 = CreateColumnMap(Helper.GetModelTypeUsage(item3), item3.Name);
			list.Add(item2);
		}
		ComplexTypeColumnMap complexTypeColumnMap = new ComplexTypeColumnMap(typeInfo.Type, name, list.ToArray(), nullSentinel);
		if (discriminatorMap != null)
		{
			discriminatorMap[typeInfo.TypeId] = complexTypeColumnMap;
		}
		allMaps?.Add(complexTypeColumnMap);
		foreach (TypeInfo immediateSubType in typeInfo.ImmediateSubTypes)
		{
			CreateComplexTypeColumnMap(immediateSubType, name, complexTypeColumnMap, discriminatorMap, allMaps);
		}
		return complexTypeColumnMap;
	}

	private EntityColumnMap CreateEntityColumnMap(TypeInfo typeInfo, string name, EntityColumnMap superTypeColumnMap, Dictionary<object, TypedColumnMap> discriminatorMap, List<TypedColumnMap> allMaps, bool handleRelProperties)
	{
		EntityColumnMap entityColumnMap = null;
		List<ColumnMap> list = new List<ColumnMap>();
		if (superTypeColumnMap != null)
		{
			ColumnMap[] properties = superTypeColumnMap.Properties;
			foreach (ColumnMap item in properties)
			{
				list.Add(item);
			}
			foreach (EdmMember declaredStructuralMember in TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type))
			{
				ColumnMap item2 = CreateColumnMap(Helper.GetModelTypeUsage(declaredStructuralMember), declaredStructuralMember.Name);
				list.Add(item2);
			}
			entityColumnMap = new EntityColumnMap(typeInfo.Type, name, list.ToArray(), superTypeColumnMap.EntityIdentity);
		}
		else
		{
			SimpleColumnMap entitySetIdColumnMap = null;
			if (typeInfo.HasEntitySetIdProperty)
			{
				entitySetIdColumnMap = CreateEntitySetIdColumnMap(typeInfo.EntitySetIdProperty);
			}
			List<SimpleColumnMap> list2 = new List<SimpleColumnMap>();
			Dictionary<EdmProperty, ColumnMap> dictionary = new Dictionary<EdmProperty, ColumnMap>();
			foreach (EdmMember declaredStructuralMember2 in TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type))
			{
				ColumnMap columnMap = CreateColumnMap(Helper.GetModelTypeUsage(declaredStructuralMember2), declaredStructuralMember2.Name);
				list.Add(columnMap);
				if (TypeSemantics.IsPartOfKey(declaredStructuralMember2))
				{
					EdmProperty edmProperty = declaredStructuralMember2 as EdmProperty;
					PlanCompiler.Assert(edmProperty != null, "EntityType key member is not property?");
					dictionary[edmProperty] = columnMap;
				}
			}
			foreach (EdmMember keyMember in TypeHelpers.GetEdmType<EntityType>(typeInfo.Type).KeyMembers)
			{
				EdmProperty edmProperty2 = keyMember as EdmProperty;
				PlanCompiler.Assert(edmProperty2 != null, "EntityType key member is not property?");
				SimpleColumnMap simpleColumnMap = dictionary[edmProperty2] as SimpleColumnMap;
				PlanCompiler.Assert(simpleColumnMap != null, "keyColumnMap is null");
				list2.Add(simpleColumnMap);
			}
			EntityIdentity entityIdentity = CreateEntityIdentity((EntityType)typeInfo.Type.EdmType, entitySetIdColumnMap, list2.ToArray());
			entityColumnMap = new EntityColumnMap(typeInfo.Type, name, list.ToArray(), entityIdentity);
		}
		if (discriminatorMap != null && typeInfo.TypeId != null)
		{
			discriminatorMap[typeInfo.TypeId] = entityColumnMap;
		}
		allMaps?.Add(entityColumnMap);
		foreach (TypeInfo immediateSubType in typeInfo.ImmediateSubTypes)
		{
			CreateEntityColumnMap(immediateSubType, name, entityColumnMap, discriminatorMap, allMaps, handleRelProperties: false);
		}
		if (handleRelProperties)
		{
			BuildRelPropertyColumnMaps(typeInfo, includeSupertypeRelProperties: true);
		}
		return entityColumnMap;
	}

	private void BuildRelPropertyColumnMaps(TypeInfo typeInfo, bool includeSupertypeRelProperties)
	{
		IEnumerable<RelProperty> enumerable = null;
		enumerable = ((!includeSupertypeRelProperties) ? m_typeInfo.RelPropertyHelper.GetDeclaredOnlyRelProperties(typeInfo.Type.EdmType as EntityTypeBase) : m_typeInfo.RelPropertyHelper.GetRelProperties(typeInfo.Type.EdmType as EntityTypeBase));
		foreach (RelProperty item in enumerable)
		{
			CreateColumnMap(item.ToEnd.TypeUsage, item.ToString());
		}
		foreach (TypeInfo immediateSubType in typeInfo.ImmediateSubTypes)
		{
			BuildRelPropertyColumnMaps(immediateSubType, includeSupertypeRelProperties: false);
		}
	}

	private SimpleColumnMap CreateEntitySetIdColumnMap(EdmProperty prop)
	{
		return CreateSimpleColumnMap(Helper.GetModelTypeUsage(prop), "__EntitySetId");
	}

	private SimplePolymorphicColumnMap CreatePolymorphicColumnMap(TypeInfo typeInfo, string name)
	{
		Dictionary<object, TypedColumnMap> dictionary = new Dictionary<object, TypedColumnMap>((typeInfo.RootType.DiscriminatorMap == null) ? null : TrailingSpaceComparer.Instance);
		List<TypedColumnMap> list = new List<TypedColumnMap>();
		TypeInfo rootType = typeInfo.RootType;
		SimpleColumnMap typeDiscriminator = CreateTypeIdColumnMap(rootType.TypeIdProperty);
		if (TypeSemantics.IsComplexType(typeInfo.Type))
		{
			CreateComplexTypeColumnMap(rootType, name, null, dictionary, list);
		}
		else
		{
			CreateEntityColumnMap(rootType, name, null, dictionary, list, handleRelProperties: true);
		}
		TypedColumnMap typedColumnMap = null;
		foreach (TypedColumnMap item in list)
		{
			if (TypeSemantics.IsStructurallyEqual(item.Type, typeInfo.Type))
			{
				typedColumnMap = item;
				break;
			}
		}
		PlanCompiler.Assert(typedColumnMap != null, "Didn't find requested type in polymorphic type hierarchy?");
		return new SimplePolymorphicColumnMap(typeInfo.Type, name, typedColumnMap.Properties, typeDiscriminator, dictionary);
	}

	private RecordColumnMap CreateRecordColumnMap(TypeInfo typeInfo, string name)
	{
		PlanCompiler.Assert(typeInfo.Type.EdmType is RowType, "not RowType");
		SimpleColumnMap nullSentinel = null;
		if (typeInfo.HasNullSentinelProperty)
		{
			nullSentinel = CreateSimpleColumnMap(Helper.GetModelTypeUsage(typeInfo.NullSentinelProperty), "__NullSentinel");
		}
		ReadOnlyMetadataCollection<EdmProperty> properties = TypeHelpers.GetProperties(typeInfo.Type);
		ColumnMap[] array = new ColumnMap[properties.Count];
		for (int i = 0; i < array.Length; i++)
		{
			EdmMember edmMember = properties[i];
			array[i] = CreateColumnMap(Helper.GetModelTypeUsage(edmMember), edmMember.Name);
		}
		return new RecordColumnMap(typeInfo.Type, name, array, nullSentinel);
	}

	private RefColumnMap CreateRefColumnMap(TypeInfo typeInfo, string name)
	{
		SimpleColumnMap entitySetIdColumnMap = null;
		if (typeInfo.HasEntitySetIdProperty)
		{
			entitySetIdColumnMap = CreateSimpleColumnMap(Helper.GetModelTypeUsage(typeInfo.EntitySetIdProperty), "__EntitySetId");
		}
		EntityType entityType = (EntityType)TypeHelpers.GetEdmType<RefType>(typeInfo.Type).ElementType;
		SimpleColumnMap[] array = new SimpleColumnMap[entityType.KeyMembers.Count];
		for (int i = 0; i < array.Length; i++)
		{
			EdmMember edmMember = entityType.KeyMembers[i];
			array[i] = CreateSimpleColumnMap(Helper.GetModelTypeUsage(edmMember), edmMember.Name);
		}
		EntityIdentity entityIdentity = CreateEntityIdentity(entityType, entitySetIdColumnMap, array);
		return new RefColumnMap(typeInfo.Type, name, entityIdentity);
	}

	private SimpleColumnMap CreateSimpleColumnMap(TypeUsage type, string name)
	{
		Var nextVar = GetNextVar();
		return new VarRefColumnMap(type, name, nextVar);
	}

	private SimpleColumnMap CreateTypeIdColumnMap(EdmProperty prop)
	{
		return CreateSimpleColumnMap(Helper.GetModelTypeUsage(prop), "__TypeId");
	}

	private ColumnMap CreateStructuralColumnMap(TypeUsage type, string name)
	{
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(type);
		if (TypeSemantics.IsRowType(type))
		{
			return CreateRecordColumnMap(typeInfo, name);
		}
		if (TypeSemantics.IsReferenceType(type))
		{
			return CreateRefColumnMap(typeInfo, name);
		}
		if (typeInfo.HasTypeIdProperty)
		{
			return CreatePolymorphicColumnMap(typeInfo, name);
		}
		if (TypeSemantics.IsComplexType(type))
		{
			return CreateComplexTypeColumnMap(typeInfo, name, null, null, null);
		}
		if (TypeSemantics.IsEntityType(type))
		{
			return CreateEntityColumnMap(typeInfo, name, null, null, null, handleRelProperties: true);
		}
		throw new NotSupportedException(type.Identity);
	}

	private EntityIdentity CreateEntityIdentity(EntityType entityType, SimpleColumnMap entitySetIdColumnMap, SimpleColumnMap[] keyColumnMaps)
	{
		if (entitySetIdColumnMap != null)
		{
			return new DiscriminatedEntityIdentity(entitySetIdColumnMap, m_typeInfo.EntitySetIdToEntitySetMap, keyColumnMaps);
		}
		EntitySet entitySet = m_typeInfo.GetEntitySet(entityType);
		PlanCompiler.Assert(entitySet != null, "Expected non-null entitySet when no entity set ID is required. Entity type = " + entityType);
		return new SimpleEntityIdentity(entitySet, keyColumnMaps);
	}
}
