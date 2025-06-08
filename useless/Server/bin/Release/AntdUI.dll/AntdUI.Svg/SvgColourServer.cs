using System;
using System.Drawing;

namespace AntdUI.Svg;

public sealed class SvgColourServer : SvgPaintServer
{
	public static readonly SvgPaintServer NotSet = new SvgColourServer();

	public static readonly SvgPaintServer Inherit = new SvgColourServer();

	private Color _colour;

	public Color Colour
	{
		get
		{
			return _colour;
		}
		set
		{
			_colour = value;
		}
	}

	public SvgColourServer()
		: this(System.Drawing.Color.Black)
	{
	}

	public SvgColourServer(Color colour)
	{
		_colour = colour;
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (this != SvgPaintServer.None)
		{
			if (!(this == NotSet && forStroke))
			{
				return (Brush)new SolidBrush(System.Drawing.Color.FromArgb((int)Math.Round((double)opacity * ((double)(int)Colour.A / 255.0) * 255.0), Colour));
			}
			return (Brush)new SolidBrush(System.Drawing.Color.Transparent);
		}
		return (Brush)new SolidBrush(System.Drawing.Color.Transparent);
	}

	public override string ToString()
	{
		if (this == SvgPaintServer.None)
		{
			return "none";
		}
		if (this == NotSet)
		{
			return string.Empty;
		}
		if (this == Inherit)
		{
			return "inherit";
		}
		Color colour = Colour;
		if (colour.IsKnownColor)
		{
			return colour.Name;
		}
		return string.Format("#{0}", colour.ToArgb().ToString("x").Substring(2));
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SvgColourServer svgColourServer))
		{
			return false;
		}
		if ((this == SvgPaintServer.None && obj != SvgPaintServer.None) || (this != SvgPaintServer.None && obj == SvgPaintServer.None) || (this == NotSet && obj != NotSet) || (this != NotSet && obj == NotSet) || (this == Inherit && obj != Inherit) || (this != Inherit && obj == Inherit))
		{
			return false;
		}
		return GetHashCode() == svgColourServer.GetHashCode();
	}

	public override int GetHashCode()
	{
		return _colour.GetHashCode();
	}
}
