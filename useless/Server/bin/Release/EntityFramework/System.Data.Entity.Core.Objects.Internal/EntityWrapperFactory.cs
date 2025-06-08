using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.Internal;

internal class EntityWrapperFactory
{
	private static readonly Memoizer<Type, Func<object, IEntityWrapper>> _delegateCache = new Memoizer<Type, Func<object, IEntityWrapper>>(CreateWrapperDelegate, null);

	internal static readonly MethodInfo CreateWrapperDelegateTypedLightweightMethod = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("CreateWrapperDelegateTypedLightweight");

	internal static readonly MethodInfo CreateWrapperDelegateTypedWithRelationshipsMethod = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("CreateWrapperDelegateTypedWithRelationships");

	internal static readonly MethodInfo CreateWrapperDelegateTypedWithoutRelationshipsMethod = typeof(EntityWrapperFactory).GetOnlyDeclaredMethod("CreateWrapperDelegateTypedWithoutRelationships");

	internal static IEntityWrapper CreateNewWrapper(object entity, EntityKey key)
	{
		if (entity == null)
		{
			return NullEntityWrapper.NullWrapper;
		}
		IEntityWrapper entityWrapper = _delegateCache.Evaluate(entity.GetType())(entity);
		entityWrapper.RelationshipManager.SetWrappedOwner(entityWrapper, entity);
		if ((object)key != null && (object)entityWrapper.EntityKey == null)
		{
			entityWrapper.EntityKey = key;
		}
		if (EntityProxyFactory.TryGetProxyType(entity.GetType(), out var proxyTypeInfo))
		{
			proxyTypeInfo.SetEntityWrapper(entityWrapper);
		}
		return entityWrapper;
	}

	private static Func<object, IEntityWrapper> CreateWrapperDelegate(Type entityType)
	{
		bool flag = typeof(IEntityWithRelationships).IsAssignableFrom(entityType);
		bool flag2 = typeof(IEntityWithChangeTracker).IsAssignableFrom(entityType);
		bool flag3 = typeof(IEntityWithKey).IsAssignableFrom(entityType);
		bool flag4 = EntityProxyFactory.IsProxyType(entityType);
		MethodInfo methodInfo = ((flag && flag2 && flag3 && !flag4) ? CreateWrapperDelegateTypedLightweightMethod : ((!flag) ? CreateWrapperDelegateTypedWithoutRelationshipsMethod : CreateWrapperDelegateTypedWithRelationshipsMethod));
		methodInfo = methodInfo.MakeGenericMethod(entityType);
		return (Func<object, IEntityWrapper>)methodInfo.Invoke(null, new object[0]);
	}

	private static Func<object, IEntityWrapper> CreateWrapperDelegateTypedLightweight<TEntity>() where TEntity : class, IEntityWithRelationships, IEntityWithKey, IEntityWithChangeTracker
	{
		bool overridesEquals = typeof(TEntity).OverridesEqualsOrGetHashCode();
		return (object entity) => new LightweightEntityWrapper<TEntity>((TEntity)entity, overridesEquals);
	}

	private static Func<object, IEntityWrapper> CreateWrapperDelegateTypedWithRelationships<TEntity>() where TEntity : class, IEntityWithRelationships
	{
		bool overridesEquals = typeof(TEntity).OverridesEqualsOrGetHashCode();
		CreateStrategies<TEntity>(out var propertyAccessorStrategy, out var changeTrackingStrategy, out var keyStrategy);
		return (object entity) => new EntityWrapperWithRelationships<TEntity>((TEntity)entity, propertyAccessorStrategy, changeTrackingStrategy, keyStrategy, overridesEquals);
	}

	private static Func<object, IEntityWrapper> CreateWrapperDelegateTypedWithoutRelationships<TEntity>() where TEntity : class
	{
		bool overridesEquals = typeof(TEntity).OverridesEqualsOrGetHashCode();
		CreateStrategies<TEntity>(out var propertyAccessorStrategy, out var changeTrackingStrategy, out var keyStrategy);
		return (object entity) => new EntityWrapperWithoutRelationships<TEntity>((TEntity)entity, propertyAccessorStrategy, changeTrackingStrategy, keyStrategy, overridesEquals);
	}

