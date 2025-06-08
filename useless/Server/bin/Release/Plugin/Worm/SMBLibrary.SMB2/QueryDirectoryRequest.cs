using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class QueryDirectoryRequest : SMB2Command
{
	public const int FixedLength = 32;

	public const int DeclaredSize = 33;

	private ushort StructureSize;

	public FileInformationClass FileInformationClass;

	public QueryDirectoryFlags Flags;

	public uint FileIndex;

	public FileID FileId;

	private ushort FileNameOffset;

	private ushort FileNameLength;

	public uint OutputBufferLength;

	public string FileName = string.Empty;

	public bool Restart
	{
		get
		{
			return (int)(Flags & QueryDirectoryFlags.SMB2_RESTART_SCANS) > 0;
		}
		set
		{
			if (value)
			{
				Flags |= QueryDirectoryFlags.SMB2_RESTART_SCANS;
			}
			else
			{
				Flags &= ~QueryDirectoryFlags.SMB2_RESTART_SCANS;
			}
		}
	}

	public bool ReturnSingleEntry
	{
		get
		{
			return (int)(Flags & QueryDirectoryFlags.SMB2_RETURN_SINGLE_ENTRY) > 0;
		}
		set
		{
			if (value)
			{
				Flags |= QueryDirectoryFlags.SMB2_RETURN_SINGLE_ENTRY;
			}
			else
			{
				Flags &= ~QueryDirectoryFlags.SMB2_RETURN_SINGLE_ENTRY;
			}
		}
	}

	public bool Reopen
	{
		get
		{
			return (int)(Flags & QueryDirectoryFlags.SMB2_REOPEN) > 0;
		}
		set
		{
			if (value)
			{
				Flags |= QueryDirectoryFlags.SMB2_REOPEN;
			}
			else
			{
				Flags &= ~QueryDirectoryFlags.SMB2_REOPEN;
			}
		}
	}

	public override int CommandLength => 32 + FileName.Length * 2;

	public QueryDirectoryRequest()
		: base(SMB2CommandName.QueryDirectory)
	{
		StructureSize = 33;
	}

	public QueryDirectoryRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		FileInformationClass = (FileInformationClass)ByteReader.ReadByte(buffer, offset + 64 + 2);
		Flags = (QueryDirectoryFlags)ByteReader.ReadByte(buffer, offset + 64 + 3);
		FileIndex = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		FileId = new FileID(buffer, offset + 64 + 8);
		FileNameOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 24);
		FileNameLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 26);
		OutputBufferLength = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 28);
		FileName = ByteReader.ReadUTF16String(buffer, offset + FileNameOffset, FileNameLength / 2);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		FileNameOffset = 0;
		FileNameLength = (ushort)(FileName.Length * 2);
		if (FileName.Length > 0)
		{
			FileNameOffset = 96;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, (byte)FileInformationClass);
		ByteWriter.WriteByte(buffer, offset + 3, (byte)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, FileIndex);
		FileId.WriteBytes(buffer, offset + 8);
		LittleEndianWriter.WriteUInt16(buffer, offset + 24, FileNameOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 26, FileNameLength);
		LittleEndianWriter.WriteUInt32(buffer, offset + 28, OutputBufferLength);
		ByteWriter.WriteUTF16String(buffer, offset + 32, FileName);
	}
}
