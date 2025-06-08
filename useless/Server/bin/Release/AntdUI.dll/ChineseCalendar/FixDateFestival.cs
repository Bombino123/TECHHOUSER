using System;

namespace ChineseCalendar;

public class FixDateFestival : Festival
{
	public DateTime? First { get; protected set; }

	public int Year { get; private set; }

	public DateTime Date { get; private set; }

	public FixDateFestival(string name, int year, int month, int day, string description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		try
		{
			base.Name = name;
			Year = year;
			base.Month = month;
			base.Day = day;
			base.Description = description;
			DateTime value = (Date = new DateTime(year, month, day));
			First = value;
		}
		catch (Exception innerException)
		{
			throw new ArgumentOutOfRangeException("日期参数不正确", innerException);
		}
	}

	public FixDateFestival(string name, DateTime date, string description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		base.Name = name;
		Year = date.Year;
		base.Month = date.Month;
		base.Day = date.Day;
		base.Description = description;
		Date = date;
	}

	public override DateTime? GetLastDate(DateTime? date, bool containsThisDate = false)
	{
		DateTime dateTime = (date.HasValue ? date.Value.Date : DateTime.Today);
		if (containsThisDate && IsThisFestival(dateTime))
		{
			return dateTime;
		}
		if (Date < dateTime)
		{
			return Date;
		}
		return null;
	}

	public override DateTime? GetNextDate(DateTime? date, bool containsThisDate = false)
	{
		DateTime dateTime = (date.HasValue ? date.Value.Date : DateTime.Today);
		if (containsThisDate && IsThisFestival(dateTime))
		{
			return dateTime;
		}
		if (Date > dateTime)
		{
			return Date;
		}
		return null;
	}

	public override bool IsThisFestival(DateTime date)
	{
		return date.Date == Date.Date;
	}
}
