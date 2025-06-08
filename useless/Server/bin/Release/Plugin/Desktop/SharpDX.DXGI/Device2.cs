using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("05008617-fbfd-4051-a790-144884b4f6a9")]
public class Device2 : Device1
{
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

	public unsafe void OfferResources(int numResources, Resource[] resourcesOut, OfferResourcePriority priority)
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
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, numResources, ptr, (int)priority)).CheckError();
	}

	public unsafe void ReclaimResources(int numResources, Resource[] resourcesOut, RawBool[] discardedRef)
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
		fixed (RawBool* ptr2 = discardedRef)
		{
			void* ptr3 = ptr2;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, numResources, ptr, ptr3);
		}
		result.CheckError();
	}

	public unsafe void EnqueueSetEvent(IntPtr hEvent)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)hEvent)).CheckError();
	}

	public unsafe void OfferResources(int numResources, ComArray<Resource> resourcesOut, OfferResourcePriority priority)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(nativePointer, numResources, (void*)intPtr, (int)priority)).CheckError();
	}

	private unsafe void OfferResources(int numResources, IntPtr resourcesOut, OfferResourcePriority priority)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, numResources, (void*)resourcesOut, (int)priority)).CheckError();
	}

	public unsafe void ReclaimResources(int numResources, ComArray<Resource> resourcesOut, RawBool[] discardedRef)
	{
		Result result;
		fixed (RawBool* ptr = discardedRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, numResources, (void*)intPtr, ptr2);
		}
		result.CheckError();
	}

	private unsafe void ReclaimResources(int numResources, IntPtr resourcesOut, IntPtr discardedRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, numResources, (void*)resourcesOut, (void*)discardedRef)).CheckError();
	}
}
