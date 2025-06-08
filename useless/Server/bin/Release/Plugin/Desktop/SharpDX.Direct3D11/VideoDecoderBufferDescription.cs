using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct VideoDecoderBufferDescription
{
	public VideoDecoderBufferType BufferType;

	public int BufferIndex;

	public int DataOffset;

	public int DataSize;

	public int FirstMBaddress;

	public int NumMBsInBuffer;

	public int Width;

	public int Height;

	public int Stride;

	public int ReservedBits;

	public IntPtr PIV;

	public int IVSize;

	public RawBool PartialEncryption;

	public EncryptedBlockInformation EncryptedBlockInfo;
}
