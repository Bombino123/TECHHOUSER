using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Writer;

internal readonly struct SectionSizes
{
	public readonly uint SizeOfHeaders;

	public readonly uint SizeOfImage;

	public readonly uint BaseOfData;

	public readonly uint BaseOfCode;

	public readonly uint SizeOfCode;

	public readonly uint SizeOfInitdData;

	public readonly uint SizeOfUninitdData;

	public static uint GetSizeOfHeaders(uint fileAlignment, uint headerLen)
	{
		return Utils.AlignUp(headerLen, fileAlignment);
	}

	public SectionSizes(uint fileAlignment, uint sectionAlignment, uint headerLen, Func<IEnumerable<SectionSizeInfo>> getSectionSizeInfos)
	{
		SizeOfHeaders = GetSizeOfHeaders(fileAlignment, headerLen);
		SizeOfImage = Utils.AlignUp(SizeOfHeaders, sectionAlignment);
		BaseOfData = 0u;
		BaseOfCode = 0u;
		SizeOfCode = 0u;
		SizeOfInitdData = 0u;
		SizeOfUninitdData = 0u;
		foreach (SectionSizeInfo item in getSectionSizeInfos())
		{
			uint num = Utils.AlignUp(item.length, sectionAlignment);
			uint num2 = Utils.AlignUp(item.length, fileAlignment);
			bool flag = (item.characteristics & 0x20) != 0;
			bool flag2 = (item.characteristics & 0x40) != 0;
			bool flag3 = (item.characteristics & 0x80) != 0;
			if (BaseOfCode == 0 && flag)
			{
				BaseOfCode = SizeOfImage;
			}
			if (BaseOfData == 0 && (flag2 || flag3))
			{
				BaseOfData = SizeOfImage;
			}
			if (flag)
			{
				SizeOfCode += num2;
			}
			if (flag2)
			{
				SizeOfInitdData += num2;
			}
			if (flag3)
			{
				SizeOfUninitdData += num2;
			}
			SizeOfImage += num;
		}
	}
}
