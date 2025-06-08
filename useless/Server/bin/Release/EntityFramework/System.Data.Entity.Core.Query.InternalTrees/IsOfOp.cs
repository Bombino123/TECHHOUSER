using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class IsOfOp : ScalarOp
{
	private readonly TypeUsage m_isOfType;

	private readonly bool m_isOfOnly;

	internal static readonly IsOfOp Pattern = new IsOfOp();

	internal override int Arity => 1;

	internal TypeUsage IsOfType => m_isOfType;

	internal bool IsOfOnly => m_isOfOnly;

	internal IsOfOp(TypeUsage isOfType, bool isOfOnly, TypeUsage type)
		: base(OpType.IsOf, type)
	{
		m_isOfType = isOfType;
		m_isOfOnly = isOfOnly;
	}

	private IsOfOp()
		: base(OpType.IsOf)
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
