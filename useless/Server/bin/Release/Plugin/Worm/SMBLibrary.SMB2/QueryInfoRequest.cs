using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class QueryInfoRequest : SMB2Command
{
	public const int FixedSize = 40;

	public const int DeclaredSize = 41;

	private ushort StructureSize;

	public InfoType InfoType;

	private byte FileInfoClass;

	public uint OutputBufferLength;

	private ushort InputBufferOffset;

	public ushort Reserved;

	private uint InputBufferLength;

	public uint AdditionalInformation;

	public uint Flags;

	public FileID FileId;

	public byte[] InputBuffer = new byte[0];

	public FileInformationClass FileInformationClass
	{
		get
		{
			return (FileInformationClass)FileInfoClass;
		}
		set
		{
			FileInfoClass = (byte)value;
		}
	}

	public FileSystemInformationClass FileSystemInformationClass
	{
		get
		{
			return (FileSystemInformationClass)FileInfoClass;
		}
		set
		{
			FileInfoClass = (byte)value;
		}
	}

	public SecurityInformation SecurityInformation
	{
		get
		{
			return (SecurityInformation)AdditionalInformation;
		}
		set
		{
			AdditionalInformation = (uint)value;
		}
	}

	public override int CommandLength => 40 + InputBuffer.Length;

	public QueryInfoRequest()
		: base(SMB2CommandName.QueryInfo)
	{
		StructureSize = 41;
	}

	public QueryInfoRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		InfoType = (InfoType)ByteReader.ReadByte(buffer, offset + 64 + 2);
		FileInfoClass = ByteReader.ReadByte(buffer, offset + 64 + 3);
		OutputBufferLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		InputBufferOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 8);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 10);
		InputBufferLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 12);
		AdditionalInformation = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 16);
		Flags = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 20);
		FileId = new FileID(buffer, offset + 64 + 24);
		InputBuffer = ByteReader.ReadBytes(buffer, offset + InputBufferOffset, (int)InputBufferLength);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		InputBufferOffset = 0;
		InputBufferLength = (uint)InputBuffer.Length;
		if (InputBuffer.Length != 0)
		{
			InputBufferOffset = 104;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, (byte)InfoType);
		ByteWriter.WriteByte(buffer, offset + 3, FileInfoClass);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, OutputBufferLength);
		LittleEndianWriter.WriteUInt16(buffer, offset + 8, InputBufferOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 10, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 12, InputBufferLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 16, AdditionalInformation);
		LittleEndianWriter.WriteUInt32(buffer, offset + 20, Flags);
		FileId.WriteBytes(buffer, offset + 24);
		ByteWriter.WriteBytes(buffer, offset + 40, InputBuffer);
	}

	public void SetFileInformation(FileInformation fileInformation)
	{
		InputBuffer = fileInformation.GetBytes();
	}
}
