using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace BotKiller.Helper;

internal class GetMutex
{
	public class Win32API
	{
		public enum ObjectInformationClass
		{
			ObjectBasicInformation,
			ObjectNameInformation,
			ObjectTypeInformation,
			ObjectAllTypesInformation,
			ObjectHandleInformation
		}

		[Flags]
		public enum ProcessAccessFlags : uint
		{
			All = 0x1F0FFFu,
			Terminate = 1u,
			CreateThread = 2u,
			VMOperation = 8u,
			VMRead = 0x10u,
			VMWrite = 0x20u,
			DupHandle = 0x40u,
			SetInformation = 0x200u,
			QueryInformation = 0x400u,
			Synchronize = 0x100000u
		}

		public struct OBJECT_BASIC_INFORMATION
		{
			public int Attributes;

			public int GrantedAccess;

			public int HandleCount;

			public int PointerCount;

			public int PagedPoolUsage;

			public int NonPagedPoolUsage;

			public int Reserved1;

			public int Reserved2;

			public int Reserved3;

			public int NameInformationLength;

			public int TypeInformationLength;

			public int SecurityDescriptorLength;

			public FILETIME CreateTime;
		}

		public struct OBJECT_TYPE_INFORMATION
		{
			public UNICODE_STRING Name;

			public int ObjectCount;

			public int HandleCount;

			public int Reserved1;

			public int Reserved2;

			public int Reserved3;

			public int Reserved4;

			public int PeakObjectCount;

			public int PeakHandleCount;

			public int Reserved5;

			public int Reserved6;

			public int Reserved7;

			public int Reserved8;

			public int InvalidAttributes;

			public GENERIC_MAPPING GenericMapping;

			public int ValidAccess;

			public byte Unknown;

			public byte MaintainHandleDatabase;

			public int PoolType;

			public int PagedPoolUsage;

			public int NonPagedPoolUsage;
		}

		public struct OBJECT_NAME_INFORMATION
		{
			public UNICODE_STRING Name;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct UNICODE_STRING
		{
			public ushort Length;

			public ushort MaximumLength;

			public IntPtr Buffer;
		}

		public struct GENERIC_MAPPING
		{
			public int GenericRead;

			public int GenericWrite;

			public int GenericExecute;

			public int GenericAll;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SYSTEM_HANDLE_INFORMATION
		{
			public int ProcessID;

			public byte ObjectTypeNumber;

			public byte Flags;

			public ushort Handle;

			public int Object_Pointer;

			public uint GrantedAccess;
		}

		public const int MAX_PATH = 260;

		public const uint STATUS_INFO_LENGTH_MISMATCH = 3221225476u;

		public const int DUPLICATE_SAME_ACCESS = 2;

		public const int DUPLICATE_CLOSE_SOURCE = 1;

		[DllImport("ntdll.dll")]
		public static extern int NtQueryObject(IntPtr ObjectHandle, int ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

		[DllImport("ntdll.dll")]
		public static extern uint NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr OpenMutex(uint desiredAccess, bool inheritHandle, string name);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetCurrentProcess();
	}

	public class Win32Processes
	{
		private const int CNST_SYSTEM_HANDLE_INFORMATION = 16;

		private const uint STATUS_INFO_LENGTH_MISMATCH = 3221225476u;

