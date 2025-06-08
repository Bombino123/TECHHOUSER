using System;

namespace AntdUI.Svg;

public class SvgException : FormatException
{
	public SvgException(string message)
		: base(message)
	{
	}
}
