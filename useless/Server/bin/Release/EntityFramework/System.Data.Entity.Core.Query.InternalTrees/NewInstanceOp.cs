using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class NewInstanceOp : ScalarOp
{
	internal static readonly NewInstanceOp Pattern = new NewInstanceOp();

	internal NewInstanceOp(TypeUsage type)
		: base(OpType.NewInstance, type)
	{
	}

	private NewInstanceOp()
		: base(OpType.NewInstance)
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
