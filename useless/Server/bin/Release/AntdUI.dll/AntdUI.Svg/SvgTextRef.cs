using System;
using System.Collections.Generic;
using System.Linq;

namespace AntdUI.Svg;

public class SvgTextRef : SvgTextBase
{
	private Uri _referencedElement;

	public override string ClassName => "tref";

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual Uri ReferencedElement
	{
		get
		{
			return _referencedElement;
		}
		set
		{
			_referencedElement = value;
		}
	}

	internal override IEnumerable<ISvgNode> GetContentNodes()
	{
		SvgTextBase svgTextBase = OwnerDocument.IdManager.GetElementById(ReferencedElement) as SvgTextBase;
		IEnumerable<ISvgNode> enumerable = null;
		enumerable = ((svgTextBase != null) ? svgTextBase.GetContentNodes() : base.GetContentNodes());
		return enumerable.Where((ISvgNode o) => !(o is ISvgDescriptiveElement));
	}
}
