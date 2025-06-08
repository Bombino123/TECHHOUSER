using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class EntityColumnMap : TypedColumnMap
{
	private readonly EntityIdentity m_entityIdentity;

	internal EntityIdentity EntityIdentity => m_entityIdentity;

	internal EntityColumnMap(TypeUsage type, string name, ColumnMap[] properties, EntityIdentity entityIdentity)
		: base(type, name, properties)
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

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "E{0}", new object[1] { base.ToString() });
	}
}
