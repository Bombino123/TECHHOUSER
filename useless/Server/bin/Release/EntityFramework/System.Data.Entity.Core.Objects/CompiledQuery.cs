using System.Collections;
using System.Data.Entity.Core.Objects.ELinq;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects;

public sealed class CompiledQuery
{
	private readonly LambdaExpression _query;

	private readonly Guid _cacheToken = Guid.NewGuid();

	private CompiledQuery(LambdaExpression query)
	{
		Funcletizer funcletizer = Funcletizer.CreateCompiledQueryLockdownFuncletizer();
		_query = (LambdaExpression)funcletizer.Funcletize(query, out var _);
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TArg3, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TResult>;
	}

	public static Func<TArg0, TArg1, TArg2, TResult> Compile<TArg0, TArg1, TArg2, TResult>(Expression<Func<TArg0, TArg1, TArg2, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TResult>;
	}

	public static Func<TArg0, TArg1, TResult> Compile<TArg0, TArg1, TResult>(Expression<Func<TArg0, TArg1, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TArg1, TResult>;
	}

	public static Func<TArg0, TResult> Compile<TArg0, TResult>(Expression<Func<TArg0, TResult>> query) where TArg0 : ObjectContext
	{
		return new CompiledQuery(query).Invoke<TArg0, TResult>;
	}

	private TResult Invoke<TArg0, TResult>(TArg0 arg0) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[0]);
	}

	private TResult Invoke<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[1] { arg1 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[2] { arg1, arg2 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[3] { arg1, arg2, arg3 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[4] { arg1, arg2, arg3, arg4 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[5] { arg1, arg2, arg3, arg4, arg5 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[6] { arg1, arg2, arg3, arg4, arg5, arg6 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[7] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[8] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[9] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[10] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 });
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[11]
		{
			arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
			arg11
		});
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[12]
		{
			arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
			arg11, arg12
		});
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[13]
		{
			arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
			arg11, arg12, arg13
		});
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[14]
		{
			arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
			arg11, arg12, arg13, arg14
		});
	}

	private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15) where TArg0 : ObjectContext
	{
		arg0.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TResult), Assembly.GetCallingAssembly());
		return ExecuteQuery<TResult>(arg0, new object[15]
		{
			arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
			arg11, arg12, arg13, arg14, arg15
		});
	}

	private TResult ExecuteQuery<TResult>(ObjectContext context, params object[] parameterValues)
	{
		bool isSingleton;
		IEnumerable enumerable = new CompiledELinqQueryState(GetElementType(typeof(TResult), out isSingleton), context, _query, _cacheToken, parameterValues).CreateQuery();
		if (isSingleton)
		{
			return ObjectQueryProvider.ExecuteSingle(enumerable.Cast<TResult>(), _query);
		}
		return (TResult)enumerable;
	}

	private static Type GetElementType(Type resultType, out bool isSingleton)
	{
		Type elementType = TypeSystem.GetElementType(resultType);
		isSingleton = elementType == resultType || !resultType.IsAssignableFrom(typeof(ObjectQuery<>).MakeGenericType(elementType));
		if (isSingleton)
		{
			return resultType;
		}
		return elementType;
	}
}
