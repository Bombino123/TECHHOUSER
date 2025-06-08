namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class AncillaryOp : Op
{
	internal override bool IsAncillaryOp => true;

	internal AncillaryOp(OpType opType)
		: base(opType)
	{
	}
}
