using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq;

internal sealed class QueryParameterExpression : Expression
{
	private readonly DbParameterReferenceExpression _parameterReference;

	private readonly Type _type;

	private readonly Expression _funcletizedExpression;

	private readonly IEnumerable<ParameterExpression> _compiledQueryParameters;

	private Delegate _cachedDelegate;

	internal DbParameterReferenceExpression ParameterReference => _parameterReference;

	public override Type Type => _type;

	public override ExpressionType NodeType => (ExpressionType)(-1);

	internal QueryParameterExpression(DbParameterReferenceExpression parameterReference, Expression funcletizedExpression, IEnumerable<ParameterExpression> compiledQueryParameters)
	{
		_compiledQueryParameters = compiledQueryParameters ?? Enumerable.Empty<ParameterExpression>();
		_parameterReference = parameterReference;
		_type = funcletizedExpression.Type;
		_funcletizedExpression = funcletizedExpression;
		_cachedDelegate = null;
	}

	internal object EvaluateParameter(object[] arguments)
	{
		if ((object)_cachedDelegate == null)
		{
			if (_funcletizedExpression.NodeType == ExpressionType.Constant)
			{
				return ((ConstantExpression)_funcletizedExpression).Value;
			}
			if (TryEvaluatePath(_funcletizedExpression, out var constantExpression))
			{
				return constantExpression.Value;
			}
		}
		try
		{
			if ((object)_cachedDelegate == null)
			{
				Type delegateType = TypeSystem.GetDelegateType(_compiledQueryParameters.Select((ParameterExpression p) => p.Type), _type);
				_cachedDelegate = Expression.Lambda(delegateType, _funcletizedExpression, _compiledQueryParameters).Compile();
			}
			return _cachedDelegate.DynamicInvoke(arguments);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException;
		}
	}

	internal QueryParameterExpression EscapeParameterForLike(Expression<Func<string, Tuple<string, bool>>> method)
	{
		Expression funcletizedExpression = Expression.Property(Expression.Invoke(method, _funcletizedExpression), "Item1");
		return new QueryParameterExpression(_parameterReference, funcletizedExpression, _compiledQueryParameters);
	}

	private static bool TryEvaluatePath(Expression expression, out ConstantExpression constantExpression)
	{
		MemberExpression memberExpression = expression as MemberExpression;
		constantExpression = null;
		if (memberExpression != null)
		{
			Stack<MemberExpression> stack = new Stack<MemberExpression>();
			stack.Push(memberExpression);
			while ((memberExpression = memberExpression.Expression as MemberExpression) != null)
			{
				stack.Push(memberExpression);
			}
			memberExpression = stack.Pop();
			if (memberExpression.Expression is ConstantExpression)
			{
				if (!TryGetFieldOrPropertyValue(memberExpression, ((ConstantExpression)memberExpression.Expression).Value, out var memberValue))
				{
					return false;
				}
				if (stack.Count > 0)
				{
					foreach (MemberExpression item in stack)
					{
						if (!TryGetFieldOrPropertyValue(item, memberValue, out memberValue))
						{
							return false;
						}
					}
				}
				constantExpression = Expression.Constant(memberValue, expression.Type);
				return true;
			}
		}
		return false;
	}

	private static bool TryGetFieldOrPropertyValue(MemberExpression me, object instance, out object memberValue)
	{
		bool result = false;
		memberValue = null;
		try
		{
			if (me.Member.MemberType == MemberTypes.Field)
			{
				memberValue = ((FieldInfo)me.Member).GetValue(instance);
				result = true;
			}
			else if (me.Member.MemberType == MemberTypes.Property)
			{
				memberValue = ((PropertyInfo)me.Member).GetValue(instance, null);
				result = true;
			}
			return result;
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException;
		}
	}
}
