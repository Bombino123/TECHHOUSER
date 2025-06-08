using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class InternalConstantOp : ConstantBaseOp
{
	internal static readonly InternalConstantOp Pattern = new InternalConstantOp();

	internal InternalConstantOp(TypeUsage type, object value)
		: base(OpType.InternalConstant, type, value)
	{
	}

	private InternalConstantOp()
		: base(OpType.InternalConstant)
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
