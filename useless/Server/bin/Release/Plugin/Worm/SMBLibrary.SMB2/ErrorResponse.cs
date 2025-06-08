using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class ErrorResponse : SMB2Command
{
	public const int FixedSize = 8;

	public const int DeclaredSize = 9;

	private ushort StructureSize;

	public byte ErrorContextCount;

	public byte Reserved;

	private uint ByteCount;

	public byte[] ErrorData = new byte[0];

	public override int CommandLength => 8 + Math.Max(ErrorData.Length, 1);

	public ErrorResponse(SMB2CommandName commandName)
		: base(commandName)
	{
		Header.IsResponse = true;
		StructureSize = 9;
	}

	public ErrorResponse(SMB2CommandName commandName, NTStatus status)
		: base(commandName)
	{
		Header.IsResponse = true;
		StructureSize = 9;
		Header.Status = status;
	}

	public ErrorResponse(SMB2CommandName commandName, NTStatus status, byte[] errorData)
		: base(commandName)
	{
		Header.IsResponse = true;
		StructureSize = 9;
		Header.Status = status;
		ErrorData = errorData;
	}

	public ErrorResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		ErrorContextCount = ByteReader.ReadByte(buffer, offset + 64 + 2);
		Reserved = ByteReader.ReadByte(buffer, offset + 64 + 3);
		ByteCount = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		ErrorData = ByteReader.ReadBytes(buffer, offset + 64 + 8, (int)ByteCount);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		ByteCount = (uint)ErrorData.Length;
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, ErrorContextCount);
		ByteWriter.WriteByte(buffer, offset + 3, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, ByteCount);
		if (ErrorData.Length != 0)
		{
			ByteWriter.WriteBytes(buffer, offset + 8, ErrorData);
		}
		else
		{
			ByteWriter.WriteBytes(buffer, offset + 8, new byte[1]);
		}
	}
}
