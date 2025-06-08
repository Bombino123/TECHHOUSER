using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace System.Data.Entity.Core.Objects.Internal;

internal class EntityProxyFactory
{
	internal class ProxyTypeBuilder
	{
		private TypeBuilder _typeBuilder;

		private readonly BaseProxyImplementor _baseImplementor;

		private readonly IPocoImplementor _ipocoImplementor;

		private readonly LazyLoadImplementor _lazyLoadImplementor;

		private readonly DataContractImplementor _dataContractImplementor;

		private readonly SerializableImplementor _iserializableImplementor;

		private readonly ClrEntityType _ospaceEntityType;

		private ModuleBuilder _moduleBuilder;

		private readonly List<FieldBuilder> _serializedFields = new List<FieldBuilder>(3);

		private static readonly ConstructorInfo _nonSerializedAttributeConstructor = typeof(NonSerializedAttribute).GetDeclaredConstructor();

		private static readonly ConstructorInfo _ignoreDataMemberAttributeConstructor = typeof(IgnoreDataMemberAttribute).GetDeclaredConstructor();

		private static readonly ConstructorInfo _xmlIgnoreAttributeConstructor = typeof(XmlIgnoreAttribute).GetDeclaredConstructor();

		private static readonly Lazy<ConstructorInfo> _scriptIgnoreAttributeConstructor = new Lazy<ConstructorInfo>(TryGetScriptIgnoreAttributeConstructor);

		public Type BaseType => _ospaceEntityType.ClrType;

		public List<PropertyInfo> BaseGetters => _baseImplementor.BaseGetters;

		public List<PropertyInfo> BaseSetters => _baseImplementor.BaseSetters;

		public IEnumerable<EdmMember> LazyLoadMembers => _lazyLoadImplementor.Members;

		private TypeBuilder TypeBuilder
		{
			get
			{
				if (_typeBuilder == null)
				{
					TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
					if ((BaseType.Attributes() & TypeAttributes.Serializable) == TypeAttributes.Serializable)
					{
						typeAttributes |= TypeAttributes.Serializable;
					}
					string text = ((BaseType.Name.Length <= 20) ? BaseType.Name : BaseType.Name.Substring(0, 20));
					string name = string.Format(CultureInfo.InvariantCulture, "System.Data.Entity.DynamicProxies.{0}_{1}", new object[2] { text, _ospaceEntityType.HashedDescription });
					_typeBuilder = _moduleBuilder.DefineType(name, typeAttributes, BaseType, _ipocoImplementor.Interfaces);
					_typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
					Action<FieldBuilder, bool> registerField = RegisterInstanceField;
					_ipocoImplementor.Implement(_typeBuilder, registerField);
					_lazyLoadImplementor.Implement(_typeBuilder, registerField);
					if (!_iserializableImplementor.TypeImplementsISerializable)
					{
						_dataContractImplementor.Implement(_typeBuilder);
					}
				}
				return _typeBuilder;
			}
		}

		public ProxyTypeBuilder(ClrEntityType ospaceEntityType)
		{
			_ospaceEntityType = ospaceEntityType;
			_baseImplementor = new BaseProxyImplementor();
			_ipocoImplementor = new IPocoImplementor(ospaceEntityType);
			_lazyLoadImplementor = new LazyLoadImplementor(ospaceEntityType);
			_dataContractImplementor = new DataContractImplementor(ospaceEntityType);
			_iserializableImplementor = new SerializableImplementor(ospaceEntityType);
		}

		public DynamicMethod CreateInitializeCollectionMethod(Type proxyType)
		{
			return _ipocoImplementor.CreateInitializeCollectionMethod(proxyType);
		}

