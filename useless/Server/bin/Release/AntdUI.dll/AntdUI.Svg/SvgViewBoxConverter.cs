using System;

namespace AntdUI.Svg;

internal class SvgViewBoxConverter
{
	public static SvgViewBox Parse(string value)
	{
		string[] array = value.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 4)
		{
			throw new SvgException("The 'viewBox' attribute must be in the format 'minX, minY, width, height'.");
		}
		return new SvgViewBox(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}
}
