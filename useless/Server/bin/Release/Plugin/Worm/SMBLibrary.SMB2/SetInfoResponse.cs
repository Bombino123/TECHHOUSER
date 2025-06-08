using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class SetInfoResponse : SMB2Command
{
	public const int DeclaredSize = 2;

	private ushort StructureSize;

	public override int CommandLength => 2;

	public SetInfoResponse()
		: base(SMB2CommandName.SetInfo)
	{
		Header.IsResponse = true;
		StructureSize = 2;
	}

	public SetInfoResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
	}
}
