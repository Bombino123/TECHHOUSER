using System;
using System.Globalization;

namespace ChineseCalendar;

public class ChineseDate
{
	private static readonly ChineseLunisolarCalendar chineseCalendar = new ChineseLunisolarCalendar();

	private static readonly string MONTHSTRING = "正二三四五六七八九十冬腊";

	private static readonly string DAYSTRING = "初一初二初三初四初五初六初七初八初九初十十一十二十三十四十五十六十七十八十九二十廿一廿二廿三廿四廿五廿六廿七廿八廿九三十";

	private static readonly string CELESTIAL_STEMS = "甲乙丙丁戊己庚辛壬癸";

	private static readonly string TERRESTRIAL_BRANCHS = "子丑寅卯辰巳午未申酉戌亥";

	private static readonly string ANIMAL_SIGNS = "鼠牛虎免龙蛇马羊猴鸡狗猪";

	private static readonly string DIGITALS = "〇一二三四五六七八九";

	public int Year { get; internal set; }

	public int Month { get; internal set; }

	public int MonthIndex { get; internal set; }

	public int Day { get; internal set; }

	public bool IsLeapMonth { get; internal set; }

	public int LeapMonthOfYear { get; internal set; }

	public static ChineseDate Today => From(DateTime.Today);

	public static ChineseDate MinValue => From(chineseCalendar.MinSupportedDateTime);

	public static ChineseDate MaxValue => From(chineseCalendar.MaxSupportedDateTime);

	public string CelestialStem => CELESTIAL_STEMS[(Year - 4) % 10].ToString();

	public string TerrestrialBranch => TERRESTRIAL_BRANCHS[(Year - 4) % 12].ToString();

	public string ChineseEra => CelestialStem + TerrestrialBranch;

	public string AnimalSign => ANIMAL_SIGNS[(Year - 4) % 12].ToString();

	public DayOfWeek DayOfWeek => ToDate().DayOfWeek;

	public int DayOfYear => chineseCalendar.GetDayOfYear(ToDate());

	public int DayInMonth => chineseCalendar.GetDaysInMonth(Year, MonthIndex);

	public int DayInYear => chineseCalendar.GetDaysInYear(Year);

	public int MonthsInYear => chineseCalendar.GetMonthsInYear(Year);

	public string CalendarName => "农历";

	public string YearString
	{
		get
		{
			string text = string.Empty;
			int num = 1000;
			int num2 = Year;
			while (num > 0)
			{
				int index = num2 / num;
				text += DIGITALS[index];
				num2 %= num;
				num /= 10;
			}
			return text;
		}
	}

	public string MonthString
	{
		get
		{
			string text = MONTHSTRING[Month - 1].ToString();
			if (IsLeapMonth)
			{
				text = "闰" + text;
			}
			return text;
		}
	}

	public string DayString => DAYSTRING.Substring((Day - 1) * 2, 2);

	private ChineseDate()
	{
	}

	public static ChineseDate From(DateTime date)
	{
		try
		{
			ChineseDate chineseDate = new ChineseDate();
			chineseDate.Year = chineseCalendar.GetYear(date);
			int monthIndex = (chineseDate.Month = chineseCalendar.GetMonth(date));
			chineseDate.MonthIndex = monthIndex;
			chineseDate.Day = chineseCalendar.GetDayOfMonth(date);
			int leapMonth = chineseCalendar.GetLeapMonth(chineseDate.Year);
			chineseDate.IsLeapMonth = leapMonth == chineseDate.Month;
			if (chineseDate.Month >= leapMonth && leapMonth > 0)
			{
				chineseDate.Month--;
			}
			chineseDate.LeapMonthOfYear = Math.Max(0, leapMonth - 1);
			return chineseDate;
		}
		catch (ArgumentOutOfRangeException innerException)
		{
			throw new ArgumentOutOfRangeException("日期超出范围 1901-02-19(公历) -- 2101-01-28(公历)", innerException);
		}
	}

	public static ChineseDate FromIndex(int year, int month, int day)
	{
		if (year < 1901 || year > 2100)
		{
			throw new ArgumentOutOfRangeException("年份超出范围 1901 -- 2100");
		}
		int monthsInYear = chineseCalendar.GetMonthsInYear(year);
		if (month == 0 || month > monthsInYear)
		{
			throw new ArgumentOutOfRangeException($"{year}年，月份允许范围为 1 -- {monthsInYear}");
		}
		if (month < -monthsInYear)
		{
			throw new ArgumentOutOfRangeException($"{year}年，月份允许范围为 -1 -- {-monthsInYear}");
		}
		if (month < 0)
		{
			month = monthsInYear + month + 1;
		}
		int daysInMonth = chineseCalendar.GetDaysInMonth(year, month);
		if (day == 0 || day > daysInMonth)
		{
			throw new ArgumentOutOfRangeException($"日期允许范围为 1 -- {daysInMonth}");
		}
		if (day < -daysInMonth)
		{
			throw new ArgumentOutOfRangeException($"日期允许范围为 -1 -- {-daysInMonth}");
		}
		if (day < 0)
		{
			day = daysInMonth + day + 1;
		}
		ChineseDate chineseDate = new ChineseDate();
		chineseDate.Year = year;
		int monthIndex = (chineseDate.Month = month);
		chineseDate.MonthIndex = monthIndex;
		chineseDate.Day = day;
		int leapMonth = chineseCalendar.GetLeapMonth(year);
		chineseDate.IsLeapMonth = leapMonth == chineseDate.MonthIndex;
		if (chineseDate.Month >= leapMonth && leapMonth > 0)
		{
			chineseDate.Month--;
		}
		chineseDate.LeapMonthOfYear = Math.Max(0, leapMonth - 1);
		return chineseDate;
	}

