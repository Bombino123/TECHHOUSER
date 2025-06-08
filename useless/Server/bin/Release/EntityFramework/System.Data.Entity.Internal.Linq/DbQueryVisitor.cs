using System.Collections.Concurrent;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Internal.Linq;

internal class DbQueryVisitor : ExpressionVisitor
{
	private const BindingFlags SetAccessBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly ConcurrentDictionary<Type, Func<ObjectQuery, object>> _wrapperFactories = new ConcurrentDictionary<Type, Func<ObjectQuery, object>>();

	protected override Expression VisitMethodCall(MethodCallExpression node)
	{
		Check.NotNull(node, "node");
		if (typeof(DbContext).IsAssignableFrom(node.Method.DeclaringType))
		{
			DbContext dbContext = null;
			if (node.Object is MemberExpression memberExpression)
			{
				dbContext = GetContextFromConstantExpression(memberExpression.Expression, memberExpression.Member);
			}
			else if (node.Object is ConstantExpression constantExpression)
			{
				dbContext = constantExpression.Value as DbContext;
			}
			if (dbContext != null && !node.Method.GetCustomAttributes<DbFunctionAttribute>(inherit: false).Any() && node.Method.GetParameters().Length == 0)
			{
				Expression expression = CreateObjectQueryConstant(node.Method.Invoke(dbContext, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null));
				if (expression != null)
				{
					return expression;
				}
			}
		}
		return base.VisitMethodCall(node);
	}

	protected override Expression VisitMember(MemberExpression node)
	{
		Check.NotNull(node, "node");
		PropertyInfo propertyInfo = node.Member as PropertyInfo;
		MemberExpression memberExpression = node.Expression as MemberExpression;
		if (propertyInfo != null && memberExpression != null && typeof(IQueryable).IsAssignableFrom(propertyInfo.PropertyType) && typeof(DbContext).IsAssignableFrom(node.Member.DeclaringType))
		{
			DbContext contextFromConstantExpression = GetContextFromConstantExpression(memberExpression.Expression, memberExpression.Member);
			if (contextFromConstantExpression != null)
			{
				Expression expression = CreateObjectQueryConstant(propertyInfo.GetValue(contextFromConstantExpression, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null, null));
				if (expression != null)
				{
					return expression;
				}
			}
		}
		return base.VisitMember(node);
	}

	private static DbContext GetContextFromConstantExpression(Expression expression, MemberInfo member)
	{
		if (expression == null)
		{
			return GetContextFromMember(member, null);
		}
		object expressionValue = GetExpressionValue(expression);
		if (expressionValue != null)
		{
			return GetContextFromMember(member, expressionValue);
		}
		return null;
	}

	private static object GetExpressionValue(Expression expression)
	{
		if (expression is ConstantExpression constantExpression)
		{
			return constantExpression.Value;
		}
		if (expression is MemberExpression memberExpression)
		{
			FieldInfo fieldInfo = memberExpression.Member as FieldInfo;
			if (fieldInfo != null)
			{
				object expressionValue = GetExpressionValue(memberExpression.Expression);
				if (expressionValue != null)
				{
					return fieldInfo.GetValue(expressionValue);
				}
			}
			PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
			if (propertyInfo != null)
			{
				object expressionValue2 = GetExpressionValue(memberExpression.Expression);
				if (expressionValue2 != null)
				{
					return propertyInfo.GetValue(expressionValue2, null);
				}
			}
		}
		return null;
	}

	private static DbContext GetContextFromMember(MemberInfo member, object value)
	{
		FieldInfo fieldInfo = member as FieldInfo;
		if (fieldInfo != null)
		{
			return fieldInfo.GetValue(value) as DbContext;
		}
		PropertyInfo propertyInfo = member as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.GetValue(value, null) as DbContext;
		}
		return null;
	}

	private static Expression CreateObjectQueryConstant(object dbQuery)
	{
		ObjectQuery objectQuery = ExtractObjectQuery(dbQuery);
		if (objectQuery != null)
		{
			Type type = objectQuery.GetType().GetGenericArguments().Single();
			if (!_wrapperFactories.TryGetValue(type, out var value))
			{
				MethodInfo declaredMethod = typeof(ReplacementDbQueryWrapper<>).MakeGenericType(type).GetDeclaredMethod("Create", typeof(ObjectQuery));
				value = (Func<ObjectQuery, object>)Delegate.CreateDelegate(typeof(Func<ObjectQuery, object>), declaredMethod);
				_wrapperFactories.TryAdd(type, value);
			}
			object obj = value(objectQuery);
			return Expression.Property(Expression.Constant(obj, obj.GetType()), "Query");
		}
		return null;
	}

	private static ObjectQuery ExtractObjectQuery(object dbQuery)
	{
		if (dbQuery is IInternalQueryAdapter internalQueryAdapter)
		{
			return internalQueryAdapter.InternalQuery.ObjectQuery;
		}
		return null;
	}
}
