using System;
using System.Drawing;

namespace AntdUI.Svg;

public class SvgGradientStop : SvgElement
{
	private SvgUnit _offset;

	public override string ClassName => "stop";

	[SvgAttribute("offset")]
	public SvgUnit Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			SvgUnit svgUnit = value;
			if (value.Type == SvgUnitType.Percentage)
			{
				if (value.Value > 100f)
				{
					svgUnit = new SvgUnit(value.Type, 100f);
				}
				else if (value.Value < 0f)
				{
					svgUnit = new SvgUnit(value.Type, 0f);
				}
			}
			else if (value.Type == SvgUnitType.User)
			{
				if (value.Value > 1f)
				{
					svgUnit = new SvgUnit(value.Type, 1f);
				}
				else if (value.Value < 0f)
				{
					svgUnit = new SvgUnit(value.Type, 0f);
				}
			}
			_offset = svgUnit.ToPercentage();
		}
	}

	[SvgAttribute("stop-color")]
	public override SvgPaintServer StopColor
	{
		get
		{
			SvgPaintServer attribute = Attributes.GetAttribute("stop-color", SvgColourServer.NotSet);
			if (attribute == SvgColourServer.Inherit)
			{
				return (Attributes["stop-color"] as SvgPaintServer) ?? SvgColourServer.NotSet;
			}
			return attribute;
		}
		set
		{
			Attributes["stop-color"] = value;
		}
	}

	[SvgAttribute("stop-opacity")]
	public override float Opacity
	{
		get
		{
			if (Attributes["stop-opacity"] != null)
			{
				return (float)Attributes["stop-opacity"];
			}
			return 1f;
		}
		set
		{
			Attributes["stop-opacity"] = SvgElement.FixOpacityValue(value);
		}
	}

	public SvgGradientStop()
	{
		_offset = new SvgUnit(0f);
	}

	public SvgGradientStop(SvgUnit offset, Color colour)
	{
		_offset = offset;
	}

	public Color GetColor(SvgElement parent)
	{
		return (SvgDeferredPaintServer.TryGet<SvgColourServer>(StopColor, parent) ?? throw new InvalidOperationException("Invalid paint server for gradient stop detected.")).Colour;
	}
}
