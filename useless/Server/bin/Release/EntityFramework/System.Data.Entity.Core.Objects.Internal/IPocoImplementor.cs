using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Data.Entity.Core.Objects.Internal;

internal class IPocoImplementor
{
	private readonly EntityType _ospaceEntityType;

	private FieldBuilder _changeTrackerField;

	private FieldBuilder _relationshipManagerField;

	private FieldBuilder _resetFKSetterFlagField;

	private FieldBuilder _compareByteArraysField;

	private MethodBuilder _entityMemberChanging;

	private MethodBuilder _entityMemberChanged;

	private MethodBuilder _getRelationshipManager;

	private readonly List<KeyValuePair<NavigationProperty, PropertyInfo>> _referenceProperties;

	private readonly List<KeyValuePair<NavigationProperty, PropertyInfo>> _collectionProperties;

	private bool _implementIEntityWithChangeTracker;

	private bool _implementIEntityWithRelationships;

	private HashSet<EdmMember> _scalarMembers;

	private HashSet<EdmMember> _relationshipMembers;

	internal static readonly MethodInfo EntityMemberChangingMethod = typeof(IEntityChangeTracker).GetDeclaredMethod("EntityMemberChanging", typeof(string));

	internal static readonly MethodInfo EntityMemberChangedMethod = typeof(IEntityChangeTracker).GetDeclaredMethod("EntityMemberChanged", typeof(string));

	internal static readonly MethodInfo CreateRelationshipManagerMethod = typeof(RelationshipManager).GetDeclaredMethod("Create", typeof(IEntityWithRelationships));

	internal static readonly MethodInfo GetRelationshipManagerMethod = typeof(IEntityWithRelationships).GetDeclaredProperty("RelationshipManager").Getter();

	internal static readonly MethodInfo GetRelatedReferenceMethod = typeof(RelationshipManager).GetDeclaredMethod("GetRelatedReference", typeof(string), typeof(string));

	internal static readonly MethodInfo GetRelatedCollectionMethod = typeof(RelationshipManager).GetDeclaredMethod("GetRelatedCollection", typeof(string), typeof(string));

	internal static readonly MethodInfo GetRelatedEndMethod = typeof(RelationshipManager).GetDeclaredMethod("GetRelatedEnd", typeof(string), typeof(string));

	internal static readonly MethodInfo ObjectEqualsMethod = typeof(object).GetDeclaredMethod("Equals", typeof(object), typeof(object));

	private static readonly ConstructorInfo _invalidOperationConstructorMethod = typeof(InvalidOperationException).GetDeclaredConstructor(typeof(string));

	internal static readonly MethodInfo GetEntityMethod = typeof(IEntityWrapper).GetDeclaredProperty("Entity").Getter();

	internal static readonly MethodInfo InvokeMethod = typeof(Action<object>).GetDeclaredMethod("Invoke", typeof(object));

	internal static readonly MethodInfo FuncInvokeMethod = typeof(Func<object, object, bool>).GetDeclaredMethod("Invoke", typeof(object), typeof(object));

	internal static readonly MethodInfo SetChangeTrackerMethod = typeof(IEntityWithChangeTracker).GetOnlyDeclaredMethod("SetChangeTracker");

	public Type[] Interfaces
	{
		get
		{
			List<Type> list = new List<Type>();
			if (_implementIEntityWithChangeTracker)
			{
				list.Add(typeof(IEntityWithChangeTracker));
			}
			if (_implementIEntityWithRelationships)
			{
				list.Add(typeof(IEntityWithRelationships));
			}
			return list.ToArray();
		}
	}

	public IPocoImplementor(EntityType ospaceEntityType)
	{
		Type clrType = ospaceEntityType.ClrType;
		_referenceProperties = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
		_collectionProperties = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
		_implementIEntityWithChangeTracker = null == clrType.GetInterface(typeof(IEntityWithChangeTracker).Name);
		_implementIEntityWithRelationships = null == clrType.GetInterface(typeof(IEntityWithRelationships).Name);
		CheckType(ospaceEntityType);
		_ospaceEntityType = ospaceEntityType;
	}

