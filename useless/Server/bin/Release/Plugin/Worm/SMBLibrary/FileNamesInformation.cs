using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class FileNamesInformation : QueryDirectoryFileInformation
{
	public const int FixedLength = 12;

	private uint FileNameLength;

	public string FileName = string.Empty;

	public override FileInformationClass FileInformationClass => FileInformationClass.FileNamesInformation;

	public override int Length => 12 + FileName.Length * 2;

	public FileNamesInformation()
	{
	}

	public FileNamesInformation(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		FileNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 8);
		FileName = ByteReader.ReadUTF16String(buffer, offset + 12, (int)FileNameLength / 2);
	}

	public override void WriteBytes(byte[] buffer, int offset)
	{
		base.WriteBytes(buffer, offset);
		FileNameLength = (uint)(FileName.Length * 2);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, FileNameLength);
		ByteWriter.WriteUTF16String(buffer, offset + 12, FileName);
	}
}
