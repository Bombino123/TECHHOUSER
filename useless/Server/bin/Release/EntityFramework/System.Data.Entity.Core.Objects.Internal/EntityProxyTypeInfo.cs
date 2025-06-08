using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class EntityProxyTypeInfo
{
	private readonly Type _proxyType;

	private readonly ClrEntityType _entityType;

	internal const string EntityWrapperFieldName = "_entityWrapper";

	private const string InitializeEntityCollectionsName = "InitializeEntityCollections";

	private readonly DynamicMethod _initializeCollections;

	private readonly Func<object, string, object> _baseGetter;

	private readonly HashSet<string> _propertiesWithBaseGetter;

	private readonly Action<object, string, object> _baseSetter;

	private readonly HashSet<string> _propertiesWithBaseSetter;

	private readonly Func<object, object> Proxy_GetEntityWrapper;

	private readonly Func<object, object, object> Proxy_SetEntityWrapper;

	private readonly Func<object> _createObject;

	private readonly Dictionary<string, AssociationType> _navigationPropertyAssociationTypes = new Dictionary<string, AssociationType>();

	internal Type ProxyType => _proxyType;

	internal DynamicMethod InitializeEntityCollections => _initializeCollections;

	public Func<object, string, object> BaseGetter => _baseGetter;

	public Action<object, string, object> BaseSetter => _baseSetter;

	internal Func<object, object> EntityWrapperDelegate => Proxy_GetEntityWrapper;

	internal EntityProxyTypeInfo(Type proxyType, ClrEntityType ospaceEntityType, DynamicMethod initializeCollections, List<PropertyInfo> baseGetters, List<PropertyInfo> baseSetters, MetadataWorkspace workspace)
	{
		_proxyType = proxyType;
		_entityType = ospaceEntityType;
		_initializeCollections = initializeCollections;
		foreach (AssociationType item in GetAllRelationshipsForType(workspace, proxyType))
		{
			_navigationPropertyAssociationTypes.Add(item.FullName, item);
			if (item.Name != item.FullName)
			{
				_navigationPropertyAssociationTypes.Add(item.Name, item);
			}
		}
		FieldInfo field = proxyType.GetField("_entityWrapper", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "proxy");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
		Expression<Func<object, object>> expression = Expression.Lambda<Func<object, object>>(Expression.Field(Expression.Convert(parameterExpression, field.DeclaringType), field), new ParameterExpression[1] { parameterExpression });
		Func<object, object> getEntityWrapperDelegate = expression.Compile();
		Proxy_GetEntityWrapper = delegate(object proxy)
		{
			IEntityWrapper entityWrapper = (IEntityWrapper)getEntityWrapperDelegate(proxy);
			if (entityWrapper != null && entityWrapper.Entity != proxy)
			{
				throw new InvalidOperationException(Strings.EntityProxyTypeInfo_ProxyHasWrongWrapper);
			}
			return entityWrapper;
		};
		Proxy_SetEntityWrapper = Expression.Lambda<Func<object, object, object>>(Expression.Assign(Expression.Field(Expression.Convert(parameterExpression, field.DeclaringType), field), parameterExpression2), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
		ParameterExpression parameterExpression3 = Expression.Parameter(typeof(string), "propertyName");
		MethodInfo publicInstanceMethod = proxyType.GetPublicInstanceMethod("GetBasePropertyValue", typeof(string));
		if (publicInstanceMethod != null)
		{
			_baseGetter = Expression.Lambda<Func<object, string, object>>(Expression.Call(Expression.Convert(parameterExpression, proxyType), publicInstanceMethod, parameterExpression3), new ParameterExpression[2] { parameterExpression, parameterExpression3 }).Compile();
		}
		ParameterExpression parameterExpression4 = Expression.Parameter(typeof(object), "propertyName");
		MethodInfo publicInstanceMethod2 = proxyType.GetPublicInstanceMethod("SetBasePropertyValue", typeof(string), typeof(object));
		if (publicInstanceMethod2 != null)
		{
			_baseSetter = Expression.Lambda<Action<object, string, object>>(Expression.Call(Expression.Convert(parameterExpression, proxyType), publicInstanceMethod2, parameterExpression3, parameterExpression4), new ParameterExpression[3] { parameterExpression, parameterExpression3, parameterExpression4 }).Compile();
		}
		_propertiesWithBaseGetter = new HashSet<string>(baseGetters.Select((PropertyInfo p) => p.Name));
		_propertiesWithBaseSetter = new HashSet<string>(baseSetters.Select((PropertyInfo p) => p.Name));
		_createObject = DelegateFactory.CreateConstructor(proxyType);
	}

	internal static IEnumerable<AssociationType> GetAllRelationshipsForType(MetadataWorkspace workspace, Type clrType)
	{
		return from a in ((ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace)).GetItems<AssociationType>()
			where IsEndMemberForType(a.AssociationEndMembers[0], clrType) || IsEndMemberForType(a.AssociationEndMembers[1], clrType)
			select a;
	}

	private static bool IsEndMemberForType(AssociationEndMember end, Type clrType)
	{
		if (end.TypeUsage.EdmType is RefType refType)
		{
			return refType.ElementType.ClrType.IsAssignableFrom(clrType);
		}
		return false;
	}

	internal object CreateProxyObject()
	{
		return _createObject();
	}

	public bool ContainsBaseGetter(string propertyName)
	{
		if (BaseGetter != null)
		{
			return _propertiesWithBaseGetter.Contains(propertyName);
		}
		return false;
	}

	public bool ContainsBaseSetter(string propertyName)
	{
		if (BaseSetter != null)
		{
			return _propertiesWithBaseSetter.Contains(propertyName);
		}
		return false;
	}

	public bool TryGetNavigationPropertyAssociationType(string relationshipName, out AssociationType associationType)
	{
		return _navigationPropertyAssociationTypes.TryGetValue(relationshipName, out associationType);
	}

	public IEnumerable<AssociationType> GetAllAssociationTypes()
	{
		return _navigationPropertyAssociationTypes.Values.Distinct();
	}

	public void ValidateType(ClrEntityType ospaceEntityType)
	{
		if (ospaceEntityType != _entityType && ospaceEntityType.HashedDescription != _entityType.HashedDescription)
		{
			throw new InvalidOperationException(Strings.EntityProxyTypeInfo_DuplicateOSpaceType(ospaceEntityType.ClrType.FullName));
		}
	}

	internal IEntityWrapper SetEntityWrapper(IEntityWrapper wrapper)
	{
		return Proxy_SetEntityWrapper(wrapper.Entity, wrapper) as IEntityWrapper;
	}

	internal IEntityWrapper GetEntityWrapper(object entity)
	{
		return Proxy_GetEntityWrapper(entity) as IEntityWrapper;
	}
}
