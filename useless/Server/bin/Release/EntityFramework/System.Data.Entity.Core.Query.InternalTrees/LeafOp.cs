namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class LeafOp : RulePatternOp
{
	internal static readonly LeafOp Instance = new LeafOp();

	internal static readonly LeafOp Pattern = Instance;

	internal override int Arity => 0;

	private LeafOp()
		: base(OpType.Leaf)
	{
	}
}
