using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class GroupByOpRules
{
	internal class VarRefReplacer : BasicOpVisitorOfNode
	{
		private readonly Dictionary<Var, Node> m_varReplacementTable;

		private readonly Command m_command;

		private VarRefReplacer(Dictionary<Var, Node> varReplacementTable, Command command)
		{
			m_varReplacementTable = varReplacementTable;
			m_command = command;
		}

		internal static Node Replace(Dictionary<Var, Node> varReplacementTable, Node root, Command command)
		{
			return new VarRefReplacer(varReplacementTable, command).VisitNode(root);
		}

		public override Node Visit(VarRefOp op, Node n)
		{
			if (m_varReplacementTable.TryGetValue(op.Var, out var value))
			{
				return value;
			}
			return n;
		}

		protected override Node VisitDefault(Node n)
		{
			Node node = base.VisitDefault(n);
			m_command.RecomputeNodeInfo(node);
			return node;
		}
	}

	internal class VarRefUsageFinder : BasicOpVisitor
	{
		private bool m_anyUsedMoreThenOnce;

		private readonly VarVec m_varVec;

		private readonly VarVec m_usedVars;

		private VarRefUsageFinder(VarVec varVec, Command command)
		{
			m_varVec = varVec;
			m_usedVars = command.CreateVarVec();
		}

		internal static bool AnyVarUsedMoreThanOnce(VarVec varVec, Node root, Command command)
		{
			VarRefUsageFinder varRefUsageFinder = new VarRefUsageFinder(varVec, command);
			varRefUsageFinder.VisitNode(root);
			return varRefUsageFinder.m_anyUsedMoreThenOnce;
		}

		public override void Visit(VarRefOp op, Node n)
		{
			Var var = op.Var;
			if (m_varVec.IsSet(var))
			{
				if (m_usedVars.IsSet(var))
				{
					m_anyUsedMoreThenOnce = true;
				}
				else
				{
					m_usedVars.Set(var);
				}
			}
		}

		protected override void VisitChildren(Node n)
		{
			if (!m_anyUsedMoreThenOnce)
			{
				base.VisitChildren(n);
			}
		}
	}

	internal static readonly SimpleRule Rule_GroupByOpWithSimpleVarRedefinitions = new SimpleRule(OpType.GroupBy, ProcessGroupByWithSimpleVarRedefinitions);

	internal static readonly SimpleRule Rule_GroupByOpOnAllInputColumnsWithAggregateOperation = new SimpleRule(OpType.GroupBy, ProcessGroupByOpOnAllInputColumnsWithAggregateOperation);

	internal static readonly PatternMatchRule Rule_GroupByOverProject = new PatternMatchRule(new Node(GroupByOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), ProcessGroupByOverProject);

	internal static readonly PatternMatchRule Rule_GroupByOpWithNoAggregates = new PatternMatchRule(new Node(GroupByOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern), new Node(VarDefListOp.Pattern)), ProcessGroupByOpWithNoAggregates);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[4] { Rule_GroupByOpWithSimpleVarRedefinitions, Rule_GroupByOverProject, Rule_GroupByOpWithNoAggregates, Rule_GroupByOpOnAllInputColumnsWithAggregateOperation };

	private static bool ProcessGroupByWithSimpleVarRedefinitions(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		GroupByOp groupByOp = (GroupByOp)n.Op;
		if (n.Child1.Children.Count == 0)
		{
			return false;
		}
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Command command = transformationRulesContext.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(n);
		bool flag = false;
		foreach (Node child3 in n.Child1.Children)
		{
			Node child = child3.Child0;
			if (child.Op.OpType == OpType.VarRef)
			{
				VarRefOp varRefOp = (VarRefOp)child.Op;
				if (!extendedNodeInfo.ExternalReferences.IsSet(varRefOp.Var))
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		List<Node> list = new List<Node>();
		foreach (Node child4 in n.Child1.Children)
		{
			VarDefOp varDefOp = (VarDefOp)child4.Op;
			if (child4.Child0.Op is VarRefOp varRefOp2 && !extendedNodeInfo.ExternalReferences.IsSet(varRefOp2.Var))
			{
				groupByOp.Outputs.Clear(varDefOp.Var);
				groupByOp.Outputs.Set(varRefOp2.Var);
				groupByOp.Keys.Clear(varDefOp.Var);
				groupByOp.Keys.Set(varRefOp2.Var);
				transformationRulesContext.AddVarMapping(varDefOp.Var, varRefOp2.Var);
			}
			else
			{
				list.Add(child4);
			}
		}
		Node child2 = command.CreateNode(command.CreateVarDefListOp(), list);
		n.Child1 = child2;
		return true;
	}

	private static bool ProcessGroupByOpOnAllInputColumnsWithAggregateOperation(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		if (!(context.Command.Root.Op is PhysicalProjectOp physicalProjectOp) || physicalProjectOp.Outputs.Count > 1)
		{
			return false;
		}
		if (n.Child0.Op.OpType != OpType.ScanTable)
		{
			return false;
		}
		if (n.Child2 == null || n.Child2.Child0 == null || n.Child2.Child0.Child0 == null || n.Child2.Child0.Child0.Op.OpType != OpType.Aggregate)
		{
			return false;
		}
		GroupByOp groupByOp = (GroupByOp)n.Op;
		Table table = ((ScanTableOp)n.Child0.Op).Table;
		VarList columns = table.Columns;
		foreach (Var item in columns)
		{
			if (!groupByOp.Keys.IsSet(item))
			{
				return false;
			}
		}
		foreach (Var item2 in columns)
		{
			groupByOp.Outputs.Clear(item2);
			groupByOp.Keys.Clear(item2);
		}
		Command command = context.Command;
		ScanTableOp scanTableOp = command.CreateScanTableOp(table.TableMetadata);
		Node arg = command.CreateNode(scanTableOp);
		Node arg2 = command.CreateNode(command.CreateOuterApplyOp(), arg, n);
		Var computedVar;
		Node arg3 = command.CreateVarDefListNode(command.CreateNode(command.CreateVarRefOp(groupByOp.Outputs.First)), out computedVar);
		newNode = command.CreateNode(command.CreateProjectOp(computedVar), arg2, arg3);
		Node node = null;
		IEnumerator<Var> enumerator2 = scanTableOp.Table.Keys.GetEnumerator();
		IEnumerator<Var> enumerator3 = table.Keys.GetEnumerator();
		for (int i = 0; i < table.Keys.Count; i++)
		{
			enumerator2.MoveNext();
			enumerator3.MoveNext();
			Node node2 = command.CreateNode(command.CreateComparisonOp(OpType.EQ), command.CreateNode(command.CreateVarRefOp(enumerator2.Current)), command.CreateNode(command.CreateVarRefOp(enumerator3.Current)));
			node = ((node == null) ? node2 : command.CreateNode(command.CreateConditionalOp(OpType.And), node, node2));
		}
		Node child = command.CreateNode(command.CreateFilterOp(), n.Child0, node);
		n.Child0 = child;
		return true;
	}

	private static bool ProcessGroupByOverProject(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		GroupByOp groupByOp = (GroupByOp)n.Op;
		Command command = context.Command;
		Node child = n.Child0;
		Node child2 = child.Child1;
		Node child3 = n.Child1;
		Node child4 = n.Child2;
		if (child3.Children.Count > 0)
		{
			return false;
		}
		VarVec varVec = command.GetExtendedNodeInfo(child).LocalDefinitions;
		if (groupByOp.Outputs.Overlaps(varVec))
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < child2.Children.Count; i++)
		{
			Node node = child2.Children[i];
			if (node.Child0.Op.OpType == OpType.Constant || node.Child0.Op.OpType == OpType.InternalConstant || node.Child0.Op.OpType == OpType.NullSentinel)
			{
				if (!flag)
				{
					varVec = command.CreateVarVec(varVec);
					flag = true;
				}
				varVec.Clear(((VarDefOp)node.Op).Var);
			}
		}
		if (VarRefUsageFinder.AnyVarUsedMoreThanOnce(varVec, child4, command))
		{
			return false;
		}
		Dictionary<Var, Node> dictionary = new Dictionary<Var, Node>(child2.Children.Count);
		for (int j = 0; j < child2.Children.Count; j++)
		{
			Node node2 = child2.Children[j];
			Var var = ((VarDefOp)node2.Op).Var;
			dictionary.Add(var, node2.Child0);
		}
		newNode.Child2 = VarRefReplacer.Replace(dictionary, child4, command);
		newNode.Child0 = child.Child0;
		return true;
	}

	private static bool ProcessGroupByOpWithNoAggregates(RuleProcessingContext context, Node n, out Node newNode)
	{
		Command command = context.Command;
		GroupByOp groupByOp = (GroupByOp)n.Op;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(n.Child0);
		ProjectOp op = command.CreateProjectOp(groupByOp.Keys);
		newNode = command.CreateNode(op, n.Child0, n.Child1);
		if (extendedNodeInfo.Keys.NoKeys || !groupByOp.Keys.Subsumes(extendedNodeInfo.Keys.KeyVars))
		{
			newNode = command.CreateNode(command.CreateDistinctOp(command.CreateVarVec(groupByOp.Keys)), newNode);
		}
		return true;
	}
}
