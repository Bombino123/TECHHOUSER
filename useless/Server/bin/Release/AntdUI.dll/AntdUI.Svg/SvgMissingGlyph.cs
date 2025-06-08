namespace AntdUI.Svg;

public class SvgMissingGlyph : SvgGlyph
{
	public override string ClassName => "missing-glyph";

	[SvgAttribute("glyph-name")]
	public override string GlyphName
	{
		get
		{
			return (Attributes["glyph-name"] as string) ?? "__MISSING_GLYPH__";
		}
		set
		{
			Attributes["glyph-name"] = value;
		}
	}
}
