using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class ConstantPredicateOp : ConstantBaseOp
{
	internal static readonly ConstantPredicateOp Pattern = new ConstantPredicateOp();

	internal new bool Value => (bool)base.Value;

	internal bool IsTrue => Value;

	internal bool IsFalse => !Value;

	internal ConstantPredicateOp(TypeUsage type, bool value)
		: base(OpType.ConstantPredicate, type, value)
	{
	}

	private ConstantPredicateOp()
		: base(OpType.ConstantPredicate)
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
