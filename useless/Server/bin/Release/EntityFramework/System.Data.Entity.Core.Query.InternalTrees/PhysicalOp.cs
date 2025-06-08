namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class PhysicalOp : Op
{
	internal override bool IsPhysicalOp => true;

	internal PhysicalOp(OpType opType)
		: base(opType)
	{
	}
}
