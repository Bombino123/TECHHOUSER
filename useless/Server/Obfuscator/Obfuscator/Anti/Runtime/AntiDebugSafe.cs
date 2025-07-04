﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Obfuscator.Obfuscator.Anti.Runtime
{
	// Token: 0x02000034 RID: 52
	internal static class AntiDebugSafe
	{
		// Token: 0x060000CE RID: 206
		[DllImport("ntdll.dll", CharSet = CharSet.Auto)]
		private static extern int NtQueryInformationProcess(IntPtr test, int test2, int[] test3, int test4, ref int test5);

		// Token: 0x060000CF RID: 207 RVA: 0x0000A390 File Offset: 0x00008590
		private static void Initialize()
		{
			if (Debugger.IsLogging())
			{
				Environment.Exit(0);
			}
			if (Debugger.IsAttached)
			{
				Environment.Exit(0);
			}
			if (Environment.GetEnvironmentVariable("complus_profapi_profilercompatibilitysetting") != null)
			{
				Environment.Exit(0);
			}
			if (string.Compare(Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING"), "1", StringComparison.Ordinal) == 0)
			{
				Environment.Exit(0);
			}
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				return;
			}
			int[] array = new int[6];
			int num = 0;
			IntPtr handle = Process.GetCurrentProcess().Handle;
			if (AntiDebugSafe.NtQueryInformationProcess(handle, 31, array, 4, ref num) == 0 && array[0] != 1)
			{
				Environment.Exit(0);
			}
			if (AntiDebugSafe.NtQueryInformationProcess(handle, 30, array, 4, ref num) == 0 && array[0] != 0)
			{
				Environment.Exit(0);
			}
			if (AntiDebugSafe.NtQueryInformationProcess(handle, 0, array, 24, ref num) != 0)
			{
				return;
			}
			IntPtr ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr((IntPtr)array[1], 12), 12);
			Marshal.WriteInt32(ptr, 32, 0);
			IntPtr intPtr = Marshal.ReadIntPtr(ptr, 0);
			IntPtr ptr2 = intPtr;
			do
			{
				ptr2 = Marshal.ReadIntPtr(ptr2, 0);
				if (Marshal.ReadInt32(ptr2, 44) == 1572886 && Marshal.ReadInt32(Marshal.ReadIntPtr(ptr2, 48), 0) == 7536749)
				{
					IntPtr intPtr2 = Marshal.ReadIntPtr(ptr2, 8);
					IntPtr intPtr3 = Marshal.ReadIntPtr(ptr2, 12);
					Marshal.WriteInt32(intPtr3, 0, (int)intPtr2);
					Marshal.WriteInt32(intPtr2, 4, (int)intPtr3);
				}
			}
			while (!ptr2.Equals(intPtr));
		}
	}
}
