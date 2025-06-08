namespace AntdUI.Svg.FilterEffects;

public class SvgMergeNode : SvgElement
{
	public override string ClassName => "feMergeNode";

	[SvgAttribute("in")]
	public string Input
	{
		get
		{
			return Attributes.GetAttribute<string>("in");
		}
		set
		{
			Attributes["in"] = value;
		}
	}
}
