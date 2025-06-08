using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("3D585D5A-BD4A-489E-B1F4-3DBCB6452FFB")]
public class SwapChain4 : SwapChain3
{
	public SwapChain4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChain4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SwapChain4(nativePtr);
		}
		return null;
	}

	public unsafe void SetHDRMetaData(HdrMetadataType type, int size, IntPtr metaDataRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, (int)type, size, (void*)metaDataRef)).CheckError();
	}
}
