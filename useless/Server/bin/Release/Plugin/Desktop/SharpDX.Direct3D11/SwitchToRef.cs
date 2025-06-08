using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("1ef337e3-58e7-4f83-a692-db221f5ed47e")]
public class SwitchToRef : ComObject
{
	public SwitchToRef(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwitchToRef(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwitchToRef(nativePtr);
		}
		return null;
	}

	public unsafe RawBool SetUseRef(RawBool useRef)
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, useRef);
	}

	public unsafe RawBool GetUseRef()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}
}
