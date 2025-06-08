using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal sealed class NotExpr<T_Identifier> : TreeExpr<T_Identifier>
{
	internal override ExprType ExprType => ExprType.Not;

	internal BoolExpr<T_Identifier> Child => base.Children.First();

	internal NotExpr(BoolExpr<T_Identifier> child)
		: base((IEnumerable<BoolExpr<T_Identifier>>)new BoolExpr<T_Identifier>[1] { child })
	{
	}

	internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
	{
		return visitor.VisitNot(this);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "!{0}", new object[1] { Child });
	}

	internal override BoolExpr<T_Identifier> MakeNegated()
	{
		return Child;
	}
}
