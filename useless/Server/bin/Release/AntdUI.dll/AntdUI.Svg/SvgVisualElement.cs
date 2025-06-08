using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using AntdUI.Svg.FilterEffects;

namespace AntdUI.Svg;

public abstract class SvgVisualElement : SvgElement, ISvgBoundable, ISvgStylable, ISvgClipable
{
	private bool? _requiresSmoothRendering;

	private Region _previousClip;

	PointF ISvgBoundable.Location => Bounds.Location;

	SizeF ISvgBoundable.Size => Bounds.Size;

	public abstract RectangleF Bounds { get; }

	[SvgAttribute("clip")]
	public virtual string Clip
	{
		get
		{
			return Attributes.GetInheritedAttribute<string>("clip");
		}
		set
		{
			Attributes["clip"] = value;
		}
	}

	[SvgAttribute("clip-path")]
	public virtual Uri ClipPath
	{
		get
		{
			return Attributes.GetAttribute<Uri>("clip-path");
		}
		set
		{
			Attributes["clip-path"] = value;
		}
	}

	[SvgAttribute("clip-rule")]
	public SvgClipRule ClipRule
	{
		get
		{
			return Attributes.GetAttribute("clip-rule", SvgClipRule.NonZero);
		}
		set
		{
			Attributes["clip-rule"] = value;
		}
	}

	[SvgAttribute("filter")]
	public virtual Uri Filter
	{
		get
		{
			return Attributes.GetInheritedAttribute<Uri>("filter");
		}
		set
		{
			Attributes["filter"] = value;
		}
	}

	protected virtual bool RequiresSmoothRendering
	{
		get
		{
			if (!_requiresSmoothRendering.HasValue)
			{
				_requiresSmoothRendering = ConvertShapeRendering2AntiAlias(ShapeRendering);
			}
			return _requiresSmoothRendering.Value;
		}
	}

	protected virtual bool Renderable => true;

	[SvgAttribute("visibility")]
	public virtual bool Visible
	{
		get
		{
			if (Attributes["visibility"] != null)
			{
				return (bool)Attributes["visibility"];
			}
			return true;
		}
		set
		{
			Attributes["visibility"] = value;
		}
	}

	[SvgAttribute("display")]
	public virtual string Display
	{
		get
		{
			return Attributes["display"] as string;
		}
		set
		{
			Attributes["display"] = value;
		}
	}

	protected virtual bool Displayable
	{
		get
		{
			string text = Attributes["display"] as string;
			if (!string.IsNullOrEmpty(text) && text == "none")
			{
				return false;
			}
			return true;
		}
	}

	[SvgAttribute("enable-background")]
	public virtual string EnableBackground
	{
		get
		{
			return Attributes["enable-background"] as string;
		}
		set
		{
			Attributes["enable-background"] = value;
		}
	}

	public abstract GraphicsPath Path(ISvgRenderer renderer);

	private bool ConvertShapeRendering2AntiAlias(SvgShapeRendering shapeRendering)
	{
		if ((uint)(shapeRendering - 2) <= 2u)
		{
			return false;
		}
		return true;
	}

	public SvgVisualElement()
	{
		IsPathDirty = true;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		Render(renderer, renderFilter: true);
	}

