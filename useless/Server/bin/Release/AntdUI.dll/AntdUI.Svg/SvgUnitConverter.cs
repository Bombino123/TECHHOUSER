using System;
using System.Globalization;

namespace AntdUI.Svg;

internal class SvgUnitConverter
{
	public static SvgUnit Parse(string? value)
	{
		if (value == null)
		{
			return new SvgUnit(SvgUnitType.User, 0f);
		}
		string text = value;
		int num = -1;
		switch (text)
		{
		case "none":
			return SvgUnit.None;
		case "medium":
			text = "1em";
			break;
		case "small":
			text = "0.8em";
			break;
		case "x-small":
			text = "0.7em";
			break;
		case "xx-small":
			text = "0.6em";
			break;
		case "large":
			text = "1.2em";
			break;
		case "x-large":
			text = "1.4em";
			break;
		case "xx-large":
			text = "1.7em";
			break;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '%' || (char.IsLetter(text[i]) && ((text[i] != 'e' && text[i] != 'E') || i >= text.Length - 1 || char.IsLetter(text[i + 1]))))
			{
				num = i;
				break;
			}
		}
		float.TryParse((num > -1) ? text.Substring(0, num) : text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
		if (num == -1)
		{
			return new SvgUnit(result);
		}
		return text.Substring(num).Trim().ToLower() switch
		{
			"mm" => new SvgUnit(SvgUnitType.Millimeter, result), 
			"cm" => new SvgUnit(SvgUnitType.Centimeter, result), 
			"in" => new SvgUnit(SvgUnitType.Inch, result), 
			"px" => new SvgUnit(SvgUnitType.Pixel, result), 
			"pt" => new SvgUnit(SvgUnitType.Point, result), 
			"pc" => new SvgUnit(SvgUnitType.Pica, result), 
			"%" => new SvgUnit(SvgUnitType.Percentage, result), 
			"em" => new SvgUnit(SvgUnitType.Em, result), 
			"ex" => new SvgUnit(SvgUnitType.Ex, result), 
			_ => throw new FormatException("Unit is in an invalid format '" + text + "'."), 
		};
	}
}
