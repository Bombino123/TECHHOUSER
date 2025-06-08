using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("51218251-1E33-4617-9CCB-4D3A4367E7BB")]
public class Texture2D1 : Texture2D
{
	public Texture2DDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public Texture2D1(Device3 device, Texture2DDescription1 description)
		: base(IntPtr.Zero)
	{
		device.CreateTexture2D1(ref description, null, this);
	}

	public Texture2D1(Device3 device, Texture2DDescription1 description, params DataRectangle[] data)
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
		device.CreateTexture2D1(ref description, array, this);
	}

	public Texture2D1(Device3 device, Texture2DDescription1 description, DataBox[] data)
		: base(IntPtr.Zero)
	{
		device.CreateTexture2D1(ref description, data, this);
	}

	public override int CalculateSubResourceIndex(int mipSlice, int arraySlice, out int mipSize)
	{
		Texture2DDescription description = base.Description;
		mipSize = Resource.CalculateMipSize(mipSlice, description.Height);
		return Resource.CalculateSubResourceIndex(mipSlice, arraySlice, description.MipLevels);
	}

	public Texture2D1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Texture2D1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Texture2D1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out Texture2DDescription1 descRef)
	{
		descRef = default(Texture2DDescription1);
		fixed (Texture2DDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
