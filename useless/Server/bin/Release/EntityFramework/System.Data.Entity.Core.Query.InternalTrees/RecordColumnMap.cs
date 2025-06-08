using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class RecordColumnMap : StructuredColumnMap
{
	private readonly SimpleColumnMap m_nullSentinel;

	internal override SimpleColumnMap NullSentinel => m_nullSentinel;

	internal RecordColumnMap(TypeUsage type, string name, ColumnMap[] properties, SimpleColumnMap nullSentinel)
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
}
