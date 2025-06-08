using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("80A07424-AB52-42EB-833C-0C42FD282D98")]
public class Output5 : Output4
{
	public Output5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Output5(nativePtr);
		}
		return null;
	}

	public unsafe OutputDuplication DuplicateOutput1(IUnknown deviceRef, int flags, int supportedFormatsCount, Format[] supportedFormatsRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(deviceRef);
		Result result;
		fixed (Format* ptr = supportedFormatsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, flags, supportedFormatsCount, ptr2, &zero2);
		}
		OutputDuplication result2 = ((!(zero2 != IntPtr.Zero)) ? null : new OutputDuplication(zero2));
		result.CheckError();
		return result2;
	}
}
