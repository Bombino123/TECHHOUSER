using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("cafcb56c-6ac3-4889-bf47-9e23bbd260ec")]
public class Surface : DeviceChild
{
	public SurfaceDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public DataRectangle Map(MapFlags flags)
	{
		Map(out var lockedRectRef, (int)flags);
		return new DataRectangle(lockedRectRef.PBits, lockedRectRef.Pitch);
	}

	public DataRectangle Map(MapFlags flags, out DataStream dataStream)
	{
		DataRectangle result = Map(flags);
		dataStream = new DataStream(result.DataPointer, Description.Height * result.Pitch, canRead: true, canWrite: true);
		return result;
	}

	public static Surface FromSwapChain(SwapChain swapChain, int index)
	{
		swapChain.GetBuffer(index, Utilities.GetGuidFromType(typeof(Surface)), out var surfaceOut);
		return new Surface(surfaceOut);
	}

	public Surface(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Surface(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Surface(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out SurfaceDescription descRef)
	{
		descRef = default(SurfaceDescription);
		Result result;
		fixed (SurfaceDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void Map(out MappedRectangle lockedRectRef, int mapFlags)
	{
		lockedRectRef = default(MappedRectangle);
		Result result;
		fixed (MappedRectangle* ptr = &lockedRectRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2, mapFlags);
		}
		result.CheckError();
	}

	public unsafe void Unmap()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
