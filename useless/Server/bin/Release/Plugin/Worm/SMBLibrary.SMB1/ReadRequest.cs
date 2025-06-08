using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ReadRequest : SMB1Command
{
	public const int ParametersLength = 10;

	public ushort FID;

	public ushort CountOfBytesToRead;

	public uint ReadOffsetInBytes;

	public ushort EstimateOfRemainingBytesToBeRead;

	public override CommandName CommandName => CommandName.SMB_COM_READ;

	public ReadRequest()
	{
	}

	public ReadRequest(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		FID = LittleEndianConverter.ToUInt16(SMBParameters, 0);
		CountOfBytesToRead = LittleEndianConverter.ToUInt16(SMBParameters, 2);
		ReadOffsetInBytes = LittleEndianConverter.ToUInt32(SMBParameters, 4);
		CountOfBytesToRead = LittleEndianConverter.ToUInt16(SMBParameters, 8);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[10];
		LittleEndianWriter.WriteUInt16(SMBParameters, 0, FID);
		LittleEndianWriter.WriteUInt16(SMBParameters, 2, CountOfBytesToRead);
		LittleEndianWriter.WriteUInt32(SMBParameters, 4, ReadOffsetInBytes);
		LittleEndianWriter.WriteUInt16(SMBParameters, 8, CountOfBytesToRead);
		return base.GetBytes(isUnicode);
	}
}
