using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using AntdUI.Svg.Document_Structure;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

public abstract class SvgElement : ISvgElement, ISvgTransformable, ICloneable, ISvgNode
{
	private enum FontParseState
	{
		fontStyle,
		fontVariant,
		fontWeight,
		fontSize,
		fontFamilyNext,
		fontFamilyCurr
	}

	internal const int StyleSpecificity_PresAttribute = 0;

	internal const int StyleSpecificity_InlineStyle = 65536;

	internal SvgElement _parent;

	private string _elementName;

	private SvgAttributeCollection _attributes;

	private EventHandlerList _eventHandlers;

	private SvgElementCollection _children;

	private Region _graphicsClip;

	private Matrix _graphicsMatrix;

	private SvgCustomAttributeCollection _customAttributes;

	private List<ISvgNode> _nodes = new List<ISvgNode>();

	private Dictionary<string, SortedDictionary<int, string>> _styles = new Dictionary<string, SortedDictionary<int, string>>();

	private string _content;

	private bool _dirty;

	protected internal string ElementName
	{
		get
		{
			if (string.IsNullOrEmpty(_elementName))
			{
				_elementName = ClassName;
			}
			return _elementName;
		}
		internal set
		{
			_elementName = value;
		}
	}

	public virtual string ClassName => string.Empty;

	[SvgAttribute("color", true)]
	public virtual SvgPaintServer Color
	{
		get
		{
			if (Attributes["color"] != null)
			{
				return (SvgPaintServer)Attributes["color"];
			}
			return SvgColourServer.NotSet;
		}
		set
		{
			Attributes["color"] = value;
		}
	}

	public virtual string Content
	{
		get
		{
			return _content;
		}
		set
		{
			if (_content != null)
			{
				string content = _content;
				_content = value;
				if (_content != content)
				{
					OnContentChanged(new ContentEventArgs
					{
						Content = value
					});
				}
			}
			else
			{
				_content = value;
				OnContentChanged(new ContentEventArgs
				{
					Content = value
				});
			}
		}
	}

	protected virtual EventHandlerList Events => _eventHandlers;

	public virtual SvgElementCollection Children => _children;

	public IList<ISvgNode> Nodes => _nodes;

	public virtual SvgElement Parent => _parent;

	public IEnumerable<SvgElement> Parents
	{
		get
		{
			SvgElement curr = this;
			while (curr.Parent != null)
			{
				curr = curr.Parent;
				yield return curr;
			}
		}
	}

	public IEnumerable<SvgElement> ParentsAndSelf
	{
		get
		{
			SvgElement curr = this;
			yield return curr;
			while (curr.Parent != null)
			{
				curr = curr.Parent;
				yield return curr;
			}
		}
	}

	public virtual SvgDocument OwnerDocument
	{
		get
		{
			if (this is SvgDocument)
			{
				return this as SvgDocument;
			}
			if (Parent != null)
			{
				return Parent.OwnerDocument;
			}
			return null;
		}
	}

	protected internal virtual SvgAttributeCollection Attributes
	{
		get
		{
			if (_attributes == null)
			{
				_attributes = new SvgAttributeCollection(this);
			}
			return _attributes;
		}
	}

	public SvgCustomAttributeCollection CustomAttributes => _customAttributes;

	[SvgAttribute("transform")]
	public SvgTransformCollection Transforms
	{
		get
		{
			return Attributes.GetAttribute<SvgTransformCollection>("transform");
		}
		set
		{
			SvgTransformCollection transforms = Transforms;
			if (transforms != null)
			{
				transforms.TransformChanged -= Attributes_AttributeChanged;
			}
			value.TransformChanged += Attributes_AttributeChanged;
			Attributes["transform"] = value;
		}
	}

	[SvgAttribute("id")]
	public string ID
	{
		get
		{
			return Attributes.GetAttribute<string>("id");
		}
		set
		{
			SetAndForceUniqueID(value, autoForceUniqueID: false);
		}
	}

	[SvgAttribute("space", "http://www.w3.org/XML/1998/namespace")]
	public virtual XmlSpaceHandling SpaceHandling
	{
		get
		{
			if (Attributes["space"] != null)
			{
				return (XmlSpaceHandling)Attributes["space"];
			}
			return XmlSpaceHandling.@default;
		}
		set
		{
			Attributes["space"] = value;
		}
	}

