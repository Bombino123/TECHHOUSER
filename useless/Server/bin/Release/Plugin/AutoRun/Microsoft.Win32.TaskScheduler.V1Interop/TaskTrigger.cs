using System;

namespace Microsoft.Win32.TaskScheduler.V1Interop;

internal struct TaskTrigger
{
	public ushort TriggerSize;

	public ushort Reserved1;

	public ushort BeginYear;

	public ushort BeginMonth;

	public ushort BeginDay;

	public ushort EndYear;

	public ushort EndMonth;

	public ushort EndDay;

	public ushort StartHour;

	public ushort StartMinute;

	public uint MinutesDuration;

	public uint MinutesInterval;

	public TaskTriggerFlags Flags;

	public TaskTriggerType Type;

	public TriggerTypeData Data;

	public ushort Reserved2;

	public ushort RandomMinutesInterval;

	public DateTime BeginDate
	{
		get
		{
			try
			{
				return (BeginYear == 0) ? DateTime.MinValue : new DateTime(BeginYear, BeginMonth, BeginDay, StartHour, StartMinute, 0, DateTimeKind.Unspecified);
			}
			catch
			{
				return DateTime.MinValue;
			}
		}
		set
		{
			if (value != DateTime.MinValue)
			{
				DateTime dateTime = ((value.Kind == DateTimeKind.Utc) ? value.ToLocalTime() : value);
				BeginYear = (ushort)dateTime.Year;
				BeginMonth = (ushort)dateTime.Month;
				BeginDay = (ushort)dateTime.Day;
				StartHour = (ushort)dateTime.Hour;
				StartMinute = (ushort)dateTime.Minute;
			}
			else
			{
				BeginYear = (BeginMonth = (BeginDay = (StartHour = (StartMinute = 0))));
			}
		}
	}

	public DateTime? EndDate
	{
		get
		{
			try
			{
				return (EndYear == 0) ? null : new DateTime?(new DateTime(EndYear, EndMonth, EndDay));
			}
			catch
			{
				return DateTime.MaxValue;
			}
		}
		set
		{
			if (value.HasValue)
			{
				EndYear = (ushort)value.Value.Year;
				EndMonth = (ushort)value.Value.Month;
				EndDay = (ushort)value.Value.Day;
				Flags |= TaskTriggerFlags.HasEndDate;
			}
			else
			{
				EndYear = (EndMonth = (EndDay = 0));
				Flags &= ~TaskTriggerFlags.HasEndDate;
			}
		}
	}

	public override string ToString()
	{
		return string.Format("Trigger Type: {0};\n> Start: {1}; End: {2};\n> DurMin: {3}; DurItv: {4};\n>", Type, BeginDate, (EndYear == 0) ? "null" : EndDate?.ToString(), MinutesDuration, MinutesInterval);
	}
}
