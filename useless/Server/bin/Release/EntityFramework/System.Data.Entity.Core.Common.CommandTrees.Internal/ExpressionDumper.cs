using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal abstract class ExpressionDumper : DbExpressionVisitor
{
	internal void Begin(string name)
	{
		Begin(name, null);
	}

	internal abstract void Begin(string name, Dictionary<string, object> attrs);

	internal abstract void End(string name);

	internal void Dump(DbExpression target)
	{
		target.Accept(this);
	}

	internal void Dump(DbExpression e, string name)
	{
		Begin(name);
		Dump(e);
		End(name);
	}

	internal void Dump(DbExpressionBinding binding, string name)
	{
		Begin(name);
		Dump(binding);
		End(name);
	}

	internal void Dump(DbExpressionBinding binding)
	{
		Begin("DbExpressionBinding", "VariableName", binding.VariableName);
		Begin("Expression");
		Dump(binding.Expression);
		End("Expression");
		End("DbExpressionBinding");
	}

	internal void Dump(DbGroupExpressionBinding binding, string name)
	{
		Begin(name);
		Dump(binding);
		End(name);
	}

	internal void Dump(DbGroupExpressionBinding binding)
	{
		Begin("DbGroupExpressionBinding", "VariableName", binding.VariableName, "GroupVariableName", binding.GroupVariableName);
		Begin("Expression");
		Dump(binding.Expression);
		End("Expression");
		End("DbGroupExpressionBinding");
	}

	internal void Dump(IEnumerable<DbExpression> exprs, string pluralName, string singularName)
	{
		Begin(pluralName);
		foreach (DbExpression expr in exprs)
		{
			Begin(singularName);
			Dump(expr);
			End(singularName);
		}
		End(pluralName);
	}

	internal void Dump(IEnumerable<FunctionParameter> paramList)
	{
		Begin("Parameters");
		foreach (FunctionParameter param in paramList)
		{
			Begin("Parameter", "Name", param.Name);
			Dump(param.TypeUsage, "ParameterType");
			End("Parameter");
		}
		End("Parameters");
	}

	internal void Dump(TypeUsage type, string name)
	{
		Begin(name);
		Dump(type);
		End(name);
	}

	internal void Dump(TypeUsage type)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (Facet facet in type.Facets)
		{
			dictionary.Add(facet.Name, facet.Value);
		}
		Begin("TypeUsage", dictionary);
		Dump(type.EdmType);
		End("TypeUsage");
	}

	internal void Dump(EdmType type, string name)
	{
		Begin(name);
		Dump(type);
		End(name);
	}

	internal void Dump(EdmType type)
	{
		Begin("EdmType", "BuiltInTypeKind", Enum.GetName(typeof(BuiltInTypeKind), type.BuiltInTypeKind), "Namespace", type.NamespaceName, "Name", type.Name);
		End("EdmType");
	}

	internal void Dump(RelationshipType type, string name)
	{
		Begin(name);
		Dump(type);
		End(name);
	}

	internal void Dump(RelationshipType type)
	{
		Begin("RelationshipType", "Namespace", type.NamespaceName, "Name", type.Name);
		End("RelationshipType");
	}

	internal void Dump(EdmFunction function)
	{
		Begin("Function", "Name", function.Name, "Namespace", function.NamespaceName);
		Dump(function.Parameters);
		if (function.ReturnParameters.Count == 1)
		{
			Dump(function.ReturnParameters[0].TypeUsage, "ReturnType");
		}
		else
		{
			Begin("ReturnTypes");
			foreach (FunctionParameter returnParameter in function.ReturnParameters)
			{
				Dump(returnParameter.TypeUsage, returnParameter.Name);
			}
			End("ReturnTypes");
		}
		End("Function");
	}

	internal void Dump(EdmProperty prop)
	{
		Begin("Property", "Name", prop.Name, "Nullable", prop.Nullable);
		Dump(prop.DeclaringType, "DeclaringType");
		Dump(prop.TypeUsage, "PropertyType");
		End("Property");
	}

	internal void Dump(RelationshipEndMember end, string name)
	{
		Begin(name);
		Begin("RelationshipEndMember", "Name", end.Name, "RelationshipMultiplicity", Enum.GetName(typeof(RelationshipMultiplicity), end.RelationshipMultiplicity));
		Dump(end.DeclaringType, "DeclaringRelation");
		Dump(end.TypeUsage, "EndType");
		End("RelationshipEndMember");
		End(name);
	}

	internal void Dump(NavigationProperty navProp, string name)
	{
		Begin(name);
		Begin("NavigationProperty", "Name", navProp.Name, "RelationshipTypeName", navProp.RelationshipType.FullName, "ToEndMemberName", navProp.ToEndMember.Name);
		Dump(navProp.DeclaringType, "DeclaringType");
		Dump(navProp.TypeUsage, "PropertyType");
		End("NavigationProperty");
		End(name);
	}

	internal void Dump(DbLambda lambda)
	{
		Begin("DbLambda");
		Dump(lambda.Variables.Cast<DbExpression>(), "Variables", "Variable");
		Dump(lambda.Body, "Body");
		End("DbLambda");
	}

	private void Begin(DbExpression expr)
	{
		Begin(expr, new Dictionary<string, object>());
	}

	private void Begin(DbExpression expr, Dictionary<string, object> attrs)
	{
		attrs.Add("DbExpressionKind", Enum.GetName(typeof(DbExpressionKind), expr.ExpressionKind));
		Begin(expr.GetType().Name, attrs);
		Dump(expr.ResultType, "ResultType");
	}

	private void Begin(DbExpression expr, string attributeName, object attributeValue)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(attributeName, attributeValue);
		Begin(expr, dictionary);
	}

	private void Begin(string expr, string attributeName, object attributeValue)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(attributeName, attributeValue);
		Begin(expr, dictionary);
	}

	private void Begin(string expr, string attributeName1, object attributeValue1, string attributeName2, object attributeValue2)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(attributeName1, attributeValue1);
		dictionary.Add(attributeName2, attributeValue2);
		Begin(expr, dictionary);
	}

	private void Begin(string expr, string attributeName1, object attributeValue1, string attributeName2, object attributeValue2, string attributeName3, object attributeValue3)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add(attributeName1, attributeValue1);
		dictionary.Add(attributeName2, attributeValue2);
		dictionary.Add(attributeName3, attributeValue3);
		Begin(expr, dictionary);
	}

	private void End(DbExpression expr)
	{
		End(expr.GetType().Name);
	}

	private void BeginUnary(DbUnaryExpression e)
	{
		Begin(e);
		Begin("Argument");
		Dump(e.Argument);
		End("Argument");
	}

	private void BeginBinary(DbBinaryExpression e)
	{
		Begin(e);
		Begin("Left");
		Dump(e.Left);
		End("Left");
		Begin("Right");
		Dump(e.Right);
		End("Right");
	}

	public override void Visit(DbExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		End(e);
	}

	public override void Visit(DbConstantExpression e)
	{
		Check.NotNull(e, "e");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Value", e.Value);
		Begin(e, dictionary);
		End(e);
	}

	public override void Visit(DbNullExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		End(e);
	}

	public override void Visit(DbVariableReferenceExpression e)
	{
		Check.NotNull(e, "e");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("VariableName", e.VariableName);
		Begin(e, dictionary);
		End(e);
	}

	public override void Visit(DbParameterReferenceExpression e)
	{
		Check.NotNull(e, "e");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("ParameterName", e.ParameterName);
		Begin(e, dictionary);
		End(e);
	}

	public override void Visit(DbFunctionExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Function);
		Dump(e.Arguments, "Arguments", "Argument");
		End(e);
	}

	public override void Visit(DbLambdaExpression expression)
	{
		Check.NotNull(expression, "expression");
		Begin(expression);
		Dump(expression.Lambda);
		Dump(expression.Arguments, "Arguments", "Argument");
		End(expression);
	}

	public override void Visit(DbPropertyExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		if (e.Property is RelationshipEndMember end)
		{
			Dump(end, "Property");
		}
		else if (Helper.IsNavigationProperty(e.Property))
		{
			Dump((NavigationProperty)e.Property, "Property");
		}
		else
		{
			Dump((EdmProperty)e.Property);
		}
		if (e.Instance != null)
		{
			Dump(e.Instance, "Instance");
		}
		End(e);
	}

	public override void Visit(DbComparisonExpression e)
	{
		Check.NotNull(e, "e");
		BeginBinary(e);
		End(e);
	}

	public override void Visit(DbLikeExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Argument, "Argument");
		Dump(e.Pattern, "Pattern");
		Dump(e.Escape, "Escape");
		End(e);
	}

	public override void Visit(DbLimitExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e, "WithTies", e.WithTies);
		Dump(e.Argument, "Argument");
		Dump(e.Limit, "Limit");
		End(e);
	}

	public override void Visit(DbIsNullExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbArithmeticExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Arguments, "Arguments", "Argument");
		End(e);
	}

	public override void Visit(DbAndExpression e)
	{
		Check.NotNull(e, "e");
		BeginBinary(e);
		End(e);
	}

	public override void Visit(DbOrExpression e)
	{
		Check.NotNull(e, "e");
		BeginBinary(e);
		End(e);
	}

	public override void Visit(DbInExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Item);
		Dump(e.List, "List", "Item");
		End(e);
	}

	public override void Visit(DbNotExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbDistinctExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbElementExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbIsEmptyExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbUnionAllExpression e)
	{
		Check.NotNull(e, "e");
		BeginBinary(e);
		End(e);
	}

	public override void Visit(DbIntersectExpression e)
	{
		Check.NotNull(e, "e");
		BeginBinary(e);
		End(e);
	}

	public override void Visit(DbExceptExpression e)
	{
		Check.NotNull(e, "e");
		BeginBinary(e);
		End(e);
	}

	public override void Visit(DbTreatExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbIsOfExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		Dump(e.OfType, "OfType");
		End(e);
	}

	public override void Visit(DbCastExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbCaseExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.When, "Whens", "When");
		Dump(e.Then, "Thens", "Then");
		Dump(e.Else, "Else");
	}

	public override void Visit(DbOfTypeExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		Dump(e.OfType, "OfType");
		End(e);
	}

	public override void Visit(DbNewInstanceExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Arguments, "Arguments", "Argument");
		if (e.HasRelatedEntityReferences)
		{
			Begin("RelatedEntityReferences");
			foreach (DbRelatedEntityRef relatedEntityReference in e.RelatedEntityReferences)
			{
				Begin("DbRelatedEntityRef");
				Dump(relatedEntityReference.SourceEnd, "SourceEnd");
				Dump(relatedEntityReference.TargetEnd, "TargetEnd");
				Dump(relatedEntityReference.TargetEntityReference, "TargetEntityReference");
				End("DbRelatedEntityRef");
			}
			End("RelatedEntityReferences");
		}
		End(e);
	}

	public override void Visit(DbRelationshipNavigationExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.NavigateFrom, "NavigateFrom");
		Dump(e.NavigateTo, "NavigateTo");
		Dump(e.Relationship, "Relationship");
		Dump(e.NavigationSource, "NavigationSource");
		End(e);
	}

	public override void Visit(DbRefExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbDerefExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbRefKeyExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbEntityRefExpression e)
	{
		Check.NotNull(e, "e");
		BeginUnary(e);
		End(e);
	}

	public override void Visit(DbScanExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Begin("Target", "Name", e.Target.Name, "Container", e.Target.EntityContainer.Name);
		Dump(e.Target.ElementType, "TargetElementType");
		End("Target");
		End(e);
	}

	public override void Visit(DbFilterExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.Predicate, "Predicate");
		End(e);
	}

	public override void Visit(DbProjectExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.Projection, "Projection");
		End(e);
	}

	public override void Visit(DbCrossJoinExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Begin("Inputs");
		foreach (DbExpressionBinding input in e.Inputs)
		{
			Dump(input, "Input");
		}
		End("Inputs");
		End(e);
	}

	public override void Visit(DbJoinExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Left, "Left");
		Dump(e.Right, "Right");
		Dump(e.JoinCondition, "JoinCondition");
		End(e);
	}

	public override void Visit(DbApplyExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.Apply, "Apply");
		End(e);
	}

	public override void Visit(DbGroupByExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.Keys, "Keys", "Key");
		Begin("Aggregates");
		foreach (DbAggregate aggregate in e.Aggregates)
		{
			if (aggregate is DbFunctionAggregate dbFunctionAggregate)
			{
				Begin("DbFunctionAggregate");
				Dump(dbFunctionAggregate.Function);
				Dump(dbFunctionAggregate.Arguments, "Arguments", "Argument");
				End("DbFunctionAggregate");
			}
			else
			{
				DbGroupAggregate dbGroupAggregate = aggregate as DbGroupAggregate;
				Begin("DbGroupAggregate");
				Dump(dbGroupAggregate.Arguments, "Arguments", "Argument");
				End("DbGroupAggregate");
			}
		}
		End("Aggregates");
		End(e);
	}

	protected virtual void Dump(IList<DbSortClause> sortOrder)
	{
		Begin("SortOrder");
		foreach (DbSortClause item in sortOrder)
		{
			string text = item.Collation;
			if (text == null)
			{
				text = "";
			}
			Begin("DbSortClause", "Ascending", item.Ascending, "Collation", text);
			Dump(item.Expression, "Expression");
			End("DbSortClause");
		}
		End("SortOrder");
	}

	public override void Visit(DbSkipExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.SortOrder);
		Dump(e.Count, "Count");
		End(e);
	}

	public override void Visit(DbSortExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.SortOrder);
		End(e);
	}

	public override void Visit(DbQuantifierExpression e)
	{
		Check.NotNull(e, "e");
		Begin(e);
		Dump(e.Input, "Input");
		Dump(e.Predicate, "Predicate");
		End(e);
	}
}
