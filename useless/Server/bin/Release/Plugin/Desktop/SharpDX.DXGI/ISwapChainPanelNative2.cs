using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("D5A2F60C-37B2-44A2-937B-8D8EB9726821")]
public class ISwapChainPanelNative2 : ISwapChainPanelNative
{
	public IntPtr SwapChainHandle
	{
		set
		{
			SetSwapChainHandle(value);
		}
	}

	public ISwapChainPanelNative2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ISwapChainPanelNative2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ISwapChainPanelNative2(nativePtr);
		}
		return null;
	}

	internal unsafe void SetSwapChainHandle(IntPtr swapChainHandle)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)swapChainHandle)).CheckError();
	}
}
