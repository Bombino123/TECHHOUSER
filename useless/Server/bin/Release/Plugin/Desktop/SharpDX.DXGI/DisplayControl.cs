using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("ea9dbf1a-c88e-4486-854a-98aa0138f30c")]
public class DisplayControl : ComObject
{
	public RawBool IsStereoEnabled => IsStereoEnabled_();

	public RawBool StereoEnabled
	{
		set
		{
			SetStereoEnabled(value);
		}
	}

	public DisplayControl(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DisplayControl(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DisplayControl(nativePtr);
		}
		return null;
	}

	internal unsafe RawBool IsStereoEnabled_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetStereoEnabled(RawBool enabled)
	{
		((delegate* unmanaged[Stdcall]<void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, enabled);
	}
}
