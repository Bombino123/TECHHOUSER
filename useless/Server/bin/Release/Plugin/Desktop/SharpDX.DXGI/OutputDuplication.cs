using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("191cfac3-a341-470d-b26e-a864f428319c")]
public class OutputDuplication : DXGIObject
{
	public OutputDuplicateDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public DataRectangle MapDesktopSurface()
	{
		MapDesktopSurface(out var lockedRectRef);
		return new DataRectangle(lockedRectRef.PBits, lockedRectRef.Pitch);
	}

	public void AcquireNextFrame(int timeoutInMilliseconds, out OutputDuplicateFrameInformation frameInfoRef, out Resource desktopResourceOut)
	{
		TryAcquireNextFrame(timeoutInMilliseconds, out frameInfoRef, out desktopResourceOut).CheckError();
	}

	public OutputDuplication(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator OutputDuplication(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new OutputDuplication(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out OutputDuplicateDescription descRef)
	{
		descRef = default(OutputDuplicateDescription);
		fixed (OutputDuplicateDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	public unsafe Result TryAcquireNextFrame(int timeoutInMilliseconds, out OutputDuplicateFrameInformation frameInfoRef, out Resource desktopResourceOut)
	{
		frameInfoRef = default(OutputDuplicateFrameInformation);
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (OutputDuplicateFrameInformation* ptr = &frameInfoRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, timeoutInMilliseconds, ptr2, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			desktopResourceOut = new Resource(zero);
			return result;
		}
		desktopResourceOut = null;
		return result;
	}

	public unsafe void GetFrameDirtyRects(int dirtyRectsBufferSize, RawRectangle[] dirtyRectsBufferRef, out int dirtyRectsBufferSizeRequiredRef)
	{
		Result result;
		fixed (int* ptr = &dirtyRectsBufferSizeRequiredRef)
		{
			void* ptr2 = ptr;
			fixed (RawRectangle* ptr3 = dirtyRectsBufferRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, dirtyRectsBufferSize, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetFrameMoveRects(int moveRectsBufferSize, OutputDuplicateMoveRectangle[] moveRectBufferRef, out int moveRectsBufferSizeRequiredRef)
	{
		Result result;
		fixed (int* ptr = &moveRectsBufferSizeRequiredRef)
		{
			void* ptr2 = ptr;
			fixed (OutputDuplicateMoveRectangle* ptr3 = moveRectBufferRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, moveRectsBufferSize, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetFramePointerShape(int pointerShapeBufferSize, IntPtr pointerShapeBufferRef, out int pointerShapeBufferSizeRequiredRef, out OutputDuplicatePointerShapeInformation pointerShapeInfoRef)
	{
		pointerShapeInfoRef = default(OutputDuplicatePointerShapeInformation);
		Result result;
		fixed (OutputDuplicatePointerShapeInformation* ptr = &pointerShapeInfoRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &pointerShapeBufferSizeRequiredRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, pointerShapeBufferSize, (void*)pointerShapeBufferRef, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void MapDesktopSurface(out MappedRectangle lockedRectRef)
	{
		lockedRectRef = default(MappedRectangle);
		Result result;
		fixed (MappedRectangle* ptr = &lockedRectRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void UnMapDesktopSurface()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void ReleaseFrame()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
