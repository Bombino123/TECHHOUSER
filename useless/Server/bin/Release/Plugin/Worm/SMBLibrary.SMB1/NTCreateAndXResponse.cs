using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTCreateAndXResponse : SMBAndXCommand
{
	public const int ParametersLength = 68;

	public OpLockLevel OpLockLevel;

	public ushort FID;

	public CreateDisposition CreateDisposition;

	public DateTime? CreateTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? LastChangeTime;

	public ExtendedFileAttributes ExtFileAttributes;

	public long AllocationSize;

	public long EndOfFile;

	public ResourceType ResourceType;

	public NamedPipeStatus NMPipeStatus;

	public bool Directory;

	public override CommandName CommandName => CommandName.SMB_COM_NT_CREATE_ANDX;

	public NTCreateAndXResponse()
	{
	}

	public NTCreateAndXResponse(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		int offset2 = 4;
		OpLockLevel = (OpLockLevel)ByteReader.ReadByte(SMBParameters, ref offset2);
		FID = LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		CreateDisposition = (CreateDisposition)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		CreateTime = SMB1Helper.ReadNullableFileTime(SMBParameters, ref offset2);
		LastAccessTime = SMB1Helper.ReadNullableFileTime(SMBParameters, ref offset2);
		LastWriteTime = SMB1Helper.ReadNullableFileTime(SMBParameters, ref offset2);
		LastChangeTime = SMB1Helper.ReadNullableFileTime(SMBParameters, ref offset2);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		AllocationSize = LittleEndianReader.ReadInt64(SMBParameters, ref offset2);
		EndOfFile = LittleEndianReader.ReadInt64(SMBParameters, ref offset2);
		ResourceType = (ResourceType)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		NMPipeStatus = NamedPipeStatus.Read(SMBParameters, ref offset2);
		Directory = ByteReader.ReadByte(SMBParameters, ref offset2) > 0;
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[68];
		int offset = 4;
		ByteWriter.WriteByte(SMBParameters, ref offset, (byte)OpLockLevel);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, FID);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)CreateDisposition);
		FileTimeHelper.WriteFileTime(SMBParameters, ref offset, CreateTime);
		FileTimeHelper.WriteFileTime(SMBParameters, ref offset, LastAccessTime);
		FileTimeHelper.WriteFileTime(SMBParameters, ref offset, LastWriteTime);
		FileTimeHelper.WriteFileTime(SMBParameters, ref offset, LastChangeTime);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)ExtFileAttributes);
		LittleEndianWriter.WriteInt64(SMBParameters, ref offset, AllocationSize);
		LittleEndianWriter.WriteInt64(SMBParameters, ref offset, EndOfFile);
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, (ushort)ResourceType);
		NMPipeStatus.WriteBytes(SMBParameters, ref offset);
		ByteWriter.WriteByte(SMBParameters, ref offset, Convert.ToByte(Directory));
		return base.GetBytes(isUnicode);
	}
}
