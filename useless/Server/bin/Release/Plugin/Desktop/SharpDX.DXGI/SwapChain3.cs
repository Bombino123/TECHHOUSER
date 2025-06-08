using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("94d99bdb-f1f8-4ab0-b236-7da0170edab1")]
public class SwapChain3 : SwapChain2
{
	public int CurrentBackBufferIndex => GetCurrentBackBufferIndex();

	public ColorSpaceType ColorSpace1
	{
		set
		{
			SetColorSpace1(value);
		}
	}

	public SwapChain3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChain3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwapChain3(nativePtr);
		}
		return null;
	}

	internal unsafe int GetCurrentBackBufferIndex()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe SwapChainColorSpaceSupportFlags CheckColorSpaceSupport(ColorSpaceType colorSpace)
	{
		SwapChainColorSpaceSupportFlags result = default(SwapChainColorSpaceSupportFlags);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, (int)colorSpace, &result)).CheckError();
		return result;
	}

	internal unsafe void SetColorSpace1(ColorSpaceType colorSpace)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, (int)colorSpace)).CheckError();
	}

	public unsafe void ResizeBuffers1(int bufferCount, int width, int height, Format format, SwapChainFlags swapChainFlags, int[] creationNodeMaskRef, IUnknown[] presentQueueOut)
	{
		IntPtr* ptr = null;
		if (presentQueueOut != null)
		{
			ptr = stackalloc IntPtr[presentQueueOut.Length];
		}
		if (presentQueueOut != null)
		{
			for (int i = 0; i < presentQueueOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<IUnknown>(presentQueueOut[i]);
			}
		}
		Result result;
		fixed (int* ptr2 = creationNodeMaskRef)
		{
			void* ptr3 = ptr2;
			result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, bufferCount, width, height, (int)format, (int)swapChainFlags, ptr3, ptr);
		}
		result.CheckError();
	}
}
