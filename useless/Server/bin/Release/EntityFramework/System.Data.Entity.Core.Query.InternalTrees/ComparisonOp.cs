using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ComparisonOp : ScalarOp
{
	internal static readonly ComparisonOp PatternEq = new ComparisonOp(OpType.EQ);

	internal override int Arity => 2;

	internal bool UseDatabaseNullSemantics { get; set; }

	internal ComparisonOp(OpType opType, TypeUsage type)
		: base(opType, type)
	{
	}

	private ComparisonOp(OpType opType)
		: base(opType)
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
