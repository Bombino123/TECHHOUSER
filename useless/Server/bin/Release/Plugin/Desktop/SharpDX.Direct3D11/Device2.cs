using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[Guid("9d06dffa-d1e5-4d07-83a8-1bb123f2f841")]
public class Device2 : Device1
{
	protected internal DeviceContext2 ImmediateContext2__;

	public DeviceContext2 ImmediateContext2
	{
		get
		{
			if (ImmediateContext2__ == null)
			{
				GetImmediateContext2(out ImmediateContext2__);
			}
			return ImmediateContext2__;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ImmediateContext2__ != null)
		{
			ImmediateContext2__.Dispose();
			ImmediateContext2__ = null;
		}
		base.Dispose(disposing);
	}

	public Device2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device2(nativePtr);
		}
		return null;
	}

	internal unsafe void GetImmediateContext2(out DeviceContext2 immediateContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			immediateContextOut = new DeviceContext2(zero);
		}
		else
		{
			immediateContextOut = null;
		}
	}

	internal unsafe void CreateDeferredContext2(int contextFlags, DeviceContext2 deferredContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, contextFlags, &zero);
		deferredContextOut.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void GetResourceTiling(Resource tiledResourceRef, out int numTilesForEntireResourceRef, out PackedMipDescription packedMipDescRef, out TileShape standardTileShapeForNonPackedMipsRef, ref int numSubresourceTilingsRef, int firstSubresourceTilingToGet, SubResourceTiling[] subresourceTilingsForNonPackedMipsRef)
	{
		IntPtr zero = IntPtr.Zero;
		packedMipDescRef = default(PackedMipDescription);
		standardTileShapeForNonPackedMipsRef = default(TileShape);
		zero = CppObject.ToCallbackPtr<Resource>(tiledResourceRef);
		fixed (SubResourceTiling* ptr = subresourceTilingsForNonPackedMipsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &numSubresourceTilingsRef)
			{
				void* ptr4 = ptr3;
				fixed (TileShape* ptr5 = &standardTileShapeForNonPackedMipsRef)
				{
					void* ptr6 = ptr5;
					fixed (PackedMipDescription* ptr7 = &packedMipDescRef)
					{
						void* ptr8 = ptr7;
						fixed (int* ptr9 = &numTilesForEntireResourceRef)
						{
							void* ptr10 = ptr9;
							((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr10, ptr8, ptr6, ptr4, firstSubresourceTilingToGet, ptr2);
						}
					}
				}
			}
		}
	}

	public unsafe int CheckMultisampleQualityLevels1(Format format, int sampleCount, CheckMultisampleQualityLevelsFlags flags)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, (int)format, sampleCount, (int)flags, &result)).CheckError();
		return result;
	}
}
