using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("41e7d1f2-a591-4f7b-a2e5-fa9c843e1c12")]
public class FactoryMedia : ComObject
{
	public FactoryMedia(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FactoryMedia(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FactoryMedia(nativePtr);
		}
		return null;
	}

	public unsafe void CreateSwapChainForCompositionSurfaceHandle(IUnknown deviceRef, IntPtr hSurface, ref SwapChainDescription1 descRef, Output restrictToOutputRef, out SwapChain1 swapChainOut)
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
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)hSurface, ptr2, (void*)zero2, &zero3);
		}
		if (zero3 != IntPtr.Zero)
		{
			swapChainOut = new SwapChain1(zero3);
		}
		else
		{
			swapChainOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateDecodeSwapChainForCompositionSurfaceHandle(IUnknown deviceRef, IntPtr hSurface, DecodeSwapChainDescription descRef, Resource yuvDecodeBuffersRef, Output restrictToOutputRef, out DecodeSwapChain swapChainOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		IntPtr zero4 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		zero2 = CppObject.ToCallbackPtr<Resource>(yuvDecodeBuffersRef);
		zero3 = CppObject.ToCallbackPtr<Output>(restrictToOutputRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)hSurface, &descRef, (void*)zero2, (void*)zero3, &zero4);
		if (zero4 != IntPtr.Zero)
		{
			swapChainOut = new DecodeSwapChain(zero4);
		}
		else
		{
			swapChainOut = null;
		}
		result.CheckError();
	}
}
