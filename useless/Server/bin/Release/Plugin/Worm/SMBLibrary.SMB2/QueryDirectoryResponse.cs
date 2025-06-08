using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class QueryDirectoryResponse : SMB2Command
{
	public const int FixedLength = 8;

	public const int DeclaredSize = 9;

	private ushort StructureSize;

	private ushort OutputBufferOffset;

	private uint OutputBufferLength;

	public byte[] OutputBuffer = new byte[0];

	public override int CommandLength => 8 + OutputBuffer.Length;

	public QueryDirectoryResponse()
		: base(SMB2CommandName.QueryDirectory)
	{
		Header.IsResponse = true;
		StructureSize = 9;
	}

	public QueryDirectoryResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		OutputBufferOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		OutputBufferLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		OutputBuffer = ByteReader.ReadBytes(buffer, offset + OutputBufferOffset, (int)OutputBufferLength);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		OutputBufferOffset = 0;
		OutputBufferLength = (uint)OutputBuffer.Length;
		if (OutputBuffer.Length != 0)
		{
			OutputBufferOffset = 72;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, OutputBufferOffset);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, OutputBufferLength);
		ByteWriter.WriteBytes(buffer, offset + 8, OutputBuffer);
	}

	public List<QueryDirectoryFileInformation> GetFileInformationList(FileInformationClass fileInformationClass)
	{
		if (OutputBuffer.Length != 0)
		{
			return QueryDirectoryFileInformation.ReadFileInformationList(OutputBuffer, 0, fileInformationClass);
		}
		return new List<QueryDirectoryFileInformation>();
	}

	public void SetFileInformationList(List<QueryDirectoryFileInformation> fileInformationList)
	{
		OutputBuffer = QueryDirectoryFileInformation.GetBytes(fileInformationList);
	}
}
