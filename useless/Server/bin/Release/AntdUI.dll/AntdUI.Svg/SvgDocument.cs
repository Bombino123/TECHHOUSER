using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Xml;

namespace AntdUI.Svg;

public class SvgDocument : SvgFragment
{
	private Dictionary<string, IEnumerable<SvgFontFace>>? _fontDefns;

	private SvgElementIdManager? _idManager;

	public Uri? BaseUri { get; set; }

	protected internal virtual SvgElementIdManager IdManager
	{
		get
		{
			if (_idManager == null)
			{
				_idManager = new SvgElementIdManager(this);
			}
			return _idManager;
		}
	}

	public static int Ppi => (int)(Config.Dpi * 96f);

	internal Dictionary<string, IEnumerable<SvgFontFace>> FontDefns()
	{
		if (_fontDefns == null)
		{
			_fontDefns = (from f in Descendants().OfType<SvgFontFace>()
				group f by f.FontFamily into family
				select (family)).ToDictionary((Func<IGrouping<string, SvgFontFace>, string>)((IGrouping<string, SvgFontFace> f) => f.Key), (Func<IGrouping<string, SvgFontFace>, IEnumerable<SvgFontFace>>)((IGrouping<string, SvgFontFace> f) => f));
		}
		return _fontDefns;
	}

	public void OverwriteIdManager(SvgElementIdManager manager)
	{
		_idManager = manager;
	}

	public virtual SvgElement GetElementById(string id)
	{
		return IdManager.GetElementById(id);
	}

	public virtual TSvgElement GetElementById<TSvgElement>(string id) where TSvgElement : SvgElement
	{
		return (TSvgElement)GetElementById(id);
	}

	public static T? Open<T>(string path) where T : SvgDocument, new()
	{
		using FileStream stream = File.OpenRead(path);
		T val = Open<T>(stream);
		if (val != null)
		{
			val.BaseUri = new Uri(System.IO.Path.GetFullPath(path));
		}
		return val;
	}

	public static T? FromSvg<T>(string svg) where T : SvgDocument, new()
	{
		if (string.IsNullOrEmpty(svg))
		{
			return null;
		}
		using StringReader reader = new StringReader(svg);
		return Open<T>(new SvgTextReader(reader)
		{
			WhitespaceHandling = WhitespaceHandling.None
		});
	}

	public static T? Open<T>(Stream stream) where T : SvgDocument, new()
	{
		return Open<T>(new SvgTextReader(stream)
		{
			WhitespaceHandling = WhitespaceHandling.None
		});
	}

