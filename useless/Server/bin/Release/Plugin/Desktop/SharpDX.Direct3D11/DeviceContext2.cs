using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("420d5b32-b90c-4da4-bef0-359f6a24a83a")]
public class DeviceContext2 : DeviceContext1
{
	public RawBool IsAnnotationEnabled => IsAnnotationEnabled_();

	public DeviceContext2(Device2 device)
		: base(IntPtr.Zero)
	{
		device.CreateDeferredContext2(0, this);
	}

	public DeviceContext2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext2(nativePtr);
		}
		return null;
	}

	public unsafe void UpdateTileMappings(Resource tiledResourceRef, int numTiledResourceRegions, TiledResourceCoordinate[] tiledResourceRegionStartCoordinatesRef, TileRegionSize[] tiledResourceRegionSizesRef, Buffer tilePoolRef, int numRanges, TileRangeFlags[] rangeFlagsRef, int[] tilePoolStartOffsetsRef, int[] rangeTileCountsRef, TileMappingFlags flags)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(tiledResourceRef);
		zero2 = CppObject.ToCallbackPtr<Buffer>(tilePoolRef);
		Result result;
		fixed (int* ptr = rangeTileCountsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = tilePoolStartOffsetsRef)
			{
				void* ptr4 = ptr3;
				fixed (TileRangeFlags* ptr5 = rangeFlagsRef)
				{
					void* ptr6 = ptr5;
					fixed (TileRegionSize* ptr7 = tiledResourceRegionSizesRef)
					{
						void* ptr8 = ptr7;
						fixed (TiledResourceCoordinate* ptr9 = tiledResourceRegionStartCoordinatesRef)
						{
							void* ptr10 = ptr9;
							result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, int, void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)134 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, numTiledResourceRegions, ptr10, ptr8, (void*)zero2, numRanges, ptr6, ptr4, ptr2, (int)flags);
						}
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void CopyTileMappings(Resource destTiledResourceRef, TiledResourceCoordinate destRegionStartCoordinateRef, Resource sourceTiledResourceRef, TiledResourceCoordinate sourceRegionStartCoordinateRef, TileRegionSize tileRegionSizeRef, TileMappingFlags flags)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(destTiledResourceRef);
		zero2 = CppObject.ToCallbackPtr<Resource>(sourceTiledResourceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)135 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &destRegionStartCoordinateRef, (void*)zero2, &sourceRegionStartCoordinateRef, &tileRegionSizeRef, (int)flags)).CheckError();
	}

	public unsafe void CopyTiles(Resource tiledResourceRef, TiledResourceCoordinate tileRegionStartCoordinateRef, TileRegionSize tileRegionSizeRef, Buffer bufferRef, long bufferStartOffsetInBytes, TileMappingFlags flags)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(tiledResourceRef);
		zero2 = CppObject.ToCallbackPtr<Buffer>(bufferRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, long, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)136 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &tileRegionStartCoordinateRef, &tileRegionSizeRef, (void*)zero2, bufferStartOffsetInBytes, (int)flags);
	}

	public unsafe void UpdateTiles(Resource destTiledResourceRef, TiledResourceCoordinate destTileRegionStartCoordinateRef, TileRegionSize destTileRegionSizeRef, IntPtr sourceTileDataRef, TileMappingFlags flags)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(destTiledResourceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)137 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &destTileRegionStartCoordinateRef, &destTileRegionSizeRef, (void*)sourceTileDataRef, (int)flags);
	}

	public unsafe void ResizeTilePool(Buffer tilePoolRef, long newSizeInBytes)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Buffer>(tilePoolRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)138 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, newSizeInBytes)).CheckError();
	}

	public unsafe void TiledResourceBarrier(DeviceChild tiledResourceOrViewAccessBeforeBarrierRef, DeviceChild tiledResourceOrViewAccessAfterBarrierRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DeviceChild>(tiledResourceOrViewAccessBeforeBarrierRef);
		zero2 = CppObject.ToCallbackPtr<DeviceChild>(tiledResourceOrViewAccessAfterBarrierRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)139 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2);
	}

	internal unsafe RawBool IsAnnotationEnabled_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)140 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void SetMarkerInt(string labelRef, int data)
	{
		fixed (char* ptr = labelRef)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)141 * (nint)sizeof(void*))))(_nativePointer, ptr, data);
		}
	}

	public unsafe void BeginEventInt(string labelRef, int data)
	{
		fixed (char* ptr = labelRef)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)142 * (nint)sizeof(void*))))(_nativePointer, ptr, data);
		}
	}

	public unsafe void EndEvent()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)143 * (nint)sizeof(void*))))(_nativePointer);
	}
}
