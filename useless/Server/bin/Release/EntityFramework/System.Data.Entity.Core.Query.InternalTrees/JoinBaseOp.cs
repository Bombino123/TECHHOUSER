namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class JoinBaseOp : RelOp
{
	internal override int Arity => 3;

	internal JoinBaseOp(OpType opType)
		: base(opType)
	{
	}
}
