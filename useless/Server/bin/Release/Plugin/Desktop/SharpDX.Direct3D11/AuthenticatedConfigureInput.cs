using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedConfigureInput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public MessageAuthenticationCode.__Native Omac;

		public Guid ConfigureType;

		public IntPtr HChannel;

		public int SequenceNumber;
	}

	public MessageAuthenticationCode Omac;

	public Guid ConfigureType;

	public IntPtr HChannel;

	public int SequenceNumber;

	internal void __MarshalFree(ref __Native @ref)
	{
		Omac.__MarshalFree(ref @ref.Omac);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Omac.__MarshalFrom(ref @ref.Omac);
		ConfigureType = @ref.ConfigureType;
		HChannel = @ref.HChannel;
		SequenceNumber = @ref.SequenceNumber;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Omac.__MarshalTo(ref @ref.Omac);
		@ref.ConfigureType = ConfigureType;
		@ref.HChannel = HChannel;
		@ref.SequenceNumber = SequenceNumber;
	}
}
