using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[Guid("31627037-53AB-4200-9061-05FAA9AB45F9")]
public class VideoProcessorEnumerator : DeviceChild
{
	public VideoProcessorContentDescription VideoProcessorContentDescription
	{
		get
		{
			GetVideoProcessorContentDescription(out var contentDescRef);
			return contentDescRef;
		}
	}

	public VideoProcessorCaps VideoProcessorCaps
	{
		get
		{
			GetVideoProcessorCaps(out var capsRef);
			return capsRef;
		}
	}

	public VideoProcessorEnumerator(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoProcessorEnumerator(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoProcessorEnumerator(nativePtr);
		}
		return null;
	}

	internal unsafe void GetVideoProcessorContentDescription(out VideoProcessorContentDescription contentDescRef)
	{
		contentDescRef = default(VideoProcessorContentDescription);
		Result result;
		fixed (VideoProcessorContentDescription* ptr = &contentDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CheckVideoProcessorFormat(Format format, out int flagsRef)
	{
		Result result;
		fixed (int* ptr = &flagsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)format, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetVideoProcessorCaps(out VideoProcessorCaps capsRef)
	{
		capsRef = default(VideoProcessorCaps);
		Result result;
		fixed (VideoProcessorCaps* ptr = &capsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetVideoProcessorRateConversionCaps(int typeIndex, out VideoProcessorRateConversionCaps capsRef)
	{
		capsRef = default(VideoProcessorRateConversionCaps);
		Result result;
		fixed (VideoProcessorRateConversionCaps* ptr = &capsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, typeIndex, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetVideoProcessorCustomRate(int typeIndex, int customRateIndex, out VideoProcessorCustomRate rateRef)
	{
		rateRef = default(VideoProcessorCustomRate);
		Result result;
		fixed (VideoProcessorCustomRate* ptr = &rateRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, typeIndex, customRateIndex, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetVideoProcessorFilterRange(VideoProcessorFilter filter, out VideoProcessorFilterRange rangeRef)
	{
		rangeRef = default(VideoProcessorFilterRange);
		Result result;
		fixed (VideoProcessorFilterRange* ptr = &rangeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (int)filter, ptr2);
		}
		result.CheckError();
	}
}
