using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees;

public class DefaultExpressionVisitor : DbExpressionVisitor<DbExpression>
{
	private readonly Dictionary<DbVariableReferenceExpression, DbVariableReferenceExpression> varMappings = new Dictionary<DbVariableReferenceExpression, DbVariableReferenceExpression>();

	protected DefaultExpressionVisitor()
	{
	}

	protected virtual void OnExpressionReplaced(DbExpression oldExpression, DbExpression newExpression)
	{
	}

	protected virtual void OnVariableRebound(DbVariableReferenceExpression fromVarRef, DbVariableReferenceExpression toVarRef)
	{
	}

	protected virtual void OnEnterScope(IEnumerable<DbVariableReferenceExpression> scopeVariables)
	{
	}

	protected virtual void OnExitScope()
	{
	}

	protected virtual DbExpression VisitExpression(DbExpression expression)
	{
		DbExpression result = null;
		if (expression != null)
		{
			result = expression.Accept(this);
		}
		return result;
	}

	protected virtual IList<DbExpression> VisitExpressionList(IList<DbExpression> list)
	{
		return VisitList(list, VisitExpression);
	}

	protected virtual DbExpressionBinding VisitExpressionBinding(DbExpressionBinding binding)
	{
		DbExpressionBinding dbExpressionBinding = binding;
		if (binding != null)
		{
			DbExpression dbExpression = VisitExpression(binding.Expression);
			if (binding.Expression != dbExpression)
			{
				dbExpressionBinding = dbExpression.BindAs(binding.VariableName);
				RebindVariable(binding.Variable, dbExpressionBinding.Variable);
			}
		}
		return dbExpressionBinding;
	}

	protected virtual IList<DbExpressionBinding> VisitExpressionBindingList(IList<DbExpressionBinding> list)
	{
		return VisitList(list, VisitExpressionBinding);
	}

	protected virtual DbGroupExpressionBinding VisitGroupExpressionBinding(DbGroupExpressionBinding binding)
	{
		DbGroupExpressionBinding dbGroupExpressionBinding = binding;
		if (binding != null)
		{
			DbExpression dbExpression = VisitExpression(binding.Expression);
			if (binding.Expression != dbExpression)
			{
				dbGroupExpressionBinding = dbExpression.GroupBindAs(binding.VariableName, binding.GroupVariableName);
				RebindVariable(binding.Variable, dbGroupExpressionBinding.Variable);
				RebindVariable(binding.GroupVariable, dbGroupExpressionBinding.GroupVariable);
			}
		}
		return dbGroupExpressionBinding;
	}

	protected virtual DbSortClause VisitSortClause(DbSortClause clause)
	{
		DbSortClause result = clause;
		if (clause != null)
		{
			DbExpression dbExpression = VisitExpression(clause.Expression);
			if (clause.Expression != dbExpression)
			{
				result = (string.IsNullOrEmpty(clause.Collation) ? (clause.Ascending ? dbExpression.ToSortClause() : dbExpression.ToSortClauseDescending()) : (clause.Ascending ? dbExpression.ToSortClause(clause.Collation) : dbExpression.ToSortClauseDescending(clause.Collation)));
			}
		}
		return result;
	}

	protected virtual IList<DbSortClause> VisitSortOrder(IList<DbSortClause> sortOrder)
	{
		return VisitList(sortOrder, VisitSortClause);
	}

	protected virtual DbAggregate VisitAggregate(DbAggregate aggregate)
	{
		if (aggregate is DbFunctionAggregate aggregate2)
		{
			return VisitFunctionAggregate(aggregate2);
		}
		DbGroupAggregate aggregate3 = (DbGroupAggregate)aggregate;
		return VisitGroupAggregate(aggregate3);
	}

