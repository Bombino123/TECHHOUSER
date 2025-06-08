using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

internal class Methods
{
	public enum EXECUTION_STATE : uint
	{
		ES_CONTINUOUS = 2147483648u,
		ES_DISPLAY_REQUIRED = 2u,
		ES_SYSTEM_REQUIRED = 1u
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

	public static void PreventSleep()
	{
		try
		{
			SetThreadExecutionState((EXECUTION_STATE)2147483651u);
		}
		catch
		{
		}
	}

	public static List<string> BytesToString(List<string> byteCount)
	{
		List<string> list = new List<string>();
		foreach (string item in byteCount)
		{
			list.Add(BytesToString(Convert.ToInt64(item)));
		}
		return list;
	}

	public static string BytesToString(long byteCount)
	{
		return Math.Round((double)byteCount / Math.Pow(1024.0, 3.0)) + "GB";
	}

	public static List<string> GetHardwareInfo(string WIN32_Class, string ClassItemField)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		List<string> list = new List<string>();
		ManagementObjectSearcher val = new ManagementObjectSearcher("SELECT * FROM " + WIN32_Class);
		try
		{
			ManagementObjectEnumerator enumerator = val.Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ManagementObject val2 = (ManagementObject)enumerator.Current;
					list.Add(((ManagementBaseObject)val2)[ClassItemField].ToString().Trim());
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}
}
