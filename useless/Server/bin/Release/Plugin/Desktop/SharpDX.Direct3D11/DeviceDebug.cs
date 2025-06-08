using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[Guid("79cf2233-7536-4948-9d36-1e4692dc5760")]
public class DeviceDebug : ComObject
{
	public DebugFeatureFlags FeatureFlags
	{
		get
		{
			return (DebugFeatureFlags)GetFeatureFlags();
		}
		set
		{
			SetFeatureFlags((int)value);
		}
	}

	public int PresentDelay
	{
		get
		{
			return GetPresentDelay();
		}
		set
		{
			SetPresentDelay(value);
		}
	}

	public SwapChain SwapChain
	{
		get
		{
			GetSwapChain(out var swapChainOut);
			return swapChainOut;
		}
		set
		{
			SetSwapChain(value);
		}
	}

	public DeviceDebug(Device device)
	{
		QueryInterfaceFrom(device);
	}

	public DeviceDebug(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceDebug(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceDebug(nativePtr);
		}
		return null;
	}

	internal unsafe void SetFeatureFlags(int mask)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, mask)).CheckError();
	}

	internal unsafe int GetFeatureFlags()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetPresentDelay(int milliseconds)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, milliseconds)).CheckError();
	}

	internal unsafe int GetPresentDelay()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetSwapChain(SwapChain swapChainRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SwapChain>(swapChainRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetSwapChain(out SwapChain swapChainOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			swapChainOut = new SwapChain(zero);
		}
		else
		{
			swapChainOut = null;
		}
		result.CheckError();
	}

	public unsafe void ValidateContext(DeviceContext contextRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DeviceContext>(contextRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void ReportLiveDeviceObjects(ReportingLevel flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)flags)).CheckError();
	}

	public unsafe void ValidateContextForDispatch(DeviceContext contextRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DeviceContext>(contextRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}
}
