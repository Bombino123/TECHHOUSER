using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("43bebd4e-add5-4035-8f85-5608d08e9dc9")]
public class ISwapChainBackgroundPanelNative : ComObject
{
	public SwapChain SwapChain
	{
		set
		{
			SetSwapChain(value);
		}
	}

	public ISwapChainBackgroundPanelNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ISwapChainBackgroundPanelNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ISwapChainBackgroundPanelNative(nativePtr);
		}
		return null;
	}

	internal unsafe void SetSwapChain(SwapChain swapChain)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SwapChain>(swapChain);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}
}
