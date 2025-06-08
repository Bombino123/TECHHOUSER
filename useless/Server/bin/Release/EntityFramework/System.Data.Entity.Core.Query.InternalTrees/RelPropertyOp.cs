using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class RelPropertyOp : ScalarOp
{
	private readonly RelProperty m_property;

	internal static readonly RelPropertyOp Pattern = new RelPropertyOp();

	internal override int Arity => 1;

	public RelProperty PropertyInfo => m_property;

	private RelPropertyOp()
		: base(OpType.RelProperty)
	{
	}

	internal RelPropertyOp(TypeUsage type, RelProperty property)
		: base(OpType.RelProperty, type)
	{
		m_property = property;
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
