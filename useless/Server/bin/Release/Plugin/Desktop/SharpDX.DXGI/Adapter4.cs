using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("3c8d99d1-4fbf-4181-a82c-af66bf7bd24e")]
public class Adapter4 : Adapter3
{
	public AdapterDescription3 Desc3
	{
		get
		{
			GetDesc3(out var descRef);
			return descRef;
		}
	}

	public Adapter4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Adapter4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Adapter4(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDesc3(out AdapterDescription3 descRef)
	{
		AdapterDescription3.__Native @ref = default(AdapterDescription3.__Native);
		descRef = default(AdapterDescription3);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}
}
