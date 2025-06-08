using System.ComponentModel;

namespace AntdUI.Svg;

[DefaultProperty("Text")]
public class SvgDescription : SvgElement, ISvgDescriptiveElement
{
	public override string ClassName => "desc";

	public override string ToString()
	{
		return Content;
	}
}
