using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class AugmentedNode
{
	private readonly int m_id;

	private readonly Node m_node;

	protected AugmentedNode m_parent;

	private readonly List<AugmentedNode> m_children;

	private readonly List<JoinEdge> m_joinEdges = new List<JoinEdge>();

	internal int Id => m_id;

	internal Node Node => m_node;

	internal AugmentedNode Parent => m_parent;

	internal List<AugmentedNode> Children => m_children;

	internal List<JoinEdge> JoinEdges => m_joinEdges;

	internal AugmentedNode(int id, Node node)
		: this(id, node, new List<AugmentedNode>())
	{
	}

	internal AugmentedNode(int id, Node node, List<AugmentedNode> children)
	{
		m_id = id;
		m_node = node;
		m_children = children;
		PlanCompiler.Assert(children != null, "null children (gasp!)");
		foreach (AugmentedNode child in m_children)
		{
			child.m_parent = this;
		}
	}
}
