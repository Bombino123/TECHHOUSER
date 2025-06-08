using System;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsObjectIdInformation : FileSystemInformation
{
	public const int FixedLength = 64;

	public Guid ObjectID;

	public byte[] ExtendedInfo;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsObjectIdInformation;

	public override int Length => 64;

	public FileFsObjectIdInformation()
	{
		ExtendedInfo = new byte[48];
	}

	public FileFsObjectIdInformation(byte[] buffer, int offset)
	{
		LittleEndianConverter.ToGuid(buffer, offset);
		ExtendedInfo = ByteReader.ReadBytes(buffer, offset + 16, 48);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteGuid(buffer, offset, ObjectID);
		ByteWriter.WriteBytes(buffer, offset + 16, ExtendedInfo);
	}
}
