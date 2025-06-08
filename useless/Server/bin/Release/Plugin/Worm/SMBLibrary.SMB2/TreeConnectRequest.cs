using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public class TreeConnectRequest : SMB2Command
{
	public const int FixedSize = 8;

	public const int DeclaredSize = 9;

	private ushort StructureSize;

	public ushort Reserved;

	private ushort PathOffset;

	private ushort PathLength;

	public string Path = string.Empty;

	public override int CommandLength => 8 + Path.Length * 2;

	public TreeConnectRequest()
		: base(SMB2CommandName.TreeConnect)
	{
		StructureSize = 9;
	}

	public TreeConnectRequest(byte[] buffer, int offset)
		: base(buffer, offset)
	{
		StructureSize = LittleEndianConverter.ToUInt16(buffer, offset + 64);
		Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 2);
		PathOffset = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 4);
		PathLength = LittleEndianConverter.ToUInt16(buffer, offset + 64 + 6);
		if (PathLength > 0)
		{
			Path = ByteReader.ReadUTF16String(buffer, offset + PathOffset, PathLength / 2);
		}
	}

	public override void WriteCommandBytes(byte[] buffer, int offset)
	{
		PathOffset = 0;
		PathLength = (ushort)(Path.Length * 2);
		if (Path.Length > 0)
		{
			PathOffset = 72;
		}
		LittleEndianWriter.WriteUInt16(buffer, offset, StructureSize);
		LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved);
		LittleEndianWriter.WriteUInt16(buffer, offset + 4, PathOffset);
		LittleEndianWriter.WriteUInt16(buffer, offset + 6, PathLength);
		if (Path.Length > 0)
		{
			ByteWriter.WriteUTF16String(buffer, offset + 8, Path);
		}
	}
}
