using System.Linq;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class ToDecisionDiagramConverter<T_Identifier> : Visitor<T_Identifier, Vertex>
{
	private readonly ConversionContext<T_Identifier> _context;

	private ToDecisionDiagramConverter(ConversionContext<T_Identifier> context)
	{
		_context = context;
	}

	internal static Vertex TranslateToRobdd(BoolExpr<T_Identifier> expr, ConversionContext<T_Identifier> context)
	{
		ToDecisionDiagramConverter<T_Identifier> visitor = new ToDecisionDiagramConverter<T_Identifier>(context);
		return expr.Accept(visitor);
	}

	internal override Vertex VisitTrue(TrueExpr<T_Identifier> expression)
	{
		return Vertex.One;
	}

	internal override Vertex VisitFalse(FalseExpr<T_Identifier> expression)
	{
		return Vertex.Zero;
	}

	internal override Vertex VisitTerm(TermExpr<T_Identifier> expression)
	{
		return _context.TranslateTermToVertex(expression);
	}

	internal override Vertex VisitNot(NotExpr<T_Identifier> expression)
	{
		return _context.Solver.Not(expression.Child.Accept(this));
	}

	internal override Vertex VisitAnd(AndExpr<T_Identifier> expression)
	{
		return _context.Solver.And(expression.Children.Select((BoolExpr<T_Identifier> child) => child.Accept(this)));
	}

	internal override Vertex VisitOr(OrExpr<T_Identifier> expression)
	{
		return _context.Solver.Or(expression.Children.Select((BoolExpr<T_Identifier> child) => child.Accept(this)));
	}
}
