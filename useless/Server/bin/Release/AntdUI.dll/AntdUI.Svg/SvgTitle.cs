namespace AntdUI.Svg;

public class SvgTitle : SvgElement, ISvgDescriptiveElement
{
	public override string ClassName => "title";

	public override string ToString()
	{
		return Content;
	}
}
