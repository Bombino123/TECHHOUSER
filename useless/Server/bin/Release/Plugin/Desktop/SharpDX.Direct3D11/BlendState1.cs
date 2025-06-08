using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("cc86fabe-da55-401d-85e7-e3c9de2877e9")]
public class BlendState1 : BlendState
{
	public BlendStateDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public BlendState1(Device1 device, BlendStateDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateBlendState1(ref description, this);
	}

	public BlendState1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BlendState1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BlendState1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out BlendStateDescription1 descRef)
	{
		BlendStateDescription1.__Native @ref = default(BlendStateDescription1.__Native);
		descRef = default(BlendStateDescription1);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
	}
}
