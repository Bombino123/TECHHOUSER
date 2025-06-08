using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[ComVisible(true)]
public class FindInformationList : List<FindInformation>
{
	public FindInformationList()
	{
	}

	public FindInformationList(byte[] buffer, FindInformationLevel informationLevel, bool isUnicode)
	{
		FindInformation findInformation;
		for (int i = 0; i < buffer.Length; i += (int)findInformation.NextEntryOffset)
		{
			findInformation = FindInformation.ReadEntry(buffer, i, informationLevel, isUnicode);
			Add(findInformation);
			if (findInformation.NextEntryOffset == 0)
			{
				break;
			}
		}
	}

	public byte[] GetBytes(bool isUnicode)
	{
		for (int i = 0; i < base.Count - 1; i++)
		{
			FindInformation findInformation = base[i];
			int length = findInformation.GetLength(isUnicode);
			findInformation.NextEntryOffset = (uint)length;
		}
		byte[] array = new byte[GetLength(isUnicode)];
		int offset = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.WriteBytes(array, ref offset, isUnicode);
		}
		return array;
	}

	public int GetLength(bool isUnicode)
	{
		int num = 0;
		for (int i = 0; i < base.Count; i++)
		{
			int length = base[i].GetLength(isUnicode);
			num += length;
		}
		return num;
	}
}
