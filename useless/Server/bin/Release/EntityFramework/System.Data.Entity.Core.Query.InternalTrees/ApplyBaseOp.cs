namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ApplyBaseOp : RelOp
{
	internal override int Arity => 2;

	internal ApplyBaseOp(OpType opType)
		: base(opType)
	{
	}
}
