using System.Data.Entity.Core.Common.Utils;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class CqlIdentifiers : InternalBase
{
	private readonly Set<string> m_identifiers;

	internal CqlIdentifiers()
	{
		m_identifiers = new Set<string>(StringComparer.Ordinal);
	}

	internal string GetFromVariable(int num)
	{
		return GetNonConflictingName("_from", num);
	}

	internal string GetBlockAlias(int num)
	{
		return GetNonConflictingName("T", num);
	}

	internal string GetBlockAlias()
	{
		return GetNonConflictingName("T", -1);
	}

	internal void AddIdentifier(string identifier)
	{
		m_identifiers.Add(identifier.ToLower(CultureInfo.InvariantCulture));
	}

	private string GetNonConflictingName(string prefix, int number)
	{
		string text = ((number < 0) ? prefix : StringUtil.FormatInvariant("{0}{1}", prefix, number));
		if (!m_identifiers.Contains(text.ToLower(CultureInfo.InvariantCulture)))
		{
			return text;
		}
		for (int i = 0; i < int.MaxValue; i++)
		{
			text = ((number >= 0) ? StringUtil.FormatInvariant("{0}_{1}_{2}", prefix, i, number) : StringUtil.FormatInvariant("{0}_{1}", prefix, i));
			if (!m_identifiers.Contains(text.ToLower(CultureInfo.InvariantCulture)))
			{
				return text;
			}
		}
		return null;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		m_identifiers.ToCompactString(builder);
	}
}