	public static ChineseDate From(int year, int month, int day)
	{
		if (year < 1901 || year > 2100)
		{
			throw new ArgumentOutOfRangeException("年份超出范围 1901 -- 2100");
		}
		if (month == 0 || month > 12)
		{
			throw new ArgumentOutOfRangeException($"{year}年，月份允许范围为 1 -- 12");
		}
		if (month < -12)
		{
			throw new ArgumentOutOfRangeException($"{year}年，月份允许范围为 -1 -- -12");
		}
		int leapMonth = chineseCalendar.GetLeapMonth(year);
		if (month < 0)
		{
			month = 12 + month + 1;
		}
		int num = month;
		if (month > 0)
		{
			if (month >= leapMonth && leapMonth > 0)
			{
				num++;
			}
		}
		else if (month < 0 && month >= leapMonth - 1 && leapMonth > 0)
		{
			num++;
		}
		int daysInMonth = chineseCalendar.GetDaysInMonth(year, num);
		if (day == 0 || day > daysInMonth)
		{
			throw new ArgumentOutOfRangeException($"日期允许范围为 1 -- {daysInMonth}");
		}
		if (day < -daysInMonth)
		{
			throw new ArgumentOutOfRangeException($"日期允许范围为 -1 -- {-daysInMonth}");
		}
		if (day < 0)
		{
			day = daysInMonth + day + 1;
		}
		return new ChineseDate
		{
			Year = year,
			MonthIndex = num,
			Month = month,
			Day = day,
			IsLeapMonth = (num == leapMonth),
			LeapMonthOfYear = Math.Max(0, leapMonth - 1)
		};
	}

	public DateTime ToDate()
	{
		return chineseCalendar.ToDateTime(Year, MonthIndex, Day, 0, 0, 0, 0);
	}

	public override bool Equals(object obj)
	{
		if (obj is ChineseDate chineseDate)
		{
			if (chineseDate.Year == Year && chineseDate.Month == Month)
			{
				return chineseDate.Day == Day;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return new { Year, MonthIndex, Day }.GetHashCode();
	}

	public override string ToString()
	{
		return ChineseEra + "[" + AnimalSign + "]年" + MonthString + "月" + DayString;
	}

	public ChineseDate AddYears(int value)
	{
		if (value == 0)
		{
			return this;
		}
		int num = Year + value;
		if (num < 1901 || num > 2100)
		{
			throw new ArgumentOutOfRangeException("年份超出范围 1901 -- 2100");
		}
		int leapMonth = chineseCalendar.GetLeapMonth(num);
		int num2 = Month;
		if (num2 >= leapMonth && leapMonth > 0)
		{
			num2++;
		}
		int daysInMonth = chineseCalendar.GetDaysInMonth(num, num2);
		int day = Math.Min(Day, daysInMonth);
		return FromIndex(num, num2, day);
	}

	public ChineseDate AddMonths(int value)
	{
		if (value == 0)
		{
			return this;
		}
		int num = Year;
		int i = MonthIndex + value;
		int monthsInYear = chineseCalendar.GetMonthsInYear(num);
		if (i > monthsInYear)
		{
			while (i > monthsInYear)
			{
				i -= monthsInYear;
				num++;
				if (num < 1901 || num > 2100)
				{
					throw new ArgumentOutOfRangeException("年份超出范围 1901 -- 2100");
				}
				monthsInYear = chineseCalendar.GetMonthsInYear(num);
			}
		}
		else if (i < 1)
		{
			for (; i < 1; i += monthsInYear)
			{
				num--;
				if (num < 1901 || num > 2100)
				{
					throw new ArgumentOutOfRangeException("年份超出范围 1901 -- 2100");
				}
				monthsInYear = chineseCalendar.GetMonthsInYear(num);
			}
		}
		int daysInMonth = chineseCalendar.GetDaysInMonth(num, i);
		int day = Math.Min(Day, daysInMonth);
		return FromIndex(num, i, day);
	}

	public ChineseDate AddDays(int value)
	{
		if (value == 0)
		{
			return this;
		}
		return From(ToDate().AddDays(value));
	}
}
