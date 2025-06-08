namespace AntdUI.Svg;

public class NonSvgElement : SvgElement
{
	public string Name => base.ElementName;

	public NonSvgElement()
	{
	}

	public NonSvgElement(string elementName)
	{
		base.ElementName = elementName;
	}
}
