using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("595e39d1-2724-4663-99b1-da969de28364")]
public class Output2 : Output1
{
	public bool SupportsOverlays => SupportsOverlays_();

	public Output2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output2(nativePtr);
		}
		return null;
	}

	internal unsafe RawBool SupportsOverlays_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer);
	}
}
