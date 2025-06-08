using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal sealed class AugmentedTableNode : AugmentedNode
{
	private readonly Table m_table;

	private AugmentedTableNode m_replacementTable;

	private int m_newLocationId;

	internal Table Table => m_table;

	internal int LastVisibleId { get; set; }

	internal bool IsEliminated => m_replacementTable != this;

	internal AugmentedTableNode ReplacementTable
	{
		get
		{
			return m_replacementTable;
		}
		set
		{
			m_replacementTable = value;
		}
	}

	internal int NewLocationId
	{
		get
		{
			return m_newLocationId;
		}
		set
		{
			m_newLocationId = value;
		}
	}

	internal bool IsMoved => m_newLocationId != base.Id;

	internal VarVec NullableColumns { get; set; }

	internal AugmentedTableNode(int id, Node node)
		: base(id, node)
	{
		ScanTableOp scanTableOp = (ScanTableOp)node.Op;
		m_table = scanTableOp.Table;
		LastVisibleId = id;
		m_replacementTable = this;
		m_newLocationId = id;
	}
}
