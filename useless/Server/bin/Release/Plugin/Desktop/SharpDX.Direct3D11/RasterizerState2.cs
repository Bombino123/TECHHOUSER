using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("6fbd02fb-209f-46c4-b059-2ed15586a6ac")]
public class RasterizerState2 : RasterizerState1
{
	public RasterizerStateDescription2 Description2
	{
		get
		{
			GetDescription2(out var descRef);
			return descRef;
		}
	}

	public RasterizerState2(Device3 device, RasterizerStateDescription2 description)
		: base(IntPtr.Zero)
	{
		device.CreateRasterizerState2(ref description, this);
	}

	public RasterizerState2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RasterizerState2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RasterizerState2(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription2(out RasterizerStateDescription2 descRef)
	{
		descRef = default(RasterizerStateDescription2);
		fixed (RasterizerStateDescription2* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
