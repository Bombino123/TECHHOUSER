using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using AntdUI.Theme;

namespace AntdUI;

public static class Style
{
	public static IColor Db;

	private static Dictionary<string, Color> colors;

	private static float warmDark;

	private static float warmRotate;

	private static float coldDark;

	private static float coldRotate;

	private static int hueStep;

	private static int darkColorCount;

	private static int lightColorCount;

	private static float saturationStep;

	private static float saturationStep2;

	private static float brightnessStep1;

	private static float brightnessStep2;

	static Style()
	{
		colors = new Dictionary<string, Color>();
		warmDark = 0.5f;
		warmRotate = -26f;
		coldDark = 0.55f;
		coldRotate = 10f;
		hueStep = 2;
		darkColorCount = 4;
		lightColorCount = 5;
		saturationStep = 0.16f;
		saturationStep2 = 0.05f;
		brightnessStep1 = 0.05f;
		brightnessStep2 = 0.15f;
		Db = new IColor();
	}

	public static Color Get(this Colour id, string control)
	{
		string key = id.ToString() + control;
		if (colors.TryGetValue(key, out var value))
		{
			return value;
		}
		return id.Get();
	}

	public static Color Get(this Colour id, string control, TAMode mode)
	{
		string key = id.ToString() + control;
		if (colors.TryGetValue(key, out var value))
		{
			return value;
		}
		return id.Get(mode);
	}

	public static Color Get(this Colour id)
	{
		string key = id.ToString();
		if (colors.TryGetValue(key, out var value))
		{
			return value;
		}
		return id.GetSystem(Config.Mode);
	}

	public static Color Get(this Colour id, TAMode mode)
	{
		string key = id.ToString();
		if (colors.TryGetValue(key, out var value))
		{
			return value;
		}
		return mode switch
		{
			TAMode.Light => id.GetSystem(TMode.Light), 
			TAMode.Dark => id.GetSystem(TMode.Dark), 
			_ => id.GetSystem(Config.Mode), 
		};
	}

