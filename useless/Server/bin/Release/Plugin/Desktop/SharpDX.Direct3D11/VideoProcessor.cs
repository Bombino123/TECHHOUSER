using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("1D7B0652-185F-41c6-85CE-0C5BE3D4AE6C")]
public class VideoProcessor : DeviceChild
{
	public VideoProcessorContentDescription ContentDescription
	{
		get
		{
			GetContentDescription(out var descRef);
			return descRef;
		}
	}

	public VideoProcessorRateConversionCaps RateConversionCaps
	{
		get
		{
			GetRateConversionCaps(out var capsRef);
			return capsRef;
		}
	}

	public VideoProcessor(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoProcessor(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoProcessor(nativePtr);
		}
		return null;
	}

	internal unsafe void GetContentDescription(out VideoProcessorContentDescription descRef)
	{
		descRef = default(VideoProcessorContentDescription);
		fixed (VideoProcessorContentDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetRateConversionCaps(out VideoProcessorRateConversionCaps capsRef)
	{
		capsRef = default(VideoProcessorRateConversionCaps);
		fixed (VideoProcessorRateConversionCaps* ptr = &capsRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
