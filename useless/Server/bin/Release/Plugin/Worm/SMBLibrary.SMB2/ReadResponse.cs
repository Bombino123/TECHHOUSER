using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class ReadResponse : SMB2Command
{
	public const int FixedSize = 16;

	public const int DeclaredSize = 17;

	private ushort StructureSize;

	private byte DataOffset;

	public byte Reserved;

	private uint DataLength;

	public uint DataRemaining;

	public uint Reserved2;

	public byte[] Data = new byte[0];

	public override int CommandLength => 16 + Data.Length;

	public ReadResponse()
		: base(SMB2CommandName.Read)
	{
		Header.IsResponse = true;
		StructureSize = 17;
	}

	public ReadResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		DataOffset = ByteReader.ReadByte(buffer, offset + 64 + 2);
		Reserved = ByteReader.ReadByte(buffer, offset + 64 + 3);
		DataLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		DataRemaining = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 8);
		Reserved2 = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 12);
		if (DataLength != 0)
		{
			Data = ByteReader.ReadBytes(buffer, offset + DataOffset, (int)DataLength);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		DataOffset = 0;
		DataLength = (uint)Data.Length;
		if (Data.Length != 0)
		{
			DataOffset = 80;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, DataOffset);
		ByteWriter.WriteByte(buffer, offset + 3, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, DataLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, DataRemaining);
		LittleEndianWriter.WriteUInt32(buffer, offset + 12, Reserved2);
		if (Data.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 16, Data);
		}
	}
}
