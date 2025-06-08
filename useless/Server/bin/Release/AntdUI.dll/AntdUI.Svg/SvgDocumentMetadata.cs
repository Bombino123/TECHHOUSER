namespace AntdUI.Svg;

public class SvgDocumentMetadata : SvgElement
{
	public override string ClassName => "metadata";

	public SvgDocumentMetadata()
	{
		Content = "";
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}
}
