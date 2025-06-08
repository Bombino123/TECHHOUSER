using System.Drawing;

namespace AntdUI;

public class ColorEventArgs : VEventArgs<Color>
{
	public ColorEventArgs(Color value)
		: base(value)
	{
	}
}
