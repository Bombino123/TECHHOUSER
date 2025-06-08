using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class QueryInfoResponse : SMB2Command
{
	public const int FixedSize = 8;

	public const int DeclaredSize = 9;

	private ushort StructureSize;

	private ushort OutputBufferOffset;

	private uint OutputBufferLength;

	public byte[] OutputBuffer = new byte[0];

	public override int CommandLength => 8 + OutputBuffer.Length;

	public QueryInfoResponse()
		: base(SMB2CommandName.QueryInfo)
	{
		Header.IsResponse = true;
		StructureSize = 9;
	}

	public QueryInfoResponse(byte[] buffer, int offset)
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

	public FileInformation GetFileInformation(FileInformationClass informationClass)
	{
		return FileInformation.GetFileInformation(OutputBuffer, 0, informationClass);
	}

	public FileSystemInformation GetFileSystemInformation(FileSystemInformationClass informationClass)
	{
		return FileSystemInformation.GetFileSystemInformation(OutputBuffer, 0, informationClass);
	}

	public SecurityDescriptor GetSecurityInformation()
	{
		return new SecurityDescriptor(OutputBuffer, 0);
	}

	public void SetFileInformation(FileInformation fileInformation)
	{
		OutputBuffer = fileInformation.GetBytes();
	}

	public void SetFileSystemInformation(FileSystemInformation fileSystemInformation)
	{
		OutputBuffer = fileSystemInformation.GetBytes();
	}

	public void SetSecurityInformation(SecurityDescriptor securityDescriptor)
	{
		OutputBuffer = securityDescriptor.GetBytes();
	}
}
