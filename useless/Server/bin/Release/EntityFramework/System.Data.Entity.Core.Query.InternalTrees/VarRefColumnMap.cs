using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class VarRefColumnMap : SimpleColumnMap
{
	private readonly Var m_var;

	internal Var Var => m_var;

	internal VarRefColumnMap(TypeUsage type, string name, Var v)
		: base(type, name)
	{
		m_var = v;
	}

	internal VarRefColumnMap(Var v)
		: this(v.Type, null, v)
	{
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
		if (!base.IsNamed)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { m_var.Id });
		}
		return base.Name;
	}
}
