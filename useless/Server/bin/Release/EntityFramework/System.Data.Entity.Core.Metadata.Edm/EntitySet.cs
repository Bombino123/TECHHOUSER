using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Utilities;
using System.Threading;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EntitySet : EntitySetBase
{
	private ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> _foreignKeyDependents;

	private ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> _foreignKeyPrincipals;

	private ReadOnlyCollection<AssociationSet> _associationSets;

	private volatile bool _hasForeignKeyRelationships;

	private volatile bool _hasIndependentRelationships;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EntitySet;

	public new virtual EntityType ElementType => (EntityType)base.ElementType;

	internal ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyDependents
	{
		get
		{
			if (_foreignKeyDependents == null)
			{
				InitializeForeignKeyLists();
			}
			return _foreignKeyDependents;
		}
	}

	internal ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> ForeignKeyPrincipals
	{
		get
		{
			if (_foreignKeyPrincipals == null)
			{
				InitializeForeignKeyLists();
			}
			return _foreignKeyPrincipals;
		}
	}

	internal ReadOnlyCollection<AssociationSet> AssociationSets
	{
		get
		{
			if (_foreignKeyPrincipals == null)
			{
				InitializeForeignKeyLists();
			}
			return _associationSets;
		}
	}

	internal bool HasForeignKeyRelationships
	{
		get
		{
			if (_foreignKeyPrincipals == null)
			{
				InitializeForeignKeyLists();
			}
			return _hasForeignKeyRelationships;
		}
	}

	internal bool HasIndependentRelationships
	{
		get
		{
			if (_foreignKeyPrincipals == null)
			{
				InitializeForeignKeyLists();
			}
			return _hasIndependentRelationships;
		}
	}

	internal EntitySet()
	{
	}

	internal EntitySet(string name, string schema, string table, string definingQuery, EntityType entityType)
		: base(name, schema, table, definingQuery, entityType)
	{
	}

	private void InitializeForeignKeyLists()
	{
		List<Tuple<AssociationSet, ReferentialConstraint>> list = new List<Tuple<AssociationSet, ReferentialConstraint>>();
		List<Tuple<AssociationSet, ReferentialConstraint>> list2 = new List<Tuple<AssociationSet, ReferentialConstraint>>();
		bool hasForeignKeyRelationships = false;
		bool hasIndependentRelationships = false;
		ReadOnlyCollection<AssociationSet> readOnlyCollection = new ReadOnlyCollection<AssociationSet>(MetadataHelper.GetAssociationsForEntitySet(this));
		foreach (AssociationSet item in readOnlyCollection)
		{
			if (item.ElementType.IsForeignKey)
			{
				hasForeignKeyRelationships = true;
				ReferentialConstraint referentialConstraint = item.ElementType.ReferentialConstraints[0];
				if (referentialConstraint.ToRole.GetEntityType().IsAssignableFrom(ElementType) || ElementType.IsAssignableFrom(referentialConstraint.ToRole.GetEntityType()))
				{
					list.Add(new Tuple<AssociationSet, ReferentialConstraint>(item, referentialConstraint));
				}
				if (referentialConstraint.FromRole.GetEntityType().IsAssignableFrom(ElementType) || ElementType.IsAssignableFrom(referentialConstraint.FromRole.GetEntityType()))
				{
					list2.Add(new Tuple<AssociationSet, ReferentialConstraint>(item, referentialConstraint));
				}
			}
			else
			{
				hasIndependentRelationships = true;
			}
		}
		_hasForeignKeyRelationships = hasForeignKeyRelationships;
		_hasIndependentRelationships = hasIndependentRelationships;
		ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> value = new ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>>(list);
		ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>> value2 = new ReadOnlyCollection<Tuple<AssociationSet, ReferentialConstraint>>(list2);
		Interlocked.CompareExchange(ref _foreignKeyDependents, value, null);
		Interlocked.CompareExchange(ref _foreignKeyPrincipals, value2, null);
		Interlocked.CompareExchange(ref _associationSets, readOnlyCollection, null);
	}

	public static EntitySet Create(string name, string schema, string table, string definingQuery, EntityType entityType, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(entityType, "entityType");
		EntitySet entitySet = new EntitySet(name, schema, table, definingQuery, entityType);
		if (metadataProperties != null)
		{
			entitySet.AddMetadataProperties(metadataProperties);
		}
		entitySet.SetReadOnly();
		return entitySet;
	}
}
