using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct EncryptedBlockInformation
{
	public int NumEncryptedBytesAtBeginning;

	public int NumBytesInSkipPattern;

	public int NumBytesInEncryptPattern;
}
