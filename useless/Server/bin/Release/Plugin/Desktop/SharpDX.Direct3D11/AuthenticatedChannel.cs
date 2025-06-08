using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("3015A308-DCBD-47aa-A747-192486D14D4A")]
public class AuthenticatedChannel : DeviceChild
{
	public int CertificateSize
	{
		get
		{
			GetCertificateSize(out var certificateSizeRef);
			return certificateSizeRef;
		}
	}

	public IntPtr ChannelHandle
	{
		get
		{
			GetChannelHandle(out var channelHandleRef);
			return channelHandleRef;
		}
	}

	public AuthenticatedChannel(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator AuthenticatedChannel(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new AuthenticatedChannel(nativePtr);
		}
		return null;
	}

	internal unsafe void GetCertificateSize(out int certificateSizeRef)
	{
		Result result;
		fixed (int* ptr = &certificateSizeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetCertificate(int certificateSize, byte[] certificateRef)
	{
		Result result;
		fixed (byte* ptr = certificateRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, certificateSize, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetChannelHandle(out IntPtr channelHandleRef)
	{
		fixed (IntPtr* ptr = &channelHandleRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
