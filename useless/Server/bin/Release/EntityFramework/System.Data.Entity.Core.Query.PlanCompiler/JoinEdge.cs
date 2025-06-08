using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class JoinEdge
{
	private readonly AugmentedTableNode m_left;

	private readonly AugmentedTableNode m_right;

	private readonly AugmentedJoinNode m_joinNode;

	private readonly List<ColumnVar> m_leftVars;

	private readonly List<ColumnVar> m_rightVars;

	internal AugmentedTableNode Left => m_left;

	internal AugmentedTableNode Right => m_right;

	internal AugmentedJoinNode JoinNode => m_joinNode;

	internal JoinKind JoinKind { get; set; }

	internal List<ColumnVar> LeftVars => m_leftVars;

	internal List<ColumnVar> RightVars => m_rightVars;

	internal bool IsEliminated
	{
		get
		{
			if (!Left.IsEliminated)
			{
				return Right.IsEliminated;
			}
			return true;
		}
	}

	internal bool RestrictedElimination
	{
		get
		{
			if (m_joinNode != null)
			{
				if (m_joinNode.OtherPredicate == null && m_left.LastVisibleId >= m_joinNode.Id)
				{
					return m_right.LastVisibleId < m_joinNode.Id;
				}
				return true;
			}
			return false;
		}
	}

	private JoinEdge(AugmentedTableNode left, AugmentedTableNode right, AugmentedJoinNode joinNode, JoinKind joinKind, List<ColumnVar> leftVars, List<ColumnVar> rightVars)
	{
		m_left = left;
		m_right = right;
		JoinKind = joinKind;
		m_joinNode = joinNode;
		m_leftVars = leftVars;
		m_rightVars = rightVars;
		PlanCompiler.Assert(m_leftVars.Count == m_rightVars.Count, "Count mismatch: " + m_leftVars.Count + "," + m_rightVars.Count);
	}

	internal static JoinEdge CreateJoinEdge(AugmentedTableNode left, AugmentedTableNode right, AugmentedJoinNode joinNode, ColumnVar leftVar, ColumnVar rightVar)
	{
		List<ColumnVar> list = new List<ColumnVar>();
		List<ColumnVar> list2 = new List<ColumnVar>();
		list.Add(leftVar);
		list2.Add(rightVar);
		OpType opType = joinNode.Node.Op.OpType;
		PlanCompiler.Assert(opType == OpType.LeftOuterJoin || opType == OpType.InnerJoin, "Unexpected join type for join edge: " + opType);
		JoinKind joinKind = ((opType == OpType.LeftOuterJoin) ? JoinKind.LeftOuter : JoinKind.Inner);
		return new JoinEdge(left, right, joinNode, joinKind, list, list2);
	}

	internal static JoinEdge CreateTransitiveJoinEdge(AugmentedTableNode left, AugmentedTableNode right, JoinKind joinKind, List<ColumnVar> leftVars, List<ColumnVar> rightVars)
	{
		return new JoinEdge(left, right, null, joinKind, leftVars, rightVars);
	}

	internal bool AddCondition(AugmentedJoinNode joinNode, ColumnVar leftVar, ColumnVar rightVar)
	{
		if (joinNode != m_joinNode)
		{
			return false;
		}
		m_leftVars.Add(leftVar);
		m_rightVars.Add(rightVar);
		return true;
	}
}
