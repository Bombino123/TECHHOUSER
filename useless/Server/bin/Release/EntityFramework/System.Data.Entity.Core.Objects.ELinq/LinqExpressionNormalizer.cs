using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq;

internal class LinqExpressionNormalizer : EntityExpressionVisitor
{
	private abstract class Pattern
	{
		internal abstract PatternKind Kind { get; }
	}

	private enum PatternKind
	{
		Compare
	}

	private sealed class ComparePattern : Pattern
	{
		internal readonly Expression Left;

		internal readonly Expression Right;

		internal override PatternKind Kind => PatternKind.Compare;

		internal ComparePattern(Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}
	}

	private const bool LiftToNull = false;

	private readonly Dictionary<Expression, Pattern> _patterns = new Dictionary<Expression, Pattern>();

	internal static readonly MethodInfo RelationalOperatorPlaceholderMethod = typeof(LinqExpressionNormalizer).GetOnlyDeclaredMethod("RelationalOperatorPlaceholder");

	internal override Expression VisitBinary(BinaryExpression b)
	{
		b = (BinaryExpression)base.VisitBinary(b);
		if (b.NodeType == ExpressionType.Equal)
		{
			Expression expression = UnwrapObjectConvert(b.Left);
			Expression expression2 = UnwrapObjectConvert(b.Right);
			if (expression != b.Left || expression2 != b.Right)
			{
				b = CreateRelationalOperator(ExpressionType.Equal, expression, expression2);
			}
		}
		if (_patterns.TryGetValue(b.Left, out var value) && value.Kind == PatternKind.Compare && IsConstantZero(b.Right))
		{
			ComparePattern comparePattern = (ComparePattern)value;
			if (TryCreateRelationalOperator(b.NodeType, comparePattern.Left, comparePattern.Right, out var result))
			{
				b = result;
			}
		}
		return b;
	}

	private static Expression UnwrapObjectConvert(Expression input)
	{
		if (input.NodeType == ExpressionType.Constant && input.Type == typeof(object))
		{
			ConstantExpression constantExpression = (ConstantExpression)input;
			if (constantExpression.Value != null && constantExpression.Value.GetType() != typeof(object))
			{
				return Expression.Constant(constantExpression.Value, constantExpression.Value.GetType());
			}
		}
		while (ExpressionType.Convert == input.NodeType && typeof(object) == input.Type)
		{
			input = ((UnaryExpression)input).Operand;
		}
		return input;
	}

	private static bool IsConstantZero(Expression expression)
	{
		if (expression.NodeType == ExpressionType.Constant)
		{
			return ((ConstantExpression)expression).Value.Equals(0);
		}
		return false;
	}

