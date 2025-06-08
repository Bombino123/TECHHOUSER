using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;

internal static class CqlWriter
{
	private static readonly Regex _wordIdentifierRegex = new Regex("^[_A-Za-z]\\w*$", RegexOptions.Compiled | RegexOptions.ECMAScript);

	internal static string GetQualifiedName(string blockName, string field)
	{
		return StringUtil.FormatInvariant("{0}.{1}", blockName, field);
	}

	internal static void AppendEscapedTypeName(StringBuilder builder, EdmType type)
	{
		AppendEscapedName(builder, GetQualifiedName(type.NamespaceName, type.Name));
	}

	internal static void AppendEscapedQualifiedName(StringBuilder builder, string name1, string name2)
	{
		AppendEscapedName(builder, name1);
		builder.Append('.');
		AppendEscapedName(builder, name2);
	}

	internal static void AppendEscapedName(StringBuilder builder, string name)
	{
		if (_wordIdentifierRegex.IsMatch(name) && !ExternalCalls.IsReservedKeyword(name))
		{
			builder.Append(name);
			return;
		}
		string value = name.Replace("]", "]]");
		builder.Append('[').Append(value).Append(']');
	}
}
