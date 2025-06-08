using System.Drawing;

namespace AntdUI;

internal interface ITooltipConfig
{
	Font? Font { get; set; }

	int Radius { get; set; }

	int ArrowSize { get; set; }

	TAlign ArrowAlign { get; set; }

	int? CustomWidth { get; set; }
}
