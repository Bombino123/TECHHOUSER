using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("7632e1f5-ee65-4dca-87fd-84cd75f8838d")]
public class Factory5 : Factory4
{
	public Factory5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory5(nativePtr);
		}
		return null;
	}

	public unsafe void CheckFeatureSupport(Feature feature, IntPtr featureSupportDataRef, int featureSupportDataSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (int)feature, (void*)featureSupportDataRef, featureSupportDataSize)).CheckError();
	}
}
