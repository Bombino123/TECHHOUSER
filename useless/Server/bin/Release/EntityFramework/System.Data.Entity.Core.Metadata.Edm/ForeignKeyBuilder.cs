using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ForeignKeyBuilder : MetadataItem, INamedDataModelItem
{
	private const string SelfRefSuffix = "Self";

	private readonly EdmModel _database;

	private readonly AssociationType _associationType;

	private readonly AssociationSet _associationSet;

	public string Name
	{
		get
		{
			return _associationType.Name;
		}
		set
		{
			_associationType.Name = value;
			_associationSet.Name = value;
		}
	}

	public virtual EntityType PrincipalTable
	{
		get
		{
			return _associationType.SourceEnd.GetEntityType();
		}
		set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			_associationType.SourceEnd = new AssociationEndMember(value.Name, value);
			_associationSet.SourceSet = _database.GetEntitySet(value);
			if (_associationType.TargetEnd != null && value.Name == _associationType.TargetEnd.Name)
			{
				_associationType.TargetEnd.Name = value.Name + "Self";
			}
		}
	}

	public virtual IEnumerable<EdmProperty> DependentColumns
	{
		get
		{
			if (_associationType.Constraint == null)
			{
				return Enumerable.Empty<EdmProperty>();
			}
			return _associationType.Constraint.ToProperties;
		}
		set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			_associationType.Constraint = new ReferentialConstraint(_associationType.SourceEnd, _associationType.TargetEnd, PrincipalTable.KeyProperties, value);
			SetMultiplicities();
		}
	}

	public OperationAction DeleteAction
	{
		get
		{
			if (_associationType.SourceEnd == null)
			{
				return OperationAction.None;
			}
			return _associationType.SourceEnd.DeleteBehavior;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_associationType.SourceEnd.DeleteBehavior = value;
		}
	}

	public override BuiltInTypeKind BuiltInTypeKind
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	string INamedDataModelItem.Identity => Identity;

	internal override string Identity
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal ForeignKeyBuilder()
	{
	}

	public ForeignKeyBuilder(EdmModel database, string name)
	{
		Check.NotNull(database, "database");
		_database = database;
		_associationType = new AssociationType(name, "CodeFirstDatabaseSchema", foreignKey: true, DataSpace.SSpace);
		_associationSet = new AssociationSet(_associationType.Name, _associationType);
	}

	public virtual void SetOwner(EntityType owner)
	{
		Util.ThrowIfReadOnly(this);
		if (owner == null)
		{
			_database.RemoveAssociationType(_associationType);
			return;
		}
		_associationType.TargetEnd = new AssociationEndMember((owner != PrincipalTable) ? owner.Name : (owner.Name + "Self"), owner);
		_associationSet.TargetSet = _database.GetEntitySet(owner);
		if (!_database.AssociationTypes.Contains(_associationType))
		{
			_database.AddAssociationType(_associationType);
			_database.AddAssociationSet(_associationSet);
		}
	}

	private void SetMultiplicities()
	{
		_associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
		_associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
		EntityType dependentTable = _associationType.TargetEnd.GetEntityType();
		List<EdmProperty> list = dependentTable.KeyProperties.Where((EdmProperty key) => dependentTable.DeclaredMembers.Contains(key)).ToList();
		if (list.Count == DependentColumns.Count() && list.All(DependentColumns.Contains<EdmProperty>))
		{
			_associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
			_associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
		}
		else if (!DependentColumns.Any((EdmProperty p) => p.Nullable))
		{
			_associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
		}
	}
}
