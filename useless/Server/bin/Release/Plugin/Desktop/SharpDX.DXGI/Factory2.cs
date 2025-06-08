using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("50c83a1c-e072-4c48-87b0-3630fa36a6d0")]
public class Factory2 : Factory1
{
	public RawBool IsWindowedStereoEnabled => IsWindowedStereoEnabled_();

	public Factory2(bool debug = false)
		: this(IntPtr.Zero)
	{
		DXGI.CreateDXGIFactory2(debug ? 1 : 0, Utilities.GetGuidFromType(GetType()), out var factoryOut);
		base.NativePointer = factoryOut;
	}

	public Factory2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory2(nativePtr);
		}
		return null;
	}

	internal unsafe RawBool IsWindowedStereoEnabled_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void CreateSwapChainForHwnd(IUnknown deviceRef, IntPtr hWnd, ref SwapChainDescription1 descRef, SwapChainFullScreenDescription? fullscreenDescRef, Output restrictToOutputRef, SwapChain1 swapChainOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		SwapChainFullScreenDescription value = default(SwapChainFullScreenDescription);
		if (fullscreenDescRef.HasValue)
		{
			value = fullscreenDescRef.Value;
		}
		zero2 = CppObject.ToCallbackPtr<Output>(restrictToOutputRef);
		Result result;
		fixed (SwapChainDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)zero;
			void* intPtr2 = (void*)hWnd;
			SwapChainFullScreenDescription* intPtr3 = ((!fullscreenDescRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, ptr2, intPtr3, (void*)zero2, &zero3);
		}
		swapChainOut.NativePointer = zero3;
		result.CheckError();
	}

	internal unsafe void CreateSwapChainForCoreWindow(IUnknown deviceRef, IUnknown windowRef, ref SwapChainDescription1 descRef, Output restrictToOutputRef, SwapChain1 swapChainOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		IntPtr zero4 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		zero2 = CppObject.ToCallbackPtr<IUnknown>(windowRef);
		zero3 = CppObject.ToCallbackPtr<Output>(restrictToOutputRef);
		Result result;
		fixed (SwapChainDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, ptr2, (void*)zero3, &zero4);
		}
		swapChainOut.NativePointer = zero4;
		result.CheckError();
	}

	public unsafe void GetSharedResourceAdapterLuid(IntPtr hResource, out long luidRef)
	{
		Result result;
		fixed (long* ptr = &luidRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)hResource, ptr2);
		}
		result.CheckError();
	}

	public unsafe void RegisterStereoStatusWindow(IntPtr windowHandle, int wMsg, out int dwCookieRef)
	{
		Result result;
		fixed (int* ptr = &dwCookieRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (void*)windowHandle, wMsg, ptr2);
		}
		result.CheckError();
	}

	public unsafe void RegisterStereoStatusEvent(IntPtr hEvent, out int dwCookieRef)
	{
		Result result;
		fixed (int* ptr = &dwCookieRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (void*)hEvent, ptr2);
		}
		result.CheckError();
	}

	public unsafe void UnregisterStereoStatus(int dwCookie)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, dwCookie);
	}

	public unsafe void RegisterOcclusionStatusWindow(IntPtr windowHandle, int wMsg, out int dwCookieRef)
	{
		Result result;
		fixed (int* ptr = &dwCookieRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)windowHandle, wMsg, ptr2);
		}
		result.CheckError();
	}

	public unsafe void RegisterOcclusionStatusEvent(IntPtr hEvent, out int dwCookieRef)
	{
		Result result;
		fixed (int* ptr = &dwCookieRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)hEvent, ptr2);
		}
		result.CheckError();
	}

	public unsafe void UnregisterOcclusionStatus(int dwCookie)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, dwCookie);
	}

	internal unsafe void CreateSwapChainForComposition(IUnknown deviceRef, ref SwapChainDescription1 descRef, Output restrictToOutputRef, SwapChain1 swapChainOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		zero2 = CppObject.ToCallbackPtr<Output>(restrictToOutputRef);
		Result result;
		fixed (SwapChainDescription1* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, (void*)zero2, &zero3);
		}
		swapChainOut.NativePointer = zero3;
		result.CheckError();
	}
}
