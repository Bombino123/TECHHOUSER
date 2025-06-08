using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class ReferentialConstraint : MetadataItem
{
	private RelationshipEndMember _fromRole;

	private RelationshipEndMember _toRole;

	private readonly ReadOnlyMetadataCollection<EdmProperty> _fromProperties;

	private readonly ReadOnlyMetadataCollection<EdmProperty> _toProperties;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.ReferentialConstraint;

	internal override string Identity => FromRole.Name + "_" + ToRole.Name;

	[MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
	public RelationshipEndMember FromRole
	{
		get
		{
			return _fromRole;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_fromRole = value;
		}
	}

	[MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
	public RelationshipEndMember ToRole
	{
		get
		{
			return _toRole;
		}
		set
		{
			Util.ThrowIfReadOnly(this);
			_toRole = value;
		}
	}

	internal AssociationEndMember PrincipalEnd => (AssociationEndMember)FromRole;

	internal AssociationEndMember DependentEnd => (AssociationEndMember)ToRole;

	[MetadataProperty(BuiltInTypeKind.EdmProperty, true)]
	public ReadOnlyMetadataCollection<EdmProperty> FromProperties
	{
		get
		{
			if (!base.IsReadOnly && _fromProperties.Count == 0)
			{
				_fromRole.GetEntityType().KeyMembers.Each(delegate(EdmMember p)
				{
					_fromProperties.Source.Add((EdmProperty)p);
				});
			}
			return _fromProperties;
		}
	}

	[MetadataProperty(BuiltInTypeKind.EdmProperty, true)]
	public ReadOnlyMetadataCollection<EdmProperty> ToProperties => _toProperties;

	public ReferentialConstraint(RelationshipEndMember fromRole, RelationshipEndMember toRole, IEnumerable<EdmProperty> fromProperties, IEnumerable<EdmProperty> toProperties)
	{
		Check.NotNull(fromRole, "fromRole");
		Check.NotNull(toRole, "toRole");
		Check.NotNull(fromProperties, "fromProperties");
		Check.NotNull(toProperties, "toProperties");
		_fromRole = fromRole;
		_toRole = toRole;
		_fromProperties = new ReadOnlyMetadataCollection<EdmProperty>(new MetadataCollection<EdmProperty>(fromProperties));
		_toProperties = new ReadOnlyMetadataCollection<EdmProperty>(new MetadataCollection<EdmProperty>(toProperties));
	}

	public override string ToString()
	{
		return FromRole.Name + "_" + ToRole.Name;
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			FromProperties.Source.SetReadOnly();
			ToProperties.Source.SetReadOnly();
			base.SetReadOnly();
			FromRole?.SetReadOnly();
			ToRole?.SetReadOnly();
		}
	}

	internal string BuildConstraintExceptionMessage()
	{
		string name = FromProperties.First().DeclaringType.Name;
		string name2 = ToProperties.First().DeclaringType.Name;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int i = 0; i < FromProperties.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
				stringBuilder2.Append(", ");
			}
			stringBuilder.Append(name).Append('.').Append(FromProperties[i]);
			stringBuilder2.Append(name2).Append('.').Append(ToProperties[i]);
		}
		return Strings.RelationshipManager_InconsistentReferentialConstraintProperties(stringBuilder.ToString(), stringBuilder2.ToString());
	}
}
