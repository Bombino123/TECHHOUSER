using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler.V1Interop;

namespace Microsoft.Win32.TaskScheduler;

internal static class TSInteropExt
{
	public static string GetDataItem(this ITask v1Task, string name)
	{
		TaskDefinition.GetV1TaskDataDictionary(v1Task).TryGetValue(name, out var value);
		return value;
	}

	public static bool HasFlags(this ITask v1Task, TaskFlags flags)
	{
		return v1Task.GetFlags().IsFlagSet(flags);
	}

	public static void SetDataItem(this ITask v1Task, string name, string value)
	{
		Dictionary<string, string> v1TaskDataDictionary = TaskDefinition.GetV1TaskDataDictionary(v1Task);
		v1TaskDataDictionary[name] = value;
		TaskDefinition.SetV1TaskData(v1Task, v1TaskDataDictionary);
	}

	public static void SetFlags(this ITask v1Task, TaskFlags flags, bool value = true)
	{
		v1Task.SetFlags(v1Task.GetFlags().SetFlags(flags, value));
	}
}
