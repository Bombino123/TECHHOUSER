using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public abstract class SMBAndXCommand : SMB1Command
{
	public CommandName AndXCommand;

	public byte AndXReserved;

	public ushort AndXOffset;

	public SMBAndXCommand()
	{
	}

	public SMBAndXCommand(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		AndXCommand = (CommandName)ByteReader.ReadByte(SMBParameters, 0);
		AndXReserved = ByteReader.ReadByte(SMBParameters, 1);
		AndXOffset = LittleEndianConverter.ToUInt16(SMBParameters, 2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		ByteWriter.WriteByte(SMBParameters, 0, (byte)AndXCommand);
		ByteWriter.WriteByte(SMBParameters, 1, AndXReserved);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, AndXOffset);
		return base.GetBytes(isUnicode);
	}

	public static void WriteAndXOffset(byte[] buffer, int commandOffset, ushort AndXOffset)
	{
		LittleEndianWriter.WriteUInt16(buffer, commandOffset + 3, AndXOffset);
	}
}