	protected virtual DbFunctionAggregate VisitFunctionAggregate(DbFunctionAggregate aggregate)
	{
		DbFunctionAggregate result = aggregate;
		if (aggregate != null)
		{
			EdmFunction edmFunction = VisitFunction(aggregate.Function);
			IList<DbExpression> list = VisitExpressionList(aggregate.Arguments);
			if (aggregate.Function != edmFunction || aggregate.Arguments != list)
			{
				result = ((!aggregate.Distinct) ? edmFunction.Aggregate(list) : edmFunction.AggregateDistinct(list));
			}
		}
		return result;
	}

	protected virtual DbGroupAggregate VisitGroupAggregate(DbGroupAggregate aggregate)
	{
		DbGroupAggregate result = aggregate;
		if (aggregate != null)
		{
			IList<DbExpression> list = VisitExpressionList(aggregate.Arguments);
			if (aggregate.Arguments != list)
			{
				result = DbExpressionBuilder.GroupAggregate(list[0]);
			}
		}
		return result;
	}

	protected virtual DbLambda VisitLambda(DbLambda lambda)
	{
		Check.NotNull(lambda, "lambda");
		DbLambda result = lambda;
		IList<DbVariableReferenceExpression> list = VisitList(lambda.Variables, delegate(DbVariableReferenceExpression varRef)
		{
			TypeUsage typeUsage = VisitTypeUsage(varRef.ResultType);
			return (varRef.ResultType != typeUsage) ? typeUsage.Variable(varRef.VariableName) : varRef;
		});
		EnterScope(list.ToArray());
		DbExpression dbExpression = VisitExpression(lambda.Body);
		ExitScope();
		if (lambda.Variables != list || lambda.Body != dbExpression)
		{
			result = DbExpressionBuilder.Lambda(dbExpression, list);
		}
		return result;
	}

	protected virtual EdmType VisitType(EdmType type)
	{
		return type;
	}

	protected virtual TypeUsage VisitTypeUsage(TypeUsage type)
	{
		return type;
	}

	protected virtual EntitySetBase VisitEntitySet(EntitySetBase entitySet)
	{
		return entitySet;
	}

	protected virtual EdmFunction VisitFunction(EdmFunction functionMetadata)
	{
		return functionMetadata;
	}

	private void NotifyIfChanged(DbExpression originalExpression, DbExpression newExpression)
	{
		if (originalExpression != newExpression)
		{
			OnExpressionReplaced(originalExpression, newExpression);
		}
	}

	private static IList<TElement> VisitList<TElement>(IList<TElement> list, Func<TElement, TElement> map)
	{
		IList<TElement> result = list;
		if (list != null)
		{
			List<TElement> list2 = null;
			for (int i = 0; i < list.Count; i++)
			{
				TElement val = map(list[i]);
				if (list2 == null && (object)list[i] != (object)val)
				{
					list2 = new List<TElement>(list);
					result = list2;
				}
				if (list2 != null)
				{
					list2[i] = val;
				}
			}
		}
		return result;
	}

