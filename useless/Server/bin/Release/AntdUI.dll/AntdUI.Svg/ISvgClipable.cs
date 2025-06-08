using System;

namespace AntdUI.Svg;

public interface ISvgClipable
{
	Uri ClipPath { get; set; }

	SvgClipRule ClipRule { get; set; }

	void SetClip(ISvgRenderer renderer);

	void ResetClip(ISvgRenderer renderer);
}
