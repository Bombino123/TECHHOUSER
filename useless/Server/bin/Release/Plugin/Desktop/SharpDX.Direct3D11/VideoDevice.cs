using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[Guid("10EC4D5B-975A-4689-B9E4-D0AAC30FE333")]
public class VideoDevice : ComObject
{
	public int VideoDecoderProfileCount => GetVideoDecoderProfileCount();

	public VideoDevice(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator VideoDevice(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new VideoDevice(nativePtr);
		}
		return null;
	}

	public unsafe void CreateVideoDecoder(ref VideoDecoderDescription videoDescRef, ref VideoDecoderConfig configRef, out VideoDecoder decoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (VideoDecoderConfig* ptr = &configRef)
		{
			void* ptr2 = ptr;
			fixed (VideoDecoderDescription* ptr3 = &videoDescRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		if (zero != IntPtr.Zero)
		{
			decoderOut = new VideoDecoder(zero);
		}
		else
		{
			decoderOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateVideoProcessor(VideoProcessorEnumerator enumRef, int rateConversionIndex, out VideoProcessor videoProcessorOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<VideoProcessorEnumerator>(enumRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, rateConversionIndex, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			videoProcessorOut = new VideoProcessor(zero2);
		}
		else
		{
			videoProcessorOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateAuthenticatedChannel(AuthenticatedChannelType channelType, out AuthenticatedChannel authenticatedChannelOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (int)channelType, &zero);
		if (zero != IntPtr.Zero)
		{
			authenticatedChannelOut = new AuthenticatedChannel(zero);
		}
		else
		{
			authenticatedChannelOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateCryptoSession(Guid cryptoTypeRef, Guid? decoderProfileRef, Guid keyExchangeTypeRef, out CryptoSession cryptoSessionOut)
	{
		IntPtr zero = IntPtr.Zero;
		Guid value = default(Guid);
		if (decoderProfileRef.HasValue)
		{
			value = decoderProfileRef.Value;
		}
		void* nativePointer = _nativePointer;
		Guid* num = &cryptoTypeRef;
		Guid* intPtr = ((!decoderProfileRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, num, intPtr, &keyExchangeTypeRef, &zero);
		if (zero != IntPtr.Zero)
		{
			cryptoSessionOut = new CryptoSession(zero);
		}
		else
		{
			cryptoSessionOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateVideoDecoderOutputView(Resource resourceRef, ref VideoDecoderOutputViewDescription descRef, out VideoDecoderOutputView vDOVViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		Result result;
		fixed (VideoDecoderOutputViewDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, &zero2);
		}
		if (zero2 != IntPtr.Zero)
		{
			vDOVViewOut = new VideoDecoderOutputView(zero2);
		}
		else
		{
			vDOVViewOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateVideoProcessorInputView(Resource resourceRef, VideoProcessorEnumerator enumRef, VideoProcessorInputViewDescription descRef, out VideoProcessorInputView vPIViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		zero2 = CppObject.ToCallbackPtr<VideoProcessorEnumerator>(enumRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, &descRef, &zero3);
		if (zero3 != IntPtr.Zero)
		{
			vPIViewOut = new VideoProcessorInputView(zero3);
		}
		else
		{
			vPIViewOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateVideoProcessorOutputView(Resource resourceRef, VideoProcessorEnumerator enumRef, VideoProcessorOutputViewDescription descRef, out VideoProcessorOutputView vPOViewOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Resource>(resourceRef);
		zero2 = CppObject.ToCallbackPtr<VideoProcessorEnumerator>(enumRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, &descRef, &zero3);
		if (zero3 != IntPtr.Zero)
		{
			vPOViewOut = new VideoProcessorOutputView(zero3);
		}
		else
		{
			vPOViewOut = null;
		}
		result.CheckError();
	}

	public unsafe void CreateVideoProcessorEnumerator(ref VideoProcessorContentDescription descRef, out VideoProcessorEnumerator enumOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (VideoProcessorContentDescription* ptr = &descRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			enumOut = new VideoProcessorEnumerator(zero);
		}
		else
		{
			enumOut = null;
		}
		result.CheckError();
	}

	internal unsafe int GetVideoDecoderProfileCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetVideoDecoderProfile(int index, out Guid decoderProfileRef)
	{
		decoderProfileRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &decoderProfileRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, index, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CheckVideoDecoderFormat(Guid decoderProfileRef, Format format, out RawBool supportedRef)
	{
		supportedRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &supportedRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &decoderProfileRef, (int)format, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetVideoDecoderConfigCount(ref VideoDecoderDescription descRef, out int countRef)
	{
		Result result;
		fixed (int* ptr = &countRef)
		{
			void* ptr2 = ptr;
			fixed (VideoDecoderDescription* ptr3 = &descRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetVideoDecoderConfig(ref VideoDecoderDescription descRef, int index, out VideoDecoderConfig configRef)
	{
		configRef = default(VideoDecoderConfig);
		Result result;
		fixed (VideoDecoderConfig* ptr = &configRef)
		{
			void* ptr2 = ptr;
			fixed (VideoDecoderDescription* ptr3 = &descRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, ptr4, index, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetContentProtectionCaps(Guid? cryptoTypeRef, Guid? decoderProfileRef, out VideoContentProtectionCaps capsRef)
	{
		capsRef = default(VideoContentProtectionCaps);
		Guid value = default(Guid);
		if (cryptoTypeRef.HasValue)
		{
			value = cryptoTypeRef.Value;
		}
		Guid value2 = default(Guid);
		if (decoderProfileRef.HasValue)
		{
			value2 = decoderProfileRef.Value;
		}
		Result result;
		fixed (VideoContentProtectionCaps* ptr = &capsRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			Guid* intPtr = ((!cryptoTypeRef.HasValue) ? null : (&value));
			Guid* intPtr2 = ((!decoderProfileRef.HasValue) ? null : (&value2));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CheckCryptoKeyExchange(Guid cryptoTypeRef, Guid? decoderProfileRef, int index, out Guid keyExchangeTypeRef)
	{
		keyExchangeTypeRef = default(Guid);
		Guid value = default(Guid);
		if (decoderProfileRef.HasValue)
		{
			value = decoderProfileRef.Value;
		}
		Result result;
		fixed (Guid* ptr = &keyExchangeTypeRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			Guid* num = &cryptoTypeRef;
			Guid* intPtr = ((!decoderProfileRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, num, intPtr, index, ptr2);
		}
		result.CheckError();
	}

	public unsafe void SetPrivateData(Guid guid, int dataSize, IntPtr dataRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, &guid, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void SetPrivateDataInterface(Guid guid, IUnknown dataRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(dataRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, &guid, (void*)zero)).CheckError();
	}
}
