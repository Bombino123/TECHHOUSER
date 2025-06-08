using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects.Internal;

internal abstract class BaseEntityWrapper<TEntity> : IEntityWrapper where TEntity : class
{
	[Flags]
	private enum WrapperFlags
	{
		None = 0,
		NoTracking = 1,
		InitializingRelatedEnds = 2,
		OverridesEquals = 4
	}

	private readonly RelationshipManager _relationshipManager;

	private Type _identityType;

	private WrapperFlags _flags;

	public RelationshipManager RelationshipManager => _relationshipManager;

	public ObjectContext Context { get; set; }

	public MergeOption MergeOption
	{
		get
		{
			if ((_flags & WrapperFlags.NoTracking) == 0)
			{
				return MergeOption.AppendOnly;
			}
			return MergeOption.NoTracking;
		}
		private set
		{
			if (value == MergeOption.NoTracking)
			{
				_flags |= WrapperFlags.NoTracking;
			}
			else
			{
				_flags &= ~WrapperFlags.NoTracking;
			}
		}
	}

	public bool InitializingProxyRelatedEnds
	{
		get
		{
			return (_flags & WrapperFlags.InitializingRelatedEnds) != 0;
		}
		set
		{
			if (value)
			{
				_flags |= WrapperFlags.InitializingRelatedEnds;
			}
			else
			{
				_flags &= ~WrapperFlags.InitializingRelatedEnds;
			}
		}
	}

	public EntityEntry ObjectStateEntry { get; set; }

	public Type IdentityType
	{
		get
		{
			if (_identityType == null)
			{
				_identityType = EntityUtil.GetEntityIdentityType(typeof(TEntity));
			}
			return _identityType;
		}
	}

	public bool OverridesEqualsOrGetHashCode => (_flags & WrapperFlags.OverridesEquals) != 0;

	public abstract EntityKey EntityKey { get; set; }

	public abstract bool OwnsRelationshipManager { get; }

	public abstract object Entity { get; }

	public abstract TEntity TypedEntity { get; }

	public abstract bool RequiresRelationshipChangeTracking { get; }

	protected BaseEntityWrapper(TEntity entity, RelationshipManager relationshipManager, bool overridesEquals)
	{
		if (relationshipManager == null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
		}
		_relationshipManager = relationshipManager;
		if (overridesEquals)
		{
			_flags = WrapperFlags.OverridesEquals;
		}
	}

	protected BaseEntityWrapper(TEntity entity, RelationshipManager relationshipManager, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType, bool overridesEquals)
	{
		if (relationshipManager == null)
		{
			throw new InvalidOperationException(Strings.RelationshipManager_UnexpectedNull);
		}
		_identityType = identityType;
		_relationshipManager = relationshipManager;
		if (overridesEquals)
		{
			_flags = WrapperFlags.OverridesEquals;
		}
		RelationshipManager.SetWrappedOwner(this, entity);
		if (entitySet != null)
		{
			Context = context;
			MergeOption = mergeOption;
			RelationshipManager.AttachContextToRelatedEnds(context, entitySet, mergeOption);
		}
	}

	public void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
		Context = context;
		MergeOption = mergeOption;
		if (entitySet != null)
		{
			RelationshipManager.AttachContextToRelatedEnds(context, entitySet, mergeOption);
		}
	}

	public void ResetContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
	{
		if (Context != context)
		{
			Context = context;
			MergeOption = mergeOption;
			RelationshipManager.ResetContextOnRelatedEnds(context, entitySet, mergeOption);
		}
	}

	public void DetachContext()
	{
		if (Context != null && Context.ObjectStateManager.TransactionManager.IsAttachTracking && Context.ObjectStateManager.TransactionManager.OriginalMergeOption == MergeOption.NoTracking)
		{
			MergeOption = MergeOption.NoTracking;
		}
		else
		{
			Context = null;
		}
		RelationshipManager.DetachContextFromRelatedEnds();
	}

	public abstract void EnsureCollectionNotNull(RelatedEnd relatedEnd);

	public abstract EntityKey GetEntityKeyFromEntity();

	public abstract void SetChangeTracker(IEntityChangeTracker changeTracker);

	public abstract void TakeSnapshot(EntityEntry entry);

	public abstract void TakeSnapshotOfRelationships(EntityEntry entry);

	public abstract object GetNavigationPropertyValue(RelatedEnd relatedEnd);

	public abstract void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value);

	public abstract void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value);

	public abstract void CollectionAdd(RelatedEnd relatedEnd, object value);

	public abstract bool CollectionRemove(RelatedEnd relatedEnd, object value);

	public abstract void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value);

	public abstract void UpdateCurrentValueRecord(object value, EntityEntry entry);
}
