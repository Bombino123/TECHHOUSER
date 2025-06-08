using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("068346e8-aaec-4b84-add7-137f513f77a1")]
public class Output6 : Output5
{
	public OutputDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public Output6(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output6(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output6(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription1(out OutputDescription1 descRef)
	{
		OutputDescription1.__Native @ref = default(OutputDescription1.__Native);
		descRef = default(OutputDescription1);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void CheckHardwareCompositionSupport(out int flagsRef)
	{
		Result result;
		fixed (int* ptr = &flagsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
