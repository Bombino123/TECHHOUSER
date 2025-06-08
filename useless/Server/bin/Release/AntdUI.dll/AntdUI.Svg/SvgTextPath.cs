using System;
using System.Drawing.Drawing2D;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

public class SvgTextPath : SvgTextBase
{
	private Uri _referencedPath;

	public override string ClassName => "textPath";

	public override SvgUnitCollection Dx
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[SvgAttribute("startOffset")]
	public virtual SvgUnit StartOffset
	{
		get
		{
			if (_dx.Count >= 1)
			{
				return _dx[0];
			}
			return SvgUnit.None;
		}
		set
		{
			if (_dx.Count < 1)
			{
				_dx.Add(value);
			}
			else
			{
				_dx[0] = value;
			}
		}
	}

	[SvgAttribute("method")]
	public virtual SvgTextPathMethod Method
	{
		get
		{
			if (Attributes["method"] != null)
			{
				return (SvgTextPathMethod)Attributes["method"];
			}
			return SvgTextPathMethod.Align;
		}
		set
		{
			Attributes["method"] = value;
		}
	}

	[SvgAttribute("spacing")]
	public virtual SvgTextPathSpacing Spacing
	{
		get
		{
			if (Attributes["spacing"] != null)
			{
				return (SvgTextPathSpacing)Attributes["spacing"];
			}
			return SvgTextPathSpacing.Exact;
		}
		set
		{
			Attributes["spacing"] = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual Uri ReferencedPath
	{
		get
		{
			return _referencedPath;
		}
		set
		{
			_referencedPath = value;
		}
	}

	protected override GraphicsPath GetBaselinePath(ISvgRenderer renderer)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (!(OwnerDocument.IdManager.GetElementById(ReferencedPath) is SvgVisualElement svgVisualElement))
		{
			return null;
		}
		GraphicsPath val = (GraphicsPath)svgVisualElement.Path(renderer).Clone();
		if (svgVisualElement.Transforms.Count > 0)
		{
			Matrix val2 = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
			foreach (SvgTransform transform in svgVisualElement.Transforms)
			{
				val2.Multiply(transform.Matrix(0f, 0f));
			}
			val.Transform(val2);
		}
		return val;
	}

	protected override float GetAuthorPathLength()
	{
		if (!(OwnerDocument.IdManager.GetElementById(ReferencedPath) is SvgPath svgPath))
		{
			return 0f;
		}
		return svgPath.PathLength;
	}
}
