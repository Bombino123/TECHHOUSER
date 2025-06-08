using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

internal static class Kernel32
{
	[Flags]
	public enum LoadLibraryFlags : uint
	{
		DontResolveDllReferences = 1u,
		LoadIgnoreCodeAuthzLevel = 0x10u,
		LoadLibraryAsDatafile = 2u,
		LoadLibraryAsDatafileExclusive = 0x40u,
		LoadLibraryAsImageResource = 0x20u,
		LoadLibrarySearchApplicationDir = 0x200u,
		LoadLibrarySearchDefaultDirs = 0x1000u,
		LoadLibrarySearchDllLoadDir = 0x100u,
		LoadLibrarySearchSystem32 = 0x800u,
		LoadLibrarySearchUserDirs = 0x400u,
		LoadWithAlteredSearchPath = 8u
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
}
