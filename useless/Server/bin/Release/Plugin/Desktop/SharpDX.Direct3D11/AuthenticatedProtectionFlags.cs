using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct AuthenticatedProtectionFlags
{
	[FieldOffset(0)]
	public AuthenticatedProtectionFlagsMidlMidlItfD3d11000000340001Inner Flags;

	[FieldOffset(0)]
	public int Value;
}
