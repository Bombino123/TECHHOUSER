using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal static class DistinctOpRules
{
	internal static readonly SimpleRule Rule_DistinctOpOfKeys = new SimpleRule(OpType.Distinct, ProcessDistinctOpOfKeys);

	internal static readonly System.Data.Entity.Core.Query.InternalTrees.Rule[] Rules = new System.Data.Entity.Core.Query.InternalTrees.Rule[1] { Rule_DistinctOpOfKeys };

	private static bool ProcessDistinctOpOfKeys(RuleProcessingContext context, Node n, out Node newNode)
	{
		Command command = context.Command;
		ExtendedNodeInfo extendedNodeInfo = command.GetExtendedNodeInfo(n.Child0);
		DistinctOp distinctOp = (DistinctOp)n.Op;
		if (!extendedNodeInfo.Keys.NoKeys && distinctOp.Keys.Subsumes(extendedNodeInfo.Keys.KeyVars))
		{
			ProjectOp op = command.CreateProjectOp(distinctOp.Keys);
			VarDefListOp op2 = command.CreateVarDefListOp();
			Node arg = command.CreateNode(op2);
			newNode = command.CreateNode(op, n.Child0, arg);
			return true;
		}
		newNode = n;
		return false;
	}
}
