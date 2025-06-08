using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.SQLite.EF6;

internal sealed class SqlChecker : DbExpressionVisitor<bool>
{
	private delegate bool ListElementHandler<TElementType>(TElementType element);

	private SqlChecker()
	{
	}

	public override bool Visit(DbAndExpression expression)
	{
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbApplyExpression expression)
	{
		throw new NotSupportedException("apply expression");
	}

	public override bool Visit(DbArithmeticExpression expression)
	{
		return VisitExpressionList(expression.Arguments);
	}

	public override bool Visit(DbCaseExpression expression)
	{
		bool num = VisitExpressionList(expression.When);
		bool flag = VisitExpressionList(expression.Then);
		bool flag2 = VisitExpression(expression.Else);
		return num || flag || flag2;
	}

	public override bool Visit(DbCastExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbComparisonExpression expression)
	{
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbConstantExpression expression)
	{
		return false;
	}

	public override bool Visit(DbCrossJoinExpression expression)
	{
		return VisitExpressionBindingList(expression.Inputs);
	}

	public override bool Visit(DbDerefExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbDistinctExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbElementExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbEntityRefExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbExceptExpression expression)
	{
		bool num = VisitExpression(((DbBinaryExpression)expression).Left);
		bool flag = VisitExpression(((DbBinaryExpression)expression).Right);
		return num || flag;
	}

	public override bool Visit(DbExpression expression)
	{
		throw new NotSupportedException(((object)expression).GetType().FullName);
	}

	public override bool Visit(DbFilterExpression expression)
	{
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitExpression(expression.Predicate);
		return num || flag;
	}

	public override bool Visit(DbFunctionExpression expression)
	{
		return VisitExpressionList(expression.Arguments);
	}

	public override bool Visit(DbGroupByExpression expression)
	{
		bool num = VisitExpression(expression.Input.Expression);
		bool flag = VisitExpressionList(expression.Keys);
		bool flag2 = VisitAggregateList(expression.Aggregates);
		return num || flag || flag2;
	}

	public override bool Visit(DbIntersectExpression expression)
	{
		bool num = VisitExpression(((DbBinaryExpression)expression).Left);
		bool flag = VisitExpression(((DbBinaryExpression)expression).Right);
		return num || flag;
	}

	public override bool Visit(DbIsEmptyExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbIsNullExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbIsOfExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbJoinExpression expression)
	{
		bool num = VisitExpressionBinding(expression.Left);
		bool flag = VisitExpressionBinding(expression.Right);
		bool flag2 = VisitExpression(expression.JoinCondition);
		return num || flag || flag2;
	}

	public override bool Visit(DbLikeExpression expression)
	{
		bool num = VisitExpression(expression.Argument);
		bool flag = VisitExpression(expression.Pattern);
		bool flag2 = VisitExpression(expression.Escape);
		return num || flag || flag2;
	}

	public override bool Visit(DbLimitExpression expression)
	{
		return VisitExpression(expression.Argument);
	}

	public override bool Visit(DbNewInstanceExpression expression)
	{
		return VisitExpressionList(expression.Arguments);
	}

	public override bool Visit(DbNotExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbNullExpression expression)
	{
		return false;
	}

	public override bool Visit(DbOfTypeExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbOrExpression expression)
	{
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbParameterReferenceExpression expression)
	{
		return false;
	}

	public override bool Visit(DbProjectExpression expression)
	{
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitExpression(expression.Projection);
		return num || flag;
	}

	public override bool Visit(DbPropertyExpression expression)
	{
		return VisitExpression(expression.Instance);
	}

	public override bool Visit(DbQuantifierExpression expression)
	{
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitExpression(expression.Predicate);
		return num || flag;
	}

	public override bool Visit(DbRefExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbRefKeyExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbRelationshipNavigationExpression expression)
	{
		return VisitExpression(expression.NavigationSource);
	}

	public override bool Visit(DbScanExpression expression)
	{
		return false;
	}

	public override bool Visit(DbSkipExpression expression)
	{
		VisitExpressionBinding(expression.Input);
		VisitSortClauseList(expression.SortOrder);
		VisitExpression(expression.Count);
		return true;
	}

	public override bool Visit(DbSortExpression expression)
	{
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitSortClauseList(expression.SortOrder);
		return num || flag;
	}

	public override bool Visit(DbTreatExpression expression)
	{
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbUnionAllExpression expression)
	{
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbVariableReferenceExpression expression)
	{
		return false;
	}

	private bool VisitAggregate(DbAggregate aggregate)
	{
		return VisitExpressionList(aggregate.Arguments);
	}

	private bool VisitAggregateList(IList<DbAggregate> list)
	{
		return VisitList(VisitAggregate, list);
	}

	private bool VisitBinaryExpression(DbBinaryExpression expr)
	{
		bool num = VisitExpression(expr.Left);
		bool flag = VisitExpression(expr.Right);
		return num || flag;
	}

	private bool VisitExpression(DbExpression expression)
	{
		if (expression == null)
		{
			return false;
		}
		return expression.Accept<bool>((DbExpressionVisitor<bool>)this);
	}

	private bool VisitExpressionBinding(DbExpressionBinding expressionBinding)
	{
		return VisitExpression(expressionBinding.Expression);
	}

	private bool VisitExpressionBindingList(IList<DbExpressionBinding> list)
	{
		return VisitList(VisitExpressionBinding, list);
	}

	private bool VisitExpressionList(IList<DbExpression> list)
	{
		return VisitList(VisitExpression, list);
	}

	private static bool VisitList<TElementType>(ListElementHandler<TElementType> handler, IList<TElementType> list)
	{
		bool flag = false;
		foreach (TElementType item in list)
		{
			bool flag2 = handler(item);
			flag = flag || flag2;
		}
		return flag;
	}

	private bool VisitSortClause(DbSortClause sortClause)
	{
		return VisitExpression(sortClause.Expression);
	}

	private bool VisitSortClauseList(IList<DbSortClause> list)
	{
		return VisitList(VisitSortClause, list);
	}

	private bool VisitUnaryExpression(DbUnaryExpression expr)
	{
		return VisitExpression(expr.Argument);
	}
}
