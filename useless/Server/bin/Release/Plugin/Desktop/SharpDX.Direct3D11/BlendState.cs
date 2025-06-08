using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("75b68faa-347d-4159-8f45-a0640f01cd9a")]
public class BlendState : DeviceChild
{
	public BlendStateDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public BlendState(Device device, BlendStateDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateBlendState(ref description, this);
	}

	public BlendState(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BlendState(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BlendState(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out BlendStateDescription descRef)
	{
		BlendStateDescription.__Native @ref = default(BlendStateDescription.__Native);
		descRef = default(BlendStateDescription);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
	}
}