	public static Color GetSystem(this Colour id, TMode mode)
	{
		switch (mode)
		{
		case TMode.Light:
			switch (id)
			{
			case Colour.Primary:
				return "#1677FF".ToColor();
			case Colour.PrimaryHover:
				return "#4096FF".ToColor();
			case Colour.PrimaryColor:
				return Color.White;
			case Colour.PrimaryActive:
				return "#0958D9".ToColor();
			case Colour.PrimaryBg:
				return "#E6F4FF".ToColor();
			case Colour.PrimaryBgHover:
				return "#BAE0FF".ToColor();
			case Colour.PrimaryBorder:
				return "#91CAFF".ToColor();
			case Colour.PrimaryBorderHover:
				return "#69B1FF".ToColor();
			case Colour.Success:
				return "#52C41A".ToColor();
			case Colour.SuccessColor:
				return Color.White;
			case Colour.SuccessBg:
				return "#F6FFED".ToColor();
			case Colour.SuccessBorder:
				return "#B7EB8F".ToColor();
			case Colour.SuccessHover:
				return "#95DE64".ToColor();
			case Colour.SuccessActive:
				return "#389E0D".ToColor();
			case Colour.Warning:
				return "#FAAD14".ToColor();
			case Colour.WarningColor:
				return Color.White;
			case Colour.WarningBg:
				return "#FFFBE6".ToColor();
			case Colour.WarningBorder:
				return "#FFE58F".ToColor();
			case Colour.WarningHover:
				return "#FFD666".ToColor();
			case Colour.WarningActive:
				return "#D48806".ToColor();
			case Colour.Error:
				return "#FF4D4F".ToColor();
			case Colour.ErrorColor:
				return Color.White;
			case Colour.ErrorBg:
				return "#FFF2F0".ToColor();
			case Colour.ErrorBorder:
				return "#FFCCC7".ToColor();
			case Colour.ErrorHover:
				return "#FF7875".ToColor();
			case Colour.ErrorActive:
				return "#D9363E".ToColor();
			case Colour.Info:
				return "#1677FF".ToColor();
			case Colour.InfoColor:
				return Color.White;
			case Colour.InfoBg:
				return "#E6F4FF".ToColor();
			case Colour.InfoBorder:
				return "#91CAFF".ToColor();
			case Colour.InfoHover:
				return "#69B1FF".ToColor();
			case Colour.InfoActive:
				return "#0958D9".ToColor();
			case Colour.DefaultBg:
				return Color.White;
			case Colour.DefaultColor:
				return rgba(0, 0, 0, 0.88f);
			case Colour.DefaultBorder:
				return "#D9D9D9".ToColor();
			case Colour.TagDefaultBg:
				return "#FAFAFA".ToColor();
			case Colour.TagDefaultColor:
				return rgba(0, 0, 0, 0.88f);
			case Colour.TextBase:
				return Color.Black;
			case Colour.Text:
				return rgba(0, 0, 0, 0.88f);
			case Colour.TextSecondary:
				return rgba(0, 0, 0, 0.65f);
			case Colour.TextTertiary:
				return rgba(0, 0, 0, 0.45f);
			case Colour.TextQuaternary:
				return rgba(0, 0, 0, 0.25f);
			case Colour.BgBase:
				return Color.White;
			case Colour.BgContainer:
				return Color.White;
			case Colour.BgElevated:
				return Color.White;
			case Colour.BgLayout:
				return "#F5F5F5".ToColor();
			case Colour.Fill:
				return rgba(0, 0, 0, 0.18f);
			case Colour.FillSecondary:
				return rgba(0, 0, 0, 0.06f);
			case Colour.FillTertiary:
				return rgba(0, 0, 0, 0.04f);
			case Colour.FillQuaternary:
				return rgba(0, 0, 0, 0.02f);
			case Colour.BorderColor:
				return "#D9D9D9".ToColor();
			case Colour.BorderSecondary:
				return "#F0F0F0".ToColor();
			case Colour.BorderColorDisable:
				return Color.FromArgb(217, 217, 217);
			case Colour.Split:
				return rgba(5, 5, 5, 0.06f);
			case Colour.HoverBg:
				return rgba(0, 0, 0, 0.06f);
			case Colour.HoverColor:
				return rgba(0, 0, 0, 0.88f);
			case Colour.SliderHandleColorDisabled:
				return "#BFBFBF".ToColor();
			}
			break;
		default:
			switch (id)
			{
			case Colour.Primary:
				return "#1668DC".ToColor();
			case Colour.PrimaryHover:
				return "#3C89E8".ToColor();
			case Colour.PrimaryColor:
				return Color.White;
			case Colour.PrimaryActive:
				return "#1554AD".ToColor();
			case Colour.PrimaryBg:
				return "#111A2C".ToColor();
			case Colour.PrimaryBgHover:
				return "#112545".ToColor();
			case Colour.PrimaryBorder:
				return "#15325B".ToColor();
			case Colour.PrimaryBorderHover:
				return "#15417E".ToColor();
			case Colour.Success:
				return "#49AA19".ToColor();
			case Colour.SuccessColor:
				return Color.White;
			case Colour.SuccessBg:
				return "#162312".ToColor();
			case Colour.SuccessBorder:
				return "#274916".ToColor();
			case Colour.SuccessHover:
				return "#306317".ToColor();
			case Colour.SuccessActive:
				return "#3C8618".ToColor();
			case Colour.Warning:
				return "#D89614".ToColor();
			case Colour.WarningColor:
				return Color.White;
			case Colour.WarningBg:
				return "#2B2111".ToColor();
			case Colour.WarningBorder:
				return "#594214".ToColor();
			case Colour.WarningHover:
				return "#7C5914".ToColor();
			case Colour.WarningActive:
				return "#AA7714".ToColor();
			case Colour.Error:
				return "#DC4446".ToColor();
			case Colour.ErrorColor:
				return Color.White;
			case Colour.ErrorBg:
				return "#2C1618".ToColor();
			case Colour.ErrorBorder:
				return "#5B2526".ToColor();
			case Colour.ErrorHover:
				return "#E86E6B".ToColor();
			case Colour.ErrorActive:
				return "#AD393A".ToColor();
			case Colour.Info:
				return "#1668DC".ToColor();
			case Colour.InfoColor:
				return Color.White;
			case Colour.InfoBg:
				return "#111A2C".ToColor();
			case Colour.InfoBorder:
				return "#15325B".ToColor();
			case Colour.InfoHover:
				return "#15417E".ToColor();
			case Colour.InfoActive:
				return "#1554AD".ToColor();
			case Colour.DefaultBg:
				return "#141414".ToColor();
			case Colour.DefaultColor:
				return rgba(255, 255, 255, 0.85f);
			case Colour.DefaultBorder:
				return "#424242".ToColor();
			case Colour.TagDefaultBg:
				return "#1D1D1D".ToColor();
			case Colour.TagDefaultColor:
				return rgba(255, 255, 255, 0.85f);
			case Colour.TextBase:
				return Color.White;
			case Colour.Text:
				return rgba(255, 255, 255, 0.85f);
			case Colour.TextSecondary:
				return rgba(255, 255, 255, 0.65f);
			case Colour.TextTertiary:
				return rgba(255, 255, 255, 0.45f);
			case Colour.TextQuaternary:
				return rgba(255, 255, 255, 0.25f);
			case Colour.BgBase:
				return Color.Black;
			case Colour.BgContainer:
				return "#141414".ToColor();
			case Colour.BgElevated:
				return "#1F1F1F".ToColor();
			case Colour.BgLayout:
				return Color.Black;
			case Colour.Fill:
				return rgba(255, 255, 255, 0.15f);
			case Colour.FillSecondary:
				return rgba(255, 255, 255, 0.12f);
			case Colour.FillTertiary:
				return rgba(255, 255, 255, 0.08f);
			case Colour.FillQuaternary:
				return rgba(255, 255, 255, 0.04f);
			case Colour.BorderColor:
				return "#424242".ToColor();
			case Colour.BorderSecondary:
				return "#303030".ToColor();
			case Colour.BorderColorDisable:
				return Color.FromArgb(66, 66, 66);
			case Colour.Split:
				return rgba(253, 253, 253, 0.12f);
			case Colour.HoverBg:
				return rgba(255, 255, 255, 0.06f);
			case Colour.HoverColor:
				return rgba(255, 255, 255, 0.88f);
			case Colour.SliderHandleColorDisabled:
				return "#4F4F4F".ToColor();
			}
			break;
		}
		return Color.Transparent;
	}

