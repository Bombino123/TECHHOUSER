using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public struct ShareTypeExtended
{
	public ShareType ShareType;

	public bool IsSpecial;

	public bool IsTemporary;

	public ShareTypeExtended(ShareType shareType)
	{
		ShareType = shareType;
		IsSpecial = false;
		IsTemporary = false;
	}

	public ShareTypeExtended(ShareType shareType, bool isSpecial, bool isTemporary)
	{
		ShareType = shareType;
		IsSpecial = isSpecial;
		IsTemporary = isTemporary;
	}

	public ShareTypeExtended(NDRParser parser)
		: this(parser.ReadUInt32())
	{
	}

	public ShareTypeExtended(uint shareTypeExtended)
	{
		ShareType = (ShareType)(shareTypeExtended & 0xFFFFFFFu);
		IsSpecial = (shareTypeExtended & 0x80000000u) != 0;
		IsTemporary = (shareTypeExtended & 0x40000000) != 0;
	}

	public void Write(NDRWriter writer)
	{
		writer.WriteUInt32(ToUInt32());
	}

	public uint ToUInt32()
	{
		uint num = (uint)ShareType;
		if (IsSpecial)
		{
			num |= 0x80000000u;
		}
		if (IsTemporary)
		{
			num |= 0x40000000u;
		}
		return num;
	}
}
