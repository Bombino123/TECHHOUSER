using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AntdUI.Svg;

public abstract class SvgTextBase : SvgVisualElement
{
	private class FontBoundable : ISvgBoundable
	{
		private IFontDefn _font;

		private float _width = 1f;

		public PointF Location => PointF.Empty;

		public SizeF Size => new SizeF(_width, _font.Size);

		public RectangleF Bounds => new RectangleF(Location, Size);

		public FontBoundable(IFontDefn font)
		{
			_font = font;
		}

		public FontBoundable(IFontDefn font, float width)
		{
			_font = font;
			_width = width;
		}
	}

	private class TextDrawingState
	{
		private float _xAnchor = float.MinValue;

		private IList<GraphicsPath> _anchoredPaths = new List<GraphicsPath>();

		private GraphicsPath _currPath;

		private GraphicsPath _finalPath;

		private float _authorPathLength;

		public GraphicsPath BaselinePath { get; set; }

		public PointF Current { get; set; }

		public RectangleF TextBounds { get; set; }

		public SvgTextBase Element { get; set; }

		public float LetterSpacingAdjust { get; set; }

		public int NumChars { get; set; }

		public TextDrawingState Parent { get; set; }

		public ISvgRenderer Renderer { get; set; }

		public float StartOffsetAdjust { get; set; }

		private TextDrawingState()
		{
		}

		public TextDrawingState(ISvgRenderer renderer, SvgTextBase element)
		{
			Element = element;
			Renderer = renderer;
			Current = PointF.Empty;
			TextBounds = RectangleF.Empty;
			_xAnchor = 0f;
			BaselinePath = element.GetBaselinePath(renderer);
			_authorPathLength = element.GetAuthorPathLength();
		}

		public TextDrawingState(TextDrawingState parent, SvgTextBase element)
		{
			Element = element;
			Renderer = parent.Renderer;
			Parent = parent;
			Current = parent.Current;
			TextBounds = parent.TextBounds;
			BaselinePath = element.GetBaselinePath(parent.Renderer) ?? parent.BaselinePath;
			float authorPathLength = element.GetAuthorPathLength();
			_authorPathLength = ((authorPathLength == 0f) ? parent._authorPathLength : authorPathLength);
		}

		public GraphicsPath GetPath()
		{
			FlushPath();
			return _finalPath;
		}

		public TextDrawingState Clone()
		{
			return new TextDrawingState
			{
				_anchoredPaths = _anchoredPaths.ToList(),
				BaselinePath = BaselinePath,
				_xAnchor = _xAnchor,
				Current = Current,
				TextBounds = TextBounds,
				Element = Element,
				NumChars = NumChars,
				Parent = Parent,
				Renderer = Renderer
			};
		}

