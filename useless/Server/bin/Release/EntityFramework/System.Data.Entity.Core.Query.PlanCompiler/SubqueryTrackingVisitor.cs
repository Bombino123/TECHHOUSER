using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal abstract class SubqueryTrackingVisitor : BasicOpVisitorOfNode
{
	protected readonly PlanCompiler m_compilerState;

	protected readonly Stack<Node> m_ancestors = new Stack<Node>();

	private readonly Dictionary<Node, List<Node>> m_nodeSubqueries = new Dictionary<Node, List<Node>>();

	protected Command m_command => m_compilerState.Command;

	protected SubqueryTrackingVisitor(PlanCompiler planCompilerState)
	{
		m_compilerState = planCompilerState;
	}

	protected void AddSubqueryToRelOpNode(Node relOpNode, Node subquery)
	{
		if (!m_nodeSubqueries.TryGetValue(relOpNode, out var value))
		{
			value = new List<Node>();
			m_nodeSubqueries[relOpNode] = value;
		}
		value.Add(subquery);
	}

	protected Node AddSubqueryToParentRelOp(Var outputVar, Node subquery)
	{
		Node node = FindRelOpAncestor();
		PlanCompiler.Assert(node != null, "no ancestors found?");
		AddSubqueryToRelOpNode(node, subquery);
		subquery = m_command.CreateNode(m_command.CreateVarRefOp(outputVar));
		return subquery;
	}

	protected Node FindRelOpAncestor()
	{
		foreach (Node ancestor in m_ancestors)
		{
			if (ancestor.Op.IsRelOp)
			{
				return ancestor;
			}
			if (ancestor.Op.IsPhysicalOp)
			{
				return null;
			}
		}
		return null;
	}

	protected override void VisitChildren(Node n)
	{
		m_ancestors.Push(n);
		for (int i = 0; i < n.Children.Count; i++)
		{
			n.Children[i] = VisitNode(n.Children[i]);
		}
		m_ancestors.Pop();
	}

	private Node AugmentWithSubqueries(Node input, List<Node> subqueries, bool inputFirst)
	{
		Node node;
		int num;
		if (inputFirst)
		{
			node = input;
			num = 0;
		}
		else
		{
			node = subqueries[0];
			num = 1;
		}
		for (int i = num; i < subqueries.Count; i++)
		{
			OuterApplyOp op = m_command.CreateOuterApplyOp();
			node = m_command.CreateNode(op, node, subqueries[i]);
		}
		if (!inputFirst)
		{
			node = m_command.CreateNode(m_command.CreateCrossApplyOp(), node, input);
		}
		m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.JoinElimination);
		return node;
	}

	protected override Node VisitRelOpDefault(RelOp op, Node n)
	{
		VisitChildren(n);
		if (m_nodeSubqueries.TryGetValue(n, out var value) && value.Count > 0)
		{
			PlanCompiler.Assert(n.Op.OpType == OpType.Project || n.Op.OpType == OpType.Filter || n.Op.OpType == OpType.GroupBy || n.Op.OpType == OpType.GroupByInto, "VisitRelOpDefault: Unexpected op?" + n.Op.OpType);
			Node child = AugmentWithSubqueries(n.Child0, value, inputFirst: true);
			n.Child0 = child;
		}
		return n;
	}

	protected bool ProcessJoinOp(Node n)
	{
		VisitChildren(n);
		if (!m_nodeSubqueries.TryGetValue(n, out var value))
		{
			return false;
		}
		PlanCompiler.Assert(n.Op.OpType == OpType.InnerJoin || n.Op.OpType == OpType.LeftOuterJoin || n.Op.OpType == OpType.FullOuterJoin, "unexpected op?");
		PlanCompiler.Assert(n.HasChild2, "missing second child to JoinOp?");
		Node child = n.Child2;
		Node input = m_command.CreateNode(m_command.CreateSingleRowTableOp());
		input = AugmentWithSubqueries(input, value, inputFirst: true);
		Node arg = m_command.CreateNode(m_command.CreateFilterOp(), input, child);
		Node child2 = m_command.CreateNode(m_command.CreateExistsOp(), arg);
		n.Child2 = child2;
		return true;
	}

	public override Node Visit(UnnestOp op, Node n)
	{
		VisitChildren(n);
		if (m_nodeSubqueries.TryGetValue(n, out var value))
		{
			return AugmentWithSubqueries(n, value, inputFirst: false);
		}
		return n;
	}
}
