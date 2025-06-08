using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FullExtendedAttributeList : List<FullExtendedAttribute>
{
	public int Length
	{
		get
		{
			int num = 4;
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				FullExtendedAttribute current = enumerator.Current;
				num += current.Length;
			}
			return num;
		}
	}

	public FullExtendedAttributeList()
	{
	}

	public FullExtendedAttributeList(byte[] buffer)
		: this(buffer, 0)
	{
	}

	public FullExtendedAttributeList(byte[] buffer, ref int offset)
		: this(buffer, offset)
	{
		int num = (int)LittleEndianConverter.ToUInt32(buffer, offset);
		offset += num;
	}

	public FullExtendedAttributeList(byte[] buffer, int offset)
	{
		int num = (int)LittleEndianConverter.ToUInt32(buffer, offset);
		int i = offset + 4;
		FullExtendedAttribute fullExtendedAttribute;
		for (int num2 = offset + num; i < num2; i += fullExtendedAttribute.Length)
		{
			fullExtendedAttribute = new FullExtendedAttribute(buffer, i);
			Add(fullExtendedAttribute);
		}
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		WriteBytes(array, 0);
		return array;
	}

	public void WriteBytes(byte[] buffer, ref int offset)
	{
		WriteBytes(buffer, offset);
		offset += Length;
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)Length);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			FullExtendedAttribute current = enumerator.Current;
			current.WriteBytes(buffer, offset);
			offset += current.Length;
		}
	}
}
