using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace AntdUI;

public static class CorrectionTextRendering
{
	internal static Dictionary<string, float> tmpChinese = new Dictionary<string, float>();

	internal static Dictionary<string, float> tmpEnglish = new Dictionary<string, float>();

	private static bool enable = false;

	private static string defTextChinese = "不";

	private static string defTextEnglish = "X";

	private static int defFontSize = 20;

	internal static void Set(string familie)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		enable = true;
		Font font = new Font(familie, (float)defFontSize);
		try
		{
			int size = Helper.GDI((Canvas g) => (int)((float)g.MeasureString(defTextChinese, font).Height * 1.2f));
			float fontOffset = GetFontOffset(font, size, defTextChinese);
			float fontOffset2 = GetFontOffset(font, size, defTextEnglish);
			if (fontOffset <= 1f && fontOffset >= -1f)
			{
				lock (tmpChinese)
				{
					if (tmpChinese.ContainsKey(familie))
					{
						tmpChinese.Remove(familie);
					}
				}
			}
			else
			{
				float value = fontOffset / (float)defFontSize;
				lock (tmpChinese)
				{
					if (tmpChinese.ContainsKey(familie))
					{
						tmpChinese[familie] = value;
					}
					else
					{
						tmpChinese.Add(familie, value);
					}
				}
			}
			if (fontOffset2 <= 1f && fontOffset2 >= -1f)
			{
				lock (tmpEnglish)
				{
					if (tmpEnglish.ContainsKey(familie))
					{
						tmpEnglish.Remove(familie);
					}
					return;
				}
			}
			float value2 = fontOffset2 / (float)defFontSize;
			lock (tmpEnglish)
			{
				if (tmpEnglish.ContainsKey(familie))
				{
					tmpEnglish[familie] = value2;
				}
				else
				{
					tmpEnglish.Add(familie, value2);
				}
			}
		}
		finally
		{
			if (font != null)
			{
				((IDisposable)font).Dispose();
			}
		}
	}

	private static float GetFontOffset(Font font, int size, string text)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		float num = (float)size / 2f;
		StringFormat val = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);
		try
		{
			Bitmap val2 = new Bitmap(size, size);
			try
			{
				using (Canvas canvas = Graphics.FromImage((Image)(object)val2).High())
				{
					canvas.String(text, font, Brushes.Black, new Rectangle(0, 0, ((Image)val2).Width, ((Image)val2).Height), val);
				}
				TextRealY(val2, out var y, out var h);
				float num2 = (float)y + (float)h / 2f;
				return num - num2;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void TextRealY(Bitmap bmp, out int y, out int h)
	{
		y = TextRealY(bmp);
		h = TextRealHeight(bmp, y);
	}

	private static int TextRealY(Bitmap bmp)
	{
		for (int i = 0; i < ((Image)bmp).Height; i++)
		{
			for (int j = 0; j < ((Image)bmp).Width; j++)
			{
				if (bmp.GetPixel(j, i).A > 0)
				{
					return i - 1;
				}
			}
		}
		return 0;
	}

	private static int TextRealHeight(Bitmap bmp, int _y)
	{
		for (int num = ((Image)bmp).Height - 1; num > _y; num--)
		{
			int num2 = 0;
			for (int i = 0; i < ((Image)bmp).Width; i++)
			{
				if (bmp.GetPixel(i, num).A > 0)
				{
					num2++;
				}
			}
			if (num2 > 0)
			{
				return num + 1 - _y;
			}
		}
		return ((Image)bmp).Height - _y;
	}

	internal static void CORE(Font font, string? text, ref RectangleF layoutRectangle)
	{
		if (!enable || text == null || (tmpEnglish.Count <= 0 && tmpChinese.Count <= 0))
		{
			return;
		}
		float value2;
		if (text.ContainsChinese())
		{
			if (tmpChinese.TryGetValue(font.Name, out var value))
			{
				layoutRectangle.Offset(0f, value * font.Size);
			}
		}
		else if (tmpEnglish.TryGetValue(font.Name, out value2))
		{
			layoutRectangle.Offset(0f, value2 * font.Size);
		}
	}

	internal static void CORE(Font font, string? text, ref Rectangle layoutRectangle)
	{
		if (!enable || text == null)
		{
			return;
		}
		float value2;
		if (text.ContainsChinese())
		{
			if (tmpChinese.TryGetValue(font.Name, out var value))
			{
				layoutRectangle.Offset(0, (int)Math.Round(value * font.Size));
			}
		}
		else if (tmpEnglish.TryGetValue(font.Name, out value2))
		{
			layoutRectangle.Offset(0, (int)Math.Round(value2 * font.Size));
		}
	}

	private static bool ContainsChinese(this string input)
	{
		return Regex.IsMatch(input, "[\\u4e00-\\u9fa5]");
	}
}