	public static void Set(this Colour id, Color value)
	{
		string key = id.ToString();
		if (colors.ContainsKey(key))
		{
			colors[key] = value;
		}
		else
		{
			colors.Add(key, value);
		}
	}

	public static void Set(this Colour id, Color value, string control)
	{
		string key = id.ToString() + control;
		if (colors.ContainsKey(key))
		{
			colors[key] = value;
		}
		else
		{
			colors.Add(key, value);
		}
	}

	public static void SetPrimary(Color primary)
	{
		Colour.Primary.Set(primary);
		List<Color> list = primary.GenerateColors();
		if (Config.Mode == TMode.Light)
		{
			Colour.PrimaryBg.Set(list[0]);
			Colour.PrimaryBgHover.Set(list[1]);
			Colour.PrimaryBorder.Set(list[2]);
			Colour.PrimaryBorderHover.Set(list[3]);
		}
		else
		{
			Colour.PrimaryBg.Set(list[9]);
			Colour.PrimaryBgHover.Set(list[8]);
			Colour.PrimaryBorder.Set(list[5]);
			Colour.PrimaryBorderHover.Set(list[6]);
		}
		Colour.PrimaryHover.Set(list[4]);
		Colour.PrimaryActive.Set(list[6]);
	}

	public static void SetSuccess(Color success)
	{
		Colour.Success.Set(success);
		List<Color> list = success.GenerateColors();
		if (Config.Mode == TMode.Light)
		{
			Colour.SuccessBg.Set(list[0]);
			Colour.SuccessHover.Set(list[2]);
			Colour.SuccessBorder.Set(list[2]);
		}
		else
		{
			Colour.SuccessBg.Set(list[9]);
			Colour.SuccessHover.Set(list[5]);
			Colour.SuccessBorder.Set(list[5]);
		}
		Colour.SuccessActive.Set(list[6]);
	}

