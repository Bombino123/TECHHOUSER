using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class NavigationProperty : EdmMember
{
	internal const string RelationshipTypeNamePropertyName = "RelationshipType";

	internal const string ToEndMemberNamePropertyName = "ToEndMember";

	private readonly NavigationPropertyAccessor _accessor;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.NavigationProperty;

	[MetadataProperty(BuiltInTypeKind.RelationshipType, false)]
	public RelationshipType RelationshipType { get; internal set; }

	[MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
	public RelationshipEndMember ToEndMember { get; internal set; }

	[MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
	public RelationshipEndMember FromEndMember { get; internal set; }

	internal AssociationType Association => (AssociationType)RelationshipType;

	internal AssociationEndMember ResultEnd => (AssociationEndMember)ToEndMember;

	internal NavigationPropertyAccessor Accessor => _accessor;

	internal NavigationProperty(string name, TypeUsage typeUsage)
		: base(name, typeUsage)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(typeUsage, "typeUsage");
		_accessor = new NavigationPropertyAccessor(name);
	}

	public IEnumerable<EdmProperty> GetDependentProperties()
	{
		AssociationType associationType = (AssociationType)RelationshipType;
		if (associationType.ReferentialConstraints.Count > 0)
		{
			ReferentialConstraint referentialConstraint = associationType.ReferentialConstraints[0];
			if (referentialConstraint.ToRole.EdmEquals(FromEndMember))
			{
				ReadOnlyMetadataCollection<EdmMember> keyMembers = referentialConstraint.FromRole.GetEntityType().KeyMembers;
				List<EdmProperty> list = new List<EdmProperty>(keyMembers.Count);
				for (int i = 0; i < keyMembers.Count; i++)
				{
					list.Add(referentialConstraint.ToProperties[referentialConstraint.FromProperties.IndexOf((EdmProperty)keyMembers[i])]);
				}
				return new ReadOnlyCollection<EdmProperty>(list);
			}
		}
		return Enumerable.Empty<EdmProperty>();
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly && ToEndMember != null && ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
		{
			TypeUsage = TypeUsage.ShallowCopy(Facet.Create(MetadataItem.NullableFacetDescription, false));
		}
		base.SetReadOnly();
	}

	public static NavigationProperty Create(string name, TypeUsage typeUsage, RelationshipType relationshipType, RelationshipEndMember from, RelationshipEndMember to, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(typeUsage, "typeUsage");
		NavigationProperty navigationProperty = new NavigationProperty(name, typeUsage);
		navigationProperty.RelationshipType = relationshipType;
		navigationProperty.FromEndMember = from;
		navigationProperty.ToEndMember = to;
		if (metadataProperties != null)
		{
			navigationProperty.AddMetadataProperties(metadataProperties);
		}
		navigationProperty.SetReadOnly();
		return navigationProperty;
	}
}
