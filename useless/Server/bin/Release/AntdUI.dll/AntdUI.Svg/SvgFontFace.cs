namespace AntdUI.Svg;

public class SvgFontFace : SvgElement
{
	public override string ClassName => "font-face";

	[SvgAttribute("alphabetic")]
	public float Alphabetic
	{
		get
		{
			if (Attributes["alphabetic"] != null)
			{
				return (float)Attributes["alphabetic"];
			}
			return 0f;
		}
		set
		{
			Attributes["alphabetic"] = value;
		}
	}

	[SvgAttribute("ascent")]
	public float Ascent
	{
		get
		{
			if (Attributes["ascent"] == null)
			{
				if (Parent is SvgFont svgFont)
				{
					return UnitsPerEm - svgFont.VertOriginY;
				}
				return 0f;
			}
			return (float)Attributes["ascent"];
		}
		set
		{
			Attributes["ascent"] = value;
		}
	}

	[SvgAttribute("ascent-height")]
	public float AscentHeight
	{
		get
		{
			if (Attributes["ascent-height"] != null)
			{
				return (float)Attributes["ascent-height"];
			}
			return Ascent;
		}
		set
		{
			Attributes["ascent-height"] = value;
		}
	}

	[SvgAttribute("descent")]
	public float Descent
	{
		get
		{
			if (Attributes["descent"] == null)
			{
				if (Parent is SvgFont svgFont)
				{
					return svgFont.VertOriginY;
				}
				return 0f;
			}
			return (float)Attributes["descent"];
		}
		set
		{
			Attributes["descent"] = value;
		}
	}

	[SvgAttribute("font-family")]
	public override string FontFamily
	{
		get
		{
			return Attributes["font-family"] as string;
		}
		set
		{
			Attributes["font-family"] = value;
		}
	}

	[SvgAttribute("font-size")]
	public override SvgUnit FontSize
	{
		get
		{
			if (Attributes["font-size"] != null)
			{
				return (SvgUnit)Attributes["font-size"];
			}
			return SvgUnit.Empty;
		}
		set
		{
			Attributes["font-size"] = value;
		}
	}

	[SvgAttribute("font-style")]
	public override SvgFontStyle FontStyle
	{
		get
		{
			if (Attributes["font-style"] != null)
			{
				return (SvgFontStyle)Attributes["font-style"];
			}
			return SvgFontStyle.All;
		}
		set
		{
			Attributes["font-style"] = value;
		}
	}

	[SvgAttribute("font-variant")]
	public override SvgFontVariant FontVariant
	{
		get
		{
			if (Attributes["font-variant"] != null)
			{
				return (SvgFontVariant)Attributes["font-variant"];
			}
			return SvgFontVariant.Inherit;
		}
		set
		{
			Attributes["font-variant"] = value;
		}
	}

	[SvgAttribute("font-weight")]
	public override SvgFontWeight FontWeight
	{
		get
		{
			if (Attributes["font-weight"] != null)
			{
				return (SvgFontWeight)Attributes["font-weight"];
			}
			return SvgFontWeight.Inherit;
		}
		set
		{
			Attributes["font-weight"] = value;
		}
	}

	[SvgAttribute("panose-1")]
	public string Panose1
	{
		get
		{
			return Attributes["panose-1"] as string;
		}
		set
		{
			Attributes["panose-1"] = value;
		}
	}

	[SvgAttribute("units-per-em")]
	public float UnitsPerEm
	{
		get
		{
			if (Attributes["units-per-em"] != null)
			{
				return (float)Attributes["units-per-em"];
			}
			return 1000f;
		}
		set
		{
			Attributes["units-per-em"] = value;
		}
	}

	[SvgAttribute("x-height")]
	public float XHeight
	{
		get
		{
			if (Attributes["x-height"] != null)
			{
				return (float)Attributes["x-height"];
			}
			return float.MinValue;
		}
		set
		{
			Attributes["x-height"] = value;
		}
	}
}
