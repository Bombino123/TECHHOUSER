using System.Drawing;

namespace AntdUI;

internal interface ITooltip
{
	string Text { get; set; }

	Font Font { get; set; }

	int Radius { get; set; }

	int ArrowSize { get; set; }

	TAlign ArrowAlign { get; set; }

	int? CustomWidth { get; set; }
}
