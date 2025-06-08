using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal static class DbHelpers
{
	public static readonly MethodInfo ConvertAndSetMethod = typeof(DbHelpers).GetOnlyDeclaredMethod("ConvertAndSet");

	private static readonly ConcurrentDictionary<Type, IDictionary<string, Type>> _propertyTypes = new ConcurrentDictionary<Type, IDictionary<string, Type>>();

	private static readonly ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>> _propertySetters = new ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>>();

	private static readonly ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>> _propertyGetters = new ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>>();

	private static readonly ConcurrentDictionary<Type, Type> _collectionTypes = new ConcurrentDictionary<Type, Type>();

	public static bool KeyValuesEqual(object x, object y)
	{
		if (x is DBNull)
		{
			x = null;
		}
		if (y is DBNull)
		{
			y = null;
		}
		if (object.Equals(x, y))
		{
			return true;
		}
		byte[] array = x as byte[];
		byte[] array2 = y as byte[];
		if (array == null || array2 == null || array.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool PropertyValuesEqual(object x, object y)
	{
		if (x is DBNull)
		{
			x = null;
		}
		if (y is DBNull)
		{
			y = null;
		}
		if (x == null)
		{
			return y == null;
		}
		if (x.GetType().IsValueType() && object.Equals(x, y))
		{
			return true;
		}
		if (x is string text)
		{
			return text.Equals(y as string, StringComparison.Ordinal);
		}
		if (!(x is byte[] array))
		{
			return x == y;
		}
		if (!(y is byte[] array2) || array.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string QuoteIdentifier(string identifier)
	{
		return "[" + identifier.Replace("]", "]]") + "]";
	}

	public static bool TreatAsConnectionString(string nameOrConnectionString)
	{
		return nameOrConnectionString.IndexOf('=') >= 0;
	}

	public static bool TryGetConnectionName(string nameOrConnectionString, out string name)
	{
		int num = nameOrConnectionString.IndexOf('=');
		if (num < 0)
		{
			name = nameOrConnectionString;
			return true;
		}
		if (nameOrConnectionString.IndexOf('=', num + 1) >= 0)
		{
			name = null;
			return false;
		}
		if (nameOrConnectionString.Substring(0, num).Trim().Equals("name", StringComparison.OrdinalIgnoreCase))
		{
			name = nameOrConnectionString.Substring(num + 1).Trim();
			return true;
		}
		name = null;
		return false;
	}

	public static bool IsFullEFConnectionString(string nameOrConnectionString)
	{
		IEnumerable<string> source = from t in nameOrConnectionString.ToUpperInvariant().Split('=', ';')
			select t.Trim();
		if (source.Contains("PROVIDER") && source.Contains("PROVIDER CONNECTION STRING"))
		{
			return source.Contains("METADATA");
		}
		return false;
	}

	public static string ParsePropertySelector<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> property, string methodName, string paramName)
	{
		if (!TryParsePath(property.Body, out var path) || path == null)
		{
			throw new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression(methodName, typeof(TEntity).Name), paramName);
		}
		return path;
	}

	public static bool TryParsePath(Expression expression, out string path)
	{
		path = null;
		Expression expression2 = expression.RemoveConvert();
		MemberExpression memberExpression = expression2 as MemberExpression;
		MethodCallExpression methodCallExpression = expression2 as MethodCallExpression;
		if (memberExpression != null)
		{
			string name = memberExpression.Member.Name;
			if (!TryParsePath(memberExpression.Expression, out var path2))
			{
				return false;
			}
			path = ((path2 == null) ? name : (path2 + "." + name));
		}
		else if (methodCallExpression != null)
		{
			if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2)
			{
				if (!TryParsePath(methodCallExpression.Arguments[0], out var path3))
				{
					return false;
				}
				if (path3 != null && methodCallExpression.Arguments[1] is LambdaExpression lambdaExpression)
				{
					if (!TryParsePath(lambdaExpression.Body, out var path4))
					{
						return false;
					}
					if (path4 != null)
					{
						path = path3 + "." + path4;
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	public static IDictionary<string, Type> GetPropertyTypes(Type type)
	{
		if (!_propertyTypes.TryGetValue(type, out var value))
		{
			IEnumerable<PropertyInfo> enumerable = from p in type.GetInstanceProperties()
				where p.GetIndexParameters().Length == 0
				select p;
			value = new Dictionary<string, Type>(enumerable.Count());
			foreach (PropertyInfo item in enumerable)
			{
				value[item.Name] = item.PropertyType;
			}
			_propertyTypes.TryAdd(type, value);
		}
		return value;
	}

	public static IDictionary<string, Action<object, object>> GetPropertySetters(Type type)
	{
		if (!_propertySetters.TryGetValue(type, out var value))
		{
			IEnumerable<PropertyInfo> source = from p in type.GetInstanceProperties()
				where p.GetIndexParameters().Length == 0
				select p;
			value = new Dictionary<string, Action<object, object>>(source.Count());
			foreach (PropertyInfo item in source.Select((PropertyInfo p) => p.GetPropertyInfoForSet()))
			{
				MethodInfo methodInfo = item.Setter();
				if (methodInfo != null)
				{
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "value");
					ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "instance");
					MethodCallExpression body = Expression.Call(Expression.Convert(parameterExpression2, type), methodInfo, Expression.Convert(parameterExpression, item.PropertyType));
					Action<object, object> setter = Expression.Lambda<Action<object, object>>(body, new ParameterExpression[2] { parameterExpression2, parameterExpression }).Compile();
					MethodInfo method = ConvertAndSetMethod.MakeGenericMethod(item.PropertyType);
					Action<object, object, Action<object, object>, string, string> convertAndSet = (Action<object, object, Action<object, object>, string, string>)Delegate.CreateDelegate(typeof(Action<object, object, Action<object, object>, string, string>), method);
					string propertyName = item.Name;
					value[item.Name] = delegate(object i, object v)
					{
						convertAndSet(i, v, setter, propertyName, type.Name);
					};
				}
			}
			_propertySetters.TryAdd(type, value);
		}
		return value;
	}

	private static void ConvertAndSet<T>(object instance, object value, Action<object, object> setter, string propertyName, string typeName)
	{
		if (value == null && typeof(T).IsValueType() && Nullable.GetUnderlyingType(typeof(T)) == null)
		{
			throw Error.DbPropertyValues_CannotSetNullValue(propertyName, typeof(T).Name, typeName);
		}
		setter(instance, (T)value);
	}

	public static IDictionary<string, Func<object, object>> GetPropertyGetters(Type type)
	{
		if (!_propertyGetters.TryGetValue(type, out var value))
		{
			IEnumerable<PropertyInfo> enumerable = from p in type.GetInstanceProperties()
				where p.GetIndexParameters().Length == 0
				select p;
			value = new Dictionary<string, Func<object, object>>(enumerable.Count());
			foreach (PropertyInfo item in enumerable)
			{
				MethodInfo methodInfo = item.Getter();
				if (methodInfo != null)
				{
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
					UnaryExpression body = Expression.Convert(Expression.Call(Expression.Convert(parameterExpression, type), methodInfo), typeof(object));
					value[item.Name] = Expression.Lambda<Func<object, object>>(body, new ParameterExpression[1] { parameterExpression }).Compile();
				}
			}
			_propertyGetters.TryAdd(type, value);
		}
		return value;
	}

	public static IQueryable CreateNoTrackingQuery(ObjectQuery query)
	{
		ObjectQuery obj = (ObjectQuery)((IQueryable)query).Provider.CreateQuery(((IQueryable)query).Expression);
		obj.ExecutionStrategy = query.ExecutionStrategy;
		obj.MergeOption = MergeOption.NoTracking;
		obj.Streaming = query.Streaming;
		return obj;
	}

	public static IQueryable CreateStreamingQuery(ObjectQuery query)
	{
		ObjectQuery obj = (ObjectQuery)((IQueryable)query).Provider.CreateQuery(((IQueryable)query).Expression);
		obj.ExecutionStrategy = query.ExecutionStrategy;
		obj.Streaming = true;
		obj.MergeOption = query.MergeOption;
		return obj;
	}

	public static IQueryable CreateQueryWithExecutionStrategy(ObjectQuery query, IDbExecutionStrategy executionStrategy)
	{
		ObjectQuery obj = (ObjectQuery)((IQueryable)query).Provider.CreateQuery(((IQueryable)query).Expression);
		obj.ExecutionStrategy = executionStrategy;
		obj.MergeOption = query.MergeOption;
		obj.Streaming = query.Streaming;
		return obj;
	}

	public static IEnumerable<DbValidationError> SplitValidationResults(string propertyName, IEnumerable<ValidationResult> validationResults)
	{
		foreach (ValidationResult validationResult in validationResults)
		{
			if (validationResult == null)
			{
				continue;
			}
			IEnumerable<string> enumerable;
			if (validationResult.MemberNames != null && validationResult.MemberNames.Any())
			{
				enumerable = validationResult.MemberNames;
			}
			else
			{
				IEnumerable<string> enumerable2 = new string[1];
				enumerable = enumerable2;
			}
			IEnumerable<string> enumerable3 = enumerable;
			foreach (string item in enumerable3)
			{
				yield return new DbValidationError(item ?? propertyName, validationResult.ErrorMessage);
			}
		}
	}

	public static string GetPropertyPath(InternalMemberEntry property)
	{
		return string.Join(".", GetPropertyPathSegments(property).Reverse());
	}

	private static IEnumerable<string> GetPropertyPathSegments(InternalMemberEntry property)
	{
		do
		{
			yield return property.Name;
			property = ((property is InternalNestedPropertyEntry) ? ((InternalNestedPropertyEntry)property).ParentPropertyEntry : null);
		}
		while (property != null);
	}

	public static Type CollectionType(Type elementType)
	{
		return _collectionTypes.GetOrAdd(elementType, (Type t) => typeof(ICollection<>).MakeGenericType(t));
	}

	public static string DatabaseName(this Type contextType)
	{
		return contextType.ToString();
	}
}
