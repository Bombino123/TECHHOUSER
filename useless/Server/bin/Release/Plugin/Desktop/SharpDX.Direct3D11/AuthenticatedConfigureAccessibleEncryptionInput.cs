using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedConfigureAccessibleEncryptionInput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedConfigureInput.__Native Parameters;

		public Guid EncryptionGuid;
	}

	public AuthenticatedConfigureInput Parameters;

	public Guid EncryptionGuid;

	internal void __MarshalFree(ref __Native @ref)
	{
		Parameters.__MarshalFree(ref @ref.Parameters);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Parameters.__MarshalFrom(ref @ref.Parameters);
		EncryptionGuid = @ref.EncryptionGuid;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Parameters.__MarshalTo(ref @ref.Parameters);
		@ref.EncryptionGuid = EncryptionGuid;
	}
}
