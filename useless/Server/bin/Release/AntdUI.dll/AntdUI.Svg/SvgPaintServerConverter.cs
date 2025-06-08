using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AntdUI.Svg;

internal class SvgPaintServerConverter
{
	public static SvgPaintServer Parse(string value, SvgDocument context)
	{
		if (string.Equals(value.Trim(), "none", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(value) || value.Trim().Length < 1)
		{
			return SvgPaintServer.None;
		}
		return Create(value, context);
	}

	public static SvgPaintServer Create(string value, SvgDocument document)
	{
		if (string.IsNullOrEmpty(value))
		{
			return SvgColourServer.NotSet;
		}
		if (value == "inherit")
		{
			return SvgColourServer.Inherit;
		}
		if (value == "currentColor")
		{
			return new SvgDeferredPaintServer(document, value);
		}
		List<SvgPaintServer> list = new List<SvgPaintServer>();
		while (!string.IsNullOrEmpty(value))
		{
			if (value.StartsWith("url(#"))
			{
				int num = value.IndexOf(')', 5);
				Uri uri = new Uri(value.Substring(5, num - 5), UriKind.Relative);
				value = value.Substring(num + 1).Trim();
				list.Add((SvgPaintServer)document.IdManager.GetElementById(uri));
				continue;
			}
			if (document.IdManager.GetElementById(value) != null && document.IdManager.GetElementById(value).GetType().BaseType == typeof(SvgGradientServer))
			{
				return (SvgPaintServer)document.IdManager.GetElementById(value);
			}
			if (value.StartsWith("#"))
			{
				switch (CountHexDigits(value, 1))
				{
				case 3:
					list.Add(new SvgColourServer(ParseColor(value.Substring(0, 4))));
					value = value.Substring(4).Trim();
					break;
				case 6:
					list.Add(new SvgColourServer(ParseColor(value.Substring(0, 7))));
					value = value.Substring(7).Trim();
					break;
				default:
					return new SvgDeferredPaintServer(document, value);
				}
				continue;
			}
			return new SvgColourServer(ParseColor(value.Trim()));
		}
		if (list.Count > 1)
		{
			return new SvgFallbackPaintServer(list[0], list.Skip(1));
		}
		return list[0];
	}

	public static Color ParseColor(string colour)
	{
		colour = colour.Trim();
		if (colour.StartsWith("rgb"))
		{
			try
			{
				int num = colour.IndexOf("(") + 1;
				string[] array = colour.Substring(num, colour.IndexOf(")") - num).Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				int alpha = 255;
				if (array.Length > 3)
				{
					string text = array[3];
					if (text.StartsWith("."))
					{
						text = "0" + text;
					}
					decimal num2 = decimal.Parse(text);
					alpha = ((!(num2 <= 1m)) ? ((int)Math.Round(num2)) : ((int)Math.Round(num2 * 255m)));
				}
				if (array[0].Trim().EndsWith("%"))
				{
					return Color.FromArgb(alpha, (int)Math.Round(255f * float.Parse(array[0].Trim().TrimEnd(new char[1] { '%' })) / 100f), (int)Math.Round(255f * float.Parse(array[1].Trim().TrimEnd(new char[1] { '%' })) / 100f), (int)Math.Round(255f * float.Parse(array[2].Trim().TrimEnd(new char[1] { '%' })) / 100f));
				}
				return Color.FromArgb(alpha, int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
			}
			catch
			{
				throw new SvgException("Colour is in an invalid format: '" + colour + "'");
			}
		}
		if (colour.StartsWith("hsl"))
		{
			try
			{
				int num3 = colour.IndexOf("(") + 1;
				string[] array2 = colour.Substring(num3, colour.IndexOf(")") - num3).Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2[1].EndsWith("%"))
				{
					array2[1] = array2[1].TrimEnd(new char[1] { '%' });
				}
				if (array2[2].EndsWith("%"))
				{
					array2[2] = array2[2].TrimEnd(new char[1] { '%' });
				}
				double h = double.Parse(array2[0]) / 360.0;
				double sl = double.Parse(array2[1]) / 100.0;
				double l = double.Parse(array2[2]) / 100.0;
				return Hsl2Rgb(h, sl, l);
			}
			catch
			{
				throw new SvgException("Colour is in an invalid format: '" + colour + "'");
			}
		}
		if (colour.StartsWith("#"))
		{
			return colour.ToColor();
		}
		return colour.ToLowerInvariant() switch
		{
			"activeborder" => SystemColors.ActiveBorder, 
			"activecaption" => SystemColors.ActiveCaption, 
			"appworkspace" => SystemColors.AppWorkspace, 
			"background" => SystemColors.Desktop, 
			"buttonface" => SystemColors.Control, 
			"buttonhighlight" => SystemColors.ControlLightLight, 
			"buttonshadow" => SystemColors.ControlDark, 
			"buttontext" => SystemColors.ControlText, 
			"captiontext" => SystemColors.ActiveCaptionText, 
			"graytext" => SystemColors.GrayText, 
			"highlight" => SystemColors.Highlight, 
			"highlighttext" => SystemColors.HighlightText, 
			"inactiveborder" => SystemColors.InactiveBorder, 
			"inactivecaption" => SystemColors.InactiveCaption, 
			"inactivecaptiontext" => SystemColors.InactiveCaptionText, 
			"infobackground" => SystemColors.Info, 
			"infotext" => SystemColors.InfoText, 
			"menu" => SystemColors.Menu, 
			"menutext" => SystemColors.MenuText, 
			"scrollbar" => SystemColors.ScrollBar, 
			"threeddarkshadow" => SystemColors.ControlDarkDark, 
			"threedface" => SystemColors.Control, 
			"threedhighlight" => SystemColors.ControlLight, 
			"threedlightshadow" => SystemColors.ControlLightLight, 
			"window" => SystemColors.Window, 
			"windowframe" => SystemColors.WindowFrame, 
			"windowtext" => SystemColors.WindowText, 
			_ => Color.Transparent, 
		};
	}

	private static Color Hsl2Rgb(double h, double sl, double l)
	{
		double num = l;
		double num2 = l;
		double num3 = l;
		double num4 = ((l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl));
		if (num4 > 0.0)
		{
			double num5 = l + l - num4;
			double num6 = (num4 - num5) / num4;
			h *= 6.0;
			int num7 = (int)h;
			double num8 = h - (double)num7;
			double num9 = num4 * num6 * num8;
			double num10 = num5 + num9;
			double num11 = num4 - num9;
			switch (num7)
			{
			case 0:
				num = num4;
				num2 = num10;
				num3 = num5;
				break;
			case 1:
				num = num11;
				num2 = num4;
				num3 = num5;
				break;
			case 2:
				num = num5;
				num2 = num4;
				num3 = num10;
				break;
			case 3:
				num = num5;
				num2 = num11;
				num3 = num4;
				break;
			case 4:
				num = num10;
				num2 = num5;
				num3 = num4;
				break;
			case 5:
				num = num4;
				num2 = num5;
				num3 = num11;
				break;
			}
		}
		return Color.FromArgb((int)Math.Round(num * 255.0), (int)Math.Round(num2 * 255.0), (int)Math.Round(num3 * 255.0));
	}

	private static int CountHexDigits(string value, int start)
	{
		int i = Math.Max(start, 0);
		int num = 0;
		for (; i < value.Length && ((value[i] >= '0' && value[i] <= '9') || (value[i] >= 'a' && value[i] <= 'f') || (value[i] >= 'A' && value[i] <= 'F')); i++)
		{
			num++;
		}
		return num;
	}
}
