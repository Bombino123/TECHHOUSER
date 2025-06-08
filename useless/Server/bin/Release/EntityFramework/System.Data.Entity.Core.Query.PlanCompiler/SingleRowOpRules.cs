using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class SingleRowOpRules
{
	internal static readonly PatternMatchRule Rule_SingleRowOpOverAnything = new PatternMatchRule(new Node(SingleRowOp.Pattern, new Node(LeafOp.Pattern)), ProcessSingleRowOpOverAnything);

	internal static readonly PatternMatchRule Rule_SingleRowOpOverProject = new PatternMatchRule(new Node(SingleRowOp.Pattern, new Node(ProjectOp.Pattern, new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))), ProcessSingleRowOpOverProject);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[2] { Rule_SingleRowOpOverAnything, Rule_SingleRowOpOverProject };

	private static bool ProcessSingleRowOpOverAnything(RuleProcessingContext context, Node singleRowNode, out Node newNode)
	{
		newNode = singleRowNode;
		_ = (TransformationRulesContext)context;
		ExtendedNodeInfo extendedNodeInfo = context.Command.GetExtendedNodeInfo(singleRowNode.Child0);
		if ((int)extendedNodeInfo.MaxRows <= 1)
		{
			newNode = singleRowNode.Child0;
			return true;
		}
		if (singleRowNode.Child0.Op.OpType == OpType.Filter && new Predicate(context.Command, singleRowNode.Child0.Child1).SatisfiesKey(extendedNodeInfo.Keys.KeyVars, extendedNodeInfo.Definitions))
		{
			extendedNodeInfo.MaxRows = RowCount.One;
			newNode = singleRowNode.Child0;
			return true;
		}
		return false;
	}

	private static bool ProcessSingleRowOpOverProject(RuleProcessingContext context, Node singleRowNode, out Node newNode)
	{
		newNode = singleRowNode;
		Node child = singleRowNode.Child0;
		Node child2 = child.Child0;
		singleRowNode.Child0 = child2;
		context.Command.RecomputeNodeInfo(singleRowNode);
		child.Child0 = singleRowNode;
		newNode = child;
		return true;
	}
}
