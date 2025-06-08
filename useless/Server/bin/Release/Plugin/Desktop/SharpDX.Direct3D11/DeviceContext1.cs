using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("bb2c6faa-b5fb-4082-8e6b-388b8cfa90e1")]
public class DeviceContext1 : DeviceContext
{
	public DeviceContext1(Device1 device)
		: base(IntPtr.Zero)
	{
		device.CreateDeferredContext1(0, this);
	}

	public void ClearView(ResourceView viewRef, RawColor4 color, params RawRectangle[] rectangles)
	{
		ClearView(viewRef, color, rectangles, rectangles.Length);
	}

	public DeviceContext1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext1(nativePtr);
		}
		return null;
	}

	public unsafe void CopySubresourceRegion1(Resource dstResourceRef, int dstSubresource, int dstX, int dstY, int dstZ, Resource srcResourceRef, int srcSubresource, ResourceRegion? srcBoxRef, int copyFlags)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(dstResourceRef);
		zero2 = CppObject.ToCallbackPtr<Resource>(srcResourceRef);
		ResourceRegion value = default(ResourceRegion);
		if (srcBoxRef.HasValue)
		{
			value = srcBoxRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		void* intPtr2 = (void*)zero2;
		ResourceRegion* intPtr3 = ((!srcBoxRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, void*, int, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)115 * (nint)sizeof(void*))))(nativePointer, intPtr, dstSubresource, dstX, dstY, dstZ, intPtr2, srcSubresource, intPtr3, copyFlags);
	}

	public unsafe void UpdateSubresource1(Resource dstResourceRef, int dstSubresource, ResourceRegion? dstBoxRef, IntPtr srcDataRef, int srcRowPitch, int srcDepthPitch, int copyFlags)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(dstResourceRef);
		ResourceRegion value = default(ResourceRegion);
		if (dstBoxRef.HasValue)
		{
			value = dstBoxRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		ResourceRegion* intPtr2 = ((!dstBoxRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)116 * (nint)sizeof(void*))))(nativePointer, intPtr, dstSubresource, intPtr2, (void*)srcDataRef, srcRowPitch, srcDepthPitch, copyFlags);
	}

	public unsafe void DiscardResource(Resource resourceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)117 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void DiscardView(ResourceView resourceViewRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ResourceView>(resourceViewRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)118 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void VSSetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		if (constantBuffersOut != null)
		{
			for (int i = 0; i < constantBuffersOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Buffer>(constantBuffersOut[i]);
			}
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)119 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
	}

	public unsafe void HSSetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		if (constantBuffersOut != null)
		{
			for (int i = 0; i < constantBuffersOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Buffer>(constantBuffersOut[i]);
			}
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)120 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
	}

	public unsafe void DSSetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		if (constantBuffersOut != null)
		{
			for (int i = 0; i < constantBuffersOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Buffer>(constantBuffersOut[i]);
			}
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)121 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
	}

	public unsafe void GSSetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		if (constantBuffersOut != null)
		{
			for (int i = 0; i < constantBuffersOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Buffer>(constantBuffersOut[i]);
			}
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)122 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
	}

	public unsafe void PSSetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		if (constantBuffersOut != null)
		{
			for (int i = 0; i < constantBuffersOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Buffer>(constantBuffersOut[i]);
			}
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)123 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
	}

	public unsafe void CSSetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		if (constantBuffersOut != null)
		{
			for (int i = 0; i < constantBuffersOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Buffer>(constantBuffersOut[i]);
			}
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)124 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
	}

	public unsafe void VSGetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)125 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	public unsafe void HSGetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)126 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	public unsafe void DSGetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)127 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	public unsafe void GSGetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)128 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	public unsafe void PSGetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)129 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	public unsafe void CSGetConstantBuffers1(int startSlot, int numBuffers, Buffer[] constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		IntPtr* ptr = null;
		if (constantBuffersOut != null)
		{
			ptr = stackalloc IntPtr[constantBuffersOut.Length];
		}
		fixed (int* ptr2 = numConstantsRef)
		{
			void* ptr3 = ptr2;
			fixed (int* ptr4 = firstConstantRef)
			{
				void* ptr5 = ptr4;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)130 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, ptr, ptr5, ptr3);
			}
		}
		if (constantBuffersOut == null)
		{
			return;
		}
		for (int i = 0; i < constantBuffersOut.Length; i++)
		{
			if (ptr[i] != IntPtr.Zero)
			{
				constantBuffersOut[i] = new Buffer(ptr[i]);
			}
			else
			{
				constantBuffersOut[i] = null;
			}
		}
	}

	public unsafe void SwapDeviceContextState(DeviceContextState stateRef, out DeviceContextState previousStateOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DeviceContextState>(stateRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)131 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			previousStateOut = new DeviceContextState(zero2);
		}
		else
		{
			previousStateOut = null;
		}
	}

	public unsafe void ClearView(ResourceView viewRef, RawColor4 color, RawRectangle[] rectRef, int numRects)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ResourceView>(viewRef);
		fixed (RawRectangle* ptr = rectRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)132 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &color, ptr2, numRects);
		}
	}

	public unsafe void DiscardView1(ResourceView resourceViewRef, RawRectangle[] rectsRef, int numRects)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ResourceView>(resourceViewRef);
		fixed (RawRectangle* ptr = rectsRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)133 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, numRects);
		}
	}

	public unsafe void VSSetConstantBuffers1(int startSlot, int numBuffers, ComArray<Buffer> constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		fixed (int* ptr = numConstantsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = firstConstantRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				IntPtr intPtr = constantBuffersOut?.NativePointer ?? IntPtr.Zero;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)119 * (nint)sizeof(void*))))(nativePointer, startSlot, numBuffers, (void*)intPtr, ptr4, ptr2);
			}
		}
	}

	private unsafe void VSSetConstantBuffers1(int startSlot, int numBuffers, IntPtr constantBuffersOut, IntPtr firstConstantRef, IntPtr numConstantsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)119 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut, (void*)firstConstantRef, (void*)numConstantsRef);
	}

	public unsafe void HSSetConstantBuffers1(int startSlot, int numBuffers, ComArray<Buffer> constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		fixed (int* ptr = numConstantsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = firstConstantRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				IntPtr intPtr = constantBuffersOut?.NativePointer ?? IntPtr.Zero;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)120 * (nint)sizeof(void*))))(nativePointer, startSlot, numBuffers, (void*)intPtr, ptr4, ptr2);
			}
		}
	}

	private unsafe void HSSetConstantBuffers1(int startSlot, int numBuffers, IntPtr constantBuffersOut, IntPtr firstConstantRef, IntPtr numConstantsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)120 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut, (void*)firstConstantRef, (void*)numConstantsRef);
	}

	public unsafe void DSSetConstantBuffers1(int startSlot, int numBuffers, ComArray<Buffer> constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		fixed (int* ptr = numConstantsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = firstConstantRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				IntPtr intPtr = constantBuffersOut?.NativePointer ?? IntPtr.Zero;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)121 * (nint)sizeof(void*))))(nativePointer, startSlot, numBuffers, (void*)intPtr, ptr4, ptr2);
			}
		}
	}

	private unsafe void DSSetConstantBuffers1(int startSlot, int numBuffers, IntPtr constantBuffersOut, IntPtr firstConstantRef, IntPtr numConstantsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)121 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut, (void*)firstConstantRef, (void*)numConstantsRef);
	}

	public unsafe void GSSetConstantBuffers1(int startSlot, int numBuffers, ComArray<Buffer> constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		fixed (int* ptr = numConstantsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = firstConstantRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				IntPtr intPtr = constantBuffersOut?.NativePointer ?? IntPtr.Zero;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)122 * (nint)sizeof(void*))))(nativePointer, startSlot, numBuffers, (void*)intPtr, ptr4, ptr2);
			}
		}
	}

	private unsafe void GSSetConstantBuffers1(int startSlot, int numBuffers, IntPtr constantBuffersOut, IntPtr firstConstantRef, IntPtr numConstantsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)122 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut, (void*)firstConstantRef, (void*)numConstantsRef);
	}

	public unsafe void PSSetConstantBuffers1(int startSlot, int numBuffers, ComArray<Buffer> constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		fixed (int* ptr = numConstantsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = firstConstantRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				IntPtr intPtr = constantBuffersOut?.NativePointer ?? IntPtr.Zero;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)123 * (nint)sizeof(void*))))(nativePointer, startSlot, numBuffers, (void*)intPtr, ptr4, ptr2);
			}
		}
	}

	private unsafe void PSSetConstantBuffers1(int startSlot, int numBuffers, IntPtr constantBuffersOut, IntPtr firstConstantRef, IntPtr numConstantsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)123 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut, (void*)firstConstantRef, (void*)numConstantsRef);
	}

	public unsafe void CSSetConstantBuffers1(int startSlot, int numBuffers, ComArray<Buffer> constantBuffersOut, int[] firstConstantRef, int[] numConstantsRef)
	{
		fixed (int* ptr = numConstantsRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = firstConstantRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				IntPtr intPtr = constantBuffersOut?.NativePointer ?? IntPtr.Zero;
				((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)124 * (nint)sizeof(void*))))(nativePointer, startSlot, numBuffers, (void*)intPtr, ptr4, ptr2);
			}
		}
	}

	private unsafe void CSSetConstantBuffers1(int startSlot, int numBuffers, IntPtr constantBuffersOut, IntPtr firstConstantRef, IntPtr numConstantsRef)
	{
		((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)124 * (nint)sizeof(void*))))(_nativePointer, startSlot, numBuffers, (void*)constantBuffersOut, (void*)firstConstantRef, (void*)numConstantsRef);
	}
}
