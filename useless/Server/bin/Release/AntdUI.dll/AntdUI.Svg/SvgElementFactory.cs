using System;
using System.Xml;
using AntdUI.Svg.DataTypes;
using AntdUI.Svg.Document_Structure;
using AntdUI.Svg.FilterEffects;
using AntdUI.Svg.Transforms;

namespace AntdUI.Svg;

internal class SvgElementFactory
{
	public T CreateDocument<T>(XmlReader reader) where T : SvgDocument, new()
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "svg")
		{
			throw new InvalidOperationException("The CreateDocument method can only be used to parse root <svg> elements.");
		}
		return (T)CreateElement<T>(reader, fragmentIsDocument: true, null);
	}

	public SvgElement CreateElement(XmlReader reader, SvgDocument document)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return CreateElement<SvgDocument>(reader, fragmentIsDocument: false, document);
	}

	private SvgElement CreateElement<T>(XmlReader reader, bool fragmentIsDocument, SvgDocument document) where T : SvgDocument, new()
	{
		string localName = reader.LocalName;
		string namespaceURI = reader.NamespaceURI;
		SvgElement svgElement;
		if (namespaceURI == "http://www.w3.org/2000/svg" || string.IsNullOrEmpty(namespaceURI))
		{
			svgElement = ((localName == "svg") ? (fragmentIsDocument ? new T() : new SvgFragment()) : (localName switch
			{
				"circle" => new SvgCircle(), 
				"ellipse" => new SvgEllipse(), 
				"line" => new SvgLine(), 
				"polygon" => new SvgPolygon(), 
				"polyline" => new SvgPolyline(), 
				"rect" => new SvgRectangle(), 
				"clipPath" => new SvgClipPath(), 
				"defs" => new SvgDefinitionList(), 
				"desc" => new SvgDescription(), 
				"metadata" => new SvgDocumentMetadata(), 
				"g" => new SvgGroup(), 
				"switch" => new SvgSwitch(), 
				"title" => new SvgTitle(), 
				"use" => new SvgUse(), 
				"foreignObject" => new SvgForeignObject(), 
				"stop" => new SvgGradientStop(), 
				"linearGradient" => new SvgLinearGradientServer(), 
				"marker" => new SvgMarker(), 
				"pattern" => new SvgPatternServer(), 
				"radialGradient" => new SvgRadialGradientServer(), 
				"path" => new SvgPath(), 
				"font" => new SvgFont(), 
				"font-face" => new SvgFontFace(), 
				"font-face-src" => new SvgFontFaceSrc(), 
				"font-face-uri" => new SvgFontFaceUri(), 
				"glyph" => new SvgGlyph(), 
				"vkern" => new SvgVerticalKern(), 
				"hkern" => new SvgHorizontalKern(), 
				"missing-glyph" => new SvgMissingGlyph(), 
				"text" => new SvgText(), 
				"textPath" => new SvgTextPath(), 
				"tref" => new SvgTextRef(), 
				"tspan" => new SvgTextSpan(), 
				"feColorMatrix" => new SvgColourMatrix(), 
				"feGaussianBlur" => new SvgGaussianBlur(), 
				"feMerge" => new SvgMerge(), 
				"feMergeNode" => new SvgMergeNode(), 
				"feOffset" => new SvgOffset(), 
				"filter" => new SvgFilter(), 
				"symbol" => new SvgSymbol(), 
				_ => new SvgUnknownElement(localName), 
			}));
			if (svgElement != null)
			{
				SetAttributes(svgElement, reader, document);
			}
		}
		else
		{
			svgElement = new NonSvgElement(localName);
			SetAttributes(svgElement, reader, document);
		}
		return svgElement;
	}

	private void SetAttributes(SvgElement element, XmlReader reader, SvgDocument document)
	{
		while (reader.MoveToNextAttribute())
		{
			if (IsStyleAttribute(reader.LocalName))
			{
				element.AddStyle(reader.LocalName, reader.Value, 0);
			}
			else
			{
				SetPropertyValue(element, reader.LocalName, reader.Value, document);
			}
		}
	}

	private static bool IsStyleAttribute(string name)
	{
		switch (name)
		{
		case "baseline-shift":
		case "letter-spacing":
		case "lighting-color":
		case "pointer-events":
		case "stroke-linecap":
		case "stroke-opacity":
		case "text-rendering":
		case "text-transform":
		case "clip":
		case "font":
		case "mask":
		case "fill":
		case "font-size":
		case "direction":
		case "clip-path":
		case "clip-rule":
		case "fill-rule":
		case "color-profile":
		case "flood-opacity":
		case "image-rendering":
		case "color-rendering":
		case "stroke-linejoin":
		case "text-decoration":
		case "shape-rendering":
		case "marker":
		case "stroke":
		case "cursor":
		case "filter":
		case "display":
		case "kerning":
		case "opacity":
		case "stroke-miterlimit":
		case "dominant-baseline":
		case "enable-background":
		case "stroke-dashoffset":
		case "fill-opacity":
		case "font-stretch":
		case "font-variant":
		case "writing-mode":
		case "marker-start":
		case "stop-opacity":
		case "stroke-width":
		case "unicode-bidi":
		case "word-spacing":
		case "font-family":
		case "font-weight":
		case "flood-color":
		case "text-anchor":
		case "font-size-adjust":
		case "stroke-dasharray":
		case "font-style":
		case "marker-end":
		case "marker-mid":
		case "stop-color":
		case "visibility":
		case "alignment-baseline":
		case "color":
		case "color-interpolation":
		case "color-interpolation-filters":
		case "glyph-orientation-horizontal":
		case "glyph-orientation-vertical":
		case "overflow":
			return true;
		default:
			return false;
		}
	}

	internal static bool SetPropertyValue(SvgElement element, string attributeName, string attributeValue, SvgDocument document, bool isStyle = false)
	{
		try
		{
			if (element is SvgFragment svgFragment)
			{
				switch (attributeName)
				{
				case "viewBox":
					svgFragment.ViewBox = SvgViewBoxConverter.Parse(attributeValue);
					return true;
				case "x":
					svgFragment.X = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "y":
					svgFragment.Y = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "width":
					svgFragment.Width = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "height":
					svgFragment.Height = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "overflow":
					svgFragment.Overflow = (SvgOverflow)Enum.Parse(typeof(SvgOverflow), attributeValue, ignoreCase: true);
					return true;
				case "preserveAspectRatio":
					svgFragment.AspectRatio = SvgPreserveAspectRatioConverter.Parse(attributeValue);
					return true;
				case "focusable":
				case "data-icon":
				case "aria-hidden":
				case "xlink":
				case "xmlns":
				case "t":
				case "class":
				case "version":
				case "p-id":
					return SetPropertyValueNULL(element, attributeName, attributeValue);
				}
			}
			if (element is SvgTextBase svgTextBase)
			{
				if (attributeName == "x")
				{
					svgTextBase.X = SvgUnitCollectionConverter.Parse(attributeValue);
					return true;
				}
				if (attributeName == "y")
				{
					svgTextBase.Y = SvgUnitCollectionConverter.Parse(attributeValue);
					return true;
				}
				if (attributeName == "dx")
				{
					svgTextBase.Dx = SvgUnitCollectionConverter.Parse(attributeValue);
					return true;
				}
				switch (attributeName)
				{
				case "dx":
					svgTextBase.Dy = SvgUnitCollectionConverter.Parse(attributeValue);
					return true;
				case "rotate":
					svgTextBase.Rotate = attributeValue;
					return true;
				case "textLength":
					svgTextBase.TextLength = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "lengthAdjust":
					svgTextBase.LengthAdjust = (SvgTextLengthAdjust)Enum.Parse(typeof(SvgTextLengthAdjust), attributeValue, ignoreCase: true);
					return true;
				case "letter-spacing":
					svgTextBase.LetterSpacing = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "word-spacing":
					svgTextBase.WordSpacing = SvgUnitConverter.Parse(attributeValue);
					return true;
				case "display":
					svgTextBase.Display = attributeValue;
					return true;
				case "visibility":
					svgTextBase.Visible = bool.Parse(attributeValue);
					return true;
				}
			}
			if (element is SvgVisualElement svgVisualElement)
			{
				switch (attributeName)
				{
				case "display":
					svgVisualElement.Display = attributeValue;
					return true;
				case "visibility":
					svgVisualElement.Visible = bool.Parse(attributeValue);
					return true;
				case "enable-background":
					svgVisualElement.EnableBackground = attributeValue;
					return true;
				case "filter":
					svgVisualElement.Filter = new Uri(attributeValue, UriKind.RelativeOrAbsolute);
					return true;
				case "clip-path":
					svgVisualElement.ClipPath = new Uri(attributeValue, UriKind.RelativeOrAbsolute);
					return true;
				case "clip-rule":
					svgVisualElement.ClipRule = (SvgClipRule)Enum.Parse(typeof(SvgClipRule), attributeValue, ignoreCase: true);
					return true;
				}
			}
			switch (attributeName)
			{
			case "id":
				element.ID = attributeValue;
				return true;
			case "fill":
				element.Fill = SvgPaintServerConverter.Parse(attributeValue, document);
				return true;
			case "fill-rule":
				element.FillRule = (SvgFillRule)Enum.Parse(typeof(SvgFillRule), attributeValue, ignoreCase: true);
				return true;
			case "stroke":
				element.Stroke = SvgPaintServerConverter.Parse(attributeValue, document);
				return true;
			case "opacity":
				if (attributeValue == "undefined")
				{
					element.Opacity = 1f;
				}
				else
				{
					element.Opacity = float.Parse(attributeValue);
				}
				return true;
			case "fill-opacity":
				if (attributeValue == "undefined")
				{
					element.FillOpacity = 1f;
				}
				else
				{
					element.FillOpacity = float.Parse(attributeValue);
				}
				return true;
			case "stroke-width":
				element.StrokeWidth = SvgUnitConverter.Parse(attributeValue);
				return true;
			case "stroke-linecap":
				element.StrokeLineCap = (SvgStrokeLineCap)Enum.Parse(typeof(SvgStrokeLineCap), attributeValue, ignoreCase: true);
				return true;
			case "stroke-linejoin":
				try
				{
					element.StrokeLineJoin = (SvgStrokeLineJoin)Enum.Parse(typeof(SvgStrokeLineJoin), attributeValue, ignoreCase: true);
				}
				catch
				{
				}
				return true;
			case "stroke-miterlimit":
				element.StrokeMiterLimit = float.Parse(attributeValue);
				return true;
			case "stroke-dasharray":
				element.StrokeDashArray = SvgUnitCollectionConverter.Parse(attributeValue);
				return true;
			case "stroke-dashoffset":
				element.StrokeDashOffset = SvgUnitConverter.Parse(attributeValue);
				return true;
			case "stroke-opacity":
				if (attributeValue == "undefined")
				{
					element.StrokeOpacity = 1f;
				}
				else
				{
					element.StrokeOpacity = float.Parse(attributeValue);
				}
				return true;
			case "stop-color":
				element.StopColor = SvgPaintServerConverter.Parse(attributeValue, document);
				return true;
			case "shape-rendering":
				element.ShapeRendering = (SvgShapeRendering)Enum.Parse(typeof(SvgShapeRendering), attributeValue, ignoreCase: true);
				return true;
			case "text-anchor":
				element.TextAnchor = (SvgTextAnchor)Enum.Parse(typeof(SvgTextAnchor), attributeValue, ignoreCase: true);
				return true;
			case "transform":
				element.Transforms = SvgTransformConverter.Parse(attributeValue);
				return true;
			case "font-family":
				element.FontFamily = attributeValue;
				return true;
			case "font-size":
				element.FontSize = SvgUnitConverter.Parse(attributeValue);
				return true;
			case "font-style":
				element.FontStyle = (SvgFontStyle)Enum.Parse(typeof(SvgFontStyle), attributeValue, ignoreCase: true);
				return true;
			case "font-variant":
				element.FontVariant = (SvgFontVariant)Enum.Parse(typeof(SvgFontVariant), attributeValue, ignoreCase: true);
				return true;
			case "text-decoration":
				element.TextDecoration = (SvgTextDecoration)Enum.Parse(typeof(SvgTextDecoration), attributeValue, ignoreCase: true);
				return true;
			case "font-weight":
				element.FontWeight = (SvgFontWeight)Enum.Parse(typeof(SvgFontWeight), attributeValue, ignoreCase: true);
				return true;
			case "text-transform":
				element.TextTransformation = (SvgTextTransformation)Enum.Parse(typeof(SvgTextTransformation), attributeValue, ignoreCase: true);
				return true;
			default:
				if (element is SvgCircle svgCircle)
				{
					switch (attributeName)
					{
					case "cx":
						svgCircle.CenterX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "cy":
						svgCircle.CenterY = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "r":
						svgCircle.Radius = SvgUnitConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgEllipse svgEllipse)
				{
					switch (attributeName)
					{
					case "cx":
						svgEllipse.CenterX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "cy":
						svgEllipse.CenterY = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "rx":
						svgEllipse.RadiusX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "ry":
						svgEllipse.RadiusY = SvgUnitConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgLine svgLine)
				{
					switch (attributeName)
					{
					case "x1":
						svgLine.StartX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y1":
						svgLine.StartY = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "x2":
						svgLine.EndX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y2":
						svgLine.EndY = SvgUnitConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgPolygon svgPolygon)
				{
					if (attributeName == "points")
					{
						svgPolygon.Points = SvgPointCollectionConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgPolyline svgPolyline)
				{
					if (attributeName == "points")
					{
						svgPolyline.Points = SvgPointCollectionConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgRectangle svgRectangle)
				{
					switch (attributeName)
					{
					case "x":
						svgRectangle.X = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y":
						svgRectangle.Y = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "width":
						svgRectangle.Width = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "height":
						svgRectangle.Height = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "rx":
						svgRectangle.CornerRadiusX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "ry":
						svgRectangle.CornerRadiusY = SvgUnitConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgClipPath svgClipPath)
				{
					if (attributeName == "clipPathUnits")
					{
						svgClipPath.ClipPathUnits = (SvgCoordinateUnits)Enum.Parse(typeof(SvgCoordinateUnits), attributeValue, ignoreCase: true);
						return true;
					}
				}
				else if (element is SvgUse svgUse)
				{
					switch (attributeName)
					{
					case "x":
						svgUse.X = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y":
						svgUse.Y = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "width":
						svgUse.Width = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "height":
						svgUse.Height = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "href":
						svgUse.ReferencedElement = new Uri(attributeValue, UriKind.RelativeOrAbsolute);
						return true;
					}
				}
				else if (element is SvgGradientStop svgGradientStop)
				{
					switch (attributeName)
					{
					case "offset":
						svgGradientStop.Offset = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "stop-color":
						svgGradientStop.StopColor = SvgPaintServerConverter.Parse(attributeValue, document);
						return true;
					case "stop-opacity":
						svgGradientStop.Opacity = float.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgGradientServer svgGradientServer)
				{
					switch (attributeName)
					{
					case "spreadMethod":
						svgGradientServer.SpreadMethod = (SvgGradientSpreadMethod)Enum.Parse(typeof(SvgGradientSpreadMethod), attributeValue, ignoreCase: true);
						return true;
					case "gradientUnits":
						svgGradientServer.GradientUnits = (SvgCoordinateUnits)Enum.Parse(typeof(SvgCoordinateUnits), attributeValue, ignoreCase: true);
						return true;
					case "href":
						svgGradientServer.InheritGradient = SvgPaintServerConverter.Parse(attributeValue, document);
						return true;
					case "gradientTransform":
						svgGradientServer.GradientTransform = SvgTransformConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgMarker svgMarker)
				{
					switch (attributeName)
					{
					case "refX":
						svgMarker.RefX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "refY":
						svgMarker.RefY = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "orient":
						svgMarker.Orient = SvgOrientConverter.Parse(attributeValue);
						return true;
					case "overflow":
						svgMarker.Overflow = (SvgOverflow)Enum.Parse(typeof(SvgOverflow), attributeValue, ignoreCase: true);
						return true;
					case "viewBox":
						svgMarker.ViewBox = SvgViewBoxConverter.Parse(attributeValue);
						return true;
					case "preserveAspectRatio":
						svgMarker.AspectRatio = SvgPreserveAspectRatioConverter.Parse(attributeValue);
						return true;
					case "markerWidth":
						svgMarker.MarkerWidth = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "markerHeight":
						svgMarker.MarkerHeight = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "markerUnits":
						svgMarker.MarkerUnits = (SvgMarkerUnits)Enum.Parse(typeof(SvgMarkerUnits), attributeValue, ignoreCase: true);
						return true;
					}
				}
				else if (element is SvgPatternServer svgPatternServer)
				{
					switch (attributeName)
					{
					case "overflow":
						svgPatternServer.Overflow = (SvgOverflow)Enum.Parse(typeof(SvgOverflow), attributeValue, ignoreCase: true);
						return true;
					case "viewBox":
						svgPatternServer.ViewBox = SvgViewBoxConverter.Parse(attributeValue);
						return true;
					case "preserveAspectRatio":
						svgPatternServer.AspectRatio = SvgPreserveAspectRatioConverter.Parse(attributeValue);
						return true;
					case "x":
						svgPatternServer.X = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y":
						svgPatternServer.Y = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "width":
						svgPatternServer.Width = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "height":
						svgPatternServer.Height = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "patternUnits":
						svgPatternServer.PatternUnits = (SvgCoordinateUnits)Enum.Parse(typeof(SvgCoordinateUnits), attributeValue, ignoreCase: true);
						return true;
					case "patternContentUnits":
						svgPatternServer.PatternContentUnits = (SvgCoordinateUnits)Enum.Parse(typeof(SvgCoordinateUnits), attributeValue, ignoreCase: true);
						return true;
					case "href":
						svgPatternServer.InheritGradient = SvgPaintServerConverter.Parse(attributeValue, document);
						return true;
					case "patternTransform":
						svgPatternServer.PatternTransform = SvgTransformConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgLinearGradientServer svgLinearGradientServer)
				{
					switch (attributeName)
					{
					case "x1":
						svgLinearGradientServer.X1 = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y1":
						svgLinearGradientServer.Y1 = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "x2":
						svgLinearGradientServer.X2 = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y2":
						svgLinearGradientServer.Y2 = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "gradientTransform":
						svgLinearGradientServer.GradientTransform = SvgTransformConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgRadialGradientServer svgRadialGradientServer)
				{
					switch (attributeName)
					{
					case "cx":
						svgRadialGradientServer.CenterX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "cy":
						svgRadialGradientServer.CenterY = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "r":
						svgRadialGradientServer.Radius = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "fx":
						svgRadialGradientServer.FocalX = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "fy":
						svgRadialGradientServer.FocalY = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "gradientTransform":
						svgRadialGradientServer.GradientTransform = SvgTransformConverter.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgPath svgPath)
				{
					if (attributeName == "d")
					{
						svgPath.PathData = SvgPathConverter.Parse(attributeValue);
						return true;
					}
					if (attributeName == "pathLength")
					{
						svgPath.PathLength = float.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgFont svgFont)
				{
					switch (attributeName)
					{
					case "horiz-adv-x":
						svgFont.HorizAdvX = float.Parse(attributeValue);
						return true;
					case "horiz-origin-x":
						svgFont.HorizOriginX = float.Parse(attributeValue);
						return true;
					case "horiz-origin-y":
						svgFont.HorizOriginY = float.Parse(attributeValue);
						return true;
					case "vert-adv-y":
						svgFont.VertAdvY = float.Parse(attributeValue);
						return true;
					case "vert-origin-x":
						svgFont.VertOriginX = float.Parse(attributeValue);
						return true;
					case "vert-origin-y":
						svgFont.VertOriginY = float.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgFontFace svgFontFace)
				{
					switch (attributeName)
					{
					case "alphabetic":
						svgFontFace.Alphabetic = float.Parse(attributeValue);
						return true;
					case "ascent":
						svgFontFace.Ascent = float.Parse(attributeValue);
						return true;
					case "ascent-height":
						svgFontFace.AscentHeight = float.Parse(attributeValue);
						return true;
					case "descent":
						svgFontFace.Descent = float.Parse(attributeValue);
						return true;
					case "panose-1":
						svgFontFace.Panose1 = attributeValue;
						return true;
					case "units-per-em":
						svgFontFace.UnitsPerEm = float.Parse(attributeValue);
						return true;
					case "x-height":
						svgFontFace.XHeight = float.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgGlyph svgGlyph)
				{
					switch (attributeName)
					{
					case "d":
						svgGlyph.PathData = SvgPathConverter.Parse(attributeValue);
						return true;
					case "glyph-name":
						svgGlyph.GlyphName = attributeValue;
						return true;
					case "unicode":
						svgGlyph.Unicode = attributeValue;
						return true;
					case "horiz-adv-x":
						svgGlyph.HorizAdvX = float.Parse(attributeValue);
						return true;
					case "vert-adv-y":
						svgGlyph.VertAdvY = float.Parse(attributeValue);
						return true;
					case "vert-origin-x":
						svgGlyph.VertOriginX = float.Parse(attributeValue);
						return true;
					case "vert-origin-y":
						svgGlyph.VertOriginY = float.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgKern svgKern)
				{
					switch (attributeName)
					{
					case "g1":
						svgKern.Glyph1 = attributeValue;
						return true;
					case "g2":
						svgKern.Glyph2 = attributeValue;
						return true;
					case "u1":
						svgKern.Unicode1 = attributeValue;
						return true;
					case "u2":
						svgKern.Unicode2 = attributeValue;
						return true;
					case "k":
						svgKern.Kerning = float.Parse(attributeValue);
						return true;
					}
				}
				else if (element is SvgTextPath svgTextPath)
				{
					switch (attributeName)
					{
					case "startOffset":
						svgTextPath.StartOffset = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "method":
						svgTextPath.Method = (SvgTextPathMethod)Enum.Parse(typeof(SvgTextPathMethod), attributeValue, ignoreCase: true);
						return true;
					case "spacing":
						svgTextPath.Spacing = (SvgTextPathSpacing)Enum.Parse(typeof(SvgTextPathSpacing), attributeValue, ignoreCase: true);
						return true;
					}
				}
				else if (element is SvgColourMatrix svgColourMatrix)
				{
					switch (attributeName)
					{
					case "values":
						svgColourMatrix.Values = attributeValue;
						return true;
					case "type":
						svgColourMatrix.Type = (SvgColourMatrixType)Enum.Parse(typeof(SvgColourMatrixType), attributeValue, ignoreCase: true);
						return true;
					case "in":
						svgColourMatrix.Input = attributeValue;
						return true;
					case "result":
						svgColourMatrix.Result = attributeValue;
						return true;
					}
				}
				else if (element is SvgGaussianBlur svgGaussianBlur)
				{
					switch (attributeName)
					{
					case "stdDeviation":
						svgGaussianBlur.StdDeviation = float.Parse(attributeValue);
						return true;
					case "in":
						svgGaussianBlur.Input = attributeValue;
						return true;
					case "result":
						svgGaussianBlur.Result = attributeValue;
						return true;
					}
				}
				else if (element is SvgMerge svgMerge)
				{
					if (attributeName == "in")
					{
						svgMerge.Input = attributeValue;
						return true;
					}
					if (attributeName == "result")
					{
						svgMerge.Result = attributeValue;
						return true;
					}
				}
				else if (element is SvgMergeNode svgMergeNode)
				{
					if (attributeName == "in")
					{
						svgMergeNode.Input = attributeValue;
						return true;
					}
				}
				else if (element is SvgOffset svgOffset)
				{
					switch (attributeName)
					{
					case "dx":
						svgOffset.Dx = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "dy":
						svgOffset.Dy = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "in":
						svgOffset.Input = attributeValue;
						return true;
					case "result":
						svgOffset.Result = attributeValue;
						return true;
					}
				}
				else if (element is SvgFilter svgFilter)
				{
					switch (attributeName)
					{
					case "x":
						svgFilter.X = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "y":
						svgFilter.Y = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "width":
						svgFilter.Width = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "height":
						svgFilter.Height = SvgUnitConverter.Parse(attributeValue);
						return true;
					case "color-interpolation-filters":
						svgFilter.ColorInterpolationFilters = (SvgColourInterpolation)Enum.Parse(typeof(SvgColourInterpolation), attributeValue, ignoreCase: true);
						return true;
					}
				}
				else if (element is SvgSymbol svgSymbol)
				{
					if (attributeName == "viewBox")
					{
						svgSymbol.ViewBox = SvgViewBoxConverter.Parse(attributeValue);
						return true;
					}
					if (attributeName == "preserveAspectRatio")
					{
						svgSymbol.AspectRatio = SvgPreserveAspectRatioConverter.Parse(attributeValue);
						return true;
					}
				}
				break;
			}
		}
		catch
		{
		}
		return SetPropertyValueNULL(element, attributeName, attributeValue);
	}

	internal static bool SetPropertyValueNULL(SvgElement element, string attributeName, string attributeValue, bool isStyle = false)
	{
		if (string.Equals(element.ElementName, "svg", StringComparison.OrdinalIgnoreCase))
		{
			if (!string.Equals(attributeName, "xmlns", StringComparison.OrdinalIgnoreCase) && !string.Equals(attributeName, "xlink", StringComparison.OrdinalIgnoreCase) && !string.Equals(attributeName, "xmlns:xlink", StringComparison.OrdinalIgnoreCase) && !string.Equals(attributeName, "version", StringComparison.OrdinalIgnoreCase))
			{
				element.CustomAttributes[attributeName] = attributeValue;
			}
		}
		else
		{
			if (isStyle)
			{
				return false;
			}
			element.CustomAttributes[attributeName] = attributeValue;
		}
		return true;
	}
}
