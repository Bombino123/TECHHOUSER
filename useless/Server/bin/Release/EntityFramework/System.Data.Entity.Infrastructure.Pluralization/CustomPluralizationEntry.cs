using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Pluralization;

public class CustomPluralizationEntry
{
	public string Singular { get; private set; }

	public string Plural { get; private set; }

	public CustomPluralizationEntry(string singular, string plural)
	{
		Check.NotEmpty(singular, "singular");
		Check.NotEmpty(plural, "plural");
		Singular = singular;
		Plural = plural;
	}
}