	private DbExpression VisitUnary(DbUnaryExpression expression, Func<DbExpression, DbExpression> callback)
	{
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Argument);
		if (expression.Argument != dbExpression2)
		{
			dbExpression = callback(dbExpression2);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	private DbExpression VisitTypeUnary(DbUnaryExpression expression, TypeUsage type, Func<DbExpression, TypeUsage, DbExpression> callback)
	{
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Argument);
		TypeUsage typeUsage = VisitTypeUsage(type);
		if (expression.Argument != dbExpression2 || type != typeUsage)
		{
			dbExpression = callback(dbExpression2, typeUsage);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	private DbExpression VisitBinary(DbBinaryExpression expression, Func<DbExpression, DbExpression, DbExpression> callback)
	{
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Left);
		DbExpression dbExpression3 = VisitExpression(expression.Right);
		if (expression.Left != dbExpression2 || expression.Right != dbExpression3)
		{
			dbExpression = callback(dbExpression2, dbExpression3);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	private DbRelatedEntityRef VisitRelatedEntityRef(DbRelatedEntityRef entityRef)
	{
		VisitRelationshipEnds(entityRef.SourceEnd, entityRef.TargetEnd, out var newSource, out var newTarget);
		DbExpression dbExpression = VisitExpression(entityRef.TargetEntityReference);
		if (entityRef.SourceEnd != newSource || entityRef.TargetEnd != newTarget || entityRef.TargetEntityReference != dbExpression)
		{
			return DbExpressionBuilder.CreateRelatedEntityRef(newSource, newTarget, dbExpression);
		}
		return entityRef;
	}

	private void VisitRelationshipEnds(RelationshipEndMember source, RelationshipEndMember target, out RelationshipEndMember newSource, out RelationshipEndMember newTarget)
	{
		RelationshipType relationshipType = (RelationshipType)VisitType(target.DeclaringType);
		newSource = relationshipType.RelationshipEndMembers[source.Name];
		newTarget = relationshipType.RelationshipEndMembers[target.Name];
	}

	private DbExpression VisitTerminal(DbExpression expression, Func<TypeUsage, DbExpression> reconstructor)
	{
		DbExpression dbExpression = expression;
		TypeUsage typeUsage = VisitTypeUsage(expression.ResultType);
		if (expression.ResultType != typeUsage)
		{
			dbExpression = reconstructor(typeUsage);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	private void RebindVariable(DbVariableReferenceExpression from, DbVariableReferenceExpression to)
	{
		if (!from.VariableName.Equals(to.VariableName, StringComparison.Ordinal) || from.ResultType.EdmType != to.ResultType.EdmType || !from.ResultType.EdmEquals(to.ResultType))
		{
			varMappings[from] = to;
			OnVariableRebound(from, to);
		}
	}

	private DbExpressionBinding VisitExpressionBindingEnterScope(DbExpressionBinding binding)
	{
		DbExpressionBinding dbExpressionBinding = VisitExpressionBinding(binding);
		OnEnterScope(new DbVariableReferenceExpression[1] { dbExpressionBinding.Variable });
		return dbExpressionBinding;
	}

	private void EnterScope(params DbVariableReferenceExpression[] scopeVars)
	{
		OnEnterScope(scopeVars);
	}

	private void ExitScope()
	{
		OnExitScope();
	}

	public override DbExpression Visit(DbExpression expression)
	{
		Check.NotNull(expression, "expression");
		throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(expression.GetType().FullName));
	}

	public override DbExpression Visit(DbConstantExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitTerminal(expression, (TypeUsage newType) => newType.Constant(expression.GetValue()));
	}

	public override DbExpression Visit(DbNullExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitTerminal(expression, DbExpressionBuilder.Null);
	}

	public override DbExpression Visit(DbVariableReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		if (varMappings.TryGetValue(expression, out var value))
		{
			dbExpression = value;
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbParameterReferenceExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitTerminal(expression, (TypeUsage newType) => newType.Parameter(expression.ParameterName));
	}

	public override DbExpression Visit(DbFunctionExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		IList<DbExpression> list = VisitExpressionList(expression.Arguments);
		EdmFunction edmFunction = VisitFunction(expression.Function);
		if (expression.Arguments != list || expression.Function != edmFunction)
		{
			dbExpression = edmFunction.Invoke(list);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbLambdaExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		IList<DbExpression> list = VisitExpressionList(expression.Arguments);
		DbLambda dbLambda = VisitLambda(expression.Lambda);
		if (expression.Arguments != list || expression.Lambda != dbLambda)
		{
			dbExpression = dbLambda.Invoke(list);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbPropertyExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Instance);
		if (expression.Instance != dbExpression2)
		{
			dbExpression = dbExpression2.Property(expression.Property.Name);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbComparisonExpression expression)
	{
		Check.NotNull(expression, "expression");
		return expression.ExpressionKind switch
		{
			DbExpressionKind.Equals => VisitBinary(expression, DbExpressionBuilder.Equal), 
			DbExpressionKind.NotEquals => VisitBinary(expression, DbExpressionBuilder.NotEqual), 
			DbExpressionKind.GreaterThan => VisitBinary(expression, DbExpressionBuilder.GreaterThan), 
			DbExpressionKind.GreaterThanOrEquals => VisitBinary(expression, DbExpressionBuilder.GreaterThanOrEqual), 
			DbExpressionKind.LessThan => VisitBinary(expression, DbExpressionBuilder.LessThan), 
			DbExpressionKind.LessThanOrEquals => VisitBinary(expression, DbExpressionBuilder.LessThanOrEqual), 
			_ => throw new NotSupportedException(), 
		};
	}

	public override DbExpression Visit(DbLikeExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Argument);
		DbExpression dbExpression3 = VisitExpression(expression.Pattern);
		DbExpression dbExpression4 = VisitExpression(expression.Escape);
		if (expression.Argument != dbExpression2 || expression.Pattern != dbExpression3 || expression.Escape != dbExpression4)
		{
			dbExpression = dbExpression2.Like(dbExpression3, dbExpression4);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbLimitExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Argument);
		DbExpression dbExpression3 = VisitExpression(expression.Limit);
		if (expression.Argument != dbExpression2 || expression.Limit != dbExpression3)
		{
			dbExpression = dbExpression2.Limit(dbExpression3);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbIsNullExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.IsNull);
	}

	public override DbExpression Visit(DbArithmeticExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		IList<DbExpression> list = VisitExpressionList(expression.Arguments);
		if (expression.Arguments != list)
		{
			dbExpression = expression.ExpressionKind switch
			{
				DbExpressionKind.Divide => list[0].Divide(list[1]), 
				DbExpressionKind.Minus => list[0].Minus(list[1]), 
				DbExpressionKind.Modulo => list[0].Modulo(list[1]), 
				DbExpressionKind.Multiply => list[0].Multiply(list[1]), 
				DbExpressionKind.Plus => list[0].Plus(list[1]), 
				DbExpressionKind.UnaryMinus => list[0].UnaryMinus(), 
				_ => throw new NotSupportedException(), 
			};
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbAndExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitBinary(expression, DbExpressionBuilder.And);
	}

	public override DbExpression Visit(DbOrExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitBinary(expression, DbExpressionBuilder.Or);
	}

	public override DbExpression Visit(DbInExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpression dbExpression2 = VisitExpression(expression.Item);
		IList<DbExpression> list = VisitExpressionList(expression.List);
		if (expression.Item != dbExpression2 || expression.List != list)
		{
			dbExpression = DbExpressionBuilder.CreateInExpression(dbExpression2, list);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbNotExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.Not);
	}

	public override DbExpression Visit(DbDistinctExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.Distinct);
	}

	public override DbExpression Visit(DbElementExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, expression.IsSinglePropertyUnwrapped ? new Func<DbExpression, DbExpression>(DbExpressionBuilder.CreateElementExpressionUnwrapSingleProperty) : new Func<DbExpression, DbExpression>(DbExpressionBuilder.Element));
	}

	public override DbExpression Visit(DbIsEmptyExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.IsEmpty);
	}

	public override DbExpression Visit(DbUnionAllExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitBinary(expression, DbExpressionBuilder.UnionAll);
	}

	public override DbExpression Visit(DbIntersectExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitBinary(expression, DbExpressionBuilder.Intersect);
	}

	public override DbExpression Visit(DbExceptExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitBinary(expression, DbExpressionBuilder.Except);
	}

	public override DbExpression Visit(DbTreatExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitTypeUnary(expression, expression.ResultType, DbExpressionBuilder.TreatAs);
	}

	public override DbExpression Visit(DbIsOfExpression expression)
	{
		Check.NotNull(expression, "expression");
		if (expression.ExpressionKind == DbExpressionKind.IsOfOnly)
		{
			return VisitTypeUnary(expression, expression.OfType, DbExpressionBuilder.IsOfOnly);
		}
		return VisitTypeUnary(expression, expression.OfType, DbExpressionBuilder.IsOf);
	}

	public override DbExpression Visit(DbCastExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitTypeUnary(expression, expression.ResultType, DbExpressionBuilder.CastTo);
	}

	public override DbExpression Visit(DbCaseExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		IList<DbExpression> list = VisitExpressionList(expression.When);
		IList<DbExpression> list2 = VisitExpressionList(expression.Then);
		DbExpression dbExpression2 = VisitExpression(expression.Else);
		if (expression.When != list || expression.Then != list2 || expression.Else != dbExpression2)
		{
			dbExpression = DbExpressionBuilder.Case(list, list2, dbExpression2);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbOfTypeExpression expression)
	{
		Check.NotNull(expression, "expression");
		if (expression.ExpressionKind == DbExpressionKind.OfTypeOnly)
		{
			return VisitTypeUnary(expression, expression.OfType, DbExpressionBuilder.OfTypeOnly);
		}
		return VisitTypeUnary(expression, expression.OfType, DbExpressionBuilder.OfType);
	}

	public override DbExpression Visit(DbNewInstanceExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		TypeUsage typeUsage = VisitTypeUsage(expression.ResultType);
		IList<DbExpression> list = VisitExpressionList(expression.Arguments);
		bool flag = expression.ResultType == typeUsage && expression.Arguments == list;
		if (expression.HasRelatedEntityReferences)
		{
			IList<DbRelatedEntityRef> list2 = VisitList(expression.RelatedEntityReferences, VisitRelatedEntityRef);
			if (!flag || expression.RelatedEntityReferences != list2)
			{
				dbExpression = DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression((EntityType)typeUsage.EdmType, list, list2);
			}
		}
		else if (!flag)
		{
			dbExpression = typeUsage.New(list.ToArray());
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbRefExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		EntityType entityType = (EntityType)TypeHelpers.GetEdmType<RefType>(expression.ResultType).ElementType;
		DbExpression dbExpression2 = VisitExpression(expression.Argument);
		EntityType entityType2 = (EntityType)VisitType(entityType);
		EntitySet entitySet = (EntitySet)VisitEntitySet(expression.EntitySet);
		if (expression.Argument != dbExpression2 || entityType != entityType2 || expression.EntitySet != entitySet)
		{
			dbExpression = entitySet.RefFromKey(dbExpression2, entityType2);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbRelationshipNavigationExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		VisitRelationshipEnds(expression.NavigateFrom, expression.NavigateTo, out var newSource, out var newTarget);
		DbExpression dbExpression2 = VisitExpression(expression.NavigationSource);
		if (expression.NavigateFrom != newSource || expression.NavigateTo != newTarget || expression.NavigationSource != dbExpression2)
		{
			dbExpression = dbExpression2.Navigate(newSource, newTarget);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbDerefExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.Deref);
	}

	public override DbExpression Visit(DbRefKeyExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.GetRefKey);
	}

	public override DbExpression Visit(DbEntityRefExpression expression)
	{
		Check.NotNull(expression, "expression");
		return VisitUnary(expression, DbExpressionBuilder.GetEntityRef);
	}

	public override DbExpression Visit(DbScanExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		EntitySetBase entitySetBase = VisitEntitySet(expression.Target);
		if (expression.Target != entitySetBase)
		{
			dbExpression = entitySetBase.Scan();
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbFilterExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBindingEnterScope(expression.Input);
		DbExpression dbExpression2 = VisitExpression(expression.Predicate);
		ExitScope();
		if (expression.Input != dbExpressionBinding || expression.Predicate != dbExpression2)
		{
			dbExpression = dbExpressionBinding.Filter(dbExpression2);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbProjectExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBindingEnterScope(expression.Input);
		DbExpression dbExpression2 = VisitExpression(expression.Projection);
		ExitScope();
		if (expression.Input != dbExpressionBinding || expression.Projection != dbExpression2)
		{
			dbExpression = dbExpressionBinding.Project(dbExpression2);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbCrossJoinExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		IList<DbExpressionBinding> list = VisitExpressionBindingList(expression.Inputs);
		if (expression.Inputs != list)
		{
			dbExpression = DbExpressionBuilder.CrossJoin(list);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbJoinExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBinding(expression.Left);
		DbExpressionBinding dbExpressionBinding2 = VisitExpressionBinding(expression.Right);
		EnterScope(dbExpressionBinding.Variable, dbExpressionBinding2.Variable);
		DbExpression dbExpression2 = VisitExpression(expression.JoinCondition);
		ExitScope();
		if (expression.Left != dbExpressionBinding || expression.Right != dbExpressionBinding2 || expression.JoinCondition != dbExpression2)
		{
			dbExpression = ((DbExpressionKind.InnerJoin == expression.ExpressionKind) ? dbExpressionBinding.InnerJoin(dbExpressionBinding2, dbExpression2) : ((DbExpressionKind.LeftOuterJoin != expression.ExpressionKind) ? dbExpressionBinding.FullOuterJoin(dbExpressionBinding2, dbExpression2) : dbExpressionBinding.LeftOuterJoin(dbExpressionBinding2, dbExpression2)));
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbApplyExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBindingEnterScope(expression.Input);
		DbExpressionBinding dbExpressionBinding2 = VisitExpressionBinding(expression.Apply);
		ExitScope();
		if (expression.Input != dbExpressionBinding || expression.Apply != dbExpressionBinding2)
		{
			dbExpression = ((DbExpressionKind.CrossApply != expression.ExpressionKind) ? dbExpressionBinding.OuterApply(dbExpressionBinding2) : dbExpressionBinding.CrossApply(dbExpressionBinding2));
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbGroupByExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbGroupExpressionBinding dbGroupExpressionBinding = VisitGroupExpressionBinding(expression.Input);
		EnterScope(dbGroupExpressionBinding.Variable);
		IList<DbExpression> list = VisitExpressionList(expression.Keys);
		ExitScope();
		EnterScope(dbGroupExpressionBinding.GroupVariable);
		IList<DbAggregate> list2 = VisitList(expression.Aggregates, VisitAggregate);
		ExitScope();
		if (expression.Input != dbGroupExpressionBinding || expression.Keys != list || expression.Aggregates != list2)
		{
			RowType edmType = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(expression.ResultType).TypeUsage);
			List<KeyValuePair<string, DbExpression>> keys = (from p in edmType.Properties.Take(list.Count)
				select p.Name).Zip(list).ToList();
			List<KeyValuePair<string, DbAggregate>> aggregates = (from p in edmType.Properties.Skip(list.Count)
				select p.Name).Zip(list2).ToList();
			dbExpression = dbGroupExpressionBinding.GroupBy(keys, aggregates);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbSkipExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBindingEnterScope(expression.Input);
		IList<DbSortClause> list = VisitSortOrder(expression.SortOrder);
		ExitScope();
		DbExpression dbExpression2 = VisitExpression(expression.Count);
		if (expression.Input != dbExpressionBinding || expression.SortOrder != list || expression.Count != dbExpression2)
		{
			dbExpression = dbExpressionBinding.Skip(list, dbExpression2);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbSortExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBindingEnterScope(expression.Input);
		IList<DbSortClause> list = VisitSortOrder(expression.SortOrder);
		ExitScope();
		if (expression.Input != dbExpressionBinding || expression.SortOrder != list)
		{
			dbExpression = dbExpressionBinding.Sort(list);
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}

	public override DbExpression Visit(DbQuantifierExpression expression)
	{
		Check.NotNull(expression, "expression");
		DbExpression dbExpression = expression;
		DbExpressionBinding dbExpressionBinding = VisitExpressionBindingEnterScope(expression.Input);
		DbExpression dbExpression2 = VisitExpression(expression.Predicate);
		ExitScope();
		if (expression.Input != dbExpressionBinding || expression.Predicate != dbExpression2)
		{
			dbExpression = ((expression.ExpressionKind != 0) ? dbExpressionBinding.Any(dbExpression2) : dbExpressionBinding.All(dbExpression2));
		}
		NotifyIfChanged(expression, dbExpression);
		return dbExpression;
	}
}
