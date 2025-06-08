using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class OpenAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 30;

	public ushort FID;

	public SMBFileAttributes FileAttrs;

	public DateTime? LastWriteTime;

	public uint FileDataSize;

	public AccessRights AccessRights;

	public ResourceType ResourceType;

	public NamedPipeStatus NMPipeStatus;

	public OpenResults OpenResults;

	public byte[] Reserved;

	public override CommandName CommandName => CommandName.SMB_COM_OPEN_ANDX;

	public OpenAndXResponse()
	{
		Reserved = new byte[6];
	}

	public OpenAndXResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		int offset2 = 4;
		FID = LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		FileAttrs = (SMBFileAttributes)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		LastWriteTime = UTimeHelper.ReadNullableUTime(SMBParameters, ref offset2);
		FileDataSize = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		AccessRights = (AccessRights)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		ResourceType = (ResourceType)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		NMPipeStatus = NamedPipeStatus.Read(SMBParameters, ref offset2);
		OpenResults = OpenResults.Read(SMBParameters, ref offset2);
		Reserved = ByteReader.ReadBytes(SMBParameters, ref offset2, 6);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[30];
		int offset = 4;
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, FID);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)FileAttrs);
		UTimeHelper.WriteUTime(SMBParameters, ref offset, LastWriteTime);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, FileDataSize);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)AccessRights);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)ResourceType);
		NMPipeStatus.WriteBytes(SMBParameters, ref offset);
		OpenResults.WriteBytes(SMBParameters, ref offset);
		ByteWriter.WriteBytes(SMBParameters, ref offset, Reserved, 6);
		return base.GetBytes(isUnicode);
	}
}
