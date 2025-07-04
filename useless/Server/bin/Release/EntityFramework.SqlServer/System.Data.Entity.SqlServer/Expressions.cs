using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.SqlServer;

internal static class Expressions
{
	internal sealed class ConditionalExpressionBuilder
	{
		private readonly Expression condition;

		private readonly Expression ifTrueThen;

		internal ConditionalExpressionBuilder(Expression conditionExpression, Expression ifTrueExpression)
		{
			condition = conditionExpression;
			ifTrueThen = ifTrueExpression;
		}

		internal Expression Else(Expression resultIfFalse)
		{
			return Expression.Condition(condition, ifTrueThen, resultIfFalse);
		}
	}

	internal static Expression Null<TNullType>()
	{
		return Expression.Constant(null, typeof(TNullType));
	}

	internal static Expression Null(Type nullType)
	{
		return Expression.Constant(null, nullType);
	}

	internal static Expression<Func<TArg, TResult>> Lambda<TArg, TResult>(string argumentName, Func<ParameterExpression, Expression> createLambdaBodyGivenParameter)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(TArg), argumentName);
		return Expression.Lambda<Func<TArg, TResult>>(createLambdaBodyGivenParameter(parameterExpression), new ParameterExpression[1] { parameterExpression });
	}

	internal static Expression Call(this Expression exp, string methodName)
	{
		return Expression.Call(exp, methodName, Type.EmptyTypes);
	}

	internal static Expression ConvertTo(this Expression exp, Type convertToType)
	{
		return Expression.Convert(exp, convertToType);
	}

	internal static Expression ConvertTo<TConvertToType>(this Expression exp)
	{
		return Expression.Convert(exp, typeof(TConvertToType));
	}

	internal static ConditionalExpressionBuilder IfTrueThen(this Expression conditionExp, Expression resultIfTrue)
	{
		return new ConditionalExpressionBuilder(conditionExp, resultIfTrue);
	}

	internal static Expression Property<TPropertyType>(this Expression exp, string propertyName)
	{
		PropertyInfo runtimeProperty = exp.Type.GetRuntimeProperty(propertyName);
		return Expression.Property(exp, runtimeProperty);
	}
}