	private void CheckType(EntityType ospaceEntityType)
	{
		_scalarMembers = new HashSet<EdmMember>();
		_relationshipMembers = new HashSet<EdmMember>();
		foreach (EdmMember member in ospaceEntityType.Members)
		{
			PropertyInfo topProperty = ospaceEntityType.ClrType.GetTopProperty(member.Name);
			if (!(topProperty != null) || !EntityProxyFactory.CanProxySetter(topProperty))
			{
				continue;
			}
			if (member.BuiltInTypeKind == BuiltInTypeKind.EdmProperty)
			{
				if (_implementIEntityWithChangeTracker)
				{
					_scalarMembers.Add(member);
				}
			}
			else
			{
				if (member.BuiltInTypeKind != BuiltInTypeKind.NavigationProperty || !_implementIEntityWithRelationships)
				{
					continue;
				}
				if (((NavigationProperty)member).ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
				{
					if (topProperty.PropertyType.IsGenericType() && topProperty.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
					{
						_relationshipMembers.Add(member);
					}
				}
				else
				{
					_relationshipMembers.Add(member);
				}
			}
		}
		if (ospaceEntityType.Members.Count != _scalarMembers.Count + _relationshipMembers.Count)
		{
			_scalarMembers.Clear();
			_relationshipMembers.Clear();
			_implementIEntityWithChangeTracker = false;
			_implementIEntityWithRelationships = false;
		}
	}

	public void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
	{
		if (_implementIEntityWithChangeTracker)
		{
			ImplementIEntityWithChangeTracker(typeBuilder, registerField);
		}
		if (_implementIEntityWithRelationships)
		{
			ImplementIEntityWithRelationships(typeBuilder, registerField);
		}
		_resetFKSetterFlagField = typeBuilder.DefineField("_resetFKSetterFlag", typeof(Action<object>), FieldAttributes.Private | FieldAttributes.Static);
		_compareByteArraysField = typeBuilder.DefineField("_compareByteArrays", typeof(Func<object, object, bool>), FieldAttributes.Private | FieldAttributes.Static);
	}

	private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes)
	{
		return new DynamicMethod(name, returnType, parameterTypes, restrictedSkipVisibility: true);
	}

	public DynamicMethod CreateInitializeCollectionMethod(Type proxyType)
	{
		if (_collectionProperties.Count > 0)
		{
			DynamicMethod dynamicMethod = CreateDynamicMethod(proxyType.Name + "_InitializeEntityCollections", typeof(IEntityWrapper), new Type[1] { typeof(IEntityWrapper) });
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.DeclareLocal(proxyType);
			iLGenerator.DeclareLocal(typeof(RelationshipManager));
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Callvirt, GetEntityMethod);
			iLGenerator.Emit(OpCodes.Castclass, proxyType);
			iLGenerator.Emit(OpCodes.Stloc_0);
			iLGenerator.Emit(OpCodes.Ldloc_0);
			iLGenerator.Emit(OpCodes.Callvirt, GetRelationshipManagerMethod);
			iLGenerator.Emit(OpCodes.Stloc_1);
			foreach (KeyValuePair<NavigationProperty, PropertyInfo> collectionProperty in _collectionProperties)
			{
				MethodInfo meth = GetRelatedCollectionMethod.MakeGenericMethod(EntityUtil.GetCollectionElementType(collectionProperty.Value.PropertyType));
				iLGenerator.Emit(OpCodes.Ldloc_0);
				iLGenerator.Emit(OpCodes.Ldloc_1);
				iLGenerator.Emit(OpCodes.Ldstr, collectionProperty.Key.RelationshipType.FullName);
				iLGenerator.Emit(OpCodes.Ldstr, collectionProperty.Key.ToEndMember.Name);
				iLGenerator.Emit(OpCodes.Callvirt, meth);
				iLGenerator.Emit(OpCodes.Callvirt, collectionProperty.Value.Setter());
			}
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ret);
			return dynamicMethod;
		}
		return null;
	}

	public bool CanProxyMember(EdmMember member)
	{
		if (!_scalarMembers.Contains(member))
		{
			return _relationshipMembers.Contains(member);
		}
		return true;
	}

	public bool EmitMember(TypeBuilder typeBuilder, EdmMember member, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, BaseProxyImplementor baseImplementor)
	{
		if (_scalarMembers.Contains(member))
		{
			bool isKeyMember = _ospaceEntityType.KeyMembers.Contains(member.Identity);
			EmitScalarSetter(typeBuilder, propertyBuilder, baseProperty, isKeyMember);
			return true;
		}
		if (_relationshipMembers.Contains(member))
		{
			NavigationProperty navigationProperty = member as NavigationProperty;
			if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				EmitCollectionProperty(typeBuilder, propertyBuilder, baseProperty, navigationProperty);
			}
			else
			{
				EmitReferenceProperty(typeBuilder, propertyBuilder, baseProperty, navigationProperty);
			}
			baseImplementor.AddBasePropertySetter(baseProperty);
			return true;
		}
		return false;
	}

	private void EmitScalarSetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, bool isKeyMember)
	{
		MethodInfo methodInfo = baseProperty.Setter();
		MethodAttributes methodAttributes = methodInfo.Attributes & MethodAttributes.MemberAccessMask;
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAttributes | (MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName), null, new Type[1] { baseProperty.PropertyType });
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		Label label = iLGenerator.DefineLabel();
		if (isKeyMember)
		{
			MethodInfo methodInfo2 = baseProperty.Getter();
			if (methodInfo2 != null)
			{
				Type propertyType = baseProperty.PropertyType;
				if (propertyType == typeof(int) || propertyType == typeof(short) || propertyType == typeof(long) || propertyType == typeof(bool) || propertyType == typeof(byte) || propertyType == typeof(uint) || propertyType == typeof(ulong) || propertyType == typeof(float) || propertyType == typeof(double) || propertyType.IsEnum())
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Call, methodInfo2);
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Beq_S, label);
				}
				else if (propertyType == typeof(byte[]))
				{
					iLGenerator.Emit(OpCodes.Ldsfld, _compareByteArraysField);
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Call, methodInfo2);
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Callvirt, FuncInvokeMethod);
					iLGenerator.Emit(OpCodes.Brtrue_S, label);
				}
				else
				{
					MethodInfo declaredMethod = propertyType.GetDeclaredMethod("op_Inequality", propertyType, propertyType);
					if (declaredMethod != null)
					{
						iLGenerator.Emit(OpCodes.Ldarg_0);
						iLGenerator.Emit(OpCodes.Call, methodInfo2);
						iLGenerator.Emit(OpCodes.Ldarg_1);
						iLGenerator.Emit(OpCodes.Call, declaredMethod);
						iLGenerator.Emit(OpCodes.Brfalse_S, label);
					}
					else
					{
						iLGenerator.Emit(OpCodes.Ldarg_0);
						iLGenerator.Emit(OpCodes.Call, methodInfo2);
						if (propertyType.IsValueType())
						{
							iLGenerator.Emit(OpCodes.Box, propertyType);
						}
						iLGenerator.Emit(OpCodes.Ldarg_1);
						if (propertyType.IsValueType())
						{
							iLGenerator.Emit(OpCodes.Box, propertyType);
						}
						iLGenerator.Emit(OpCodes.Call, ObjectEqualsMethod);
						iLGenerator.Emit(OpCodes.Brtrue_S, label);
					}
				}
			}
		}
		iLGenerator.BeginExceptionBlock();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldstr, baseProperty.Name);
		iLGenerator.Emit(OpCodes.Call, _entityMemberChanging);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Call, methodInfo);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldstr, baseProperty.Name);
		iLGenerator.Emit(OpCodes.Call, _entityMemberChanged);
		iLGenerator.BeginFinallyBlock();
		iLGenerator.Emit(OpCodes.Ldsfld, _resetFKSetterFlagField);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Callvirt, InvokeMethod);
		iLGenerator.EndExceptionBlock();
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ret);
		propertyBuilder.SetSetMethod(methodBuilder);
	}

	private void EmitReferenceProperty(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, NavigationProperty navProperty)
	{
		MethodAttributes methodAttributes = baseProperty.Setter().Attributes & MethodAttributes.MemberAccessMask;
		MethodInfo meth = GetRelatedReferenceMethod.MakeGenericMethod(baseProperty.PropertyType);
		MethodInfo onlyDeclaredMethod = typeof(EntityReference<>).MakeGenericType(baseProperty.PropertyType).GetOnlyDeclaredMethod("set_Value");
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAttributes | (MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName), null, new Type[1] { baseProperty.PropertyType });
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Callvirt, _getRelationshipManager);
		iLGenerator.Emit(OpCodes.Ldstr, navProperty.RelationshipType.FullName);
		iLGenerator.Emit(OpCodes.Ldstr, navProperty.ToEndMember.Name);
		iLGenerator.Emit(OpCodes.Callvirt, meth);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Callvirt, onlyDeclaredMethod);
		iLGenerator.Emit(OpCodes.Ret);
		propertyBuilder.SetSetMethod(methodBuilder);
		_referenceProperties.Add(new KeyValuePair<NavigationProperty, PropertyInfo>(navProperty, baseProperty));
	}

	private void EmitCollectionProperty(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, NavigationProperty navProperty)
	{
		MethodAttributes methodAttributes = baseProperty.Setter().Attributes & MethodAttributes.MemberAccessMask;
		string str = Strings.EntityProxyTypeInfo_CannotSetEntityCollectionProperty(propertyBuilder.Name, typeBuilder.Name);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAttributes | (MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName), null, new Type[1] { baseProperty.PropertyType });
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		Label label = iLGenerator.DefineLabel();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Call, _getRelationshipManager);
		iLGenerator.Emit(OpCodes.Ldstr, navProperty.RelationshipType.FullName);
		iLGenerator.Emit(OpCodes.Ldstr, navProperty.ToEndMember.Name);
		iLGenerator.Emit(OpCodes.Callvirt, GetRelatedEndMethod);
		iLGenerator.Emit(OpCodes.Beq_S, label);
		iLGenerator.Emit(OpCodes.Ldstr, str);
		iLGenerator.Emit(OpCodes.Newobj, _invalidOperationConstructorMethod);
		iLGenerator.Emit(OpCodes.Throw);
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Call, baseProperty.Setter());
		iLGenerator.Emit(OpCodes.Ret);
		propertyBuilder.SetSetMethod(methodBuilder);
		_collectionProperties.Add(new KeyValuePair<NavigationProperty, PropertyInfo>(navProperty, baseProperty));
	}

	private void ImplementIEntityWithChangeTracker(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
	{
		_changeTrackerField = typeBuilder.DefineField("_changeTracker", typeof(IEntityChangeTracker), FieldAttributes.Private);
		registerField(_changeTrackerField, arg2: false);
		_entityMemberChanging = typeBuilder.DefineMethod("EntityMemberChanging", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(void), new Type[1] { typeof(string) });
		ILGenerator iLGenerator = _entityMemberChanging.GetILGenerator();
		Label label = iLGenerator.DefineLabel();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, _changeTrackerField);
		iLGenerator.Emit(OpCodes.Brfalse_S, label);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, _changeTrackerField);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Callvirt, EntityMemberChangingMethod);
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ret);
		_entityMemberChanged = typeBuilder.DefineMethod("EntityMemberChanged", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(void), new Type[1] { typeof(string) });
		ILGenerator iLGenerator2 = _entityMemberChanged.GetILGenerator();
		label = iLGenerator2.DefineLabel();
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ldfld, _changeTrackerField);
		iLGenerator2.Emit(OpCodes.Brfalse_S, label);
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ldfld, _changeTrackerField);
		iLGenerator2.Emit(OpCodes.Ldarg_1);
		iLGenerator2.Emit(OpCodes.Callvirt, EntityMemberChangedMethod);
		iLGenerator2.MarkLabel(label);
		iLGenerator2.Emit(OpCodes.Ret);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("IEntityWithChangeTracker.SetChangeTracker", MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask, typeof(void), new Type[1] { typeof(IEntityChangeTracker) });
		ILGenerator iLGenerator3 = methodBuilder.GetILGenerator();
		iLGenerator3.Emit(OpCodes.Ldarg_0);
		iLGenerator3.Emit(OpCodes.Ldarg_1);
		iLGenerator3.Emit(OpCodes.Stfld, _changeTrackerField);
		iLGenerator3.Emit(OpCodes.Ret);
		typeBuilder.DefineMethodOverride(methodBuilder, SetChangeTrackerMethod);
	}

	private void ImplementIEntityWithRelationships(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
	{
		_relationshipManagerField = typeBuilder.DefineField("_relationshipManager", typeof(RelationshipManager), FieldAttributes.Private);
		registerField(_relationshipManagerField, arg2: true);
		PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("RelationshipManager", PropertyAttributes.None, typeof(RelationshipManager), Type.EmptyTypes);
		_getRelationshipManager = typeBuilder.DefineMethod("IEntityWithRelationships.get_RelationshipManager", MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName, typeof(RelationshipManager), Type.EmptyTypes);
		ILGenerator iLGenerator = _getRelationshipManager.GetILGenerator();
		Label label = iLGenerator.DefineLabel();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, _relationshipManagerField);
		iLGenerator.Emit(OpCodes.Brtrue_S, label);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Call, CreateRelationshipManagerMethod);
		iLGenerator.Emit(OpCodes.Stfld, _relationshipManagerField);
		iLGenerator.MarkLabel(label);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ldfld, _relationshipManagerField);
		iLGenerator.Emit(OpCodes.Ret);
		propertyBuilder.SetGetMethod(_getRelationshipManager);
		typeBuilder.DefineMethodOverride(_getRelationshipManager, GetRelationshipManagerMethod);
	}
}
