using System;

namespace ChineseCalendar;

public abstract class LoopFestival : Festival
{
	protected virtual bool TryGetDate(int year, int month, int day, out DateTime date)
	{
		try
		{
			date = new DateTime(year, month, day);
			return true;
		}
		catch (Exception)
		{
			date = DateTime.Now;
			return false;
		}
	}

	public override DateTime? GetLastDate(DateTime? date, bool containsThisDate = false)
	{
		DateTime dateTime = (date.HasValue ? date.Value.Date : DateTime.Today);
		if (containsThisDate && IsThisFestival(dateTime))
		{
			return dateTime;
		}
		if (TryGetDate(dateTime.Year, base.Month, base.Day, out var date2) && date2 < dateTime)
		{
			return date2;
		}
		int num = dateTime.Year - 1;
		while (true)
		{
			int num2 = num;
			DateTime minValue = DateTime.MinValue;
			if (num2 < minValue.Year || (base.FirstYear.HasValue && num < base.FirstYear.Value))
			{
				break;
			}
			if (TryGetDate(num, base.Month, base.Day, out var date3))
			{
				return date3;
			}
			num--;
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
		if (TryGetDate(dateTime.Year, base.Month, base.Day, out var date2) && date2 > dateTime)
		{
			return date2;
		}
		int num = dateTime.Year + 1;
		while (true)
		{
			int num2 = num;
			DateTime maxValue = DateTime.MaxValue;
			if (num2 > maxValue.Year)
			{
				break;
			}
			if (TryGetDate(num, base.Month, base.Day, out var date3))
			{
				return date3;
			}
			num++;
		}
		return null;
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
