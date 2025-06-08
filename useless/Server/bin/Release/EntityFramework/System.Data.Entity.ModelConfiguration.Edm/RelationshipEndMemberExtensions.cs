using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Edm;

internal static class RelationshipEndMemberExtensions
{
	public static bool IsMany(this RelationshipEndMember associationEnd)
	{
		return associationEnd.RelationshipMultiplicity.IsMany();
	}

	public static bool IsOptional(this RelationshipEndMember associationEnd)
	{
		return associationEnd.RelationshipMultiplicity.IsOptional();
	}

	public static bool IsRequired(this RelationshipEndMember associationEnd)
	{
		return associationEnd.RelationshipMultiplicity.IsRequired();
	}
}
