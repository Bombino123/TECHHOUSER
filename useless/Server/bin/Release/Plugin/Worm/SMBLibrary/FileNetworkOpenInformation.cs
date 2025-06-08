using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileNetworkOpenInformation : FileInformation
{
	public const int FixedLength = 56;

	public DateTime? CreationTime;

	public DateTime? LastAccessTime;

	public DateTime? LastWriteTime;

	public DateTime? ChangeTime;

	public long AllocationSize;

	public long EndOfFile;

	public FileAttributes FileAttributes;

	public uint Reserved;

	public bool IsDirectory
	{
		get
		{
			return (FileAttributes & FileAttributes.Directory) != 0;
		}
		set
		{
			if (value)
			{
				FileAttributes |= FileAttributes.Directory;
			}
			else
			{
				FileAttributes &= ~FileAttributes.Directory;
			}
		}
	}

	public override FileInformationClass FileInformationClass => FileInformationClass.FileNetworkOpenInformation;

	public override int Length => 56;

	public FileNetworkOpenInformation()
	{
	}

	public FileNetworkOpenInformation(byte[] buffer, int offset)
	{
		CreationTime = FileTimeHelper.ReadNullableFileTime(buffer, offset);
		LastAccessTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 8);
		LastWriteTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 16);
		ChangeTime = FileTimeHelper.ReadNullableFileTime(buffer, offset + 24);
		AllocationSize = LittleEndianConverter.ToInt64(buffer, offset + 32);
		EndOfFile = LittleEndianConverter.ToInt64(buffer, offset + 40);
		FileAttributes = (FileAttributes)LittleEndianConverter.ToUInt32(buffer, offset + 48);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 52);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		FileTimeHelper.WriteFileTime(buffer, offset, CreationTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 8, LastAccessTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 16, LastWriteTime);
		FileTimeHelper.WriteFileTime(buffer, offset + 24, ChangeTime);
		LittleEndianWriter.WriteInt64(buffer, offset + 32, AllocationSize);
		LittleEndianWriter.WriteInt64(buffer, offset + 40, EndOfFile);
		LittleEndianWriter.WriteUInt32(buffer, offset + 48, (uint)FileAttributes);
		LittleEndianWriter.WriteUInt32(buffer, offset + 52, Reserved);
	}
}
