using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("8992ab71-02e6-4b8d-ba48-b056dcda42c4")]
public class Device4 : Device3
{
	public Device4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device4(nativePtr);
		}
		return null;
	}

	public unsafe int RegisterDeviceRemovedEvent(IntPtr hEvent)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)65 * (nint)sizeof(void*))))(_nativePointer, (void*)hEvent, &result)).CheckError();
		return result;
	}

	public unsafe void UnregisterDeviceRemoved(int dwCookie)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)66 * (nint)sizeof(void*))))(_nativePointer, dwCookie);
	}
}
