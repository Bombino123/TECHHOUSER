using System;
using System.Runtime.InteropServices;

namespace Toolbelt.Drawing.Win32;

internal class Kernel32
{
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LOAD_LIBRARY dwFlags);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool EnumResourceNames(IntPtr hModule, RT type, EnumResNameProcDelegate lpEnumFunc, IntPtr lParam);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpszName, RT type);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

	[DllImport("Kernel32.dll", SetLastError = true)]
	public static extern IntPtr LockResource(IntPtr hResource);

	[DllImport("Kernel32.dll", SetLastError = true)]
	public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
}
