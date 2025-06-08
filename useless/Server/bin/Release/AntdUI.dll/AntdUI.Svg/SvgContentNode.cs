namespace AntdUI.Svg;

public class SvgContentNode : ISvgNode
{
	public string Content { get; set; }

	public ISvgNode DeepCopy()
	{
		return new SvgContentNode
		{
			Content = Content
		};
	}
}
