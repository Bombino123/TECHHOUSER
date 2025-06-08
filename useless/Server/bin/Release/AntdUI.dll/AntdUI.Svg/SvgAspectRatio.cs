using System.ComponentModel;

namespace AntdUI.Svg;

public class SvgAspectRatio
{
	public SvgPreserveAspectRatio Align { get; set; }

	public bool Slice { get; set; }

	public bool Defer { get; set; }

	public SvgAspectRatio()
		: this(SvgPreserveAspectRatio.none)
	{
	}

	public SvgAspectRatio(SvgPreserveAspectRatio align)
		: this(align, slice: false)
	{
	}

	public SvgAspectRatio(SvgPreserveAspectRatio align, bool slice)
		: this(align, slice, defer: false)
	{
	}

	public SvgAspectRatio(SvgPreserveAspectRatio align, bool slice, bool defer)
	{
		Align = align;
		Slice = slice;
		Defer = defer;
	}

	public override string ToString()
	{
		return TypeDescriptor.GetConverter(typeof(SvgPreserveAspectRatio)).ConvertToString(Align) + (Slice ? " slice" : "");
	}
}
