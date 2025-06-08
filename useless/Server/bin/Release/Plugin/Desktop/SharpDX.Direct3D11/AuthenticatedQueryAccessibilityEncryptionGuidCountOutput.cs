using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct AuthenticatedQueryAccessibilityEncryptionGuidCountOutput
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public AuthenticatedQueryOutput.__Native Output;

		public int EncryptionGuidCount;
	}

	public AuthenticatedQueryOutput Output;

	public int EncryptionGuidCount;

	internal void __MarshalFree(ref __Native @ref)
	{
		Output.__MarshalFree(ref @ref.Output);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Output.__MarshalFrom(ref @ref.Output);
		EncryptionGuidCount = @ref.EncryptionGuidCount;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		Output.__MarshalTo(ref @ref.Output);
		@ref.EncryptionGuidCount = EncryptionGuidCount;
	}
}
