using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("0C711683-2853-4846-9BB0-F3E60639E46A")]
public class Texture3D1 : Texture3D
{
	public Texture3DDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public Texture3D1(Device3 device, Texture3DDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateTexture3D1(ref description, null, this);
	}

	public Texture3D1(Device3 device, Texture3DDescription1 description, DataBox[] data)
		: base(IntPtr.Zero)
	{
		device.CreateTexture3D1(ref description, data, this);
	}

	public override int CalculateSubResourceIndex(int mipSlice, int arraySlice, out int mipSize)
	{
		Texture3DDescription description = base.Description;
		mipSize = Resource.CalculateMipSize(mipSlice, description.Depth);
		return Resource.CalculateSubResourceIndex(mipSlice, arraySlice, description.MipLevels);
	}

	public Texture3D1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Texture3D1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Texture3D1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out Texture3DDescription1 descRef)
	{
		descRef = default(Texture3DDescription1);
		fixed (Texture3DDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
