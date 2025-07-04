using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public interface IFontDefn : IDisposable
{
	float Size { get; }

	float SizeInPoints { get; }

	void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location);

	float Ascent(ISvgRenderer renderer);

	IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text);

	SizeF MeasureString(ISvgRenderer renderer, string text);
}
