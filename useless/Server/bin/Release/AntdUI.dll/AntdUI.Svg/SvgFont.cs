using System.Linq;

namespace AntdUI.Svg;

public class SvgFont : SvgElement
{
	public override string ClassName => "font";

	[SvgAttribute("horiz-adv-x")]
	public float HorizAdvX
	{
		get
		{
			if (Attributes["horiz-adv-x"] != null)
			{
				return (float)Attributes["horiz-adv-x"];
			}
			return 0f;
		}
		set
		{
			Attributes["horiz-adv-x"] = value;
		}
	}

	[SvgAttribute("horiz-origin-x")]
	public float HorizOriginX
	{
		get
		{
			if (Attributes["horiz-origin-x"] != null)
			{
				return (float)Attributes["horiz-origin-x"];
			}
			return 0f;
		}
		set
		{
			Attributes["horiz-origin-x"] = value;
		}
	}

	[SvgAttribute("horiz-origin-y")]
	public float HorizOriginY
	{
		get
		{
			if (Attributes["horiz-origin-y"] != null)
			{
				return (float)Attributes["horiz-origin-y"];
			}
			return 0f;
		}
		set
		{
			Attributes["horiz-origin-y"] = value;
		}
	}

	[SvgAttribute("vert-adv-y")]
	public float VertAdvY
	{
		get
		{
			if (Attributes["vert-adv-y"] != null)
			{
				return (float)Attributes["vert-adv-y"];
			}
			return Children.OfType<SvgFontFace>().First().UnitsPerEm;
		}
		set
		{
			Attributes["vert-adv-y"] = value;
		}
	}

	[SvgAttribute("vert-origin-x")]
	public float VertOriginX
	{
		get
		{
			if (Attributes["vert-origin-x"] != null)
			{
				return (float)Attributes["vert-origin-x"];
			}
			return HorizAdvX / 2f;
		}
		set
		{
			Attributes["vert-origin-x"] = value;
		}
	}

	[SvgAttribute("vert-origin-y")]
	public float VertOriginY
	{
		get
		{
			if (Attributes["vert-origin-y"] != null)
			{
				return (float)Attributes["vert-origin-y"];
			}
			if (Children.OfType<SvgFontFace>().First().Attributes["ascent"] != null)
			{
				return Children.OfType<SvgFontFace>().First().Ascent;
			}
			return 0f;
		}
		set
		{
			Attributes["vert-origin-y"] = value;
		}
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}
}
