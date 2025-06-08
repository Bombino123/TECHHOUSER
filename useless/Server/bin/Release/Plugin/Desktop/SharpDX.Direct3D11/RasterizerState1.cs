using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("1217d7a6-5039-418c-b042-9cbe256afd6e")]
public class RasterizerState1 : RasterizerState
{
	public RasterizerStateDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public RasterizerState1(Device1 device, RasterizerStateDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateRasterizerState1(ref description, this);
	}

	public RasterizerState1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RasterizerState1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RasterizerState1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out RasterizerStateDescription1 descRef)
	{
		descRef = default(RasterizerStateDescription1);
		fixed (RasterizerStateDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
