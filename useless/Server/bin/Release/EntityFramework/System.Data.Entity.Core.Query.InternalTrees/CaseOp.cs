using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class CaseOp : ScalarOp
{
	internal static readonly CaseOp Pattern = new CaseOp();

	internal CaseOp(TypeUsage type)
		: base(OpType.Case, type)
	{
	}

	private CaseOp()
		: base(OpType.Case)
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
