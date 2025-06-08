using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class SetInfoRequest : SMB2Command
{
	public const int FixedSize = 32;

	public const int DeclaredSize = 33;

	private ushort StructureSize;

	public InfoType InfoType;

	private byte FileInfoClass;

	public uint BufferLength;

	private ushort BufferOffset;

	public ushort Reserved;

	public uint AdditionalInformation;

	public FileID FileId;

	public byte[] Buffer = new byte[0];

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

	public override int CommandLength => 32 + Buffer.Length;

	public SetInfoRequest()
		: base(SMB2CommandName.SetInfo)
	{
		StructureSize = 33;
	}

	public SetInfoRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		InfoType = (InfoType)ByteReader.ReadByte(buffer, offset + 64 + 2);
		FileInfoClass = ByteReader.ReadByte(buffer, offset + 64 + 3);
		BufferLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		BufferOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 8);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 10);
		AdditionalInformation = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 12);
		FileId = new FileID(buffer, offset + 64 + 16);
		Buffer = ByteReader.ReadBytes(buffer, offset + BufferOffset, (int)BufferLength);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		BufferOffset = 0;
		BufferLength = (uint)Buffer.Length;
		if (Buffer.Length != 0)
		{
			BufferOffset = 96;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, (byte)InfoType);
		ByteWriter.WriteByte(buffer, offset + 3, FileInfoClass);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, BufferLength);
		LittleEndianWriter.WriteUInt16(buffer, offset + 8, BufferOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 10, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 12, AdditionalInformation);
		FileId.WriteBytes(buffer, offset + 16);
		ByteWriter.WriteBytes(buffer, offset + 32, Buffer);
	}

	public void SetFileInformation(FileInformation fileInformation)
	{
		Buffer = fileInformation.GetBytes();
	}

	public void SetFileSystemInformation(FileSystemInformation fileSystemInformation)
	{
		Buffer = fileSystemInformation.GetBytes();
	}

	public void SetSecurityInformation(SecurityDescriptor securityDescriptor)
	{
		Buffer = securityDescriptor.GetBytes();
	}
}
