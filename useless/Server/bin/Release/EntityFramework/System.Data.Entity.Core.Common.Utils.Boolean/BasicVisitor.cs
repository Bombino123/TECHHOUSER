using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal abstract class BasicVisitor<T_Identifier> : Visitor<T_Identifier, BoolExpr<T_Identifier>>
{
	internal override BoolExpr<T_Identifier> VisitFalse(FalseExpr<T_Identifier> expression)
	{
		return expression;
	}

	internal override BoolExpr<T_Identifier> VisitTrue(TrueExpr<T_Identifier> expression)
	{
		return expression;
	}

	internal override BoolExpr<T_Identifier> VisitTerm(TermExpr<T_Identifier> expression)
	{
		return expression;
	}

	internal override BoolExpr<T_Identifier> VisitNot(NotExpr<T_Identifier> expression)
	{
		return new NotExpr<T_Identifier>(expression.Child.Accept(this));
	}

	internal override BoolExpr<T_Identifier> VisitAnd(AndExpr<T_Identifier> expression)
	{
		return new AndExpr<T_Identifier>(AcceptChildren(expression.Children));
	}

	internal override BoolExpr<T_Identifier> VisitOr(OrExpr<T_Identifier> expression)
	{
		return new OrExpr<T_Identifier>(AcceptChildren(expression.Children));
	}

	private IEnumerable<BoolExpr<T_Identifier>> AcceptChildren(IEnumerable<BoolExpr<T_Identifier>> children)
	{
		foreach (BoolExpr<T_Identifier> child in children)
		{
			yield return child.Accept(this);
		}
	}
}
