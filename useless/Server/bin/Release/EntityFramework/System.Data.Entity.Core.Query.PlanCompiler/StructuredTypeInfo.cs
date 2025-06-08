using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Globalization;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class StructuredTypeInfo
{
	private TypeUsage m_stringType;

	private TypeUsage m_intType;

	private readonly Dictionary<TypeUsage, TypeInfo> m_typeInfoMap;

	private bool m_typeInfoMapPopulated;

	private EntitySet[] m_entitySetIdToEntitySetMap;

	private Dictionary<EntitySet, int> m_entitySetToEntitySetIdMap;

	private Dictionary<EntityTypeBase, EntitySet> m_entityTypeToEntitySetMap;

	private Dictionary<EntitySetBase, ExplicitDiscriminatorMap> m_discriminatorMaps;

	private RelPropertyHelper m_relPropertyHelper;

	private readonly HashSet<string> m_typesNeedingNullSentinel;

	internal EntitySet[] EntitySetIdToEntitySetMap => m_entitySetIdToEntitySetMap;

	internal RelPropertyHelper RelPropertyHelper => m_relPropertyHelper;

	private StructuredTypeInfo(HashSet<string> typesNeedingNullSentinel)
	{
		m_typeInfoMap = new Dictionary<TypeUsage, TypeInfo>(TypeUsageEqualityComparer.Instance);
		m_typeInfoMapPopulated = false;
		m_typesNeedingNullSentinel = typesNeedingNullSentinel;
	}

	internal static void Process(Command itree, HashSet<TypeUsage> referencedTypes, HashSet<EntitySet> referencedEntitySets, HashSet<EntityType> freeFloatingEntityConstructorTypes, Dictionary<EntitySetBase, DiscriminatorMapInfo> discriminatorMaps, RelPropertyHelper relPropertyHelper, HashSet<string> typesNeedingNullSentinel, out StructuredTypeInfo structuredTypeInfo)
	{
		structuredTypeInfo = new StructuredTypeInfo(typesNeedingNullSentinel);
		structuredTypeInfo.Process(itree, referencedTypes, referencedEntitySets, freeFloatingEntityConstructorTypes, discriminatorMaps, relPropertyHelper);
	}

	private void Process(Command itree, HashSet<TypeUsage> referencedTypes, HashSet<EntitySet> referencedEntitySets, HashSet<EntityType> freeFloatingEntityConstructorTypes, Dictionary<EntitySetBase, DiscriminatorMapInfo> discriminatorMaps, RelPropertyHelper relPropertyHelper)
	{
		PlanCompiler.Assert(itree != null, "null itree?");
		m_stringType = itree.StringType;
		m_intType = itree.IntegerType;
		m_relPropertyHelper = relPropertyHelper;
		ProcessEntitySets(referencedEntitySets, freeFloatingEntityConstructorTypes);
		ProcessDiscriminatorMaps(discriminatorMaps);
		ProcessTypes(referencedTypes);
	}

	internal EntitySet GetEntitySet(EntityTypeBase type)
	{
		EntityTypeBase rootType = GetRootType(type);
		if (!m_entityTypeToEntitySetMap.TryGetValue(rootType, out var value))
		{
			return null;
		}
		return value;
	}

	internal int GetEntitySetId(EntitySet e)
	{
		int value = 0;
		if (!m_entitySetToEntitySetIdMap.TryGetValue(e, out value))
		{
			PlanCompiler.Assert(condition: false, "no such entity set?");
		}
		return value;
	}

	internal Set<EntitySet> GetEntitySets()
	{
		return new Set<EntitySet>(m_entitySetIdToEntitySetMap).MakeReadOnly();
	}

	internal TypeInfo GetTypeInfo(TypeUsage type)
	{
		if (!TypeUtils.IsStructuredType(type))
		{
			return null;
		}
		TypeInfo value = null;
		if (!m_typeInfoMap.TryGetValue(type, out value))
		{
			PlanCompiler.Assert(!TypeUtils.IsStructuredType(type) || !m_typeInfoMapPopulated, "cannot find typeInfo for type " + type);
		}
		return value;
	}

	private void AddEntityTypeToSetEntry(EntityType entityType, EntitySet entitySet)
	{
		EntityTypeBase rootType = GetRootType(entityType);
		bool flag = true;
		EntitySet value;
		if (entitySet == null)
		{
			flag = false;
		}
		else if (m_entityTypeToEntitySetMap.TryGetValue(rootType, out value) && value != entitySet)
		{
			flag = false;
		}
		if (flag)
		{
			m_entityTypeToEntitySetMap[rootType] = entitySet;
		}
		else
		{
			m_entityTypeToEntitySetMap[rootType] = null;
		}
	}

	private void ProcessEntitySets(HashSet<EntitySet> referencedEntitySets, HashSet<EntityType> freeFloatingEntityConstructorTypes)
	{
		AssignEntitySetIds(referencedEntitySets);
		m_entityTypeToEntitySetMap = new Dictionary<EntityTypeBase, EntitySet>();
		foreach (EntitySet referencedEntitySet in referencedEntitySets)
		{
			AddEntityTypeToSetEntry(referencedEntitySet.ElementType, referencedEntitySet);
		}
		foreach (EntityType freeFloatingEntityConstructorType in freeFloatingEntityConstructorTypes)
		{
			AddEntityTypeToSetEntry(freeFloatingEntityConstructorType, null);
		}
	}

	private void ProcessDiscriminatorMaps(Dictionary<EntitySetBase, DiscriminatorMapInfo> discriminatorMaps)
	{
		Dictionary<EntitySetBase, ExplicitDiscriminatorMap> dictionary = null;
		if (discriminatorMaps != null)
		{
			dictionary = new Dictionary<EntitySetBase, ExplicitDiscriminatorMap>(discriminatorMaps.Count, discriminatorMaps.Comparer);
			foreach (KeyValuePair<EntitySetBase, DiscriminatorMapInfo> discriminatorMap2 in discriminatorMaps)
			{
				EntitySetBase key = discriminatorMap2.Key;
				ExplicitDiscriminatorMap discriminatorMap = discriminatorMap2.Value.DiscriminatorMap;
				if (discriminatorMap != null)
				{
					EntityTypeBase rootType = GetRootType(key.ElementType);
					if (GetEntitySet(rootType) != null)
					{
						dictionary.Add(key, discriminatorMap);
					}
				}
			}
			if (dictionary.Count == 0)
			{
				dictionary = null;
			}
		}
		m_discriminatorMaps = dictionary;
	}

	private void AssignEntitySetIds(HashSet<EntitySet> referencedEntitySets)
	{
		m_entitySetIdToEntitySetMap = new EntitySet[referencedEntitySets.Count];
		m_entitySetToEntitySetIdMap = new Dictionary<EntitySet, int>();
		int num = 0;
		foreach (EntitySet referencedEntitySet in referencedEntitySets)
		{
			if (!m_entitySetToEntitySetIdMap.ContainsKey(referencedEntitySet))
			{
				m_entitySetIdToEntitySetMap[num] = referencedEntitySet;
				m_entitySetToEntitySetIdMap[referencedEntitySet] = num;
				num++;
			}
		}
	}

	private void ProcessTypes(HashSet<TypeUsage> referencedTypes)
	{
		PopulateTypeInfoMap(referencedTypes);
		AssignTypeIds();
		ExplodeTypes();
	}

	private void PopulateTypeInfoMap(HashSet<TypeUsage> referencedTypes)
	{
		foreach (TypeUsage referencedType in referencedTypes)
		{
			CreateTypeInfoForType(referencedType);
		}
		m_typeInfoMapPopulated = true;
	}

	private bool TryGetDiscriminatorMap(EdmType type, out ExplicitDiscriminatorMap discriminatorMap)
	{
		discriminatorMap = null;
		if (m_discriminatorMaps == null)
		{
			return false;
		}
		if (type.BuiltInTypeKind != BuiltInTypeKind.EntityType)
		{
			return false;
		}
		EntityTypeBase rootType = GetRootType((EntityType)type);
		if (!m_entityTypeToEntitySetMap.TryGetValue(rootType, out var value))
		{
			return false;
		}
		if (value == null)
		{
			return false;
		}
		return m_discriminatorMaps.TryGetValue(value, out discriminatorMap);
	}

	private void CreateTypeInfoForType(TypeUsage type)
	{
		while (TypeUtils.IsCollectionType(type))
		{
			type = TypeHelpers.GetEdmType<CollectionType>(type).TypeUsage;
		}
		if (TypeUtils.IsStructuredType(type))
		{
			TryGetDiscriminatorMap(type.EdmType, out var discriminatorMap);
			CreateTypeInfoForStructuredType(type, discriminatorMap);
		}
	}

	private TypeInfo CreateTypeInfoForStructuredType(TypeUsage type, ExplicitDiscriminatorMap discriminatorMap)
	{
		PlanCompiler.Assert(TypeUtils.IsStructuredType(type), "expected structured type. Found " + type);
		TypeInfo typeInfo = GetTypeInfo(type);
		if (typeInfo != null)
		{
			return typeInfo;
		}
		TypeInfo superTypeInfo = null;
		RefType type2;
		if (type.EdmType.BaseType != null)
		{
			superTypeInfo = CreateTypeInfoForStructuredType(TypeUsage.Create(type.EdmType.BaseType), discriminatorMap);
		}
		else if (TypeHelpers.TryGetEdmType<RefType>(type, out type2) && type2.ElementType is EntityType { BaseType: not null } entityType)
		{
			TypeUsage type3 = TypeHelpers.CreateReferenceTypeUsage(entityType.BaseType as EntityType);
			superTypeInfo = CreateTypeInfoForStructuredType(type3, discriminatorMap);
		}
		foreach (EdmMember declaredStructuralMember in TypeHelpers.GetDeclaredStructuralMembers(type))
		{
			CreateTypeInfoForType(declaredStructuralMember.TypeUsage);
		}
		if (TypeHelpers.TryGetEdmType<EntityTypeBase>(type, out var type4))
		{
			foreach (RelProperty declaredOnlyRelProperty in m_relPropertyHelper.GetDeclaredOnlyRelProperties(type4))
			{
				CreateTypeInfoForType(declaredOnlyRelProperty.ToEnd.TypeUsage);
			}
		}
		typeInfo = TypeInfo.Create(type, superTypeInfo, discriminatorMap);
		m_typeInfoMap.Add(type, typeInfo);
		return typeInfo;
	}

	private void AssignTypeIds()
	{
		int num = 0;
		foreach (KeyValuePair<TypeUsage, TypeInfo> item in m_typeInfoMap)
		{
			if (item.Value.RootType.DiscriminatorMap != null)
			{
				EntityType entityType = (EntityType)item.Key.EdmType;
				item.Value.TypeId = item.Value.RootType.DiscriminatorMap.GetTypeId(entityType);
			}
			else if (item.Value.IsRootType && (TypeSemantics.IsEntityType(item.Key) || TypeSemantics.IsComplexType(item.Key)))
			{
				AssignRootTypeId(item.Value, string.Format(CultureInfo.InvariantCulture, "{0}X", new object[1] { num }));
				num++;
			}
		}
	}

	private void AssignRootTypeId(TypeInfo typeInfo, string typeId)
	{
		typeInfo.TypeId = typeId;
		AssignTypeIdsToSubTypes(typeInfo);
	}

	private void AssignTypeIdsToSubTypes(TypeInfo typeInfo)
	{
		int num = 0;
		foreach (TypeInfo immediateSubType in typeInfo.ImmediateSubTypes)
		{
			AssignTypeId(immediateSubType, num);
			num++;
		}
	}

	private void AssignTypeId(TypeInfo typeInfo, int subtypeNum)
	{
		typeInfo.TypeId = string.Format(CultureInfo.InvariantCulture, "{0}{1}X", new object[2]
		{
			typeInfo.SuperType.TypeId,
			subtypeNum
		});
		AssignTypeIdsToSubTypes(typeInfo);
	}

	private static bool NeedsTypeIdProperty(TypeInfo typeInfo)
	{
		if (typeInfo.ImmediateSubTypes.Count > 0)
		{
			return !TypeSemantics.IsReferenceType(typeInfo.Type);
		}
		return false;
	}

	private bool NeedsNullSentinelProperty(TypeInfo typeInfo)
	{
		return m_typesNeedingNullSentinel.Contains(typeInfo.Type.EdmType.Identity);
	}

	private bool NeedsEntitySetIdProperty(TypeInfo typeInfo)
	{
		EntityType entityType = ((!(typeInfo.Type.EdmType is RefType refType)) ? (typeInfo.Type.EdmType as EntityType) : (refType.ElementType as EntityType));
		if (entityType != null)
		{
			return GetEntitySet(entityType) == null;
		}
		return false;
	}

	private void ExplodeTypes()
	{
		foreach (KeyValuePair<TypeUsage, TypeInfo> item in m_typeInfoMap)
		{
			if (item.Value.IsRootType)
			{
				ExplodeType(item.Value);
			}
		}
	}

	private TypeInfo ExplodeType(TypeUsage type)
	{
		if (TypeUtils.IsStructuredType(type))
		{
			TypeInfo typeInfo = GetTypeInfo(type);
			ExplodeType(typeInfo);
			return typeInfo;
		}
		if (TypeUtils.IsCollectionType(type))
		{
			TypeUsage typeUsage = TypeHelpers.GetEdmType<CollectionType>(type).TypeUsage;
			ExplodeType(typeUsage);
			return null;
		}
		return null;
	}

	private void ExplodeType(TypeInfo typeInfo)
	{
		ExplodeRootStructuredType(typeInfo.RootType);
	}

	private void ExplodeRootStructuredType(RootTypeInfo rootType)
	{
		if (rootType.FlattenedType != null)
		{
			return;
		}
		if (NeedsTypeIdProperty(rootType))
		{
			rootType.AddPropertyRef(TypeIdPropertyRef.Instance);
			if (rootType.DiscriminatorMap != null)
			{
				rootType.TypeIdKind = TypeIdKind.UserSpecified;
				rootType.TypeIdType = Helper.GetModelTypeUsage(rootType.DiscriminatorMap.DiscriminatorProperty);
			}
			else
			{
				rootType.TypeIdKind = TypeIdKind.Generated;
				rootType.TypeIdType = m_stringType;
			}
		}
		if (NeedsEntitySetIdProperty(rootType))
		{
			rootType.AddPropertyRef(EntitySetIdPropertyRef.Instance);
		}
		if (NeedsNullSentinelProperty(rootType))
		{
			rootType.AddPropertyRef(NullSentinelPropertyRef.Instance);
		}
		ExplodeRootStructuredTypeHelper(rootType);
		if (TypeSemantics.IsEntityType(rootType.Type))
		{
			AddRelProperties(rootType);
		}
		CreateFlattenedRecordType(rootType);
	}

	private void ExplodeRootStructuredTypeHelper(TypeInfo typeInfo)
	{
		RootTypeInfo rootType = typeInfo.RootType;
		IEnumerable enumerable = null;
		if (TypeHelpers.TryGetEdmType<RefType>(typeInfo.Type, out var type))
		{
			if (!typeInfo.IsRootType)
			{
				return;
			}
			enumerable = type.ElementType.KeyMembers;
		}
		else
		{
			enumerable = TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type);
		}
		foreach (EdmMember item in enumerable)
		{
			TypeInfo typeInfo2 = ExplodeType(item.TypeUsage);
			if (typeInfo2 == null)
			{
				rootType.AddPropertyRef(new SimplePropertyRef(item));
				continue;
			}
			foreach (PropertyRef propertyRef in typeInfo2.PropertyRefList)
			{
				rootType.AddPropertyRef(propertyRef.CreateNestedPropertyRef(item));
			}
		}
		foreach (TypeInfo immediateSubType in typeInfo.ImmediateSubTypes)
		{
			ExplodeRootStructuredTypeHelper(immediateSubType);
		}
	}

	private void AddRelProperties(TypeInfo typeInfo)
	{
		EntityTypeBase entityType = (EntityTypeBase)typeInfo.Type.EdmType;
		foreach (RelProperty declaredOnlyRelProperty in m_relPropertyHelper.GetDeclaredOnlyRelProperties(entityType))
		{
			TypeInfo typeInfo2 = GetTypeInfo(declaredOnlyRelProperty.ToEnd.TypeUsage);
			ExplodeType(typeInfo2);
			foreach (PropertyRef propertyRef in typeInfo2.PropertyRefList)
			{
				typeInfo.RootType.AddPropertyRef(propertyRef.CreateNestedPropertyRef(declaredOnlyRelProperty));
			}
		}
		foreach (TypeInfo immediateSubType in typeInfo.ImmediateSubTypes)
		{
			AddRelProperties(immediateSubType);
		}
	}

	private void CreateFlattenedRecordType(RootTypeInfo type)
	{
		bool flag = ((TypeSemantics.IsEntityType(type.Type) && type.ImmediateSubTypes.Count == 0) ? true : false);
		List<KeyValuePair<string, TypeUsage>> list = new List<KeyValuePair<string, TypeUsage>>();
		HashSet<string> hashSet = new HashSet<string>();
		int num = 0;
		foreach (PropertyRef propertyRef in type.PropertyRefList)
		{
			string text = null;
			if (flag && propertyRef is SimplePropertyRef simplePropertyRef)
			{
				text = simplePropertyRef.Property.Name;
			}
			if (text == null)
			{
				text = "F" + num.ToString(CultureInfo.InvariantCulture);
				num++;
			}
			while (hashSet.Contains(text))
			{
				text = "F" + num.ToString(CultureInfo.InvariantCulture);
				num++;
			}
			TypeUsage propertyType = GetPropertyType(type, propertyRef);
			list.Add(new KeyValuePair<string, TypeUsage>(text, propertyType));
			hashSet.Add(text);
		}
		type.FlattenedType = TypeHelpers.CreateRowType(list);
		IEnumerator<PropertyRef> enumerator2 = type.PropertyRefList.GetEnumerator();
		foreach (EdmProperty property in type.FlattenedType.Properties)
		{
			if (!enumerator2.MoveNext())
			{
				PlanCompiler.Assert(condition: false, "property refs count and flattened type member count mismatch?");
			}
			type.AddPropertyMapping(enumerator2.Current, property);
		}
	}

	private TypeUsage GetNewType(TypeUsage type)
	{
		if (TypeUtils.IsStructuredType(type))
		{
			return GetTypeInfo(type).FlattenedTypeUsage;
		}
		if (TypeHelpers.TryGetCollectionElementType(type, out var elementType))
		{
			TypeUsage newType = GetNewType(elementType);
			if (newType.EdmEquals(elementType))
			{
				return type;
			}
			return TypeHelpers.CreateCollectionTypeUsage(newType);
		}
		if (TypeUtils.IsEnumerationType(type))
		{
			return TypeHelpers.CreateEnumUnderlyingTypeUsage(type);
		}
		if (TypeSemantics.IsStrongSpatialType(type))
		{
			return TypeHelpers.CreateSpatialUnionTypeUsage(type);
		}
		return type;
	}

	private TypeUsage GetPropertyType(RootTypeInfo typeInfo, PropertyRef p)
	{
		TypeUsage type = null;
		PropertyRef propertyRef = null;
		while (p is NestedPropertyRef)
		{
			NestedPropertyRef obj = (NestedPropertyRef)p;
			p = obj.OuterProperty;
			propertyRef = obj.InnerProperty;
		}
		if (p is TypeIdPropertyRef)
		{
			SimplePropertyRef simplePropertyRef = (SimplePropertyRef)propertyRef;
			if (simplePropertyRef != null)
			{
				TypeUsage typeUsage = simplePropertyRef.Property.TypeUsage;
				type = GetTypeInfo(typeUsage).RootType.TypeIdType;
			}
			else
			{
				type = typeInfo.TypeIdType;
			}
		}
		else if (p is EntitySetIdPropertyRef || p is NullSentinelPropertyRef)
		{
			type = m_intType;
		}
		else if (p is RelPropertyRef)
		{
			type = ((RelPropertyRef)p).Property.ToEnd.TypeUsage;
		}
		else if (p is SimplePropertyRef simplePropertyRef2)
		{
			type = Helper.GetModelTypeUsage(simplePropertyRef2.Property);
		}
		type = GetNewType(type);
		PlanCompiler.Assert(type != null, "unrecognized property type?");
		return type;
	}

	private static EntityTypeBase GetRootType(EntityTypeBase type)
	{
		while (type.BaseType != null)
		{
			type = (EntityTypeBase)type.BaseType;
		}
		return type;
	}
}
