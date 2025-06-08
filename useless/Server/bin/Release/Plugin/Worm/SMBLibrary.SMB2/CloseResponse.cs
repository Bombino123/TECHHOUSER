using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class CloseResponse : SMB2Command
{
	public const int DeclaredSize = 60;

	private ushort StructureSize;

	public CloseFlags Flags;

	public uint Reserved;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? ChangeTime;

	public long AllocationSize;

	public long EndofFile;

	public FileAttributes FileAttributes;

	public override int CommandLength => 60;

	public CloseResponse()
		: base(SMB2CommandName.Close)
	{
		Header.IsResponse = true;
		StructureSize = 60;
	}

	public CloseResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Flags = (CloseFlags)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 8);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 16);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 24);
		ChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 64 + 32);
		AllocationSize = LittleEndianConverter.ToInt64(buffer, offset + 64 + 40);
		EndofFile = LittleEndianConverter.ToInt64(buffer, offset + 64 + 48);
		FileAttributes = (FileAttributes)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 56);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, Reserved);
		FileTimeHelper.WriteFileTime(buffer, offset + 8, CreationTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 16, LastAccessTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 24, LastWriteTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 32, ChangeTime);
		LittleEndianWriter.WriteInt64(buffer, offset + 40, AllocationSize);
		LittleEndianWriter.WriteInt64(buffer, offset + 48, EndofFile);
		LittleEndianWriter.WriteUInt32(buffer, offset + 56, (uint)FileAttributes);
	}
}
