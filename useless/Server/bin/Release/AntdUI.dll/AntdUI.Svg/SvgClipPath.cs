using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

public sealed class SvgClipPath : SvgElement
{
	private bool _pathDirty = true;

	private GraphicsPath cachedClipPath;

	public override string ClassName => "clipPath";

	[SvgAttribute("clipPathUnits")]
	public SvgCoordinateUnits ClipPathUnits { get; set; }

	public SvgClipPath()
	{
		ClipPathUnits = SvgCoordinateUnits.Inherit;
	}

	public Region GetClipRegion(SvgVisualElement owner)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (cachedClipPath == null || _pathDirty)
		{
			cachedClipPath = new GraphicsPath();
			foreach (SvgElement child in Children)
			{
				CombinePaths(cachedClipPath, child);
			}
			_pathDirty = false;
		}
		GraphicsPath val = cachedClipPath;
		if (ClipPathUnits == SvgCoordinateUnits.ObjectBoundingBox)
		{
			val = (GraphicsPath)cachedClipPath.Clone();
			Matrix val2 = new Matrix();
			try
			{
				RectangleF bounds = owner.Bounds;
				val2.Scale(bounds.Width, bounds.Height, (MatrixOrder)1);
				val2.Translate(bounds.Left, bounds.Top, (MatrixOrder)1);
				val.Transform(val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return new Region(val);
	}

	private void CombinePaths(GraphicsPath path, SvgElement element)
	{
		if (element is SvgVisualElement svgVisualElement && svgVisualElement.Path(null) != null)
		{
			path.FillMode = (FillMode)(svgVisualElement.ClipRule == SvgClipRule.NonZero);
			GraphicsPath val = svgVisualElement.Path(null);
			if (svgVisualElement.Transforms != null)
			{
				foreach (SvgTransform transform in svgVisualElement.Transforms)
				{
					val.Transform(transform.Matrix(0f, 0f));
				}
			}
			if (val.PointCount > 0)
			{
				path.AddPath(val, false);
			}
		}
		foreach (SvgElement child in element.Children)
		{
			CombinePaths(path, child);
		}
	}

	protected override void AddElement(SvgElement child, int index)
	{
		base.AddElement(child, index);
		_pathDirty = true;
	}

	protected override void RemoveElement(SvgElement child)
	{
		base.RemoveElement(child);
		_pathDirty = true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
	}
}
