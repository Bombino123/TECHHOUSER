using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileNameInformation : FileInformation
{
	public const int FixedLength = 4;

	private uint FileNameLength;

	public string FileName = string.Empty;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileNameInformation;

	public override int Length => 4 + FileName.Length * 2;

	public FileNameInformation()
	{
	}

	public FileNameInformation(byte[] buffer, int offset)
	{
		FileNameLength = LittleEndianConverter.ToUInt32(buffer, offset);
		FileName = ByteReader.ReadUTF16String(buffer, offset + 4, (int)FileNameLength / 2);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		FileNameLength = (uint)(FileName.Length * 2);
		LittleEndianWriter.WriteUInt32(buffer, offset, FileNameLength);
		ByteWriter.WriteUTF16String(buffer, offset + 4, FileName);
	}
}
