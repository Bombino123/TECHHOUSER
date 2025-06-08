using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("6007896c-3244-4afd-bf18-a6d3beda5023")]
public class Device3 : Device2
{
	public Device3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device3(nativePtr);
		}
		return null;
	}

	public unsafe void Trim()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer);
	}
}
