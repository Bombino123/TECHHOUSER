using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c")]
public class Texture2D : Resource
{
	public Texture2DDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Texture2D(Device device, Texture2DDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateTexture2D(ref description, null, this);
	}

	public Texture2D(Device device, Texture2DDescription description, params DataRectangle[] data)
		: base(IntPtr.Zero)
	{
		DataBox[] array = null;
		if (data != null && data.Length != 0)
		{
			array = new DataBox[data.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DataPointer = data[i].DataPointer;
				array[i].RowPitch = data[i].Pitch;
			}
		}
		device.CreateTexture2D(ref description, array, this);
	}

	public Texture2D(Device device, Texture2DDescription description, DataBox[] data)
		: base(IntPtr.Zero)
	{
		device.CreateTexture2D(ref description, data, this);
	}

	public override int CalculateSubResourceIndex(int mipSlice, int arraySlice, out int mipSize)
	{
		Texture2DDescription description = Description;
		mipSize = Resource.CalculateMipSize(mipSlice, description.Height);
		return Resource.CalculateSubResourceIndex(mipSlice, arraySlice, description.MipLevels);
	}

	public Texture2D(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Texture2D(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Texture2D(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out Texture2DDescription descRef)
	{
		descRef = default(Texture2DDescription);
		fixed (Texture2DDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
