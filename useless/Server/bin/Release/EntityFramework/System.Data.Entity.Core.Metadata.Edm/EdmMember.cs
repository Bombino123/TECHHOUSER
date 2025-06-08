using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class EdmMember : MetadataItem, INamedDataModelItem
{
	private StructuralType _declaringType;

	private TypeUsage _typeUsage;

	private string _name;

	private string _identity;

	string INamedDataModelItem.Identity => Identity;

	internal override string Identity => _identity ?? Name;

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
			if (string.Equals(_name, value, StringComparison.Ordinal))
			{
				return;
			}
			string identity = Identity;
			_name = value;
			if (_declaringType == null)
			{
				return;
			}
			if (_declaringType.Members.Except(new EdmMember[1] { this }).Any((EdmMember c) => string.Equals(Identity, c.Identity, StringComparison.Ordinal)))
			{
				_identity = _declaringType.Members.Select((EdmMember i) => i.Identity).Uniquify(Identity);
			}
			_declaringType.NotifyItemIdentityChanged(this, identity);
		}
	}

	public virtual StructuralType DeclaringType => _declaringType;

	[MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
	public virtual TypeUsage TypeUsage
	{
		get
		{
			return _typeUsage;
		}
		protected set
		{
			Check.NotNull(value, "value");
			Util.ThrowIfReadOnly(this);
			_typeUsage = value;
		}
	}

	public bool IsStoreGeneratedComputed
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", ignoreCase: false, out var item))
			{
				return (StoreGeneratedPattern)item.Value == StoreGeneratedPattern.Computed;
			}
			return false;
		}
	}

	public bool IsStoreGeneratedIdentity
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", ignoreCase: false, out var item))
			{
				return (StoreGeneratedPattern)item.Value == StoreGeneratedPattern.Identity;
			}
			return false;
		}
	}

	internal virtual bool IsPrimaryKeyColumn
	{
		get
		{
			if (_declaringType is EntityTypeBase entityTypeBase)
			{
				return entityTypeBase.KeyMembers.Contains(this);
			}
			return false;
		}
	}

	internal EdmMember()
	{
	}

	internal EdmMember(string name, TypeUsage memberTypeUsage)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(memberTypeUsage, "memberTypeUsage");
		_name = name;
		_typeUsage = memberTypeUsage;
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
			string identity = _identity;
			_identity = Name;
			if (_declaringType != null && identity != null && !string.Equals(identity, _identity, StringComparison.Ordinal))
			{
				_declaringType.NotifyItemIdentityChanged(this, identity);
			}
		}
	}

	internal void ChangeDeclaringTypeWithoutCollectionFixup(StructuralType newDeclaringType)
	{
		_declaringType = newDeclaringType;
	}
}