	internal override Expression VisitMethodCall(MethodCallExpression m)
	{
		m = (MethodCallExpression)base.VisitMethodCall(m);
		if (m.Method.IsStatic)
		{
			if (m.Method.Name.StartsWith("op_", StringComparison.Ordinal))
			{
				if (m.Arguments.Count == 2)
				{
					switch (m.Method.Name)
					{
					case "op_Equality":
						return Expression.Equal(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
					case "op_Inequality":
						return Expression.NotEqual(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
					case "op_GreaterThan":
						return Expression.GreaterThan(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
					case "op_GreaterThanOrEqual":
						return Expression.GreaterThanOrEqual(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
					case "op_LessThan":
						return Expression.LessThan(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
					case "op_LessThanOrEqual":
						return Expression.LessThanOrEqual(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
					case "op_Multiply":
						return Expression.Multiply(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_Subtraction":
						return Expression.Subtract(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_Addition":
						return Expression.Add(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_Division":
						return Expression.Divide(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_Modulus":
						return Expression.Modulo(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_BitwiseAnd":
						return Expression.And(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_BitwiseOr":
						return Expression.Or(m.Arguments[0], m.Arguments[1], m.Method);
					case "op_ExclusiveOr":
						return Expression.ExclusiveOr(m.Arguments[0], m.Arguments[1], m.Method);
					}
				}
				if (m.Arguments.Count == 1)
				{
					switch (m.Method.Name)
					{
					case "op_UnaryNegation":
						return Expression.Negate(m.Arguments[0], m.Method);
					case "op_UnaryPlus":
						return Expression.UnaryPlus(m.Arguments[0], m.Method);
					case "op_Explicit":
					case "op_Implicit":
						return Expression.Convert(m.Arguments[0], m.Type, m.Method);
					case "op_OnesComplement":
					case "op_False":
						return Expression.Not(m.Arguments[0], m.Method);
					}
				}
			}
			if (m.Method.Name == "Equals" && m.Arguments.Count > 1)
			{
				return Expression.Equal(m.Arguments[0], m.Arguments[1], liftToNull: false, m.Method);
			}
			if (m.Method.Name == "CompareString" && (m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators" || m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.EmbeddedOperators"))
			{
				return CreateCompareExpression(m.Arguments[0], m.Arguments[1]);
			}
			if (m.Method.Name == "Compare" && m.Arguments.Count > 1 && m.Method.ReturnType == typeof(int))
			{
				return CreateCompareExpression(m.Arguments[0], m.Arguments[1]);
			}
		}
		else
		{
			if (m.Method.Name == "Equals" && m.Arguments.Count > 0)
			{
				Type parameterType = m.Method.GetParameters()[0].ParameterType;
				if (parameterType != typeof(DbGeography) && parameterType != typeof(DbGeometry))
				{
					return CreateRelationalOperator(ExpressionType.Equal, m.Object, m.Arguments[0]);
				}
			}
			if (m.Method.Name == "CompareTo" && m.Arguments.Count == 1 && m.Method.ReturnType == typeof(int))
			{
				return CreateCompareExpression(m.Object, m.Arguments[0]);
			}
			if (m.Method.Name == "Contains" && m.Arguments.Count == 1)
			{
				Type declaringType = m.Method.DeclaringType;
				if (declaringType.IsGenericType() && declaringType.GetGenericTypeDefinition() == typeof(List<>) && ReflectionUtil.TryLookupMethod(SequenceMethod.Contains, out var method))
				{
					return Expression.Call(method.MakeGenericMethod(declaringType.GetGenericArguments()), m.Object, m.Arguments[0]);
				}
			}
		}
		return NormalizePredicateArgument(m);
	}

	private static MethodCallExpression NormalizePredicateArgument(MethodCallExpression callExpression)
	{
		if (HasPredicateArgument(callExpression, out var argumentOrdinal) && TryMatchCoalescePattern(callExpression.Arguments[argumentOrdinal], out var normalized))
		{
			List<Expression> list = new List<Expression>(callExpression.Arguments);
			list[argumentOrdinal] = normalized;
			return Expression.Call(callExpression.Object, callExpression.Method, list);
		}
		return callExpression;
	}

	private static bool HasPredicateArgument(MethodCallExpression callExpression, out int argumentOrdinal)
	{
		argumentOrdinal = 0;
		bool result = false;
		if (2 <= callExpression.Arguments.Count && ReflectionUtil.TryIdentifySequenceMethod(callExpression.Method, out var sequenceMethod))
		{
			switch (sequenceMethod)
			{
			case SequenceMethod.Where:
			case SequenceMethod.WhereOrdinal:
			case SequenceMethod.TakeWhile:
			case SequenceMethod.TakeWhileOrdinal:
			case SequenceMethod.SkipWhile:
			case SequenceMethod.SkipWhileOrdinal:
			case SequenceMethod.FirstPredicate:
			case SequenceMethod.FirstOrDefaultPredicate:
			case SequenceMethod.LastPredicate:
			case SequenceMethod.LastOrDefaultPredicate:
			case SequenceMethod.SinglePredicate:
			case SequenceMethod.SingleOrDefaultPredicate:
			case SequenceMethod.AnyPredicate:
			case SequenceMethod.All:
			case SequenceMethod.CountPredicate:
			case SequenceMethod.LongCountPredicate:
				argumentOrdinal = 1;
				result = true;
				break;
			}
		}
		return result;
	}

	private static bool TryMatchCoalescePattern(Expression expression, out Expression normalized)
	{
		normalized = null;
		bool result = false;
		if (expression.NodeType == ExpressionType.Quote)
		{
			if (TryMatchCoalescePattern(((UnaryExpression)expression).Operand, out normalized))
			{
				result = true;
				normalized = Expression.Quote(normalized);
			}
		}
		else if (expression.NodeType == ExpressionType.Lambda)
		{
			LambdaExpression lambdaExpression = (LambdaExpression)expression;
			if (lambdaExpression.Body.NodeType == ExpressionType.Coalesce && lambdaExpression.Body.Type == typeof(bool))
			{
				BinaryExpression binaryExpression = (BinaryExpression)lambdaExpression.Body;
				if (binaryExpression.Right.NodeType == ExpressionType.Constant && false.Equals(((ConstantExpression)binaryExpression.Right).Value))
				{
					normalized = Expression.Lambda(lambdaExpression.Type, Expression.Convert(binaryExpression.Left, typeof(bool)), lambdaExpression.Parameters);
					result = true;
				}
			}
		}
		return result;
	}

	private static bool RelationalOperatorPlaceholder<TLeft, TRight>(TLeft left, TRight right)
	{
		return (object)left == (object)right;
	}

	private static BinaryExpression CreateRelationalOperator(ExpressionType op, Expression left, Expression right)
	{
		TryCreateRelationalOperator(op, left, right, out var result);
		return result;
	}

	private static bool TryCreateRelationalOperator(ExpressionType op, Expression left, Expression right, out BinaryExpression result)
	{
		MethodInfo method = RelationalOperatorPlaceholderMethod.MakeGenericMethod(left.Type, right.Type);
		switch (op)
		{
		case ExpressionType.Equal:
			result = Expression.Equal(left, right, liftToNull: false, method);
			return true;
		case ExpressionType.NotEqual:
			result = Expression.NotEqual(left, right, liftToNull: false, method);
			return true;
		case ExpressionType.LessThan:
			result = Expression.LessThan(left, right, liftToNull: false, method);
			return true;
		case ExpressionType.LessThanOrEqual:
			result = Expression.LessThanOrEqual(left, right, liftToNull: false, method);
			return true;
		case ExpressionType.GreaterThan:
			result = Expression.GreaterThan(left, right, liftToNull: false, method);
			return true;
		case ExpressionType.GreaterThanOrEqual:
			result = Expression.GreaterThanOrEqual(left, right, liftToNull: false, method);
			return true;
		default:
			result = null;
			return false;
		}
	}

	private Expression CreateCompareExpression(Expression left, Expression right)
	{
		Expression expression = Expression.Condition(CreateRelationalOperator(ExpressionType.Equal, left, right), Expression.Constant(0), Expression.Condition(CreateRelationalOperator(ExpressionType.GreaterThan, left, right), Expression.Constant(1), Expression.Constant(-1)));
		_patterns[expression] = new ComparePattern(left, right);
		return expression;
	}
}
