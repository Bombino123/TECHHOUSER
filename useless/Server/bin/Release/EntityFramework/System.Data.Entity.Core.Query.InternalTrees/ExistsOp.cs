using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ExistsOp : ScalarOp
{
	internal static readonly ExistsOp Pattern = new ExistsOp();

	internal override int Arity => 1;

	internal ExistsOp(TypeUsage type)
		: base(OpType.Exists, type)
	{
	}

	private ExistsOp()
		: base(OpType.Exists)
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
