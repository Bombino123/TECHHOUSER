using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class ACL : List<ACE>
{
	public const int FixedLength = 8;

	public byte AclRevision;

	public byte Sbz1;

	public ushort Sbz2;

	public int Length
	{
		get
		{
			int num = 8;
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				ACE current = enumerator.Current;
				num += current.Length;
			}
			return num;
		}
	}

	public ACL()
	{
		AclRevision = 2;
	}

	public ACL(byte[] buffer, int offset)
	{
		AclRevision = ByteReader.ReadByte(buffer, offset);
		Sbz1 = ByteReader.ReadByte(buffer, offset + 1);
		LittleEndianConverter.ToUInt16(buffer, offset + 2);
		ushort num = LittleEndianConverter.ToUInt16(buffer, offset + 4);
		Sbz2 = LittleEndianConverter.ToUInt16(buffer, offset + 6);
		offset += 8;
		for (int i = 0; i < num; i++)
		{
			ACE ace = ACE.GetAce(buffer, offset);
			Add(ace);
			offset += ace.Length;
		}
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		ByteWriter.WriteByte(buffer, ref offset, AclRevision);
		ByteWriter.WriteByte(buffer, ref offset, Sbz1);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)Length);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)base.Count);
		LittleEndianWriter.WriteUInt16(buffer, ref offset, Sbz2);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.WriteBytes(buffer, ref offset);
		}
	}
}