		public Type CreateType(ModuleBuilder moduleBuilder)
		{
			_moduleBuilder = moduleBuilder;
			bool flag = false;
			if (_iserializableImplementor.TypeIsSuitable)
			{
				foreach (EdmMember member in _ospaceEntityType.Members)
				{
					if (_ipocoImplementor.CanProxyMember(member) || _lazyLoadImplementor.CanProxyMember(member))
					{
						PropertyInfo topProperty = BaseType.GetTopProperty(member.Name);
						PropertyBuilder propertyBuilder = TypeBuilder.DefineProperty(member.Name, PropertyAttributes.None, topProperty.PropertyType, Type.EmptyTypes);
						if (!_ipocoImplementor.EmitMember(TypeBuilder, member, propertyBuilder, topProperty, _baseImplementor))
						{
							EmitBaseSetter(TypeBuilder, propertyBuilder, topProperty);
						}
						if (!_lazyLoadImplementor.EmitMember(TypeBuilder, member, propertyBuilder, topProperty, _baseImplementor))
						{
							EmitBaseGetter(TypeBuilder, propertyBuilder, topProperty);
						}
						flag = true;
					}
				}
				if (_typeBuilder != null)
				{
					_baseImplementor.Implement(TypeBuilder);
					_iserializableImplementor.Implement(TypeBuilder, _serializedFields);
				}
			}
			if (!flag)
			{
				return null;
			}
			return TypeBuilder.CreateType();
		}

