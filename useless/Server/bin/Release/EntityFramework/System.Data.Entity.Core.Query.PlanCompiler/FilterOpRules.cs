using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class FilterOpRules
{
	internal static readonly PatternMatchRule Rule_FilterOverFilter = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverFilter);

	internal static readonly PatternMatchRule Rule_FilterOverProject = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverProject);

	internal static readonly PatternMatchRule Rule_FilterOverUnionAll = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(UnionAllOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverSetOp);

	internal static readonly PatternMatchRule Rule_FilterOverIntersect = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(IntersectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverSetOp);

	internal static readonly PatternMatchRule Rule_FilterOverExcept = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(ExceptOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverSetOp);

	internal static readonly PatternMatchRule Rule_FilterOverDistinct = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(DistinctOp.Pattern, new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverDistinct);

	internal static readonly PatternMatchRule Rule_FilterOverGroupBy = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(GroupByOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverGroupBy);

	internal static readonly PatternMatchRule Rule_FilterOverCrossJoin = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(CrossJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverJoin);

	internal static readonly PatternMatchRule Rule_FilterOverInnerJoin = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(InnerJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverJoin);

	internal static readonly PatternMatchRule Rule_FilterOverLeftOuterJoin = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(LeftOuterJoinOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverJoin);

	internal static readonly PatternMatchRule Rule_FilterOverOuterApply = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(OuterApplyOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern)), new Node(LeafOp.Pattern)), ProcessFilterOverOuterApply);

	internal static readonly PatternMatchRule Rule_FilterWithConstantPredicate = new PatternMatchRule(new Node(FilterOp.Pattern, new Node(LeafOp.Pattern), new Node(ConstantPredicateOp.Pattern)), ProcessFilterWithConstantPredicate);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[12]
	{
		Rule_FilterWithConstantPredicate, Rule_FilterOverCrossJoin, Rule_FilterOverDistinct, Rule_FilterOverExcept, Rule_FilterOverFilter, Rule_FilterOverGroupBy, Rule_FilterOverInnerJoin, Rule_FilterOverIntersect, Rule_FilterOverLeftOuterJoin, Rule_FilterOverProject,
		Rule_FilterOverUnionAll, Rule_FilterOverOuterApply
	};

	private static Node GetPushdownPredicate(Command command, Node filterNode, VarVec columns, out Node nonPushdownPredicateNode)
	{
		Node child = filterNode.Child1;
		nonPushdownPredicateNode = null;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(filterNode);
		if (columns == null && extendedNodeInfo.ExternalReferences.IsEmpty)
		{
			return child;
		}
		if (columns == null)
		{
			columns = command.GetExtendedNodeInfo(filterNode.Child0).Definitions;
		}
		child = new Predicate(command, child).GetSingleTablePredicates(columns, out var otherPredicates).BuildAndTree();
		nonPushdownPredicateNode = otherPredicates.BuildAndTree();
		return child;
	}

	private static bool ProcessFilterOverFilter(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		Node arg = context.Command.CreateNode(context.Command.CreateConditionalOp(OpType.And), filterNode.Child0.Child1, filterNode.Child1);
		newNode = context.Command.CreateNode(context.Command.CreateFilterOp(), filterNode.Child0.Child0, arg);
		return true;
	}

	private static bool ProcessFilterOverProject(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		newNode = filterNode;
		Node child = filterNode.Child1;
		if (child.Op.OpType == OpType.ConstantPredicate)
		{
			return false;
		}
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
		if (!transformationRulesContext.IsScalarOpTree(child, varRefMap))
		{
			return false;
		}
		Node child2 = filterNode.Child0;
		Dictionary<Var, Node> varMap = transformationRulesContext.GetVarMap(child2.Child1, varRefMap);
		if (varMap == null)
		{
			return false;
		}
		if (transformationRulesContext.IncludeCustomFunctionOp(child, varMap))
		{
			return false;
		}
		Node arg = transformationRulesContext.ReMap(child, varMap);
		Node arg2 = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateFilterOp(), child2.Child0, arg);
		Node node = transformationRulesContext.Command.CreateNode(child2.Op, arg2, child2.Child1);
		newNode = node;
		return true;
	}

	private static bool ProcessFilterOverSetOp(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		newNode = filterNode;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Node nonPushdownPredicateNode;
		Node pushdownPredicate = GetPushdownPredicate(transformationRulesContext.Command, filterNode, null, out nonPushdownPredicateNode);
		if (pushdownPredicate == null)
		{
			return false;
		}
		if (!transformationRulesContext.IsScalarOpTree(pushdownPredicate))
		{
			return false;
		}
		Node child = filterNode.Child0;
		SetOp setOp = (SetOp)child.Op;
		List<Node> list = new List<Node>();
		int num = 0;
		VarMap[] varMap = setOp.VarMap;
		foreach (VarMap varMap2 in varMap)
		{
			if (setOp.OpType == OpType.Except && num == 1)
			{
				list.Add(child.Child1);
				break;
			}
			Dictionary<Var, Node> dictionary = new Dictionary<Var, Node>();
			foreach (KeyValuePair<Var, Var> item2 in varMap2)
			{
				Node value = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateVarRefOp(item2.Value));
				dictionary.Add(item2.Key, value);
			}
			Node node = pushdownPredicate;
			if (num == 0 && filterNode.Op.OpType != OpType.Except)
			{
				node = transformationRulesContext.Copy(node);
			}
			Node node2 = transformationRulesContext.ReMap(node, dictionary);
			transformationRulesContext.Command.RecomputeNodeInfo(node2);
			Node item = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateFilterOp(), child.Children[num], node2);
			list.Add(item);
			num++;
		}
		Node node3 = transformationRulesContext.Command.CreateNode(child.Op, list);
		if (nonPushdownPredicateNode != null)
		{
			newNode = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateFilterOp(), node3, nonPushdownPredicateNode);
		}
		else
		{
			newNode = node3;
		}
		return true;
	}

	private static bool ProcessFilterOverDistinct(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		newNode = filterNode;
		Node nonPushdownPredicateNode;
		Node pushdownPredicate = GetPushdownPredicate(context.Command, filterNode, null, out nonPushdownPredicateNode);
		if (pushdownPredicate == null)
		{
			return false;
		}
		Node child = filterNode.Child0;
		Node arg = context.Command.CreateNode(context.Command.CreateFilterOp(), child.Child0, pushdownPredicate);
		Node node = context.Command.CreateNode(child.Op, arg);
		if (nonPushdownPredicateNode != null)
		{
			newNode = context.Command.CreateNode(context.Command.CreateFilterOp(), node, nonPushdownPredicateNode);
		}
		else
		{
			newNode = node;
		}
		return true;
	}

	private static bool ProcessFilterOverGroupBy(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		newNode = filterNode;
		Node child = filterNode.Child0;
		GroupByOp groupByOp = (GroupByOp)child.Op;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
		if (!transformationRulesContext.IsScalarOpTree(filterNode.Child1, varRefMap))
		{
			return false;
		}
		Node nonPushdownPredicateNode;
		Node pushdownPredicate = GetPushdownPredicate(context.Command, filterNode, groupByOp.Keys, out nonPushdownPredicateNode);
		if (pushdownPredicate == null)
		{
			return false;
		}
		Dictionary<Var, Node> varMap = transformationRulesContext.GetVarMap(child.Child1, varRefMap);
		if (varMap == null)
		{
			return false;
		}
		Node arg = transformationRulesContext.ReMap(pushdownPredicate, varMap);
		Node arg2 = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateFilterOp(), child.Child0, arg);
		Node node = transformationRulesContext.Command.CreateNode(child.Op, arg2, child.Child1, child.Child2);
		if (nonPushdownPredicateNode == null)
		{
			newNode = node;
		}
		else
		{
			newNode = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateFilterOp(), node, nonPushdownPredicateNode);
		}
		return true;
	}

	private static bool ProcessFilterOverJoin(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		newNode = filterNode;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		if (transformationRulesContext.IsFilterPushdownSuppressed(filterNode))
		{
			return false;
		}
		Node child = filterNode.Child0;
		Op op = child.Op;
		Node node = child.Child0;
		Node node2 = child.Child1;
		Command command = transformationRulesContext.Command;
		bool flag = false;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(node2);
		Predicate otherPredicates = new Predicate(command, filterNode.Child1);
		if (op.OpType == OpType.LeftOuterJoin && !otherPredicates.PreservesNulls(extendedNodeInfo.Definitions, ansiNullSemantics: true))
		{
			if (transformationRulesContext.PlanCompiler.IsAfterPhase(PlanCompilerPhase.NullSemantics) && transformationRulesContext.PlanCompiler.IsAfterPhase(PlanCompilerPhase.JoinElimination))
			{
				op = command.CreateInnerJoinOp();
				flag = true;
			}
			else
			{
				transformationRulesContext.PlanCompiler.TransformationsDeferred = true;
			}
		}
		ExtendedNodeInfo extendedNodeInfo2 = command.GetExtendedNodeInfo(node);
		Node node3 = null;
		if (node.Op.OpType != OpType.ScanTable)
		{
			node3 = otherPredicates.GetSingleTablePredicates(extendedNodeInfo2.Definitions, out otherPredicates).BuildAndTree();
		}
		Node node4 = null;
		if (node2.Op.OpType != OpType.ScanTable && op.OpType != OpType.LeftOuterJoin)
		{
			node4 = otherPredicates.GetSingleTablePredicates(extendedNodeInfo.Definitions, out otherPredicates).BuildAndTree();
		}
		Node node5 = null;
		if (op.OpType == OpType.CrossJoin || op.OpType == OpType.InnerJoin)
		{
			node5 = otherPredicates.GetJoinPredicates(extendedNodeInfo2.Definitions, extendedNodeInfo.Definitions, out otherPredicates).BuildAndTree();
		}
		if (node3 != null)
		{
			node = command.CreateNode(command.CreateFilterOp(), node, node3);
			flag = true;
		}
		if (node4 != null)
		{
			node2 = command.CreateNode(command.CreateFilterOp(), node2, node4);
			flag = true;
		}
		if (node5 != null)
		{
			flag = true;
			if (op.OpType == OpType.CrossJoin)
			{
				op = command.CreateInnerJoinOp();
			}
			else
			{
				PlanCompiler.Assert(op.OpType == OpType.InnerJoin, "unexpected non-InnerJoin?");
				node5 = PlanCompilerUtil.CombinePredicates(child.Child2, node5, command);
			}
		}
		else
		{
			node5 = ((op.OpType == OpType.CrossJoin) ? null : child.Child2);
		}
		if (!flag)
		{
			return false;
		}
		Node node6 = ((op.OpType != OpType.CrossJoin) ? command.CreateNode(op, node, node2, node5) : command.CreateNode(op, node, node2));
		Node node7 = otherPredicates.BuildAndTree();
		if (node7 == null)
		{
			newNode = node6;
		}
		else
		{
			newNode = command.CreateNode(command.CreateFilterOp(), node6, node7);
		}
		return true;
	}

	private static bool ProcessFilterOverOuterApply(RuleProcessingContext context, Node filterNode, out Node newNode)
	{
		newNode = filterNode;
		Node child = filterNode.Child0;
		Node child2 = child.Child1;
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		Command command = transformationRulesContext.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(child2);
		if (!new Predicate(command, filterNode.Child1).PreservesNulls(extendedNodeInfo.Definitions, ansiNullSemantics: true))
		{
			if (transformationRulesContext.PlanCompiler.IsAfterPhase(PlanCompilerPhase.NullSemantics) && transformationRulesContext.PlanCompiler.IsAfterPhase(PlanCompilerPhase.JoinElimination))
			{
				Node arg = command.CreateNode(command.CreateCrossApplyOp(), child.Child0, child2);
				Node node = command.CreateNode(command.CreateFilterOp(), arg, filterNode.Child1);
				newNode = node;
				return true;
			}
			transformationRulesContext.PlanCompiler.TransformationsDeferred = true;
		}
		return false;
	}

	private static bool ProcessFilterWithConstantPredicate(RuleProcessingContext context, Node n, out Node newNode)
	{
		newNode = n;
		ConstantPredicateOp constantPredicateOp = (ConstantPredicateOp)n.Child1.Op;
		if (constantPredicateOp.IsTrue)
		{
			newNode = n.Child0;
			return true;
		}
		PlanCompiler.Assert(constantPredicateOp.IsFalse, "unexpected non-false predicate?");
		if (n.Child0.Op.OpType == OpType.SingleRowTable || (n.Child0.Op.OpType == OpType.Project && n.Child0.Child0.Op.OpType == OpType.SingleRowTable))
		{
			return false;
		}
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		ExtendedNodeInfo extendedNodeInfo = transformationRulesContext.Command.GetExtendedNodeInfo(n.Child0);
		List<Node> list = new List<Node>();
		VarVec varVec = transformationRulesContext.Command.CreateVarVec();
		foreach (Var definition in extendedNodeInfo.Definitions)
		{
			NullOp op = transformationRulesContext.Command.CreateNullOp(definition.Type);
			Node definingExpr = transformationRulesContext.Command.CreateNode(op);
			Var computedVar;
			Node item = transformationRulesContext.Command.CreateVarDefNode(definingExpr, out computedVar);
			transformationRulesContext.AddVarMapping(definition, computedVar);
			varVec.Set(computedVar);
			list.Add(item);
		}
		if (varVec.IsEmpty)
		{
			NullOp op2 = transformationRulesContext.Command.CreateNullOp(transformationRulesContext.Command.BooleanType);
			Node definingExpr2 = transformationRulesContext.Command.CreateNode(op2);
			Var computedVar2;
			Node item2 = transformationRulesContext.Command.CreateVarDefNode(definingExpr2, out computedVar2);
			varVec.Set(computedVar2);
			list.Add(item2);
		}
		Node child = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateSingleRowTableOp());
		n.Child0 = child;
		Node arg = transformationRulesContext.Command.CreateNode(transformationRulesContext.Command.CreateVarDefListOp(), list);
		ProjectOp op3 = transformationRulesContext.Command.CreateProjectOp(varVec);
		Node node = transformationRulesContext.Command.CreateNode(op3, n, arg);
		node.Child0 = n;
		newNode = node;
		return true;
	}
}
