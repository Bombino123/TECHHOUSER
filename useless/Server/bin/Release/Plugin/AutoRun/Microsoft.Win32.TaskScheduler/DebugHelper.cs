namespace Microsoft.Win32.TaskScheduler;

internal static class DebugHelper
{
	public static string GetDebugString(object inst)
	{
		return inst.GetType().ToString();
	}
}
