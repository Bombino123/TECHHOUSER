using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class TreeConnectResponse : SMB2Command
{
	public const int DeclaredSize = 16;

	private ushort StructureSize;

	public ShareType ShareType;

	public byte Reserved;

	public ShareFlags ShareFlags;

	public ShareCapabilities Capabilities;

	public AccessMask MaximalAccess;

	public override int CommandLength => 16;

	public TreeConnectResponse()
		: base(SMB2CommandName.TreeConnect)
	{
		Header.IsResponse = true;
		StructureSize = 16;
	}

	public TreeConnectResponse(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		ShareType = (ShareType)ByteReader.ReadByte(buffer, offset + 64 + 2);
		Reserved = ByteReader.ReadByte(buffer, offset + 64 + 3);
		ShareFlags = (ShareFlags)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 4);
		Capabilities = (ShareCapabilities)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 8);
		MaximalAccess = (AccessMask)LittleEndianConverter.ToUInt32(buffer, offset + 64 + 12);
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		ByteWriter.WriteByte(buffer, offset + 2, (byte)ShareType);
		ByteWriter.WriteByte(buffer, offset + 3, Reserved);
		LittleEndianWriter.WriteUInt32(buffer, offset + 4, (uint)ShareFlags);
		LittleEndianWriter.WriteUInt32(buffer, offset + 8, (uint)Capabilities);
		LittleEndianWriter.WriteUInt32(buffer, offset + 12, (uint)MaximalAccess);
	}
}
