namespace AntdUI.Svg;

public abstract class SvgKern : SvgElement
{
	[SvgAttribute("g1")]
	public string Glyph1
	{
		get
		{
			return Attributes["g1"] as string;
		}
		set
		{
			Attributes["g1"] = value;
		}
	}

	[SvgAttribute("g2")]
	public string Glyph2
	{
		get
		{
			return Attributes["g2"] as string;
		}
		set
		{
			Attributes["g2"] = value;
		}
	}

	[SvgAttribute("u1")]
	public string Unicode1
	{
		get
		{
			return Attributes["u1"] as string;
		}
		set
		{
			Attributes["u1"] = value;
		}
	}

	[SvgAttribute("u2")]
	public string Unicode2
	{
		get
		{
			return Attributes["u2"] as string;
		}
		set
		{
			Attributes["u2"] = value;
		}
	}

	[SvgAttribute("k")]
	public float Kerning
	{
		get
		{
			if (Attributes["k"] != null)
			{
				return (float)Attributes["k"];
			}
			return 0f;
		}
		set
		{
			Attributes["k"] = value;
		}
	}
}
