namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class SimpleRule : Rule
{
	internal SimpleRule(OpType opType, ProcessNodeDelegate processDelegate)
		: base(opType, processDelegate)
	{
	}

	internal override bool Match(Node node)
	{
		return node.Op.OpType == base.RuleOpType;
	}
}
