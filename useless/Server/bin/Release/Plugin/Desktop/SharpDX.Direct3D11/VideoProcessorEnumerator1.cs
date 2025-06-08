using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("465217F2-5568-43CF-B5B9-F61D54531CA1")]
public class VideoProcessorEnumerator1 : VideoProcessorEnumerator
{
	public VideoProcessorEnumerator1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoProcessorEnumerator1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoProcessorEnumerator1(nativePtr);
		}
		return null;
	}

	public unsafe void CheckVideoProcessorFormatConversion(Format inputFormat, ColorSpaceType inputColorSpace, Format outputFormat, ColorSpaceType outputColorSpace, out RawBool supportedRef)
	{
		supportedRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &supportedRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (int)inputFormat, (int)inputColorSpace, (int)outputFormat, (int)outputColorSpace, ptr2);
		}
		result.CheckError();
	}
}
