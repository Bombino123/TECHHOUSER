using System;
using System.Drawing;

namespace AntdUI.Svg;

public abstract class SvgPaintServer : SvgElement
{
	public static readonly SvgPaintServer None = new SvgColourServer();

	public Func<SvgPaintServer> GetCallback { get; set; }

	public SvgPaintServer()
	{
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}

	public abstract Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false);

	public override string ToString()
	{
		return $"url(#{base.ID})";
	}
}