		public void DrawString(string value)
		{
			IList<float> values = GetValues(value.Length, (SvgTextBase e) => e._x, UnitRenderingType.HorizontalOffset);
			IList<float> values2 = GetValues(value.Length, (SvgTextBase e) => e._y, UnitRenderingType.VerticalOffset);
			using IFontDefn fontDefn = Element.GetFont(Renderer);
			float fontBaselineHeight = fontDefn.Ascent(Renderer);
			PathStatistics pathStatistics = null;
			double num = 1.0;
			if (BaselinePath != null)
			{
				pathStatistics = new PathStatistics(BaselinePath.PathData);
				if (_authorPathLength > 0f)
				{
					num = (double)_authorPathLength / pathStatistics.TotalLength;
				}
			}
			float num2 = 0f;
			IList<float> values3;
			IList<float> values4;
			IList<float> values5;
			try
			{
				Renderer.SetBoundable(new FontBoundable(fontDefn, (float)(pathStatistics?.TotalLength ?? 1.0)));
				values3 = GetValues(value.Length, (SvgTextBase e) => e._dx, UnitRenderingType.Horizontal);
				values4 = GetValues(value.Length, (SvgTextBase e) => e._dy, UnitRenderingType.Vertical);
				if (StartOffsetAdjust != 0f)
				{
					if (values3.Count < 1)
					{
						values3.Add(StartOffsetAdjust);
					}
					else
					{
						values3[0] += StartOffsetAdjust;
					}
				}
				if (Element.LetterSpacing.Value != 0f || Element.WordSpacing.Value != 0f || LetterSpacingAdjust != 0f)
				{
					float num3 = Element.LetterSpacing.ToDeviceValue(Renderer, UnitRenderingType.Horizontal, Element) + LetterSpacingAdjust;
					float num4 = Element.WordSpacing.ToDeviceValue(Renderer, UnitRenderingType.Horizontal, Element);
					if (Parent == null && NumChars == 0 && values3.Count < 1)
					{
						values3.Add(0f);
					}
					for (int i = ((Parent == null && NumChars == 0) ? 1 : 0); i < value.Length; i++)
					{
						if (i >= values3.Count)
						{
							values3.Add(num3 + (char.IsWhiteSpace(value[i]) ? num4 : 0f));
						}
						else
						{
							values3[i] += num3 + (char.IsWhiteSpace(value[i]) ? num4 : 0f);
						}
					}
				}
				values5 = GetValues(value.Length, (SvgTextBase e) => e._rotations);
				string attribute = Element.Attributes.GetAttribute<string>("baseline-shift");
				if (attribute != null && (attribute == null || attribute.Length != 0))
				{
					switch (attribute)
					{
					case "sub":
						num2 = new SvgUnit(SvgUnitType.Ex, 1f).ToDeviceValue(Renderer, UnitRenderingType.Vertical, Element);
						break;
					case "super":
						num2 = -1f * new SvgUnit(SvgUnitType.Ex, 1f).ToDeviceValue(Renderer, UnitRenderingType.Vertical, Element);
						break;
					default:
						num2 = -1f * SvgUnitConverter.Parse(attribute).ToDeviceValue(Renderer, UnitRenderingType.Vertical, Element);
						break;
					case "baseline":
					case "inherit":
						break;
					}
				}
				if (num2 != 0f)
				{
					if (values4.Any())
					{
						values4[0] += num2;
					}
					else
					{
						values4.Add(num2);
					}
				}
			}
			finally
			{
				Renderer.PopBoundable();
			}
			float num5 = Current.X;
			float num6 = Current.Y;
			for (int j = 0; j < values.Count - 1; j++)
			{
				FlushPath();
				_xAnchor = values[j] + ((values3.Count > j) ? values3[j] : 0f);
				EnsurePath();
				num6 = ((values2.Count > j) ? values2[j] : num6) + ((values4.Count > j) ? values4[j] : 0f);
				num5 = (num5.Equals(Current.X) ? _xAnchor : num5);
				DrawStringOnCurrPath(value[j].ToString(), fontDefn, new PointF(_xAnchor, num6), fontBaselineHeight, (values5.Count > j) ? values5[j] : values5.LastOrDefault());
			}
			int num7 = 0;
			float num8 = Current.X;
			if (values.Any())
			{
				FlushPath();
				num7 = values.Count - 1;
				num8 = (_xAnchor = values.Last());
			}
			EnsurePath();
			int num9 = num7 + Math.Max(Math.Max(Math.Max(Math.Max(values3.Count, values4.Count), values2.Count), values5.Count) - num7 - 1, 0);
			if (values5.LastOrDefault() != 0f || pathStatistics != null)
			{
				num9 = value.Length;
			}
			if (num9 > num7)
			{
				IList<RectangleF> list = fontDefn.MeasureCharacters(Renderer, value.Substring(num7, Math.Min(num9 + 1, value.Length) - num7));
				for (int k = num7; k < num9; k++)
				{
					num8 += (float)num * ((values3.Count > k) ? values3[k] : 0f) + (list[k - num7].X - ((k == num7) ? 0f : list[k - num7 - 1].X));
					num6 = ((values2.Count > k) ? values2[k] : num6) + ((values4.Count > k) ? values4[k] : 0f);
					if (pathStatistics == null)
					{
						num5 = (num5.Equals(Current.X) ? num8 : num5);
						DrawStringOnCurrPath(value[k].ToString(), fontDefn, new PointF(num8, num6), fontBaselineHeight, (values5.Count > k) ? values5[k] : values5.LastOrDefault());
						continue;
					}
					num8 = Math.Max(num8, 0f);
					float num10 = list[k - num7].Width / 2f;
					if (pathStatistics.OffsetOnPath(num8 + num10))
					{
						pathStatistics.LocationAngleAtOffset(num8 + num10, out var point, out var angle);
						point = new PointF((float)((double)point.X - (double)num10 * Math.Cos((double)angle * Math.PI / 180.0) - (double)((float)num * num6) * Math.Sin((double)angle * Math.PI / 180.0)), (float)((double)point.Y - (double)num10 * Math.Sin((double)angle * Math.PI / 180.0) + (double)((float)num * num6) * Math.Cos((double)angle * Math.PI / 180.0)));
						num5 = (num5.Equals(Current.X) ? point.X : num5);
						DrawStringOnCurrPath(value[k].ToString(), fontDefn, point, fontBaselineHeight, angle);
					}
				}
				num8 = ((num9 >= value.Length) ? (num8 + list.Last().Width) : (num8 + (list[list.Count - 1].X - list[list.Count - 2].X)));
			}
			if (num9 < value.Length)
			{
				num8 += ((values3.Count > num9) ? values3[num9] : 0f);
				num6 = ((values2.Count > num9) ? values2[num9] : num6) + ((values4.Count > num9) ? values4[num9] : 0f);
				num5 = (num5.Equals(Current.X) ? num8 : num5);
				DrawStringOnCurrPath(value.Substring(num9), fontDefn, new PointF(num8, num6), fontBaselineHeight, values5.LastOrDefault());
				num8 += fontDefn.MeasureString(Renderer, value.Substring(num9)).Width;
			}
			NumChars += value.Length;
			Current = new PointF(num8, num6 - num2);
			TextBounds = new RectangleF(num5, 0f, Current.X - num5, 0f);
		}

