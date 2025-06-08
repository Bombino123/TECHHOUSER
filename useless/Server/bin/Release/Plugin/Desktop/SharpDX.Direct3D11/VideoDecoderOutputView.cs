using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("C2931AEA-2A85-4f20-860F-FBA1FD256E18")]
public class VideoDecoderOutputView : ResourceView
{
	public VideoDecoderOutputViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public VideoDecoderOutputView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoDecoderOutputView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoDecoderOutputView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out VideoDecoderOutputViewDescription descRef)
	{
		descRef = default(VideoDecoderOutputViewDescription);
		fixed (VideoDecoderOutputViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
