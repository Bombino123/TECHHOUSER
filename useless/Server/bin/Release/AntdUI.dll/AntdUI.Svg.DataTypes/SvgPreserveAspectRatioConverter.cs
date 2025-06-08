using System;

namespace AntdUI.Svg.DataTypes;

internal class SvgPreserveAspectRatioConverter
{
	public static SvgAspectRatio Parse(string value)
	{
		if (value == null)
		{
			return new SvgAspectRatio();
		}
		bool defer = false;
		bool slice = false;
		string[] array = value.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		int num = 0;
		if (array[0].Equals("defer"))
		{
			defer = true;
			num++;
			if (array.Length < 2)
			{
				throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
			}
		}
		SvgPreserveAspectRatio align = (SvgPreserveAspectRatio)Enum.Parse(typeof(SvgPreserveAspectRatio), array[num]);
		num++;
		if (array.Length > num)
		{
			string text = array[num];
			if (!(text == "meet"))
			{
				if (!(text == "slice"))
				{
					throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
				}
				slice = true;
			}
		}
		num++;
		if (array.Length > num)
		{
			throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
		}
		return new SvgAspectRatio(align, slice, defer);
	}
}
