using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class DiscriminatedEntityIdentity : EntityIdentity
{
	private readonly SimpleColumnMap m_entitySetColumn;

	private readonly EntitySet[] m_entitySetMap;

	internal SimpleColumnMap EntitySetColumnMap => m_entitySetColumn;

	internal EntitySet[] EntitySetMap => m_entitySetMap;

	internal DiscriminatedEntityIdentity(SimpleColumnMap entitySetColumn, EntitySet[] entitySetMap, SimpleColumnMap[] keyColumns)
		: base(keyColumns)
	{
		m_entitySetColumn = entitySetColumn;
		m_entitySetMap = entitySetMap;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[(Keys={");
		SimpleColumnMap[] keys = base.Keys;
		foreach (SimpleColumnMap simpleColumnMap in keys)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", new object[2] { text, simpleColumnMap });
			text = ",";
		}
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "})]");
		return stringBuilder.ToString();
	}
}
