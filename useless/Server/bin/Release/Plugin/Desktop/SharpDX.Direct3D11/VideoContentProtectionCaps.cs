using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoContentProtectionCaps
{
	public int Caps;

	public int KeyExchangeTypeCount;

	public int BlockAlignmentSize;

	public long ProtectedMemorySize;
}
