using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal class DefaultObjectMappingItemCollection : MappingItemCollection
{
	private readonly ObjectItemCollection _objectCollection;

	private readonly EdmItemCollection _edmCollection;

	private Dictionary<string, int> _clrTypeIndexes = new Dictionary<string, int>(StringComparer.Ordinal);

	private Dictionary<string, int> _edmTypeIndexes = new Dictionary<string, int>(StringComparer.Ordinal);

	private readonly object _lock = new object();

	public ObjectItemCollection ObjectItemCollection => _objectCollection;

	public EdmItemCollection EdmItemCollection => _edmCollection;

	public DefaultObjectMappingItemCollection(EdmItemCollection edmCollection, ObjectItemCollection objectCollection)
		: base(DataSpace.OCSpace)
	{
		_edmCollection = edmCollection;
		_objectCollection = objectCollection;
		foreach (PrimitiveType primitiveType in _edmCollection.GetPrimitiveTypes())
		{
			PrimitiveType mappedPrimitiveType = _objectCollection.GetMappedPrimitiveType(primitiveType.PrimitiveTypeKind);
			AddInternalMapping(new ObjectTypeMapping(mappedPrimitiveType, primitiveType), _clrTypeIndexes, _edmTypeIndexes);
		}
	}

	internal override MappingBase GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
	{
		if (!TryGetMap(identity, typeSpace, ignoreCase, out var map))
		{
			throw new InvalidOperationException(Strings.Mapping_Object_InvalidType(identity));
		}
		return map;
	}

	internal override bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out MappingBase map)
	{
		EdmType item = null;
		EdmType item2 = null;
		switch (typeSpace)
		{
		case DataSpace.CSpace:
		{
			if (ignoreCase)
			{
				if (!_edmCollection.TryGetItem<EdmType>(identity, ignoreCase: true, out item))
				{
					map = null;
					return false;
				}
				identity = item.Identity;
			}
			if (_edmTypeIndexes.TryGetValue(identity, out var value2))
			{
				map = (MappingBase)base[value2];
				return true;
			}
			if (item != null || _edmCollection.TryGetItem<EdmType>(identity, ignoreCase, out item))
			{
				_objectCollection.TryGetOSpaceType(item, out item2);
			}
			break;
		}
		case DataSpace.OSpace:
		{
			if (ignoreCase)
			{
				if (!_objectCollection.TryGetItem<EdmType>(identity, ignoreCase: true, out item2))
				{
					map = null;
					return false;
				}
				identity = item2.Identity;
			}
			if (_clrTypeIndexes.TryGetValue(identity, out var value))
			{
				map = (MappingBase)base[value];
				return true;
			}
			if (item2 != null || _objectCollection.TryGetItem<EdmType>(identity, ignoreCase, out item2))
			{
				string identity2 = ObjectItemCollection.TryGetMappingCSpaceTypeIdentity(item2);
				_edmCollection.TryGetItem<EdmType>(identity2, out item);
			}
			break;
		}
		}
		if (item2 == null || item == null)
		{
			map = null;
			return false;
		}
		map = GetDefaultMapping(item, item2);
		return true;
	}

	internal override MappingBase GetMap(string identity, DataSpace typeSpace)
	{
		return GetMap(identity, typeSpace, ignoreCase: false);
	}

	internal override bool TryGetMap(string identity, DataSpace typeSpace, out MappingBase map)
	{
		return TryGetMap(identity, typeSpace, ignoreCase: false, out map);
	}

	internal override MappingBase GetMap(GlobalItem item)
	{
		if (!TryGetMap(item, out var map))
		{
			throw new InvalidOperationException(Strings.Mapping_Object_InvalidType(item.Identity));
		}
		return map;
	}

	internal override bool TryGetMap(GlobalItem item, out MappingBase map)
	{
		if (item == null)
		{
			map = null;
			return false;
		}
		DataSpace dataSpace = item.DataSpace;
		if (item is EdmType edmType && Helper.IsTransientType(edmType))
		{
			map = GetOCMapForTransientType(edmType, dataSpace);
			if (map != null)
			{
				return true;
			}
			return false;
		}
		return TryGetMap(item.Identity, dataSpace, out map);
	}

	private MappingBase GetDefaultMapping(EdmType cdmType, EdmType clrType)
	{
		return LoadObjectMapping(cdmType, clrType, this);
	}

	private MappingBase GetOCMapForTransientType(EdmType edmType, DataSpace typeSpace)
	{
		EdmType edmType2 = null;
		EdmType edmType3 = null;
		int value = -1;
		if (typeSpace != 0)
		{
			if (_edmTypeIndexes.TryGetValue(edmType.Identity, out value))
			{
				return (MappingBase)base[value];
			}
			edmType3 = edmType;
			edmType2 = ConvertCSpaceToOSpaceType(edmType);
		}
		else if (typeSpace == DataSpace.OSpace)
		{
			if (_clrTypeIndexes.TryGetValue(edmType.Identity, out value))
			{
				return (MappingBase)base[value];
			}
			edmType2 = edmType;
			edmType3 = ConvertOSpaceToCSpaceType(edmType2);
		}
		ObjectTypeMapping objectTypeMapping = new ObjectTypeMapping(edmType2, edmType3);
		if (BuiltInTypeKind.RowType == edmType.BuiltInTypeKind)
		{
			RowType rowType = (RowType)edmType2;
			RowType rowType2 = (RowType)edmType3;
			for (int i = 0; i < rowType.Properties.Count; i++)
			{
				objectTypeMapping.AddMemberMap(new ObjectPropertyMapping(rowType2.Properties[i], rowType.Properties[i]));
			}
		}
		if (!_edmTypeIndexes.ContainsKey(edmType3.Identity) && !_clrTypeIndexes.ContainsKey(edmType2.Identity))
		{
			lock (_lock)
			{
				Dictionary<string, int> clrTypeIndexes = new Dictionary<string, int>(_clrTypeIndexes);
				Dictionary<string, int> edmTypeIndexes = new Dictionary<string, int>(_edmTypeIndexes);
				objectTypeMapping = AddInternalMapping(objectTypeMapping, clrTypeIndexes, edmTypeIndexes);
				_clrTypeIndexes = clrTypeIndexes;
				_edmTypeIndexes = edmTypeIndexes;
			}
		}
		return objectTypeMapping;
	}

	private EdmType ConvertCSpaceToOSpaceType(EdmType cdmType)
	{
		EdmType edmType = null;
		if (Helper.IsCollectionType(cdmType))
		{
			return new CollectionType(ConvertCSpaceToOSpaceType(((CollectionType)cdmType).TypeUsage.EdmType));
		}
		if (Helper.IsRowType(cdmType))
		{
			List<EdmProperty> list = new List<EdmProperty>();
			RowType rowType = (RowType)cdmType;
			foreach (EdmProperty property in rowType.Properties)
			{
				EdmType edmType2 = ConvertCSpaceToOSpaceType(property.TypeUsage.EdmType);
				EdmProperty item = new EdmProperty(property.Name, TypeUsage.Create(edmType2));
				list.Add(item);
			}
			return new RowType(list, rowType.InitializerMetadata);
		}
		if (Helper.IsRefType(cdmType))
		{
			return new RefType((EntityType)ConvertCSpaceToOSpaceType(((RefType)cdmType).ElementType));
		}
		if (Helper.IsPrimitiveType(cdmType))
		{
			return _objectCollection.GetMappedPrimitiveType(((PrimitiveType)cdmType).PrimitiveTypeKind);
		}
		return ((ObjectTypeMapping)GetMap(cdmType)).ClrType;
	}

	private EdmType ConvertOSpaceToCSpaceType(EdmType clrType)
	{
		EdmType edmType = null;
		if (Helper.IsCollectionType(clrType))
		{
			return new CollectionType(ConvertOSpaceToCSpaceType(((CollectionType)clrType).TypeUsage.EdmType));
		}
		if (Helper.IsRowType(clrType))
		{
			List<EdmProperty> list = new List<EdmProperty>();
			RowType rowType = (RowType)clrType;
			foreach (EdmProperty property in rowType.Properties)
			{
				EdmType edmType2 = ConvertOSpaceToCSpaceType(property.TypeUsage.EdmType);
				EdmProperty item = new EdmProperty(property.Name, TypeUsage.Create(edmType2));
				list.Add(item);
			}
			return new RowType(list, rowType.InitializerMetadata);
		}
		if (Helper.IsRefType(clrType))
		{
			return new RefType((EntityType)ConvertOSpaceToCSpaceType(((RefType)clrType).ElementType));
		}
		return ((ObjectTypeMapping)GetMap(clrType)).EdmType;
	}

	private void AddInternalMappings(IEnumerable<ObjectTypeMapping> typeMappings)
	{
		lock (_lock)
		{
			Dictionary<string, int> clrTypeIndexes = new Dictionary<string, int>(_clrTypeIndexes);
			Dictionary<string, int> edmTypeIndexes = new Dictionary<string, int>(_edmTypeIndexes);
			foreach (ObjectTypeMapping typeMapping in typeMappings)
			{
				AddInternalMapping(typeMapping, clrTypeIndexes, edmTypeIndexes);
			}
			_clrTypeIndexes = clrTypeIndexes;
			_edmTypeIndexes = edmTypeIndexes;
		}
	}

	private ObjectTypeMapping AddInternalMapping(ObjectTypeMapping objectMap, Dictionary<string, int> clrTypeIndexes, Dictionary<string, int> edmTypeIndexes)
	{
		if (base.Source.ContainsIdentity(objectMap.Identity))
		{
			return (ObjectTypeMapping)base.Source[objectMap.Identity];
		}
		objectMap.DataSpace = DataSpace.OCSpace;
		int count = base.Count;
		AddInternal(objectMap);
		string identity = objectMap.ClrType.Identity;
		if (!clrTypeIndexes.ContainsKey(identity))
		{
			clrTypeIndexes.Add(identity, count);
		}
		string identity2 = objectMap.EdmType.Identity;
		if (!edmTypeIndexes.ContainsKey(identity2))
		{
			edmTypeIndexes.Add(identity2, count);
		}
		return objectMap;
	}

	internal static ObjectTypeMapping LoadObjectMapping(EdmType cdmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection)
	{
		Dictionary<string, ObjectTypeMapping> dictionary = new Dictionary<string, ObjectTypeMapping>(StringComparer.Ordinal);
		ObjectTypeMapping result = LoadObjectMapping(cdmType, objectType, ocItemCollection, dictionary);
		ocItemCollection?.AddInternalMappings(dictionary.Values);
		return result;
	}

	private static ObjectTypeMapping LoadObjectMapping(EdmType edmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
	{
		if (Helper.IsEnumType(edmType) ^ Helper.IsEnumType(objectType))
		{
			throw new MappingException(Strings.Mapping_EnumTypeMappingToNonEnumType(edmType.FullName, objectType.FullName));
		}
		if (edmType.Abstract != objectType.Abstract)
		{
			throw new MappingException(Strings.Mapping_AbstractTypeMappingToNonAbstractType(edmType.FullName, objectType.FullName));
		}
		ObjectTypeMapping objectTypeMapping = new ObjectTypeMapping(objectType, edmType);
		typeMappings.Add(edmType.FullName, objectTypeMapping);
		if (Helper.IsEntityType(edmType) || Helper.IsComplexType(edmType))
		{
			LoadEntityTypeOrComplexTypeMapping(objectTypeMapping, edmType, objectType, ocItemCollection, typeMappings);
		}
		else if (Helper.IsEnumType(edmType))
		{
			ValidateEnumTypeMapping((EnumType)edmType, (EnumType)objectType);
		}
		else
		{
			LoadAssociationTypeMapping(objectTypeMapping, edmType, objectType, ocItemCollection, typeMappings);
		}
		return objectTypeMapping;
	}

	private static EdmMember GetObjectMember(EdmMember edmMember, StructuralType objectType)
	{
		if (!objectType.Members.TryGetValue(edmMember.Name, ignoreCase: false, out var item))
		{
			throw new MappingException(Strings.Mapping_Default_OCMapping_Clr_Member(edmMember.Name, edmMember.DeclaringType.FullName, objectType.FullName));
		}
		return item;
	}

	private static void ValidateMembersMatch(EdmMember edmMember, EdmMember objectMember)
	{
		if (edmMember.BuiltInTypeKind != objectMember.BuiltInTypeKind)
		{
			throw new MappingException(Strings.Mapping_Default_OCMapping_MemberKind_Mismatch(edmMember.Name, edmMember.DeclaringType.FullName, edmMember.BuiltInTypeKind, objectMember.Name, objectMember.DeclaringType.FullName, objectMember.BuiltInTypeKind));
		}
		if (edmMember.TypeUsage.EdmType.BuiltInTypeKind != objectMember.TypeUsage.EdmType.BuiltInTypeKind)
		{
			throw Error.Mapping_Default_OCMapping_Member_Type_Mismatch(edmMember.TypeUsage.EdmType.Name, edmMember.TypeUsage.EdmType.BuiltInTypeKind, edmMember.Name, edmMember.DeclaringType.FullName, objectMember.TypeUsage.EdmType.Name, objectMember.TypeUsage.EdmType.BuiltInTypeKind, objectMember.Name, objectMember.DeclaringType.FullName);
		}
		if (Helper.IsPrimitiveType(edmMember.TypeUsage.EdmType))
		{
			if (Helper.GetSpatialNormalizedPrimitiveType(edmMember.TypeUsage.EdmType).PrimitiveTypeKind != ((PrimitiveType)objectMember.TypeUsage.EdmType).PrimitiveTypeKind)
			{
				throw new MappingException(Strings.Mapping_Default_OCMapping_Invalid_MemberType(edmMember.TypeUsage.EdmType.FullName, edmMember.Name, edmMember.DeclaringType.FullName, objectMember.TypeUsage.EdmType.FullName, objectMember.Name, objectMember.DeclaringType.FullName));
			}
			return;
		}
		if (Helper.IsEnumType(edmMember.TypeUsage.EdmType))
		{
			ValidateEnumTypeMapping((EnumType)edmMember.TypeUsage.EdmType, (EnumType)objectMember.TypeUsage.EdmType);
			return;
		}
		EdmType edmType;
		EdmType edmType2;
		if (edmMember.BuiltInTypeKind == BuiltInTypeKind.AssociationEndMember)
		{
			edmType = ((RefType)edmMember.TypeUsage.EdmType).ElementType;
			edmType2 = ((RefType)objectMember.TypeUsage.EdmType).ElementType;
		}
		else if (BuiltInTypeKind.NavigationProperty == edmMember.BuiltInTypeKind && Helper.IsCollectionType(edmMember.TypeUsage.EdmType))
		{
			edmType = ((CollectionType)edmMember.TypeUsage.EdmType).TypeUsage.EdmType;
			edmType2 = ((CollectionType)objectMember.TypeUsage.EdmType).TypeUsage.EdmType;
		}
		else
		{
			edmType = edmMember.TypeUsage.EdmType;
			edmType2 = objectMember.TypeUsage.EdmType;
		}
		if (edmType.Identity != ObjectItemCollection.TryGetMappingCSpaceTypeIdentity(edmType2))
		{
			throw new MappingException(Strings.Mapping_Default_OCMapping_Invalid_MemberType(edmMember.TypeUsage.EdmType.FullName, edmMember.Name, edmMember.DeclaringType.FullName, objectMember.TypeUsage.EdmType.FullName, objectMember.Name, objectMember.DeclaringType.FullName));
		}
	}

	private static ObjectPropertyMapping LoadScalarPropertyMapping(EdmProperty edmProperty, EdmProperty objectProperty)
	{
		return new ObjectPropertyMapping(edmProperty, objectProperty);
	}

	private static void LoadEntityTypeOrComplexTypeMapping(ObjectTypeMapping objectMapping, EdmType edmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
	{
		StructuralType obj = (StructuralType)edmType;
		StructuralType structuralType = (StructuralType)objectType;
		ValidateAllMembersAreMapped(obj, structuralType);
		foreach (EdmMember member in obj.Members)
		{
			EdmMember objectMember = GetObjectMember(member, structuralType);
			ValidateMembersMatch(member, objectMember);
			if (Helper.IsEdmProperty(member))
			{
				EdmProperty edmProperty = (EdmProperty)member;
				EdmProperty edmProperty2 = (EdmProperty)objectMember;
				if (Helper.IsComplexType(member.TypeUsage.EdmType))
				{
					objectMapping.AddMemberMap(LoadComplexMemberMapping(edmProperty, edmProperty2, ocItemCollection, typeMappings));
				}
				else
				{
					objectMapping.AddMemberMap(LoadScalarPropertyMapping(edmProperty, edmProperty2));
				}
			}
			else
			{
				NavigationProperty navigationProperty = (NavigationProperty)member;
				NavigationProperty navigationProperty2 = (NavigationProperty)objectMember;
				LoadTypeMapping(navigationProperty.RelationshipType, navigationProperty2.RelationshipType, ocItemCollection, typeMappings);
				objectMapping.AddMemberMap(new ObjectNavigationPropertyMapping(navigationProperty, navigationProperty2));
			}
		}
	}

	private static void ValidateAllMembersAreMapped(StructuralType cdmStructuralType, StructuralType objectStructuralType)
	{
		if (cdmStructuralType.Members.Count != objectStructuralType.Members.Count)
		{
			throw new MappingException(Strings.Mapping_Default_OCMapping_Member_Count_Mismatch(cdmStructuralType.FullName, objectStructuralType.FullName));
		}
		foreach (EdmMember member in objectStructuralType.Members)
		{
			if (!cdmStructuralType.Members.Contains(member.Identity))
			{
				throw new MappingException(Strings.Mapping_Default_OCMapping_Clr_Member2(member.Name, objectStructuralType.FullName, cdmStructuralType.FullName));
			}
		}
	}

	private static void ValidateEnumTypeMapping(EnumType edmEnumType, EnumType objectEnumType)
	{
		if (edmEnumType.UnderlyingType.PrimitiveTypeKind != objectEnumType.UnderlyingType.PrimitiveTypeKind)
		{
			throw new MappingException(Strings.Mapping_Enum_OCMapping_UnderlyingTypesMismatch(edmEnumType.UnderlyingType.Name, edmEnumType.FullName, objectEnumType.UnderlyingType.Name, objectEnumType.FullName));
		}
		IEnumerator<EnumMember> enumerator = (from m in edmEnumType.Members
			orderby Convert.ToInt64(m.Value, CultureInfo.InvariantCulture), m.Name
			select m).GetEnumerator();
		IEnumerator<EnumMember> enumerator2 = (from m in objectEnumType.Members
			orderby Convert.ToInt64(m.Value, CultureInfo.InvariantCulture), m.Name
			select m).GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		while (enumerator2.MoveNext())
		{
			if (enumerator.Current.Name == enumerator2.Current.Name && enumerator.Current.Value.Equals(enumerator2.Current.Value) && !enumerator.MoveNext())
			{
				return;
			}
		}
		throw new MappingException(Strings.Mapping_Enum_OCMapping_MemberMismatch(objectEnumType.FullName, enumerator.Current.Name, enumerator.Current.Value, edmEnumType.FullName));
	}

	private static void LoadAssociationTypeMapping(ObjectTypeMapping objectMapping, EdmType edmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
	{
		AssociationType associationType = (AssociationType)edmType;
		AssociationType associationType2 = (AssociationType)objectType;
		foreach (AssociationEndMember associationEndMember2 in associationType.AssociationEndMembers)
		{
			AssociationEndMember associationEndMember = (AssociationEndMember)GetObjectMember(associationEndMember2, associationType2);
			ValidateMembersMatch(associationEndMember2, associationEndMember);
			if (associationEndMember2.RelationshipMultiplicity != associationEndMember.RelationshipMultiplicity)
			{
				throw new MappingException(Strings.Mapping_Default_OCMapping_MultiplicityMismatch(associationEndMember2.RelationshipMultiplicity, associationEndMember2.Name, associationType.FullName, associationEndMember.RelationshipMultiplicity, associationEndMember.Name, associationType2.FullName));
			}
			LoadTypeMapping(((RefType)associationEndMember2.TypeUsage.EdmType).ElementType, ((RefType)associationEndMember.TypeUsage.EdmType).ElementType, ocItemCollection, typeMappings);
			objectMapping.AddMemberMap(new ObjectAssociationEndMapping(associationEndMember2, associationEndMember));
		}
	}

	private static ObjectComplexPropertyMapping LoadComplexMemberMapping(EdmProperty containingEdmMember, EdmProperty containingClrMember, DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
	{
		ComplexType edmType = (ComplexType)containingEdmMember.TypeUsage.EdmType;
		ComplexType objectType = (ComplexType)containingClrMember.TypeUsage.EdmType;
		LoadTypeMapping(edmType, objectType, ocItemCollection, typeMappings);
		return new ObjectComplexPropertyMapping(containingEdmMember, containingClrMember);
	}

	private static ObjectTypeMapping LoadTypeMapping(EdmType edmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
	{
		if (typeMappings.TryGetValue(edmType.FullName, out var value))
		{
			return value;
		}
		if (ocItemCollection != null && ocItemCollection.ContainsMap(edmType, out var map))
		{
			return map;
		}
		return LoadObjectMapping(edmType, objectType, ocItemCollection, typeMappings);
	}

	private bool ContainsMap(GlobalItem cspaceItem, out ObjectTypeMapping map)
	{
		if (_edmTypeIndexes.TryGetValue(cspaceItem.Identity, out var value))
		{
			map = (ObjectTypeMapping)base[value];
			return true;
		}
		map = null;
		return false;
	}
}
