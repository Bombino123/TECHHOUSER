using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class NavigateOp : ScalarOp
{
	private readonly RelProperty m_property;

	internal static readonly NavigateOp Pattern = new NavigateOp();

	internal override int Arity => 1;

	internal RelProperty RelProperty => m_property;

	internal RelationshipType Relationship => m_property.Relationship;

	internal RelationshipEndMember FromEnd => m_property.FromEnd;

	internal RelationshipEndMember ToEnd => m_property.ToEnd;

	internal NavigateOp(TypeUsage type, RelProperty relProperty)
		: base(OpType.Navigate, type)
	{
		m_property = relProperty;
	}

	private NavigateOp()
		: base(OpType.Navigate)
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
