using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Internal;

internal class DbSetDiscoveryService
{
	private static readonly ConcurrentDictionary<Type, DbContextTypesInitializersPair> _objectSetInitializers = new ConcurrentDictionary<Type, DbContextTypesInitializersPair>();

	public static readonly MethodInfo SetMethod = typeof(DbContext).GetDeclaredMethod("Set");

	private readonly DbContext _context;

	public DbSetDiscoveryService(DbContext context)
	{
		_context = context;
	}

	private Dictionary<Type, List<string>> GetSets()
	{
		if (!_objectSetInitializers.TryGetValue(_context.GetType(), out var value))
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(DbContext), "dbContext");
			List<Action<DbContext>> initDelegates = new List<Action<DbContext>>();
			Dictionary<Type, List<string>> dictionary = new Dictionary<Type, List<string>>();
			foreach (PropertyInfo item in from p in _context.GetType().GetInstanceProperties()
				where p.GetIndexParameters().Length == 0 && p.DeclaringType != typeof(DbContext)
				select p)
			{
				Type setType = GetSetType(item.PropertyType);
				if (!(setType != null))
				{
					continue;
				}
				if (!setType.IsValidStructuralType())
				{
					throw Error.InvalidEntityType(setType);
				}
				if (!dictionary.TryGetValue(setType, out var value2))
				{
					value2 = (dictionary[setType] = new List<string>());
				}
				value2.Add(item.Name);
				if (DbSetPropertyShouldBeInitialized(item))
				{
					MethodInfo methodInfo = item.Setter();
					if (methodInfo != null && methodInfo.IsPublic)
					{
						MethodInfo method = SetMethod.MakeGenericMethod(setType);
						MethodCallExpression methodCallExpression = Expression.Call(parameterExpression, method);
						MethodCallExpression body = Expression.Call(Expression.Convert(parameterExpression, _context.GetType()), methodInfo, methodCallExpression);
						initDelegates.Add(Expression.Lambda<Action<DbContext>>(body, new ParameterExpression[1] { parameterExpression }).Compile());
					}
				}
			}
			Action<DbContext> setsInitializer = delegate(DbContext dbContext)
			{
				foreach (Action<DbContext> item2 in initDelegates)
				{
					item2(dbContext);
				}
			};
			value = new DbContextTypesInitializersPair(dictionary, setsInitializer);
			_objectSetInitializers.TryAdd(_context.GetType(), value);
		}
		return value.EntityTypeToPropertyNameMap;
	}

	public void InitializeSets()
	{
		GetSets();
		_objectSetInitializers[_context.GetType()].SetsInitializer(_context);
	}

	public void RegisterSets(DbModelBuilder modelBuilder)
	{
		IEnumerable<KeyValuePair<Type, List<string>>> enumerable = GetSets();
		if (modelBuilder.Version.IsEF6OrHigher())
		{
			enumerable = enumerable.OrderBy((KeyValuePair<Type, List<string>> s) => s.Value[0]);
		}
		foreach (KeyValuePair<Type, List<string>> item in enumerable)
		{
			if (item.Value.Count > 1)
			{
				throw Error.Mapping_MESTNotSupported(item.Value[0], item.Value[1], item.Key);
			}
			modelBuilder.Entity(item.Key).EntitySetName = item.Value[0];
		}
	}

	private static bool DbSetPropertyShouldBeInitialized(PropertyInfo propertyInfo)
	{
		if (!propertyInfo.GetCustomAttributes<SuppressDbSetInitializationAttribute>(inherit: false).Any())
		{
			return !propertyInfo.DeclaringType.GetCustomAttributes<SuppressDbSetInitializationAttribute>(inherit: false).Any();
		}
		return false;
	}

	private static Type GetSetType(Type declaredType)
	{
		if (!declaredType.IsArray)
		{
			Type setElementType = GetSetElementType(declaredType);
			if (setElementType != null)
			{
				Type c = typeof(DbSet<>).MakeGenericType(setElementType);
				if (declaredType.IsAssignableFrom(c))
				{
					return setElementType;
				}
			}
		}
		return null;
	}

	private static Type GetSetElementType(Type setType)
	{
		try
		{
			Type type = ((setType.IsGenericType() && typeof(IDbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition())) ? setType : setType.GetInterface(typeof(IDbSet<>).FullName));
			if (type != null && !type.ContainsGenericParameters())
			{
				return type.GetGenericArguments()[0];
			}
		}
		catch (AmbiguousMatchException)
		{
		}
		return null;
	}
}
