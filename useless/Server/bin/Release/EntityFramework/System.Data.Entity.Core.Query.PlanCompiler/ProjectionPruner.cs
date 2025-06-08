using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class ProjectionPruner : BasicOpVisitorOfNode
{
	private class ColumnMapVarTracker : ColumnMapVisitor<VarVec>
	{
		internal static void FindVars(ColumnMap columnMap, VarVec vec)
		{
			ColumnMapVarTracker visitor = new ColumnMapVarTracker();
			columnMap.Accept(visitor, vec);
		}

		private ColumnMapVarTracker()
		{
		}

		internal override void Visit(VarRefColumnMap columnMap, VarVec arg)
		{
			arg.Set(columnMap.Var);
			base.Visit(columnMap, arg);
		}
	}

	private readonly PlanCompiler m_compilerState;

	private readonly VarVec m_referencedVars;

	private Command m_command => m_compilerState.Command;

	private ProjectionPruner(PlanCompiler compilerState)
	{
		m_compilerState = compilerState;
		m_referencedVars = compilerState.Command.CreateVarVec();
	}

	internal static void Process(PlanCompiler compilerState)
	{
		compilerState.Command.Root = Process(compilerState, compilerState.Command.Root);
	}

	internal static Node Process(PlanCompiler compilerState, Node node)
	{
		return new ProjectionPruner(compilerState).Process(node);
	}

	private Node Process(Node node)
	{
		return VisitNode(node);
	}

	private void AddReference(Var v)
	{
		m_referencedVars.Set(v);
	}

	private void AddReference(IEnumerable<Var> varSet)
	{
		foreach (Var item in varSet)
		{
			AddReference(item);
		}
	}

	private bool IsReferenced(Var v)
	{
		return m_referencedVars.IsSet(v);
	}

	private bool IsUnreferenced(Var v)
	{
		return !IsReferenced(v);
	}

	private void PruneVarMap(VarMap varMap)
	{
		List<Var> list = new List<Var>();
		foreach (Var key in varMap.Keys)
		{
			if (!IsReferenced(key))
			{
				list.Add(key);
			}
			else
			{
				AddReference(varMap[key]);
			}
		}
		foreach (Var item in list)
		{
			varMap.Remove(item);
		}
	}

	private void PruneVarSet(VarVec varSet)
	{
		varSet.And(m_referencedVars);
	}

	protected override void VisitChildren(Node n)
	{
		base.VisitChildren(n);
		m_command.RecomputeNodeInfo(n);
	}

	protected override void VisitChildrenReverse(Node n)
	{
		base.VisitChildrenReverse(n);
		m_command.RecomputeNodeInfo(n);
	}

	public override Node Visit(VarDefListOp op, Node n)
	{
		List<Node> list = new List<Node>();
		foreach (Node child in n.Children)
		{
			VarDefOp varDefOp = child.Op as VarDefOp;
			if (IsReferenced(varDefOp.Var))
			{
				list.Add(VisitNode(child));
			}
		}
		return m_command.CreateNode(op, list);
	}

	public override Node Visit(PhysicalProjectOp op, Node n)
	{
		if (n == m_command.Root)
		{
			ColumnMapVarTracker.FindVars(op.ColumnMap, m_referencedVars);
			op.Outputs.RemoveAll(IsUnreferenced);
		}
		else
		{
			AddReference(op.Outputs);
		}
		VisitChildren(n);
		return n;
	}

	protected override Node VisitNestOp(NestBaseOp op, Node n)
	{
		AddReference(op.Outputs);
		VisitChildren(n);
		return n;
	}

	public override Node Visit(SingleStreamNestOp op, Node n)
	{
		AddReference(op.Discriminator);
		return VisitNestOp(op, n);
	}

	public override Node Visit(MultiStreamNestOp op, Node n)
	{
		return VisitNestOp(op, n);
	}

	protected override Node VisitApplyOp(ApplyBaseOp op, Node n)
	{
		VisitChildrenReverse(n);
		return n;
	}

	public override Node Visit(DistinctOp op, Node n)
	{
		if (op.Keys.Count > 1 && n.Child0.Op.OpType == OpType.Project)
		{
			RemoveRedundantConstantKeys(op.Keys, ((ProjectOp)n.Child0.Op).Outputs, n.Child0.Child1);
		}
		AddReference(op.Keys);
		VisitChildren(n);
		return n;
	}

	public override Node Visit(ElementOp op, Node n)
	{
		ExtendedNodeInfo extendedNodeInfo = m_command.GetExtendedNodeInfo(n.Child0);
		AddReference(extendedNodeInfo.Definitions);
		n.Child0 = VisitNode(n.Child0);
		m_command.RecomputeNodeInfo(n);
		return n;
	}

	public override Node Visit(FilterOp op, Node n)
	{
		VisitChildrenReverse(n);
		return n;
	}

	protected override Node VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		for (int num = n.Children.Count - 1; num >= 2; num--)
		{
			n.Children[num] = VisitNode(n.Children[num]);
		}
		if (op.Keys.Count > 1)
		{
			RemoveRedundantConstantKeys(op.Keys, op.Outputs, n.Child1);
		}
		AddReference(op.Keys);
		n.Children[1] = VisitNode(n.Children[1]);
		n.Children[0] = VisitNode(n.Children[0]);
		PruneVarSet(op.Outputs);
		if (op.Keys.Count == 0 && op.Outputs.Count == 0)
		{
			return m_command.CreateNode(m_command.CreateSingleRowTableOp());
		}
		m_command.RecomputeNodeInfo(n);
		return n;
	}

	private void RemoveRedundantConstantKeys(VarVec keyVec, VarVec outputVec, Node varDefListNode)
	{
		List<Node> constantKeys = varDefListNode.Children.Where((Node d) => d.Op.OpType == OpType.VarDef && PlanCompilerUtil.IsConstantBaseOp(d.Child0.Op.OpType)).ToList();
		VarVec constantKeyVars = m_command.CreateVarVec(constantKeys.Select((Node d) => ((VarDefOp)d.Op).Var));
		constantKeyVars.Minus(m_referencedVars);
		keyVec.Minus(constantKeyVars);
		outputVec.Minus(constantKeyVars);
		varDefListNode.Children.RemoveAll((Node c) => constantKeys.Contains(c) && constantKeyVars.IsSet(((VarDefOp)c.Op).Var));
		if (keyVec.Count == 0)
		{
			Node node = constantKeys.First();
			Var var = ((VarDefOp)node.Op).Var;
			keyVec.Set(var);
			outputVec.Set(var);
			varDefListNode.Children.Add(node);
		}
	}

	public override Node Visit(GroupByIntoOp op, Node n)
	{
		Node node = VisitGroupByOp(op, n);
		if (node.Op.OpType == OpType.GroupByInto && n.Child3.Children.Count == 0)
		{
			GroupByIntoOp groupByIntoOp = (GroupByIntoOp)node.Op;
			node = m_command.CreateNode(m_command.CreateGroupByOp(groupByIntoOp.Keys, groupByIntoOp.Outputs), node.Child0, node.Child1, node.Child2);
		}
		return node;
	}

	protected override Node VisitJoinOp(JoinBaseOp op, Node n)
	{
		if (n.Op.OpType == OpType.CrossJoin)
		{
			VisitChildren(n);
			return n;
		}
		n.Child2 = VisitNode(n.Child2);
		n.Child0 = VisitNode(n.Child0);
		n.Child1 = VisitNode(n.Child1);
		m_command.RecomputeNodeInfo(n);
		return n;
	}

	public override Node Visit(ProjectOp op, Node n)
	{
		PruneVarSet(op.Outputs);
		VisitChildrenReverse(n);
		if (!op.Outputs.IsEmpty)
		{
			return n;
		}
		return n.Child0;
	}

	public override Node Visit(ScanTableOp op, Node n)
	{
		PlanCompiler.Assert(!n.HasChild0, "scanTable with an input?");
		op.Table.ReferencedColumns.And(m_referencedVars);
		m_command.RecomputeNodeInfo(n);
		return n;
	}

	protected override Node VisitSetOp(SetOp op, Node n)
	{
		if (OpType.Intersect == op.OpType || OpType.Except == op.OpType)
		{
			AddReference(op.Outputs);
		}
		PruneVarSet(op.Outputs);
		VarMap[] varMap = op.VarMap;
		foreach (VarMap varMap2 in varMap)
		{
			PruneVarMap(varMap2);
		}
		VisitChildren(n);
		return n;
	}

	protected override Node VisitSortOp(SortBaseOp op, Node n)
	{
		foreach (SortKey key in op.Keys)
		{
			AddReference(key.Var);
		}
		if (n.HasChild1)
		{
			n.Child1 = VisitNode(n.Child1);
		}
		n.Child0 = VisitNode(n.Child0);
		m_command.RecomputeNodeInfo(n);
		return n;
	}

	public override Node Visit(UnnestOp op, Node n)
	{
		AddReference(op.Var);
		VisitChildren(n);
		return n;
	}

	public override Node Visit(VarRefOp op, Node n)
	{
		AddReference(op.Var);
		return n;
	}

	public override Node Visit(ExistsOp op, Node n)
	{
		ProjectOp projectOp = (ProjectOp)n.Child0.Op;
		AddReference(projectOp.Outputs.First);
		VisitChildren(n);
		return n;
	}
}
