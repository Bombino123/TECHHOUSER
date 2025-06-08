using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ReadAndXRequest : SMBAndXCommand
{
	public const int ParametersFixedLength = 20;

	public ushort FID;

	public ulong Offset;

	private ushort MaxCountOfBytesToReturn;

	public ushort MinCountOfBytesToReturn;

	public uint Timeout_or_MaxCountHigh;

	public ushort Remaining;

	public uint MaxCountLarge
	{
		get
		{
			return (uint)(((ushort)(Timeout_or_MaxCountHigh & 0xFFFF) << 16) | MaxCountOfBytesToReturn);
		}
		set
		{
			MaxCountOfBytesToReturn = (ushort)(value & 0xFFFFu);
			Timeout_or_MaxCountHigh = (ushort)(value >> 16);
		}
	}

	public ushort MaxCount
	{
		get
		{
			return MaxCountOfBytesToReturn;
		}
		set
		{
			MaxCountOfBytesToReturn = value;
		}
	}

	public override CommandName CommandName => CommandName.SMB_COM_READ_ANDX;

	public ReadAndXRequest()
	{
	}

	public ReadAndXRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		Offset = LittleEndianConverter.ToUInt32(SMBParameters, 6);
		MaxCountOfBytesToReturn = LittleEndianConverter.ToUInt16(SMBParameters, 10);
		MinCountOfBytesToReturn = LittleEndianConverter.ToUInt16(SMBParameters, 12);
		Timeout_or_MaxCountHigh = LittleEndianConverter.ToUInt32(SMBParameters, 14);
		Remaining = LittleEndianConverter.ToUInt16(SMBParameters, 18);
		if (SMBParameters.Length == 24)
		{
			uint num = LittleEndianConverter.ToUInt32(SMBParameters, 20);
			Offset |= (ulong)num << 32;
		}
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		int num = 20;
		if (Offset > uint.MaxValue)
		{
			num += 4;
		}
		SMBParameters = new byte[num];
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, FID);
		LittleEndianWriter.WriteUInt32(SMBParameters, 6, (uint)(Offset & 0xFFFFFFFFu));
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, (ushort)(MaxCountOfBytesToReturn & 0xFFFFu));
		LittleEndianWriter.WriteUInt16(SMBParameters, 12, MinCountOfBytesToReturn);
		LittleEndianWriter.WriteUInt32(SMBParameters, 14, Timeout_or_MaxCountHigh);
		LittleEndianWriter.WriteUInt16(SMBParameters, 18, Remaining);
		if (Offset > uint.MaxValue)
		{
			uint value = (uint)(Offset >> 32);
			LittleEndianWriter.WriteUInt32(SMBParameters, 20, value);
		}
		return base.GetBytes(isUnicode);
	}
}
