using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public abstract class Trigger : IDisposable, ICloneable, IEquatable<Trigger>, IComparable, IComparable<Trigger>, INotifyPropertyChanged
{
	internal class CronExpression
	{
		public enum CronFieldType
		{
			Minutes,
			Hours,
			Days,
			Months,
			DaysOfWeek
		}

		public struct FieldVal
		{
			private enum FieldFlags
			{
				List,
				Every,
				Range,
				Increment
			}

			private struct MinMax
			{
				public int Min;

				public int Max;

				public MinMax(int min, int max)
				{
					Min = min;
					Max = max;
				}
			}

			private const string rangeRegEx = "^(?:(?<A>\\*)|(?<D1>\\d+)(?:-(?<D2>\\d+))?)(?:\\/(?<I>\\d+))?$";

			private static readonly Dictionary<string, string> dow = new Dictionary<string, string>(7)
			{
				{ "SUN", "0" },
				{ "MON", "1" },
				{ "TUE", "2" },
				{ "WED", "3" },
				{ "THU", "4" },
				{ "FRI", "5" },
				{ "SAT", "6" }
			};

			private static readonly Dictionary<string, string> mon = new Dictionary<string, string>(12)
			{
				{ "JAN", "1" },
				{ "FEB", "2" },
				{ "MAR", "3" },
				{ "APR", "4" },
				{ "MAY", "5" },
				{ "JUN", "6" },
				{ "JUL", "7" },
				{ "AUG", "8" },
				{ "SEP", "9" },
				{ "OCT", "10" },
				{ "NOV", "11" },
				{ "DEC", "12" }
			};

			private static readonly Dictionary<CronFieldType, MinMax> validRange = new Dictionary<CronFieldType, MinMax>(5)
			{
				{
					CronFieldType.Days,
					new MinMax(1, 31)
				},
				{
					CronFieldType.DaysOfWeek,
					new MinMax(0, 6)
				},
				{
					CronFieldType.Hours,
					new MinMax(0, 23)
				},
				{
					CronFieldType.Minutes,
					new MinMax(0, 59)
				},
				{
					CronFieldType.Months,
					new MinMax(1, 12)
				}
			};

			private CronFieldType cft;

			private FieldFlags flags;

			private int incr;

			private int[] vals;

			public int Duration
			{
				get
				{
					if (vals.Length != 1)
					{
						return vals[1] - vals[0] + 1;
					}
					return 1;
				}
			}

			public int Increment => incr;

			public bool IsEvery
			{
				get
				{
					return flags == FieldFlags.Every;
				}
				private set
				{
					flags = FieldFlags.Every;
				}
			}

			public bool IsIncr
			{
				get
				{
					return flags == FieldFlags.Increment;
				}
				private set
				{
					flags = FieldFlags.Increment;
				}
			}

			public bool IsList
			{
				get
				{
					return flags == FieldFlags.List;
				}
				private set
				{
					flags = FieldFlags.List;
				}
			}

			public bool IsRange
			{
				get
				{
					return flags == FieldFlags.Range;
				}
				private set
				{
					flags = FieldFlags.Range;
				}
			}

			public bool FullRange { get; private set; }

			public int FirstValue => vals[0];

			public IEnumerable<int> Values
			{
				get
				{
					if (flags == FieldFlags.List)
					{
						int[] array = vals;
						for (int i = 0; i < array.Length; i++)
						{
							yield return array[i];
						}
					}
					else
					{
						for (int i = vals[0]; i <= vals[1]; i += incr)
						{
							yield return i;
						}
					}
				}
			}

			public FieldVal(CronFieldType cft)
			{
				this.cft = cft;
				flags = FieldFlags.List;
				vals = new int[0];
				incr = 1;
				FullRange = false;
			}

			public DaysOfTheWeek ToDOW()
			{
				if (IsEvery)
				{
					return DaysOfTheWeek.AllDays;
				}
				DaysOfTheWeek daysOfTheWeek = (DaysOfTheWeek)0;
				foreach (int value in Values)
				{
					daysOfTheWeek |= (DaysOfTheWeek)(1 << value);
				}
				return daysOfTheWeek;
			}

			public MonthsOfTheYear ToMOY()
			{
				if (IsEvery)
				{
					return MonthsOfTheYear.AllMonths;
				}
				MonthsOfTheYear monthsOfTheYear = (MonthsOfTheYear)0;
				foreach (int value in Values)
				{
					monthsOfTheYear |= (MonthsOfTheYear)(1 << value - 1);
				}
				return monthsOfTheYear;
			}

			public static FieldVal Parse(string str, CronFieldType cft)
			{
				FieldVal result = new FieldVal(cft);
				if (string.IsNullOrEmpty(str))
				{
					throw new ArgumentNullException("str", "A crontab field value cannot be empty.");
				}
				str = DoSubs(str, cft);
				if (Regex.IsMatch(str, "^\\d+(,\\d+)*$"))
				{
					if (str.Contains("/"))
					{
						throw new NotSupportedException();
					}
					result.vals = (from i in str.Split(new char[1] { ',' }).Select(ParseInt)
						orderby i
						select i).Distinct().ToArray();
					result.Validate();
					return result;
				}
				Match match = Regex.Match(str, "^(?:(?<A>\\*)|(?<D1>\\d+)(?:-(?<D2>\\d+))?)(?:\\/(?<I>\\d+))?$");
				if (match.Success)
				{
					bool flag = (result.FullRange = match.Groups["A"].Success);
					bool flag2 = flag;
					if (match.Groups["I"].Success)
					{
						result.incr = ParseInt(match.Groups["I"].Value);
						result.IsIncr = true;
					}
					else if (flag2)
					{
						result.IsEvery = true;
					}
					else
					{
						result.IsRange = true;
					}
					MinMax minMax = validRange[cft];
					int num = (flag2 ? minMax.Min : ParseInt(match.Groups["D1"].Value));
					int num2 = (flag2 ? minMax.Max : (match.Groups["D2"].Success ? ParseInt(match.Groups["D2"].Value) : (result.IsIncr ? minMax.Max : num)));
					if (num2 < num)
					{
						throw new ArgumentOutOfRangeException();
					}
					if (num == num2 && result.IsRange)
					{
						result.IsList = true;
						result.vals = new int[1] { num };
					}
					else
					{
						result.vals = new int[2] { num, num2 };
					}
					result.Validate();
					return result;
				}
				throw new FormatException();
			}

			public override string ToString()
			{
				return string.Format("Type:{0}; Vals:{1}; Incr:{2}", flags, string.Join(",", vals.Select((int i) => i.ToString()).ToArray()), incr);
			}

			private void Validate()
			{
				MinMax j = validRange[cft];
				if (vals.Any((int i) => i < j.Min || i > j.Max))
				{
					throw new ArgumentOutOfRangeException();
				}
				if (IsIncr && (incr < j.Min || incr > j.Max))
				{
					throw new ArgumentOutOfRangeException();
				}
			}

			private static string DoSubs(string str, CronFieldType cft)
			{
				StringBuilder stringBuilder = new StringBuilder(str);
				if (cft == CronFieldType.DaysOfWeek)
				{
					foreach (KeyValuePair<string, string> item in dow)
					{
						stringBuilder.Replace(item.Key, item.Value);
					}
				}
				if (cft == CronFieldType.Months)
				{
					foreach (KeyValuePair<string, string> item2 in mon)
					{
						stringBuilder.Replace(item2.Key, item2.Value);
					}
				}
				if (stringBuilder.Length == 1 && stringBuilder.ToString() == "?")
				{
					DateTime now = DateTime.Now;
					int value = 0;
					switch (cft)
					{
					case CronFieldType.Minutes:
						value = now.Minute;
						break;
					case CronFieldType.Hours:
						value = now.Hour;
						break;
					case CronFieldType.Days:
						value = now.Day;
						break;
					case CronFieldType.Months:
						value = now.Month;
						break;
					case CronFieldType.DaysOfWeek:
						value = (int)now.DayOfWeek;
						break;
					}
					stringBuilder.Remove(0, 1);
					stringBuilder.Append(value);
				}
				MinMax minMax = validRange[cft];
				foreach (Match item3 in Regex.Matches(stringBuilder.ToString(), "(\\d+)-(\\d+)"))
				{
					int num = ParseInt(item3.Groups[1].Value);
					int num2 = ParseInt(item3.Groups[2].Value);
					if (num == minMax.Min && num2 == minMax.Max)
					{
						stringBuilder.Replace(item3.Value, "*");
					}
					else if (Enumerable.Contains(stringBuilder.ToString(), ','))
					{
						StringBuilder stringBuilder2 = new StringBuilder(num.ToString());
						for (int i = num; i < num2; i++)
						{
							stringBuilder2.Append($",{i + 1}");
						}
						stringBuilder.Replace(item3.Value, stringBuilder2.ToString());
					}
				}
				return stringBuilder.ToString();
			}

			private static int ParseInt(string str)
			{
				return int.Parse(str.Trim());
			}
		}

		private FieldVal[] Fields = new FieldVal[5];

		public FieldVal Days => Fields[2];

		public FieldVal DOW => Fields[4];

		public FieldVal Hours => Fields[1];

		public FieldVal Minutes => Fields[0];

		public FieldVal Months => Fields[3];

		private CronExpression()
		{
		}

		public static CronExpression Parse(string cronString)
		{
			CronExpression cronExpression = new CronExpression();
			if (cronString == null)
			{
				throw new ArgumentNullException("cronString");
			}
			string[] array = cronString.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 5)
			{
				throw new ArgumentException("'" + cronString + "' is not a valid crontab expression. It must contain at least 5 components of a schedule (in the sequence of minutes, hours, days, months, days of week).");
			}
			for (int i = 0; i < cronExpression.Fields.Length; i++)
			{
				cronExpression.Fields[i] = FieldVal.Parse(array[i], (CronFieldType)i);
			}
			return cronExpression;
		}
	}

	internal const string V2BoundaryDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFK";

	internal static readonly CultureInfo DefaultDateCulture = CultureInfo.CreateSpecificCulture("en-US");

	internal ITaskTrigger v1Trigger;

	internal TaskTrigger v1TriggerData;

	internal ITrigger v2Trigger;

	protected Dictionary<string, object> unboundValues = new Dictionary<string, object>();

	private static bool? foundTimeSpan2;

	private static Type timeSpan2Type;

	private readonly TaskTriggerType ttype;

	private RepetitionPattern repititionPattern;

	public bool Enabled
	{
		get
		{
			return v2Trigger?.Enabled ?? GetUnboundValueOrDefault("Enabled", !v1TriggerData.Flags.IsFlagSet(TaskTriggerFlags.Disabled));
		}
		set
		{
			if (v2Trigger != null)
			{
				v2Trigger.Enabled = value;
			}
			else
			{
				v1TriggerData.Flags = v1TriggerData.Flags.SetFlags(TaskTriggerFlags.Disabled, !value);
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["Enabled"] = value;
				}
			}
			OnNotifyPropertyChanged("Enabled");
		}
	}

	[DefaultValue(typeof(DateTime), "9999-12-31T23:59:59.9999999")]
	public DateTime EndBoundary
	{
		get
		{
			if (v2Trigger != null)
			{
				if (!string.IsNullOrEmpty(v2Trigger.EndBoundary))
				{
					return DateTime.Parse(v2Trigger.EndBoundary, DefaultDateCulture);
				}
				return DateTime.MaxValue;
			}
			return GetUnboundValueOrDefault("EndBoundary", v1TriggerData.EndDate.GetValueOrDefault(DateTime.MaxValue));
		}
		set
		{
			if (v2Trigger != null)
			{
				if (value <= StartBoundary)
				{
					throw new ArgumentException(Resources.Error_TriggerEndBeforeStart);
				}
				v2Trigger.EndBoundary = ((value == DateTime.MaxValue) ? null : value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFK", DefaultDateCulture));
			}
			else
			{
				v1TriggerData.EndDate = ((value == DateTime.MaxValue) ? null : new DateTime?(value));
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["EndBoundary"] = value;
				}
			}
			OnNotifyPropertyChanged("EndBoundary");
		}
	}

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	[XmlIgnore]
	public TimeSpan ExecutionTimeLimit
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault("ExecutionTimeLimit", TimeSpan.Zero);
			}
			return Task.StringToTimeSpan(v2Trigger.ExecutionTimeLimit);
		}
		set
		{
			if (v2Trigger != null)
			{
				v2Trigger.ExecutionTimeLimit = Task.TimeSpanToString(value);
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["ExecutionTimeLimit"] = value;
			}
			OnNotifyPropertyChanged("ExecutionTimeLimit");
		}
	}

	[DefaultValue(null)]
	[XmlIgnore]
	public string Id
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault<string>("Id");
			}
			return v2Trigger.Id;
		}
		set
		{
			if (v2Trigger != null)
			{
				v2Trigger.Id = value;
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["Id"] = value;
			}
			OnNotifyPropertyChanged("Id");
		}
	}

	public RepetitionPattern Repetition
	{
		get
		{
			return repititionPattern ?? (repititionPattern = new RepetitionPattern(this));
		}
		set
		{
			Repetition.Set(value);
			OnNotifyPropertyChanged("Repetition");
		}
	}

	public DateTime StartBoundary
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault("StartBoundary", v1TriggerData.BeginDate);
			}
			if (string.IsNullOrEmpty(v2Trigger.StartBoundary))
			{
				return DateTime.MinValue;
			}
			DateTime result = DateTime.Parse(v2Trigger.StartBoundary, DefaultDateCulture);
			if (v2Trigger.StartBoundary.EndsWith("Z"))
			{
				result = result.ToUniversalTime();
			}
			return result;
		}
		set
		{
			if (v2Trigger != null)
			{
				if (value > EndBoundary)
				{
					throw new ArgumentException(Resources.Error_TriggerEndBeforeStart);
				}
				v2Trigger.StartBoundary = ((value == DateTime.MinValue) ? null : value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFK", DefaultDateCulture));
			}
			else
			{
				v1TriggerData.BeginDate = value;
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["StartBoundary"] = value;
				}
			}
			OnNotifyPropertyChanged("StartBoundary");
		}
	}

	[XmlIgnore]
	public TaskTriggerType TriggerType => ttype;

	public event PropertyChangedEventHandler PropertyChanged;

	public static Trigger[] FromCronFormat([NotNull] string cronString)
	{
		CronExpression cronExpression = CronExpression.Parse(cronString);
		List<Trigger> list = new List<Trigger>();
		if (cronExpression.Days.FullRange && cronExpression.Months.FullRange && !cronExpression.DOW.IsEvery)
		{
			WeeklyTrigger baseTrigger = new WeeklyTrigger(cronExpression.DOW.ToDOW(), 1);
			list.AddRange(ProcessCronTimes(cronExpression, baseTrigger));
		}
		if (!cronExpression.DOW.FullRange && (!cronExpression.Days.FullRange || !cronExpression.Months.FullRange))
		{
			MonthlyDOWTrigger baseTrigger2 = new MonthlyDOWTrigger(cronExpression.DOW.ToDOW(), cronExpression.Months.ToMOY(), WhichWeek.AllWeeks);
			list.AddRange(ProcessCronTimes(cronExpression, baseTrigger2));
		}
		if (!cronExpression.Days.FullRange || (!cronExpression.Months.FullRange && cronExpression.DOW.FullRange))
		{
			MonthlyTrigger baseTrigger3 = new MonthlyTrigger(1, cronExpression.Months.ToMOY())
			{
				DaysOfMonth = cronExpression.Days.Values.ToArray()
			};
			list.AddRange(ProcessCronTimes(cronExpression, baseTrigger3));
		}
		if (cronExpression.Days.FullRange && cronExpression.Months.FullRange && cronExpression.DOW.IsEvery)
		{
			DailyTrigger baseTrigger4 = new DailyTrigger((short)cronExpression.Days.Increment);
			list.AddRange(ProcessCronTimes(cronExpression, baseTrigger4));
		}
		if (list.Count == 0)
		{
			throw new NotSupportedException();
		}
		return list.ToArray();
	}

	private static IEnumerable<Trigger> ProcessCronTimes(CronExpression cron, Trigger baseTrigger)
	{
		if (cron.Minutes.FullRange && (cron.Hours.IsEvery || cron.Hours.IsRange))
		{
			yield return MakeTrigger(new TimeSpan(cron.Hours.FirstValue, 0, 0), TimeSpan.FromMinutes(cron.Minutes.Increment), TimeSpan.FromHours(cron.Hours.Duration));
			yield break;
		}
		if (cron.Minutes.FullRange && (cron.Hours.IsList || cron.Hours.IsIncr))
		{
			foreach (int value in cron.Hours.Values)
			{
				yield return MakeTrigger(new TimeSpan(value, 0, 0), TimeSpan.FromMinutes(cron.Minutes.Increment), TimeSpan.FromHours(1.0));
			}
			yield break;
		}
		if (!cron.Minutes.FullRange && (cron.Hours.IsEvery || cron.Hours.IsRange))
		{
			foreach (int value2 in cron.Minutes.Values)
			{
				yield return MakeTrigger(new TimeSpan(cron.Hours.FirstValue, value2, 0), TimeSpan.FromHours(1.0), TimeSpan.FromHours(cron.Hours.Duration));
			}
			yield break;
		}
		if ((cron.Minutes.IsRange || cron.Minutes.IsIncr) && (cron.Hours.IsList || cron.Hours.IsIncr))
		{
			foreach (int value3 in cron.Hours.Values)
			{
				yield return MakeTrigger(new TimeSpan(value3, cron.Minutes.FirstValue, 0), TimeSpan.FromMinutes(cron.Minutes.Increment), TimeSpan.FromMinutes(cron.Minutes.Duration));
			}
			yield break;
		}
		foreach (int h in cron.Hours.Values)
		{
			foreach (int value4 in cron.Minutes.Values)
			{
				yield return MakeTrigger(new TimeSpan(h, value4, 0));
			}
		}
		Trigger MakeTrigger(TimeSpan start, TimeSpan interval = default(TimeSpan), TimeSpan duration = default(TimeSpan))
		{
			Trigger trigger = (Trigger)baseTrigger.Clone();
			trigger.StartBoundary = trigger.StartBoundary.Date + start;
			if (interval != default(TimeSpan))
			{
				trigger.Repetition.Interval = interval;
				trigger.Repetition.Duration = duration;
			}
			return trigger;
		}
	}

	internal Trigger([NotNull] ITaskTrigger trigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType type)
	{
		v1Trigger = trigger;
		v1TriggerData = trigger.GetTrigger();
		v1TriggerData.Type = type;
		ttype = ConvertFromV1TriggerType(type);
	}

	internal Trigger([NotNull] ITrigger iTrigger)
	{
		v2Trigger = iTrigger;
		ttype = iTrigger.Type;
		if (string.IsNullOrEmpty(v2Trigger.StartBoundary) && this is ICalendarTrigger)
		{
			StartBoundary = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
		}
	}

	internal Trigger(TaskTriggerType triggerType)
	{
		ttype = triggerType;
		v1TriggerData.TriggerSize = (ushort)Marshal.SizeOf(typeof(TaskTrigger));
		if (ttype != TaskTriggerType.Registration && ttype != 0 && ttype != TaskTriggerType.SessionStateChange)
		{
			v1TriggerData.Type = ConvertToV1TriggerType(ttype);
		}
		if (this is ICalendarTrigger)
		{
			StartBoundary = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
		}
	}

	public static Trigger CreateTrigger(TaskTriggerType triggerType)
	{
		return triggerType switch
		{
			TaskTriggerType.Boot => new BootTrigger(), 
			TaskTriggerType.Daily => new DailyTrigger(1), 
			TaskTriggerType.Event => new EventTrigger(), 
			TaskTriggerType.Idle => new IdleTrigger(), 
			TaskTriggerType.Logon => new LogonTrigger(), 
			TaskTriggerType.Monthly => new MonthlyTrigger(), 
			TaskTriggerType.MonthlyDOW => new MonthlyDOWTrigger(), 
			TaskTriggerType.Registration => new RegistrationTrigger(), 
			TaskTriggerType.SessionStateChange => new SessionStateChangeTrigger(), 
			TaskTriggerType.Time => new TimeTrigger(), 
			TaskTriggerType.Weekly => new WeeklyTrigger(DaysOfTheWeek.Sunday, 1), 
			TaskTriggerType.Custom => null, 
			_ => throw new ArgumentOutOfRangeException("triggerType", triggerType, null), 
		};
	}

	public virtual object Clone()
	{
		Trigger trigger = CreateTrigger(TriggerType);
		trigger.CopyProperties(this);
		return trigger;
	}

	public int CompareTo(Trigger other)
	{
		return string.Compare(Id, other?.Id, StringComparison.InvariantCulture);
	}

	public virtual void CopyProperties(Trigger sourceTrigger)
	{
		if (sourceTrigger == null)
		{
			return;
		}
		Enabled = sourceTrigger.Enabled;
		EndBoundary = sourceTrigger.EndBoundary;
		try
		{
			ExecutionTimeLimit = sourceTrigger.ExecutionTimeLimit;
		}
		catch
		{
		}
		Id = sourceTrigger.Id;
		Repetition.Duration = sourceTrigger.Repetition.Duration;
		Repetition.Interval = sourceTrigger.Repetition.Interval;
		Repetition.StopAtDurationEnd = sourceTrigger.Repetition.StopAtDurationEnd;
		StartBoundary = sourceTrigger.StartBoundary;
		if (sourceTrigger is ITriggerDelay triggerDelay && this is ITriggerDelay)
		{
			try
			{
				((ITriggerDelay)this).Delay = triggerDelay.Delay;
			}
			catch
			{
			}
		}
		if (!(sourceTrigger is ITriggerUserId triggerUserId) || !(this is ITriggerUserId))
		{
			return;
		}
		try
		{
			((ITriggerUserId)this).UserId = triggerUserId.UserId;
		}
		catch
		{
		}
	}

	public virtual void Dispose()
	{
		if (v2Trigger != null)
		{
			Marshal.ReleaseComObject(v2Trigger);
		}
		if (v1Trigger != null)
		{
			Marshal.ReleaseComObject(v1Trigger);
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Trigger other))
		{
			return base.Equals(obj);
		}
		return Equals(other);
	}

	public virtual bool Equals(Trigger other)
	{
		if (other == null)
		{
			return false;
		}
		bool flag = TriggerType == other.TriggerType && Enabled == other.Enabled && EndBoundary == other.EndBoundary && ExecutionTimeLimit == other.ExecutionTimeLimit && Id == other.Id && Repetition.Equals(other.Repetition) && StartBoundary == other.StartBoundary;
		if (other is ITriggerDelay triggerDelay && this is ITriggerDelay)
		{
			try
			{
				flag = flag && ((ITriggerDelay)this).Delay == triggerDelay.Delay;
			}
			catch
			{
			}
		}
		if (other is ITriggerUserId triggerUserId && this is ITriggerUserId)
		{
			try
			{
				flag = flag && ((ITriggerUserId)this).UserId == triggerUserId.UserId;
			}
			catch
			{
			}
		}
		return flag;
	}

	public override int GetHashCode()
	{
		return new
		{
			A = TriggerType,
			B = Enabled,
			C = EndBoundary,
			D = ExecutionTimeLimit,
			E = Id,
			F = Repetition,
			G = StartBoundary,
			H = ((this as ITriggerDelay)?.Delay ?? TimeSpan.Zero),
			I = (this as ITriggerUserId)?.UserId
		}.GetHashCode();
	}

	[Obsolete("Set the Repetition property directly with a new instance of RepetitionPattern.", false)]
	public void SetRepetition(TimeSpan interval, TimeSpan duration, bool stopAtDurationEnd = true)
	{
		Repetition.Duration = duration;
		Repetition.Interval = interval;
		Repetition.StopAtDurationEnd = stopAtDurationEnd;
	}

	public override string ToString()
	{
		if (v1Trigger == null)
		{
			return V2GetTriggerString() + V2BaseTriggerString();
		}
		return v1Trigger.GetTriggerString();
	}

	public virtual string ToString([NotNull] CultureInfo culture)
	{
		using (new CultureSwitcher(culture))
		{
			return ToString();
		}
	}

	int IComparable.CompareTo(object obj)
	{
		return CompareTo(obj as Trigger);
	}

	internal static DateTime AdjustToLocal(DateTime dt)
	{
		if (dt.Kind != DateTimeKind.Utc)
		{
			return dt;
		}
		return dt.ToLocalTime();
	}

	internal static Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType ConvertToV1TriggerType(TaskTriggerType type)
	{
		if (type == TaskTriggerType.Registration || type == TaskTriggerType.Event || type == TaskTriggerType.SessionStateChange)
		{
			throw new NotV1SupportedException();
		}
		int num = (int)(type - 1);
		if (num >= 7)
		{
			num--;
		}
		return (Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType)num;
	}

	internal static Trigger CreateTrigger([NotNull] ITaskTrigger trigger)
	{
		return CreateTrigger(trigger, trigger.GetTrigger().Type);
	}

	internal static Trigger CreateTrigger([NotNull] ITaskTrigger trigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType triggerType)
	{
		return triggerType switch
		{
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunOnce => new TimeTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunDaily => new DailyTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunWeekly => new WeeklyTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunMonthly => new MonthlyTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunMonthlyDOW => new MonthlyDOWTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.OnIdle => new IdleTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.OnSystemStart => new BootTrigger(trigger), 
			Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.OnLogon => new LogonTrigger(trigger), 
			_ => throw new ArgumentOutOfRangeException("triggerType", triggerType, null), 
		};
	}

	internal static Trigger CreateTrigger([NotNull] ITrigger iTrigger, ITaskDefinition iDef = null)
	{
		switch (iTrigger.Type)
		{
		case TaskTriggerType.Boot:
			return new BootTrigger((IBootTrigger)iTrigger);
		case TaskTriggerType.Daily:
			return new DailyTrigger((IDailyTrigger)iTrigger);
		case TaskTriggerType.Event:
			return new EventTrigger((IEventTrigger)iTrigger);
		case TaskTriggerType.Idle:
			return new IdleTrigger((IIdleTrigger)iTrigger);
		case TaskTriggerType.Logon:
			return new LogonTrigger((ILogonTrigger)iTrigger);
		case TaskTriggerType.Monthly:
			return new MonthlyTrigger((IMonthlyTrigger)iTrigger);
		case TaskTriggerType.MonthlyDOW:
			return new MonthlyDOWTrigger((IMonthlyDOWTrigger)iTrigger);
		case TaskTriggerType.Registration:
			return new RegistrationTrigger((IRegistrationTrigger)iTrigger);
		case TaskTriggerType.SessionStateChange:
			return new SessionStateChangeTrigger((ISessionStateChangeTrigger)iTrigger);
		case TaskTriggerType.Time:
			return new TimeTrigger((ITimeTrigger)iTrigger);
		case TaskTriggerType.Weekly:
			return new WeeklyTrigger((IWeeklyTrigger)iTrigger);
		case TaskTriggerType.Custom:
		{
			CustomTrigger customTrigger = new CustomTrigger(iTrigger);
			if (iDef != null)
			{
				try
				{
					customTrigger.UpdateFromXml(iDef.XmlText);
				}
				catch
				{
				}
			}
			return customTrigger;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	internal static string GetBestTimeSpanString(TimeSpan span)
	{
		if (!foundTimeSpan2.HasValue)
		{
			try
			{
				foundTimeSpan2 = false;
				timeSpan2Type = ReflectionHelper.LoadType("System.TimeSpan2", "TimeSpan2.dll");
				if (timeSpan2Type != null)
				{
					foundTimeSpan2 = true;
				}
			}
			catch
			{
			}
		}
		if (foundTimeSpan2 == true && timeSpan2Type != null)
		{
			try
			{
				return ReflectionHelper.InvokeMethod<string>(timeSpan2Type, new object[1] { span }, "ToString", new object[1] { "f" });
			}
			catch
			{
			}
		}
		return span.ToString();
	}

	internal virtual void Bind([NotNull] ITask iTask)
	{
		if (v1Trigger == null)
		{
			v1Trigger = iTask.CreateTrigger(out var _);
		}
		SetV1TriggerData();
	}

	internal virtual void Bind([NotNull] ITaskDefinition iTaskDef)
	{
		ITriggerCollection triggers = iTaskDef.Triggers;
		v2Trigger = triggers.Create(ttype);
		Marshal.ReleaseComObject(triggers);
		if ((unboundValues.TryGetValue("StartBoundary", out var value) ? ((DateTime)value) : StartBoundary) > (unboundValues.TryGetValue("EndBoundary", out value) ? ((DateTime)value) : EndBoundary))
		{
			throw new ArgumentException(Resources.Error_TriggerEndBeforeStart);
		}
		foreach (string key in unboundValues.Keys)
		{
			try
			{
				object o = unboundValues[key];
				CheckBindValue(key, ref o);
				v2Trigger.GetType().InvokeMember(key, BindingFlags.SetProperty, null, v2Trigger, new object[1] { o });
			}
			catch (TargetInvocationException ex) when (ex.InnerException != null)
			{
				throw ex.InnerException;
			}
			catch
			{
			}
		}
		unboundValues.Clear();
		unboundValues = null;
		repititionPattern = new RepetitionPattern(this);
		repititionPattern.Bind();
	}

	internal void SetV1TriggerData()
	{
		if (v1TriggerData.MinutesInterval != 0 && v1TriggerData.MinutesInterval >= v1TriggerData.MinutesDuration)
		{
			throw new ArgumentException("Trigger.Repetition.Interval must be less than Trigger.Repetition.Duration under Task Scheduler 1.0.");
		}
		if (v1TriggerData.EndDate <= v1TriggerData.BeginDate)
		{
			throw new ArgumentException(Resources.Error_TriggerEndBeforeStart);
		}
		if (v1TriggerData.BeginDate == DateTime.MinValue)
		{
			v1TriggerData.BeginDate = DateTime.Now;
		}
		v1Trigger?.SetTrigger(ref v1TriggerData);
	}

	protected virtual void CheckBindValue(string key, ref object o)
	{
		if (o is TimeSpan span)
		{
			o = Task.TimeSpanToString(span);
		}
		if (o is DateTime dateTime)
		{
			if ((key == "EndBoundary" && dateTime == DateTime.MaxValue) || (key == "StartBoundary" && dateTime == DateTime.MinValue))
			{
				o = null;
			}
			else
			{
				o = dateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFK", DefaultDateCulture);
			}
		}
	}

	protected T GetUnboundValueOrDefault<T>(string prop, T def = default(T))
	{
		if (!unboundValues.TryGetValue(prop, out var value))
		{
			return def;
		}
		return (T)value;
	}

	protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected virtual string V2GetTriggerString()
	{
		return string.Empty;
	}

	private static TaskTriggerType ConvertFromV1TriggerType(Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType v1Type)
	{
		int num = (int)(v1Type + 1);
		if (num > 6)
		{
			num++;
		}
		return (TaskTriggerType)num;
	}

	private string V2BaseTriggerString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Repetition.Interval != TimeSpan.Zero)
		{
			string arg = ((Repetition.Duration == TimeSpan.Zero) ? Resources.TriggerDuration0 : string.Format(Resources.TriggerDurationNot0, GetBestTimeSpanString(Repetition.Duration)));
			stringBuilder.AppendFormat(Resources.TriggerRepetition, GetBestTimeSpanString(Repetition.Interval), arg);
		}
		if (EndBoundary != DateTime.MaxValue)
		{
			stringBuilder.AppendFormat(Resources.TriggerEndBoundary, AdjustToLocal(EndBoundary));
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Insert(0, Resources.HyphenSeparator);
		}
		return stringBuilder.ToString();
	}
}
