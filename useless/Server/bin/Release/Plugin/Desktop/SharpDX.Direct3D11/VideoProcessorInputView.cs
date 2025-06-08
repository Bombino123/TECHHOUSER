using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("11EC5A5F-51DC-4945-AB34-6E8C21300EA5")]
public class VideoProcessorInputView : ResourceView
{
	public VideoProcessorInputViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public VideoProcessorInputView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoProcessorInputView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoProcessorInputView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out VideoProcessorInputViewDescription descRef)
	{
		descRef = default(VideoProcessorInputViewDescription);
		fixed (VideoProcessorInputViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
