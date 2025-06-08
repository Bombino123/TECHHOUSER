using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class JoinElimination : BasicOpVisitorOfNode
{
	private readonly PlanCompiler m_compilerState;

	private readonly Dictionary<Node, Node> m_joinGraphUnnecessaryMap = new Dictionary<Node, Node>();

	private readonly VarRemapper m_varRemapper;

	private bool m_treeModified;

	private readonly VarRefManager m_varRefManager;

	private Command Command => m_compilerState.Command;

	private ConstraintManager ConstraintManager => m_compilerState.ConstraintManager;

	private JoinElimination(PlanCompiler compilerState)
	{
		m_compilerState = compilerState;
		m_varRemapper = new VarRemapper(m_compilerState.Command);
		m_varRefManager = new VarRefManager(m_compilerState.Command);
	}

	internal static bool Process(PlanCompiler compilerState)
	{
		JoinElimination joinElimination = new JoinElimination(compilerState);
		joinElimination.Process();
		return joinElimination.m_treeModified;
	}

	private void Process()
	{
		Command.Root = VisitNode(Command.Root);
	}

	private bool NeedsJoinGraph(Node joinNode)
	{
		return !m_joinGraphUnnecessaryMap.ContainsKey(joinNode);
	}

	private Node ProcessJoinGraph(Node joinNode)
	{
		VarMap varMap;
		Dictionary<Node, Node> processedNodes;
		Node result = new JoinGraph(Command, ConstraintManager, m_varRefManager, joinNode).DoJoinElimination(out varMap, out processedNodes);
		foreach (KeyValuePair<Var, Var> item in varMap)
		{
			m_varRemapper.AddMapping(item.Key, item.Value);
		}
		foreach (Node key in processedNodes.Keys)
		{
			m_joinGraphUnnecessaryMap[key] = key;
		}
		return result;
	}

	private Node VisitDefaultForAllNodes(Node n)
	{
		VisitChildren(n);
		m_varRemapper.RemapNode(n);
		Command.RecomputeNodeInfo(n);
		return n;
	}

	protected override Node VisitDefault(Node n)
	{
		m_varRefManager.AddChildren(n);
		return VisitDefaultForAllNodes(n);
	}

	protected override Node VisitJoinOp(JoinBaseOp op, Node joinNode)
	{
		Node node;
		if (NeedsJoinGraph(joinNode))
		{
			node = ProcessJoinGraph(joinNode);
			if (node != joinNode)
			{
				m_treeModified = true;
			}
		}
		else
		{
			node = joinNode;
		}
		return VisitDefaultForAllNodes(node);
	}
}
