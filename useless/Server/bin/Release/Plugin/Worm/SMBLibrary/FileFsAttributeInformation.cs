using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileFsAttributeInformation : FileSystemInformation
{
	public const int FixedLength = 12;

	public FileSystemAttributes FileSystemAttributes;

	public uint MaximumComponentNameLength;

	private uint FileSystemNameLength;

	public string FileSystemName = string.Empty;

	public override FileSystemInformationClass FileSystemInformationClass => FileSystemInformationClass.FileFsAttributeInformation;

	public override int Length => 12 + FileSystemName.Length * 2;

	public FileFsAttributeInformation()
	{
	}

	public FileFsAttributeInformation(byte[] buffer, int offset)
	{
		FileSystemAttributes = (FileSystemAttributes)LittleEndianConverter.ToUInt32(buffer, offset);
		MaximumComponentNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 4);
		FileSystemNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		FileSystemName = ByteReader.ReadUTF16String(buffer, offset + 12, (int)FileSystemNameLength / 2);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		FileSystemNameLength = (uint)(FileSystemName.Length * 2);
		LittleEndianWriter.WriteUInt32(buffer, offset, (uint)FileSystemAttributes);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, MaximumComponentNameLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, FileSystemNameLength);
		ByteWriter.WriteUTF16String(buffer, offset + 12, FileSystemName);
	}
}
