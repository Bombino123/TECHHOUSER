using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("037e866e-f56d-4357-a8af-9dabbe6e250e")]
public class Texture3D : Resource
{
	public Texture3DDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Texture3D(Device device, Texture3DDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateTexture3D(ref description, null, this);
	}

	public Texture3D(Device device, Texture3DDescription description, DataBox[] data)
		: base(IntPtr.Zero)
	{
		device.CreateTexture3D(ref description, data, this);
	}

	public override int CalculateSubResourceIndex(int mipSlice, int arraySlice, out int mipSize)
	{
		Texture3DDescription description = Description;
		mipSize = Resource.CalculateMipSize(mipSlice, description.Depth);
		return Resource.CalculateSubResourceIndex(mipSlice, arraySlice, description.MipLevels);
	}

	public Texture3D(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Texture3D(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Texture3D(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out Texture3DDescription descRef)
	{
		descRef = default(Texture3DDescription);
		fixed (Texture3DDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
