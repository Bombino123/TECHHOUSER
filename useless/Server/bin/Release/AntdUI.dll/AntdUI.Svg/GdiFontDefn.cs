using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AntdUI.Svg;

public class GdiFontDefn : IFontDefn, IDisposable
{
	private Font _font;

	private Graphics _graphics;

	public float Size => _font.Size;

	public float SizeInPoints => _font.SizeInPoints;

	public GdiFontDefn(Font font)
	{
		_font = font;
	}

	public void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected I4, but got Unknown
		path.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, location, StringFormat.GenericTypographic);
	}

	public float Ascent(ISvgRenderer renderer)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		FontFamily fontFamily = _font.FontFamily;
		float num = fontFamily.GetCellAscent(_font.Style);
		float num2 = _font.SizeInPoints / (float)fontFamily.GetEmHeight(_font.Style) * num;
		return (float)SvgDocument.Ppi / 72f * num2;
	}

	public IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Graphics g = GetGraphics(renderer);
		List<RectangleF> list = new List<RectangleF>();
		for (int i = 0; i <= (text.Length - 1) / 32; i++)
		{
			StringFormat genericTypographic = StringFormat.GenericTypographic;
			genericTypographic.FormatFlags = (StringFormatFlags)(genericTypographic.FormatFlags | 0x800);
			genericTypographic.SetMeasurableCharacterRanges(Enumerable.Range(32 * i, Math.Min(32, text.Length - 32 * i)).Select((Func<int, CharacterRange>)((int r) => new CharacterRange(r, 1))).ToArray());
			list.AddRange(from r in g.MeasureCharacterRanges(text, _font, (RectangleF)new Rectangle(0, 0, 1000, 1000), genericTypographic)
				select r.GetBounds(g));
		}
		return list;
	}

	public SizeF MeasureString(ISvgRenderer renderer, string text)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Graphics graphics = GetGraphics(renderer);
		object obj = StringFormat.GenericTypographic.Clone();
		StringFormat val = (StringFormat)((obj is StringFormat) ? obj : null);
		val.SetMeasurableCharacterRanges((CharacterRange[])(object)new CharacterRange[1]
		{
			new CharacterRange(0, text.Length)
		});
		val.FormatFlags = (StringFormatFlags)(val.FormatFlags | 0x800);
		return new SizeF(graphics.MeasureCharacterRanges(text, _font, (RectangleF)new Rectangle(0, 0, 1000, 1000), val)[0].GetBounds(graphics).Width, Ascent(renderer));
	}

	private Graphics GetGraphics(object renderer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (!(renderer is IGraphicsProvider graphicsProvider))
		{
			if (_graphics == null)
			{
				Bitmap val = new Bitmap(1, 1);
				_graphics = Graphics.FromImage((Image)(object)val);
			}
			return _graphics;
		}
		return graphicsProvider.GetGraphics();
	}

	public void Dispose()
	{
		_font.Dispose();
	}
}
