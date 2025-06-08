using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("9bb4ab81-ab1a-4d8f-b506-fc04200b6ee7")]
public class RasterizerState : DeviceChild
{
	public RasterizerStateDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public RasterizerState(Device device, RasterizerStateDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateRasterizerState(ref description, this);
	}

	public RasterizerState(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RasterizerState(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RasterizerState(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out RasterizerStateDescription descRef)
	{
		descRef = default(RasterizerStateDescription);
		fixed (RasterizerStateDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
