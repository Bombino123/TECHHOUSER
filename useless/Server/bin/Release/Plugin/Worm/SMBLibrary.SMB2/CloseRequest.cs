using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class CloseRequest : SMB2Command
{
	public const int DeclaredSize = 24;

	private ushort StructureSize;

	public CloseFlags Flags;

	public uint Reserved;

	public FileID FileId;

	public bool PostQueryAttributes => (int)(Flags & CloseFlags.PostQueryAttributes) > 0;

	public override int CommandLength => 24;

	public CloseRequest()
		: base(SMB2CommandName.Close)
	{
		StructureSize = 24;
	}

	public CloseRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Flags = (CloseFlags)LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		Reserved = LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		FileId = new FileID(buffer, offset + 64 + 8);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)Flags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, Reserved);
		FileId.WriteBytes(buffer, offset + 8);
	}
}
