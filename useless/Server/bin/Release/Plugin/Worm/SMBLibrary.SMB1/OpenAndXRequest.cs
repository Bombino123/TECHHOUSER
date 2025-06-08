using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class OpenAndXRequest : SMBAndXCommand
{
	public const int ParametersLength = 30;

	public OpenFlags Flags;

	public AccessModeOptions AccessMode;

	public SMBFileAttributes SearchAttrs;

	public SMBFileAttributes FileAttrs;

	public DateTime? CreationTime;

	public OpenMode OpenMode;

	public uint AllocationSize;

	public uint Timeout;

	public uint Reserved;

	public string FileName;

	public override CommandName CommandName => CommandName.SMB_COM_OPEN_ANDX;

	public OpenAndXRequest()
	{
	}

	public OpenAndXRequest(byte[] buffer, int offset, bool isUnicode)
		: base(buffer, offset, isUnicode)
	{
		int offset2 = 4;
		Flags = (OpenFlags)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		AccessMode = AccessModeOptions.Read(SMBParameters, ref offset2);
		SearchAttrs = (SMBFileAttributes)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		FileAttrs = (SMBFileAttributes)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		CreationTime = UTimeHelper.ReadNullableUTime(SMBParameters, ref offset2);
		OpenMode = OpenMode.Read(SMBParameters, ref offset2);
		AllocationSize = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		Timeout = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		Reserved = LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		int offset3 = 0;
		if (isUnicode)
		{
			offset3 = 1;
		}
		FileName = SMB1Helper.ReadSMBString(SMBData, offset3, isUnicode);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[30];
		int offset = 4;
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)Flags);
		AccessMode.WriteBytes(SMBParameters, ref offset);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)SearchAttrs);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)FileAttrs);
		UTimeHelper.WriteUTime(SMBParameters, ref offset, CreationTime);
		OpenMode.WriteBytes(SMBParameters, ref offset);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, AllocationSize);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, Timeout);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, Reserved);
		int num = 0;
		if (isUnicode)
		{
			num = 1;
			SMBData = new byte[num + FileName.Length * 2 + 2];
		}
		else
		{
			SMBData = new byte[FileName.Length + 1];
		}
		SMB1Helper.WriteSMBString(SMBData, num, isUnicode, FileName);
		return base.GetBytes(isUnicode);
	}
}
