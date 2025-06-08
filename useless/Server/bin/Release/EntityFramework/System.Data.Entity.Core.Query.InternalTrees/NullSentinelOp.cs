using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class NullSentinelOp : ConstantBaseOp
{
	internal static readonly NullSentinelOp Pattern = new NullSentinelOp();

	internal NullSentinelOp(TypeUsage type, object value)
		: base(OpType.NullSentinel, type, value)
	{
	}

	private NullSentinelOp()
		: base(OpType.NullSentinel)
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
