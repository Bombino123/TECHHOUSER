using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("dc7dca35-2196-414d-9F53-617884032a60")]
public class Output4 : Output3
{
	public Output4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output4(nativePtr);
		}
		return null;
	}

	public unsafe OverlayColorSpaceSupportFlags CheckOverlayColorSpaceSupport(Format format, ColorSpaceType colorSpace, IUnknown concernedDeviceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(concernedDeviceRef);
		OverlayColorSpaceSupportFlags result = default(OverlayColorSpaceSupportFlags);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, (int)format, (int)colorSpace, (void*)zero, &result)).CheckError();
		return result;
	}
}
