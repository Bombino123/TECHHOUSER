using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using AntdUI.Svg.DataTypes;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg.FilterEffects;

public sealed class SvgFilter : SvgElement
{
	public override string ClassName => "filter";

	[SvgAttribute("x")]
	public SvgUnit X
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("x");
		}
		set
		{
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("y")]
	public SvgUnit Y
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("y");
		}
		set
		{
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("width")]
	public SvgUnit Width
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("width");
		}
		set
		{
			Attributes["width"] = value;
		}
	}

	[SvgAttribute("height")]
	public SvgUnit Height
	{
		get
		{
			return Attributes.GetAttribute<SvgUnit>("height");
		}
		set
		{
			Attributes["height"] = value;
		}
	}

	[SvgAttribute("color-interpolation-filters")]
	public SvgColourInterpolation ColorInterpolationFilters
	{
		get
		{
			return Attributes.GetAttribute<SvgColourInterpolation>("color-interpolation-filters");
		}
		set
		{
			Attributes["color-interpolation-filters"] = value;
		}
	}

	protected override void Render(ISvgRenderer renderer)
	{
		base.RenderChildren(renderer);
	}

	public override object Clone()
	{
		return (SvgFilter)MemberwiseClone();
	}

	private Matrix GetTransform(SvgVisualElement element)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		Matrix val = new Matrix();
		foreach (SvgTransform transform in element.Transforms)
		{
			val.Multiply(transform.Matrix(0f, 0f));
		}
		return val;
	}

	private RectangleF GetPathBounds(SvgVisualElement element, ISvgRenderer renderer, Matrix transform)
	{
		RectangleF bounds = element.Path(renderer).GetBounds();
		PointF[] array = new PointF[2]
		{
			bounds.Location,
			new PointF(bounds.Right, bounds.Bottom)
		};
		transform.TransformPoints(array);
		return new RectangleF(Math.Min(array[0].X, array[1].X), Math.Min(array[0].Y, array[1].Y), Math.Abs(array[0].X - array[1].X), Math.Abs(array[0].Y - array[1].Y));
	}

	public void ApplyFilter(SvgVisualElement element, ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		float num = 0.5f;
		Matrix transform = GetTransform(element);
		RectangleF pathBounds = GetPathBounds(element, renderer, transform);
		if (pathBounds.Width == 0f || pathBounds.Height == 0f)
		{
			return;
		}
		ImageBuffer imageBuffer = new ImageBuffer(pathBounds, num, renderer, transform, renderMethod);
		IEnumerable<SvgFilterPrimitive> enumerable = Children.OfType<SvgFilterPrimitive>();
		if (enumerable.Count() <= 0)
		{
			return;
		}
		foreach (SvgFilterPrimitive item in enumerable)
		{
			item.Process(imageBuffer);
		}
		Bitmap buffer = imageBuffer.Buffer;
		RectangleF rectangleF = RectangleF.Inflate(pathBounds, num * pathBounds.Width, num * pathBounds.Height);
		Region clip = renderer.GetClip();
		renderer.SetClip(new Region(rectangleF), (CombineMode)0);
		renderer.DrawImage((Image)(object)buffer, rectangleF, new RectangleF(pathBounds.X, pathBounds.Y, rectangleF.Width, rectangleF.Height), (GraphicsUnit)2);
		renderer.SetClip(clip, (CombineMode)0);
	}
}
