using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Stub.Helper;

internal class Win32
{
	private enum SvcStartupType : uint
	{
		BootStart,
		SystemStart,
		Automatic,
		Manual,
		Disabled
	}

	[StructLayout(LayoutKind.Sequential)]
	private class QUERY_SERVICE_CONFIG
	{
		[MarshalAs(UnmanagedType.U4)]
		public uint dwServiceType;

		[MarshalAs(UnmanagedType.U4)]
		public SvcStartupType dwStartType;

		[MarshalAs(UnmanagedType.U4)]
		public uint dwErrorControl;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpBinaryPathName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpLoadOrderGroup;

		[MarshalAs(UnmanagedType.U4)]
		public uint dwTagID;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpDependencies;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpServiceStartName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpDisplayName;
	}

	private const uint SC_MANAGER_CONNECT = 1u;

	private const uint SC_MANAGER_ALL_ACCESS = 983103u;

	private const uint SERVICE_QUERY_CONFIG = 1u;

	private const uint SERVICE_CHANGE_CONFIG = 2u;

	private const uint SERVICE_START = 22u;

	private const uint SERVICE_NO_CHANGE = uint.MaxValue;

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

	[DllImport("advapi32.dll")]
	private static extern int CloseServiceHandle(IntPtr hSCObject);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool ChangeServiceConfig(IntPtr hService, uint nServiceType, SvcStartupType nStartType, uint nErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, [In] char[] lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, uint cbBufSize, out uint pcbBytesNeeded);

	[DllImport("advapi32", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);

	public static bool TryStartService(string svcName)
	{
		bool flag = false;
		QUERY_SERVICE_CONFIG qUERY_SERVICE_CONFIG = new QUERY_SERVICE_CONFIG();
		IntPtr intPtr = OpenSCManager(null, null, 1u);
		IntPtr intPtr2 = OpenService(intPtr, "TrustedInstaller", 23u);
		uint pcbBytesNeeded = 0u;
		IntPtr intPtr3 = Marshal.AllocHGlobal(4096);
		if (!QueryServiceConfig(intPtr2, intPtr3, 4096u, out pcbBytesNeeded))
		{
			return false;
		}
		Marshal.PtrToStructure(intPtr3, (object)qUERY_SERVICE_CONFIG);
		Marshal.FreeHGlobal(intPtr3);
		flag = qUERY_SERVICE_CONFIG.dwStartType == SvcStartupType.Disabled;
		if (flag && !ChangeServiceConfig(intPtr2, uint.MaxValue, SvcStartupType.Manual, uint.MaxValue, null, null, IntPtr.Zero, null, null, null, null))
		{
			return false;
		}
		StartService(intPtr2, 0, null);
		if (flag && !ChangeServiceConfig(intPtr2, uint.MaxValue, SvcStartupType.Disabled, uint.MaxValue, null, null, IntPtr.Zero, null, null, null, null))
		{
			return false;
		}
		CloseServiceHandle(intPtr2);
		CloseServiceHandle(intPtr);
		return true;
	}

	public static string WinAPILastErrMsg(int err)
	{
		return new Win32Exception(err).Message + " (Error code: " + err + ")";
	}
}
