using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct AuthenticatedQueryAccessibilityEncryptionGuidInput
{
	public AuthenticatedQueryInput Input;

	public int EncryptionGuidIndex;
}
