using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("61F21C45-3C0E-4a74-9CEA-67100D9AD5E4")]
public class VideoContext : DeviceChild
{
	public DataPointer GetDecoderBuffer(VideoDecoder decoder, VideoDecoderBufferType type)
	{
		GetDecoderBuffer(decoder, type, out var bufferSizeRef, out var bufferOut);
		return new DataPointer(bufferOut, bufferSizeRef);
	}

	public VideoContext(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoContext(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoContext(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDecoderBuffer(VideoDecoder decoderRef, VideoDecoderBufferType type, out int bufferSizeRef, out IntPtr bufferOut)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		Result result;
		fixed (IntPtr* ptr = &bufferOut)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &bufferSizeRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)type, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void ReleaseDecoderBuffer(VideoDecoder decoderRef, VideoDecoderBufferType type)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)type)).CheckError();
	}

	public unsafe void DecoderBeginFrame(VideoDecoder decoderRef, VideoDecoderOutputView viewRef, int contentKeySize, IntPtr contentKeyRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		zero2 = CppObject.ToCallbackPtr<VideoDecoderOutputView>(viewRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, contentKeySize, (void*)contentKeyRef)).CheckError();
	}

	public unsafe void DecoderEndFrame(VideoDecoder decoderRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void SubmitDecoderBuffers(VideoDecoder decoderRef, int numBuffers, VideoDecoderBufferDescription[] bufferDescRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		Result result;
		fixed (VideoDecoderBufferDescription* ptr = bufferDescRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, numBuffers, ptr2);
		}
		result.CheckError();
	}

	public unsafe void DecoderExtension(VideoDecoder decoderRef, ref VideoDecoderExtension extensionDataRef)
	{
		IntPtr zero = IntPtr.Zero;
		VideoDecoderExtension.__Native @ref = default(VideoDecoderExtension.__Native);
		zero = CppObject.ToCallbackPtr<VideoDecoder>(decoderRef);
		extensionDataRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &@ref);
		extensionDataRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	public unsafe void VideoProcessorSetOutputTargetRect(VideoProcessor videoProcessorRef, RawBool enable, RawRectangle? rectRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		RawRectangle value = default(RawRectangle);
		if (rectRef.HasValue)
		{
			value = rectRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangle* intPtr2 = ((!rectRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, enable, intPtr2);
	}

	public unsafe void VideoProcessorSetOutputBackgroundColor(VideoProcessor videoProcessorRef, RawBool yCbCr, VideoColor colorRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, yCbCr, &colorRef);
	}

	public unsafe void VideoProcessorSetOutputColorSpace(VideoProcessor videoProcessorRef, VideoProcessorColorSpace colorSpaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &colorSpaceRef);
	}

	public unsafe void VideoProcessorSetOutputAlphaFillMode(VideoProcessor videoProcessorRef, VideoProcessorAlphaFillMode alphaFillMode, int streamIndex)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)alphaFillMode, streamIndex);
	}

	public unsafe void VideoProcessorSetOutputConstriction(VideoProcessor videoProcessorRef, RawBool enable, Size2 size)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, Size2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, enable, size);
	}

	public unsafe void VideoProcessorSetOutputStereoMode(VideoProcessor videoProcessorRef, RawBool enable)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, enable);
	}

	public unsafe void VideoProcessorSetOutputExtension(VideoProcessor videoProcessorRef, Guid extensionGuidRef, int dataSize, IntPtr dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &extensionGuidRef, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void VideoProcessorGetOutputTargetRect(VideoProcessor videoProcessorRef, out RawBool enabled, out RawRectangle rectRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabled = default(RawBool);
		rectRef = default(RawRectangle);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawRectangle* ptr = &rectRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enabled)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetOutputBackgroundColor(VideoProcessor videoProcessorRef, out RawBool yCbCrRef, out VideoColor colorRef)
	{
		IntPtr zero = IntPtr.Zero;
		yCbCrRef = default(RawBool);
		colorRef = default(VideoColor);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (VideoColor* ptr = &colorRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &yCbCrRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetOutputColorSpace(VideoProcessor videoProcessorRef, out VideoProcessorColorSpace colorSpaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		colorSpaceRef = default(VideoProcessorColorSpace);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (VideoProcessorColorSpace* ptr = &colorSpaceRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
	}

	public unsafe void VideoProcessorGetOutputAlphaFillMode(VideoProcessor videoProcessorRef, out VideoProcessorAlphaFillMode alphaFillModeRef, out int streamIndexRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (int* ptr = &streamIndexRef)
		{
			void* ptr2 = ptr;
			fixed (VideoProcessorAlphaFillMode* ptr3 = &alphaFillModeRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetOutputConstriction(VideoProcessor videoProcessorRef, out RawBool enabledRef, out Size2 sizeRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		sizeRef = default(Size2);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (Size2* ptr = &sizeRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enabledRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetOutputStereoMode(VideoProcessor videoProcessorRef, out RawBool enabledRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawBool* ptr = &enabledRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
	}

	public unsafe void VideoProcessorGetOutputExtension(VideoProcessor videoProcessorRef, Guid extensionGuidRef, int dataSize, IntPtr dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &extensionGuidRef, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void VideoProcessorSetStreamFrameFormat(VideoProcessor videoProcessorRef, int streamIndex, VideoFrameFormat frameFormat)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, (int)frameFormat);
	}

	public unsafe void VideoProcessorSetStreamColorSpace(VideoProcessor videoProcessorRef, int streamIndex, VideoProcessorColorSpace colorSpaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, &colorSpaceRef);
	}

	public unsafe void VideoProcessorSetStreamOutputRate(VideoProcessor videoProcessorRef, int streamIndex, VideoProcessorOutputRate outputRate, RawBool repeatFrame, Rational? customRateRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		Rational value = default(Rational);
		if (customRateRef.HasValue)
		{
			value = customRateRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		Rational* intPtr2 = ((!customRateRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, RawBool, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(nativePointer, intPtr, streamIndex, (int)outputRate, repeatFrame, intPtr2);
	}

	public unsafe void VideoProcessorSetStreamSourceRect(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, RawRectangle? rectRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		RawRectangle value = default(RawRectangle);
		if (rectRef.HasValue)
		{
			value = rectRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangle* intPtr2 = ((!rectRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(nativePointer, intPtr, streamIndex, enable, intPtr2);
	}

	public unsafe void VideoProcessorSetStreamDestRect(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, RawRectangle? rectRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		RawRectangle value = default(RawRectangle);
		if (rectRef.HasValue)
		{
			value = rectRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangle* intPtr2 = ((!rectRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(nativePointer, intPtr, streamIndex, enable, intPtr2);
	}

	public unsafe void VideoProcessorSetStreamAlpha(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, float alpha)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, enable, alpha);
	}

	public unsafe void VideoProcessorSetStreamPalette(VideoProcessor videoProcessorRef, int streamIndex, int count, int[] entriesRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (int* ptr = entriesRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, count, ptr2);
		}
	}

	public unsafe void VideoProcessorSetStreamPixelAspectRatio(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, Rational? sourceAspectRatioRef, Rational? destinationAspectRatioRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		Rational value = default(Rational);
		if (sourceAspectRatioRef.HasValue)
		{
			value = sourceAspectRatioRef.Value;
		}
		Rational value2 = default(Rational);
		if (destinationAspectRatioRef.HasValue)
		{
			value2 = destinationAspectRatioRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		Rational* intPtr2 = ((!sourceAspectRatioRef.HasValue) ? null : (&value));
		Rational* intPtr3 = ((!destinationAspectRatioRef.HasValue) ? null : (&value2));
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(nativePointer, intPtr, streamIndex, enable, intPtr2, intPtr3);
	}

	public unsafe void VideoProcessorSetStreamLumaKey(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, float lower, float upper)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, float, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, enable, lower, upper);
	}

	public unsafe void VideoProcessorSetStreamStereoFormat(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, VideoProcessorStereoFormat format, RawBool leftViewFrame0, RawBool baseViewFrame0, VideoProcessorStereoFlipMode flipMode, int monoOffset)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, int, RawBool, RawBool, int, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, enable, (int)format, leftViewFrame0, baseViewFrame0, (int)flipMode, monoOffset);
	}

	public unsafe void VideoProcessorSetStreamAutoProcessingMode(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, enable);
	}

	public unsafe void VideoProcessorSetStreamFilter(VideoProcessor videoProcessorRef, int streamIndex, VideoProcessorFilter filter, RawBool enable, int level)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, int, RawBool, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, (int)filter, enable, level);
	}

	public unsafe void VideoProcessorSetStreamExtension(VideoProcessor videoProcessorRef, int streamIndex, Guid extensionGuidRef, int dataSize, IntPtr dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, &extensionGuidRef, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void VideoProcessorGetStreamFrameFormat(VideoProcessor videoProcessorRef, int streamIndex, out VideoFrameFormat frameFormatRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (VideoFrameFormat* ptr = &frameFormatRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr2);
		}
	}

	public unsafe void VideoProcessorGetStreamColorSpace(VideoProcessor videoProcessorRef, int streamIndex, out VideoProcessorColorSpace colorSpaceRef)
	{
		IntPtr zero = IntPtr.Zero;
		colorSpaceRef = default(VideoProcessorColorSpace);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (VideoProcessorColorSpace* ptr = &colorSpaceRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr2);
		}
	}

	public unsafe void VideoProcessorGetStreamOutputRate(VideoProcessor videoProcessorRef, int streamIndex, out VideoProcessorOutputRate outputRateRef, out RawBool repeatFrameRef, out Rational customRateRef)
	{
		IntPtr zero = IntPtr.Zero;
		repeatFrameRef = default(RawBool);
		customRateRef = default(Rational);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (Rational* ptr = &customRateRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &repeatFrameRef)
			{
				void* ptr4 = ptr3;
				fixed (VideoProcessorOutputRate* ptr5 = &outputRateRef)
				{
					void* ptr6 = ptr5;
					((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr6, ptr4, ptr2);
				}
			}
		}
	}

	public unsafe void VideoProcessorGetStreamSourceRect(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enabledRef, out RawRectangle rectRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		rectRef = default(RawRectangle);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawRectangle* ptr = &rectRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enabledRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetStreamDestRect(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enabledRef, out RawRectangle rectRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		rectRef = default(RawRectangle);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawRectangle* ptr = &rectRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enabledRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetStreamAlpha(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enabledRef, out float alphaRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (float* ptr = &alphaRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enabledRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetStreamPalette(VideoProcessor videoProcessorRef, int streamIndex, int count, int[] entriesRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (int* ptr = entriesRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, count, ptr2);
		}
	}

	public unsafe void VideoProcessorGetStreamPixelAspectRatio(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enabledRef, out Rational sourceAspectRatioRef, out Rational destinationAspectRatioRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		sourceAspectRatioRef = default(Rational);
		destinationAspectRatioRef = default(Rational);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (Rational* ptr = &destinationAspectRatioRef)
		{
			void* ptr2 = ptr;
			fixed (Rational* ptr3 = &sourceAspectRatioRef)
			{
				void* ptr4 = ptr3;
				fixed (RawBool* ptr5 = &enabledRef)
				{
					void* ptr6 = ptr5;
					((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr6, ptr4, ptr2);
				}
			}
		}
	}

	public unsafe void VideoProcessorGetStreamLumaKey(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enabledRef, out float lowerRef, out float upperRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (float* ptr = &upperRef)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &lowerRef)
			{
				void* ptr4 = ptr3;
				fixed (RawBool* ptr5 = &enabledRef)
				{
					void* ptr6 = ptr5;
					((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr6, ptr4, ptr2);
				}
			}
		}
	}

	public unsafe void VideoProcessorGetStreamStereoFormat(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enableRef, out VideoProcessorStereoFormat formatRef, out RawBool leftViewFrame0Ref, out RawBool baseViewFrame0Ref, out VideoProcessorStereoFlipMode flipModeRef, out int monoOffset)
	{
		IntPtr zero = IntPtr.Zero;
		enableRef = default(RawBool);
		leftViewFrame0Ref = default(RawBool);
		baseViewFrame0Ref = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (int* ptr = &monoOffset)
		{
			void* ptr2 = ptr;
			fixed (VideoProcessorStereoFlipMode* ptr3 = &flipModeRef)
			{
				void* ptr4 = ptr3;
				fixed (RawBool* ptr5 = &baseViewFrame0Ref)
				{
					void* ptr6 = ptr5;
					fixed (RawBool* ptr7 = &leftViewFrame0Ref)
					{
						void* ptr8 = ptr7;
						fixed (VideoProcessorStereoFormat* ptr9 = &formatRef)
						{
							void* ptr10 = ptr9;
							fixed (RawBool* ptr11 = &enableRef)
							{
								void* ptr12 = ptr11;
								((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr12, ptr10, ptr8, ptr6, ptr4, ptr2);
							}
						}
					}
				}
			}
		}
	}

	public unsafe void VideoProcessorGetStreamAutoProcessingMode(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enabledRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (RawBool* ptr = &enabledRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr2);
		}
	}

	public unsafe void VideoProcessorGetStreamFilter(VideoProcessor videoProcessorRef, int streamIndex, VideoProcessorFilter filter, out RawBool enabledRef, out int levelRef)
	{
		IntPtr zero = IntPtr.Zero;
		enabledRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (int* ptr = &levelRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enabledRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, (int)filter, ptr4, ptr2);
			}
		}
	}

	public unsafe void VideoProcessorGetStreamExtension(VideoProcessor videoProcessorRef, int streamIndex, Guid extensionGuidRef, int dataSize, IntPtr dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, &extensionGuidRef, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void VideoProcessorBlt(VideoProcessor videoProcessorRef, VideoProcessorOutputView viewRef, int outputFrame, int streamCount, VideoProcessorStream[] streamsRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		VideoProcessorStream.__Native[] array = new VideoProcessorStream.__Native[streamsRef.Length];
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		zero2 = CppObject.ToCallbackPtr<VideoProcessorOutputView>(viewRef);
		for (int i = 0; i < streamsRef.Length; i++)
		{
			streamsRef[i].__MarshalTo(ref array[i]);
		}
		Result result;
		fixed (VideoProcessorStream.__Native* ptr = array)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, outputFrame, streamCount, ptr2);
		}
		for (int j = 0; j < streamsRef.Length; j++)
		{
			streamsRef[j].__MarshalFree(ref array[j]);
		}
		result.CheckError();
	}

	public unsafe void NegotiateCryptoSessionKeyExchange(CryptoSession cryptoSessionRef, int dataSize, IntPtr dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void EncryptionBlt(CryptoSession cryptoSessionRef, Texture2D srcSurfaceRef, Texture2D dstSurfaceRef, int iVSize, IntPtr iVRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		zero2 = CppObject.ToCallbackPtr<Texture2D>(srcSurfaceRef);
		zero3 = CppObject.ToCallbackPtr<Texture2D>(dstSurfaceRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, (void*)zero3, iVSize, (void*)iVRef);
	}

	public unsafe void DecryptionBlt(CryptoSession cryptoSessionRef, Texture2D srcSurfaceRef, Texture2D dstSurfaceRef, EncryptedBlockInformation? encryptedBlockInfoRef, int contentKeySize, IntPtr contentKeyRef, int iVSize, IntPtr iVRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		zero2 = CppObject.ToCallbackPtr<Texture2D>(srcSurfaceRef);
		zero3 = CppObject.ToCallbackPtr<Texture2D>(dstSurfaceRef);
		EncryptedBlockInformation value = default(EncryptedBlockInformation);
		if (encryptedBlockInfoRef.HasValue)
		{
			value = encryptedBlockInfoRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		void* intPtr2 = (void*)zero2;
		void* intPtr3 = (void*)zero3;
		EncryptedBlockInformation* intPtr4 = ((!encryptedBlockInfoRef.HasValue) ? null : (&value));
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, intPtr4, contentKeySize, (void*)contentKeyRef, iVSize, (void*)iVRef);
	}

	public unsafe void StartSessionKeyRefresh(CryptoSession cryptoSessionRef, int randomNumberSize, IntPtr randomNumberRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, randomNumberSize, (void*)randomNumberRef);
	}

	public unsafe void FinishSessionKeyRefresh(CryptoSession cryptoSessionRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void GetEncryptionBltKey(CryptoSession cryptoSessionRef, int keySize, IntPtr readbackKeyRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CryptoSession>(cryptoSessionRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)59 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, keySize, (void*)readbackKeyRef)).CheckError();
	}

	public unsafe void NegotiateAuthenticatedChannelKeyExchange(AuthenticatedChannel channelRef, int dataSize, IntPtr dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<AuthenticatedChannel>(channelRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)60 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void QueryAuthenticatedChannel(AuthenticatedChannel channelRef, int inputSize, IntPtr inputRef, int outputSize, IntPtr outputRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<AuthenticatedChannel>(channelRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)61 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, inputSize, (void*)inputRef, outputSize, (void*)outputRef)).CheckError();
	}

	public unsafe void ConfigureAuthenticatedChannel(AuthenticatedChannel channelRef, int inputSize, IntPtr inputRef, out AuthenticatedConfigureOutput outputRef)
	{
		IntPtr zero = IntPtr.Zero;
		AuthenticatedConfigureOutput.__Native @ref = default(AuthenticatedConfigureOutput.__Native);
		outputRef = default(AuthenticatedConfigureOutput);
		zero = CppObject.ToCallbackPtr<AuthenticatedChannel>(channelRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)62 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, inputSize, (void*)inputRef, &@ref);
		outputRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void VideoProcessorSetStreamRotation(VideoProcessor videoProcessorRef, int streamIndex, RawBool enable, VideoProcessorRotation rotation)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)63 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, enable, (int)rotation);
	}

	public unsafe void VideoProcessorGetStreamRotation(VideoProcessor videoProcessorRef, int streamIndex, out RawBool enableRef, out VideoProcessorRotation rotationRef)
	{
		IntPtr zero = IntPtr.Zero;
		enableRef = default(RawBool);
		zero = CppObject.ToCallbackPtr<VideoProcessor>(videoProcessorRef);
		fixed (VideoProcessorRotation* ptr = &rotationRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &enableRef)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, streamIndex, ptr4, ptr2);
			}
		}
	}
}
