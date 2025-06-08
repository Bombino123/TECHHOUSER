using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("9B32F9AD-BDCC-40a6-A39D-D5C865845720")]
public class CryptoSession : DeviceChild
{
	public Guid CryptoType
	{
		get
		{
			GetCryptoType(out var cryptoTypeRef);
			return cryptoTypeRef;
		}
	}

	public Guid DecoderProfile
	{
		get
		{
			GetDecoderProfile(out var decoderProfileRef);
			return decoderProfileRef;
		}
	}

	public int CertificateSize
	{
		get
		{
			GetCertificateSize(out var certificateSizeRef);
			return certificateSizeRef;
		}
	}

	public IntPtr CryptoSessionHandle
	{
		get
		{
			GetCryptoSessionHandle(out var cryptoSessionHandleRef);
			return cryptoSessionHandleRef;
		}
	}

	public CryptoSession(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CryptoSession(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CryptoSession(nativePtr);
		}
		return null;
	}

	internal unsafe void GetCryptoType(out Guid cryptoTypeRef)
	{
		cryptoTypeRef = default(Guid);
		fixed (Guid* ptr = &cryptoTypeRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetDecoderProfile(out Guid decoderProfileRef)
	{
		decoderProfileRef = default(Guid);
		fixed (Guid* ptr = &decoderProfileRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe void GetCertificateSize(out int certificateSizeRef)
	{
		Result result;
		fixed (int* ptr = &certificateSizeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetCertificate(int certificateSize, byte[] certificateRef)
	{
		Result result;
		fixed (byte* ptr = certificateRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, certificateSize, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetCryptoSessionHandle(out IntPtr cryptoSessionHandleRef)
	{
		fixed (IntPtr* ptr = &cryptoSessionHandleRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
