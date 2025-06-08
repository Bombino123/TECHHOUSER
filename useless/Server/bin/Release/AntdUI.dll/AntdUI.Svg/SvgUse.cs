using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public class SvgUse : SvgVisualElement
{
	private Uri _referencedElement;

	public override string ClassName => "use";

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

	[SvgAttribute("x")]
	public virtual SvgUnit X
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
	public virtual SvgUnit Y
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
	public virtual SvgUnit Width
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
	public virtual SvgUnit Height
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

	public SvgPoint Location => new SvgPoint(X, Y);

	public override RectangleF Bounds
	{
		get
		{
			float num = Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this);
			float num2 = Height.ToDeviceValue(null, UnitRenderingType.Vertical, this);
			if (num > 0f && num2 > 0f)
			{
				return TransformedBounds(new RectangleF(Location.ToDeviceValue(null, this), new SizeF(num, num2)));
			}
			if (OwnerDocument.IdManager.GetElementById(ReferencedElement) is SvgVisualElement svgVisualElement)
			{
				return svgVisualElement.Bounds;
			}
			return default(RectangleF);
		}
	}

	protected override bool Renderable => false;

	private bool ElementReferencesUri(SvgElement element, List<Uri> elementUris)
	{
		if (element is SvgUse svgUse)
		{
			if (elementUris.Contains(svgUse.ReferencedElement))
			{
				return true;
			}
			if (OwnerDocument.IdManager.GetElementById(svgUse.ReferencedElement) is SvgUse)
			{
				elementUris.Add(svgUse.ReferencedElement);
			}
			return svgUse.ReferencedElementReferencesUri(elementUris);
		}
		if (element is SvgGroup svgGroup)
		{
			foreach (SvgElement child in svgGroup.Children)
			{
				if (ElementReferencesUri(child, elementUris))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ReferencedElementReferencesUri(List<Uri> elementUris)
	{
		SvgElement elementById = OwnerDocument.IdManager.GetElementById(ReferencedElement);
		return ElementReferencesUri(elementById, elementUris);
	}

	private bool HasRecursiveReference()
	{
		SvgElement elementById = OwnerDocument.IdManager.GetElementById(ReferencedElement);
		List<Uri> elementUris = new List<Uri> { ReferencedElement };
		return ElementReferencesUri(elementById, elementUris);
	}

	protected internal override bool PushTransforms(ISvgRenderer renderer)
	{
		if (!base.PushTransforms(renderer))
		{
			return false;
		}
		renderer.TranslateTransform(X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this), (MatrixOrder)0);
		return true;
	}

	public SvgUse()
	{
		X = 0f;
		Y = 0f;
		Width = 0f;
		Height = 0f;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		SvgVisualElement svgVisualElement = (SvgVisualElement)OwnerDocument.IdManager.GetElementById(ReferencedElement);
		if (svgVisualElement == null || HasRecursiveReference())
		{
			return null;
		}
		return svgVisualElement.Path(renderer);
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (!Visible || !Displayable || !(ReferencedElement != null) || HasRecursiveReference() || !PushTransforms(renderer))
		{
			return;
		}
		SetClip(renderer);
		if (OwnerDocument.IdManager.GetElementById(ReferencedElement) is SvgVisualElement svgVisualElement)
		{
			float num = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
			float num2 = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
			if (num > 0f && num2 > 0f)
			{
				SvgViewBox attribute = svgVisualElement.Attributes.GetAttribute<SvgViewBox>("viewBox");
				if (attribute != SvgViewBox.Empty && Math.Abs(num - attribute.Width) > float.Epsilon && Math.Abs(num2 - attribute.Height) > float.Epsilon)
				{
					float sx = num / attribute.Width;
					float sy = num2 / attribute.Height;
					renderer.ScaleTransform(sx, sy, (MatrixOrder)0);
				}
			}
			SvgElement parent = svgVisualElement.Parent;
			svgVisualElement._parent = this;
			svgVisualElement.InvalidateChildPaths();
			svgVisualElement.RenderElement(renderer);
			svgVisualElement._parent = parent;
		}
		ResetClip(renderer);
		PopTransforms(renderer);
	}
}
