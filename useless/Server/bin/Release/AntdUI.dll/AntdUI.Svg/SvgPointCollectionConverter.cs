using System;

namespace AntdUI.Svg;

internal class SvgPointCollectionConverter
{
	public static SvgPointCollection Parse(string value)
	{
		string text = value.Trim();
		if (string.Compare(text, "none", StringComparison.InvariantCultureIgnoreCase) == 0)
		{
			return null;
		}
		CoordinateParser coordinateParser = new CoordinateParser(text);
		SvgPointCollection svgPointCollection = new SvgPointCollection();
		float result;
		while (coordinateParser.TryGetFloat(out result))
		{
			svgPointCollection.Add(new SvgUnit(SvgUnitType.User, result));
		}
		return svgPointCollection;
	}
}
