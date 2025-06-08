using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class SetOpRules
{
	internal static readonly SimpleRule Rule_UnionAllOverEmptySet = new SimpleRule(OpType.UnionAll, ProcessSetOpOverEmptySet);

	internal static readonly SimpleRule Rule_IntersectOverEmptySet = new SimpleRule(OpType.Intersect, ProcessSetOpOverEmptySet);

	internal static readonly SimpleRule Rule_ExceptOverEmptySet = new SimpleRule(OpType.Except, ProcessSetOpOverEmptySet);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[3] { Rule_UnionAllOverEmptySet, Rule_IntersectOverEmptySet, Rule_ExceptOverEmptySet };

	private static bool ProcessSetOpOverEmptySet(RuleProcessingContext context, Node setOpNode, out Node newNode)
	{
		bool flag = context.Command.GetExtendedNodeInfo(setOpNode.Child0).MaxRows == RowCount.Zero;
		bool flag2 = context.Command.GetExtendedNodeInfo(setOpNode.Child1).MaxRows == RowCount.Zero;
		if (!flag && !flag2)
		{
			newNode = setOpNode;
			return false;
		}
		SetOp setOp = (SetOp)setOpNode.Op;
		int num = (((!flag2 && setOp.OpType == OpType.UnionAll) || (!flag && setOp.OpType == OpType.Intersect)) ? 1 : 0);
		newNode = setOpNode.Children[num];
		TransformationRulesContext transformationRulesContext = (TransformationRulesContext)context;
		foreach (KeyValuePair<Var, Var> item in setOp.VarMap[num])
		{
			transformationRulesContext.AddVarMapping(item.Key, item.Value);
		}
		return true;
	}
}
