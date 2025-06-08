namespace System.Data.Entity.Core.Query.InternalTrees;

internal class NodeInfo
{
	private readonly VarVec m_externalReferences;

	protected int m_hashValue;

	internal VarVec ExternalReferences => m_externalReferences;

	internal int HashValue => m_hashValue;

	internal NodeInfo(Command cmd)
	{
		m_externalReferences = cmd.CreateVarVec();
	}

	internal virtual void Clear()
	{
		m_externalReferences.Clear();
		m_hashValue = 0;
	}

	internal static int GetHashValue(VarVec vec)
	{
		int num = 0;
		foreach (Var item in vec)
		{
			num ^= item.GetHashCode();
		}
		return num;
	}

	internal virtual void ComputeHashValue(Command cmd, Node n)
	{
		m_hashValue = 0;
		foreach (Node child in n.Children)
		{
			NodeInfo nodeInfo = cmd.GetNodeInfo(child);
			m_hashValue ^= nodeInfo.HashValue;
		}
		m_hashValue = (m_hashValue << 4) ^ (int)n.Op.OpType;
		m_hashValue = (m_hashValue << 4) ^ GetHashValue(m_externalReferences);
	}
}
