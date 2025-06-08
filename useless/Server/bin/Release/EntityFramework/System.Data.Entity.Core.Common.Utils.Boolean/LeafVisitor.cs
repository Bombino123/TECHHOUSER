using System.Collections.Generic;
using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class LeafVisitor<T_Identifier> : Visitor<T_Identifier, bool>
{
	private readonly List<TermExpr<T_Identifier>> _terms;

	private LeafVisitor()
	{
		_terms = new List<TermExpr<T_Identifier>>();
	}

	internal static List<TermExpr<T_Identifier>> GetTerms(BoolExpr<T_Identifier> expression)
	{
		LeafVisitor<T_Identifier> leafVisitor = new LeafVisitor<T_Identifier>();
		expression.Accept(leafVisitor);
		return leafVisitor._terms;
	}

	internal static IEnumerable<T_Identifier> GetLeaves(BoolExpr<T_Identifier> expression)
	{
		return from term in GetTerms(expression)
			select term.Identifier;
	}

	internal override bool VisitTrue(TrueExpr<T_Identifier> expression)
	{
		return true;
	}

	internal override bool VisitFalse(FalseExpr<T_Identifier> expression)
	{
		return true;
	}

	internal override bool VisitTerm(TermExpr<T_Identifier> expression)
	{
		_terms.Add(expression);
		return true;
	}

	internal override bool VisitNot(NotExpr<T_Identifier> expression)
	{
		return expression.Child.Accept(this);
	}

	internal override bool VisitAnd(AndExpr<T_Identifier> expression)
	{
		return VisitTree(expression);
	}

	internal override bool VisitOr(OrExpr<T_Identifier> expression)
	{
		return VisitTree(expression);
	}

	private bool VisitTree(TreeExpr<T_Identifier> expression)
	{
		foreach (BoolExpr<T_Identifier> child in expression.Children)
		{
			child.Accept(this);
		}
		return true;
	}
}
