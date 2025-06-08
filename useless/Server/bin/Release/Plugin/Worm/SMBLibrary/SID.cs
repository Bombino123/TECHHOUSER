using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class SID
{
	public static readonly byte[] WORLD_SID_AUTHORITY = new byte[6] { 0, 0, 0, 0, 0, 1 };

	public static readonly byte[] LOCAL_SID_AUTHORITY = new byte[6] { 0, 0, 0, 0, 0, 2 };

	public static readonly byte[] CREATOR_SID_AUTHORITY = new byte[6] { 0, 0, 0, 0, 0, 2 };

	public static readonly byte[] SECURITY_NT_AUTHORITY = new byte[6] { 0, 0, 0, 0, 0, 5 };

	public const int FixedLength = 8;

	public byte Revision;

	public byte[] IdentifierAuthority;

	public List<uint> SubAuthority = new List<uint>();

	public int Length => 8 + SubAuthority.Count * 4;

	public static SID Everyone => new SID
	{
		IdentifierAuthority = WORLD_SID_AUTHORITY,
		SubAuthority = { 0u }
	};

	public static SID LocalSystem => new SID
	{
		IdentifierAuthority = SECURITY_NT_AUTHORITY,
		SubAuthority = { 18u }
	};

	public SID()
	{
		Revision = 1;
	}

	public SID(byte[] buffer, int offset)
	{
		Revision = ByteReader.ReadByte(buffer, ref offset);
		byte b = ByteReader.ReadByte(buffer, ref offset);
		IdentifierAuthority = ByteReader.ReadBytes(buffer, ref offset, 6);
		for (int i = 0; i < b; i++)
		{
			uint item = LittleEndianReader.ReadUInt32(buffer, ref offset);
			SubAuthority.Add(item);
		}
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		byte value = (byte)SubAuthority.Count;
		ByteWriter.WriteByte(buffer, ref offset, Revision);
		ByteWriter.WriteByte(buffer, ref offset, value);
		ByteWriter.WriteBytes(buffer, ref offset, IdentifierAuthority, 6);
		for (int i = 0; i < SubAuthority.Count; i++)
		{
			LittleEndianWriter.WriteUInt32(buffer, ref offset, SubAuthority[i]);
		}
	}
}
