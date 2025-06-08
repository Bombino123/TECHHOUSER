using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedConfigureCryptoSessionInput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedConfigureInput.__Native Parameters;

		public IntPtr DecoderHandle;

		public IntPtr CryptoSessionHandle;

		public IntPtr DeviceHandle;
	}

	public AuthenticatedConfigureInput Parameters;

	public IntPtr DecoderHandle;

	public IntPtr CryptoSessionHandle;

	public IntPtr DeviceHandle;

	internal void __MarshalFree(ref __Native @ref)
	{
		Parameters.__MarshalFree(ref @ref.Parameters);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Parameters.__MarshalFrom(ref @ref.Parameters);
		DecoderHandle = @ref.DecoderHandle;
		CryptoSessionHandle = @ref.CryptoSessionHandle;
		DeviceHandle = @ref.DeviceHandle;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Parameters.__MarshalTo(ref @ref.Parameters);
		@ref.DecoderHandle = DecoderHandle;
		@ref.CryptoSessionHandle = CryptoSessionHandle;
		@ref.DeviceHandle = DeviceHandle;
	}
}
