using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ComplexTypeColumnMap : TypedColumnMap
{
	private readonly SimpleColumnMap m_nullSentinel;

	internal override SimpleColumnMap NullSentinel => m_nullSentinel;

	internal ComplexTypeColumnMap(TypeUsage type, string name, ColumnMap[] properties, SimpleColumnMap nullSentinel)
		: base(type, name, properties)
	{
		m_nullSentinel = nullSentinel;
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
		return string.Format(CultureInfo.InvariantCulture, "C{0}", new object[1] { base.ToString() });
	}
}
