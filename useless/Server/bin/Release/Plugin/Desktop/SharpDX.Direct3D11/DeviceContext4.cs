using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("917600da-f58c-4c33-98d8-3e15b390fa24")]
public class DeviceContext4 : DeviceContext3
{
	public DeviceContext4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext4(nativePtr);
		}
		return null;
	}

	public unsafe void Signal(Fence fenceRef, long value)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Fence>(fenceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)147 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, value)).CheckError();
	}

	public unsafe void Wait(Fence fenceRef, long value)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Fence>(fenceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)148 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, value)).CheckError();
	}
}
