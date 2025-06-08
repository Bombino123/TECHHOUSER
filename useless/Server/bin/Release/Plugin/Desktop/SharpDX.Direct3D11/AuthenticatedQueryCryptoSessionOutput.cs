using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryCryptoSessionOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public IntPtr DecoderHandle;

		public IntPtr CryptoSessionHandle;

		public IntPtr DeviceHandle;
	}

	public AuthenticatedQueryOutput Output;

	public IntPtr DecoderHandle;

	public IntPtr CryptoSessionHandle;

	public IntPtr DeviceHandle;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		DecoderHandle = @ref.DecoderHandle;
		CryptoSessionHandle = @ref.CryptoSessionHandle;
		DeviceHandle = @ref.DeviceHandle;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.DecoderHandle = DecoderHandle;
		@ref.CryptoSessionHandle = CryptoSessionHandle;
		@ref.DeviceHandle = DeviceHandle;
	}
}
