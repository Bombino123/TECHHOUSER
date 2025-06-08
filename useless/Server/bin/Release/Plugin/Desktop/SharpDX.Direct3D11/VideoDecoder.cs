using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("3C9C5B51-995D-48d1-9B8D-FA5CAEDED65C")]
public class VideoDecoder : DeviceChild
{
	public IntPtr DriverHandle
	{
		get
		{
			GetDriverHandle(out var driverHandleRef);
			return driverHandleRef;
		}
	}

	public VideoDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoDecoder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoDecoder(nativePtr);
		}
		return null;
	}

	public unsafe void GetCreationParameters(out VideoDecoderDescription videoDescRef, out VideoDecoderConfig configRef)
	{
		videoDescRef = default(VideoDecoderDescription);
		configRef = default(VideoDecoderConfig);
		Result result;
		fixed (VideoDecoderConfig* ptr = &configRef)
		{
			void* ptr2 = ptr;
			fixed (VideoDecoderDescription* ptr3 = &videoDescRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetDriverHandle(out IntPtr driverHandleRef)
	{
		Result result;
		fixed (IntPtr* ptr = &driverHandleRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
