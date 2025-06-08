namespace AntdUI.Svg;

public class SvgText : SvgTextBase
{
	public override string ClassName => "text";

	public SvgText()
	{
	}

	public SvgText(string text)
		: this()
	{
		Text = text;
	}
}
