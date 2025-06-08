using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class AggregatePushdownUtil
{
	internal static bool IsVarRefOverGivenVar(Node node, Var var)
	{
		if (node.Op.OpType != OpType.VarRef)
		{
			return false;
		}
		return ((VarRefOp)node.Op).Var == var;
	}
}
