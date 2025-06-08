using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("29DA1D51-1321-4454-804B-F5FC9F861F0F")]
public class VideoDevice1 : VideoDevice
{
	public VideoDevice1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoDevice1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoDevice1(nativePtr);
		}
		return null;
	}

	public unsafe void GetCryptoSessionPrivateDataSize(Guid cryptoTypeRef, Guid? decoderProfileRef, Guid keyExchangeTypeRef, out int privateInputSizeRef, out int privateOutputSizeRef)
	{
		Guid value = default(Guid);
		if (decoderProfileRef.HasValue)
		{
			value = decoderProfileRef.Value;
		}
		Result result;
		fixed (int* ptr = &privateOutputSizeRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &privateInputSizeRef)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				Guid* num = &cryptoTypeRef;
				Guid* intPtr = ((!decoderProfileRef.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &keyExchangeTypeRef, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetVideoDecoderCaps(Guid decoderProfileRef, int sampleWidth, int sampleHeight, Rational frameRateRef, int bitRate, Guid? cryptoTypeRef, out int decoderCapsRef)
	{
		Guid value = default(Guid);
		if (cryptoTypeRef.HasValue)
		{
			value = cryptoTypeRef.Value;
		}
		Result result;
		fixed (int* ptr = &decoderCapsRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			Guid* num = &decoderProfileRef;
			Rational* num2 = &frameRateRef;
			Guid* intPtr = ((!cryptoTypeRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(nativePointer, num, sampleWidth, sampleHeight, num2, bitRate, intPtr, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CheckVideoDecoderDownsampling(ref VideoDecoderDescription inputDescRef, ColorSpaceType inputColorSpace, ref VideoDecoderConfig inputConfigRef, Rational frameRateRef, VideoSampleDescription outputDescRef, out RawBool supportedRef, out RawBool realTimeHintRef)
	{
		supportedRef = default(RawBool);
		realTimeHintRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &realTimeHintRef)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &supportedRef)
			{
				void* ptr4 = ptr3;
				fixed (VideoDecoderConfig* ptr5 = &inputConfigRef)
				{
					void* ptr6 = ptr5;
					fixed (VideoDecoderDescription* ptr7 = &inputDescRef)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, ptr8, (int)inputColorSpace, ptr6, &frameRateRef, &outputDescRef, ptr4, ptr2);
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void RecommendVideoDecoderDownsampleParameters(ref VideoDecoderDescription inputDescRef, ColorSpaceType inputColorSpace, ref VideoDecoderConfig inputConfigRef, Rational frameRateRef, out VideoSampleDescription recommendedOutputDescRef)
	{
		recommendedOutputDescRef = default(VideoSampleDescription);
		Result result;
		fixed (VideoSampleDescription* ptr = &recommendedOutputDescRef)
		{
			void* ptr2 = ptr;
			fixed (VideoDecoderConfig* ptr3 = &inputConfigRef)
			{
				void* ptr4 = ptr3;
				fixed (VideoDecoderDescription* ptr5 = &inputDescRef)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, ptr6, (int)inputColorSpace, ptr4, &frameRateRef, ptr2);
				}
			}
		}
		result.CheckError();
	}
}
