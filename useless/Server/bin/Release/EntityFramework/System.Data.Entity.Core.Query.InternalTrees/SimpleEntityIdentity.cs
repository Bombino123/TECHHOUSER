using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class SimpleEntityIdentity : EntityIdentity
{
	private readonly EntitySet m_entitySet;

	internal EntitySet EntitySet => m_entitySet;

	internal SimpleEntityIdentity(EntitySet entitySet, SimpleColumnMap[] keyColumns)
		: base(keyColumns)
	{
		m_entitySet = entitySet;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Empty;
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[(ES={0}) (Keys={", new object[1] { EntitySet.Name });
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
