using System;
using System.Collections.Generic;

namespace ChineseCalendar;

public class GregorianFestival : LoopFestival
{
	private static readonly Dictionary<int, int> monthDays = new Dictionary<int, int>
	{
		{ 1, 31 },
		{ 2, 29 },
		{ 3, 31 },
		{ 4, 30 },
		{ 5, 31 },
		{ 6, 30 },
		{ 7, 31 },
		{ 8, 31 },
		{ 9, 30 },
		{ 10, 31 },
		{ 11, 30 },
		{ 12, 31 }
	};

	public static readonly GregorianFestival NewYearsDay = new GregorianFestival
	{
		Name = "元旦",
		Description = "1月1日",
		Month = 1,
		Day = 1
	};

	public static readonly GregorianFestival ArborDay = new GregorianFestival
	{
		Name = "植树节",
		Description = "3月12日",
		Month = 3,
		Day = 12,
		FirstYear = 1979
	};

	public static readonly GregorianFestival TheTombWeepingDay = new GregorianFestival
	{
		Name = "清明",
		Description = "4月5日",
		Month = 4,
		Day = 5
	};

	public static readonly GregorianFestival InternationalWorkersDay = new GregorianFestival
	{
		Name = "劳动节",
		Description = "5月1日",
		Month = 5,
		Day = 1,
		FirstYear = 1890
	};

	public static readonly GregorianFestival TheNationalDay = new GregorianFestival
	{
		Name = "国庆节",
		Description = "10月1日",
		Month = 10,
		Day = 1,
		FirstYear = 1949
	};

	public DateTime? First { get; protected set; }

	private GregorianFestival()
	{
	}

	public GregorianFestival(string name, int month, int day, int firstYear = 0, string description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		if (month < 1 || month > 12)
		{
			throw new ArgumentOutOfRangeException("month", "[1,12]", "月份超出范围");
		}
		int num = monthDays[month];
		if (day < 1 || day > num)
		{
			throw new ArgumentOutOfRangeException("day", $"[1,{num}]", "日期超出范围");
		}
		base.Name = name;
		base.Month = month;
		base.Day = day;
		base.FirstYear = firstYear;
		base.Description = description;
		if (base.FirstYear.HasValue)
		{
			First = new DateTime(firstYear, month, day);
		}
	}

	public override bool IsThisFestival(DateTime date)
	{
		if (date.Month == base.Month)
		{
			return date.Day == base.Day;
		}
		return false;
	}
}
