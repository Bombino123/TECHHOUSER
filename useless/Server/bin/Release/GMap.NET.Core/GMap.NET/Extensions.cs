using System;
using System.Runtime.Serialization;

namespace GMap.NET;

public static class Extensions
{
	public static T GetValue<T>(SerializationInfo info, string key) where T : class
	{
		try
		{
			return info.GetValue(key, typeof(T)) as T;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static T GetValue<T>(SerializationInfo info, string key, T defaultValue) where T : class
	{
		T value = GetValue<T>(info, key);
		if (value != null)
		{
			return value;
		}
		return defaultValue;
	}

	public static T GetStruct<T>(SerializationInfo info, string key, T defaultValue) where T : struct
	{
		try
		{
			return (T)info.GetValue(key, typeof(T));
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	public static T? GetStruct<T>(SerializationInfo info, string key, T? defaultValue) where T : struct
	{
		try
		{
			return (T?)info.GetValue(key, typeof(T?));
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}
}
