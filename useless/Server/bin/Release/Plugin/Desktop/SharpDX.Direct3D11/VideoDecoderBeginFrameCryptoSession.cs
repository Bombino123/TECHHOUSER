using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct VideoDecoderBeginFrameCryptoSession
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr PCryptoSession;

		public int BlobSize;

		public IntPtr PBlob;

		public IntPtr PKeyInfoId;

		public int PrivateDataSize;

		public IntPtr PPrivateData;
	}

	public CryptoSession PCryptoSession;

	public int BlobSize;

	public IntPtr PBlob;

	public IntPtr PKeyInfoId;

	public int PrivateDataSize;

	public IntPtr PPrivateData;

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		if (@ref.PCryptoSession != IntPtr.Zero)
		{
			PCryptoSession = new CryptoSession(@ref.PCryptoSession);
		}
		else
		{
			PCryptoSession = null;
		}
		BlobSize = @ref.BlobSize;
		PBlob = @ref.PBlob;
		PKeyInfoId = @ref.PKeyInfoId;
		PrivateDataSize = @ref.PrivateDataSize;
		PPrivateData = @ref.PPrivateData;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.PCryptoSession = CppObject.ToCallbackPtr<CryptoSession>(PCryptoSession);
		@ref.BlobSize = BlobSize;
		@ref.PBlob = PBlob;
		@ref.PKeyInfoId = PKeyInfoId;
		@ref.PrivateDataSize = PrivateDataSize;
		@ref.PPrivateData = PPrivateData;
	}
}
