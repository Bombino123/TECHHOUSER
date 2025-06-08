using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ConstantOp : ConstantBaseOp
{
	internal static readonly ConstantOp Pattern = new ConstantOp();

	internal ConstantOp(TypeUsage type, object value)
		: base(OpType.Constant, type, value)
	{
	}

	private ConstantOp()
		: base(OpType.Constant)
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
