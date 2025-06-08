using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("b2daad8b-03d4-4dbf-95eb-32ab4b63d0ab")]
public class UserDefinedAnnotation : ComObject
{
	public RawBool Status => GetStatus();

	public UserDefinedAnnotation(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator UserDefinedAnnotation(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new UserDefinedAnnotation(nativePtr);
		}
		return null;
	}

	public unsafe int BeginEvent(string name)
	{
		int result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		return result;
	}

	public unsafe int EndEvent()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void SetMarker(string name)
	{
		fixed (char* ptr = name)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe RawBool GetStatus()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}
}
