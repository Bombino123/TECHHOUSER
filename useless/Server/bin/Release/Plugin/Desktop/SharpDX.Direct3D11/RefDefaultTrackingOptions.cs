using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("03916615-c644-418c-9bf4-75db5be63ca0")]
public class RefDefaultTrackingOptions : ComObject
{
	public RefDefaultTrackingOptions(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RefDefaultTrackingOptions(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RefDefaultTrackingOptions(nativePtr);
		}
		return null;
	}

	public unsafe void SetTrackingOptions(int resourceTypeFlags, int options)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, resourceTypeFlags, options)).CheckError();
	}
}
