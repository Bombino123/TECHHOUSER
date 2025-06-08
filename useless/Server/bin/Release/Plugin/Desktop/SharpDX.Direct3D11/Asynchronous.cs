using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("4b35d0cd-1e15-4258-9c98-1b1333f6dd3b")]
public class Asynchronous : DeviceChild
{
	public int DataSize => GetDataSize();

	public Asynchronous(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Asynchronous(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Asynchronous(nativePtr);
		}
		return null;
	}

	internal unsafe int GetDataSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}
}
