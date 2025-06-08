using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DXGI;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct OutputDuplicateFrameInformation
{
	public long LastPresentTime;

	public long LastMouseUpdateTime;

	public int AccumulatedFrames;

	public RawBool RectsCoalesced;

	public RawBool ProtectedContentMaskedOut;

	public OutputDuplicatePointerPosition PointerPosition;

	public int TotalMetadataBufferSize;

	public int PointerShapeBufferSize;
}
