namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class RelOp : Op
{
	internal override bool IsRelOp => true;

	internal RelOp(OpType opType)
		: base(opType)
	{
	}
}
