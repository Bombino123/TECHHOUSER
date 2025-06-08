using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class ConstrainedSortOpRules
{
	internal static readonly SimpleRule Rule_ConstrainedSortOpOverEmptySet = new SimpleRule(OpType.ConstrainedSort, ProcessConstrainedSortOpOverEmptySet);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[1] { Rule_ConstrainedSortOpOverEmptySet };

	private static bool ProcessConstrainedSortOpOverEmptySet(RuleProcessingContext context, Node n, out Node newNode)
	{
		if (context.Command.GetExtendedNodeInfo(n.Child0).MaxRows == RowCount.Zero)
		{
			newNode = n.Child0;
			return true;
		}
		newNode = n;
		return false;
	}
}
