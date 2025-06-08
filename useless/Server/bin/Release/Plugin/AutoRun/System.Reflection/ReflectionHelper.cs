using System.IO;

namespace System.Reflection;

internal static class ReflectionHelper
{
	public static Type LoadType(string typeName, string asmRef)
	{
		Type type = null;
		if (!TryGetType(Assembly.GetCallingAssembly(), typeName, ref type) && !TryGetType(asmRef, typeName, ref type) && !TryGetType(Assembly.GetExecutingAssembly(), typeName, ref type))
		{
			TryGetType(Assembly.GetEntryAssembly(), typeName, ref type);
		}
		return type;
	}

	private static bool TryGetType(string asmRef, string typeName, ref Type type)
	{
		try
		{
			if (File.Exists(asmRef))
			{
				return TryGetType(Assembly.LoadFrom(asmRef), typeName, ref type);
			}
		}
		catch
		{
		}
		return false;
	}

	private static bool TryGetType(Assembly asm, string typeName, ref Type type)
	{
		if (asm != null)
		{
			try
			{
				type = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
				return type != null;
			}
			catch
			{
			}
		}
		return false;
	}

	public static T InvokeMethod<T>(Type type, string methodName, params object[] args)
	{
		return InvokeMethod<T>(Activator.CreateInstance(type), methodName, args);
	}

	public static T InvokeMethod<T>(Type type, object[] instArgs, string methodName, params object[] args)
	{
		return InvokeMethod<T>(Activator.CreateInstance(type, instArgs), methodName, args);
	}

	public static T InvokeMethod<T>(object obj, string methodName, params object[] args)
	{
		Type[] argTypes = ((args == null || args.Length == 0) ? Type.EmptyTypes : Array.ConvertAll(args, (object o) => (o != null) ? o.GetType() : typeof(object)));
		return InvokeMethod<T>(obj, methodName, argTypes, args);
	}

	public static T InvokeMethod<T>(object obj, string methodName, Type[] argTypes, object[] args)
	{
		MethodInfo methodInfo = obj?.GetType().GetMethod(methodName, argTypes);
		if (methodInfo != null)
		{
			return (T)Convert.ChangeType(methodInfo.Invoke(obj, args), typeof(T));
		}
		return default(T);
	}

	public static T GetProperty<T>(object obj, string propName, T defaultValue = default(T))
	{
		if (obj != null)
		{
			try
			{
				return (T)Convert.ChangeType(obj.GetType().InvokeMember(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty, null, obj, null, null), typeof(T));
			}
			catch
			{
			}
		}
		return defaultValue;
	}

	public static void SetProperty<T>(object obj, string propName, T value)
	{
		try
		{
			obj?.GetType().InvokeMember(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty, null, obj, new object[1] { value }, null);
		}
		catch
		{
		}
	}
}
