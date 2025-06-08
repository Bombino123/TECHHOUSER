using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class SortRemover : BasicOpVisitorOfNode
{
	private readonly Command m_command;

	private readonly Node m_topMostSort;

	private readonly HashSet<Node> changedNodes = new HashSet<Node>();

	private SortRemover(Command command, Node topMostSort)
	{
		m_command = command;
		m_topMostSort = topMostSort;
	}

	internal static void Process(Command command)
	{
		Node topMostSort = ((command.Root.Child0 == null || command.Root.Child0.Op.OpType != OpType.Sort) ? null : command.Root.Child0);
		SortRemover sortRemover = new SortRemover(command, topMostSort);
		command.Root = sortRemover.VisitNode(command.Root);
	}

	protected override void VisitChildren(Node n)
	{
		bool flag = false;
		for (int i = 0; i < n.Children.Count; i++)
		{
			Node node = n.Children[i];
			n.Children[i] = VisitNode(n.Children[i]);
			if (node != n.Children[i] || changedNodes.Contains(node))
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_command.RecomputeNodeInfo(n);
			changedNodes.Add(n);
		}
	}

	public override Node Visit(ConstrainedSortOp op, Node n)
	{
		if (op.Keys.Count > 0 || n.Children.Count != 3 || n.Child0 == null || n.Child1 == null || n.Child0.Op.OpType != OpType.Sort || n.Child1.Op.OpType != OpType.Null || n.Child0.Children.Count != 1)
		{
			return n;
		}
		return m_command.CreateNode(m_command.CreateConstrainedSortOp(((SortOp)n.Child0.Op).Keys, op.WithTies), n.Child0.Child0, n.Child1, n.Child2);
	}

	public override Node Visit(SortOp op, Node n)
	{
		VisitChildren(n);
		if (n == m_topMostSort)
		{
			return n;
		}
		return n.Child0;
	}
}
