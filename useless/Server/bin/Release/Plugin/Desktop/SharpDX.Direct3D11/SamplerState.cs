using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("da6fea51-564c-4487-9810-f0d0f9b4e3a5")]
public class SamplerState : DeviceChild
{
	public SamplerStateDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public SamplerState(Device device, SamplerStateDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateSamplerState(ref description, this);
	}

	public SamplerState(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SamplerState(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SamplerState(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out SamplerStateDescription descRef)
	{
		descRef = default(SamplerStateDescription);
		fixed (SamplerStateDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
