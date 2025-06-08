using System;
using System.Drawing;
using AntdUI.Svg;

namespace AntdUI;

public static class SvgExtend
{
	public static Bitmap? GetImgExtend(string svg, Rectangle rect, Color? color = null)
	{
		if (rect.Width > 0 && rect.Height > 0)
		{
			return svg.SvgToBmp(rect.Width, rect.Height, color);
		}
		return null;
	}

	public static bool GetImgExtend(this Canvas g, string svg, Rectangle rect, Color? color = null)
	{
		if (rect.Width > 0 && rect.Height > 0)
		{
			Bitmap val = svg.SvgToBmp(rect.Width, rect.Height, color);
			try
			{
				if (val == null)
				{
					return false;
				}
				g.Image(val, rect);
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return false;
	}

	public static Bitmap? SvgToBmp(this string svg, int width, int height, Color? color)
	{
		SvgDocument svgDocument = SvgDocument(svg);
		if (svgDocument == null)
		{
			return null;
		}
		if (color.HasValue)
		{
			svgDocument.Fill = new SvgColourServer(color.Value);
		}
		svgDocument.Width = width;
		svgDocument.Height = height;
		return svgDocument.Draw();
	}

	public static Bitmap? SvgToBmp(this string svg)
	{
		SvgDocument svgDocument = SvgDocument(svg);
		if (svgDocument == null)
		{
			return null;
		}
		float dpi = Config.Dpi;
		if (dpi != 1f)
		{
			svgDocument.Width = (float)svgDocument.Width * dpi;
			svgDocument.Height = (float)svgDocument.Height * dpi;
		}
		return svgDocument.Draw();
	}

	private static SvgDocument? SvgDocument(string svg)
	{
		if (svg.StartsWith("<svg"))
		{
			return AntdUI.Svg.SvgDocument.FromSvg<SvgDocument>(svg);
		}
		if (SvgDb.Custom.TryGetValue(svg, out string value))
		{
			return AntdUI.Svg.SvgDocument.FromSvg<SvgDocument>(value);
		}
		return null;
	}
}
