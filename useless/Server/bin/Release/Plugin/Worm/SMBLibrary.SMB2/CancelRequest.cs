using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class CancelRequest : SMB2Command
{
	public const int DeclaredSize = 4;

	private ushort StructureSize;

	public ushort Reserved;

	public override int CommandLength => 4;

	public CancelRequest()
		: base(SMB2CommandName.Cancel)
	{
		StructureSize = 4;
	}

	public CancelRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved);
	}
}
