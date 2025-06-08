using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[Guid("c5a05f0c-16f2-4adf-9f4d-a8c4d58ac550")]
public class DXGIDebug1 : DXGIDebug
{
	public RawBool IsLeakTrackingEnabledForThread => IsLeakTrackingEnabledForThread_();

	public new static DXGIDebug1 TryCreate()
	{
		if (!DebugInterface.TryCreateComPtr<DXGIDebug1>(out var comPtr))
		{
			return null;
		}
		return new DXGIDebug1(comPtr);
	}

	public DXGIDebug1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DXGIDebug1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DXGIDebug1(nativePtr);
		}
		return null;
	}

	public unsafe void EnableLeakTrackingForThread()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void DisableLeakTrackingForThread()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe RawBool IsLeakTrackingEnabledForThread_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}
}
