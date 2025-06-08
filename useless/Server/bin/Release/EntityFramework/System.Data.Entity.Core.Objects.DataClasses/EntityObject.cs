using System.ComponentModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace System.Data.Entity.Core.Objects.DataClasses;

[Serializable]
[DataContract(IsReference = true)]
public abstract class EntityObject : StructuralObject, IEntityWithKey, IEntityWithChangeTracker, IEntityWithRelationships
{
	private class DetachedEntityChangeTracker : IEntityChangeTracker
	{
		EntityState IEntityChangeTracker.EntityState => EntityState.Detached;

		void IEntityChangeTracker.EntityMemberChanging(string entityMemberName)
		{
		}

		void IEntityChangeTracker.EntityMemberChanged(string entityMemberName)
		{
		}

		void IEntityChangeTracker.EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexMemberName)
		{
		}

		void IEntityChangeTracker.EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexMemberName)
		{
		}
	}

	private RelationshipManager _relationships;

	private EntityKey _entityKey;

	[NonSerialized]
	private IEntityChangeTracker _entityChangeTracker = _detachedEntityChangeTracker;

	[NonSerialized]
	private static readonly DetachedEntityChangeTracker _detachedEntityChangeTracker = new DetachedEntityChangeTracker();

	private IEntityChangeTracker EntityChangeTracker
	{
		get
		{
			if (_entityChangeTracker == null)
			{
				_entityChangeTracker = _detachedEntityChangeTracker;
			}
			return _entityChangeTracker;
		}
		set
		{
			_entityChangeTracker = value;
		}
	}

	[Browsable(false)]
	[XmlIgnore]
	public EntityState EntityState => EntityChangeTracker.EntityState;

	[Browsable(false)]
	[DataMember]
	public EntityKey EntityKey
	{
		get
		{
			return _entityKey;
		}
		set
		{
			EntityChangeTracker.EntityMemberChanging("-EntityKey-");
			_entityKey = value;
			EntityChangeTracker.EntityMemberChanged("-EntityKey-");
		}
	}

	RelationshipManager IEntityWithRelationships.RelationshipManager
	{
		get
		{
			if (_relationships == null)
			{
				_relationships = RelationshipManager.Create(this);
			}
			return _relationships;
		}
	}

	internal sealed override bool IsChangeTracked => EntityState != EntityState.Detached;

	void IEntityWithChangeTracker.SetChangeTracker(IEntityChangeTracker changeTracker)
	{
		if (changeTracker != null && EntityChangeTracker != _detachedEntityChangeTracker && changeTracker != EntityChangeTracker && (!(EntityChangeTracker is EntityEntry entityEntry) || !entityEntry.ObjectStateManager.IsDisposed))
		{
			throw new InvalidOperationException(Strings.Entity_EntityCantHaveMultipleChangeTrackers);
		}
		EntityChangeTracker = changeTracker;
	}

	protected sealed override void ReportPropertyChanging(string property)
	{
		Check.NotEmpty(property, "property");
		base.ReportPropertyChanging(property);
		EntityChangeTracker.EntityMemberChanging(property);
	}

	protected sealed override void ReportPropertyChanged(string property)
	{
		Check.NotEmpty(property, "property");
		EntityChangeTracker.EntityMemberChanged(property);
		base.ReportPropertyChanged(property);
	}

	internal sealed override void ReportComplexPropertyChanging(string entityMemberName, ComplexObject complexObject, string complexMemberName)
	{
		EntityChangeTracker.EntityComplexMemberChanging(entityMemberName, complexObject, complexMemberName);
	}

	internal sealed override void ReportComplexPropertyChanged(string entityMemberName, ComplexObject complexObject, string complexMemberName)
	{
		EntityChangeTracker.EntityComplexMemberChanged(entityMemberName, complexObject, complexMemberName);
	}
}