	protected virtual bool IsPathDirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
		}
	}

	[SvgAttribute("fill", true)]
	public virtual SvgPaintServer Fill
	{
		get
		{
			return ((SvgPaintServer)Attributes["fill"]) ?? SvgColourServer.NotSet;
		}
		set
		{
			Attributes["fill"] = value;
		}
	}

	[SvgAttribute("stroke", true)]
	public virtual SvgPaintServer Stroke
	{
		get
		{
			return (SvgPaintServer)Attributes["stroke"];
		}
		set
		{
			Attributes["stroke"] = value;
		}
	}

	[SvgAttribute("fill-rule", true)]
	public virtual SvgFillRule FillRule
	{
		get
		{
			return (SvgFillRule)(Attributes["fill-rule"] ?? ((object)SvgFillRule.NonZero));
		}
		set
		{
			Attributes["fill-rule"] = value;
		}
	}

	[SvgAttribute("fill-opacity", true)]
	public virtual float FillOpacity
	{
		get
		{
			return (float)(Attributes["fill-opacity"] ?? ((object)1f));
		}
		set
		{
			Attributes["fill-opacity"] = FixOpacityValue(value);
		}
	}

	[SvgAttribute("stroke-width", true)]
	public virtual SvgUnit StrokeWidth
	{
		get
		{
			return (SvgUnit)(Attributes["stroke-width"] ?? ((object)new SvgUnit(1f)));
		}
		set
		{
			Attributes["stroke-width"] = value;
		}
	}

	[SvgAttribute("stroke-linecap", true)]
	public virtual SvgStrokeLineCap StrokeLineCap
	{
		get
		{
			return (SvgStrokeLineCap)(Attributes["stroke-linecap"] ?? ((object)SvgStrokeLineCap.Butt));
		}
		set
		{
			Attributes["stroke-linecap"] = value;
		}
	}

	[SvgAttribute("stroke-linejoin", true)]
	public virtual SvgStrokeLineJoin StrokeLineJoin
	{
		get
		{
			return (SvgStrokeLineJoin)(Attributes["stroke-linejoin"] ?? ((object)SvgStrokeLineJoin.Miter));
		}
		set
		{
			Attributes["stroke-linejoin"] = value;
		}
	}

	[SvgAttribute("stroke-miterlimit", true)]
	public virtual float StrokeMiterLimit
	{
		get
		{
			return (float)(Attributes["stroke-miterlimit"] ?? ((object)4f));
		}
		set
		{
			Attributes["stroke-miterlimit"] = value;
		}
	}

	[SvgAttribute("stroke-dasharray", true)]
	public virtual SvgUnitCollection StrokeDashArray
	{
		get
		{
			return Attributes["stroke-dasharray"] as SvgUnitCollection;
		}
		set
		{
			Attributes["stroke-dasharray"] = value;
		}
	}

	[SvgAttribute("stroke-dashoffset", true)]
	public virtual SvgUnit StrokeDashOffset
	{
		get
		{
			return (SvgUnit)(Attributes["stroke-dashoffset"] ?? ((object)SvgUnit.Empty));
		}
		set
		{
			Attributes["stroke-dashoffset"] = value;
		}
	}

	[SvgAttribute("stroke-opacity", true)]
	public virtual float StrokeOpacity
	{
		get
		{
			return (float)(Attributes["stroke-opacity"] ?? ((object)1f));
		}
		set
		{
			Attributes["stroke-opacity"] = FixOpacityValue(value);
		}
	}

	[SvgAttribute("stop-color", true)]
	public virtual SvgPaintServer StopColor
	{
		get
		{
			return Attributes["stop-color"] as SvgPaintServer;
		}
		set
		{
			Attributes["stop-color"] = value;
		}
	}

	[SvgAttribute("opacity", true)]
	public virtual float Opacity
	{
		get
		{
			return (float)(Attributes["opacity"] ?? ((object)1f));
		}
		set
		{
			Attributes["opacity"] = FixOpacityValue(value);
		}
	}

	[SvgAttribute("shape-rendering")]
	public virtual SvgShapeRendering ShapeRendering
	{
		get
		{
			return Attributes.GetInheritedAttribute<SvgShapeRendering>("shape-rendering");
		}
		set
		{
			Attributes["shape-rendering"] = value;
		}
	}

	[SvgAttribute("text-anchor", true)]
	public virtual SvgTextAnchor TextAnchor
	{
		get
		{
			return Attributes.GetInheritedAttribute<SvgTextAnchor>("text-anchor");
		}
		set
		{
			Attributes["text-anchor"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("baseline-shift", true)]
	public virtual string BaselineShift
	{
		get
		{
			return Attributes.GetInheritedAttribute<string>("baseline-shift");
		}
		set
		{
			Attributes["baseline-shift"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-family", true)]
	public virtual string FontFamily
	{
		get
		{
			return Attributes["font-family"] as string;
		}
		set
		{
			Attributes["font-family"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-size", true)]
	public virtual SvgUnit FontSize
	{
		get
		{
			return (SvgUnit)(Attributes["font-size"] ?? ((object)SvgUnit.Empty));
		}
		set
		{
			Attributes["font-size"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-style", true)]
	public virtual SvgFontStyle FontStyle
	{
		get
		{
			return (SvgFontStyle)(Attributes["font-style"] ?? ((object)SvgFontStyle.All));
		}
		set
		{
			Attributes["font-style"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-variant", true)]
	public virtual SvgFontVariant FontVariant
	{
		get
		{
			return (SvgFontVariant)(Attributes["font-variant"] ?? ((object)SvgFontVariant.Inherit));
		}
		set
		{
			Attributes["font-variant"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("text-decoration", true)]
	public virtual SvgTextDecoration TextDecoration
	{
		get
		{
			return (SvgTextDecoration)(Attributes["text-decoration"] ?? ((object)SvgTextDecoration.Inherit));
		}
		set
		{
			Attributes["text-decoration"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font-weight", true)]
	public virtual SvgFontWeight FontWeight
	{
		get
		{
			return (SvgFontWeight)(Attributes["font-weight"] ?? ((object)SvgFontWeight.Inherit));
		}
		set
		{
			Attributes["font-weight"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("text-transform", true)]
	public virtual SvgTextTransformation TextTransformation
	{
		get
		{
			return (SvgTextTransformation)(Attributes["text-transform"] ?? ((object)SvgTextTransformation.Inherit));
		}
		set
		{
			Attributes["text-transform"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("font", true)]
	public virtual string Font
	{
		get
		{
			return (Attributes["font"] ?? string.Empty) as string;
		}
		set
		{
			FontParseState fontParseState = FontParseState.fontStyle;
			string[] array = value.Split(new char[1] { ' ' });
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				bool flag = false;
				while (!flag)
				{
					switch (fontParseState)
					{
					case FontParseState.fontStyle:
					{
						flag = Enums.TryParse<SvgFontStyle>(text, out var result3);
						if (flag)
						{
							FontStyle = result3;
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontVariant:
					{
						flag = Enums.TryParse<SvgFontVariant>(text, out var result);
						if (flag)
						{
							FontVariant = result;
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontWeight:
					{
						flag = Enums.TryParse<SvgFontWeight>(text, out var result2);
						if (flag)
						{
							FontWeight = result2;
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontSize:
					{
						string[] array2 = text.Split(new char[1] { '/' });
						try
						{
							SvgUnit fontSize = SvgUnitConverter.Parse(array2[0]);
							flag = true;
							FontSize = fontSize;
						}
						catch
						{
						}
						fontParseState++;
						break;
					}
					case FontParseState.fontFamilyNext:
						fontParseState++;
						flag = true;
						break;
					}
				}
				switch (fontParseState)
				{
				case FontParseState.fontFamilyNext:
					FontFamily = string.Join(" ", array, i + 1, array.Length - (i + 1));
					i = 2147483645;
					break;
				case FontParseState.fontFamilyCurr:
					FontFamily = string.Join(" ", array, i, array.Length - i);
					i = 2147483645;
					break;
				}
			}
			Attributes["font"] = value;
			IsPathDirty = true;
		}
	}

	public event EventHandler<ChildAddedEventArgs> ChildAdded;

	public event EventHandler<ContentEventArgs> ContentChanged;

	public void AddStyle(string name, string value, int specificity)
	{
		if (!_styles.TryGetValue(name, out SortedDictionary<int, string> value2))
		{
			value2 = new SortedDictionary<int, string>();
			_styles[name] = value2;
		}
		while (value2.ContainsKey(specificity))
		{
			specificity++;
		}
		value2[specificity] = value;
	}

	public void FlushStyles()
	{
		if (!_styles.Any())
		{
			return;
		}
		Dictionary<string, SortedDictionary<int, string>> dictionary = new Dictionary<string, SortedDictionary<int, string>>();
		foreach (KeyValuePair<string, SortedDictionary<int, string>> style in _styles)
		{
			if (!SvgElementFactory.SetPropertyValue(this, style.Key, style.Value.Last().Value, OwnerDocument, isStyle: true))
			{
				dictionary.Add(style.Key, style.Value);
			}
		}
		_styles = dictionary;
	}

	public IEnumerable<SvgElement> Descendants()
	{
		return AsEnumerable().Descendants();
	}

	private IEnumerable<SvgElement> AsEnumerable()
	{
		yield return this;
	}

	public virtual bool HasChildren()
	{
		return Children.Count > 0;
	}

	protected internal virtual bool PushTransforms(ISvgRenderer renderer)
	{
		_graphicsMatrix = renderer.Transform;
		_graphicsClip = renderer.GetClip();
		if (Transforms == null || Transforms.Count == 0)
		{
			return true;
		}
		Matrix val = renderer.Transform.Clone();
		ISvgBoundable boundable = renderer.GetBoundable();
		foreach (SvgTransform transform in Transforms)
		{
			val.Multiply(transform.Matrix(boundable.Bounds.Width, boundable.Bounds.Height));
		}
		renderer.Transform = val;
		return true;
	}

	protected internal virtual void PopTransforms(ISvgRenderer renderer)
	{
		renderer.Transform = _graphicsMatrix;
		_graphicsMatrix = null;
		renderer.SetClip(_graphicsClip, (CombineMode)0);
		_graphicsClip = null;
	}

	void ISvgTransformable.PushTransforms(ISvgRenderer renderer)
	{
		PushTransforms(renderer);
	}

	void ISvgTransformable.PopTransforms(ISvgRenderer renderer)
	{
		PopTransforms(renderer);
	}

	protected RectangleF TransformedBounds(RectangleF bounds)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (Transforms != null && Transforms.Count > 0)
		{
			GraphicsPath val = new GraphicsPath();
			val.AddRectangle(bounds);
			val.Transform(Transforms.GetMatrix());
			return val.GetBounds();
		}
		return bounds;
	}

	public void SetAndForceUniqueID(string value, bool autoForceUniqueID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		if (string.Compare(ID, value) != 0)
		{
			if (OwnerDocument != null)
			{
				OwnerDocument.IdManager.Remove(this);
			}
			Attributes["id"] = value;
			if (OwnerDocument != null)
			{
				OwnerDocument.IdManager.AddAndForceUniqueID(this, null, autoForceUniqueID, logElementOldIDNewID);
			}
		}
	}

	internal void ForceUniqueID(string newID)
	{
		Attributes["id"] = newID;
	}

	protected virtual void AddElement(SvgElement child, int index)
	{
	}

	internal void OnElementAdded(SvgElement child, int index)
	{
		AddElement(child, index);
		SvgElement beforeSibling = null;
		if (index < Children.Count - 1)
		{
			beforeSibling = Children[index + 1];
		}
		this.ChildAdded?.Invoke(this, new ChildAddedEventArgs
		{
			NewChild = child,
			BeforeSibling = beforeSibling
		});
	}

	protected virtual void RemoveElement(SvgElement child)
	{
	}

	internal void OnElementRemoved(SvgElement child)
	{
		RemoveElement(child);
	}

	public SvgElement()
	{
		_children = new SvgElementCollection(this);
		_eventHandlers = new EventHandlerList();
		_customAttributes = new SvgCustomAttributeCollection(this);
		Transforms = new SvgTransformCollection();
		Attributes.AttributeChanged += Attributes_AttributeChanged;
		CustomAttributes.AttributeChanged += Attributes_AttributeChanged;
	}

	private void Attributes_AttributeChanged(object sender, AttributeEventArgs e)
	{
		OnAttributeChanged(e);
	}

	public void RenderElement(ISvgRenderer renderer)
	{
		Render(renderer);
	}

	public virtual bool ShouldWriteElement()
	{
		return ElementName != string.Empty;
	}

	protected virtual void Render(ISvgRenderer renderer)
	{
		PushTransforms(renderer);
		RenderChildren(renderer);
		PopTransforms(renderer);
	}

	protected virtual void RenderChildren(ISvgRenderer renderer)
	{
		foreach (SvgElement child in Children)
		{
			child.Render(renderer);
		}
	}

	void ISvgElement.Render(ISvgRenderer renderer)
	{
		Render(renderer);
	}

	protected void AddPaths(SvgElement elem, GraphicsPath path)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		foreach (SvgElement child in elem.Children)
		{
			if (child is SvgSymbol)
			{
				continue;
			}
			if (child is SvgVisualElement && !(child is SvgGroup))
			{
				GraphicsPath val = ((SvgVisualElement)child).Path(null);
				if (val != null)
				{
					val = (GraphicsPath)val.Clone();
					if (child.Transforms != null)
					{
						val.Transform(child.Transforms.GetMatrix());
					}
					if (val.PointCount > 0)
					{
						path.AddPath(val, false);
					}
				}
			}
			if (!(child is SvgPaintServer))
			{
				AddPaths(child, path);
			}
		}
	}

	protected GraphicsPath GetPaths(SvgElement elem, ISvgRenderer renderer)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		foreach (SvgElement child in elem.Children)
		{
			if (!(child is SvgVisualElement))
			{
				continue;
			}
			if (!(child is SvgGroup))
			{
				GraphicsPath val2 = ((SvgVisualElement)child).Path(renderer);
				if (child.Children.Count > 0)
				{
					val2.AddPath(GetPaths(child, renderer), false);
				}
				if (val2 != null && val2.PointCount > 0)
				{
					val2 = (GraphicsPath)val2.Clone();
					if (child.Transforms != null)
					{
						val2.Transform(child.Transforms.GetMatrix());
					}
					val.AddPath(val2, false);
				}
				continue;
			}
			GraphicsPath paths = GetPaths(child, renderer);
			if (paths != null && paths.PointCount > 0)
			{
				if (child.Transforms != null)
				{
					paths.Transform(child.Transforms.GetMatrix());
				}
				val.AddPath(paths, false);
			}
		}
		return val;
	}

	public virtual object Clone()
	{
		return MemberwiseClone();
	}

	protected void OnAttributeChanged(AttributeEventArgs args)
	{
	}

	protected void OnContentChanged(ContentEventArgs args)
	{
		this.ContentChanged?.Invoke(this, args);
	}

	public void InvalidateChildPaths()
	{
		IsPathDirty = true;
		foreach (SvgElement child in Children)
		{
			child.InvalidateChildPaths();
		}
	}

	protected static float FixOpacityValue(float value)
	{
		return Math.Min(Math.Max(value, 0f), 1f);
	}

	internal IFontDefn GetFont(ISvgRenderer renderer)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		SvgUnit fontSize = FontSize;
		float num = ((!(fontSize == SvgUnit.None) && !(fontSize == SvgUnit.Empty)) ? fontSize.ToDeviceValue(renderer, UnitRenderingType.Vertical, this) : ((float)new SvgUnit(SvgUnitType.Em, 1f)));
		object obj = ValidateFontFamily(FontFamily, OwnerDocument);
		if (!(obj is IEnumerable<SvgFontFace> source))
		{
			FontStyle val = (FontStyle)0;
			switch (FontWeight)
			{
			case SvgFontWeight.W600:
			case SvgFontWeight.Bold:
			case SvgFontWeight.W800:
			case SvgFontWeight.W900:
			case SvgFontWeight.Bolder:
				val = (FontStyle)(val | 1);
				break;
			}
			SvgFontStyle fontStyle = FontStyle;
			if (fontStyle == SvgFontStyle.Oblique || fontStyle == SvgFontStyle.Italic)
			{
				val = (FontStyle)(val | 2);
			}
			switch (TextDecoration)
			{
			case SvgTextDecoration.LineThrough:
				val = (FontStyle)(val | 8);
				break;
			case SvgTextDecoration.Underline:
				val = (FontStyle)(val | 4);
				break;
			}
			object obj2 = ((obj is FontFamily) ? obj : null);
			((FontFamily)obj2).IsStyleAvailable(val);
			return new GdiFontDefn(new Font((FontFamily)obj2, num, val, (GraphicsUnit)2));
		}
		SvgFont svgFont = source.First().Parent as SvgFont;
		if (svgFont == null)
		{
			Uri referencedElement = source.First().Descendants().OfType<SvgFontFaceUri>()
				.First()
				.ReferencedElement;
			svgFont = OwnerDocument.IdManager.GetElementById(referencedElement) as SvgFont;
		}
		return new SvgFontDefn(svgFont, num, SvgDocument.Ppi);
	}

	public static object ValidateFontFamily(string fontFamilyList, SvgDocument doc)
	{
		foreach (string item in from fontName in (fontFamilyList ?? string.Empty).Split(new char[1] { ',' })
			select fontName.Trim('"', ' ', '\''))
		{
			if (doc != null && doc.FontDefns().TryGetValue(item, out IEnumerable<SvgFontFace> value))
			{
				return value;
			}
			FontFamily val = SvgFontManager.FindFont(item);
			if (val != null)
			{
				return val;
			}
			switch (item.ToLower())
			{
			case "serif":
				return FontFamily.GenericSerif;
			case "sans-serif":
				return FontFamily.GenericSansSerif;
			case "monospace":
				return FontFamily.GenericMonospace;
			}
		}
		return FontFamily.GenericSansSerif;
	}
}
