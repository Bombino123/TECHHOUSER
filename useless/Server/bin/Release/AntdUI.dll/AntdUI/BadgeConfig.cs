using System.Drawing;

namespace AntdUI;

public interface BadgeConfig
{
	string? Badge { get; set; }

	string? BadgeSvg { get; set; }

	TAlignFrom BadgeAlign { get; set; }

	float BadgeSize { get; set; }

	bool BadgeMode { get; set; }

	Color? BadgeBack { get; set; }

	int BadgeOffsetX { get; set; }

	int BadgeOffsetY { get; set; }
}
