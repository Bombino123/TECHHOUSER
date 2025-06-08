using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class VarDefListOp : AncillaryOp
{
	internal static readonly VarDefListOp Instance = new VarDefListOp();

	internal static readonly VarDefListOp Pattern = Instance;

	private VarDefListOp()
		: base(OpType.VarDefList)
	{
	}

	[DebuggerNonUserCode]
	internal override void Accept(BasicOpVisitor v, Node n)
	{
		v.Visit(this, n);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
	{
		return v.Visit(this, n);
	}
}
