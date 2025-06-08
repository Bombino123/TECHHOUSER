using System;

namespace AntdUI.Svg;

[Flags]
public enum SvgFontWeight
{
	All = 0x1FF,
	Inherit = 0,
	Normal = 8,
	Bold = 0x40,
	Bolder = 0x200,
	Lighter = 0x400,
	W100 = 1,
	W200 = 2,
	W300 = 4,
	W400 = 8,
	W500 = 0x10,
	W600 = 0x20,
	W700 = 0x40,
	W800 = 0x80,
	W900 = 0x100
}
