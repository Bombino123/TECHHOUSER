using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class EntitySetBase : MetadataItem, INamedDataModelItem
{
	private EntityContainer _entityContainer;

	private string _name;

	private EntityTypeBase _elementType;

	private string _table;

	private string _schema;

	private string _definingQuery;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EntitySetBase;

	string INamedDataModelItem.Identity => Identity;

	internal override string Identity => Name;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string DefiningQuery
	{
		get
		{
			return _definingQuery;
		}
		internal set
		{
			Check.NotEmpty(value, "value");
			Util.ThrowIfReadOnly(this);
			_definingQuery = value;
		}
	}

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public virtual string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Check.NotEmpty(value, "value");
			Util.ThrowIfReadOnly(this);
			if (!string.Equals(_name, value, StringComparison.Ordinal))
			{
				string identity = Identity;
				_name = value;
				if (_entityContainer != null)
				{
					_entityContainer.NotifyItemIdentityChanged(this, identity);
				}
			}
		}
	}

	public virtual EntityContainer EntityContainer => _entityContainer;

	[MetadataProperty(BuiltInTypeKind.EntityTypeBase, false)]
	public EntityTypeBase ElementType
	{
		get
		{
			return _elementType;
		}
		internal set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			_elementType = value;
		}
	}

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string Table
	{
		get
		{
			return _table;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_table = value;
		}
	}

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string Schema
	{
		get
		{
			return _schema;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_schema = value;
		}
	}

	internal EntitySetBase()
	{
	}

	internal EntitySetBase(string name, string schema, string table, string definingQuery, EntityTypeBase entityType)
	{
		Check.NotNull(entityType, "entityType");
		Check.NotEmpty(name, "name");
		_name = name;
		_schema = schema;
		_table = table;
		_definingQuery = definingQuery;
		ElementType = entityType;
	}

	public override string ToString()
	{
		return Name;
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
			ElementType?.SetReadOnly();
		}
	}

	internal void ChangeEntityContainerWithoutCollectionFixup(EntityContainer newEntityContainer)
	{
		_entityContainer = newEntityContainer;
	}
}
