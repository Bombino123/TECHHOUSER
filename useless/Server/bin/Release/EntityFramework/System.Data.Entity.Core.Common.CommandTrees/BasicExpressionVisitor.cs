using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees;

public abstract class BasicExpressionVisitor : DbExpressionVisitor
{
	protected virtual void VisitUnaryExpression(DbUnaryExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpression(expression.Argument);
	}

	protected virtual void VisitBinaryExpression(DbBinaryExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpression(expression.Left);
		VisitExpression(expression.Right);
	}

	protected virtual void VisitExpressionBindingPre(DbExpressionBinding binding)
	{
		Check.NotNull(binding, "binding");
		VisitExpression(binding.Expression);
	}

	protected virtual void VisitExpressionBindingPost(DbExpressionBinding binding)
	{
	}

	protected virtual void VisitGroupExpressionBindingPre(DbGroupExpressionBinding binding)
	{
		Check.NotNull(binding, "binding");
		VisitExpression(binding.Expression);
	}

	protected virtual void VisitGroupExpressionBindingMid(DbGroupExpressionBinding binding)
	{
	}

	protected virtual void VisitGroupExpressionBindingPost(DbGroupExpressionBinding binding)
	{
	}

	protected virtual void VisitLambdaPre(DbLambda lambda)
	{
		Check.NotNull(lambda, "lambda");
	}

	protected virtual void VisitLambdaPost(DbLambda lambda)
	{
	}

	public virtual void VisitExpression(DbExpression expression)
	{
		Check.NotNull(expression, "expression");
		expression.Accept(this);
	}

	public virtual void VisitExpressionList(IList<DbExpression> expressionList)
	{
		Check.NotNull(expressionList, "expressionList");
		for (int i = 0; i < expressionList.Count; i++)
		{
			VisitExpression(expressionList[i]);
		}
	}

	public virtual void VisitAggregateList(IList<DbAggregate> aggregates)
	{
		Check.NotNull(aggregates, "aggregates");
		for (int i = 0; i < aggregates.Count; i++)
		{
			VisitAggregate(aggregates[i]);
		}
	}

	public virtual void VisitAggregate(DbAggregate aggregate)
	{
		Check.NotNull(aggregate, "aggregate");
		VisitExpressionList(aggregate.Arguments);
	}

	internal virtual void VisitRelatedEntityReferenceList(IList<DbRelatedEntityRef> relatedEntityReferences)
	{
		for (int i = 0; i < relatedEntityReferences.Count; i++)
		{
			VisitRelatedEntityReference(relatedEntityReferences[i]);
		}
	}

	internal virtual void VisitRelatedEntityReference(DbRelatedEntityRef relatedEntityRef)
	{
		VisitExpression(relatedEntityRef.TargetEntityReference);
	}

	public override void Visit(DbExpression expression)
	{
		Check.NotNull(expression, "expression");
		throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(expression.GetType().FullName));
	}

	public override void Visit(DbConstantExpression expression)
	{
		Check.NotNull(expression, "expression");
	}

	public override void Visit(DbNullExpression expression)
	{
		Check.NotNull(expression, "expression");
	}

	public override void Visit(DbVariableReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
	}

	public override void Visit(DbParameterReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
	}

	public override void Visit(DbFunctionExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionList(expression.Arguments);
	}

	public override void Visit(DbLambdaExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionList(expression.Arguments);
		VisitLambdaPre(expression.Lambda);
		VisitExpression(expression.Lambda.Body);
		VisitLambdaPost(expression.Lambda);
	}

	public override void Visit(DbPropertyExpression expression)
	{
		Check.NotNull(expression, "expression");
		if (expression.Instance != null)
		{
			VisitExpression(expression.Instance);
		}
	}

	public override void Visit(DbComparisonExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitBinaryExpression(expression);
	}

	public override void Visit(DbLikeExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpression(expression.Argument);
		VisitExpression(expression.Pattern);
		VisitExpression(expression.Escape);
	}

	public override void Visit(DbLimitExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpression(expression.Argument);
		VisitExpression(expression.Limit);
	}

	public override void Visit(DbIsNullExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbArithmeticExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionList(expression.Arguments);
	}

	public override void Visit(DbAndExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitBinaryExpression(expression);
	}

	public override void Visit(DbOrExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitBinaryExpression(expression);
	}

	public override void Visit(DbInExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpression(expression.Item);
		VisitExpressionList(expression.List);
	}

	public override void Visit(DbNotExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbDistinctExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbElementExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbIsEmptyExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbUnionAllExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitBinaryExpression(expression);
	}

	public override void Visit(DbIntersectExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitBinaryExpression(expression);
	}

	public override void Visit(DbExceptExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitBinaryExpression(expression);
	}

	public override void Visit(DbOfTypeExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbTreatExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbCastExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbIsOfExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbCaseExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionList(expression.When);
		VisitExpressionList(expression.Then);
		VisitExpression(expression.Else);
	}

	public override void Visit(DbNewInstanceExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionList(expression.Arguments);
		if (expression.HasRelatedEntityReferences)
		{
			VisitRelatedEntityReferenceList(expression.RelatedEntityReferences);
		}
	}

	public override void Visit(DbRefExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbRelationshipNavigationExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpression(expression.NavigationSource);
	}

	public override void Visit(DbDerefExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbRefKeyExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbEntityRefExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitUnaryExpression(expression);
	}

	public override void Visit(DbScanExpression expression)
	{
		Check.NotNull(expression, "expression");
	}

	public override void Visit(DbFilterExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Input);
		VisitExpression(expression.Predicate);
		VisitExpressionBindingPost(expression.Input);
	}

	public override void Visit(DbProjectExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Input);
		VisitExpression(expression.Projection);
		VisitExpressionBindingPost(expression.Input);
	}

	public override void Visit(DbCrossJoinExpression expression)
	{
		Check.NotNull(expression, "expression");
		foreach (DbExpressionBinding input in expression.Inputs)
		{
			VisitExpressionBindingPre(input);
		}
		foreach (DbExpressionBinding input2 in expression.Inputs)
		{
			VisitExpressionBindingPost(input2);
		}
	}

	public override void Visit(DbJoinExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Left);
		VisitExpressionBindingPre(expression.Right);
		VisitExpression(expression.JoinCondition);
		VisitExpressionBindingPost(expression.Left);
		VisitExpressionBindingPost(expression.Right);
	}

	public override void Visit(DbApplyExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Input);
		if (expression.Apply != null)
		{
			VisitExpression(expression.Apply.Expression);
		}
		VisitExpressionBindingPost(expression.Input);
	}

	public override void Visit(DbGroupByExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitGroupExpressionBindingPre(expression.Input);
		VisitExpressionList(expression.Keys);
		VisitGroupExpressionBindingMid(expression.Input);
		VisitAggregateList(expression.Aggregates);
		VisitGroupExpressionBindingPost(expression.Input);
	}

	public override void Visit(DbSkipExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Input);
		foreach (DbSortClause item in expression.SortOrder)
		{
			VisitExpression(item.Expression);
		}
		VisitExpressionBindingPost(expression.Input);
		VisitExpression(expression.Count);
	}

	public override void Visit(DbSortExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Input);
		for (int i = 0; i < expression.SortOrder.Count; i++)
		{
			VisitExpression(expression.SortOrder[i].Expression);
		}
		VisitExpressionBindingPost(expression.Input);
	}

	public override void Visit(DbQuantifierExpression expression)
	{
		Check.NotNull(expression, "expression");
		VisitExpressionBindingPre(expression.Input);
		VisitExpression(expression.Predicate);
		VisitExpressionBindingPost(expression.Input);
	}
}
