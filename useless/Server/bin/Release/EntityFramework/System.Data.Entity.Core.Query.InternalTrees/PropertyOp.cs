using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class PropertyOp : ScalarOp
{
	private readonly EdmMember m_property;

	internal static readonly PropertyOp Pattern = new PropertyOp();

	internal override int Arity => 1;

	internal EdmMember PropertyInfo => m_property;

	internal PropertyOp(TypeUsage type, EdmMember property)
		: base(OpType.Property, type)
	{
		m_property = property;
	}

	private PropertyOp()
		: base(OpType.Property)
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

	internal override bool IsEquivalent(Op other)
	{
		if (other is PropertyOp propertyOp && propertyOp.PropertyInfo.EdmEquals(PropertyInfo))
		{
			return base.IsEquivalent(other);
		}
		return false;
	}
}
