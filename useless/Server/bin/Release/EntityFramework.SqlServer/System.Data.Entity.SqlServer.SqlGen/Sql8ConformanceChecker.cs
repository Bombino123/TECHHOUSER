using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class Sql8ConformanceChecker : DbExpressionVisitor<bool>
{
	private delegate bool ListElementHandler<TElementType>(TElementType element);

	internal static bool NeedsRewrite(DbExpression expr)
	{
		Sql8ConformanceChecker sql8ConformanceChecker = new Sql8ConformanceChecker();
		return expr.Accept<bool>((DbExpressionVisitor<bool>)sql8ConformanceChecker);
	}

	private Sql8ConformanceChecker()
	{
	}

	private bool VisitUnaryExpression(DbUnaryExpression expr)
	{
		return VisitExpression(expr.Argument);
	}

	private bool VisitBinaryExpression(DbBinaryExpression expr)
	{
		bool num = VisitExpression(expr.Left);
		bool flag = VisitExpression(expr.Right);
		return num || flag;
	}

	private bool VisitAggregate(DbAggregate aggregate)
	{
		return VisitExpressionList(aggregate.Arguments);
	}

	private bool VisitExpressionBinding(DbExpressionBinding expressionBinding)
	{
		return VisitExpression(expressionBinding.Expression);
	}

	private bool VisitExpression(DbExpression expression)
	{
		if (expression == null)
		{
			return false;
		}
		return expression.Accept<bool>((DbExpressionVisitor<bool>)this);
	}

	private bool VisitSortClause(DbSortClause sortClause)
	{
		return VisitExpression(sortClause.Expression);
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

	private bool VisitAggregateList(IList<DbAggregate> list)
	{
		return VisitList(VisitAggregate, list);
	}

	private bool VisitExpressionBindingList(IList<DbExpressionBinding> list)
	{
		return VisitList(VisitExpressionBinding, list);
	}

	private bool VisitExpressionList(IList<DbExpression> list)
	{
		return VisitList(VisitExpression, list);
	}

	private bool VisitSortClauseList(IList<DbSortClause> list)
	{
		return VisitList(VisitSortClause, list);
	}

	public override bool Visit(DbExpression expression)
	{
		Check.NotNull<DbExpression>(expression, "expression");
		throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(((object)expression).GetType().FullName));
	}

	public override bool Visit(DbAndExpression expression)
	{
		Check.NotNull<DbAndExpression>(expression, "expression");
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbApplyExpression expression)
	{
		Check.NotNull<DbApplyExpression>(expression, "expression");
		throw new NotSupportedException(Strings.SqlGen_ApplyNotSupportedOnSql8);
	}

	public override bool Visit(DbArithmeticExpression expression)
	{
		Check.NotNull<DbArithmeticExpression>(expression, "expression");
		return VisitExpressionList(expression.Arguments);
	}

	public override bool Visit(DbCaseExpression expression)
	{
		Check.NotNull<DbCaseExpression>(expression, "expression");
		bool num = VisitExpressionList(expression.When);
		bool flag = VisitExpressionList(expression.Then);
		bool flag2 = VisitExpression(expression.Else);
		return num || flag || flag2;
	}

	public override bool Visit(DbCastExpression expression)
	{
		Check.NotNull<DbCastExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbComparisonExpression expression)
	{
		Check.NotNull<DbComparisonExpression>(expression, "expression");
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbConstantExpression expression)
	{
		Check.NotNull<DbConstantExpression>(expression, "expression");
		return false;
	}

	public override bool Visit(DbCrossJoinExpression expression)
	{
		Check.NotNull<DbCrossJoinExpression>(expression, "expression");
		return VisitExpressionBindingList(expression.Inputs);
	}

	public override bool Visit(DbDerefExpression expression)
	{
		Check.NotNull<DbDerefExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbDistinctExpression expression)
	{
		Check.NotNull<DbDistinctExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbElementExpression expression)
	{
		Check.NotNull<DbElementExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbEntityRefExpression expression)
	{
		Check.NotNull<DbEntityRefExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbExceptExpression expression)
	{
		Check.NotNull<DbExceptExpression>(expression, "expression");
		VisitExpression(((DbBinaryExpression)expression).Left);
		VisitExpression(((DbBinaryExpression)expression).Right);
		return true;
	}

	public override bool Visit(DbFilterExpression expression)
	{
		Check.NotNull<DbFilterExpression>(expression, "expression");
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitExpression(expression.Predicate);
		return num || flag;
	}

	public override bool Visit(DbFunctionExpression expression)
	{
		Check.NotNull<DbFunctionExpression>(expression, "expression");
		return VisitExpressionList(expression.Arguments);
	}

	public override bool Visit(DbLambdaExpression expression)
	{
		Check.NotNull<DbLambdaExpression>(expression, "expression");
		bool num = VisitExpressionList(expression.Arguments);
		bool flag = VisitExpression(expression.Lambda.Body);
		return num || flag;
	}

	public override bool Visit(DbGroupByExpression expression)
	{
		Check.NotNull<DbGroupByExpression>(expression, "expression");
		bool num = VisitExpression(expression.Input.Expression);
		bool flag = VisitExpressionList(expression.Keys);
		bool flag2 = VisitAggregateList(expression.Aggregates);
		return num || flag || flag2;
	}

	public override bool Visit(DbIntersectExpression expression)
	{
		Check.NotNull<DbIntersectExpression>(expression, "expression");
		VisitExpression(((DbBinaryExpression)expression).Left);
		VisitExpression(((DbBinaryExpression)expression).Right);
		return true;
	}

	public override bool Visit(DbIsEmptyExpression expression)
	{
		Check.NotNull<DbIsEmptyExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbIsNullExpression expression)
	{
		Check.NotNull<DbIsNullExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbIsOfExpression expression)
	{
		Check.NotNull<DbIsOfExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbJoinExpression expression)
	{
		Check.NotNull<DbJoinExpression>(expression, "expression");
		bool num = VisitExpressionBinding(expression.Left);
		bool flag = VisitExpressionBinding(expression.Right);
		bool flag2 = VisitExpression(expression.JoinCondition);
		return num || flag || flag2;
	}

	public override bool Visit(DbLikeExpression expression)
	{
		Check.NotNull<DbLikeExpression>(expression, "expression");
		bool num = VisitExpression(expression.Argument);
		bool flag = VisitExpression(expression.Pattern);
		bool flag2 = VisitExpression(expression.Escape);
		return num || flag || flag2;
	}

	public override bool Visit(DbLimitExpression expression)
	{
		Check.NotNull<DbLimitExpression>(expression, "expression");
		if (expression.Limit is DbParameterReferenceExpression)
		{
			throw new NotSupportedException(Strings.SqlGen_ParameterForLimitNotSupportedOnSql8);
		}
		return VisitExpression(expression.Argument);
	}

	public override bool Visit(DbNewInstanceExpression expression)
	{
		Check.NotNull<DbNewInstanceExpression>(expression, "expression");
		return VisitExpressionList(expression.Arguments);
	}

	public override bool Visit(DbNotExpression expression)
	{
		Check.NotNull<DbNotExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbNullExpression expression)
	{
		Check.NotNull<DbNullExpression>(expression, "expression");
		return false;
	}

	public override bool Visit(DbOfTypeExpression expression)
	{
		Check.NotNull<DbOfTypeExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbOrExpression expression)
	{
		Check.NotNull<DbOrExpression>(expression, "expression");
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbInExpression expression)
	{
		Check.NotNull<DbInExpression>(expression, "expression");
		if (!VisitExpression(expression.Item))
		{
			return VisitExpressionList(expression.List);
		}
		return true;
	}

	public override bool Visit(DbParameterReferenceExpression expression)
	{
		Check.NotNull<DbParameterReferenceExpression>(expression, "expression");
		return false;
	}

	public override bool Visit(DbProjectExpression expression)
	{
		Check.NotNull<DbProjectExpression>(expression, "expression");
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitExpression(expression.Projection);
		return num || flag;
	}

	public override bool Visit(DbPropertyExpression expression)
	{
		Check.NotNull<DbPropertyExpression>(expression, "expression");
		return VisitExpression(expression.Instance);
	}

	public override bool Visit(DbQuantifierExpression expression)
	{
		Check.NotNull<DbQuantifierExpression>(expression, "expression");
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitExpression(expression.Predicate);
		return num || flag;
	}

	public override bool Visit(DbRefExpression expression)
	{
		Check.NotNull<DbRefExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbRefKeyExpression expression)
	{
		Check.NotNull<DbRefKeyExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbRelationshipNavigationExpression expression)
	{
		Check.NotNull<DbRelationshipNavigationExpression>(expression, "expression");
		return VisitExpression(expression.NavigationSource);
	}

	public override bool Visit(DbScanExpression expression)
	{
		Check.NotNull<DbScanExpression>(expression, "expression");
		return false;
	}

	public override bool Visit(DbSkipExpression expression)
	{
		Check.NotNull<DbSkipExpression>(expression, "expression");
		if (expression.Count is DbParameterReferenceExpression)
		{
			throw new NotSupportedException(Strings.SqlGen_ParameterForSkipNotSupportedOnSql8);
		}
		VisitExpressionBinding(expression.Input);
		VisitSortClauseList(expression.SortOrder);
		VisitExpression(expression.Count);
		return true;
	}

	public override bool Visit(DbSortExpression expression)
	{
		Check.NotNull<DbSortExpression>(expression, "expression");
		bool num = VisitExpressionBinding(expression.Input);
		bool flag = VisitSortClauseList(expression.SortOrder);
		return num || flag;
	}

	public override bool Visit(DbTreatExpression expression)
	{
		Check.NotNull<DbTreatExpression>(expression, "expression");
		return VisitUnaryExpression((DbUnaryExpression)(object)expression);
	}

	public override bool Visit(DbUnionAllExpression expression)
	{
		Check.NotNull<DbUnionAllExpression>(expression, "expression");
		return VisitBinaryExpression((DbBinaryExpression)(object)expression);
	}

	public override bool Visit(DbVariableReferenceExpression expression)
	{
		Check.NotNull<DbVariableReferenceExpression>(expression, "expression");
		return false;
	}
}
