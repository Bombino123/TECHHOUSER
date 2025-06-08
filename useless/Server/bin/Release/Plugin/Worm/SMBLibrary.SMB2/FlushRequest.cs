using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class FlushRequest : SMB2Command
{
	public const int DeclaredSize = 24;

	private ushort StructureSize;

	public ushort Reserved1;

	public uint Reserved2;

	public FileID FileId;

	public override int CommandLength => 24;

	public FlushRequest()
		: base(SMB2CommandName.Flush)
	{
		StructureSize = 24;
	}

	public FlushRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Reserved1 = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		Reserved2 = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		FileId = new FileID(buffer, offset + 64 + 8);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved1);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, Reserved2);
		FileId.WriteBytes(buffer, offset + 8);
	}
}
