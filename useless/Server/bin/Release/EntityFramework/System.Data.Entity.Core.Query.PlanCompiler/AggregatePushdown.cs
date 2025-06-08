using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class AggregatePushdown
{
	private readonly Command m_command;

	private TryGetValue m_tryGetParent;

	private AggregatePushdown(Command command)
	{
		m_command = command;
	}

	internal static void Process(PlanCompiler planCompilerState)
	{
		new AggregatePushdown(planCompilerState.Command).Process();
	}

	private void Process()
	{
		foreach (GroupAggregateVarInfo item in GroupAggregateRefComputingVisitor.Process(m_command, out m_tryGetParent))
		{
			if (!item.HasCandidateAggregateNodes)
			{
				continue;
			}
			foreach (KeyValuePair<Node, List<Node>> candidateAggregateNode in item.CandidateAggregateNodes)
			{
				TryProcessCandidate(candidateAggregateNode, item);
			}
		}
	}

	private void TryProcessCandidate(KeyValuePair<Node, List<Node>> candidate, GroupAggregateVarInfo groupAggregateVarInfo)
	{
		Node definingGroupNode = groupAggregateVarInfo.DefiningGroupNode;
		FindPathsToLeastCommonAncestor(candidate.Key, definingGroupNode, out var _, out var ancestors2);
		if (!AreAllNodesSupportedForPropagation(ancestors2))
		{
			return;
		}
		GroupByIntoOp obj = (GroupByIntoOp)definingGroupNode.Op;
		PlanCompiler.Assert(obj.Inputs.Count == 1, "There should be one input var to GroupByInto at this stage");
		Var first = obj.Inputs.First;
		FunctionOp functionOp = (FunctionOp)candidate.Key.Op;
		Dictionary<Var, Var> dictionary = new Dictionary<Var, Var>(1);
		dictionary.Add(groupAggregateVarInfo.GroupAggregateVar, first);
		VarRemapper varRemapper = new VarRemapper(m_command, dictionary);
		List<Node> list = new List<Node>(candidate.Value.Count);
		foreach (Node item2 in candidate.Value)
		{
			Node node = OpCopier.Copy(m_command, item2);
			varRemapper.RemapSubtree(node);
			list.Add(node);
		}
		Node definingExpr = m_command.CreateNode(m_command.CreateAggregateOp(functionOp.Function, distinctAgg: false), list);
		Var computedVar;
		Node item = m_command.CreateVarDefNode(definingExpr, out computedVar);
		definingGroupNode.Child2.Children.Add(item);
		((GroupByIntoOp)definingGroupNode.Op).Outputs.Set(computedVar);
		for (int i = 0; i < ancestors2.Count; i++)
		{
			Node node2 = ancestors2[i];
			if (node2.Op.OpType == OpType.Project)
			{
				((ProjectOp)node2.Op).Outputs.Set(computedVar);
			}
		}
		candidate.Key.Op = m_command.CreateVarRefOp(computedVar);
		candidate.Key.Children.Clear();
	}

	private static bool AreAllNodesSupportedForPropagation(IList<Node> nodes)
	{
		foreach (Node node in nodes)
		{
			if (node.Op.OpType != OpType.Project && node.Op.OpType != OpType.Filter && node.Op.OpType != OpType.ConstrainedSort)
			{
				return false;
			}
		}
		return true;
	}

	private void FindPathsToLeastCommonAncestor(Node node1, Node node2, out IList<Node> ancestors1, out IList<Node> ancestors2)
	{
		ancestors1 = FindAncestors(node1);
		ancestors2 = FindAncestors(node2);
		int num = ancestors1.Count - 1;
		int num2 = ancestors2.Count - 1;
		while (ancestors1[num] == ancestors2[num2])
		{
			num--;
			num2--;
		}
		for (int num3 = ancestors1.Count - 1; num3 > num; num3--)
		{
			ancestors1.RemoveAt(num3);
		}
		for (int num4 = ancestors2.Count - 1; num4 > num2; num4--)
		{
			ancestors2.RemoveAt(num4);
		}
	}

	private IList<Node> FindAncestors(Node node)
	{
		List<Node> list = new List<Node>();
		Node key = node;
		Node value;
		while (m_tryGetParent(key, out value))
		{
			list.Add(value);
			key = value;
		}
		return list;
	}
}
