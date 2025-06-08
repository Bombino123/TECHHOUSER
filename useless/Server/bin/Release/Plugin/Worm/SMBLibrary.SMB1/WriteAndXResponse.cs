using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class WriteAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 12;

	public uint Count;

	public ushort Available;

	public ushort Reserved;

	public override CommandName CommandName => CommandName.SMB_COM_WRITE_ANDX;

	public WriteAndXResponse()
	{
	}

	public WriteAndXResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		Count = LittleEndianConverter.ToUInt16(SMBParameters, 4);
		Available = LittleEndianConverter.ToUInt16(SMBParameters, 6);
		ushort num = LittleEndianConverter.ToUInt16(SMBParameters, 8);
		Reserved = LittleEndianConverter.ToUInt16(SMBParameters, 10);
		Count |= (uint)(num << 16);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[12];
		ushort value = (ushort)(Count >> 16);
		LittleEndianWriter.WriteUInt16(SMBParameters, 4, (ushort)(Count & 0xFFFFu));
		LittleEndianWriter.WriteUInt16(SMBParameters, 6, Available);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, value);
		LittleEndianWriter.WriteUInt16(SMBParameters, 10, Reserved);
		return base.GetBytes(isUnicode);
	}
}
