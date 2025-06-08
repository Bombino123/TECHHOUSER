using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("A048285E-25A9-4527-BD93-D68B68C44254")]
public class VideoProcessorOutputView : ResourceView
{
	public VideoProcessorOutputViewDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public VideoProcessorOutputView(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoProcessorOutputView(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoProcessorOutputView(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDescription(out VideoProcessorOutputViewDescription descRef)
	{
		descRef = default(VideoProcessorOutputViewDescription);
		fixed (VideoProcessorOutputViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
