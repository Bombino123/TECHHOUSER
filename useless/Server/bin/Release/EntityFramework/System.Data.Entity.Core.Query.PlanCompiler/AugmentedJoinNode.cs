using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal sealed class AugmentedJoinNode : AugmentedNode
{
	private readonly List<ColumnVar> m_leftVars;

	private readonly List<ColumnVar> m_rightVars;

	private readonly Node m_otherPredicate;

	internal Node OtherPredicate => m_otherPredicate;

	internal List<ColumnVar> LeftVars => m_leftVars;

	internal List<ColumnVar> RightVars => m_rightVars;

	internal AugmentedJoinNode(int id, Node node, AugmentedNode leftChild, AugmentedNode rightChild, List<ColumnVar> leftVars, List<ColumnVar> rightVars, Node otherPredicate)
		: this(id, node, new List<AugmentedNode>(new AugmentedNode[2] { leftChild, rightChild }))
	{
		m_otherPredicate = otherPredicate;
		m_rightVars = rightVars;
		m_leftVars = leftVars;
	}

	internal AugmentedJoinNode(int id, Node node, List<AugmentedNode> children)
		: base(id, node, children)
	{
		m_leftVars = new List<ColumnVar>();
		m_rightVars = new List<ColumnVar>();
	}
}
