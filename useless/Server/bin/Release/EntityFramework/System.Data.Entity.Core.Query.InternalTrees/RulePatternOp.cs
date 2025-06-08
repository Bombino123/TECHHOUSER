namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class RulePatternOp : Op
{
	internal override bool IsRulePatternOp => true;

	internal RulePatternOp(OpType opType)
		: base(opType)
	{
	}
}
