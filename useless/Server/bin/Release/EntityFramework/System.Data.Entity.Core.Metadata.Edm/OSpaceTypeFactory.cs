using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class OSpaceTypeFactory
{
	public abstract List<Action> ReferenceResolutions { get; }

	public abstract Dictionary<EdmType, EdmType> CspaceToOspace { get; }

	public abstract Dictionary<string, EdmType> LoadedTypes { get; }

	public abstract void LogLoadMessage(string message, EdmType relatedType);

	public abstract void LogError(string errorMessage, EdmType relatedType);

	public abstract void TrackClosure(Type type);

	public abstract void AddToTypesInAssembly(EdmType type);

	public virtual EdmType TryCreateType(Type type, EdmType cspaceType)
	{
		if (Helper.IsEnumType(cspaceType) ^ type.IsEnum())
		{
			LogLoadMessage(Strings.Validator_OSpace_Convention_SSpaceOSpaceTypeMismatch(cspaceType.FullName, cspaceType.FullName), cspaceType);
			return null;
		}
		EdmType newOSpaceType;
		if (Helper.IsEnumType(cspaceType))
		{
			TryCreateEnumType(type, (EnumType)cspaceType, out newOSpaceType);
			return newOSpaceType;
		}
		TryCreateStructuralType(type, (StructuralType)cspaceType, out newOSpaceType);
		return newOSpaceType;
	}

	private bool TryCreateEnumType(Type enumType, EnumType cspaceEnumType, out EdmType newOSpaceType)
	{
		newOSpaceType = null;
		if (!UnderlyingEnumTypesMatch(enumType, cspaceEnumType) || !EnumMembersMatch(enumType, cspaceEnumType))
		{
			return false;
		}
		newOSpaceType = new ClrEnumType(enumType, cspaceEnumType.NamespaceName, cspaceEnumType.Name);
		LoadedTypes.Add(enumType.FullName, newOSpaceType);
		return true;
	}

	private bool TryCreateStructuralType(Type type, StructuralType cspaceType, out EdmType newOSpaceType)
	{
		List<Action> list = new List<Action>();
		newOSpaceType = null;
		StructuralType ospaceType;
		if (Helper.IsEntityType(cspaceType))
		{
			ospaceType = new ClrEntityType(type, cspaceType.NamespaceName, cspaceType.Name);
		}
		else
		{
			ospaceType = new ClrComplexType(type, cspaceType.NamespaceName, cspaceType.Name);
		}
		if (cspaceType.BaseType != null)
		{
			if (!TypesMatchByConvention(type.BaseType(), cspaceType.BaseType))
			{
				string message = Strings.Validator_OSpace_Convention_BaseTypeIncompatible(type.BaseType().FullName, type.FullName, cspaceType.BaseType.FullName);
				LogLoadMessage(message, cspaceType);
				return false;
			}
			TrackClosure(type.BaseType());
			list.Add(delegate
			{
				ospaceType.BaseType = ResolveBaseType((StructuralType)cspaceType.BaseType, type);
			});
		}
		if (!TryCreateMembers(type, cspaceType, ospaceType, list))
		{
			return false;
		}
		LoadedTypes.Add(type.FullName, ospaceType);
		foreach (Action item in list)
		{
			ReferenceResolutions.Add(item);
		}
		newOSpaceType = ospaceType;
		return true;
	}

	internal static bool TypesMatchByConvention(Type type, EdmType cspaceType)
	{
		return type.Name == cspaceType.Name;
	}

	private bool UnderlyingEnumTypesMatch(Type enumType, EnumType cspaceEnumType)
	{
		if (!ClrProviderManifest.Instance.TryGetPrimitiveType(enumType.GetEnumUnderlyingType(), out var primitiveType))
		{
			LogLoadMessage(Strings.Validator_UnsupportedEnumUnderlyingType(enumType.GetEnumUnderlyingType().FullName), cspaceEnumType);
			return false;
		}
		if (primitiveType.PrimitiveTypeKind != cspaceEnumType.UnderlyingType.PrimitiveTypeKind)
		{
			LogLoadMessage(Strings.Validator_OSpace_Convention_NonMatchingUnderlyingTypes, cspaceEnumType);
			return false;
		}
		return true;
	}

	private bool EnumMembersMatch(Type enumType, EnumType cspaceEnumType)
	{
		Type enumUnderlyingType = enumType.GetEnumUnderlyingType();
		IEnumerator<EnumMember> enumerator = cspaceEnumType.Members.OrderBy((EnumMember m) => m.Name).GetEnumerator();
		IEnumerator<string> enumerator2 = (from n in enumType.GetEnumNames()
			orderby n
			select n).GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return true;
		}
		while (enumerator2.MoveNext())
		{
			if (enumerator.Current.Name == enumerator2.Current && enumerator.Current.Value.Equals(Convert.ChangeType(Enum.Parse(enumType, enumerator2.Current), enumUnderlyingType, CultureInfo.InvariantCulture)) && !enumerator.MoveNext())
			{
				return true;
			}
		}
		LogLoadMessage(Strings.Mapping_Enum_OCMapping_MemberMismatch(enumType.FullName, enumerator.Current.Name, enumerator.Current.Value, cspaceEnumType.FullName), cspaceEnumType);
		return false;
	}

	private bool TryCreateMembers(Type type, StructuralType cspaceType, StructuralType ospaceType, List<Action> referenceResolutionListForCurrentType)
	{
		IEnumerable<PropertyInfo> clrProperties = ((cspaceType.BaseType == null) ? type.GetRuntimeProperties() : type.GetDeclaredProperties()).Where((PropertyInfo p) => !p.IsStatic());
		if (!TryFindAndCreatePrimitiveProperties(type, cspaceType, ospaceType, clrProperties))
		{
			return false;
		}
		if (!TryFindAndCreateEnumProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
		{
			return false;
		}
		if (!TryFindComplexProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
		{
			return false;
		}
		if (!TryFindNavigationProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
		{
			return false;
		}
		return true;
	}

	private bool TryFindComplexProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties, List<Action> referenceResolutionListForCurrentType)
	{
		List<KeyValuePair<EdmProperty, PropertyInfo>> list = new List<KeyValuePair<EdmProperty, PropertyInfo>>();
		foreach (EdmProperty cspaceProperty in from m in cspaceType.GetDeclaredOnlyMembers<EdmProperty>()
			where Helper.IsComplexType(m.TypeUsage.EdmType)
			select m)
		{
			PropertyInfo propertyInfo = clrProperties.FirstOrDefault((PropertyInfo p) => MemberMatchesByConvention(p, cspaceProperty));
			if (propertyInfo != null)
			{
				list.Add(new KeyValuePair<EdmProperty, PropertyInfo>(cspaceProperty, propertyInfo));
				continue;
			}
			string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
			LogLoadMessage(message, cspaceType);
			return false;
		}
		foreach (KeyValuePair<EdmProperty, PropertyInfo> item in list)
		{
			TrackClosure(item.Value.PropertyType);
			StructuralType ot = ospaceType;
			EdmProperty cp = item.Key;
			PropertyInfo clrp = item.Value;
			referenceResolutionListForCurrentType.Add(delegate
			{
				CreateAndAddComplexType(type, ot, cp, clrp);
			});
		}
		return true;
	}

	private bool TryFindNavigationProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties, List<Action> referenceResolutionListForCurrentType)
	{
		List<KeyValuePair<NavigationProperty, PropertyInfo>> list = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
		foreach (NavigationProperty cspaceProperty in cspaceType.GetDeclaredOnlyMembers<NavigationProperty>())
		{
			PropertyInfo propertyInfo = clrProperties.FirstOrDefault((PropertyInfo p) => NonPrimitiveMemberMatchesByConvention(p, cspaceProperty));
			if (propertyInfo != null)
			{
				bool flag = cspaceProperty.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many;
				if (propertyInfo.CanRead && (!flag || propertyInfo.CanWriteExtended()))
				{
					list.Add(new KeyValuePair<NavigationProperty, PropertyInfo>(cspaceProperty, propertyInfo));
				}
				continue;
			}
			string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
			LogLoadMessage(message, cspaceType);
			return false;
		}
		foreach (KeyValuePair<NavigationProperty, PropertyInfo> item in list)
		{
			TrackClosure(item.Value.PropertyType);
			StructuralType ct = cspaceType;
			StructuralType ot = ospaceType;
			NavigationProperty cp = item.Key;
			referenceResolutionListForCurrentType.Add(delegate
			{
				CreateAndAddNavigationProperty(ct, ot, cp);
			});
		}
		return true;
	}

	private EdmType ResolveBaseType(StructuralType baseCSpaceType, Type type)
	{
		if (!CspaceToOspace.TryGetValue(baseCSpaceType, out var value))
		{
			LogError(Strings.Validator_OSpace_Convention_BaseTypeNotLoaded(type, baseCSpaceType), baseCSpaceType);
		}
		return value;
	}

	private bool TryFindAndCreatePrimitiveProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties)
	{
		foreach (EdmProperty cspaceProperty in from p in cspaceType.GetDeclaredOnlyMembers<EdmProperty>()
			where Helper.IsPrimitiveType(p.TypeUsage.EdmType)
			select p)
		{
			PropertyInfo propertyInfo = clrProperties.FirstOrDefault((PropertyInfo p) => MemberMatchesByConvention(p, cspaceProperty));
			if (propertyInfo != null)
			{
				if (TryGetPrimitiveType(propertyInfo.PropertyType, out var primitiveType))
				{
					if (propertyInfo.CanRead && propertyInfo.CanWriteExtended())
					{
						AddScalarMember(type, propertyInfo, ospaceType, cspaceProperty, primitiveType);
						continue;
					}
					string message = Strings.Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter(propertyInfo.Name, type.FullName, type.Assembly().FullName);
					LogLoadMessage(message, cspaceType);
					return false;
				}
				string message2 = Strings.Validator_OSpace_Convention_NonPrimitiveTypeProperty(propertyInfo.Name, type.FullName, propertyInfo.PropertyType.FullName);
				LogLoadMessage(message2, cspaceType);
				return false;
			}
			string message3 = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
			LogLoadMessage(message3, cspaceType);
			return false;
		}
		return true;
	}

	protected static bool TryGetPrimitiveType(Type type, out PrimitiveType primitiveType)
	{
		return ClrProviderManifest.Instance.TryGetPrimitiveType(Nullable.GetUnderlyingType(type) ?? type, out primitiveType);
	}

	private bool TryFindAndCreateEnumProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, IEnumerable<PropertyInfo> clrProperties, List<Action> referenceResolutionListForCurrentType)
	{
		List<KeyValuePair<EdmProperty, PropertyInfo>> list = new List<KeyValuePair<EdmProperty, PropertyInfo>>();
		foreach (EdmProperty cspaceProperty in from p in cspaceType.GetDeclaredOnlyMembers<EdmProperty>()
			where Helper.IsEnumType(p.TypeUsage.EdmType)
			select p)
		{
			PropertyInfo propertyInfo = clrProperties.FirstOrDefault((PropertyInfo p) => MemberMatchesByConvention(p, cspaceProperty));
			if (propertyInfo != null)
			{
				list.Add(new KeyValuePair<EdmProperty, PropertyInfo>(cspaceProperty, propertyInfo));
				continue;
			}
			string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
			LogLoadMessage(message, cspaceType);
			return false;
		}
		foreach (KeyValuePair<EdmProperty, PropertyInfo> item in list)
		{
			TrackClosure(item.Value.PropertyType);
			StructuralType ot = ospaceType;
			EdmProperty cp = item.Key;
			PropertyInfo clrp = item.Value;
			referenceResolutionListForCurrentType.Add(delegate
			{
				CreateAndAddEnumProperty(type, ot, cp, clrp);
			});
		}
		return true;
	}

	private static bool MemberMatchesByConvention(PropertyInfo clrProperty, EdmMember cspaceMember)
	{
		return clrProperty.Name == cspaceMember.Name;
	}

	private void CreateAndAddComplexType(Type type, StructuralType ospaceType, EdmProperty cspaceProperty, PropertyInfo clrProperty)
	{
		if (CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out var value))
		{
			EdmProperty member = new EdmProperty(cspaceProperty.Name, TypeUsage.Create(value, new FacetValues
			{
				Nullable = false
			}), clrProperty, type);
			ospaceType.AddMember(member);
		}
		else
		{
			LogError(Strings.Validator_OSpace_Convention_MissingOSpaceType(cspaceProperty.TypeUsage.EdmType.FullName), cspaceProperty.TypeUsage.EdmType);
		}
	}

	private static bool NonPrimitiveMemberMatchesByConvention(PropertyInfo clrProperty, EdmMember cspaceMember)
	{
		if (!clrProperty.PropertyType.IsValueType() && !clrProperty.PropertyType.IsAssignableFrom(typeof(string)))
		{
			return clrProperty.Name == cspaceMember.Name;
		}
		return false;
	}

	private void CreateAndAddNavigationProperty(StructuralType cspaceType, StructuralType ospaceType, NavigationProperty cspaceProperty)
	{
		if (CspaceToOspace.TryGetValue(cspaceProperty.RelationshipType, out var value))
		{
			EdmType edmType = null;
			EdmType value3;
			if (Helper.IsCollectionType(cspaceProperty.TypeUsage.EdmType))
			{
				if (CspaceToOspace.TryGetValue(((CollectionType)cspaceProperty.TypeUsage.EdmType).TypeUsage.EdmType, out var value2))
				{
					edmType = value2.GetCollectionType();
				}
			}
			else if (CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out value3))
			{
				edmType = value3;
			}
			NavigationProperty navigationProperty = new NavigationProperty(cspaceProperty.Name, TypeUsage.Create(edmType));
			RelationshipType relationshipType2 = (navigationProperty.RelationshipType = (RelationshipType)value);
			navigationProperty.ToEndMember = (RelationshipEndMember)relationshipType2.Members.First((EdmMember e) => e.Name == cspaceProperty.ToEndMember.Name);
			navigationProperty.FromEndMember = (RelationshipEndMember)relationshipType2.Members.First((EdmMember e) => e.Name == cspaceProperty.FromEndMember.Name);
			ospaceType.AddMember(navigationProperty);
		}
		else
		{
			EntityTypeBase entityTypeBase = cspaceProperty.RelationshipType.RelationshipEndMembers.Select((RelationshipEndMember e) => ((RefType)e.TypeUsage.EdmType).ElementType).First((EntityTypeBase e) => e != cspaceType);
			LogError(Strings.Validator_OSpace_Convention_RelationshipNotLoaded(cspaceProperty.RelationshipType.FullName, entityTypeBase.FullName), entityTypeBase);
		}
	}

	private void CreateAndAddEnumProperty(Type type, StructuralType ospaceType, EdmProperty cspaceProperty, PropertyInfo clrProperty)
	{
		if (CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out var value))
		{
			if (clrProperty.CanRead && clrProperty.CanWriteExtended())
			{
				AddScalarMember(type, clrProperty, ospaceType, cspaceProperty, value);
			}
			else
			{
				LogError(Strings.Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter(clrProperty.Name, type.FullName, type.Assembly().FullName), cspaceProperty.TypeUsage.EdmType);
			}
		}
		else
		{
			LogError(Strings.Validator_OSpace_Convention_MissingOSpaceType(cspaceProperty.TypeUsage.EdmType.FullName), cspaceProperty.TypeUsage.EdmType);
		}
	}

	private static void AddScalarMember(Type type, PropertyInfo clrProperty, StructuralType ospaceType, EdmProperty cspaceProperty, EdmType propertyType)
	{
		StructuralType declaringType = cspaceProperty.DeclaringType;
		int num;
		int num2;
		if (Helper.IsEntityType(declaringType))
		{
			num = (((EntityType)declaringType).KeyMemberNames.Contains(clrProperty.Name) ? 1 : 0);
			if (num != 0)
			{
				num2 = 0;
				goto IL_004f;
			}
		}
		else
		{
			num = 0;
		}
		num2 = ((!clrProperty.PropertyType.IsValueType() || Nullable.GetUnderlyingType(clrProperty.PropertyType) != null) ? 1 : 0);
		goto IL_004f;
		IL_004f:
		bool value = (byte)num2 != 0;
		EdmProperty member = new EdmProperty(cspaceProperty.Name, TypeUsage.Create(propertyType, new FacetValues
		{
			Nullable = value
		}), clrProperty, type);
		if (num != 0)
		{
			((EntityType)ospaceType).AddKeyMember(member);
		}
		else
		{
			ospaceType.AddMember(member);
		}
	}

	public virtual void CreateRelationships(EdmItemCollection edmItemCollection)
	{
		foreach (AssociationType item in edmItemCollection.GetItems<AssociationType>())
		{
			if (CspaceToOspace.ContainsKey(item))
			{
				continue;
			}
			EdmType[] array = new EdmType[2];
			if (CspaceToOspace.TryGetValue(GetRelationshipEndType(item.RelationshipEndMembers[0]), out array[0]) && CspaceToOspace.TryGetValue(GetRelationshipEndType(item.RelationshipEndMembers[1]), out array[1]))
			{
				AssociationType associationType = new AssociationType(item.Name, item.NamespaceName, item.IsForeignKey, DataSpace.OSpace);
				for (int i = 0; i < item.RelationshipEndMembers.Count; i++)
				{
					EntityType entityType = (EntityType)array[i];
					RelationshipEndMember relationshipEndMember = item.RelationshipEndMembers[i];
					associationType.AddKeyMember(new AssociationEndMember(relationshipEndMember.Name, entityType.GetReferenceType(), relationshipEndMember.RelationshipMultiplicity));
				}
				AddToTypesInAssembly(associationType);
				LoadedTypes.Add(associationType.FullName, associationType);
				CspaceToOspace.Add(item, associationType);
			}
		}
	}

	private static StructuralType GetRelationshipEndType(RelationshipEndMember relationshipEndMember)
	{
		return ((RefType)relationshipEndMember.TypeUsage.EdmType).ElementType;
	}
}
