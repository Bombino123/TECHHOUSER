using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ExtendedNodeInfo : NodeInfo
{
	private readonly VarVec m_localDefinitions;

	private readonly VarVec m_definitions;

	private readonly KeyVec m_keys;

	private readonly VarVec m_nonNullableDefinitions;

	private readonly VarVec m_nonNullableVisibleDefinitions;

	private RowCount m_minRows;

	private RowCount m_maxRows;

	internal VarVec LocalDefinitions => m_localDefinitions;

	internal VarVec Definitions => m_definitions;

	internal KeyVec Keys => m_keys;

	internal VarVec NonNullableDefinitions => m_nonNullableDefinitions;

	internal VarVec NonNullableVisibleDefinitions => m_nonNullableVisibleDefinitions;

	internal RowCount MinRows
	{
		get
		{
			return m_minRows;
		}
		set
		{
			m_minRows = value;
		}
	}

	internal RowCount MaxRows
	{
		get
		{
			return m_maxRows;
		}
		set
		{
			m_maxRows = value;
		}
	}

	internal ExtendedNodeInfo(Command cmd)
		: base(cmd)
	{
		m_localDefinitions = cmd.CreateVarVec();
		m_definitions = cmd.CreateVarVec();
		m_nonNullableDefinitions = cmd.CreateVarVec();
		m_nonNullableVisibleDefinitions = cmd.CreateVarVec();
		m_keys = new KeyVec(cmd);
		m_minRows = RowCount.Zero;
		m_maxRows = RowCount.Unbounded;
	}

	internal override void Clear()
	{
		base.Clear();
		m_definitions.Clear();
		m_localDefinitions.Clear();
		m_nonNullableDefinitions.Clear();
		m_nonNullableVisibleDefinitions.Clear();
		m_keys.Clear();
		m_minRows = RowCount.Zero;
		m_maxRows = RowCount.Unbounded;
	}

	internal override void ComputeHashValue(Command cmd, Node n)
	{
		base.ComputeHashValue(cmd, n);
		m_hashValue = (m_hashValue << 4) ^ NodeInfo.GetHashValue(Definitions);
		m_hashValue = (m_hashValue << 4) ^ NodeInfo.GetHashValue(Keys.KeyVars);
	}

	internal void SetRowCount(RowCount minRows, RowCount maxRows)
	{
		m_minRows = minRows;
		m_maxRows = maxRows;
	}

	internal void InitRowCountFrom(ExtendedNodeInfo source)
	{
		m_minRows = source.m_minRows;
		m_maxRows = source.m_maxRows;
	}

	[Conditional("DEBUG")]
	private void ValidateRowCount()
	{
	}
}
