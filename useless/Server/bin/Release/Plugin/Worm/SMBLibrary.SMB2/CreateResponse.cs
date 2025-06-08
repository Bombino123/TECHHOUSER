using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class CreateResponse : SMB2Command
{
	public const int DeclaredSize = 89;

	private ushort StructureSize;

	public OplockLevel OplockLevel;

	public CreateResponseFlags Flags;

	public CreateAction CreateAction;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? ChangeTime;

	public long AllocationSize;

	public long EndofFile;

	public FileAttributes FileAttributes;

	public uint Reserved2;

	public FileID FileId;

	private uint CreateContextsOffsets;

	private uint CreateContextsLength;

	public List<CreateContext> CreateContexts = new List<CreateContext>();

	public override int CommandLength => 88 + CreateContext.GetCreateContextListLength(CreateContexts);

	public CreateResponse()
		: base(SMB2CommandName.Create)
	{
		Header.IsResponse = true;
		StructureSize = 89;
	}

	public CreateResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		OplockLevel = (OplockLevel)ByteReader.ReadByte(buffer, offset + 64 + 2);
		Flags = (CreateResponseFlags)ByteReader.ReadByte(buffer, offset + 64 + 3);
		CreateAction = (CreateAction)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 8);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 16);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 24);
		ChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 32);
		AllocationSize = LittleEndianConverter.ToInt64(buffer, offset + 64 + 40);
		EndofFile = LittleEndianConverter.ToInt64(buffer, offset + 64 + 48);
		FileAttributes = (FileAttributes)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 56);
		Reserved2 = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 60);
		FileId = new FileID(buffer, offset + 64 + 64);
		CreateContextsOffsets = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 80);
		CreateContextsLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 84);
		if (CreateContextsLength != 0)
		{
			CreateContexts = CreateContext.ReadCreateContextList(buffer, offset + (int)CreateContextsOffsets);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, (byte)OplockLevel);
		ByteWriter.WriteByte(buffer, offset + 3, (byte)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)CreateAction);
		FileTimeHelper.WriteFileTime(buffer, offset + 8, CreationTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 16, LastAccessTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 24, LastWriteTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 32, ChangeTime);
		LittleEndianWriter.WriteInt64(buffer, offset + 40, AllocationSize);
		LittleEndianWriter.WriteInt64(buffer, offset + 48, EndofFile);
		LittleEndianWriter.WriteUInt32(buffer, offset + 56, (uint)FileAttributes);
		LittleEndianWriter.WriteUInt32(buffer, offset + 60, Reserved2);
		FileId.WriteBytes(buffer, offset + 64);
		CreateContextsOffsets = 0u;
		CreateContextsLength = (uint)CreateContext.GetCreateContextListLength(CreateContexts);
		if (CreateContexts.Count > 0)
		{
			CreateContextsOffsets = 152u;
			CreateContext.WriteCreateContextList(buffer, 88, CreateContexts);
		}
	}
}
