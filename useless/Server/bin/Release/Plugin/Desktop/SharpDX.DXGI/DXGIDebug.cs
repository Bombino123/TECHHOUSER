using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("119E7452-DE9E-40fe-8806-88F90C12B441")]
public class DXGIDebug : ComObject
{
	public static DXGIDebug TryCreate()
	{
		if (!DebugInterface.TryCreateComPtr<DXGIDebug>(out var comPtr))
		{
			return null;
		}
		return new DXGIDebug(comPtr);
	}

	public DXGIDebug(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DXGIDebug(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DXGIDebug(nativePtr);
		}
		return null;
	}

	public unsafe void ReportLiveObjects(Guid apiid, DebugRloFlags flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Guid, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, apiid, (int)flags)).CheckError();
	}
}
