using System;
using System.Runtime.InteropServices;

namespace NAudio.Utils;

public static class NativeMethods
{
	[DllImport("kernel32.dll")]
	public static extern IntPtr LoadLibrary(string dllToLoad);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

	[DllImport("kernel32.dll")]
	public static extern bool FreeLibrary(IntPtr hModule);
}
