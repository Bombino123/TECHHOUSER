using System.Collections.Generic;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class Node
{
	private readonly int m_id;

	private readonly List<Node> m_children;

	private NodeInfo m_nodeInfo;

	internal List<Node> Children => m_children;

	internal Op Op { get; set; }

	internal Node Child0
	{
		get
		{
			return m_children[0];
		}
		set
		{
			m_children[0] = value;
		}
	}

	internal bool HasChild0 => m_children.Count > 0;

	internal Node Child1
	{
		get
		{
			return m_children[1];
		}
		set
		{
			m_children[1] = value;
		}
	}

	internal bool HasChild1 => m_children.Count > 1;

	internal Node Child2
	{
		get
		{
			return m_children[2];
		}
		set
		{
			m_children[2] = value;
		}
	}

	internal Node Child3 => m_children[3];

	internal bool HasChild2 => m_children.Count > 2;

	internal bool HasChild3 => m_children.Count > 3;

	internal bool IsNodeInfoInitialized => m_nodeInfo != null;

	internal Node(int nodeId, Op op, List<Node> children)
	{
		m_id = nodeId;
		Op = op;
		m_children = children;
	}

	internal Node(Op op, params Node[] children)
		: this(-1, op, new List<Node>(children))
	{
	}

	internal bool IsEquivalent(Node other)
	{
		if (Children.Count != other.Children.Count)
		{
			return false;
		}
		if (new bool?(Op.IsEquivalent(other.Op)) != true)
		{
			return false;
		}
		for (int i = 0; i < Children.Count; i++)
		{
			if (!Children[i].IsEquivalent(other.Children[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal NodeInfo GetNodeInfo(Command command)
	{
		if (m_nodeInfo == null)
		{
			InitializeNodeInfo(command);
		}
		return m_nodeInfo;
	}

	internal ExtendedNodeInfo GetExtendedNodeInfo(Command command)
	{
		if (m_nodeInfo == null)
		{
			InitializeNodeInfo(command);
		}
		return m_nodeInfo as ExtendedNodeInfo;
	}

	private void InitializeNodeInfo(Command command)
	{
		if (Op.IsRelOp || Op.IsPhysicalOp)
		{
			m_nodeInfo = new ExtendedNodeInfo(command);
		}
		else
		{
			m_nodeInfo = new NodeInfo(command);
		}
		command.RecomputeNodeInfo(this);
	}
}