	public static void SetWarning(Color warning)
	{
		Colour.Warning.Set(warning);
		List<Color> list = warning.GenerateColors();
		if (Config.Mode == TMode.Light)
		{
			Colour.WarningBg.Set(list[0]);
			Colour.WarningHover.Set(list[2]);
			Colour.WarningBorder.Set(list[2]);
		}
		else
		{
			Colour.WarningBg.Set(list[9]);
			Colour.WarningHover.Set(list[5]);
			Colour.WarningBorder.Set(list[5]);
		}
		Colour.WarningActive.Set(list[6]);
	}

	public static void SetError(Color error)
	{
		Colour.Error.Set(error);
		List<Color> list = error.GenerateColors();
		if (Config.Mode == TMode.Light)
		{
			Colour.ErrorBg.Set(list[0]);
			Colour.ErrorHover.Set(list[2]);
			Colour.ErrorBorder.Set(list[2]);
		}
		else
		{
			Colour.ErrorBg.Set(list[9]);
			Colour.ErrorHover.Set(list[5]);
			Colour.ErrorBorder.Set(list[5]);
		}
		Colour.ErrorActive.Set(list[6]);
	}

	public static void SetInfo(Color info)
	{
		Colour.Info.Set(info);
		List<Color> list = info.GenerateColors();
		if (Config.Mode == TMode.Light)
		{
			Colour.InfoBg.Set(list[0]);
			Colour.InfoHover.Set(list[2]);
			Colour.InfoBorder.Set(list[2]);
		}
		else
		{
			Colour.InfoBg.Set(list[9]);
			Colour.InfoHover.Set(list[5]);
			Colour.InfoBorder.Set(list[5]);
		}
		Colour.InfoActive.Set(list[6]);
	}

	public static void LoadCustom(this Dictionary<string, Color> color)
	{
		colors = color;
		EventHub.Dispatch(EventType.THEME);
	}

	public static void LoadCustom(this Dictionary<string, string> color)
	{
		Dictionary<string, Color> dictionary = new Dictionary<string, Color>(color.Count);
		foreach (KeyValuePair<string, string> item in color)
		{
			dictionary.Add(item.Key, item.Value.ToColor());
		}
		colors = dictionary;
		EventHub.Dispatch(EventType.THEME);
	}

	public static void Clear()
	{
		colors.Clear();
		EventHub.Dispatch(EventType.THEME);
	}

	public static bool ColorMode(this Color color)
	{
		return (color.R * 299 + color.G * 587 + color.B * 114) / 1000 > 128;
	}

	public static Color shade(this Color shadeColor)
	{
		if (shadeColor.R > shadeColor.B)
		{
			return shadeColor.darken(shadeColor.ToHSL().l * warmDark).spin(warmRotate).HSLToColor();
		}
		return shadeColor.darken(shadeColor.ToHSL().l * coldDark).spin(coldRotate).HSLToColor();
	}

	public static HSL darken(this Color color, float amount)
	{
		HSL hSL = color.ToHSL();
		hSL.l -= amount / 100f;
		hSL.l = clamp01(hSL.l);
		return hSL;
	}

	private static HSL spin(this HSL hsl, float amount)
	{
		float num = (hsl.h + amount) % 360f;
		hsl.h = ((num < 0f) ? (360f + num) : num);
		return hsl;
	}

	private static float clamp01(float val)
	{
		return Math.Min(1f, Math.Max(0f, val));
	}

	public static List<Color> GenerateColors(this Color primaryColor)
	{
		HSV hsv = primaryColor.ToHSV();
		List<Color> list = new List<Color>(lightColorCount + darkColorCount);
		for (int num = lightColorCount; num > 0; num--)
		{
			list.Add(GenerateColor(hsv, num, isLight: true));
		}
		list.Add(primaryColor);
		for (int i = 1; i <= darkColorCount; i++)
		{
			list.Add(GenerateColor(hsv, i, isLight: false));
		}
		return list;
	}

