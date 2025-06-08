using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class PocoPropertyAccessorStrategy : IPropertyAccessorStrategy
{
	internal static readonly MethodInfo AddToCollectionGeneric = typeof(PocoPropertyAccessorStrategy).GetOnlyDeclaredMethod("AddToCollection");

	internal static readonly MethodInfo RemoveFromCollectionGeneric = typeof(PocoPropertyAccessorStrategy).GetOnlyDeclaredMethod("RemoveFromCollection");

	private readonly object _entity;

	public PocoPropertyAccessorStrategy(object entity)
	{
		_entity = entity;
	}

	public object GetNavigationPropertyValue(RelatedEnd relatedEnd)
	{
		object result = null;
		if (relatedEnd != null)
		{
			if (relatedEnd.TargetAccessor.ValueGetter == null)
			{
				Type declaringType = GetDeclaringType(relatedEnd);
				PropertyInfo topProperty = declaringType.GetTopProperty(relatedEnd.TargetAccessor.PropertyName);
				if (topProperty == null)
				{
					throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, declaringType.FullName));
				}
				EntityProxyFactory entityProxyFactory = new EntityProxyFactory();
				relatedEnd.TargetAccessor.ValueGetter = entityProxyFactory.CreateBaseGetter(topProperty.DeclaringType, topProperty);
			}
			bool state = relatedEnd.DisableLazyLoading();
			try
			{
				result = relatedEnd.TargetAccessor.ValueGetter(_entity);
			}
			catch (Exception innerException)
			{
				throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, _entity.GetType().FullName), innerException);
			}
			finally
			{
				relatedEnd.ResetLazyLoading(state);
			}
		}
		return result;
	}

	public void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
	{
		if (relatedEnd == null)
		{
			return;
		}
		if (relatedEnd.TargetAccessor.ValueSetter == null)
		{
			Type declaringType = GetDeclaringType(relatedEnd);
			PropertyInfo topProperty = declaringType.GetTopProperty(relatedEnd.TargetAccessor.PropertyName);
			if (topProperty == null)
			{
				throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, declaringType.FullName));
			}
			EntityProxyFactory entityProxyFactory = new EntityProxyFactory();
			relatedEnd.TargetAccessor.ValueSetter = entityProxyFactory.CreateBaseSetter(topProperty.DeclaringType, topProperty);
		}
		try
		{
			relatedEnd.TargetAccessor.ValueSetter(_entity, value);
		}
		catch (Exception innerException)
		{
			throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, _entity.GetType().FullName), innerException);
		}
	}

	private static Type GetDeclaringType(RelatedEnd relatedEnd)
	{
		if (relatedEnd.NavigationProperty != null)
		{
			return Util.GetObjectMapping((EntityType)relatedEnd.NavigationProperty.DeclaringType, relatedEnd.WrappedOwner.Context.MetadataWorkspace).ClrType.ClrType;
		}
		return relatedEnd.WrappedOwner.IdentityType;
	}

	private static Type GetNavigationPropertyType(Type entityType, string propertyName)
	{
		PropertyInfo topProperty = entityType.GetTopProperty(propertyName);
		if (topProperty != null)
		{
			return topProperty.PropertyType;
		}
		FieldInfo field = entityType.GetField(propertyName);
		if (field != null)
		{
			return field.FieldType;
		}
		throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(propertyName, entityType.FullName));
	}

	public void CollectionAdd(RelatedEnd relatedEnd, object value)
	{
		object entity = _entity;
		try
		{
			object obj = GetNavigationPropertyValue(relatedEnd);
			if (obj == null)
			{
				obj = CollectionCreate(relatedEnd);
				SetNavigationPropertyValue(relatedEnd, obj);
			}
			if (obj != relatedEnd)
			{
				if (relatedEnd.TargetAccessor.CollectionAdd == null)
				{
					relatedEnd.TargetAccessor.CollectionAdd = CreateCollectionAddFunction(GetDeclaringType(relatedEnd), relatedEnd.TargetAccessor.PropertyName);
				}
				relatedEnd.TargetAccessor.CollectionAdd(obj, value);
			}
		}
		catch (Exception innerException)
		{
			throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, entity.GetType().FullName), innerException);
		}
	}

	private static Action<object, object> CreateCollectionAddFunction(Type type, string propertyName)
	{
		Type collectionElementType = EntityUtil.GetCollectionElementType(GetNavigationPropertyType(type, propertyName));
		return (Action<object, object>)AddToCollectionGeneric.MakeGenericMethod(collectionElementType).Invoke(null, null);
	}

	private static Action<object, object> AddToCollection<T>()
	{
		return delegate(object collectionArg, object item)
		{
			ICollection<T> obj = (ICollection<T>)collectionArg;
			if (obj is Array { IsFixedSize: not false } array)
			{
				throw new InvalidOperationException(Strings.RelatedEnd_CannotAddToFixedSizeArray(array.GetType()));
			}
			obj.Add((T)item);
		};
	}

	public bool CollectionRemove(RelatedEnd relatedEnd, object value)
	{
		object entity = _entity;
		try
		{
			object navigationPropertyValue = GetNavigationPropertyValue(relatedEnd);
			if (navigationPropertyValue != null)
			{
				if (navigationPropertyValue == relatedEnd)
				{
					return true;
				}
				if (relatedEnd.TargetAccessor.CollectionRemove == null)
				{
					relatedEnd.TargetAccessor.CollectionRemove = CreateCollectionRemoveFunction(GetDeclaringType(relatedEnd), relatedEnd.TargetAccessor.PropertyName);
				}
				return relatedEnd.TargetAccessor.CollectionRemove(navigationPropertyValue, value);
			}
		}
		catch (Exception innerException)
		{
			throw new EntityException(Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, entity.GetType().FullName), innerException);
		}
		return false;
	}

	private static Func<object, object, bool> CreateCollectionRemoveFunction(Type type, string propertyName)
	{
		Type collectionElementType = EntityUtil.GetCollectionElementType(GetNavigationPropertyType(type, propertyName));
		return (Func<object, object, bool>)RemoveFromCollectionGeneric.MakeGenericMethod(collectionElementType).Invoke(null, null);
	}

	private static Func<object, object, bool> RemoveFromCollection<T>()
	{
		return delegate(object collectionArg, object item)
		{
			ICollection<T> obj = (ICollection<T>)collectionArg;
			if (obj is Array { IsFixedSize: not false } array)
			{
				throw new InvalidOperationException(Strings.RelatedEnd_CannotRemoveFromFixedSizeArray(array.GetType()));
			}
			return obj.Remove((T)item);
		};
	}

	public object CollectionCreate(RelatedEnd relatedEnd)
	{
		if (_entity is IEntityWithRelationships)
		{
			return relatedEnd;
		}
		if (relatedEnd.TargetAccessor.CollectionCreate == null)
		{
			Type declaringType = GetDeclaringType(relatedEnd);
			string propertyName = relatedEnd.TargetAccessor.PropertyName;
			Type navigationPropertyType = GetNavigationPropertyType(declaringType, propertyName);
			relatedEnd.TargetAccessor.CollectionCreate = CreateCollectionCreateDelegate(navigationPropertyType, propertyName);
		}
		return relatedEnd.TargetAccessor.CollectionCreate();
	}

	private static Func<object> CreateCollectionCreateDelegate(Type navigationPropertyType, string propName)
	{
		Type type = EntityUtil.DetermineCollectionType(navigationPropertyType);
		if (type == null)
		{
			throw new EntityException(Strings.PocoEntityWrapper_UnableToMaterializeArbitaryNavPropType(propName, navigationPropertyType));
		}
		return Expression.Lambda<Func<object>>(DelegateFactory.GetNewExpressionForCollectionType(type), new ParameterExpression[0]).Compile();
	}
}
