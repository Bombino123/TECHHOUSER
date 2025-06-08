using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class ObjectItemAttributeAssemblyLoader : ObjectItemAssemblyLoader
{
	private readonly List<Action> _unresolvedNavigationProperties = new List<Action>();

	private readonly List<Action> _referenceResolutions = new List<Action>();

	private new MutableAssemblyCacheEntry CacheEntry => (MutableAssemblyCacheEntry)base.CacheEntry;

	internal ObjectItemAttributeAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
		: base(assembly, new MutableAssemblyCacheEntry(), sessionData)
	{
	}

	internal override void OnLevel1SessionProcessing()
	{
		foreach (Action referenceResolution in _referenceResolutions)
		{
			referenceResolution();
		}
	}

	internal override void OnLevel2SessionProcessing()
	{
		foreach (Action unresolvedNavigationProperty in _unresolvedNavigationProperties)
		{
			unresolvedNavigationProperty();
		}
	}

	internal override void Load()
	{
		base.Load();
	}

	protected override void AddToAssembliesLoaded()
	{
		base.SessionData.AssembliesLoaded.Add(base.SourceAssembly, CacheEntry);
	}

	private bool TryGetLoadedType(Type clrType, out EdmType edmType)
	{
		if (base.SessionData.TypesInLoading.TryGetValue(clrType.FullName, out edmType) || TryGetCachedEdmType(clrType, out edmType))
		{
			if (edmType.ClrType != clrType)
			{
				base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NewTypeConflictsWithExistingType(clrType.AssemblyQualifiedName, edmType.ClrType.AssemblyQualifiedName)));
				edmType = null;
				return false;
			}
			return true;
		}
		if (clrType.IsGenericType())
		{
			if (!TryGetLoadedType(clrType.GetGenericArguments()[0], out var edmType2))
			{
				return false;
			}
			if (typeof(IEnumerable).IsAssignableFrom(clrType))
			{
				if (!(edmType2 is EntityType entityType))
				{
					return false;
				}
				edmType = entityType.GetCollectionType();
			}
			else
			{
				edmType = edmType2;
			}
			return true;
		}
		edmType = null;
		return false;
	}

	private bool TryGetCachedEdmType(Type clrType, out EdmType edmType)
	{
		if (base.SessionData.LockedAssemblyCache.TryGetValue(clrType.Assembly(), out var cacheEntry))
		{
			return cacheEntry.TryGetEdmType(clrType.FullName, out edmType);
		}
		edmType = null;
		return false;
	}

	protected override void LoadTypesFromAssembly()
	{
		LoadRelationshipTypes();
		foreach (Type accessibleType in base.SourceAssembly.GetAccessibleTypes())
		{
			if (accessibleType.GetCustomAttributes<EdmTypeAttribute>(inherit: false).Any())
			{
				if (accessibleType.IsGenericType())
				{
					base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.GenericTypeNotSupported(accessibleType.FullName)));
				}
				else
				{
					LoadType(accessibleType);
				}
			}
		}
		if (_referenceResolutions.Count != 0)
		{
			base.SessionData.RegisterForLevel1PostSessionProcessing(this);
		}
		if (_unresolvedNavigationProperties.Count != 0)
		{
			base.SessionData.RegisterForLevel2PostSessionProcessing(this);
		}
	}

	private void LoadRelationshipTypes()
	{
		foreach (EdmRelationshipAttribute customAttribute in base.SourceAssembly.GetCustomAttributes<EdmRelationshipAttribute>())
		{
			if (TryFindNullParametersInRelationshipAttribute(customAttribute))
			{
				continue;
			}
			bool flag = false;
			if (customAttribute.Role1Name == customAttribute.Role2Name)
			{
				base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.SameRoleNameOnRelationshipAttribute(customAttribute.RelationshipName, customAttribute.Role2Name)));
				flag = true;
			}
			if (!flag)
			{
				AssociationType associationType = new AssociationType(customAttribute.RelationshipName, customAttribute.RelationshipNamespaceName, customAttribute.IsForeignKey, DataSpace.OSpace);
				base.SessionData.TypesInLoading.Add(associationType.FullName, associationType);
				TrackClosure(customAttribute.Role1Type);
				TrackClosure(customAttribute.Role2Type);
				string r1Name = customAttribute.Role1Name;
				Type r1Type = customAttribute.Role1Type;
				RelationshipMultiplicity r1Multiplicity = customAttribute.Role1Multiplicity;
				AddTypeResolver(delegate
				{
					ResolveAssociationEnd(associationType, r1Name, r1Type, r1Multiplicity);
				});
				string r2Name = customAttribute.Role2Name;
				Type r2Type = customAttribute.Role2Type;
				RelationshipMultiplicity r2Multiplicity = customAttribute.Role2Multiplicity;
				AddTypeResolver(delegate
				{
					ResolveAssociationEnd(associationType, r2Name, r2Type, r2Multiplicity);
				});
				CacheEntry.TypesInAssembly.Add(associationType);
			}
		}
	}

	private void ResolveAssociationEnd(AssociationType associationType, string roleName, Type clrType, RelationshipMultiplicity multiplicity)
	{
		if (!TryGetRelationshipEndEntityType(clrType, out var entityType))
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.RoleTypeInEdmRelationshipAttributeIsInvalidType(associationType.Name, roleName, clrType)));
		}
		else
		{
			associationType.AddKeyMember(new AssociationEndMember(roleName, entityType.GetReferenceType(), multiplicity));
		}
	}

	private void LoadType(Type clrType)
	{
		EdmType edmType = null;
		IEnumerable<EdmTypeAttribute> customAttributes = clrType.GetCustomAttributes<EdmTypeAttribute>(inherit: false);
		if (!customAttributes.Any())
		{
			return;
		}
		if (clrType.IsNested)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NestedClassNotSupported(clrType.FullName, clrType.Assembly().FullName)));
			return;
		}
		EdmTypeAttribute edmTypeAttribute = customAttributes.First();
		string cspaceTypeName = (string.IsNullOrEmpty(edmTypeAttribute.Name) ? clrType.Name : edmTypeAttribute.Name);
		if (string.IsNullOrEmpty(edmTypeAttribute.NamespaceName) && clrType.Namespace == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_TypeHasNoNamespace));
			return;
		}
		string cspaceNamespaceName = (string.IsNullOrEmpty(edmTypeAttribute.NamespaceName) ? clrType.Namespace : edmTypeAttribute.NamespaceName);
		if (edmTypeAttribute.GetType() == typeof(EdmEntityTypeAttribute))
		{
			edmType = new ClrEntityType(clrType, cspaceNamespaceName, cspaceTypeName);
		}
		else if (edmTypeAttribute.GetType() == typeof(EdmComplexTypeAttribute))
		{
			edmType = new ClrComplexType(clrType, cspaceNamespaceName, cspaceTypeName);
		}
		else
		{
			if (!ClrProviderManifest.Instance.TryGetPrimitiveType(clrType.GetEnumUnderlyingType(), out var _))
			{
				base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_UnsupportedEnumUnderlyingType(clrType.GetEnumUnderlyingType().FullName)));
				return;
			}
			edmType = new ClrEnumType(clrType, cspaceNamespaceName, cspaceTypeName);
		}
		CacheEntry.TypesInAssembly.Add(edmType);
		base.SessionData.TypesInLoading.Add(clrType.FullName, edmType);
		if (!Helper.IsStructuralType(edmType))
		{
			return;
		}
		if (Helper.IsEntityType(edmType))
		{
			TrackClosure(clrType.BaseType());
			AddTypeResolver(delegate
			{
				edmType.BaseType = ResolveBaseType(clrType.BaseType());
			});
		}
		LoadPropertiesFromType((StructuralType)edmType);
	}

	private void AddTypeResolver(Action resolver)
	{
		_referenceResolutions.Add(resolver);
	}

	private EdmType ResolveBaseType(Type type)
	{
		if (type.GetCustomAttributes<EdmEntityTypeAttribute>(inherit: false).Any() && TryGetLoadedType(type, out var edmType))
		{
			return edmType;
		}
		return null;
	}

	private bool TryFindNullParametersInRelationshipAttribute(EdmRelationshipAttribute roleAttribute)
	{
		if (roleAttribute.RelationshipName == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NullRelationshipNameforEdmRelationshipAttribute(base.SourceAssembly.FullName)));
			return true;
		}
		bool result = false;
		if (roleAttribute.RelationshipNamespaceName == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NullParameterForEdmRelationshipAttribute("RelationshipNamespaceName", roleAttribute.RelationshipName)));
			result = true;
		}
		if (roleAttribute.Role1Name == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NullParameterForEdmRelationshipAttribute("Role1Name", roleAttribute.RelationshipName)));
			result = true;
		}
		if (roleAttribute.Role1Type == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NullParameterForEdmRelationshipAttribute("Role1Type", roleAttribute.RelationshipName)));
			result = true;
		}
		if (roleAttribute.Role2Name == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NullParameterForEdmRelationshipAttribute("Role2Name", roleAttribute.RelationshipName)));
			result = true;
		}
		if (roleAttribute.Role2Type == null)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NullParameterForEdmRelationshipAttribute("Role2Type", roleAttribute.RelationshipName)));
			result = true;
		}
		return result;
	}

	private bool TryGetRelationshipEndEntityType(Type type, out EntityType entityType)
	{
		if (type == null)
		{
			entityType = null;
			return false;
		}
		if (!TryGetLoadedType(type, out var edmType) || !Helper.IsEntityType(edmType))
		{
			entityType = null;
			return false;
		}
		entityType = (EntityType)edmType;
		return true;
	}

	private void LoadPropertiesFromType(StructuralType structuralType)
	{
		foreach (PropertyInfo item in from p in structuralType.ClrType.GetDeclaredProperties()
			where !p.IsStatic()
			select p)
		{
			EdmMember edmMember = null;
			bool isEntityKeyProperty = false;
			if (item.GetCustomAttributes<EdmRelationshipNavigationPropertyAttribute>(inherit: false).Any())
			{
				PropertyInfo pi = item;
				_unresolvedNavigationProperties.Add(delegate
				{
					ResolveNavigationProperty(structuralType, pi);
				});
			}
			else if (item.GetCustomAttributes<EdmScalarPropertyAttribute>(inherit: false).Any())
			{
				if ((Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType).IsEnum())
				{
					TrackClosure(item.PropertyType);
					PropertyInfo local2 = item;
					AddTypeResolver(delegate
					{
						ResolveEnumTypeProperty(structuralType, local2);
					});
				}
				else
				{
					edmMember = LoadScalarProperty(structuralType.ClrType, item, out isEntityKeyProperty);
				}
			}
			else if (item.GetCustomAttributes<EdmComplexPropertyAttribute>(inherit: false).Any())
			{
				TrackClosure(item.PropertyType);
				PropertyInfo local = item;
				AddTypeResolver(delegate
				{
					ResolveComplexTypeProperty(structuralType, local);
				});
			}
			if (edmMember != null)
			{
				structuralType.AddMember(edmMember);
				if (Helper.IsEntityType(structuralType) && isEntityKeyProperty)
				{
					((EntityType)structuralType).AddKeyMember(edmMember);
				}
			}
		}
	}

	internal void ResolveNavigationProperty(StructuralType declaringType, PropertyInfo propertyInfo)
	{
		IEnumerable<EdmRelationshipNavigationPropertyAttribute> customAttributes = propertyInfo.GetCustomAttributes<EdmRelationshipNavigationPropertyAttribute>(inherit: false);
		if (!TryGetLoadedType(propertyInfo.PropertyType, out var edmType) || (edmType.BuiltInTypeKind != BuiltInTypeKind.EntityType && edmType.BuiltInTypeKind != BuiltInTypeKind.CollectionType))
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_InvalidNavPropReturnType(propertyInfo.Name, propertyInfo.DeclaringType.FullName, propertyInfo.PropertyType.FullName)));
			return;
		}
		EdmRelationshipNavigationPropertyAttribute edmRelationshipNavigationPropertyAttribute = customAttributes.First();
		EdmMember edmMember = null;
		if (base.SessionData.TypesInLoading.TryGetValue(edmRelationshipNavigationPropertyAttribute.RelationshipNamespaceName + "." + edmRelationshipNavigationPropertyAttribute.RelationshipName, out var value) && Helper.IsAssociationType(value))
		{
			AssociationType associationType = (AssociationType)value;
			if (associationType != null)
			{
				NavigationProperty navigationProperty = new NavigationProperty(propertyInfo.Name, TypeUsage.Create(edmType));
				navigationProperty.RelationshipType = associationType;
				edmMember = navigationProperty;
				if (associationType.Members[0].Name == edmRelationshipNavigationPropertyAttribute.TargetRoleName)
				{
					navigationProperty.ToEndMember = (RelationshipEndMember)associationType.Members[0];
					navigationProperty.FromEndMember = (RelationshipEndMember)associationType.Members[1];
				}
				else if (associationType.Members[1].Name == edmRelationshipNavigationPropertyAttribute.TargetRoleName)
				{
					navigationProperty.ToEndMember = (RelationshipEndMember)associationType.Members[1];
					navigationProperty.FromEndMember = (RelationshipEndMember)associationType.Members[0];
				}
				else
				{
					base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.TargetRoleNameInNavigationPropertyNotValid(propertyInfo.Name, propertyInfo.DeclaringType.FullName, edmRelationshipNavigationPropertyAttribute.TargetRoleName, edmRelationshipNavigationPropertyAttribute.RelationshipName)));
					edmMember = null;
				}
				if (edmMember != null && ((RefType)navigationProperty.FromEndMember.TypeUsage.EdmType).ElementType.ClrType != declaringType.ClrType)
				{
					base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.NavigationPropertyRelationshipEndTypeMismatch(declaringType.FullName, navigationProperty.Name, associationType.FullName, navigationProperty.FromEndMember.Name, ((RefType)navigationProperty.FromEndMember.TypeUsage.EdmType).ElementType.ClrType)));
					edmMember = null;
				}
			}
		}
		else
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.RelationshipNameInNavigationPropertyNotValid(propertyInfo.Name, propertyInfo.DeclaringType.FullName, edmRelationshipNavigationPropertyAttribute.RelationshipName)));
		}
		if (edmMember != null)
		{
			declaringType.AddMember(edmMember);
		}
	}

	private EdmMember LoadScalarProperty(Type clrType, PropertyInfo property, out bool isEntityKeyProperty)
	{
		EdmMember result = null;
		isEntityKeyProperty = false;
		if (!ObjectItemAssemblyLoader.TryGetPrimitiveType(property.PropertyType, out var primitiveType))
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_ScalarPropertyNotPrimitive(property.Name, property.DeclaringType.FullName, property.PropertyType.FullName)));
		}
		else
		{
			IEnumerable<EdmScalarPropertyAttribute> customAttributes = property.GetCustomAttributes<EdmScalarPropertyAttribute>(inherit: false);
			isEntityKeyProperty = customAttributes.First().EntityKeyProperty;
			bool isNullable = customAttributes.First().IsNullable;
			result = new EdmProperty(property.Name, TypeUsage.Create(primitiveType, new FacetValues
			{
				Nullable = isNullable
			}), property, clrType);
		}
		return result;
	}

	private void ResolveEnumTypeProperty(StructuralType declaringType, PropertyInfo clrProperty)
	{
		if (!TryGetLoadedType(clrProperty.PropertyType, out var edmType) || !Helper.IsEnumType(edmType))
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_ScalarPropertyNotPrimitive(clrProperty.Name, clrProperty.DeclaringType.FullName, clrProperty.PropertyType.FullName)));
			return;
		}
		EdmScalarPropertyAttribute edmScalarPropertyAttribute = clrProperty.GetCustomAttributes<EdmScalarPropertyAttribute>(inherit: false).Single();
		EdmProperty member = new EdmProperty(clrProperty.Name, TypeUsage.Create(edmType, new FacetValues
		{
			Nullable = edmScalarPropertyAttribute.IsNullable
		}), clrProperty, declaringType.ClrType);
		declaringType.AddMember(member);
		if (declaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType && edmScalarPropertyAttribute.EntityKeyProperty)
		{
			((EntityType)declaringType).AddKeyMember(member);
		}
	}

	private void ResolveComplexTypeProperty(StructuralType type, PropertyInfo clrProperty)
	{
		if (!TryGetLoadedType(clrProperty.PropertyType, out var edmType) || edmType.BuiltInTypeKind != BuiltInTypeKind.ComplexType)
		{
			base.SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_ComplexPropertyNotComplex(clrProperty.Name, clrProperty.DeclaringType.FullName, clrProperty.PropertyType.FullName)));
			return;
		}
		EdmProperty member = new EdmProperty(clrProperty.Name, TypeUsage.Create(edmType, new FacetValues
		{
			Nullable = false
		}), clrProperty, type.ClrType);
		type.AddMember(member);
	}

	private void TrackClosure(Type type)
	{
		if (base.SourceAssembly != type.Assembly() && !CacheEntry.ClosureAssemblies.Contains(type.Assembly()) && IsSchemaAttributePresent(type.Assembly()) && (!type.IsGenericType() || (!EntityUtil.IsAnICollection(type) && !(type.GetGenericTypeDefinition() == typeof(EntityReference<>)) && !(type.GetGenericTypeDefinition() == typeof(Nullable<>)))))
		{
			CacheEntry.ClosureAssemblies.Add(type.Assembly());
		}
		if (type.IsGenericType())
		{
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				TrackClosure(type2);
			}
		}
	}

	internal static bool IsSchemaAttributePresent(Assembly assembly)
	{
		return assembly.GetCustomAttributes<EdmSchemaAttribute>().Any();
	}

	internal static ObjectItemAssemblyLoader Create(Assembly assembly, ObjectItemLoadingSessionData sessionData)
	{
		if (!IsSchemaAttributePresent(assembly))
		{
			return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
		}
		return new ObjectItemAttributeAssemblyLoader(assembly, sessionData);
	}
}
