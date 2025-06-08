using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class NewMultisetOp : ScalarOp
{
	internal static readonly NewMultisetOp Pattern = new NewMultisetOp();

	internal NewMultisetOp(TypeUsage type)
		: base(OpType.NewMultiset, type)
	{
	}

	private NewMultisetOp()
		: base(OpType.NewMultiset)
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
