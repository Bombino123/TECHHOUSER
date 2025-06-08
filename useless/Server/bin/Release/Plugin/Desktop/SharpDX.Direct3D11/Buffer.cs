using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("48570b85-d1ee-4fcd-a250-eb350722b037")]
public class Buffer : Resource
{
	public BufferDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Buffer(Device device, BufferDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateBuffer(ref description, null, this);
	}

	public Buffer(Device device, DataStream data, BufferDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateBuffer(ref description, new DataBox(data.PositionPointer, 0, 0), this);
	}

	public Buffer(Device device, IntPtr dataPointer, BufferDescription description)
		: base(IntPtr.Zero)
	{
		device.CreateBuffer(ref description, (dataPointer != IntPtr.Zero) ? new DataBox?(new DataBox(dataPointer, 0, 0)) : null, this);
	}

	public Buffer(Device device, int sizeInBytes, ResourceUsage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceOptionFlags optionFlags, int structureByteStride)
		: base(IntPtr.Zero)
	{
		BufferDescription descRef = new BufferDescription
		{
			BindFlags = bindFlags,
			CpuAccessFlags = accessFlags,
			OptionFlags = optionFlags,
			SizeInBytes = sizeInBytes,
			Usage = usage,
			StructureByteStride = structureByteStride
		};
		device.CreateBuffer(ref descRef, null, this);
	}

	public Buffer(Device device, DataStream data, int sizeInBytes, ResourceUsage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceOptionFlags optionFlags, int structureByteStride)
		: base(IntPtr.Zero)
	{
		BufferDescription descRef = new BufferDescription
		{
			BindFlags = bindFlags,
			CpuAccessFlags = accessFlags,
			OptionFlags = optionFlags,
			SizeInBytes = sizeInBytes,
			Usage = usage,
			StructureByteStride = structureByteStride
		};
		device.CreateBuffer(ref descRef, new DataBox(data.PositionPointer, 0, 0), this);
	}

	public unsafe static Buffer Create<T>(Device device, BindFlags bindFlags, ref T data, int sizeInBytes = 0, ResourceUsage usage = ResourceUsage.Default, CpuAccessFlags accessFlags = CpuAccessFlags.None, ResourceOptionFlags optionFlags = ResourceOptionFlags.None, int structureByteStride = 0) where T : struct
	{
		Buffer buffer = new Buffer(IntPtr.Zero);
		BufferDescription bufferDescription = default(BufferDescription);
		bufferDescription.BindFlags = bindFlags;
		bufferDescription.CpuAccessFlags = accessFlags;
		bufferDescription.OptionFlags = optionFlags;
		bufferDescription.SizeInBytes = ((sizeInBytes == 0) ? Utilities.SizeOf<T>() : sizeInBytes);
		bufferDescription.Usage = usage;
		bufferDescription.StructureByteStride = structureByteStride;
		BufferDescription descRef = bufferDescription;
		fixed (T* ptr = &data)
		{
			device.CreateBuffer(ref descRef, new DataBox((IntPtr)ptr), buffer);
			return buffer;
		}
	}

	public unsafe static Buffer Create<T>(Device device, BindFlags bindFlags, T[] data, int sizeInBytes = 0, ResourceUsage usage = ResourceUsage.Default, CpuAccessFlags accessFlags = CpuAccessFlags.None, ResourceOptionFlags optionFlags = ResourceOptionFlags.None, int structureByteStride = 0) where T : struct
	{
		Buffer buffer = new Buffer(IntPtr.Zero);
		BufferDescription bufferDescription = default(BufferDescription);
		bufferDescription.BindFlags = bindFlags;
		bufferDescription.CpuAccessFlags = accessFlags;
		bufferDescription.OptionFlags = optionFlags;
		bufferDescription.SizeInBytes = ((sizeInBytes == 0) ? (Utilities.SizeOf<T>() * data.Length) : sizeInBytes);
		bufferDescription.Usage = usage;
		bufferDescription.StructureByteStride = structureByteStride;
		BufferDescription descRef = bufferDescription;
		fixed (T* ptr = &data[0])
		{
			device.CreateBuffer(ref descRef, new DataBox((IntPtr)ptr), buffer);
			return buffer;
		}
	}

	public unsafe static Buffer Create<T>(Device device, ref T data, BufferDescription description) where T : struct
	{
		Buffer buffer = new Buffer(IntPtr.Zero);
		if (description.SizeInBytes == 0)
		{
			description.SizeInBytes = Utilities.SizeOf<T>();
		}
		fixed (T* ptr = &data)
		{
			device.CreateBuffer(ref description, new DataBox((IntPtr)ptr), buffer);
			return buffer;
		}
	}

	public unsafe static Buffer Create<T>(Device device, T[] data, BufferDescription description) where T : struct
	{
		Buffer buffer = new Buffer(IntPtr.Zero);
		if (description.SizeInBytes == 0)
		{
			description.SizeInBytes = Utilities.SizeOf<T>() * data.Length;
		}
		fixed (T* ptr = &data[0])
		{
			device.CreateBuffer(ref description, new DataBox((IntPtr)ptr), buffer);
			return buffer;
		}
	}

	public Buffer(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Buffer(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Buffer(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out BufferDescription descRef)
	{
		descRef = default(BufferDescription);
		fixed (BufferDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
