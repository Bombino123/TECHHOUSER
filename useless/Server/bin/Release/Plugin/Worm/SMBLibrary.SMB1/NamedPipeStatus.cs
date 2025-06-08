using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public struct NamedPipeStatus
{
	public const int Length = 2;

	public byte ICount;

	public ReadMode ReadMode;

	public NamedPipeType NamedPipeType;

	public Endpoint Endpoint;

	public NonBlocking NonBlocking;

	public NamedPipeStatus(byte[] buffer, int offset)
	{
		ICount = buffer[offset];
		ReadMode = (ReadMode)(buffer[offset + 1] & 3u);
		NamedPipeType = (NamedPipeType)((buffer[offset + 1] & 0xC) >> 2);
		Endpoint = (Endpoint)((buffer[offset + 1] & 0x40) >> 6);
		NonBlocking = (NonBlocking)((buffer[offset + 1] & 0x80) >> 7);
	}

	public NamedPipeStatus(ushort value)
	{
		ICount = (byte)(value & 0xFFu);
		ReadMode = (ReadMode)((value & 0x300) >> 8);
		NamedPipeType = (NamedPipeType)((value & 0xC00) >> 10);
		Endpoint = (Endpoint)((value & 0x4000) >> 14);
		NonBlocking = (NonBlocking)((value & 0x80) >> 15);
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		buffer[offset] = ICount;
		buffer[offset + 1] = (byte)(ReadMode & (ReadMode)3);
		buffer[offset + 1] |= (byte)(((uint)NamedPipeType << 2) & 0xC);
		buffer[offset + 1] |= (byte)(((uint)Endpoint << 6) & 0x40);
		buffer[offset + 1] |= (byte)(((uint)NonBlocking << 7) & 0x80);
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += 2;
	}

	public ushort ToUInt16()
	{
		return (ushort)((ushort)((ushort)((ushort)(ICount | (ushort)(((uint)ReadMode << 8) & 0x300u)) | (ushort)(((uint)NamedPipeType << 10) & 0xC00u)) | (ushort)(((uint)Endpoint << 14) & 0x4000u)) | (ushort)(((uint)NonBlocking << 15) & 0x8000u));
	}

	public static NamedPipeStatus Read(byte[] buffer, ref int offset)
	{
		offset += 2;
		return new NamedPipeStatus(buffer, offset - 2);
	}
}
