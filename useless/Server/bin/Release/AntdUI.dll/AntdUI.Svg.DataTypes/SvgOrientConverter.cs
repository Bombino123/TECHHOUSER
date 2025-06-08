using System;

namespace AntdUI.Svg.DataTypes;

internal class SvgOrientConverter
{
	public static SvgOrient Parse(string value)
	{
		if (value == null)
		{
			return new SvgOrient(0f);
		}
		if (value == "auto")
		{
			return new SvgOrient(isAuto: true);
		}
		if (!float.TryParse(value.ToString(), out var result))
		{
			throw new ArgumentOutOfRangeException("value must be a valid float.");
		}
		return new SvgOrient(result);
	}
}