		private static void EmitBaseGetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty)
		{
			if (CanProxyGetter(baseProperty))
			{
				MethodInfo methodInfo = baseProperty.Getter();
				MethodAttributes methodAttributes = methodInfo.Attributes & MethodAttributes.MemberAccessMask;
				MethodBuilder methodBuilder = typeBuilder.DefineMethod("get_" + baseProperty.Name, methodAttributes | (MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName), baseProperty.PropertyType, Type.EmptyTypes);
				ILGenerator iLGenerator = methodBuilder.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Call, methodInfo);
				iLGenerator.Emit(OpCodes.Ret);
				propertyBuilder.SetGetMethod(methodBuilder);
			}
		}

		private static void EmitBaseSetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty)
		{
			if (CanProxySetter(baseProperty))
			{
				MethodInfo methodInfo = baseProperty.Setter();
				MethodAttributes methodAttributes = methodInfo.Attributes & MethodAttributes.MemberAccessMask;
				MethodBuilder methodBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAttributes | (MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName), null, new Type[1] { baseProperty.PropertyType });
				ILGenerator iLGenerator = methodBuilder.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Call, methodInfo);
				iLGenerator.Emit(OpCodes.Ret);
				propertyBuilder.SetSetMethod(methodBuilder);
			}
		}

		private void RegisterInstanceField(FieldBuilder field, bool serializable)
		{
			if (serializable)
			{
				_serializedFields.Add(field);
			}
			else
			{
				MarkAsNotSerializable(field);
			}
		}

		private static ConstructorInfo TryGetScriptIgnoreAttributeConstructor()
		{
			try
			{
				if (AspProxy.IsSystemWebLoaded())
				{
					Type type = Assembly.Load("System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").GetType("System.Web.Script.Serialization.ScriptIgnoreAttribute");
					if (type != null)
					{
						return type.GetDeclaredConstructor();
					}
				}
			}
			catch
			{
			}
			return null;
		}

		public static void MarkAsNotSerializable(FieldBuilder field)
		{
			object[] constructorArgs = new object[0];
			field.SetCustomAttribute(new CustomAttributeBuilder(_nonSerializedAttributeConstructor, constructorArgs));
			if (field.IsPublic)
			{
				field.SetCustomAttribute(new CustomAttributeBuilder(_ignoreDataMemberAttributeConstructor, constructorArgs));
				field.SetCustomAttribute(new CustomAttributeBuilder(_xmlIgnoreAttributeConstructor, constructorArgs));
				if (_scriptIgnoreAttributeConstructor.Value != null)
				{
					field.SetCustomAttribute(new CustomAttributeBuilder(_scriptIgnoreAttributeConstructor.Value, constructorArgs));
				}
			}
		}
	}

	internal const string ResetFKSetterFlagFieldName = "_resetFKSetterFlag";

	internal const string CompareByteArraysFieldName = "_compareByteArrays";

	private static readonly Dictionary<Tuple<Type, string>, EntityProxyTypeInfo> _proxyNameMap = new Dictionary<Tuple<Type, string>, EntityProxyTypeInfo>();

	private static readonly Dictionary<Type, EntityProxyTypeInfo> _proxyTypeMap = new Dictionary<Type, EntityProxyTypeInfo>();

	private static readonly Dictionary<Assembly, ModuleBuilder> _moduleBuilders = new Dictionary<Assembly, ModuleBuilder>();

	private static readonly ReaderWriterLockSlim _typeMapLock = new ReaderWriterLockSlim();

	private static readonly HashSet<Assembly> _proxyRuntimeAssemblies = new HashSet<Assembly>();

	internal static readonly MethodInfo GetInterceptorDelegateMethod = typeof(LazyLoadBehavior).GetOnlyDeclaredMethod("GetInterceptorDelegate");

	private static ModuleBuilder GetDynamicModule(EntityType ospaceEntityType)
	{
		Assembly assembly = ospaceEntityType.ClrType.Assembly();
		if (!_moduleBuilders.TryGetValue(assembly, out var value))
		{
			value = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(string.Format(CultureInfo.InvariantCulture, "EntityFrameworkDynamicProxies-{0}", new object[1] { assembly.FullName }))
			{
				Version = new Version(1, 0, 0, 0)
			}, AssemblyBuilderAccess.Run).DefineDynamicModule("EntityProxyModule");
			_moduleBuilders.Add(assembly, value);
		}
		return value;
	}

	private static void DiscardDynamicModule(EntityType ospaceEntityType)
	{
		_moduleBuilders.Remove(ospaceEntityType.ClrType.Assembly());
	}

	internal static bool TryGetProxyType(Type clrType, string entityTypeName, out EntityProxyTypeInfo proxyTypeInfo)
	{
		_typeMapLock.EnterReadLock();
		try
		{
			return _proxyNameMap.TryGetValue(new Tuple<Type, string>(clrType, entityTypeName), out proxyTypeInfo);
		}
		finally
		{
			_typeMapLock.ExitReadLock();
		}
	}

	internal static bool TryGetProxyType(Type proxyType, out EntityProxyTypeInfo proxyTypeInfo)
	{
		_typeMapLock.EnterReadLock();
		try
		{
			return _proxyTypeMap.TryGetValue(proxyType, out proxyTypeInfo);
		}
		finally
		{
			_typeMapLock.ExitReadLock();
		}
	}

	internal static bool TryGetProxyWrapper(object instance, out IEntityWrapper wrapper)
	{
		wrapper = null;
		if (IsProxyType(instance.GetType()) && TryGetProxyType(instance.GetType(), out var proxyTypeInfo))
		{
			wrapper = proxyTypeInfo.GetEntityWrapper(instance);
		}
		return wrapper != null;
	}

	internal static EntityProxyTypeInfo GetProxyType(ClrEntityType ospaceEntityType, MetadataWorkspace workspace)
	{
		EntityProxyTypeInfo proxyTypeInfo = null;
		if (TryGetProxyType(ospaceEntityType.ClrType, ospaceEntityType.CSpaceTypeName, out proxyTypeInfo))
		{
			proxyTypeInfo?.ValidateType(ospaceEntityType);
			return proxyTypeInfo;
		}
		_typeMapLock.EnterUpgradeableReadLock();
		try
		{
			return TryCreateProxyType(ospaceEntityType, workspace);
		}
		finally
		{
			_typeMapLock.ExitUpgradeableReadLock();
		}
	}

	internal static bool TryGetAssociationTypeFromProxyInfo(IEntityWrapper wrappedEntity, string relationshipName, out AssociationType associationType)
	{
		associationType = null;
		if (TryGetProxyType(wrappedEntity.Entity.GetType(), out var proxyTypeInfo) && proxyTypeInfo != null)
		{
			return proxyTypeInfo.TryGetNavigationPropertyAssociationType(relationshipName, out associationType);
		}
		return false;
	}

	internal static IEnumerable<AssociationType> TryGetAllAssociationTypesFromProxyInfo(IEntityWrapper wrappedEntity)
	{
		if (!TryGetProxyType(wrappedEntity.Entity.GetType(), out var proxyTypeInfo))
		{
			return null;
		}
		return proxyTypeInfo.GetAllAssociationTypes();
	}

	internal static void TryCreateProxyTypes(IEnumerable<EntityType> ospaceEntityTypes, MetadataWorkspace workspace)
	{
		_typeMapLock.EnterUpgradeableReadLock();
		try
		{
			foreach (EntityType ospaceEntityType in ospaceEntityTypes)
			{
				TryCreateProxyType(ospaceEntityType, workspace);
			}
		}
		finally
		{
			_typeMapLock.ExitUpgradeableReadLock();
		}
	}

	private static EntityProxyTypeInfo TryCreateProxyType(EntityType ospaceEntityType, MetadataWorkspace workspace)
	{
		ClrEntityType clrEntityType = (ClrEntityType)ospaceEntityType;
		Tuple<Type, string> key = new Tuple<Type, string>(clrEntityType.ClrType, clrEntityType.HashedDescription);
		if (!_proxyNameMap.TryGetValue(key, out var value) && CanProxyType(ospaceEntityType))
		{
			try
			{
				value = BuildType(GetDynamicModule(ospaceEntityType), clrEntityType, workspace);
				_typeMapLock.EnterWriteLock();
				try
				{
					_proxyNameMap[key] = value;
					if (value != null)
					{
						_proxyTypeMap[value.ProxyType] = value;
					}
				}
				finally
				{
					_typeMapLock.ExitWriteLock();
				}
			}
			catch
			{
				DiscardDynamicModule(ospaceEntityType);
				throw;
			}
		}
		return value;
	}

	internal static bool IsProxyType(Type type)
	{
		if (type != null)
		{
			return _proxyRuntimeAssemblies.Contains(type.Assembly());
		}
		return false;
	}

	internal static IEnumerable<Type> GetKnownProxyTypes()
	{
		_typeMapLock.EnterReadLock();
		try
		{
			return (from info in _proxyNameMap.Values
				where info != null
				select info.ProxyType).ToArray();
		}
		finally
		{
			_typeMapLock.ExitReadLock();
		}
	}

	public virtual Func<object, object> CreateBaseGetter(Type declaringType, PropertyInfo propertyInfo)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
		Func<object, object> nonProxyGetter = Expression.Lambda<Func<object, object>>(Expression.Property(Expression.Convert(parameterExpression, declaringType), propertyInfo), new ParameterExpression[1] { parameterExpression }).Compile();
		string propertyName = propertyInfo.Name;
		return delegate(object entity)
		{
			Type type = entity.GetType();
			object value;
			return (IsProxyType(type) && TryGetBasePropertyValue(type, propertyName, entity, out value)) ? value : nonProxyGetter(entity);
		};
	}

	private static bool TryGetBasePropertyValue(Type proxyType, string propertyName, object entity, out object value)
	{
		value = null;
		if (TryGetProxyType(proxyType, out var proxyTypeInfo) && proxyTypeInfo.ContainsBaseGetter(propertyName))
		{
			value = proxyTypeInfo.BaseGetter(entity, propertyName);
			return true;
		}
		return false;
	}

	public virtual Action<object, object> CreateBaseSetter(Type declaringType, PropertyInfo propertyInfo)
	{
		Action<object, object> nonProxySetter = DelegateFactory.CreateNavigationPropertySetter(declaringType, propertyInfo);
		string propertyName = propertyInfo.Name;
		return delegate(object entity, object value)
		{
			Type type = entity.GetType();
			if (!IsProxyType(type) || !TrySetBasePropertyValue(type, propertyName, entity, value))
			{
				nonProxySetter(entity, value);
			}
		};
	}

	private static bool TrySetBasePropertyValue(Type proxyType, string propertyName, object entity, object value)
	{
		if (TryGetProxyType(proxyType, out var proxyTypeInfo) && proxyTypeInfo.ContainsBaseSetter(propertyName))
		{
			proxyTypeInfo.BaseSetter(entity, propertyName, value);
			return true;
		}
		return false;
	}

	private static EntityProxyTypeInfo BuildType(ModuleBuilder moduleBuilder, ClrEntityType ospaceEntityType, MetadataWorkspace workspace)
	{
		ProxyTypeBuilder proxyTypeBuilder = new ProxyTypeBuilder(ospaceEntityType);
		Type type = proxyTypeBuilder.CreateType(moduleBuilder);
		EntityProxyTypeInfo entityProxyTypeInfo;
		if (type != null)
		{
			Assembly assembly = type.Assembly();
			if (!_proxyRuntimeAssemblies.Contains(assembly))
			{
				_proxyRuntimeAssemblies.Add(assembly);
				AddAssemblyToResolveList(assembly);
			}
			entityProxyTypeInfo = new EntityProxyTypeInfo(type, ospaceEntityType, proxyTypeBuilder.CreateInitializeCollectionMethod(type), proxyTypeBuilder.BaseGetters, proxyTypeBuilder.BaseSetters, workspace);
			foreach (EdmMember lazyLoadMember in proxyTypeBuilder.LazyLoadMembers)
			{
				InterceptMember(lazyLoadMember, type, entityProxyTypeInfo);
			}
			SetResetFKSetterFlagDelegate(type, entityProxyTypeInfo);
			SetCompareByteArraysDelegate(type);
		}
		else
		{
			entityProxyTypeInfo = null;
		}
		return entityProxyTypeInfo;
	}

	private static void AddAssemblyToResolveList(Assembly assembly)
	{
		try
		{
			AppDomain.CurrentDomain.AssemblyResolve += (object _, ResolveEventArgs args) => (!(args.Name == assembly.FullName)) ? null : assembly;
		}
		catch (MethodAccessException)
		{
		}
	}

	private static void InterceptMember(EdmMember member, Type proxyType, EntityProxyTypeInfo proxyTypeInfo)
	{
		PropertyInfo topProperty = proxyType.GetTopProperty(member.Name);
		FieldInfo field = proxyType.GetField(LazyLoadImplementor.GetInterceptorFieldName(member.Name), BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
		AssignInterceptionDelegate(GetInterceptorDelegateMethod.MakeGenericMethod(proxyType, topProperty.PropertyType).Invoke(null, new object[2] { member, proxyTypeInfo.EntityWrapperDelegate }) as Delegate, field);
	}

	private static void AssignInterceptionDelegate(Delegate interceptorDelegate, FieldInfo interceptorField)
	{
		interceptorField.SetValue(null, interceptorDelegate);
	}

	private static void SetResetFKSetterFlagDelegate(Type proxyType, EntityProxyTypeInfo proxyTypeInfo)
	{
		FieldInfo field = proxyType.GetField("_resetFKSetterFlag", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
		AssignInterceptionDelegate(GetResetFKSetterFlagDelegate(proxyTypeInfo.EntityWrapperDelegate), field);
	}

	private static Action<object> GetResetFKSetterFlagDelegate(Func<object, object> getEntityWrapperDelegate)
	{
		return delegate(object proxy)
		{
			ResetFKSetterFlag(getEntityWrapperDelegate(proxy));
		};
	}

	private static void ResetFKSetterFlag(object wrappedEntityAsObject)
	{
		IEntityWrapper entityWrapper = (IEntityWrapper)wrappedEntityAsObject;
		if (entityWrapper != null && entityWrapper.Context != null)
		{
			entityWrapper.Context.ObjectStateManager.EntityInvokingFKSetter = null;
		}
	}

	private static void SetCompareByteArraysDelegate(Type proxyType)
	{
		FieldInfo field = proxyType.GetField("_compareByteArrays", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
		AssignInterceptionDelegate(new Func<object, object, bool>(ByValueEqualityComparer.Default.Equals), field);
	}

	private static bool CanProxyType(EntityType ospaceEntityType)
	{
		Type clrType = ospaceEntityType.ClrType;
		if (!clrType.IsPublic() || clrType.IsSealed() || typeof(IEntityWithRelationships).IsAssignableFrom(clrType) || ospaceEntityType.Abstract)
		{
			return false;
		}
		ConstructorInfo declaredConstructor = clrType.GetDeclaredConstructor();
		if (declaredConstructor != null)
		{
			if ((declaredConstructor.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public && (declaredConstructor.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Family)
			{
				return (declaredConstructor.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
			}
			return true;
		}
		return false;
	}

	private static bool CanProxyMethod(MethodInfo method)
	{
		bool result = false;
		if (method != null)
		{
			MethodAttributes methodAttributes = method.Attributes & MethodAttributes.MemberAccessMask;
			result = method.IsVirtual && !method.IsFinal && (methodAttributes == MethodAttributes.Public || methodAttributes == MethodAttributes.Family || methodAttributes == MethodAttributes.FamORAssem);
		}
		return result;
	}

	internal static bool CanProxyGetter(PropertyInfo clrProperty)
	{
		return CanProxyMethod(clrProperty.Getter());
	}

	internal static bool CanProxySetter(PropertyInfo clrProperty)
	{
		return CanProxyMethod(clrProperty.Setter());
	}
}
