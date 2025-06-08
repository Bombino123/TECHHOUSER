using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Plugin;

internal static class TaskManager
{
	private const int PROCESS_TERMINATE = 1;

	private const int PROCESS_SUSPEND_RESUME = 2048;

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll")]
	private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

	[DllImport("kernel32.dll")]
	private static extern int SuspendThread(IntPtr hThread);

	[DllImport("kernel32.dll")]
	private static extern int ResumeThread(IntPtr hThread);

	public static bool KillProcess(int processId)
	{
		IntPtr intPtr = OpenProcess(1, bInheritHandle: false, processId);
		if (intPtr == IntPtr.Zero)
		{
			return false;
		}
		bool result = TerminateProcess(intPtr, 0u);
		CloseHandle(intPtr);
		return result;
	}

	public static bool SuspendProcess(int processId)
	{
		IntPtr intPtr = OpenProcess(2048, bInheritHandle: false, processId);
		if (intPtr == IntPtr.Zero)
		{
			return false;
		}
		foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
		{
			IntPtr intPtr2 = OpenThread(2048, bInheritHandle: false, (uint)thread.Id);
			SuspendThread(intPtr2);
			CloseHandle(intPtr2);
		}
		CloseHandle(intPtr);
		return true;
	}

	public static bool ResumeProcess(int processId)
	{
		IntPtr intPtr = OpenProcess(2048, bInheritHandle: false, processId);
		if (intPtr == IntPtr.Zero)
		{
			return false;
		}
		foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
		{
			IntPtr intPtr2 = OpenThread(2048, bInheritHandle: false, (uint)thread.Id);
			ResumeThread(intPtr2);
			CloseHandle(intPtr2);
		}
		CloseHandle(intPtr);
		return true;
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
}
