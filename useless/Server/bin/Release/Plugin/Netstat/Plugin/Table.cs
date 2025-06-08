using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Plugin;

public class Table
{
	public struct MIB_UDPROW_OWNER_PID
	{
		public uint dwLocalAddr;

		public uint dwLocalPort;

		public int dwOwningPid;
	}

	public struct MIB_TCPROW_OWNER_PID
	{
		public uint state;

		public uint localAddr;

		public byte localPort1;

		public byte localPort2;

		public byte localPort3;

		public byte localPort4;

		public uint remoteAddr;

		public byte remotePort1;

		public byte remotePort2;

		public byte remotePort3;

		public byte remotePort4;

		public int owningPid;

		public ushort LocalPort => BitConverter.ToUInt16(new byte[2] { localPort2, localPort1 }, 0);

		public ushort RemotePort => BitConverter.ToUInt16(new byte[2] { remotePort2, remotePort1 }, 0);
	}

	public struct MIB_TCPTABLE_OWNER_PID
	{
		public uint dwNumEntries;

		private MIB_TCPROW_OWNER_PID table;
	}

	public enum UDP_TABLE_TYPE
	{
		UDP_TABLE_BASIC,
		UDP_TABLE_OWNER_PID,
		UDP_TABLE_OWNER_MODULE,
		UDP_TABLE_OWNER_PID_AND_MODULE
	}

	public enum TCP_TABLE_TYPE
	{
		TCP_TABLE_BASIC_LISTENER,
		TCP_TABLE_BASIC_CONNECTIONS,
		TCP_TABLE_BASIC_ALL,
		TCP_TABLE_OWNER_PID_LISTENER,
		TCP_TABLE_OWNER_PID_CONNECTIONS,
		TCP_TABLE_OWNER_PID_ALL,
		TCP_TABLE_OWNER_MODULE_LISTENER,
		TCP_TABLE_OWNER_MODULE_CONNECTIONS,
		TCP_TABLE_OWNER_MODULE_ALL
	}

	public enum TCP_CONNECTION_STATE
	{
		CLOSED = 1,
		LISTENING,
		SYN_SENT,
		SYN_RCVD,
		ESTABLISHED,
		FIN_WAIT_1,
		FIN_WAIT_2,
		CLOSE_WAIT,
		CLOSING,
		LAST_ACK,
		TIME_WAIT,
		DELETE_TCP
	}

	[DllImport("Ws2_32.dll")]
	private static extern ushort ntohs(ushort netshort);

	[DllImport("iphlpapi.dll", SetLastError = true)]
	private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_TYPE tblClass, int reserved);

	[DllImport("iphlpapi.dll", SetLastError = true)]
	private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool sort, int ipVersion, UDP_TABLE_TYPE tblClass, int reserved);

	[DllImport("kernel32.dll")]
	public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("psapi.dll")]
	public static extern bool GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, out string lpFilename, uint nSize);

	public static string GetProcessPath(int processId)
	{
		try
		{
			return Process.GetProcessById(processId).MainModule.FileName;
		}
		catch
		{
		}
		return "";
	}

	public static string GetIpAddress(long ipAddrs)
	{
		try
		{
			return new IPAddress(ipAddrs).ToString();
		}
		catch
		{
			return ipAddrs.ToString();
		}
	}

	public static ushort GetTcpPort(int tcpPort)
	{
		return ntohs((ushort)tcpPort);
	}

	public static MIB_UDPROW_OWNER_PID[] GetAllUdpConnections()
	{
		int ipVersion = 2;
		int dwOutBufLen = 0;
		uint extendedUdpTable = GetExtendedUdpTable(IntPtr.Zero, ref dwOutBufLen, sort: true, ipVersion, UDP_TABLE_TYPE.UDP_TABLE_OWNER_PID, 0);
		if (extendedUdpTable != 0 && extendedUdpTable != 122)
		{
			throw new Exception("Error occurred when trying to query tcp table, return code: " + extendedUdpTable);
		}
		IntPtr intPtr = Marshal.AllocHGlobal(dwOutBufLen);
		try
		{
			extendedUdpTable = GetExtendedUdpTable(intPtr, ref dwOutBufLen, sort: true, ipVersion, UDP_TABLE_TYPE.UDP_TABLE_OWNER_PID, 0);
			if (extendedUdpTable != 0)
			{
				throw new Exception("Error occurred when trying to query tcp table, return code: " + extendedUdpTable);
			}
			MIB_TCPTABLE_OWNER_PID mIB_TCPTABLE_OWNER_PID = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(intPtr, typeof(MIB_TCPTABLE_OWNER_PID));
			IntPtr intPtr2 = (IntPtr)((long)intPtr + Marshal.SizeOf((object)mIB_TCPTABLE_OWNER_PID.dwNumEntries));
			MIB_UDPROW_OWNER_PID[] array = new MIB_UDPROW_OWNER_PID[mIB_TCPTABLE_OWNER_PID.dwNumEntries];
			for (int i = 0; i < mIB_TCPTABLE_OWNER_PID.dwNumEntries; i++)
			{
				MIB_UDPROW_OWNER_PID mIB_UDPROW_OWNER_PID = (array[i] = (MIB_UDPROW_OWNER_PID)Marshal.PtrToStructure(intPtr2, typeof(MIB_UDPROW_OWNER_PID)));
				intPtr2 = (IntPtr)((long)intPtr2 + Marshal.SizeOf((object)mIB_UDPROW_OWNER_PID));
			}
			return array;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	public static MIB_TCPROW_OWNER_PID[] GetAllTcpConnections()
	{
		int ipVersion = 2;
		int dwOutBufLen = 0;
		uint extendedTcpTable = GetExtendedTcpTable(IntPtr.Zero, ref dwOutBufLen, sort: true, ipVersion, TCP_TABLE_TYPE.TCP_TABLE_OWNER_PID_ALL, 0);
		if (extendedTcpTable != 0 && extendedTcpTable != 122)
		{
			throw new Exception("Error occurred when trying to query tcp table, return code: " + extendedTcpTable);
		}
		IntPtr intPtr = Marshal.AllocHGlobal(dwOutBufLen);
		try
		{
			extendedTcpTable = GetExtendedTcpTable(intPtr, ref dwOutBufLen, sort: true, ipVersion, TCP_TABLE_TYPE.TCP_TABLE_OWNER_PID_ALL, 0);
			if (extendedTcpTable != 0)
			{
				throw new Exception("Error occurred when trying to query tcp table, return code: " + extendedTcpTable);
			}
			MIB_TCPTABLE_OWNER_PID mIB_TCPTABLE_OWNER_PID = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(intPtr, typeof(MIB_TCPTABLE_OWNER_PID));
			IntPtr intPtr2 = (IntPtr)((long)intPtr + Marshal.SizeOf((object)mIB_TCPTABLE_OWNER_PID.dwNumEntries));
			MIB_TCPROW_OWNER_PID[] array = new MIB_TCPROW_OWNER_PID[mIB_TCPTABLE_OWNER_PID.dwNumEntries];
			for (int i = 0; i < mIB_TCPTABLE_OWNER_PID.dwNumEntries; i++)
			{
				MIB_TCPROW_OWNER_PID mIB_TCPROW_OWNER_PID = (array[i] = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(intPtr2, typeof(MIB_TCPROW_OWNER_PID)));
				intPtr2 = (IntPtr)((long)intPtr2 + Marshal.SizeOf((object)mIB_TCPROW_OWNER_PID));
			}
			return array;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}
}