	private static void CreateStrategies<TEntity>(out Func<object, IPropertyAccessorStrategy> createPropertyAccessorStrategy, out Func<object, IChangeTrackingStrategy> createChangeTrackingStrategy, out Func<object, IEntityKeyStrategy> createKeyStrategy)
	{
		Type typeFromHandle = typeof(TEntity);
		bool num = typeof(IEntityWithRelationships).IsAssignableFrom(typeFromHandle);
		bool flag = typeof(IEntityWithChangeTracker).IsAssignableFrom(typeFromHandle);
		bool flag2 = typeof(IEntityWithKey).IsAssignableFrom(typeFromHandle);
		bool flag3 = EntityProxyFactory.IsProxyType(typeFromHandle);
		if (!num || flag3)
		{
			createPropertyAccessorStrategy = GetPocoPropertyAccessorStrategyFunc();
		}
		else
		{
			createPropertyAccessorStrategy = GetNullPropertyAccessorStrategyFunc();
		}
		if (flag)
		{
			createChangeTrackingStrategy = GetEntityWithChangeTrackerStrategyFunc();
		}
		else
		{
			createChangeTrackingStrategy = GetSnapshotChangeTrackingStrategyFunc();
		}
		if (flag2)
		{
			createKeyStrategy = GetEntityWithKeyStrategyStrategyFunc();
		}
		else
		{
			createKeyStrategy = GetPocoEntityKeyStrategyFunc();
		}
	}

	internal IEntityWrapper WrapEntityUsingContext(object entity, ObjectContext context)
	{
		EntityEntry existingEntry;
		return WrapEntityUsingStateManagerGettingEntry(entity, context?.ObjectStateManager, out existingEntry);
	}

	internal IEntityWrapper WrapEntityUsingContextGettingEntry(object entity, ObjectContext context, out EntityEntry existingEntry)
	{
		return WrapEntityUsingStateManagerGettingEntry(entity, context?.ObjectStateManager, out existingEntry);
	}

	internal IEntityWrapper WrapEntityUsingStateManager(object entity, ObjectStateManager stateManager)
	{
		EntityEntry existingEntry;
		return WrapEntityUsingStateManagerGettingEntry(entity, stateManager, out existingEntry);
	}

	internal virtual IEntityWrapper WrapEntityUsingStateManagerGettingEntry(object entity, ObjectStateManager stateManager, out EntityEntry existingEntry)
	{
		IEntityWrapper value = null;
		existingEntry = null;
		if (entity == null)
		{
			return NullEntityWrapper.NullWrapper;
		}
		if (stateManager != null)
		{
			existingEntry = stateManager.FindEntityEntry(entity);
			if (existingEntry != null)
			{
				return existingEntry.WrappedEntity;
			}
			if (stateManager.TransactionManager.TrackProcessedEntities && stateManager.TransactionManager.WrappedEntities.TryGetValue(entity, out value))
			{
				return value;
			}
		}
		if (entity is IEntityWithRelationships entityWithRelationships)
		{
			IEntityWrapper wrappedOwner = (entityWithRelationships.RelationshipManager ?? throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull)).WrappedOwner;
			if (wrappedOwner.Entity != entity)
			{
				throw new InvalidOperationException(Strings.RelationshipManager_InvalidRelationshipManagerOwner);
			}
			return wrappedOwner;
		}
		EntityProxyFactory.TryGetProxyWrapper(entity, out value);
		if (value == null)
		{
			value = CreateNewWrapper(entity, (entity as IEntityWithKey)?.EntityKey);
		}
		if (stateManager != null && stateManager.TransactionManager.TrackProcessedEntities)
		{
			stateManager.TransactionManager.WrappedEntities.Add(entity, value);
		}
		return value;
	}

	internal virtual void UpdateNoTrackingWrapper(IEntityWrapper wrapper, ObjectContext context, EntitySet entitySet)
	{
		if (wrapper.EntityKey == null)
		{
			wrapper.EntityKey = context.ObjectStateManager.CreateEntityKey(entitySet, wrapper.Entity);
		}
		if (wrapper.Context == null)
		{
			wrapper.AttachContext(context, entitySet, MergeOption.NoTracking);
		}
	}

	internal static Func<object, IPropertyAccessorStrategy> GetPocoPropertyAccessorStrategyFunc()
	{
		return (object entity) => new PocoPropertyAccessorStrategy(entity);
	}

	internal static Func<object, IPropertyAccessorStrategy> GetNullPropertyAccessorStrategyFunc()
	{
		return (object entity) => (IPropertyAccessorStrategy)null;
	}

	internal static Func<object, IChangeTrackingStrategy> GetEntityWithChangeTrackerStrategyFunc()
	{
		return (object entity) => new EntityWithChangeTrackerStrategy((IEntityWithChangeTracker)entity);
	}

	internal static Func<object, IChangeTrackingStrategy> GetSnapshotChangeTrackingStrategyFunc()
	{
		return (object entity) => SnapshotChangeTrackingStrategy.Instance;
	}

	internal static Func<object, IEntityKeyStrategy> GetEntityWithKeyStrategyStrategyFunc()
	{
		return (object entity) => new EntityWithKeyStrategy((IEntityWithKey)entity);
	}

	internal static Func<object, IEntityKeyStrategy> GetPocoEntityKeyStrategyFunc()
	{
		return (object entity) => new PocoEntityKeyStrategy();
	}
}
