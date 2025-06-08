using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class NTCreateAndXResponseExtended : SMBAndXCommand
{
	public const int ParametersLength = 100;

	public const int DeclaredParametersLength = 84;

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

	public ushort NMPipeStatus_or_FileStatusFlags;

	public bool Directory;

	public Guid VolumeGuid;

	public ulong FileID;

	public AccessMask MaximalAccessRights;

	public AccessMask GuestMaximalAccessRights;

	public NamedPipeStatus NMPipeStatus
	{
		get
		{
			return new NamedPipeStatus(NMPipeStatus_or_FileStatusFlags);
		}
		set
		{
			NMPipeStatus_or_FileStatusFlags = value.ToUInt16();
		}
	}

	public FileStatusFlags FileStatusFlags
	{
		get
		{
			return (FileStatusFlags)NMPipeStatus_or_FileStatusFlags;
		}
		set
		{
			NMPipeStatus_or_FileStatusFlags = (ushort)value;
		}
	}

	public override CommandName CommandName => CommandName.SMB_COM_NT_CREATE_ANDX;

	public NTCreateAndXResponseExtended()
	{
	}

	public NTCreateAndXResponseExtended(byte[] buffer, int offset)
		: base(buffer, offset, isUnicode: false)
	{
		int offset2 = 4;
		OpLockLevel = (OpLockLevel)ByteReader.ReadByte(SMBParameters, ref offset2);
		FID = LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		CreateDisposition = (CreateDisposition)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		CreateTime = FileTimeHelper.ReadNullableFileTime(SMBParameters, ref offset2);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(SMBParameters, ref offset2);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(SMBParameters, ref offset2);
		LastChangeTime = FileTimeHelper.ReadNullableFileTime(SMBParameters, ref offset2);
		ExtFileAttributes = (ExtendedFileAttributes)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		AllocationSize = LittleEndianReader.ReadInt64(SMBParameters, ref offset2);
		EndOfFile = LittleEndianReader.ReadInt64(SMBParameters, ref offset2);
		ResourceType = (ResourceType)LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		NMPipeStatus_or_FileStatusFlags = LittleEndianReader.ReadUInt16(SMBParameters, ref offset2);
		Directory = ByteReader.ReadByte(SMBParameters, ref offset2) > 0;
		VolumeGuid = LittleEndianReader.ReadGuid(SMBParameters, ref offset2);
		FileID = LittleEndianReader.ReadUInt64(SMBParameters, ref offset2);
		MaximalAccessRights = (AccessMask)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
		GuestMaximalAccessRights = (AccessMask)LittleEndianReader.ReadUInt32(SMBParameters, ref offset2);
	}

	public override byte[] GetBytes(bool isUnicode)
	{
		SMBParameters = new byte[100];
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
		LittleEndianWriter.WriteUInt16(SMBParameters, ref offset, NMPipeStatus_or_FileStatusFlags);
		ByteWriter.WriteByte(SMBParameters, ref offset, Convert.ToByte(Directory));
		LittleEndianWriter.WriteGuid(SMBParameters, ref offset, VolumeGuid);
		LittleEndianWriter.WriteUInt64(SMBParameters, ref offset, FileID);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)MaximalAccessRights);
		LittleEndianWriter.WriteUInt32(SMBParameters, ref offset, (uint)GuestMaximalAccessRights);
		return base.GetBytes(isUnicode);
	}
}
