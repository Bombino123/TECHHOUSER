namespace System.Data.Entity.Core.Metadata.Edm;

internal static class RelationshipMultiplicityConverter
{
	internal static string MultiplicityToString(RelationshipMultiplicity multiplicity)
	{
		return multiplicity switch
		{
			RelationshipMultiplicity.Many => "*", 
			RelationshipMultiplicity.One => "1", 
			RelationshipMultiplicity.ZeroOrOne => "0..1", 
			_ => string.Empty, 
		};
	}

	internal static bool TryParseMultiplicity(string value, out RelationshipMultiplicity multiplicity)
	{
		switch (value)
		{
		case "*":
			multiplicity = RelationshipMultiplicity.Many;
			return true;
		case "1":
			multiplicity = RelationshipMultiplicity.One;
			return true;
		case "0..1":
			multiplicity = RelationshipMultiplicity.ZeroOrOne;
			return true;
		default:
			multiplicity = (RelationshipMultiplicity)(-1);
			return false;
		}
	}
}
