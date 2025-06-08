using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("A7F026DA-A5F8-4487-A564-15E34357651E")]
public class VideoContext1 : VideoContext
{
	public VideoContext1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoContext1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoContext1(nativePtr);
		}
		return null;
	}

	public unsafe void SubmitDecoderBuffers1(VideoDecoder decoderRef, int numBuffers, VideoDecoderBufferDescription1[] bufferDescRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		Result result;
		fixed (VideoDecoderBufferDescription1* ptr = bufferDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)65 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, numBuffers, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetDataForNewHardwareKey(CryptoSession cryptoSessionRef, int privateInputSize, IntPtr privatInputDataRef, out long privateOutputDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		Result result;
		fixed (long* ptr = &privateOutputDataRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)66 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, privateInputSize, (void*)privatInputDataRef, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CheckCryptoSessionStatus(CryptoSession cryptoSessionRef, out CryptoSessionStatus statusRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		Result result;
		fixed (CryptoSessionStatus* ptr = &statusRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)67 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
		result.CheckError();
	}

	public unsafe void DecoderEnableDownsampling(VideoDecoder decoderRef, ColorSpaceType inputColorSpace, VideoSampleDescription outputDescRef, int referenceFrameCount)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)68 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)inputColorSpace, &outputDescRef, referenceFrameCount)).CheckError();
	}

	public unsafe void DecoderUpdateDownsampling(VideoDecoder decoderRef, VideoSampleDescription outputDescRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)69 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &outputDescRef)).CheckError();
	}

	public unsafe void VideoProcessorSetOutputColorSpace1(VideoProcessor videoProcessorRef, ColorSpaceType colorSpace)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)70 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)colorSpace);
	}

	public unsafe void VideoProcessorSetOutputShaderUsage(VideoProcessor videoProcessorRef, RawBool shaderUsage)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)71 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, shaderUsage);
	}

	public unsafe void VideoProcessorGetOutputColorSpace1(VideoProcessor videoProcessorRef, out ColorSpaceType colorSpaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (ColorSpaceType* ptr = &colorSpaceRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)72 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
	}

	public unsafe void VideoProcessorGetOutputShaderUsage(VideoProcessor videoProcessorRef, out RawBool shaderUsageRef)
	{
		IntPtr zero = IntPtr.Zero;
		shaderUsageRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawBool* ptr = &shaderUsageRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)73 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
	}

	public unsafe void VideoProcessorSetStreamColorSpace1(VideoProcessor videoProcessorRef, int streamIndex, ColorSpaceType colorSpace)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)74 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, (int)colorSpace);
	}

	public unsafe void VideoProcessorSetStreamMirror(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, RawBool flipHorizontal, RawBool flipVertical)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, RawBool, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)75 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, enable, flipHorizontal, flipVertical);
	}

	public unsafe void VideoProcessorGetStreamColorSpace1(VideoProcessor videoProcessorRef, int streamIndex, out ColorSpaceType colorSpaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (ColorSpaceType* ptr = &colorSpaceRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)76 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr2);
		}
	}

	public unsafe void VideoProcessorGetStreamMirror(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enableRef, out RawBool flipHorizontalRef, out RawBool flipVerticalRef)
	{
		IntPtr zero = IntPtr.Zero;
		enableRef = default(RawBool);
		flipHorizontalRef = default(RawBool);
		flipVerticalRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawBool* ptr = &flipVerticalRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &flipHorizontalRef)
			{
				void* ptr4 = ptr3;
				fixed (RawBool* ptr5 = &enableRef)
				{
					void* ptr6 = ptr5;
					((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)77 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr6, ptr4, ptr2);
				}
			}
		}
	}

	public unsafe void VideoProcessorGetBehaviorHints(VideoProcessor videoProcessorRef, int outputWidth, int outputHeight, Format outputFormat, int streamCount, VideoProcessorStreamBehaviorHint[] streamsRef, out int behaviorHintsRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		Result result;
		fixed (int* ptr = &behaviorHintsRef)
		{
			void* ptr2 = ptr;
			fixed (VideoProcessorStreamBehaviorHint* ptr3 = streamsRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)78 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, outputWidth, outputHeight, (int)outputFormat, streamCount, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}
