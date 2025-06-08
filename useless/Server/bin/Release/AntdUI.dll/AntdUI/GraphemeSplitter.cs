using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AntdUI;

public class GraphemeSplitter
{
	protected struct RangeInfo
	{
		public int Start;

		public int End;

		public int Type;
	}

	public enum STRE_TYPE
	{
		STR,
		SVG,
		BASE64IMG
	}

	public const int Other = 0;

	public const int CR = 1;

	public const int LF = 2;

	public const int Control = 3;

	public const int Extend = 4;

	public const int Regional_Indicator = 5;

	public const int SpacingMark = 6;

	public const int L = 7;

	public const int V = 8;

	public const int T = 9;

	public const int LV = 10;

	public const int LVT = 11;

	public const int Prepend = 12;

	public const int E_Base = 13;

	public const int E_Modifier = 14;

	public const int ZWJ = 15;

	public const int Glue_After_Zwj = 16;

	public const int E_Base_GAZ = 17;

	public const int Extended_Pictographic = 18;

	private static List<RangeInfo> m_lst_code_range;

	public static int Each(string strText, Func<string, int, int, bool> cb)
	{
		return Each(strText, 0, cb);
	}

	public static int Each(string strText, int nIndex, Func<string, int, int, bool> cb)
	{
		int num = 0;
		if (string.IsNullOrEmpty(strText) || nIndex >= strText.Length)
		{
			return 0;
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		while (nIndex < strText.Length && char.IsLowSurrogate(strText, nIndex))
		{
			nIndex++;
			num3++;
		}
		if (num3 != 0)
		{
			num++;
			if (!CharCompleted(strText, nIndex - num3, num3, cb))
			{
				return num;
			}
		}
		num2 = nIndex;
		int codePoint = GetCodePoint(strText, nIndex);
		num4 = ((codePoint < 65536) ? 1 : 2);
		num5 = GetBreakProperty(codePoint);
		nIndex += num4;
		num3 = num4;
		List<int> list = new List<int> { num5 };
		while (nIndex < strText.Length)
		{
			int codePoint2 = GetCodePoint(strText, nIndex);
			num4 = ((codePoint2 < 65536) ? 1 : 2);
			num6 = GetBreakProperty(codePoint2);
			if (ShouldBreak(num6, list))
			{
				num++;
				if (!CharCompleted(strText, num2, num3, cb))
				{
					return num;
				}
				num2 = nIndex;
				num3 = num4;
				list.Clear();
			}
			else
			{
				num3 += num4;
			}
			list.Add(num6);
			nIndex += num4;
			num5 = num6;
		}
		if (num3 != 0)
		{
			num++;
			CharCompleted(strText, num2, num3, cb);
		}
		return num;
	}

	public static int EachT(string strText, int nIndex, Func<string, STRE_TYPE, int, int, bool> cb)
	{
		int num = 0;
		if (string.IsNullOrEmpty(strText) || nIndex >= strText.Length)
		{
			return 0;
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		while (nIndex < strText.Length && char.IsLowSurrogate(strText, nIndex))
		{
			nIndex++;
			num3++;
		}
		if (num3 != 0)
		{
			num++;
			if (!cb(strText, STRE_TYPE.STR, nIndex - num3, num3))
			{
				return num;
			}
		}
		num2 = nIndex;
		int codePoint = GetCodePoint(strText, nIndex);
		num4 = ((codePoint < 65536) ? 1 : 2);
		num5 = GetBreakProperty(codePoint);
		nIndex += num4;
		num3 = num4;
		List<int> list = new List<int> { num5 };
		while (nIndex < strText.Length)
		{
			int codePoint2 = GetCodePoint(strText, nIndex);
			num4 = ((codePoint2 < 65536) ? 1 : 2);
			num6 = GetBreakProperty(codePoint2);
			int num7 = num4;
			if (ShouldBreak(num6, list))
			{
				num++;
				int endIndex2;
				if (ReadSvg(strText, num2, num3, out var endIndex))
				{
					if (!cb(strText, STRE_TYPE.SVG, num2, endIndex))
					{
						return num;
					}
					num2 += endIndex;
					num7 = endIndex;
					num3 = num4;
				}
				else if (ReadBase64Image(strText, num2, num3, out endIndex2))
				{
					if (!cb(strText, STRE_TYPE.BASE64IMG, num2, endIndex2))
					{
						return num;
					}
					num2 += endIndex2;
					num7 = endIndex2;
					num3 = num4;
				}
				else
				{
					if (!cb(strText, STRE_TYPE.STR, num2, num3))
					{
						return num;
					}
					num2 = nIndex;
					num3 = num4;
				}
				list.Clear();
			}
			else
			{
				num3 += num4;
			}
			list.Add(num6);
			nIndex += num7;
			num5 = num6;
		}
		if (num3 != 0 && num2 < strText.Length)
		{
			num++;
			cb(strText, STRE_TYPE.STR, num2, num3);
		}
		return num;
	}

	private static bool ReadSvg(string strText, int start, int len, out int endIndex)
	{
		if (strText.Substring(start, len) == "<" && start + 5 < strText.Length && strText.Substring(start, 4) == "<svg")
		{
			int num = strText.IndexOf("</svg>", start + 5, StringComparison.OrdinalIgnoreCase);
			if (num > 0)
			{
				endIndex = num + 6 - start;
				return true;
			}
		}
		endIndex = -1;
		return false;
	}

	private static bool ReadBase64Image(string strText, int start, int len, out int endIndex)
	{
		if (strText.Substring(start, len) == "d" && start + 20 < strText.Length && strText.Substring(start, 11) == "data:image/" && strText.IndexOf(";base64,", start + 12, StringComparison.OrdinalIgnoreCase) > 0)
		{
			Match match = new Regex("data:image/(?<type>.+?);base64,(?<data>[A-Za-z0-9+/=]+)").Match(strText.Substring(start, strText.Length - start));
			if (match.Success && match.Index == 0)
			{
				endIndex = match.Value.Length;
				return true;
			}
		}
		endIndex = -1;
		return false;
	}

	public static int GetCodePoint(string strText, int nIndex)
	{
		if (strText[nIndex] < '\ud800' || strText[nIndex] > '\udfff')
		{
			return strText[nIndex];
		}
		if (char.IsHighSurrogate(strText, nIndex))
		{
			if (nIndex + 1 >= strText.Length)
			{
				return 0;
			}
		}
		else if (--nIndex < 0)
		{
			return 0;
		}
		return ((strText[nIndex] & 0x3FF) << 10) + (strText[nIndex + 1] & 0x3FF) + 65536;
	}

	private static bool CharCompleted(string strText, int nIndex, int nLen, Func<string, int, int, bool> cb_bool)
	{
		if (!cb_bool(strText, nIndex, nLen))
		{
			return false;
		}
		return true;
	}

	static GraphemeSplitter()
	{
		m_lst_code_range = new List<RangeInfo>();
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 0,
			End = 9,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 11,
			End = 12,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 14,
			End = 31,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127,
			End = 159,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 768,
			End = 879,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1155,
			End = 1159,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1160,
			End = 1161,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1425,
			End = 1469,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1473,
			End = 1474,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1476,
			End = 1477,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1536,
			End = 1541,
			Type = 12
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1552,
			End = 1562,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1611,
			End = 1631,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1750,
			End = 1756,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1759,
			End = 1764,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1767,
			End = 1768,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1770,
			End = 1773,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1840,
			End = 1866,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 1958,
			End = 1968,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2027,
			End = 2035,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2070,
			End = 2073,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2075,
			End = 2083,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2085,
			End = 2087,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2089,
			End = 2093,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2137,
			End = 2139,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2192,
			End = 2193,
			Type = 12
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2200,
			End = 2207,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2250,
			End = 2273,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2275,
			End = 2306,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2366,
			End = 2368,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2369,
			End = 2376,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2377,
			End = 2380,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2382,
			End = 2383,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2385,
			End = 2391,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2402,
			End = 2403,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2434,
			End = 2435,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2495,
			End = 2496,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2497,
			End = 2500,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2503,
			End = 2504,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2507,
			End = 2508,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2530,
			End = 2531,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2561,
			End = 2562,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2622,
			End = 2624,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2625,
			End = 2626,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2631,
			End = 2632,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2635,
			End = 2637,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2672,
			End = 2673,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2689,
			End = 2690,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2750,
			End = 2752,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2753,
			End = 2757,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2759,
			End = 2760,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2763,
			End = 2764,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2786,
			End = 2787,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2810,
			End = 2815,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2818,
			End = 2819,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2881,
			End = 2884,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2887,
			End = 2888,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2891,
			End = 2892,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2901,
			End = 2902,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 2914,
			End = 2915,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3009,
			End = 3010,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3014,
			End = 3016,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3018,
			End = 3020,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3073,
			End = 3075,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3134,
			End = 3136,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3137,
			End = 3140,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3142,
			End = 3144,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3146,
			End = 3149,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3157,
			End = 3158,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3170,
			End = 3171,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3202,
			End = 3203,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3264,
			End = 3265,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3267,
			End = 3268,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3271,
			End = 3272,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3274,
			End = 3275,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3276,
			End = 3277,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3285,
			End = 3286,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3298,
			End = 3299,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3328,
			End = 3329,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3330,
			End = 3331,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3387,
			End = 3388,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3391,
			End = 3392,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3393,
			End = 3396,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3398,
			End = 3400,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3402,
			End = 3404,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3426,
			End = 3427,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3458,
			End = 3459,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3536,
			End = 3537,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3538,
			End = 3540,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3544,
			End = 3550,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3570,
			End = 3571,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3636,
			End = 3642,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3655,
			End = 3662,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3764,
			End = 3772,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3784,
			End = 3789,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3864,
			End = 3865,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3902,
			End = 3903,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3953,
			End = 3966,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3968,
			End = 3972,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3974,
			End = 3975,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3981,
			End = 3991,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 3993,
			End = 4028,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4141,
			End = 4144,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4146,
			End = 4151,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4153,
			End = 4154,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4155,
			End = 4156,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4157,
			End = 4158,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4182,
			End = 4183,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4184,
			End = 4185,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4190,
			End = 4192,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4209,
			End = 4212,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4229,
			End = 4230,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4352,
			End = 4447,
			Type = 7
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4448,
			End = 4519,
			Type = 8
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4520,
			End = 4607,
			Type = 9
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 4957,
			End = 4959,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 5906,
			End = 5908,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 5938,
			End = 5939,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 5970,
			End = 5971,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6002,
			End = 6003,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6068,
			End = 6069,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6071,
			End = 6077,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6078,
			End = 6085,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6087,
			End = 6088,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6089,
			End = 6099,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6155,
			End = 6157,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6277,
			End = 6278,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6432,
			End = 6434,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6435,
			End = 6438,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6439,
			End = 6440,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6441,
			End = 6443,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6448,
			End = 6449,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6451,
			End = 6456,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6457,
			End = 6459,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6679,
			End = 6680,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6681,
			End = 6682,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6744,
			End = 6750,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6757,
			End = 6764,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6765,
			End = 6770,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6771,
			End = 6780,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6832,
			End = 6845,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6847,
			End = 6862,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6912,
			End = 6915,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6966,
			End = 6970,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6973,
			End = 6977,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 6979,
			End = 6980,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7019,
			End = 7027,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7040,
			End = 7041,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7074,
			End = 7077,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7078,
			End = 7079,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7080,
			End = 7081,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7083,
			End = 7085,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7144,
			End = 7145,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7146,
			End = 7148,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7151,
			End = 7153,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7154,
			End = 7155,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7204,
			End = 7211,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7212,
			End = 7219,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7220,
			End = 7221,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7222,
			End = 7223,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7376,
			End = 7378,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7380,
			End = 7392,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7394,
			End = 7400,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7416,
			End = 7417,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 7616,
			End = 7679,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8206,
			End = 8207,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8234,
			End = 8238,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8288,
			End = 8292,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8294,
			End = 8303,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8400,
			End = 8412,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8413,
			End = 8416,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8418,
			End = 8420,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8421,
			End = 8432,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8596,
			End = 8601,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8617,
			End = 8618,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 8986,
			End = 8987,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9193,
			End = 9196,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9197,
			End = 9198,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9201,
			End = 9202,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9208,
			End = 9210,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9642,
			End = 9643,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9723,
			End = 9726,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9728,
			End = 9729,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9730,
			End = 9731,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9735,
			End = 9741,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9743,
			End = 9744,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9748,
			End = 9749,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9750,
			End = 9751,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9753,
			End = 9756,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9758,
			End = 9759,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9762,
			End = 9763,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9764,
			End = 9765,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9767,
			End = 9769,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9771,
			End = 9773,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9776,
			End = 9783,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9784,
			End = 9785,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9787,
			End = 9791,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9795,
			End = 9799,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9800,
			End = 9811,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9812,
			End = 9822,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9825,
			End = 9826,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9829,
			End = 9830,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9833,
			End = 9850,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9852,
			End = 9853,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9856,
			End = 9861,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9872,
			End = 9873,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9878,
			End = 9879,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9883,
			End = 9884,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9885,
			End = 9887,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9888,
			End = 9889,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9890,
			End = 9894,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9896,
			End = 9897,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9898,
			End = 9899,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9900,
			End = 9903,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9904,
			End = 9905,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9906,
			End = 9916,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9917,
			End = 9918,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9919,
			End = 9923,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9924,
			End = 9925,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9926,
			End = 9927,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9929,
			End = 9933,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9941,
			End = 9960,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9963,
			End = 9967,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9968,
			End = 9969,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9970,
			End = 9971,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9975,
			End = 9977,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9979,
			End = 9980,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9982,
			End = 9985,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9987,
			End = 9988,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 9992,
			End = 9996,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 10000,
			End = 10001,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 10035,
			End = 10036,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 10067,
			End = 10069,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 10085,
			End = 10087,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 10133,
			End = 10135,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 10548,
			End = 10549,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 11013,
			End = 11015,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 11035,
			End = 11036,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 11503,
			End = 11505,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 11744,
			End = 11775,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 12330,
			End = 12333,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 12334,
			End = 12335,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 12441,
			End = 12442,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 42608,
			End = 42610,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 42612,
			End = 42621,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 42654,
			End = 42655,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 42736,
			End = 42737,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43043,
			End = 43044,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43045,
			End = 43046,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43136,
			End = 43137,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43188,
			End = 43203,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43204,
			End = 43205,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43232,
			End = 43249,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43302,
			End = 43309,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43335,
			End = 43345,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43346,
			End = 43347,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43360,
			End = 43388,
			Type = 7
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43392,
			End = 43394,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43444,
			End = 43445,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43446,
			End = 43449,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43450,
			End = 43451,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43452,
			End = 43453,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43454,
			End = 43456,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43561,
			End = 43566,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43567,
			End = 43568,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43569,
			End = 43570,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43571,
			End = 43572,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43573,
			End = 43574,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43698,
			End = 43700,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43703,
			End = 43704,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43710,
			End = 43711,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43756,
			End = 43757,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 43758,
			End = 43759,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44003,
			End = 44004,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44006,
			End = 44007,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44009,
			End = 44010,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44033,
			End = 44059,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44061,
			End = 44087,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44089,
			End = 44115,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44117,
			End = 44143,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44145,
			End = 44171,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44173,
			End = 44199,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44201,
			End = 44227,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44229,
			End = 44255,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44257,
			End = 44283,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44285,
			End = 44311,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44313,
			End = 44339,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44341,
			End = 44367,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44369,
			End = 44395,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44397,
			End = 44423,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44425,
			End = 44451,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44453,
			End = 44479,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44481,
			End = 44507,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44509,
			End = 44535,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44537,
			End = 44563,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44565,
			End = 44591,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44593,
			End = 44619,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44621,
			End = 44647,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44649,
			End = 44675,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44677,
			End = 44703,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44705,
			End = 44731,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44733,
			End = 44759,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44761,
			End = 44787,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44789,
			End = 44815,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44817,
			End = 44843,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44845,
			End = 44871,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44873,
			End = 44899,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44901,
			End = 44927,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44929,
			End = 44955,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44957,
			End = 44983,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 44985,
			End = 45011,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45013,
			End = 45039,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45041,
			End = 45067,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45069,
			End = 45095,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45097,
			End = 45123,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45125,
			End = 45151,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45153,
			End = 45179,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45181,
			End = 45207,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45209,
			End = 45235,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45237,
			End = 45263,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45265,
			End = 45291,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45293,
			End = 45319,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45321,
			End = 45347,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45349,
			End = 45375,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45377,
			End = 45403,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45405,
			End = 45431,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45433,
			End = 45459,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45461,
			End = 45487,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45489,
			End = 45515,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45517,
			End = 45543,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45545,
			End = 45571,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45573,
			End = 45599,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45601,
			End = 45627,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45629,
			End = 45655,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45657,
			End = 45683,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45685,
			End = 45711,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45713,
			End = 45739,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45741,
			End = 45767,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45769,
			End = 45795,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45797,
			End = 45823,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45825,
			End = 45851,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45853,
			End = 45879,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45881,
			End = 45907,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45909,
			End = 45935,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45937,
			End = 45963,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45965,
			End = 45991,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 45993,
			End = 46019,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46021,
			End = 46047,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46049,
			End = 46075,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46077,
			End = 46103,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46105,
			End = 46131,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46133,
			End = 46159,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46161,
			End = 46187,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46189,
			End = 46215,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46217,
			End = 46243,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46245,
			End = 46271,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46273,
			End = 46299,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46301,
			End = 46327,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46329,
			End = 46355,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46357,
			End = 46383,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46385,
			End = 46411,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46413,
			End = 46439,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46441,
			End = 46467,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46469,
			End = 46495,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46497,
			End = 46523,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46525,
			End = 46551,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46553,
			End = 46579,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46581,
			End = 46607,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46609,
			End = 46635,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46637,
			End = 46663,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46665,
			End = 46691,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46693,
			End = 46719,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46721,
			End = 46747,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46749,
			End = 46775,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46777,
			End = 46803,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46805,
			End = 46831,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46833,
			End = 46859,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46861,
			End = 46887,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46889,
			End = 46915,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46917,
			End = 46943,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46945,
			End = 46971,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 46973,
			End = 46999,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47001,
			End = 47027,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47029,
			End = 47055,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47057,
			End = 47083,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47085,
			End = 47111,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47113,
			End = 47139,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47141,
			End = 47167,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47169,
			End = 47195,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47197,
			End = 47223,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47225,
			End = 47251,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47253,
			End = 47279,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47281,
			End = 47307,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47309,
			End = 47335,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47337,
			End = 47363,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47365,
			End = 47391,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47393,
			End = 47419,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47421,
			End = 47447,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47449,
			End = 47475,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47477,
			End = 47503,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47505,
			End = 47531,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47533,
			End = 47559,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47561,
			End = 47587,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47589,
			End = 47615,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47617,
			End = 47643,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47645,
			End = 47671,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47673,
			End = 47699,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47701,
			End = 47727,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47729,
			End = 47755,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47757,
			End = 47783,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47785,
			End = 47811,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47813,
			End = 47839,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47841,
			End = 47867,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47869,
			End = 47895,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47897,
			End = 47923,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47925,
			End = 47951,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47953,
			End = 47979,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 47981,
			End = 48007,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48009,
			End = 48035,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48037,
			End = 48063,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48065,
			End = 48091,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48093,
			End = 48119,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48121,
			End = 48147,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48149,
			End = 48175,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48177,
			End = 48203,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48205,
			End = 48231,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48233,
			End = 48259,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48261,
			End = 48287,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48289,
			End = 48315,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48317,
			End = 48343,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48345,
			End = 48371,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48373,
			End = 48399,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48401,
			End = 48427,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48429,
			End = 48455,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48457,
			End = 48483,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48485,
			End = 48511,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48513,
			End = 48539,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48541,
			End = 48567,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48569,
			End = 48595,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48597,
			End = 48623,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48625,
			End = 48651,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48653,
			End = 48679,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48681,
			End = 48707,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48709,
			End = 48735,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48737,
			End = 48763,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48765,
			End = 48791,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48793,
			End = 48819,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48821,
			End = 48847,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48849,
			End = 48875,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48877,
			End = 48903,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48905,
			End = 48931,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48933,
			End = 48959,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48961,
			End = 48987,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 48989,
			End = 49015,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49017,
			End = 49043,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49045,
			End = 49071,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49073,
			End = 49099,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49101,
			End = 49127,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49129,
			End = 49155,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49157,
			End = 49183,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49185,
			End = 49211,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49213,
			End = 49239,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49241,
			End = 49267,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49269,
			End = 49295,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49297,
			End = 49323,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49325,
			End = 49351,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49353,
			End = 49379,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49381,
			End = 49407,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49409,
			End = 49435,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49437,
			End = 49463,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49465,
			End = 49491,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49493,
			End = 49519,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49521,
			End = 49547,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49549,
			End = 49575,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49577,
			End = 49603,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49605,
			End = 49631,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49633,
			End = 49659,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49661,
			End = 49687,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49689,
			End = 49715,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49717,
			End = 49743,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49745,
			End = 49771,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49773,
			End = 49799,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49801,
			End = 49827,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49829,
			End = 49855,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49857,
			End = 49883,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49885,
			End = 49911,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49913,
			End = 49939,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49941,
			End = 49967,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49969,
			End = 49995,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 49997,
			End = 50023,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50025,
			End = 50051,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50053,
			End = 50079,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50081,
			End = 50107,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50109,
			End = 50135,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50137,
			End = 50163,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50165,
			End = 50191,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50193,
			End = 50219,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50221,
			End = 50247,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50249,
			End = 50275,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50277,
			End = 50303,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50305,
			End = 50331,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50333,
			End = 50359,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50361,
			End = 50387,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50389,
			End = 50415,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50417,
			End = 50443,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50445,
			End = 50471,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50473,
			End = 50499,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50501,
			End = 50527,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50529,
			End = 50555,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50557,
			End = 50583,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50585,
			End = 50611,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50613,
			End = 50639,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50641,
			End = 50667,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50669,
			End = 50695,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50697,
			End = 50723,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50725,
			End = 50751,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50753,
			End = 50779,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50781,
			End = 50807,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50809,
			End = 50835,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50837,
			End = 50863,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50865,
			End = 50891,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50893,
			End = 50919,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50921,
			End = 50947,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50949,
			End = 50975,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 50977,
			End = 51003,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51005,
			End = 51031,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51033,
			End = 51059,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51061,
			End = 51087,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51089,
			End = 51115,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51117,
			End = 51143,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51145,
			End = 51171,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51173,
			End = 51199,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51201,
			End = 51227,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51229,
			End = 51255,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51257,
			End = 51283,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51285,
			End = 51311,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51313,
			End = 51339,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51341,
			End = 51367,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51369,
			End = 51395,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51397,
			End = 51423,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51425,
			End = 51451,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51453,
			End = 51479,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51481,
			End = 51507,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51509,
			End = 51535,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51537,
			End = 51563,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51565,
			End = 51591,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51593,
			End = 51619,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51621,
			End = 51647,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51649,
			End = 51675,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51677,
			End = 51703,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51705,
			End = 51731,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51733,
			End = 51759,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51761,
			End = 51787,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51789,
			End = 51815,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51817,
			End = 51843,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51845,
			End = 51871,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51873,
			End = 51899,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51901,
			End = 51927,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51929,
			End = 51955,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51957,
			End = 51983,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 51985,
			End = 52011,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52013,
			End = 52039,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52041,
			End = 52067,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52069,
			End = 52095,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52097,
			End = 52123,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52125,
			End = 52151,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52153,
			End = 52179,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52181,
			End = 52207,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52209,
			End = 52235,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52237,
			End = 52263,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52265,
			End = 52291,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52293,
			End = 52319,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52321,
			End = 52347,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52349,
			End = 52375,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52377,
			End = 52403,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52405,
			End = 52431,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52433,
			End = 52459,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52461,
			End = 52487,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52489,
			End = 52515,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52517,
			End = 52543,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52545,
			End = 52571,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52573,
			End = 52599,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52601,
			End = 52627,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52629,
			End = 52655,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52657,
			End = 52683,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52685,
			End = 52711,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52713,
			End = 52739,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52741,
			End = 52767,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52769,
			End = 52795,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52797,
			End = 52823,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52825,
			End = 52851,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52853,
			End = 52879,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52881,
			End = 52907,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52909,
			End = 52935,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52937,
			End = 52963,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52965,
			End = 52991,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 52993,
			End = 53019,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53021,
			End = 53047,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53049,
			End = 53075,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53077,
			End = 53103,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53105,
			End = 53131,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53133,
			End = 53159,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53161,
			End = 53187,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53189,
			End = 53215,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53217,
			End = 53243,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53245,
			End = 53271,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53273,
			End = 53299,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53301,
			End = 53327,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53329,
			End = 53355,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53357,
			End = 53383,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53385,
			End = 53411,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53413,
			End = 53439,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53441,
			End = 53467,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53469,
			End = 53495,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53497,
			End = 53523,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53525,
			End = 53551,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53553,
			End = 53579,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53581,
			End = 53607,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53609,
			End = 53635,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53637,
			End = 53663,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53665,
			End = 53691,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53693,
			End = 53719,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53721,
			End = 53747,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53749,
			End = 53775,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53777,
			End = 53803,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53805,
			End = 53831,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53833,
			End = 53859,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53861,
			End = 53887,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53889,
			End = 53915,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53917,
			End = 53943,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53945,
			End = 53971,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 53973,
			End = 53999,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54001,
			End = 54027,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54029,
			End = 54055,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54057,
			End = 54083,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54085,
			End = 54111,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54113,
			End = 54139,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54141,
			End = 54167,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54169,
			End = 54195,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54197,
			End = 54223,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54225,
			End = 54251,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54253,
			End = 54279,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54281,
			End = 54307,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54309,
			End = 54335,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54337,
			End = 54363,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54365,
			End = 54391,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54393,
			End = 54419,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54421,
			End = 54447,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54449,
			End = 54475,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54477,
			End = 54503,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54505,
			End = 54531,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54533,
			End = 54559,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54561,
			End = 54587,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54589,
			End = 54615,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54617,
			End = 54643,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54645,
			End = 54671,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54673,
			End = 54699,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54701,
			End = 54727,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54729,
			End = 54755,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54757,
			End = 54783,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54785,
			End = 54811,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54813,
			End = 54839,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54841,
			End = 54867,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54869,
			End = 54895,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54897,
			End = 54923,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54925,
			End = 54951,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54953,
			End = 54979,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 54981,
			End = 55007,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55009,
			End = 55035,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55037,
			End = 55063,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55065,
			End = 55091,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55093,
			End = 55119,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55121,
			End = 55147,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55149,
			End = 55175,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55177,
			End = 55203,
			Type = 11
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55216,
			End = 55238,
			Type = 8
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 55243,
			End = 55291,
			Type = 9
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 65024,
			End = 65039,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 65056,
			End = 65071,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 65438,
			End = 65439,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 65520,
			End = 65528,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 65529,
			End = 65531,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 66422,
			End = 66426,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 68097,
			End = 68099,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 68101,
			End = 68102,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 68108,
			End = 68111,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 68152,
			End = 68154,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 68325,
			End = 68326,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 68900,
			End = 68903,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69291,
			End = 69292,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69446,
			End = 69456,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69506,
			End = 69509,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69688,
			End = 69702,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69747,
			End = 69748,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69759,
			End = 69761,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69808,
			End = 69810,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69811,
			End = 69814,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69815,
			End = 69816,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69817,
			End = 69818,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69888,
			End = 69890,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69927,
			End = 69931,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69933,
			End = 69940,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 69957,
			End = 69958,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70016,
			End = 70017,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70067,
			End = 70069,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70070,
			End = 70078,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70079,
			End = 70080,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70082,
			End = 70083,
			Type = 12
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70089,
			End = 70092,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70188,
			End = 70190,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70191,
			End = 70193,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70194,
			End = 70195,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70198,
			End = 70199,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70368,
			End = 70370,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70371,
			End = 70378,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70400,
			End = 70401,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70402,
			End = 70403,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70459,
			End = 70460,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70465,
			End = 70468,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70471,
			End = 70472,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70475,
			End = 70477,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70498,
			End = 70499,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70502,
			End = 70508,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70512,
			End = 70516,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70709,
			End = 70711,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70712,
			End = 70719,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70720,
			End = 70721,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70722,
			End = 70724,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70833,
			End = 70834,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70835,
			End = 70840,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70843,
			End = 70844,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70847,
			End = 70848,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 70850,
			End = 70851,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71088,
			End = 71089,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71090,
			End = 71093,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71096,
			End = 71099,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71100,
			End = 71101,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71103,
			End = 71104,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71132,
			End = 71133,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71216,
			End = 71218,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71219,
			End = 71226,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71227,
			End = 71228,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71231,
			End = 71232,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71342,
			End = 71343,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71344,
			End = 71349,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71453,
			End = 71455,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71458,
			End = 71461,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71463,
			End = 71467,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71724,
			End = 71726,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71727,
			End = 71735,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71737,
			End = 71738,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71985,
			End = 71989,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71991,
			End = 71992,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 71995,
			End = 71996,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72145,
			End = 72147,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72148,
			End = 72151,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72154,
			End = 72155,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72156,
			End = 72159,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72193,
			End = 72202,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72243,
			End = 72248,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72251,
			End = 72254,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72273,
			End = 72278,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72279,
			End = 72280,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72281,
			End = 72283,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72324,
			End = 72329,
			Type = 12
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72330,
			End = 72342,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72344,
			End = 72345,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72752,
			End = 72758,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72760,
			End = 72765,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72850,
			End = 72871,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72874,
			End = 72880,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72882,
			End = 72883,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 72885,
			End = 72886,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73009,
			End = 73014,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73020,
			End = 73021,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73023,
			End = 73029,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73098,
			End = 73102,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73104,
			End = 73105,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73107,
			End = 73108,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73459,
			End = 73460,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 73461,
			End = 73462,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 78896,
			End = 78904,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 92912,
			End = 92916,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 92976,
			End = 92982,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 94033,
			End = 94087,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 94095,
			End = 94098,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 94192,
			End = 94193,
			Type = 6
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 113821,
			End = 113822,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 113824,
			End = 113827,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 118528,
			End = 118573,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 118576,
			End = 118598,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119143,
			End = 119145,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119150,
			End = 119154,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119155,
			End = 119162,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119163,
			End = 119170,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119173,
			End = 119179,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119210,
			End = 119213,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 119362,
			End = 119364,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 121344,
			End = 121398,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 121403,
			End = 121452,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 121499,
			End = 121503,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 121505,
			End = 121519,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 122880,
			End = 122886,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 122888,
			End = 122904,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 122907,
			End = 122913,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 122915,
			End = 122916,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 122918,
			End = 122922,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 123184,
			End = 123190,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 123628,
			End = 123631,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 125136,
			End = 125142,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 125252,
			End = 125258,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 126976,
			End = 126979,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 126981,
			End = 127182,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127184,
			End = 127231,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127245,
			End = 127247,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127340,
			End = 127343,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127344,
			End = 127345,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127358,
			End = 127359,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127377,
			End = 127386,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127405,
			End = 127461,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127462,
			End = 127487,
			Type = 5
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127489,
			End = 127490,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127491,
			End = 127503,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127538,
			End = 127546,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127548,
			End = 127551,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127561,
			End = 127567,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127568,
			End = 127569,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127570,
			End = 127743,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127744,
			End = 127756,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127757,
			End = 127758,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127763,
			End = 127765,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127766,
			End = 127768,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127773,
			End = 127774,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127775,
			End = 127776,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127778,
			End = 127779,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127780,
			End = 127788,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127789,
			End = 127791,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127792,
			End = 127793,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127794,
			End = 127795,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127796,
			End = 127797,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127799,
			End = 127818,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127820,
			End = 127823,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127825,
			End = 127867,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127870,
			End = 127871,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127872,
			End = 127891,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127892,
			End = 127893,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127894,
			End = 127895,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127897,
			End = 127899,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127900,
			End = 127901,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127902,
			End = 127903,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127904,
			End = 127940,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127947,
			End = 127950,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127951,
			End = 127955,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127956,
			End = 127967,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127968,
			End = 127971,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127973,
			End = 127984,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127985,
			End = 127986,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127992,
			End = 127994,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 127995,
			End = 127999,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128000,
			End = 128007,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128009,
			End = 128011,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128012,
			End = 128014,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128015,
			End = 128016,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128017,
			End = 128018,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128023,
			End = 128041,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128043,
			End = 128062,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128066,
			End = 128100,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128102,
			End = 128107,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128108,
			End = 128109,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128110,
			End = 128172,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128174,
			End = 128181,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128182,
			End = 128183,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128184,
			End = 128235,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128236,
			End = 128237,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128240,
			End = 128244,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128246,
			End = 128247,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128249,
			End = 128252,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128255,
			End = 128258,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128260,
			End = 128263,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128266,
			End = 128276,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128278,
			End = 128299,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128300,
			End = 128301,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128302,
			End = 128317,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128326,
			End = 128328,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128329,
			End = 128330,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128331,
			End = 128334,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128336,
			End = 128347,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128348,
			End = 128359,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128360,
			End = 128366,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128367,
			End = 128368,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128369,
			End = 128370,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128371,
			End = 128377,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128379,
			End = 128390,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128392,
			End = 128393,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128394,
			End = 128397,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128398,
			End = 128399,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128401,
			End = 128404,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128405,
			End = 128406,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128407,
			End = 128419,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128422,
			End = 128423,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128425,
			End = 128432,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128433,
			End = 128434,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128435,
			End = 128443,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128445,
			End = 128449,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128450,
			End = 128452,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128453,
			End = 128464,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128465,
			End = 128467,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128468,
			End = 128475,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128476,
			End = 128478,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128479,
			End = 128480,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128484,
			End = 128487,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128489,
			End = 128494,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128496,
			End = 128498,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128500,
			End = 128505,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128507,
			End = 128511,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128513,
			End = 128518,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128519,
			End = 128520,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128521,
			End = 128525,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128530,
			End = 128532,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128540,
			End = 128542,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128544,
			End = 128549,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128550,
			End = 128551,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128552,
			End = 128555,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128558,
			End = 128559,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128560,
			End = 128563,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128567,
			End = 128576,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128577,
			End = 128580,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128581,
			End = 128591,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128641,
			End = 128642,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128643,
			End = 128645,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128650,
			End = 128651,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128657,
			End = 128659,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128665,
			End = 128666,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128667,
			End = 128673,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128676,
			End = 128677,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128679,
			End = 128685,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128686,
			End = 128689,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128691,
			End = 128693,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128695,
			End = 128696,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128697,
			End = 128702,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128705,
			End = 128709,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128710,
			End = 128714,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128717,
			End = 128719,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128721,
			End = 128722,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128723,
			End = 128724,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128726,
			End = 128727,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128728,
			End = 128732,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128733,
			End = 128735,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128736,
			End = 128741,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128742,
			End = 128744,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128747,
			End = 128748,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128749,
			End = 128751,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128753,
			End = 128754,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128756,
			End = 128758,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128759,
			End = 128760,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128763,
			End = 128764,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128765,
			End = 128767,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128884,
			End = 128895,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128981,
			End = 128991,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 128992,
			End = 129003,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129004,
			End = 129007,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129009,
			End = 129023,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129036,
			End = 129039,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129096,
			End = 129103,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129114,
			End = 129119,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129160,
			End = 129167,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129198,
			End = 129279,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129293,
			End = 129295,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129296,
			End = 129304,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129305,
			End = 129310,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129312,
			End = 129319,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129320,
			End = 129327,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129329,
			End = 129330,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129331,
			End = 129338,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129340,
			End = 129342,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129344,
			End = 129349,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129351,
			End = 129355,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129357,
			End = 129359,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129360,
			End = 129374,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129375,
			End = 129387,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129388,
			End = 129392,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129395,
			End = 129398,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129399,
			End = 129400,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129404,
			End = 129407,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129408,
			End = 129412,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129413,
			End = 129425,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129426,
			End = 129431,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129432,
			End = 129442,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129443,
			End = 129444,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129445,
			End = 129450,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129451,
			End = 129453,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129454,
			End = 129455,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129456,
			End = 129465,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129466,
			End = 129471,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129473,
			End = 129474,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129475,
			End = 129482,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129485,
			End = 129487,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129488,
			End = 129510,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129511,
			End = 129535,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129536,
			End = 129647,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129648,
			End = 129651,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129653,
			End = 129655,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129656,
			End = 129658,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129659,
			End = 129660,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129661,
			End = 129663,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129664,
			End = 129666,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129667,
			End = 129670,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129671,
			End = 129679,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129680,
			End = 129685,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129686,
			End = 129704,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129705,
			End = 129708,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129709,
			End = 129711,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129712,
			End = 129718,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129719,
			End = 129722,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129723,
			End = 129727,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129728,
			End = 129730,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129731,
			End = 129733,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129734,
			End = 129743,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129744,
			End = 129750,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129751,
			End = 129753,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129754,
			End = 129759,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129760,
			End = 129767,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129768,
			End = 129775,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129776,
			End = 129782,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 129783,
			End = 129791,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 130048,
			End = 131069,
			Type = 18
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 917506,
			End = 917535,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 917536,
			End = 917631,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 917632,
			End = 917759,
			Type = 3
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 917760,
			End = 917999,
			Type = 4
		});
		m_lst_code_range.Add(new RangeInfo
		{
			Start = 918000,
			End = 921599,
			Type = 3
		});
	}

