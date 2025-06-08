using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class DbExpressionVisitor
{
	public abstract void Visit(DbExpression expression);

	public abstract void Visit(DbAndExpression expression);

	public abstract void Visit(DbApplyExpression expression);

	public abstract void Visit(DbArithmeticExpression expression);

	public abstract void Visit(DbCaseExpression expression);

	public abstract void Visit(DbCastExpression expression);

	public abstract void Visit(DbComparisonExpression expression);

	public abstract void Visit(DbConstantExpression expression);

	public abstract void Visit(DbCrossJoinExpression expression);

	public abstract void Visit(DbDerefExpression expression);

	public abstract void Visit(DbDistinctExpression expression);

	public abstract void Visit(DbElementExpression expression);

	public abstract void Visit(DbExceptExpression expression);

	public abstract void Visit(DbFilterExpression expression);

	public abstract void Visit(DbFunctionExpression expression);

	public abstract void Visit(DbEntityRefExpression expression);

	public abstract void Visit(DbRefKeyExpression expression);

	public abstract void Visit(DbGroupByExpression expression);

	public abstract void Visit(DbIntersectExpression expression);

	public abstract void Visit(DbIsEmptyExpression expression);

	public abstract void Visit(DbIsNullExpression expression);

	public abstract void Visit(DbIsOfExpression expression);

	public abstract void Visit(DbJoinExpression expression);

	public virtual void Visit(DbLambdaExpression expression)
	{
		throw new NotSupportedException();
	}

	public abstract void Visit(DbLikeExpression expression);

	public abstract void Visit(DbLimitExpression expression);

	public abstract void Visit(DbNewInstanceExpression expression);

	public abstract void Visit(DbNotExpression expression);

	public abstract void Visit(DbNullExpression expression);

	public abstract void Visit(DbOfTypeExpression expression);

	public abstract void Visit(DbOrExpression expression);

	public abstract void Visit(DbParameterReferenceExpression expression);

	public abstract void Visit(DbProjectExpression expression);

	public abstract void Visit(DbPropertyExpression expression);

	public abstract void Visit(DbQuantifierExpression expression);

	public abstract void Visit(DbRefExpression expression);

	public abstract void Visit(DbRelationshipNavigationExpression expression);

	public abstract void Visit(DbScanExpression expression);

	public abstract void Visit(DbSkipExpression expression);

	public abstract void Visit(DbSortExpression expression);

	public abstract void Visit(DbTreatExpression expression);

	public abstract void Visit(DbUnionAllExpression expression);

	public abstract void Visit(DbVariableReferenceExpression expression);

	public virtual void Visit(DbInExpression expression)
	{
		throw new NotImplementedException(Strings.VisitDbInExpressionNotImplemented);
	}
}
public abstract class DbExpressionVisitor<TResultType>
{
	public abstract TResultType Visit(DbExpression expression);

	public abstract TResultType Visit(DbAndExpression expression);

	public abstract TResultType Visit(DbApplyExpression expression);

	public abstract TResultType Visit(DbArithmeticExpression expression);

	public abstract TResultType Visit(DbCaseExpression expression);

	public abstract TResultType Visit(DbCastExpression expression);

	public abstract TResultType Visit(DbComparisonExpression expression);

	public abstract TResultType Visit(DbConstantExpression expression);

	public abstract TResultType Visit(DbCrossJoinExpression expression);

	public abstract TResultType Visit(DbDerefExpression expression);

	public abstract TResultType Visit(DbDistinctExpression expression);

	public abstract TResultType Visit(DbElementExpression expression);

	public abstract TResultType Visit(DbExceptExpression expression);

	public abstract TResultType Visit(DbFilterExpression expression);

	public abstract TResultType Visit(DbFunctionExpression expression);

	public abstract TResultType Visit(DbEntityRefExpression expression);

	public abstract TResultType Visit(DbRefKeyExpression expression);

	public abstract TResultType Visit(DbGroupByExpression expression);

	public abstract TResultType Visit(DbIntersectExpression expression);

	public abstract TResultType Visit(DbIsEmptyExpression expression);

	public abstract TResultType Visit(DbIsNullExpression expression);

	public abstract TResultType Visit(DbIsOfExpression expression);

	public abstract TResultType Visit(DbJoinExpression expression);

	public virtual TResultType Visit(DbLambdaExpression expression)
	{
		throw new NotSupportedException();
	}

	public abstract TResultType Visit(DbLikeExpression expression);

	public abstract TResultType Visit(DbLimitExpression expression);

	public abstract TResultType Visit(DbNewInstanceExpression expression);

	public abstract TResultType Visit(DbNotExpression expression);

	public abstract TResultType Visit(DbNullExpression expression);

	public abstract TResultType Visit(DbOfTypeExpression expression);

	public abstract TResultType Visit(DbOrExpression expression);

	public abstract TResultType Visit(DbParameterReferenceExpression expression);

	public abstract TResultType Visit(DbProjectExpression expression);

	public abstract TResultType Visit(DbPropertyExpression expression);

	public abstract TResultType Visit(DbQuantifierExpression expression);

	public abstract TResultType Visit(DbRefExpression expression);

	public abstract TResultType Visit(DbRelationshipNavigationExpression expression);

	public abstract TResultType Visit(DbScanExpression expression);

	public abstract TResultType Visit(DbSortExpression expression);

	public abstract TResultType Visit(DbSkipExpression expression);

	public abstract TResultType Visit(DbTreatExpression expression);

	public abstract TResultType Visit(DbUnionAllExpression expression);

	public abstract TResultType Visit(DbVariableReferenceExpression expression);

	public virtual TResultType Visit(DbInExpression expression)
	{
		throw new NotImplementedException(Strings.VisitDbInExpressionNotImplemented);
	}
}
