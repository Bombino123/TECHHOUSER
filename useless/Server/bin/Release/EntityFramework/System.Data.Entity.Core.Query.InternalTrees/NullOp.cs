using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class NullOp : ConstantBaseOp
{
	internal static readonly NullOp Pattern = new NullOp();

	internal NullOp(TypeUsage type)
		: base(OpType.Null, type, null)
	{
	}

	private NullOp()
		: base(OpType.Null)
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
