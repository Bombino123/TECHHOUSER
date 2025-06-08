using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("0AA1AE0A-FA0E-4B84-8644-E05FF8E5ACB5")]
public class Adapter2 : Adapter1
{
	public AdapterDescription2 Description2
	{
		get
		{
			GetDescription2(out var descRef);
			return descRef;
		}
	}

	public Adapter2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Adapter2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Adapter2(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription2(out AdapterDescription2 descRef)
	{
		AdapterDescription2.__Native @ref = default(AdapterDescription2.__Native);
		descRef = default(AdapterDescription2);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}
}
