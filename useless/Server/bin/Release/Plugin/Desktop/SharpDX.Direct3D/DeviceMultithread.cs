using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D;

[Guid("9B7E4E00-342C-4106-A19F-4F2704F689F0")]
public class DeviceMultithread : ComObject
{
	public DeviceMultithread(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceMultithread(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceMultithread(nativePtr);
		}
		return null;
	}

	public unsafe void Enter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Leave()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe RawBool SetMultithreadProtected(RawBool bMTProtect)
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, bMTProtect);
	}

	public unsafe RawBool GetMultithreadProtected()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}
}
