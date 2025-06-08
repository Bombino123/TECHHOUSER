using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class SortOpRules
{
	internal static readonly SimpleRule Rule_SortOpOverAtMostOneRow = new SimpleRule(OpType.Sort, ProcessSortOpOverAtMostOneRow);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[1] { Rule_SortOpOverAtMostOneRow };

	private static bool ProcessSortOpOverAtMostOneRow(RuleProcessingContext context, Node n, out Node newNode)
	{
		ExtendedNodeInfo extendedNodeInfo = context.Command.GetExtendedNodeInfo(n.Child0);
		if (extendedNodeInfo.MaxRows == RowCount.Zero || extendedNodeInfo.MaxRows == RowCount.One)
		{
			newNode = n.Child0;
			return true;
		}
		newNode = n;
		return false;
	}
}