	public static Color GenerateColor(HSV hsv, int i, bool isLight)
	{
		return HSVToColor(getHue(hsv, i, isLight), getSaturation(hsv, i, isLight), getValue(hsv, i, isLight));
	}

	public static float getHue(HSV hsv, int i, bool isLight)
	{
		float num = ((!(hsv.h >= 60f) || !(hsv.h <= 240f)) ? (isLight ? (hsv.h + (float)(hueStep * i)) : (hsv.h - (float)(hueStep * i))) : (isLight ? (hsv.h - (float)(hueStep * i)) : (hsv.h + (float)(hueStep * i))));
		if (num < 0f)
		{
			num += 360f;
		}
		else if (num >= 360f)
		{
			num -= 360f;
		}
		return num;
	}

	public static float getSaturation(HSV hsv, int i, bool isLight)
	{
		if (hsv.h == 0f && hsv.s == 0f)
		{
			return hsv.s;
		}
		float num = (isLight ? (hsv.s - saturationStep * (float)i) : ((i != darkColorCount) ? (hsv.s + saturationStep2 * (float)i) : (hsv.s + saturationStep)));
		if (num > 1f)
		{
			num = 1f;
		}
		if (isLight && i == lightColorCount && (double)num > 0.1)
		{
			num = 0.1f;
		}
		if ((double)num < 0.06)
		{
			num = 0.06f;
		}
		return num;
	}

	public static float getValue(HSV hsv, int i, bool isLight)
	{
		float num = ((!isLight) ? (hsv.v - brightnessStep2 * (float)i) : (hsv.v + brightnessStep1 * (float)i));
		if (num > 1f)
		{
			num = 1f;
		}
		return num;
	}

	public static HSV ToHSV(this Color color)
	{
		float num = (float)(int)Math.Min(Math.Min(color.R, color.G), color.B) / 255f;
		float num2 = (float)(int)Math.Max(Math.Max(color.R, color.G), color.B) / 255f;
		return new HSV(color.GetHue(), (num2 == 0f) ? 0f : ((num2 - num) / num2), num2);
	}

	public static Color HSVToColor(this HSV hsv, float alpha = 1f)
	{
		return HSVToColor(hsv.h, hsv.s, hsv.v, alpha);
	}

	public static Color HSVToColor(float hue, float saturation, float value, float alpha = 1f)
	{
		int num = Convert.ToInt32(Math.Floor(hue / 60f)) % 6;
		float num2 = hue / 60f - (float)Math.Floor((double)hue / 60.0);
		float num3 = value * (1f - saturation);
		float num4 = value * (1f - num2 * saturation);
		float num5 = value * (1f - (1f - num2) * saturation);
		return num switch
		{
			0 => rgba(value, num5, num3, alpha), 
			1 => rgba(num4, value, num3, alpha), 
			2 => rgba(num3, value, num5, alpha), 
			3 => rgba(num3, num4, value, alpha), 
			4 => rgba(num5, num3, value, alpha), 
			_ => rgba(value, num3, num4, alpha), 
		};
	}

	public static HSL ToHSL(this Color color)
	{
		float num = (float)(int)Math.Min(Math.Min(color.R, color.G), color.B) / 255f;
		float num2 = (float)(int)Math.Max(Math.Max(color.R, color.G), color.B) / 255f;
		float num3 = (num2 + num) / 2f;
		if (num3 == 0f || num == num2)
		{
			return new HSL(color.GetHue(), 0f, num3);
		}
		if (num3 > 0f && num3 <= 0.5f)
		{
			return new HSL(color.GetHue(), (num2 - num) / (num2 + num), num3);
		}
		return new HSL(color.GetHue(), (num2 - num) / (2f - (num2 + num)), num3);
	}

	public static Color HSLToColor(this HSL hsl, float alpha = 1f)
	{
		return HSLToColor(hsl.h, hsl.s, hsl.l, alpha);
	}