		public static string getObjectTypeName(Win32API.SYSTEM_HANDLE_INFORMATION shHandle, Process process)
		{
			IntPtr hSourceProcessHandle = Win32API.OpenProcess(Win32API.ProcessAccessFlags.All, bInheritHandle: false, process.Id);
			IntPtr lpTargetHandle = IntPtr.Zero;
			Win32API.OBJECT_BASIC_INFORMATION oBJECT_BASIC_INFORMATION = default(Win32API.OBJECT_BASIC_INFORMATION);
			IntPtr zero = IntPtr.Zero;
			Win32API.OBJECT_TYPE_INFORMATION oBJECT_TYPE_INFORMATION = default(Win32API.OBJECT_TYPE_INFORMATION);
			IntPtr zero2 = IntPtr.Zero;
			_ = IntPtr.Zero;
			int returnLength = 0;
			IntPtr zero3 = IntPtr.Zero;
			if (!Win32API.DuplicateHandle(hSourceProcessHandle, shHandle.Handle, Win32API.GetCurrentProcess(), out lpTargetHandle, 0u, bInheritHandle: false, 2u))
			{
				return null;
			}
			zero = Marshal.AllocHGlobal(Marshal.SizeOf((object)oBJECT_BASIC_INFORMATION));
			Win32API.NtQueryObject(lpTargetHandle, 0, zero, Marshal.SizeOf((object)oBJECT_BASIC_INFORMATION), ref returnLength);
			oBJECT_BASIC_INFORMATION = (Win32API.OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(zero, oBJECT_BASIC_INFORMATION.GetType());
			Marshal.FreeHGlobal(zero);
			zero2 = Marshal.AllocHGlobal(oBJECT_BASIC_INFORMATION.TypeInformationLength);
			returnLength = oBJECT_BASIC_INFORMATION.TypeInformationLength;
			while (Win32API.NtQueryObject(lpTargetHandle, 2, zero2, returnLength, ref returnLength) == -1073741820)
			{
				Marshal.FreeHGlobal(zero2);
				zero2 = Marshal.AllocHGlobal(returnLength);
			}
			oBJECT_TYPE_INFORMATION = (Win32API.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(zero2, oBJECT_TYPE_INFORMATION.GetType());
			zero3 = ((!Is64Bits()) ? oBJECT_TYPE_INFORMATION.Name.Buffer : new IntPtr(Convert.ToInt64(oBJECT_TYPE_INFORMATION.Name.Buffer.ToString(), 10) >> 32));
			string result = Marshal.PtrToStringUni(zero3, oBJECT_TYPE_INFORMATION.Name.Length >> 1);
			Marshal.FreeHGlobal(zero2);
			return result;
		}

		public static string getObjectName(Win32API.SYSTEM_HANDLE_INFORMATION shHandle, Process process)
		{
			IntPtr hSourceProcessHandle = Win32API.OpenProcess(Win32API.ProcessAccessFlags.All, bInheritHandle: false, process.Id);
			IntPtr lpTargetHandle = IntPtr.Zero;
			Win32API.OBJECT_BASIC_INFORMATION oBJECT_BASIC_INFORMATION = default(Win32API.OBJECT_BASIC_INFORMATION);
			IntPtr zero = IntPtr.Zero;
			_ = IntPtr.Zero;
			Win32API.OBJECT_NAME_INFORMATION oBJECT_NAME_INFORMATION = default(Win32API.OBJECT_NAME_INFORMATION);
			IntPtr zero2 = IntPtr.Zero;
			int returnLength = 0;
			IntPtr zero3 = IntPtr.Zero;
			if (!Win32API.DuplicateHandle(hSourceProcessHandle, shHandle.Handle, Win32API.GetCurrentProcess(), out lpTargetHandle, 0u, bInheritHandle: false, 2u))
			{
				return null;
			}
			zero = Marshal.AllocHGlobal(Marshal.SizeOf((object)oBJECT_BASIC_INFORMATION));
			Win32API.NtQueryObject(lpTargetHandle, 0, zero, Marshal.SizeOf((object)oBJECT_BASIC_INFORMATION), ref returnLength);
			oBJECT_BASIC_INFORMATION = (Win32API.OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(zero, oBJECT_BASIC_INFORMATION.GetType());
			Marshal.FreeHGlobal(zero);
			returnLength = oBJECT_BASIC_INFORMATION.NameInformationLength;
			zero2 = Marshal.AllocHGlobal(returnLength);
			while (Win32API.NtQueryObject(lpTargetHandle, 1, zero2, returnLength, ref returnLength) == -1073741820)
			{
				Marshal.FreeHGlobal(zero2);
				zero2 = Marshal.AllocHGlobal(returnLength);
			}
			oBJECT_NAME_INFORMATION = (Win32API.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(zero2, oBJECT_NAME_INFORMATION.GetType());
			zero3 = ((!Is64Bits()) ? oBJECT_NAME_INFORMATION.Name.Buffer : new IntPtr(Convert.ToInt64(oBJECT_NAME_INFORMATION.Name.Buffer.ToString(), 10) >> 32));
			if (zero3 != IntPtr.Zero)
			{
				byte[] destination = new byte[returnLength];
				try
				{
					Marshal.Copy(zero3, destination, 0, returnLength);
					return Marshal.PtrToStringUni(Is64Bits() ? new IntPtr(zero3.ToInt64()) : new IntPtr(zero3.ToInt32()));
				}
				catch (AccessViolationException)
				{
					return null;
				}
				finally
				{
					Marshal.FreeHGlobal(zero2);
					Win32API.CloseHandle(lpTargetHandle);
				}
			}
			return null;
		}

		public static List<Win32API.SYSTEM_HANDLE_INFORMATION> GetHandles(Process process = null, string IN_strObjectTypeName = null, string IN_strObjectName = null)
		{
			int num = 65536;
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			int returnLength = 0;
			IntPtr zero = IntPtr.Zero;
			while (Win32API.NtQuerySystemInformation(16, intPtr, num, ref returnLength) == 3221225476u)
			{
				num = returnLength;
				Marshal.FreeHGlobal(intPtr);
				intPtr = Marshal.AllocHGlobal(returnLength);
			}
			byte[] destination = new byte[returnLength];
			Marshal.Copy(intPtr, destination, 0, returnLength);
			long num2 = 0L;
			if (Is64Bits())
			{
				num2 = Marshal.ReadInt64(intPtr);
				zero = new IntPtr(intPtr.ToInt64() + 8);
			}
			else
			{
				num2 = Marshal.ReadInt32(intPtr);
				zero = new IntPtr(intPtr.ToInt32() + 4);
			}
			List<Win32API.SYSTEM_HANDLE_INFORMATION> list = new List<Win32API.SYSTEM_HANDLE_INFORMATION>();
			for (long num3 = 0L; num3 < num2; num3++)
			{
				Win32API.SYSTEM_HANDLE_INFORMATION sYSTEM_HANDLE_INFORMATION = default(Win32API.SYSTEM_HANDLE_INFORMATION);
				if (Is64Bits())
				{
					sYSTEM_HANDLE_INFORMATION = (Win32API.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(zero, sYSTEM_HANDLE_INFORMATION.GetType());
					zero = new IntPtr(zero.ToInt64() + Marshal.SizeOf((object)sYSTEM_HANDLE_INFORMATION) + 8);
				}
				else
				{
					zero = new IntPtr(zero.ToInt64() + Marshal.SizeOf((object)sYSTEM_HANDLE_INFORMATION));
					sYSTEM_HANDLE_INFORMATION = (Win32API.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(zero, sYSTEM_HANDLE_INFORMATION.GetType());
				}
				if ((process == null || sYSTEM_HANDLE_INFORMATION.ProcessID == process.Id) && (IN_strObjectTypeName == null || !(getObjectTypeName(sYSTEM_HANDLE_INFORMATION, Process.GetProcessById(sYSTEM_HANDLE_INFORMATION.ProcessID)) != IN_strObjectTypeName)) && (IN_strObjectName == null || !(getObjectName(sYSTEM_HANDLE_INFORMATION, Process.GetProcessById(sYSTEM_HANDLE_INFORMATION.ProcessID)) != IN_strObjectName)))
				{
					getObjectTypeName(sYSTEM_HANDLE_INFORMATION, Process.GetProcessById(sYSTEM_HANDLE_INFORMATION.ProcessID));
					getObjectName(sYSTEM_HANDLE_INFORMATION, Process.GetProcessById(sYSTEM_HANDLE_INFORMATION.ProcessID));
					list.Add(sYSTEM_HANDLE_INFORMATION);
				}
			}
			return list;
		}

		public static bool Is64Bits()
		{
			return Marshal.SizeOf(typeof(IntPtr)) == 8;
		}
	}

	public static string[] Get(Process process)
	{
		List<string> list = new List<string>();
		foreach (Win32API.SYSTEM_HANDLE_INFORMATION handle in Win32Processes.GetHandles(process, "Mutant"))
		{
			string objectName = Win32Processes.getObjectName(handle, Process.GetProcessById(handle.ProcessID));
			if (!string.IsNullOrEmpty(objectName) && objectName.StartsWith("\\Sessions\\1\\BaseNamedObjects\\") && !objectName.StartsWith("\\Sessions\\1\\BaseNamedObjects\\SM0:"))
			{
				list.Add(objectName.Replace("\\Sessions\\1\\BaseNamedObjects\\", ""));
			}
		}
		return list.ToArray();
	}
}
