using System.Collections.Generic;
using System.Runtime.InteropServices;
using Utilities;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class ExtendedAttributeNameList : List<ExtendedAttributeName>
{
	public int Length
	{
		get
		{
			int num = 4;
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				ExtendedAttributeName current = enumerator.Current;
				num += current.Length;
			}
			return num;
		}
	}

	public ExtendedAttributeNameList()
	{
	}

	public ExtendedAttributeNameList(byte[] buffer, int offset)
	{
		int num = (int)LittleEndianConverter.ToUInt32(buffer, offset);
		int i = offset + 4;
		ExtendedAttributeName extendedAttributeName;
		for (int num2 = offset + num; i < num2; i += extendedAttributeName.Length)
		{
			extendedAttributeName = new ExtendedAttributeName(buffer, i);
			Add(extendedAttributeName);
		}
	}

	public byte[] GetBytes()
	{
		byte[] array = new byte[Length];
		WriteBytes(array, 0);
		return array;
	}

	public void WriteBytes(byte[] buffer, int offset)
	{
		LittleEndianWriter.WriteUInt32(buffer, ref offset, (uint)Length);
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ExtendedAttributeName current = enumerator.Current;
			current.WriteBytes(buffer, offset);
			offset += current.Length;
		}
	}
}
