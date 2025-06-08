using System.Drawing;

namespace AntdUI;

public interface ShadowConfig
{
	int Shadow { get; set; }

	Color? ShadowColor { get; set; }

	float ShadowOpacity { get; set; }

	int ShadowOffsetX { get; set; }

	int ShadowOffsetY { get; set; }
}
