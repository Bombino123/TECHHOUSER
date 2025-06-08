using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryAccessibilityEncryptionGuidOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public int EncryptionGuidIndex;

		public Guid EncryptionGuid;
	}

	public AuthenticatedQueryOutput Output;

	public int EncryptionGuidIndex;

	public Guid EncryptionGuid;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		EncryptionGuidIndex = @ref.EncryptionGuidIndex;
		EncryptionGuid = @ref.EncryptionGuid;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.EncryptionGuidIndex = EncryptionGuidIndex;
		@ref.EncryptionGuid = EncryptionGuid;
	}
}
