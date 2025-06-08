using System.Drawing;

namespace AntdUI;

public class TooltipConfig : ITooltipConfig
{
	public Font? Font { get; set; }

	public int Radius { get; set; } = 6;


	public int ArrowSize { get; set; } = 8;


	public TAlign ArrowAlign { get; set; } = TAlign.Top;


	public int? CustomWidth { get; set; }
}
