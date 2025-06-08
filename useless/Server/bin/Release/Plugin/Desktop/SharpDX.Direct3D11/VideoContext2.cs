using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct3D11;

[Guid("C4E7374C-6243-4D1B-AE87-52B4F740E261")]
public class VideoContext2 : VideoContext1
{
	public VideoContext2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoContext2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoContext2(nativePtr);
		}
		return null;
	}

	public unsafe void VideoProcessorSetOutputHDRMetaData(VideoProcessor videoProcessorRef, HdrMetadataType type, int size, IntPtr hDRMetaDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)79 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)type, size, (void*)hDRMetaDataRef);
	}

	public unsafe void VideoProcessorGetOutputHDRMetaData(VideoProcessor videoProcessorRef, out HdrMetadataType typeRef, int size, IntPtr metaDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (HdrMetadataType* ptr = &typeRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)80 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, size, (void*)metaDataRef);
		}
	}

	public unsafe void VideoProcessorSetStreamHDRMetaData(VideoProcessor videoProcessorRef, int streamIndex, HdrMetadataType type, int size, IntPtr hDRMetaDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)81 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, (int)type, size, (void*)hDRMetaDataRef);
	}

	public unsafe void VideoProcessorGetStreamHDRMetaData(VideoProcessor videoProcessorRef, int streamIndex, out HdrMetadataType typeRef, int size, IntPtr metaDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (HdrMetadataType* ptr = &typeRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)82 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr2, size, (void*)metaDataRef);
		}
	}
}
