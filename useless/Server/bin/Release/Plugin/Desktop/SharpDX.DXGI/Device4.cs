using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("95B4F95F-D8DA-4CA4-9EE6-3B76D5968A10")]
public class Device4 : Device3
{
	public Device4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device4(nativePtr);
		}
		return null;
	}

	public unsafe void OfferResources1(int numResources, Resource[] resourcesOut, OfferResourcePriority priority, int flags)
	{
		IntPtr* ptr = null;
		if (resourcesOut != null)
		{
			ptr = stackalloc IntPtr[resourcesOut.Length];
		}
		if (resourcesOut != null)
		{
			for (int i = 0; i < resourcesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Resource>(resourcesOut[i]);
			}
		}
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, numResources, ptr, (int)priority, flags)).CheckError();
	}

	public unsafe void ReclaimResources1(int numResources, Resource[] resourcesOut, ReclaimResourceResults[] resultsRef)
	{
		IntPtr* ptr = null;
		if (resourcesOut != null)
		{
			ptr = stackalloc IntPtr[resourcesOut.Length];
		}
		if (resourcesOut != null)
		{
			for (int i = 0; i < resourcesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Resource>(resourcesOut[i]);
			}
		}
		Result result;
		fixed (ReclaimResourceResults* ptr2 = resultsRef)
		{
			void* ptr3 = ptr2;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, numResources, ptr, ptr3);
		}
		result.CheckError();
	}

	public unsafe void OfferResources1(int numResources, ComArray<Resource> resourcesOut, OfferResourcePriority priority, int flags)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(nativePointer, numResources, (void*)intPtr, (int)priority, flags)).CheckError();
	}

	private unsafe void OfferResources1(int numResources, IntPtr resourcesOut, OfferResourcePriority priority, int flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, numResources, (void*)resourcesOut, (int)priority, flags)).CheckError();
	}

	public unsafe void ReclaimResources1(int numResources, ComArray<Resource> resourcesOut, ReclaimResourceResults[] resultsRef)
	{
		Result result;
		fixed (ReclaimResourceResults* ptr = resultsRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(nativePointer, numResources, (void*)intPtr, ptr2);
		}
		result.CheckError();
	}

	private unsafe void ReclaimResources1(int numResources, IntPtr resourcesOut, IntPtr resultsRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, numResources, (void*)resourcesOut, (void*)resultsRef)).CheckError();
	}
}
