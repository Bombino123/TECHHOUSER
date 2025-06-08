using System.Collections.Generic;

namespace AntdUI.Svg;

internal interface ISvgElement
{
	SvgElement Parent { get; }

	SvgElementCollection Children { get; }

	IList<ISvgNode> Nodes { get; }

	string ClassName { get; }

	void Render(ISvgRenderer renderer);
}
