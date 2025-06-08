using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class StructuredColumnMap : ColumnMap
{
	private readonly ColumnMap[] m_properties;

	internal virtual SimpleColumnMap NullSentinel => null;

	internal ColumnMap[] Properties => m_properties;

	internal StructuredColumnMap(TypeUsage type, string name, ColumnMap[] properties)
		: base(type, name)
	{
		m_properties = properties;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		stringBuilder.Append("{");
		ColumnMap[] properties = Properties;
		foreach (ColumnMap columnMap in properties)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", new object[2] { text, columnMap });
			text = ",";
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
