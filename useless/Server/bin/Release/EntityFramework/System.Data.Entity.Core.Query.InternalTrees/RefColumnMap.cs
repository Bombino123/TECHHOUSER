using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class RefColumnMap : ColumnMap
{
	private readonly EntityIdentity m_entityIdentity;

	internal EntityIdentity EntityIdentity => m_entityIdentity;

	internal RefColumnMap(TypeUsage type, string name, EntityIdentity entityIdentity)
		: base(type, name)
	{
		m_entityIdentity = entityIdentity;
	}

	[DebuggerNonUserCode]
	internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
	{
		visitor.Visit(this, arg);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType, TArgType>(ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
	{
		return visitor.Visit(this, arg);
	}
}
