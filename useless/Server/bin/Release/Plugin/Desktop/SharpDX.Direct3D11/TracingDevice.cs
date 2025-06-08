using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("1911c771-1587-413e-a7e0-fb26c3de0268")]
public class TracingDevice : ComObject
{
	public TracingDevice(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TracingDevice(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TracingDevice(nativePtr);
		}
		return null;
	}

	public unsafe void SetShaderTrackingOptionsByType(int resourceTypeFlags, int options)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, resourceTypeFlags, options)).CheckError();
	}

	public unsafe void SetShaderTrackingOptions(IUnknown shaderRef, int options)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(shaderRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, options)).CheckError();
	}
}
