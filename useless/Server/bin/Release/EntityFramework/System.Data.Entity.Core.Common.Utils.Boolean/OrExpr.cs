using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class OrExpr<T_Identifier> : TreeExpr<T_Identifier>
{
	internal override ExprType ExprType => ExprType.Or;

	internal OrExpr(params BoolExpr<T_Identifier>[] children)
		: this((IEnumerable<BoolExpr<T_Identifier>>)children)
	{
	}

	internal OrExpr(IEnumerable<BoolExpr<T_Identifier>> children)
		: base(children)
	{
	}

	internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
	{
		return visitor.VisitOr(this);
	}
}
