using System;
using System.Collections.Generic;

namespace ChineseCalendar;

public abstract class Festival
{
	public string Name { get; protected set; }

	public string Description { get; protected set; }

	public int? FirstYear { get; protected set; }

	public int Month { get; protected set; }

	public int Day { get; protected set; }

	public abstract DateTime? GetLastDate(DateTime? date = null, bool containsThisDate = false);

	public abstract DateTime? GetNextDate(DateTime? date = null, bool containsThisDate = false);

	public abstract bool IsThisFestival(DateTime date);

	public override string ToString()
	{
		return Name;
	}

	public static IEnumerable<Festival> GetAllDefined()
	{
		yield return GregorianFestival.NewYearsDay;
		yield return GregorianFestival.TheTombWeepingDay;
		yield return GregorianFestival.InternationalWorkersDay;
		yield return GregorianFestival.TheNationalDay;
		yield return ChineseFestival.SpringFestival;
		yield return ChineseFestival.LanternFestival;
		yield return ChineseFestival.DragonBoatFestival;
		yield return ChineseFestival.QixiFestival;
		yield return ChineseFestival.MidAutumnFestival;
		yield return ChineseFestival.DoubleNinthFestival;
		yield return ChineseFestival.NewYearsEve;
	}
}
