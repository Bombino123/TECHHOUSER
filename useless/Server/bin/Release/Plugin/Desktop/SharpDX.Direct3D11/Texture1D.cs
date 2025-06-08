using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("f8fb5c27-c6b3-4f75-a4c8-439af2ef564c")]
public class Texture1D : Resource
{
	public Texture1DDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Texture1D(Device device, Texture1DDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateTexture1D(ref description, null, this);
	}

	public Texture1D(Device device, Texture1DDescription description, params DataStream[] data)
		: base(IntPtr.Zero)
	{
		DataBox[] array = null;
		if (data != null && data.Length != 0)
		{
			array = new DataBox[data.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DataPointer = data[i].DataPointer;
			}
		}
		device.CreateTexture1D(ref description, array, this);
	}

	public Texture1D(Device device, Texture1DDescription description, params IntPtr[] data)
		: base(IntPtr.Zero)
	{
		DataBox[] array = null;
		if (data != null && data.Length != 0)
		{
			array = new DataBox[data.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DataPointer = data[i];
			}
		}
		device.CreateTexture1D(ref description, array, this);
	}

	public Texture1D(Device device, Texture1DDescription description, DataBox[] data)
		: base(IntPtr.Zero)
	{
		device.CreateTexture1D(ref description, data, this);
	}

	public override int CalculateSubResourceIndex(int mipSlice, int arraySlice, out int mipSize)
	{
		Texture1DDescription description = Description;
		mipSize = Resource.CalculateMipSize(mipSlice, description.Width);
		return Resource.CalculateSubResourceIndex(mipSlice, arraySlice, description.MipLevels);
	}

	public Texture1D(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Texture1D(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Texture1D(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out Texture1DDescription descRef)
	{
		descRef = default(Texture1DDescription);
		fixed (Texture1DDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
