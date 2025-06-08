using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public struct NameFlags
{
	public const int Length = 2;

	public OwnerNodeType NodeType;

	public bool WorkGroup;

	public static explicit operator ushort(NameFlags nameFlags)
	{
		ushort num = (ushort)((uint)nameFlags.NodeType << 13);
		if (nameFlags.WorkGroup)
		{
			num = (ushort)(num | 0x8000u);
		}
		return num;
	}

	public static explicit operator NameFlags(ushort value)
	{
		NameFlags result = default(NameFlags);
		result.NodeType = (OwnerNodeType)((uint)(value >> 13) & 3u);
		result.WorkGroup = (value & 0x8000) > 0;
		return result;
	}
}
