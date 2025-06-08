using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("dbf2e947-8e6c-4254-9eee-7738f71386c9")]
internal class IVirtualSurfaceUpdatesCallbackNativeNative : ComObject, IVirtualSurfaceUpdatesCallbackNative, IUnknown, ICallbackable, IDisposable
{
	public void UpdatesNeeded()
	{
		UpdatesNeeded_();
	}

	public IVirtualSurfaceUpdatesCallbackNativeNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator IVirtualSurfaceUpdatesCallbackNativeNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new IVirtualSurfaceUpdatesCallbackNativeNative(nativePtr);
		}
		return null;
	}

	internal unsafe void UpdatesNeeded_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
