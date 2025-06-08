using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("8a6bb301-7e7e-41F4-a8e0-5b32f7f99b18")]
public class Output3 : Output2
{
	public Output3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output3(nativePtr);
		}
		return null;
	}

	public unsafe OverlaySupportFlags CheckOverlaySupport(Format enumFormat, IUnknown concernedDeviceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(concernedDeviceRef);
		OverlaySupportFlags result = default(OverlaySupportFlags);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (int)enumFormat, (void*)zero, &result)).CheckError();
		return result;
	}
}
