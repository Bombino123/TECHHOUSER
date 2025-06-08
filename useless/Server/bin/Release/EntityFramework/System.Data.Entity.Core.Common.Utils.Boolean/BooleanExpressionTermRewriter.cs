using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class BooleanExpressionTermRewriter<T_From, T_To> : Visitor<T_From, BoolExpr<T_To>>
{
	private readonly Func<TermExpr<T_From>, BoolExpr<T_To>> _translator;

	internal BooleanExpressionTermRewriter(Func<TermExpr<T_From>, BoolExpr<T_To>> translator)
	{
		_translator = translator;
	}

	internal override BoolExpr<T_To> VisitFalse(FalseExpr<T_From> expression)
	{
		return FalseExpr<T_To>.Value;
	}

	internal override BoolExpr<T_To> VisitTrue(TrueExpr<T_From> expression)
	{
		return TrueExpr<T_To>.Value;
	}

	internal override BoolExpr<T_To> VisitNot(NotExpr<T_From> expression)
	{
		return new NotExpr<T_To>(expression.Child.Accept(this));
	}

	internal override BoolExpr<T_To> VisitTerm(TermExpr<T_From> expression)
	{
		return _translator(expression);
	}

	internal override BoolExpr<T_To> VisitAnd(AndExpr<T_From> expression)
	{
		return new AndExpr<T_To>(VisitChildren(expression));
	}

	internal override BoolExpr<T_To> VisitOr(OrExpr<T_From> expression)
	{
		return new OrExpr<T_To>(VisitChildren(expression));
	}

	private IEnumerable<BoolExpr<T_To>> VisitChildren(TreeExpr<T_From> expression)
	{
		foreach (BoolExpr<T_From> child in expression.Children)
		{
			yield return child.Accept(this);
		}
	}
}
