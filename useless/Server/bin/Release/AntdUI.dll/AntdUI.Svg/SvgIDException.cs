using System;

namespace AntdUI.Svg;

public class SvgIDException : FormatException
{
	public SvgIDException(string message)
		: base(message)
	{
	}
}