	private static bool ShouldBreak(int nRightType, List<int> lstHistoryBreakType)
	{
		int num = lstHistoryBreakType[lstHistoryBreakType.Count - 1];
		if (num == 1 && nRightType == 2)
		{
			return false;
		}
		if (num == 3 || num == 1 || num == 2)
		{
			return true;
		}
		if (nRightType == 3 || nRightType == 1 || nRightType == 2)
		{
			return true;
		}
		if (num == 7 && (nRightType == 7 || nRightType == 8 || nRightType == 10 || nRightType == 11))
		{
			return false;
		}
		if ((num == 10 || num == 8) && (nRightType == 8 || nRightType == 9))
		{
			return false;
		}
		if ((num == 11 || num == 9) && nRightType == 9)
		{
			return false;
		}
		switch (nRightType)
		{
		case 4:
		case 15:
			return false;
		case 6:
			return false;
		default:
			if (num == 12)
			{
				return false;
			}
			if (nRightType == 18 && lstHistoryBreakType.Count >= 2 && lstHistoryBreakType[lstHistoryBreakType.Count - 1] == 15)
			{
				for (int num2 = lstHistoryBreakType.Count - 2; num2 >= 0; num2--)
				{
					switch (lstHistoryBreakType[num2])
					{
					case 18:
						return false;
					default:
						num2 = -1;
						break;
					case 4:
						break;
					}
				}
			}
			if (nRightType == 5)
			{
				int num3 = 0;
				int num4 = lstHistoryBreakType.Count - 1;
				while (num4 >= 0 && lstHistoryBreakType[num4] == 5)
				{
					num3++;
					num4--;
				}
				if (num3 % 2 == 1)
				{
					return false;
				}
			}
			return true;
		}
	}

