using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Utilities;

internal static class ExpressionExtensions
{
	public static PropertyPath GetSimplePropertyAccess(this LambdaExpression propertyAccessExpression)
	{
		PropertyPath propertyPath = propertyAccessExpression.Parameters.Single().MatchSimplePropertyAccess(propertyAccessExpression.Body);
		if (propertyPath == null)
		{
			throw Error.InvalidPropertyExpression(propertyAccessExpression);
		}
		return propertyPath;
	}

	public static PropertyPath GetComplexPropertyAccess(this LambdaExpression propertyAccessExpression)
	{
		PropertyPath propertyPath = propertyAccessExpression.Parameters.Single().MatchComplexPropertyAccess(propertyAccessExpression.Body);
		if (propertyPath == null)
		{
			throw Error.InvalidComplexPropertyExpression(propertyAccessExpression);
		}
		return propertyPath;
	}

	public static IEnumerable<PropertyPath> GetSimplePropertyAccessList(this LambdaExpression propertyAccessExpression)
	{
		return propertyAccessExpression.MatchPropertyAccessList((Expression p, Expression e) => e.MatchSimplePropertyAccess(p)) ?? throw Error.InvalidPropertiesExpression(propertyAccessExpression);
	}

	public static IEnumerable<PropertyPath> GetComplexPropertyAccessList(this LambdaExpression propertyAccessExpression)
	{
		return propertyAccessExpression.MatchPropertyAccessList((Expression p, Expression e) => e.MatchComplexPropertyAccess(p)) ?? throw Error.InvalidComplexPropertiesExpression(propertyAccessExpression);
	}

	private static IEnumerable<PropertyPath> MatchPropertyAccessList(this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyPath> propertyMatcher)
	{
		if (lambdaExpression.Body.RemoveConvert() is NewExpression newExpression)
		{
			ParameterExpression parameterExpression = lambdaExpression.Parameters.Single();
			IEnumerable<PropertyPath> enumerable = from a in newExpression.Arguments
				select propertyMatcher(a, parameterExpression) into p
				where p != null
				select p;
			if (enumerable.Count() == newExpression.Arguments.Count())
			{
				if (!newExpression.HasDefaultMembersOnly(enumerable))
				{
					return null;
				}
				return enumerable;
			}
		}
		PropertyPath propertyPath = propertyMatcher(lambdaExpression.Body, lambdaExpression.Parameters.Single());
		if (!(propertyPath != null))
		{
			return null;
		}
		return new PropertyPath[1] { propertyPath };
	}

	private static bool HasDefaultMembersOnly(this NewExpression newExpression, IEnumerable<PropertyPath> propertyPaths)
	{
		if (newExpression.Members != null)
		{
			return !newExpression.Members.Where((MemberInfo t, int i) => !string.Equals(t.Name, propertyPaths.ElementAt(i).Last().Name, StringComparison.Ordinal)).Any();
		}
		return true;
	}

	private static PropertyPath MatchSimplePropertyAccess(this Expression parameterExpression, Expression propertyAccessExpression)
	{
		PropertyPath propertyPath = parameterExpression.MatchPropertyAccess(propertyAccessExpression);
		if (!(propertyPath != null) || propertyPath.Count != 1)
		{
			return null;
		}
		return propertyPath;
	}

	private static PropertyPath MatchComplexPropertyAccess(this Expression parameterExpression, Expression propertyAccessExpression)
	{
		return parameterExpression.MatchPropertyAccess(propertyAccessExpression);
	}

	private static PropertyPath MatchPropertyAccess(this Expression parameterExpression, Expression propertyAccessExpression)
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		do
		{
			if (!(propertyAccessExpression.RemoveConvert() is MemberExpression memberExpression))
			{
				return null;
			}
			PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
			if (propertyInfo == null)
			{
				return null;
			}
			list.Insert(0, propertyInfo);
			propertyAccessExpression = memberExpression.Expression;
		}
		while (memberExpression.Expression != parameterExpression);
		return new PropertyPath(list);
	}

	public static Expression RemoveConvert(this Expression expression)
	{
		while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
		{
			expression = ((UnaryExpression)expression).Operand;
		}
		return expression;
	}

	public static bool IsNullConstant(this Expression expression)
	{
		expression = expression.RemoveConvert();
		if (expression.NodeType != ExpressionType.Constant)
		{
			return false;
		}
		return ((ConstantExpression)expression).Value == null;
	}

	public static bool IsStringAddExpression(this Expression expression)
	{
		if (!(expression is BinaryExpression binaryExpression))
		{
			return false;
		}
		if (binaryExpression.Method == null || binaryExpression.NodeType != 0)
		{
			return false;
		}
		if (binaryExpression.Method.DeclaringType == typeof(string))
		{
			return string.Equals(binaryExpression.Method.Name, "Concat", StringComparison.Ordinal);
		}
		return false;
	}
}
