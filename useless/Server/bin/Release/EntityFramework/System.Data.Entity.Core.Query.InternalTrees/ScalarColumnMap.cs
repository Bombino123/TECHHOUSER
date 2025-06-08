using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ScalarColumnMap : SimpleColumnMap
{
	private readonly int m_commandId;

	private readonly int m_columnPos;

	internal int CommandId => m_commandId;

	internal int ColumnPos => m_columnPos;

	internal ScalarColumnMap(TypeUsage type, string name, int commandId, int columnPos)
		: base(type, name)
	{
		m_commandId = commandId;
		m_columnPos = columnPos;
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
		return string.Format(CultureInfo.InvariantCulture, "S({0},{1})", new object[2] { CommandId, ColumnPos });
	}
}
