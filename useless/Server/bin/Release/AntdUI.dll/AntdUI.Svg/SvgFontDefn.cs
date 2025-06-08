using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AntdUI.Svg;

public class SvgFontDefn : IFontDefn, IDisposable
{
	private SvgFont _font;

	private float _emScale;

	private float _ppi;

	private float _size;

	private Dictionary<string, SvgGlyph> _glyphs;

	private Dictionary<string, SvgKern> _kerning;

	public float Size => _size;

	public float SizeInPoints => _size * 72f / _ppi;

	public SvgFontDefn(SvgFont font, float size, float ppi)
	{
		_font = font;
		_size = size;
		_ppi = ppi;
		SvgFontFace svgFontFace = _font.Children.OfType<SvgFontFace>().First();
		_emScale = _size / svgFontFace.UnitsPerEm;
	}

	public float Ascent(ISvgRenderer renderer)
	{
		float ascent = _font.Descendants().OfType<SvgFontFace>().First()
			.Ascent;
		float num = SizeInPoints * (_emScale / _size) * ascent;
		return (float)SvgDocument.Ppi / 72f * num;
	}

	public IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text)
	{
		List<RectangleF> list = new List<RectangleF>();
		GraphicsPath path = GetPath(renderer, text, list, measureSpaces: false);
		try
		{
			return list;
		}
		finally
		{
			((IDisposable)path)?.Dispose();
		}
	}

	public SizeF MeasureString(ISvgRenderer renderer, string text)
	{
		List<RectangleF> list = new List<RectangleF>();
		GraphicsPath path = GetPath(renderer, text, list, measureSpaces: true);
		try
		{
		}
		finally
		{
			((IDisposable)path)?.Dispose();
		}
		IEnumerable<RectangleF> source = list.Where((RectangleF r) => r != RectangleF.Empty);
		if (!source.Any())
		{
			return SizeF.Empty;
		}
		return new SizeF(source.Last().Right - source.First().Left, Ascent(renderer));
	}

	public void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		GraphicsPath path2 = GetPath(renderer, text, null, measureSpaces: false);
		if (path2.PointCount > 0)
		{
			Matrix val = new Matrix();
			try
			{
				val.Translate(location.X, location.Y);
				path2.Transform(val);
				path.AddPath(path2, false);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private GraphicsPath GetPath(ISvgRenderer renderer, string text, IList<RectangleF> ranges, bool measureSpaces)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		EnsureDictionaries();
		SvgGlyph svgGlyph = null;
		float num = 0f;
		float num2 = Ascent(renderer);
		GraphicsPath val = new GraphicsPath();
		if (string.IsNullOrEmpty(text))
		{
			return val;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!_glyphs.TryGetValue(text.Substring(i, 1), out SvgGlyph value))
			{
				value = _font.Descendants().OfType<SvgMissingGlyph>().First();
			}
			if (svgGlyph != null && _kerning.TryGetValue(svgGlyph.GlyphName + "|" + value.GlyphName, out SvgKern value2))
			{
				num -= value2.Kerning * _emScale;
			}
			GraphicsPath val2 = (GraphicsPath)value.Path(renderer).Clone();
			Matrix val3 = new Matrix();
			val3.Scale(_emScale, -1f * _emScale, (MatrixOrder)1);
			val3.Translate(num, num2, (MatrixOrder)1);
			val2.Transform(val3);
			val3.Dispose();
			RectangleF bounds = val2.GetBounds();
			if (ranges != null)
			{
				if (measureSpaces && bounds == RectangleF.Empty)
				{
					ranges.Add(new RectangleF(num, 0f, value.HorizAdvX * _emScale, num2));
				}
				else
				{
					ranges.Add(bounds);
				}
			}
			if (val2.PointCount > 0)
			{
				val.AddPath(val2, false);
			}
			num += value.HorizAdvX * _emScale;
			svgGlyph = value;
		}
		return val;
	}

	private void EnsureDictionaries()
	{
		if (_glyphs == null)
		{
			_glyphs = _font.Descendants().OfType<SvgGlyph>().ToDictionary((SvgGlyph g) => g.Unicode ?? g.GlyphName ?? g.ID);
		}
		if (_kerning == null)
		{
			_kerning = _font.Descendants().OfType<SvgKern>().ToDictionary((SvgKern k) => k.Glyph1 + "|" + k.Glyph2);
		}
	}

	public void Dispose()
	{
		_glyphs = null;
		_kerning = null;
	}
}
