using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("4C8798B7-1D88-4A0F-B59B-B93F600DE8C8")]
public class ISurfaceImageSourceManagerNative : ComObject
{
	public ISurfaceImageSourceManagerNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ISurfaceImageSourceManagerNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ISurfaceImageSourceManagerNative(nativePtr);
		}
		return null;
	}

	public unsafe void FlushAllSurfacesWithDevice(IUnknown device)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(device);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}
}
