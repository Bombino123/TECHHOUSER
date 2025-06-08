using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AntdUI.Svg;

public static class SvgFontManager
{
	private static readonly Dictionary<string, FontFamily> SystemFonts;

	static SvgFontManager()
	{
		SystemFonts = FontFamily.Families.ToDictionary((FontFamily ff) => ff.Name.ToLower());
	}

	public static FontFamily? FindFont(string name)
	{
		if (name == null)
		{
			return null;
		}
		string key = name.ToLower();
		if (SystemFonts.TryGetValue(key, out FontFamily value))
		{
			return value;
		}
		return null;
	}
}