	public static Color HSLToColor(float hue, float saturation, float lightness, float alpha = 1f)
	{
		float num = (1f - Math.Abs(2f * lightness - 1f)) * saturation;
		float num2 = hue % 360f / 60f;
		float num3 = num * (1f - Math.Abs(num2 % 2f - 1f));
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		if (num2 >= 0f && num2 < 1f)
		{
			num4 = num;
			num5 = num3;
			num6 = 0f;
		}
		else if (num2 >= 1f && num2 <= 2f)
		{
			num4 = num3;
			num5 = num;
			num6 = 0f;
		}
		else if (num2 >= 2f && num2 <= 3f)
		{
			num4 = 0f;
			num5 = num;
			num6 = num3;
		}
		else if (num2 > 3f && num2 <= 4f)
		{
			num4 = 0f;
			num5 = num3;
			num6 = num;
		}
		else if (num2 > 4f && num2 <= 5f)
		{
			num4 = num3;
			num5 = 9f;
			num6 = num;
		}
		else if (num2 > 5f && num2 <= 6f)
		{
			num4 = num;
			num5 = 0f;
			num6 = num3;
		}
		float num7 = lightness - num / 2f;
		return rgba(Math.Abs(num4 + num7), Math.Abs(num5 + num7), Math.Abs(num6 + num7), alpha);
	}

	public static Color rgba(int r, int g, int b, float a = 1f)
	{
		return Color.FromArgb((int)Math.Round(255f * a), r, g, b);
	}

	public static Color rgba(this Color color, float a = 1f)
	{
		return rgba(color.R, color.G, color.B, a);
	}

	public static Color rgba(float r, float g, float b, float a = 1f)
	{
		if (r < 0f)
		{
			r = 0f;
		}
		else if (r > 1f)
		{
			r = 1f;
		}
		if (g < 0f)
		{
			g = 0f;
		}
		else if (g > 1f)
		{
			g = 1f;
		}
		if (b < 0f)
		{
			b = 0f;
		}
		else if (b > 1f)
		{
			b = 1f;
		}
		return Color.FromArgb((int)Math.Round(255f * a), (int)Math.Round(255f * r), (int)Math.Round(255f * g), (int)Math.Round(255f * b));
	}

	public static Color ToColor(this string hex)
	{
		try
		{
			if (hex != null && hex.Length > 5)
			{
				if (hex.StartsWith("#"))
				{
					hex = hex.Substring(1);
				}
				if (hex.Length == 6)
				{
					return Color.FromArgb(hex.Substring(0, 2).HexToInt(), hex.Substring(2, 2).HexToInt(), hex.Substring(4, 2).HexToInt());
				}
				if (hex.Length == 8)
				{
					return Color.FromArgb(hex.Substring(6, 2).HexToInt(), hex.Substring(0, 2).HexToInt(), hex.Substring(2, 2).HexToInt(), hex.Substring(4, 2).HexToInt());
				}
			}
		}
		catch
		{
		}
		return Color.Black;
	}

	public static string ToHex(this Color color)
	{
		if (color.A == byte.MaxValue)
		{
			return $"{color.R:X2}{color.G:X2}{color.B:X2}";
		}
		return $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
	}

	private static int HexToInt(this string str)
	{
		return int.Parse(str, NumberStyles.AllowHexSpecifier);
	}

	public static Color BlendColors(this Color baseColor, int alpha, Color overlay)
	{
		return baseColor.BlendColors(Helper.ToColor(alpha, overlay));
	}

	public static Color BlendColors(this Color baseColor, Color overlay)
	{
		byte a = baseColor.A;
		byte a2 = overlay.A;
		byte b = (byte)(a2 + a * (255 - a2) / 255);
		if (b == 0)
		{
			return Color.Transparent;
		}
		byte red = (byte)((overlay.R * a2 + baseColor.R * a * (255 - a2) / 255) / b);
		byte green = (byte)((overlay.G * a2 + baseColor.G * a * (255 - a2) / 255) / b);
		byte blue = (byte)((overlay.B * a2 + baseColor.B * a * (255 - a2) / 255) / b);
		return Color.FromArgb(b, red, green, blue);
	}
}
