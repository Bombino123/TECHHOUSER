using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("03823efb-8d8f-4e1c-9aa2-f64bb2cbfdf1")]
public class DepthStencilState : DeviceChild
{
	public DepthStencilStateDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public DepthStencilState(Device device, DepthStencilStateDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateDepthStencilState(ref description, this);
	}

	public DepthStencilState(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DepthStencilState(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DepthStencilState(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out DepthStencilStateDescription descRef)
	{
		descRef = default(DepthStencilStateDescription);
		fixed (DepthStencilStateDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
