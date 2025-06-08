using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("25483823-cd46-4c7d-86ca-47aa95b837bd")]
public class Factory3 : Factory2
{
	public int CreationFlags => GetCreationFlags();

	public Factory3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory3(nativePtr);
		}
		return null;
	}

	internal unsafe int GetCreationFlags()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer);
	}
}