	private void Render(ISvgRenderer renderer, bool renderFilter)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		if (!Visible || !Displayable || !PushTransforms(renderer) || (Renderable && Path(renderer) == null) || (renderFilter && RenderFilter(renderer)))
		{
			return;
		}
		SetClip(renderer);
		if (Renderable)
		{
			float num = Math.Min(Math.Max(Opacity, 0f), 1f);
			if (num == 1f)
			{
				RenderFillAndStroke(renderer);
			}
			else
			{
				IsPathDirty = true;
				RectangleF bounds = Bounds;
				IsPathDirty = true;
				Bitmap val = new Bitmap((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
				try
				{
					using (ISvgRenderer svgRenderer = SvgRenderer.FromImage((Image)(object)val))
					{
						svgRenderer.SetBoundable(renderer.GetBoundable());
						svgRenderer.TranslateTransform(0f - bounds.X, 0f - bounds.Y, (MatrixOrder)1);
						RenderFillAndStroke(svgRenderer);
					}
					renderer.DrawImage(srcRect: new RectangleF(0f, 0f, bounds.Width, bounds.Height), image: (Image)(object)val, destRect: bounds, graphicsUnit: (GraphicsUnit)2, opacity: num);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		else
		{
			base.RenderChildren(renderer);
		}
		ResetClip(renderer);
		PopTransforms(renderer);
	}

	private bool RenderFilter(ISvgRenderer renderer)
	{
		bool result = false;
		Uri uri = Filter;
		if (uri != null)
		{
			if (uri.ToString().StartsWith("url("))
			{
				uri = new Uri(uri.ToString().Substring(4, uri.ToString().Length - 5), UriKind.RelativeOrAbsolute);
			}
			SvgElement elementById = OwnerDocument.IdManager.GetElementById(uri);
			if (elementById is SvgFilter)
			{
				PopTransforms(renderer);
				try
				{
					((SvgFilter)elementById).ApplyFilter(this, renderer, delegate(ISvgRenderer r)
					{
						Render(r, renderFilter: false);
					});
				}
				catch
				{
				}
				result = true;
			}
		}
		return result;
	}

	protected void RenderFillAndStroke(ISvgRenderer renderer)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		if (RequiresSmoothRendering)
		{
			renderer.SmoothingMode = (SmoothingMode)4;
		}
		RenderFill(renderer);
		RenderStroke(renderer);
		if (RequiresSmoothRendering && (int)renderer.SmoothingMode == 4)
		{
			renderer.SmoothingMode = (SmoothingMode)0;
		}
	}

	protected internal virtual void RenderFill(ISvgRenderer renderer)
	{
		if (Fill == null)
		{
			return;
		}
		Brush brush = Fill.GetBrush(this, renderer, Math.Min(Math.Max(FillOpacity, 0f), 1f));
		try
		{
			if (brush != null)
			{
				Path(renderer).FillMode = (FillMode)(FillRule == SvgFillRule.NonZero);
				renderer.FillPath(brush, Path(renderer));
			}
		}
		finally
		{
			((IDisposable)brush)?.Dispose();
		}
	}

	protected internal virtual bool RenderStroke(ISvgRenderer renderer)
	{
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		ISvgRenderer renderer2 = renderer;
		if (Stroke != null && Stroke != SvgPaintServer.None && (float)StrokeWidth > 0f)
		{
			float strokeWidth = StrokeWidth.ToDeviceValue(renderer2, UnitRenderingType.Other, this);
			Brush brush = Stroke.GetBrush(this, renderer2, SvgElement.FixOpacityValue(StrokeOpacity), forStroke: true);
			try
			{
				if (brush != null)
				{
					GraphicsPath val = Path(renderer2);
					RectangleF bounds = val.GetBounds();
					if (val.PointCount < 1)
					{
						return false;
					}
					if (!(bounds.Width <= 0f) || !(bounds.Height <= 0f))
					{
						Pen val2 = new Pen(brush, strokeWidth);
						try
						{
							if (StrokeDashArray != null && StrokeDashArray.Count > 0)
							{
								if (StrokeDashArray.Count % 2 != 0)
								{
									StrokeDashArray.AddRange(StrokeDashArray);
								}
								val2.DashPattern = StrokeDashArray.ConvertAll((SvgUnit u) => ((u.ToDeviceValue(renderer2, UnitRenderingType.Other, this) <= 0f) ? 1f : u.ToDeviceValue(renderer2, UnitRenderingType.Other, this)) / ((strokeWidth <= 0f) ? 1f : strokeWidth)).ToArray();
								if (StrokeLineCap == SvgStrokeLineCap.Round)
								{
									float[] array = new float[val2.DashPattern.Length];
									int num = 1;
									for (int i = 0; i < val2.DashPattern.Length; i++)
									{
										array[i] = val2.DashPattern[i] + (float)num;
										num *= -1;
									}
									val2.DashPattern = array;
									val2.DashCap = (DashCap)2;
								}
								_ = StrokeDashOffset;
								if (StrokeDashOffset.Value != 0f)
								{
									val2.DashOffset = ((StrokeDashOffset.ToDeviceValue(renderer2, UnitRenderingType.Other, this) <= 0f) ? 1f : StrokeDashOffset.ToDeviceValue(renderer2, UnitRenderingType.Other, this)) / ((strokeWidth <= 0f) ? 1f : strokeWidth);
								}
							}
							switch (StrokeLineJoin)
							{
							case SvgStrokeLineJoin.Bevel:
								val2.LineJoin = (LineJoin)1;
								break;
							case SvgStrokeLineJoin.Round:
								val2.LineJoin = (LineJoin)2;
								break;
							case SvgStrokeLineJoin.MiterClip:
								val2.LineJoin = (LineJoin)3;
								break;
							default:
								val2.LineJoin = (LineJoin)0;
								break;
							}
							val2.MiterLimit = StrokeMiterLimit;
							switch (StrokeLineCap)
							{
							case SvgStrokeLineCap.Round:
								val2.StartCap = (LineCap)2;
								val2.EndCap = (LineCap)2;
								break;
							case SvgStrokeLineCap.Square:
								val2.StartCap = (LineCap)1;
								val2.EndCap = (LineCap)1;
								break;
							}
							renderer2.DrawPath(val2, val);
							return true;
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
					switch (StrokeLineCap)
					{
					case SvgStrokeLineCap.Round:
					{
						GraphicsPath val4 = new GraphicsPath();
						try
						{
							val4.AddEllipse(val.PathPoints[0].X - strokeWidth / 2f, val.PathPoints[0].Y - strokeWidth / 2f, strokeWidth, strokeWidth);
							renderer2.FillPath(brush, val4);
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
						break;
					}
					case SvgStrokeLineCap.Square:
					{
						GraphicsPath val3 = new GraphicsPath();
						try
						{
							val3.AddRectangle(new RectangleF(val.PathPoints[0].X - strokeWidth / 2f, val.PathPoints[0].Y - strokeWidth / 2f, strokeWidth, strokeWidth));
							renderer2.FillPath(brush, val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
						break;
					}
					}
				}
			}
			finally
			{
				((IDisposable)brush)?.Dispose();
			}
		}
		return false;
	}

	protected internal virtual void SetClip(ISvgRenderer renderer)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		if (!(ClipPath != null) && string.IsNullOrEmpty(Clip))
		{
			return;
		}
		_previousClip = renderer.GetClip();
		if (ClipPath != null)
		{
			SvgClipPath elementById = OwnerDocument.GetElementById<SvgClipPath>(ClipPath.ToString());
			if (elementById != null)
			{
				renderer.SetClip(elementById.GetClipRegion(this), (CombineMode)1);
			}
		}
		string clip = Clip;
		if (!string.IsNullOrEmpty(clip) && clip.StartsWith("rect("))
		{
			clip = clip.Trim();
			List<float> list = (from o in clip.Substring(5, clip.Length - 6).Split(new char[1] { ',' })
				select float.Parse(o.Trim())).ToList();
			RectangleF bounds = Bounds;
			RectangleF rectangleF = new RectangleF(bounds.Left + list[3], bounds.Top + list[0], bounds.Width - (list[3] + list[1]), bounds.Height - (list[2] + list[0]));
			renderer.SetClip(new Region(rectangleF), (CombineMode)1);
		}
	}

	protected internal virtual void ResetClip(ISvgRenderer renderer)
	{
		if (_previousClip != null)
		{
			renderer.SetClip(_previousClip, (CombineMode)0);
			_previousClip = null;
		}
	}

	void ISvgClipable.SetClip(ISvgRenderer renderer)
	{
		SetClip(renderer);
	}

	void ISvgClipable.ResetClip(ISvgRenderer renderer)
	{
		ResetClip(renderer);
	}
}