	private static T? Open<T>(XmlReader reader) where T : SvgDocument, new()
	{
		Stack<SvgElement> stack = new Stack<SvgElement>();
		T val = null;
		SvgElementFactory svgElementFactory = new SvgElementFactory();
		try
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.Element:
				{
					bool isEmptyElement = reader.IsEmptyElement;
					SvgElement svgElement;
					if (stack.Count > 0)
					{
						svgElement = svgElementFactory.CreateElement(reader, val);
					}
					else
					{
						val = svgElementFactory.CreateDocument<T>(reader);
						svgElement = val;
					}
					if (stack.Count > 0)
					{
						SvgElement svgElement2 = stack.Peek();
						if (svgElement2 != null && svgElement != null)
						{
							svgElement2.Children.Add(svgElement);
							svgElement2.Nodes.Add(svgElement);
						}
					}
					stack.Push(svgElement);
					if (!isEmptyElement)
					{
						break;
					}
					goto case XmlNodeType.EndElement;
				}
				case XmlNodeType.EndElement:
				{
					SvgElement svgElement = stack.Pop();
					if (svgElement.Nodes.OfType<SvgContentNode>().Any())
					{
						svgElement.Content = svgElement.Nodes.Select((ISvgNode e) => e.Content).Aggregate((string p, string c) => p + c);
					}
					else
					{
						svgElement.Nodes.Clear();
					}
					break;
				}
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				{
					SvgElement svgElement = stack.Peek();
					svgElement.Nodes.Add(new SvgContentNode
					{
						Content = reader.Value
					});
					break;
				}
				case XmlNodeType.EntityReference:
				{
					reader.ResolveEntity();
					SvgElement svgElement = stack.Peek();
					svgElement.Nodes.Add(new SvgContentNode
					{
						Content = reader.Value
					});
					break;
				}
				}
			}
		}
		catch
		{
		}
		if (val != null)
		{
			FlushStyles(val);
		}
		return val;
	}

	private static void FlushStyles(SvgElement elem)
	{
		elem.FlushStyles();
		foreach (SvgElement child in elem.Children)
		{
			FlushStyles(child);
		}
	}

	private void Draw(ISvgRenderer renderer, ISvgBoundable boundable)
	{
		renderer.SetBoundable(boundable);
		Render(renderer);
	}

	public void Draw(ISvgRenderer renderer)
	{
		Draw(renderer, this);
	}

	public void Draw(Graphics graphics)
	{
		Draw(graphics, null);
	}

	public void Draw(Graphics graphics, SizeF? size)
	{
		using ISvgRenderer renderer = SvgRenderer.FromGraphics(graphics);
		ISvgBoundable svgBoundable2;
		if (!size.HasValue)
		{
			ISvgBoundable svgBoundable = this;
			svgBoundable2 = svgBoundable;
		}
		else
		{
			ISvgBoundable svgBoundable = new GenericBoundable(0f, 0f, size.Value.Width, size.Value.Height);
			svgBoundable2 = svgBoundable;
		}
		ISvgBoundable boundable = svgBoundable2;
		Draw(renderer, boundable);
	}

	public virtual Bitmap? Draw()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		Bitmap val = null;
		try
		{
			try
			{
				SizeF dimensions = GetDimensions();
				val = new Bitmap((int)Math.Round(dimensions.Width), (int)Math.Round(dimensions.Height));
				Draw(val);
			}
			catch
			{
			}
		}
		catch
		{
			if (val != null)
			{
				((Image)val).Dispose();
			}
			val = null;
		}
		return val;
	}

	public virtual void Draw(Bitmap bitmap)
	{
		using ISvgRenderer renderer = SvgRenderer.FromImage((Image)(object)bitmap);
		Overflow = SvgOverflow.Auto;
		GenericBoundable boundable = new GenericBoundable(0f, 0f, ((Image)bitmap).Width, ((Image)bitmap).Height);
		Draw(renderer, boundable);
	}

	public virtual Bitmap? Draw(int rasterWidth, int rasterHeight)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		SizeF dimensions = GetDimensions();
		SizeF size = dimensions;
		RasterizeDimensions(ref size, rasterWidth, rasterHeight);
		if (size.Width == 0f || size.Height == 0f)
		{
			return null;
		}
		Bitmap val = null;
		try
		{
			try
			{
				val = new Bitmap((int)Math.Round(size.Width), (int)Math.Round(size.Height));
				using ISvgRenderer svgRenderer = SvgRenderer.FromImage((Image)(object)val);
				svgRenderer.ScaleTransform(size.Width / dimensions.Width, size.Height / dimensions.Height, (MatrixOrder)1);
				GenericBoundable boundable = new GenericBoundable(0f, 0f, dimensions.Width, dimensions.Height);
				Draw(svgRenderer, boundable);
			}
			catch
			{
			}
		}
		catch
		{
			if (val != null)
			{
				((Image)val).Dispose();
			}
			val = null;
		}
		return val;
	}

	public virtual void RasterizeDimensions(ref SizeF size, int rasterWidth, int rasterHeight)
	{
		if (size.Width != 0f)
		{
			float num = size.Height / size.Width;
			size.Width = ((rasterWidth > 0) ? ((float)rasterWidth) : size.Width);
			size.Height = ((rasterHeight > 0) ? ((float)rasterHeight) : size.Height);
			if (rasterHeight == 0 && rasterWidth > 0)
			{
				size.Height = (int)((float)rasterWidth * num);
			}
			else if (rasterHeight > 0 && rasterWidth == 0)
			{
				size.Width = (int)((float)rasterHeight / num);
			}
		}
	}
}
