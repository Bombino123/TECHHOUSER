namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class EntityIdentity
{
	private readonly SimpleColumnMap[] m_keys;

	internal SimpleColumnMap[] Keys => m_keys;

	internal EntityIdentity(SimpleColumnMap[] keyColumns)
	{
		m_keys = keyColumns;
	}
}
