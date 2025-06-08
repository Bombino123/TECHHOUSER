using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("193dacdf-0db2-4c05-a55c-ef06cac56fd9")]
public class RefTrackingOptions : ComObject
{
	public int TrackingOptions
	{
		set
		{
			SetTrackingOptions(value);
		}
	}

	public RefTrackingOptions(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RefTrackingOptions(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RefTrackingOptions(nativePtr);
		}
		return null;
	}

	internal unsafe void SetTrackingOptions(int uOptions)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, uOptions)).CheckError();
	}
}
