using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("F92F19D2-3ADE-45A6-A20C-F6F1EA90554B")]
public class ISwapChainPanelNative : ComObject
{
	public SwapChain SwapChain
	{
		set
		{
			SetSwapChain(value);
		}
	}

	public ISwapChainPanelNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ISwapChainPanelNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ISwapChainPanelNative(nativePtr);
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
