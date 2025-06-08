using System;

namespace AntdUI.Svg;

internal class SvgUnitCollectionConverter
{
	public static SvgUnitCollection Parse(string value)
	{
		if (string.Compare(value.Trim(), "none", StringComparison.InvariantCultureIgnoreCase) == 0)
		{
			return null;
		}
		string[] array = value.Trim().Split(new char[5] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		SvgUnitCollection svgUnitCollection = new SvgUnitCollection();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			SvgUnit item = SvgUnitConverter.Parse(array2[i].Trim());
			if (!item.IsNone)
			{
				svgUnitCollection.Add(item);
			}
		}
		return svgUnitCollection;
	}
}
