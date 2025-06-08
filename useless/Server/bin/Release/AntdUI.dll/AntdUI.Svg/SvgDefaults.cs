using System.Collections.Generic;

namespace AntdUI.Svg;

public static class SvgDefaults
{
	private static Dictionary<string, Dictionary<string, string>> _propDefaults;

	private static readonly Dictionary<string, string> _defaults;

	static SvgDefaults()
	{
		_propDefaults = new Dictionary<string, Dictionary<string, string>>
		{
			{
				"SvgRadialGradientServer",
				new Dictionary<string, string>
				{
					{ "cx", "50%" },
					{ "cy", "50%" },
					{ "r", "50%" }
				}
			},
			{
				"SvgLinearGradientServer",
				new Dictionary<string, string>
				{
					{ "x1", "0%" },
					{ "x2", "100%" },
					{ "y1", "0%" },
					{ "y2", "100%" }
				}
			}
		};
		_defaults = new Dictionary<string, string>
		{
			{ "d", "" },
			{ "viewBox", "0, 0, 0, 0" },
			{ "visibility", "visible" },
			{ "display", "inline" },
			{ "enable-background", "accumulate" },
			{ "opacity", "1" },
			{ "clip", "auto" },
			{ "clip-rule", "nonzero" },
			{ "clipPathUnits", "userSpaceOnUse" },
			{ "transform", "" },
			{ "x1", "0" },
			{ "x2", "0" },
			{ "y1", "0" },
			{ "y2", "0" },
			{ "cx", "0" },
			{ "cy", "0" },
			{ "fill", "" },
			{ "fill-opacity", "1" },
			{ "fill-rule", "nonzero" },
			{ "stop-color", "black" },
			{ "stop-opacity", "1" },
			{ "stroke", "none" },
			{ "stroke-opacity", "1" },
			{ "stroke-width", "1" },
			{ "stroke-miterlimit", "4" },
			{ "stroke-linecap", "butt" },
			{ "stroke-linejoin", "miter" },
			{ "stroke-dasharray", "none" },
			{ "stroke-dashoffset", "0" },
			{ "markerUnits", "strokeWidth" },
			{ "refX", "0" },
			{ "refY", "0" },
			{ "markerWidth", "3" },
			{ "markerHeight", "3" },
			{ "orient", "0" }
		};
	}

	public static bool IsDefault(string attributeName, string componentType, string value)
	{
		if (_propDefaults.ContainsKey(componentType) && _propDefaults[componentType].ContainsKey(attributeName))
		{
			return _propDefaults[componentType][attributeName] == value;
		}
		if (_defaults.ContainsKey(attributeName))
		{
			return _defaults[attributeName] == value;
		}
		return false;
	}
}
