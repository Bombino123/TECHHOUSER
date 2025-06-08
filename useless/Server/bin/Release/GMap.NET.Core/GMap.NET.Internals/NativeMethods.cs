using System;
using System.Runtime.InteropServices;
using System.Security;

namespace GMap.NET.Internals;

[SuppressUnmanagedCodeSecurity]
internal class NativeMethods
{
	public const int WaitObject0 = 0;

	public const int WaitAbandoned = 128;

	public const int WaitTimeout = 258;

	public const int WaitFailed = -1;

	public static readonly int SpinCount = ((Environment.ProcessorCount != 1) ? 4000 : 0);

	public static readonly bool SpinEnabled = Environment.ProcessorCount != 1;

	[DllImport("kernel32.dll")]
	public static extern bool CloseHandle([In] IntPtr handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr CreateEvent([Optional][In] IntPtr eventAttributes, [In] bool manualReset, [In] bool initialState, [Optional][In] string name);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr CreateSemaphore([Optional][In] IntPtr semaphoreAttributes, [In] int initialCount, [In] int maximumCount, [Optional][In] string name);

	[DllImport("kernel32.dll")]
	public static extern bool ReleaseSemaphore([In] IntPtr semaphoreHandle, [In] int releaseCount, [In] IntPtr previousCount);

	[DllImport("kernel32.dll")]
	public static extern bool ResetEvent([In] IntPtr eventHandle);

	[DllImport("kernel32.dll")]
	public static extern bool SetEvent([In] IntPtr eventHandle);

	[DllImport("kernel32.dll")]
	public static extern int WaitForSingleObject([In] IntPtr handle, [In] int milliseconds);

	[DllImport("ntdll.dll")]
	public static extern int NtCreateKeyedEvent(out IntPtr keyedEventHandle, [In] int desiredAccess, [Optional][In] IntPtr objectAttributes, [In] int flags);

	[DllImport("ntdll.dll")]
	public static extern int NtReleaseKeyedEvent([In] IntPtr keyedEventHandle, [In] IntPtr keyValue, [In] bool alertable, [Optional][In] IntPtr timeout);

	[DllImport("ntdll.dll")]
	public static extern int NtWaitForKeyedEvent([In] IntPtr keyedEventHandle, [In] IntPtr keyValue, [In] bool alertable, [Optional][In] IntPtr timeout);
}