	private static int GetBreakProperty(int nCodePoint)
	{
		return nCodePoint switch
		{
			1757 => 12, 
			1807 => 12, 
			2274 => 12, 
			3406 => 12, 
			69821 => 12, 
			69837 => 12, 
			71999 => 12, 
			72001 => 12, 
			72250 => 12, 
			73030 => 12, 
			13 => 1, 
			10 => 2, 
			173 => 3, 
			1564 => 3, 
			6158 => 3, 
			8203 => 3, 
			8232 => 3, 
			8233 => 3, 
			8293 => 3, 
			65279 => 3, 
			917504 => 3, 
			917505 => 3, 
			1471 => 4, 
			1479 => 4, 
			1648 => 4, 
			1809 => 4, 
			2045 => 4, 
			2362 => 4, 
			2364 => 4, 
			2381 => 4, 
			2433 => 4, 
			2492 => 4, 
			2494 => 4, 
			2509 => 4, 
			2519 => 4, 
			2558 => 4, 
			2620 => 4, 
			2641 => 4, 
			2677 => 4, 
			2748 => 4, 
			2765 => 4, 
			2817 => 4, 
			2876 => 4, 
			2878 => 4, 
			2879 => 4, 
			2893 => 4, 
			2903 => 4, 
			2946 => 4, 
			3006 => 4, 
			3008 => 4, 
			3021 => 4, 
			3031 => 4, 
			3072 => 4, 
			3076 => 4, 
			3132 => 4, 
			3201 => 4, 
			3260 => 4, 
			3263 => 4, 
			3266 => 4, 
			3270 => 4, 
			3390 => 4, 
			3405 => 4, 
			3415 => 4, 
			3457 => 4, 
			3530 => 4, 
			3535 => 4, 
			3542 => 4, 
			3551 => 4, 
			3633 => 4, 
			3761 => 4, 
			3893 => 4, 
			3895 => 4, 
			3897 => 4, 
			4038 => 4, 
			4226 => 4, 
			4237 => 4, 
			4253 => 4, 
			6086 => 4, 
			6109 => 4, 
			6159 => 4, 
			6313 => 4, 
			6450 => 4, 
			6683 => 4, 
			6742 => 4, 
			6752 => 4, 
			6754 => 4, 
			6783 => 4, 
			6846 => 4, 
			6964 => 4, 
			6965 => 4, 
			6972 => 4, 
			6978 => 4, 
			7142 => 4, 
			7149 => 4, 
			7405 => 4, 
			7412 => 4, 
			8204 => 4, 
			8417 => 4, 
			11647 => 4, 
			42607 => 4, 
			43010 => 4, 
			43014 => 4, 
			43019 => 4, 
			43052 => 4, 
			43263 => 4, 
			43443 => 4, 
			43493 => 4, 
			43587 => 4, 
			43596 => 4, 
			43644 => 4, 
			43696 => 4, 
			43713 => 4, 
			43766 => 4, 
			44005 => 4, 
			44008 => 4, 
			44013 => 4, 
			64286 => 4, 
			66045 => 4, 
			66272 => 4, 
			68159 => 4, 
			69633 => 4, 
			69744 => 4, 
			69826 => 4, 
			70003 => 4, 
			70095 => 4, 
			70196 => 4, 
			70206 => 4, 
			70367 => 4, 
			70462 => 4, 
			70464 => 4, 
			70487 => 4, 
			70726 => 4, 
			70750 => 4, 
			70832 => 4, 
			70842 => 4, 
			70845 => 4, 
			71087 => 4, 
			71229 => 4, 
			71339 => 4, 
			71341 => 4, 
			71351 => 4, 
			71984 => 4, 
			71998 => 4, 
			72003 => 4, 
			72160 => 4, 
			72263 => 4, 
			72767 => 4, 
			73018 => 4, 
			73031 => 4, 
			73109 => 4, 
			73111 => 4, 
			94031 => 4, 
			94180 => 4, 
			119141 => 4, 
			121461 => 4, 
			121476 => 4, 
			123566 => 4, 
			2307 => 6, 
			2363 => 6, 
			2563 => 6, 
			2691 => 6, 
			2761 => 6, 
			2880 => 6, 
			3007 => 6, 
			3262 => 6, 
			3635 => 6, 
			3763 => 6, 
			3967 => 6, 
			4145 => 6, 
			4228 => 6, 
			5909 => 6, 
			5940 => 6, 
			6070 => 6, 
			6741 => 6, 
			6743 => 6, 
			6916 => 6, 
			6971 => 6, 
			7042 => 6, 
			7073 => 6, 
			7082 => 6, 
			7143 => 6, 
			7150 => 6, 
			7393 => 6, 
			7415 => 6, 
			43047 => 6, 
			43395 => 6, 
			43597 => 6, 
			43755 => 6, 
			43765 => 6, 
			44012 => 6, 
			69632 => 6, 
			69634 => 6, 
			69762 => 6, 
			69932 => 6, 
			70018 => 6, 
			70094 => 6, 
			70197 => 6, 
			70463 => 6, 
			70725 => 6, 
			70841 => 6, 
			70846 => 6, 
			70849 => 6, 
			71102 => 6, 
			71230 => 6, 
			71340 => 6, 
			71350 => 6, 
			71462 => 6, 
			71736 => 6, 
			71997 => 6, 
			72000 => 6, 
			72002 => 6, 
			72164 => 6, 
			72249 => 6, 
			72343 => 6, 
			72751 => 6, 
			72766 => 6, 
			72873 => 6, 
			72881 => 6, 
			72884 => 6, 
			73110 => 6, 
			119142 => 6, 
			119149 => 6, 
			44032 => 10, 
			44060 => 10, 
			44088 => 10, 
			44116 => 10, 
			44144 => 10, 
			44172 => 10, 
			44200 => 10, 
			44228 => 10, 
			44256 => 10, 
			44284 => 10, 
			44312 => 10, 
			44340 => 10, 
			44368 => 10, 
			44396 => 10, 
			44424 => 10, 
			44452 => 10, 
			44480 => 10, 
			44508 => 10, 
			44536 => 10, 
			44564 => 10, 
			44592 => 10, 
			44620 => 10, 
			44648 => 10, 
			44676 => 10, 
			44704 => 10, 
			44732 => 10, 
			44760 => 10, 
			44788 => 10, 
			44816 => 10, 
			44844 => 10, 
			44872 => 10, 
			44900 => 10, 
			44928 => 10, 
			44956 => 10, 
			44984 => 10, 
			45012 => 10, 
			45040 => 10, 
			45068 => 10, 
			45096 => 10, 
			45124 => 10, 
			45152 => 10, 
			45180 => 10, 
			45208 => 10, 
			45236 => 10, 
			45264 => 10, 
			45292 => 10, 
			45320 => 10, 
			45348 => 10, 
			45376 => 10, 
			45404 => 10, 
			45432 => 10, 
			45460 => 10, 
			45488 => 10, 
			45516 => 10, 
			45544 => 10, 
			45572 => 10, 
			45600 => 10, 
			45628 => 10, 
			45656 => 10, 
			45684 => 10, 
			45712 => 10, 
			45740 => 10, 
			45768 => 10, 
			45796 => 10, 
			45824 => 10, 
			45852 => 10, 
			45880 => 10, 
			45908 => 10, 
			45936 => 10, 
			45964 => 10, 
			45992 => 10, 
			46020 => 10, 
			46048 => 10, 
			46076 => 10, 
			46104 => 10, 
			46132 => 10, 
			46160 => 10, 
			46188 => 10, 
			46216 => 10, 
			46244 => 10, 
			46272 => 10, 
			46300 => 10, 
			46328 => 10, 
			46356 => 10, 
			46384 => 10, 
			46412 => 10, 
			46440 => 10, 
			46468 => 10, 
			46496 => 10, 
			46524 => 10, 
			46552 => 10, 
			46580 => 10, 
			46608 => 10, 
			46636 => 10, 
			46664 => 10, 
			46692 => 10, 
			46720 => 10, 
			46748 => 10, 
			46776 => 10, 
			46804 => 10, 
			46832 => 10, 
			46860 => 10, 
			46888 => 10, 
			46916 => 10, 
			46944 => 10, 
			46972 => 10, 
			47000 => 10, 
			47028 => 10, 
			47056 => 10, 
			47084 => 10, 
			47112 => 10, 
			47140 => 10, 
			47168 => 10, 
			47196 => 10, 
			47224 => 10, 
			47252 => 10, 
			47280 => 10, 
			47308 => 10, 
			47336 => 10, 
			47364 => 10, 
			47392 => 10, 
			47420 => 10, 
			47448 => 10, 
			47476 => 10, 
			47504 => 10, 
			47532 => 10, 
			47560 => 10, 
			47588 => 10, 
			47616 => 10, 
			47644 => 10, 
			47672 => 10, 
			47700 => 10, 
			47728 => 10, 
			47756 => 10, 
			47784 => 10, 
			47812 => 10, 
			47840 => 10, 
			47868 => 10, 
			47896 => 10, 
			47924 => 10, 
			47952 => 10, 
			47980 => 10, 
			48008 => 10, 
			48036 => 10, 
			48064 => 10, 
			48092 => 10, 
			48120 => 10, 
			48148 => 10, 
			48176 => 10, 
			48204 => 10, 
			48232 => 10, 
			48260 => 10, 
			48288 => 10, 
			48316 => 10, 
			48344 => 10, 
			48372 => 10, 
			48400 => 10, 
			48428 => 10, 
			48456 => 10, 
			48484 => 10, 
			48512 => 10, 
			48540 => 10, 
			48568 => 10, 
			48596 => 10, 
			48624 => 10, 
			48652 => 10, 
			48680 => 10, 
			48708 => 10, 
			48736 => 10, 
			48764 => 10, 
			48792 => 10, 
			48820 => 10, 
			48848 => 10, 
			48876 => 10, 
			48904 => 10, 
			48932 => 10, 
			48960 => 10, 
			48988 => 10, 
			49016 => 10, 
			49044 => 10, 
			49072 => 10, 
			49100 => 10, 
			49128 => 10, 
			49156 => 10, 
			49184 => 10, 
			49212 => 10, 
			49240 => 10, 
			49268 => 10, 
			49296 => 10, 
			49324 => 10, 
			49352 => 10, 
			49380 => 10, 
			49408 => 10, 
			49436 => 10, 
			49464 => 10, 
			49492 => 10, 
			49520 => 10, 
			49548 => 10, 
			49576 => 10, 
			49604 => 10, 
			49632 => 10, 
			49660 => 10, 
			49688 => 10, 
			49716 => 10, 
			49744 => 10, 
			49772 => 10, 
			49800 => 10, 
			49828 => 10, 
			49856 => 10, 
			49884 => 10, 
			49912 => 10, 
			49940 => 10, 
			49968 => 10, 
			49996 => 10, 
			50024 => 10, 
			50052 => 10, 
			50080 => 10, 
			50108 => 10, 
			50136 => 10, 
			50164 => 10, 
			50192 => 10, 
			50220 => 10, 
			50248 => 10, 
			50276 => 10, 
			50304 => 10, 
			50332 => 10, 
			50360 => 10, 
			50388 => 10, 
			50416 => 10, 
			50444 => 10, 
			50472 => 10, 
			50500 => 10, 
			50528 => 10, 
			50556 => 10, 
			50584 => 10, 
			50612 => 10, 
			50640 => 10, 
			50668 => 10, 
			50696 => 10, 
			50724 => 10, 
			50752 => 10, 
			50780 => 10, 
			50808 => 10, 
			50836 => 10, 
			50864 => 10, 
			50892 => 10, 
			50920 => 10, 
			50948 => 10, 
			50976 => 10, 
			51004 => 10, 
			51032 => 10, 
			51060 => 10, 
			51088 => 10, 
			51116 => 10, 
			51144 => 10, 
			51172 => 10, 
			51200 => 10, 
			51228 => 10, 
			51256 => 10, 
			51284 => 10, 
			51312 => 10, 
			51340 => 10, 
			51368 => 10, 
			51396 => 10, 
			51424 => 10, 
			51452 => 10, 
			51480 => 10, 
			51508 => 10, 
			51536 => 10, 
			51564 => 10, 
			51592 => 10, 
			51620 => 10, 
			51648 => 10, 
			51676 => 10, 
			51704 => 10, 
			51732 => 10, 
			51760 => 10, 
			51788 => 10, 
			51816 => 10, 
			51844 => 10, 
			51872 => 10, 
			51900 => 10, 
			51928 => 10, 
			51956 => 10, 
			51984 => 10, 
			52012 => 10, 
			52040 => 10, 
			52068 => 10, 
			52096 => 10, 
			52124 => 10, 
			52152 => 10, 
			52180 => 10, 
			52208 => 10, 
			52236 => 10, 
			52264 => 10, 
			52292 => 10, 
			52320 => 10, 
			52348 => 10, 
			52376 => 10, 
			52404 => 10, 
			52432 => 10, 
			52460 => 10, 
			52488 => 10, 
			52516 => 10, 
			52544 => 10, 
			52572 => 10, 
			52600 => 10, 
			52628 => 10, 
			52656 => 10, 
			52684 => 10, 
			52712 => 10, 
			52740 => 10, 
			52768 => 10, 
			52796 => 10, 
			52824 => 10, 
			52852 => 10, 
			52880 => 10, 
			52908 => 10, 
			52936 => 10, 
			52964 => 10, 
			52992 => 10, 
			53020 => 10, 
			53048 => 10, 
			53076 => 10, 
			53104 => 10, 
			53132 => 10, 
			53160 => 10, 
			53188 => 10, 
			53216 => 10, 
			53244 => 10, 
			53272 => 10, 
			53300 => 10, 
			53328 => 10, 
			53356 => 10, 
			53384 => 10, 
			53412 => 10, 
			53440 => 10, 
			53468 => 10, 
			53496 => 10, 
			53524 => 10, 
			53552 => 10, 
			53580 => 10, 
			53608 => 10, 
			53636 => 10, 
			53664 => 10, 
			53692 => 10, 
			53720 => 10, 
			53748 => 10, 
			53776 => 10, 
			53804 => 10, 
			53832 => 10, 
			53860 => 10, 
			53888 => 10, 
			53916 => 10, 
			53944 => 10, 
			53972 => 10, 
			54000 => 10, 
			54028 => 10, 
			54056 => 10, 
			54084 => 10, 
			54112 => 10, 
			54140 => 10, 
			54168 => 10, 
			54196 => 10, 
			54224 => 10, 
			54252 => 10, 
			54280 => 10, 
			54308 => 10, 
			54336 => 10, 
			54364 => 10, 
			54392 => 10, 
			54420 => 10, 
			54448 => 10, 
			54476 => 10, 
			54504 => 10, 
			54532 => 10, 
			54560 => 10, 
			54588 => 10, 
			54616 => 10, 
			54644 => 10, 
			54672 => 10, 
			54700 => 10, 
			54728 => 10, 
			54756 => 10, 
			54784 => 10, 
			54812 => 10, 
			54840 => 10, 
			54868 => 10, 
			54896 => 10, 
			54924 => 10, 
			54952 => 10, 
			54980 => 10, 
			55008 => 10, 
			55036 => 10, 
			55064 => 10, 
			55092 => 10, 
			55120 => 10, 
			55148 => 10, 
			55176 => 10, 
			8205 => 15, 
			169 => 18, 
			174 => 18, 
			8252 => 18, 
			8265 => 18, 
			8482 => 18, 
			8505 => 18, 
			9000 => 18, 
			9096 => 18, 
			9167 => 18, 
			9199 => 18, 
			9200 => 18, 
			9203 => 18, 
			9410 => 18, 
			9654 => 18, 
			9664 => 18, 
			9732 => 18, 
			9733 => 18, 
			9742 => 18, 
			9745 => 18, 
			9746 => 18, 
			9752 => 18, 
			9757 => 18, 
			9760 => 18, 
			9761 => 18, 
			9766 => 18, 
			9770 => 18, 
			9774 => 18, 
			9775 => 18, 
			9786 => 18, 
			9792 => 18, 
			9793 => 18, 
			9794 => 18, 
			9823 => 18, 
			9824 => 18, 
			9827 => 18, 
			9828 => 18, 
			9831 => 18, 
			9832 => 18, 
			9851 => 18, 
			9854 => 18, 
			9855 => 18, 
			9874 => 18, 
			9875 => 18, 
			9876 => 18, 
			9877 => 18, 
			9880 => 18, 
			9881 => 18, 
			9882 => 18, 
			9895 => 18, 
			9928 => 18, 
			9934 => 18, 
			9935 => 18, 
			9936 => 18, 
			9937 => 18, 
			9938 => 18, 
			9939 => 18, 
			9940 => 18, 
			9961 => 18, 
			9962 => 18, 
			9972 => 18, 
			9973 => 18, 
			9974 => 18, 
			9978 => 18, 
			9981 => 18, 
			9986 => 18, 
			9989 => 18, 
			9997 => 18, 
			9998 => 18, 
			9999 => 18, 
			10002 => 18, 
			10004 => 18, 
			10006 => 18, 
			10013 => 18, 
			10017 => 18, 
			10024 => 18, 
			10052 => 18, 
			10055 => 18, 
			10060 => 18, 
			10062 => 18, 
			10071 => 18, 
			10083 => 18, 
			10084 => 18, 
			10145 => 18, 
			10160 => 18, 
			10175 => 18, 
			11088 => 18, 
			11093 => 18, 
			12336 => 18, 
			12349 => 18, 
			12951 => 18, 
			12953 => 18, 
			126980 => 18, 
			127183 => 18, 
			127279 => 18, 
			127374 => 18, 
			127514 => 18, 
			127535 => 18, 
			127759 => 18, 
			127760 => 18, 
			127761 => 18, 
			127762 => 18, 
			127769 => 18, 
			127770 => 18, 
			127771 => 18, 
			127772 => 18, 
			127777 => 18, 
			127798 => 18, 
			127819 => 18, 
			127824 => 18, 
			127868 => 18, 
			127869 => 18, 
			127896 => 18, 
			127941 => 18, 
			127942 => 18, 
			127943 => 18, 
			127944 => 18, 
			127945 => 18, 
			127946 => 18, 
			127972 => 18, 
			127987 => 18, 
			127988 => 18, 
			127989 => 18, 
			127990 => 18, 
			127991 => 18, 
			128008 => 18, 
			128019 => 18, 
			128020 => 18, 
			128021 => 18, 
			128022 => 18, 
			128042 => 18, 
			128063 => 18, 
			128064 => 18, 
			128065 => 18, 
			128101 => 18, 
			128173 => 18, 
			128238 => 18, 
			128239 => 18, 
			128245 => 18, 
			128248 => 18, 
			128253 => 18, 
			128254 => 18, 
			128259 => 18, 
			128264 => 18, 
			128265 => 18, 
			128277 => 18, 
			128335 => 18, 
			128378 => 18, 
			128391 => 18, 
			128400 => 18, 
			128420 => 18, 
			128421 => 18, 
			128424 => 18, 
			128444 => 18, 
			128481 => 18, 
			128482 => 18, 
			128483 => 18, 
			128488 => 18, 
			128495 => 18, 
			128499 => 18, 
			128506 => 18, 
			128512 => 18, 
			128526 => 18, 
			128527 => 18, 
			128528 => 18, 
			128529 => 18, 
			128533 => 18, 
			128534 => 18, 
			128535 => 18, 
			128536 => 18, 
			128537 => 18, 
			128538 => 18, 
			128539 => 18, 
			128543 => 18, 
			128556 => 18, 
			128557 => 18, 
			128564 => 18, 
			128565 => 18, 
			128566 => 18, 
			128640 => 18, 
			128646 => 18, 
			128647 => 18, 
			128648 => 18, 
			128649 => 18, 
			128652 => 18, 
			128653 => 18, 
			128654 => 18, 
			128655 => 18, 
			128656 => 18, 
			128660 => 18, 
			128661 => 18, 
			128662 => 18, 
			128663 => 18, 
			128664 => 18, 
			128674 => 18, 
			128675 => 18, 
			128678 => 18, 
			128690 => 18, 
			128694 => 18, 
			128703 => 18, 
			128704 => 18, 
			128715 => 18, 
			128716 => 18, 
			128720 => 18, 
			128725 => 18, 
			128745 => 18, 
			128746 => 18, 
			128752 => 18, 
			128755 => 18, 
			128761 => 18, 
			128762 => 18, 
			129008 => 18, 
			129292 => 18, 
			129311 => 18, 
			129328 => 18, 
			129343 => 18, 
			129356 => 18, 
			129393 => 18, 
			129394 => 18, 
			129401 => 18, 
			129402 => 18, 
			129403 => 18, 
			129472 => 18, 
			129483 => 18, 
			129484 => 18, 
			129652 => 18, 
			_ => BinarySearchRangeFromList(0, m_lst_code_range.Count - 1, nCodePoint, m_lst_code_range), 
		};
	}

	private static int BinarySearchRangeFromList(int nStart, int nEnd, int nValue, List<RangeInfo> lst)
	{
		if (nEnd < nStart)
		{
			return 0;
		}
		int num = nStart + (nEnd - nStart) / 2;
		if (lst[num].Start > nValue)
		{
			return BinarySearchRangeFromList(nStart, num - 1, nValue, lst);
		}
		if (lst[num].End < nValue)
		{
			return BinarySearchRangeFromList(num + 1, nEnd, nValue, lst);
		}
		return lst[num].Type;
	}
}
