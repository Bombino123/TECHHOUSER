using System;

namespace ChineseCalendar;

public class ChineseFestival : LoopFestival
{
	public static readonly ChineseFestival SpringFestival = new ChineseFestival
	{
		Name = "春节",
		Description = "正月初一",
		Month = 1,
		Day = 1
	};

	public static readonly ChineseFestival LanternFestival = new ChineseFestival
	{
		Name = "元宵节",
		Description = "正月十五",
		Month = 1,
		Day = 15
	};

	public static readonly ChineseFestival DragonHeadraisingDay = new ChineseFestival
	{
		Name = "龙抬头",
		Description = "二月初二",
		Month = 2,
		Day = 2
	};

	public static readonly ChineseFestival DragonBoatFestival = new ChineseFestival
	{
		Name = "端午",
		Description = "五月初五",
		Month = 5,
		Day = 5
	};

	public static readonly ChineseFestival QixiFestival = new ChineseFestival
	{
		Name = "七夕",
		Description = "七月初七",
		Month = 7,
		Day = 7
	};

	public static readonly ChineseFestival GhostFestival = new ChineseFestival
	{
		Name = "中元节",
		Description = "七月十五",
		Month = 7,
		Day = 15
	};

	public static readonly ChineseFestival MidAutumnFestival = new ChineseFestival
	{
		Name = "中秋",
		Description = "八月十五",
		Month = 8,
		Day = 15
	};

	public static readonly ChineseFestival DoubleNinthFestival = new ChineseFestival
	{
		Name = "重阳节",
		Description = "九月初九",
		Month = 9,
		Day = 9
	};

	public static readonly ChineseFestival NewYearsEve = new ChineseFestival
	{
		Name = "除夕",
		Description = "大年三十",
		Month = -1,
		Day = -1
	};

	public ChineseDate First { get; protected set; }

	private ChineseFestival()
	{
	}

	public ChineseFestival(string name, int month, int day, int? firstYear = null, string description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		if (month == 0 || month > 12 || month < -12)
		{
			throw new ArgumentOutOfRangeException("month", "[-12,-1],[1,12]", "月份超出范围");
		}
		if (day == 0 || day > 30 || day < -30)
		{
			throw new ArgumentOutOfRangeException("day", "[-30,-1],[1,30]", "日期超出范围");
		}
		base.Name = name;
		base.Month = month;
		base.Day = day;
		base.FirstYear = firstYear;
		base.Description = description;
		if (base.FirstYear.HasValue)
		{
			First = ChineseDate.From(firstYear.Value, month, day);
		}
	}

	protected override bool TryGetDate(int year, int month, int day, out DateTime date)
	{
		try
		{
			date = ChineseDate.From(year, month, day).ToDate();
			return true;
		}
		catch (Exception)
		{
			date = DateTime.Now;
			return false;
		}
	}

	public override bool IsThisFestival(DateTime date)
	{
		ChineseDate chineseDate = ChineseDate.From(date);
		ChineseDate chineseDate2 = ChineseDate.From(chineseDate.Year, base.Month, base.Day);
		return chineseDate == chineseDate2;
	}
}
