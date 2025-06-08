using System;

namespace AntdUI.Svg;

public class SvgFontFaceUri : SvgElement
{
	private Uri _referencedElement;

	public override string ClassName => "font-face-uri";

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
}
