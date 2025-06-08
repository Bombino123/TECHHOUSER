using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoDecoderBufferDescription1
{
	public VideoDecoderBufferType BufferType;

	public int DataOffset;

	public int DataSize;

	public IntPtr PIV;

	public int IVSize;

	public IntPtr PSubSampleMappingBlock;

	public int SubSampleMappingCount;
}
