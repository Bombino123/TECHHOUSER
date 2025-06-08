using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary;

[ComVisible(true)]
public class SecurityDescriptor
{
	public const int FixedLength = 20;

	public byte Revision;

	public byte Sbz1;

	public SecurityDescriptorControl Control;

	public SID OwnerSid;

	public SID GroupSid;

	public ACL Sacl;

	public ACL Dacl;

	public int Length
	{
		get
		{
			int num = 20;
			if (OwnerSid != null)
			{
				num += OwnerSid.Length;
			}
			if (GroupSid != null)
			{
				num += GroupSid.Length;
			}
			if (Sacl != null)
			{
				num += Sacl.Length;
			}
			if (Dacl != null)
			{
				num += Dacl.Length;
			}
			return num;
		}
	}

	public SecurityDescriptor()
	{
		Revision = 1;
	}

	public SecurityDescriptor(byte[] buffer, int offset)
	{
		Revision = ByteReader.ReadByte(buffer, ref offset);
		Sbz1 = ByteReader.ReadByte(buffer, ref offset);
		Control = (SecurityDescriptorControl)LittleEndianReader.ReadUInt16(buffer, ref offset);
		uint num = LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint num2 = LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint num3 = LittleEndianReader.ReadUInt32(buffer, ref offset);
		uint num4 = LittleEndianReader.ReadUInt32(buffer, ref offset);
		if (num != 0)
		{
			OwnerSid = new SID(buffer, (int)num);
		}
		if (num2 != 0)
		{
			GroupSid = new SID(buffer, (int)num2);
		}
		if (num3 != 0)
		{
			Sacl = new ACL(buffer, (int)num3);
		}
		if (num4 != 0)
		{
			Dacl = new ACL(buffer, (int)num4);
		}
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		uint value = 0u;
		uint value2 = 0u;
		uint value3 = 0u;
		uint value4 = 0u;
		int num = 20;
		if (OwnerSid != null)
		{
			value = (uint)num;
			num += OwnerSid.Length;
		}
		if (GroupSid != null)
		{
			value2 = (uint)num;
			num += GroupSid.Length;
		}
		if (Sacl != null)
		{
			value3 = (uint)num;
			num += Sacl.Length;
		}
		if (Dacl != null)
		{
			value4 = (uint)num;
			num += Dacl.Length;
		}
		num = 0;
		ByteWriter.WriteByte(array, ref num, Revision);
		ByteWriter.WriteByte(array, ref num, Sbz1);
		LittleEndianWriter.WriteUInt16(array, ref num, (ushort)Control);
		LittleEndianWriter.WriteUInt32(array, ref num, value);
		LittleEndianWriter.WriteUInt32(array, ref num, value2);
		LittleEndianWriter.WriteUInt32(array, ref num, value3);
		LittleEndianWriter.WriteUInt32(array, ref num, value4);
		if (OwnerSid != null)
		{
			OwnerSid.WriteBytes(array, ref num);
		}
		if (GroupSid != null)
		{
			GroupSid.WriteBytes(array, ref num);
		}
		if (Sacl != null)
		{
			Sacl.WriteBytes(array, ref num);
		}
		if (Dacl != null)
		{
			Dacl.WriteBytes(array, ref num);
		}
		return array;
	}
}
