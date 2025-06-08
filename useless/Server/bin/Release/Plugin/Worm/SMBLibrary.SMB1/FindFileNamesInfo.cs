using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindFileNamesInfo : FindInformation
{
	public const int FixedLength = 12;

	public uint FileIndex;

	public string FileName;

	public override FindInformationLevel InformationLevel => FindInformationLevel.SMB_FIND_FILE_NAMES_INFO;

	public FindFileNamesInfo()
	{
	}

	public FindFileNamesInfo(byte[] buffer, int offset, bool isUnicode)
	{
		NextEntryOffset = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileIndex = LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint byteCount = LittleEndianReader.ReadUInt32(buffer, ref offset);
		FileName = SMB1Helper.ReadFixedLengthString(buffer, ref offset, isUnicode, (int)byteCount);
	}

	public override void WriteBytes(byte[] buffer, ref int offset, bool isUnicode)
	{
		uint value = (uint)(isUnicode ? (FileName.Length * 2) : FileName.Length);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, NextEntryOffset);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, FileIndex);
		LittleEndianWriter.WriteUInt32(buffer, ref offset, value);
		SMB1Helper.WriteSMBString(buffer, ref offset, isUnicode, FileName);
	}

	public override int GetLength(bool isUnicode)
	{
		int num = 12;
		if (isUnicode)
		{
			return num + (FileName.Length * 2 + 2);
		}
		return num + (FileName.Length + 1);
	}
}
