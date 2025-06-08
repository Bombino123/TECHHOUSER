using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

internal static class DebugInterface
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate Result GetDebugInterface(ref Guid guid, out IntPtr result);

	private static readonly GetDebugInterface getDebugInterface;

	static DebugInterface()
	{
		IntPtr intPtr = Kernel32.LoadLibraryEx("dxgidebug.dll", IntPtr.Zero, Kernel32.LoadLibraryFlags.LoadLibrarySearchSystem32);
		if (intPtr != IntPtr.Zero)
		{
			IntPtr procAddress = Kernel32.GetProcAddress(intPtr, "DXGIGetDebugInterface");
			if (procAddress != IntPtr.Zero)
			{
				getDebugInterface = (GetDebugInterface)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(GetDebugInterface));
			}
		}
	}

	public static bool TryCreateComPtr<T>(out IntPtr comPtr) where T : class
	{
		comPtr = IntPtr.Zero;
		if (getDebugInterface == null)
		{
			return false;
		}
		Guid guid = SharpDXTypeExtensions.GetTypeInfo(typeof(T)).GUID;
		if (getDebugInterface(ref guid, out comPtr).Failure)
		{
			return false;
		}
		return comPtr != IntPtr.Zero;
	}
}
