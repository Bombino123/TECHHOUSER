using System.Drawing.Drawing2D;
using System.Linq;
using AntdUI.Svg.Pathing;

namespace AntdUI.Svg;

public class SvgGlyph : SvgPathBasedElement
{
	private GraphicsPath _path;

	public override string ClassName => "glyph";

	[SvgAttribute("d", true)]
	public SvgPathSegmentList PathData
	{
		get
		{
			return Attributes.GetAttribute<SvgPathSegmentList>("d");
		}
		set
		{
			Attributes["d"] = value;
		}
	}

	[SvgAttribute("glyph-name", true)]
	public virtual string GlyphName
	{
		get
		{
			return Attributes["glyph-name"] as string;
		}
		set
		{
			Attributes["glyph-name"] = value;
		}
	}

	[SvgAttribute("horiz-adv-x", true)]
	public float HorizAdvX
	{
		get
		{
			if (Attributes["horiz-adv-x"] != null)
			{
				return (float)Attributes["horiz-adv-x"];
			}
			return base.Parents.OfType<SvgFont>().First().HorizAdvX;
		}
		set
		{
			Attributes["horiz-adv-x"] = value;
		}
	}

	[SvgAttribute("unicode", true)]
	public string Unicode
	{
		get
		{
			return Attributes["unicode"] as string;
		}
		set
		{
			Attributes["unicode"] = value;
		}
	}

	[SvgAttribute("vert-adv-y", true)]
	public float VertAdvY
	{
		get
		{
			if (Attributes["vert-adv-y"] != null)
			{
				return (float)Attributes["vert-adv-y"];
			}
			return base.Parents.OfType<SvgFont>().First().VertAdvY;
		}
		set
		{
			Attributes["vert-adv-y"] = value;
		}
	}

	[SvgAttribute("vert-origin-x", true)]
	public float VertOriginX
	{
		get
		{
			if (Attributes["vert-origin-x"] != null)
			{
				return (float)Attributes["vert-origin-x"];
			}
			return base.Parents.OfType<SvgFont>().First().VertOriginX;
		}
		set
		{
			Attributes["vert-origin-x"] = value;
		}
	}

	[SvgAttribute("vert-origin-y", true)]
	public float VertOriginY
	{
		get
		{
			if (Attributes["vert-origin-y"] != null)
			{
				return (float)Attributes["vert-origin-y"];
			}
			return base.Parents.OfType<SvgFont>().First().VertOriginY;
		}
		set
		{
			Attributes["vert-origin-y"] = value;
		}
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			foreach (SvgPathSegment pathDatum in PathData)
			{
				pathDatum.AddToPath(_path);
			}
			IsPathDirty = false;
		}
		return _path;
	}

	public SvgGlyph()
	{
		SvgPathSegmentList value = new SvgPathSegmentList();
		Attributes["d"] = value;
	}
}
