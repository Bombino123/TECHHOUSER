namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ScanTableBaseOp : RelOp
{
	private readonly Table m_table;

	internal Table Table => m_table;

	protected ScanTableBaseOp(OpType opType, Table table)
		: base(opType)
	{
		m_table = table;
	}

	protected ScanTableBaseOp(OpType opType)
		: base(opType)
	{
	}
}