		private void DrawStringOnCurrPath(string value, IFontDefn font, PointF location, float fontBaselineHeight, float rotation)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			GraphicsPath val = _currPath;
			if (rotation != 0f)
			{
				val = new GraphicsPath();
			}
			font.AddStringToPath(Renderer, val, value, new PointF(location.X, location.Y - fontBaselineHeight));
			if (rotation != 0f && val.PointCount > 0)
			{
				Matrix val2 = new Matrix();
				try
				{
					val2.Translate(-1f * location.X, -1f * location.Y, (MatrixOrder)1);
					val2.Rotate(rotation, (MatrixOrder)1);
					val2.Translate(location.X, location.Y, (MatrixOrder)1);
					val.Transform(val2);
					_currPath.AddPath(val, false);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}

		private void EnsurePath()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (_currPath == null)
			{
				_currPath = new GraphicsPath();
				_currPath.StartFigure();
				TextDrawingState textDrawingState = this;
				while (textDrawingState != null && textDrawingState._xAnchor <= float.MinValue)
				{
					textDrawingState = textDrawingState.Parent;
				}
				textDrawingState._anchoredPaths.Add(_currPath);
			}
		}

		private void FlushPath()
		{
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Expected O, but got Unknown
			if (_currPath == null)
			{
				return;
			}
			_currPath.CloseFigure();
			if (_currPath.PointCount < 1)
			{
				_anchoredPaths.Clear();
				_xAnchor = float.MinValue;
				_currPath = null;
				return;
			}
			if (_xAnchor > float.MinValue)
			{
				float num = float.MaxValue;
				float num2 = float.MinValue;
				foreach (GraphicsPath anchoredPath in _anchoredPaths)
				{
					RectangleF bounds = anchoredPath.GetBounds();
					if (bounds.Left < num)
					{
						num = bounds.Left;
					}
					if (bounds.Right > num2)
					{
						num2 = bounds.Right;
					}
				}
				float num3 = 0f;
				switch (Element.TextAnchor)
				{
				case SvgTextAnchor.Middle:
					num3 = ((_anchoredPaths.Count() != 1) ? (num3 - (num2 - num) / 2f) : (num3 - TextBounds.Width / 2f));
					break;
				case SvgTextAnchor.End:
					num3 = ((_anchoredPaths.Count() != 1) ? (num3 - (num2 - num)) : (num3 - TextBounds.Width));
					break;
				}
				if (num3 != 0f)
				{
					Matrix val = new Matrix();
					try
					{
						val.Translate(num3, 0f);
						foreach (GraphicsPath anchoredPath2 in _anchoredPaths)
						{
							anchoredPath2.Transform(val);
						}
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				_anchoredPaths.Clear();
				_xAnchor = float.MinValue;
			}
			if (_finalPath == null)
			{
				_finalPath = _currPath;
			}
			else
			{
				_finalPath.AddPath(_currPath, false);
			}
			_currPath = null;
		}

		private IList<float> GetValues(int maxCount, Func<SvgTextBase, IEnumerable<float>> listGetter)
		{
			TextDrawingState textDrawingState = this;
			int num = 0;
			List<float> list = new List<float>();
			int num2 = 0;
			while (textDrawingState != null)
			{
				num += textDrawingState.NumChars;
				list.AddRange(listGetter(textDrawingState.Element).Skip(num).Take(maxCount));
				if (list.Count > num2)
				{
					maxCount -= list.Count - num2;
					num += list.Count - num2;
					num2 = list.Count;
				}
				if (maxCount < 1)
				{
					return list;
				}
				textDrawingState = textDrawingState.Parent;
			}
			return list;
		}

		private IList<float> GetValues(int maxCount, Func<SvgTextBase, IEnumerable<SvgUnit>> listGetter, UnitRenderingType renderingType)
		{
			int num = 0;
			List<float> list = new List<float>();
			int num2 = 0;
			while (this != null)
			{
				num += NumChars;
				list.AddRange(from p in listGetter(Element).Skip(num).Take(maxCount)
					select p.ToDeviceValue(Renderer, renderingType, Element));
				if (list.Count > num2)
				{
					maxCount -= list.Count - num2;
					num += list.Count - num2;
					num2 = list.Count;
				}
				if (maxCount < 1)
				{
					return list;
				}
				this = Parent;
			}
			return list;
		}
	}

	protected SvgUnitCollection _x = new SvgUnitCollection();

	protected SvgUnitCollection _y = new SvgUnitCollection();

	protected SvgUnitCollection _dy = new SvgUnitCollection();

	protected SvgUnitCollection _dx = new SvgUnitCollection();

	private string _rotate;

	private List<float> _rotations = new List<float>();

	private GraphicsPath _path;

	private static readonly Regex MultipleSpaces = new Regex(" {2,}", RegexOptions.Compiled);

	public virtual string Text
	{
		get
		{
			return base.Content;
		}
		set
		{
			base.Nodes.Clear();
			Children.Clear();
			if (value != null)
			{
				base.Nodes.Add(new SvgContentNode
				{
					Content = value
				});
			}
			IsPathDirty = true;
			Content = value;
		}
	}

	public override XmlSpaceHandling SpaceHandling
	{
		get
		{
			return base.SpaceHandling;
		}
		set
		{
			base.SpaceHandling = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("x")]
	public virtual SvgUnitCollection X
	{
		get
		{
			return _x;
		}
		set
		{
			if (_x != value)
			{
				_x = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "x",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("dx")]
	public virtual SvgUnitCollection Dx
	{
		get
		{
			return _dx;
		}
		set
		{
			if (_dx != value)
			{
				_dx = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "dx",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("y")]
	public virtual SvgUnitCollection Y
	{
		get
		{
			return _y;
		}
		set
		{
			if (_y != value)
			{
				_y = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "y",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("dy")]
	public virtual SvgUnitCollection Dy
	{
		get
		{
			return _dy;
		}
		set
		{
			if (_dy != value)
			{
				_dy = value;
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "dy",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("rotate")]
	public virtual string Rotate
	{
		get
		{
			return _rotate;
		}
		set
		{
			if (_rotate != value)
			{
				_rotate = value;
				_rotations.Clear();
				_rotations.AddRange(from r in _rotate.Split(new char[5] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
					select float.Parse(r));
				IsPathDirty = true;
				OnAttributeChanged(new AttributeEventArgs
				{
					Attribute = "rotate",
					Value = value
				});
			}
		}
	}

	[SvgAttribute("textLength", true)]
	public virtual SvgUnit TextLength
	{
		get
		{
			if (Attributes["textLength"] != null)
			{
				return (SvgUnit)Attributes["textLength"];
			}
			return SvgUnit.None;
		}
		set
		{
			Attributes["textLength"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("lengthAdjust", true)]
	public virtual SvgTextLengthAdjust LengthAdjust
	{
		get
		{
			if (Attributes["lengthAdjust"] != null)
			{
				return (SvgTextLengthAdjust)Attributes["lengthAdjust"];
			}
			return SvgTextLengthAdjust.Spacing;
		}
		set
		{
			Attributes["lengthAdjust"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("letter-spacing", true)]
	public virtual SvgUnit LetterSpacing
	{
		get
		{
			if (Attributes["letter-spacing"] != null)
			{
				return (SvgUnit)Attributes["letter-spacing"];
			}
			return SvgUnit.None;
		}
		set
		{
			Attributes["letter-spacing"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("word-spacing", true)]
	public virtual SvgUnit WordSpacing
	{
		get
		{
			if (Attributes["word-spacing"] != null)
			{
				return (SvgUnit)Attributes["word-spacing"];
			}
			return SvgUnit.None;
		}
		set
		{
			Attributes["word-spacing"] = value;
			IsPathDirty = true;
		}
	}

	public override SvgPaintServer Fill
	{
		get
		{
			if (Attributes["fill"] != null)
			{
				return (SvgPaintServer)Attributes["fill"];
			}
			return new SvgColourServer(System.Drawing.Color.Black);
		}
		set
		{
			Attributes["fill"] = value;
		}
	}

	public override RectangleF Bounds
	{
		get
		{
			GraphicsPath val = Path(null);
			foreach (SvgVisualElement item in Children.OfType<SvgVisualElement>())
			{
				if (!(item is SvgTextSpan { Text: null }))
				{
					val.AddPath(item.Path(null), false);
				}
			}
			if (base.Transforms != null && base.Transforms.Count > 0)
			{
				val.Transform(base.Transforms.GetMatrix());
			}
			return val.GetBounds();
		}
	}

	[SvgAttribute("onchange")]
	public event EventHandler<StringArg> Change;

	public override string ToString()
	{
		return Text;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		if (Path(renderer) == null || !Visible || !Displayable)
		{
			return;
		}
		PushTransforms(renderer);
		SetClip(renderer);
		float num = Math.Min(Math.Max(Opacity, 0f), 1f);
		if (num == 1f)
		{
			RenderFillAndStroke(renderer);
			RenderChildren(renderer);
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
					RenderChildren(svgRenderer);
				}
				renderer.DrawImage(srcRect: new RectangleF(0f, 0f, bounds.Width, bounds.Height), image: (Image)(object)val, destRect: bounds, graphicsUnit: (GraphicsUnit)2, opacity: num);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		ResetClip(renderer);
		PopTransforms(renderer);
	}

	internal virtual IEnumerable<ISvgNode> GetContentNodes()
	{
		if (base.Nodes != null && base.Nodes.Count >= 1)
		{
			return base.Nodes;
		}
		return from o in Children.OfType<ISvgNode>()
			where !(o is ISvgDescriptiveElement)
			select o;
	}

	protected virtual GraphicsPath GetBaselinePath(ISvgRenderer renderer)
	{
		return null;
	}

	protected virtual float GetAuthorPathLength()
	{
		return 0f;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		IEnumerable<ISvgNode> source = from x in GetContentNodes()
			where x is SvgContentNode && string.IsNullOrEmpty(x.Content.Trim('\r', '\n', '\t'))
			select x;
		if (_path == null || IsPathDirty || source.Count() == 1)
		{
			renderer = renderer ?? SvgRenderer.FromNull();
			SetPath(new TextDrawingState(renderer, this));
		}
		return _path;
	}

	private void SetPath(TextDrawingState state)
	{
		SetPath(state, doMeasurements: true);
	}

	private void SetPath(TextDrawingState state, bool doMeasurements)
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		TextDrawingState textDrawingState = null;
		bool flag = state.BaselinePath != null && (TextAnchor == SvgTextAnchor.Middle || TextAnchor == SvgTextAnchor.End);
		if (doMeasurements)
		{
			if (TextLength != SvgUnit.None)
			{
				textDrawingState = state.Clone();
			}
			else if (flag)
			{
				textDrawingState = state.Clone();
				state.BaselinePath = null;
			}
		}
		foreach (ISvgNode contentNode in GetContentNodes())
		{
			if (!(contentNode is SvgTextBase svgTextBase))
			{
				if (!string.IsNullOrEmpty(contentNode.Content))
				{
					state.DrawString(PrepareText(contentNode.Content));
				}
			}
			else
			{
				TextDrawingState textDrawingState2 = new TextDrawingState(state, svgTextBase);
				svgTextBase.SetPath(textDrawingState2);
				state.NumChars += textDrawingState2.NumChars;
				state.Current = textDrawingState2.Current;
			}
		}
		GraphicsPath val = (GraphicsPath)(((object)state.GetPath()) ?? ((object)new GraphicsPath()));
		if (doMeasurements)
		{
			if (TextLength != SvgUnit.None)
			{
				float num = TextLength.ToDeviceValue(state.Renderer, UnitRenderingType.Horizontal, this);
				float width = state.TextBounds.Width;
				float num2 = width - num;
				if ((double)Math.Abs(num2) > 1.5)
				{
					if (LengthAdjust == SvgTextLengthAdjust.Spacing)
					{
						if (X.Count < 2)
						{
							textDrawingState.LetterSpacingAdjust = -1f * num2 / (float)(state.NumChars - textDrawingState.NumChars - 1);
							SetPath(textDrawingState, doMeasurements: false);
							return;
						}
					}
					else
					{
						Matrix val2 = new Matrix();
						try
						{
							val2.Translate(-1f * state.TextBounds.X, 0f, (MatrixOrder)1);
							val2.Scale(num / width, 1f, (MatrixOrder)1);
							val2.Translate(state.TextBounds.X, 0f, (MatrixOrder)1);
							val.Transform(val2);
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
				}
			}
			else if (flag)
			{
				RectangleF bounds = val.GetBounds();
				if (TextAnchor == SvgTextAnchor.Middle)
				{
					textDrawingState.StartOffsetAdjust = -1f * bounds.Width / 2f;
				}
				else
				{
					textDrawingState.StartOffsetAdjust = -1f * bounds.Width;
				}
				SetPath(textDrawingState, doMeasurements: false);
				return;
			}
		}
		_path = val;
		IsPathDirty = false;
	}

	protected string PrepareText(string value)
	{
		value = ApplyTransformation(value);
		if (SpaceHandling == XmlSpaceHandling.preserve)
		{
			return value.Replace('\t', ' ').Replace("\r\n", " ").Replace('\r', ' ')
				.Replace('\n', ' ');
		}
		return MultipleSpaces.Replace(value.Replace("\r", "").Replace("\n", "").Replace('\t', ' '), " ");
	}

	private string ApplyTransformation(string value)
	{
		return TextTransformation switch
		{
			SvgTextTransformation.Capitalize => value.ToUpper(), 
			SvgTextTransformation.Uppercase => value.ToUpper(), 
			SvgTextTransformation.Lowercase => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value), 
			_ => value, 
		};
	}

	protected void OnChange(string newString, string sessionID)
	{
		RaiseChange(this, new StringArg
		{
			s = newString,
			SessionID = sessionID
		});
	}

	protected void RaiseChange(object sender, StringArg s)
	{
		this.Change?.Invoke(sender, s);
	}

	public override bool ShouldWriteElement()
	{
		if (!HasChildren())
		{
			return base.Nodes.Count > 0;
		}
		return true;
	}
}
