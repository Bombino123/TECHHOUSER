using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("CalendarTrigger", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[ComVisible(true)]
public sealed class MonthlyTrigger : Trigger, ICalendarTrigger, ITriggerDelay, IXmlSerializable
{
	public int[] DaysOfMonth
	{
		get
		{
			if (v2Trigger == null)
			{
				return MaskToIndices((int)v1TriggerData.Data.monthlyDate.Days);
			}
			return MaskToIndices(((IMonthlyTrigger)v2Trigger).DaysOfMonth);
		}
		set
		{
			int num = IndicesToMask(value);
			if (v2Trigger != null)
			{
				((IMonthlyTrigger)v2Trigger).DaysOfMonth = num;
			}
			else
			{
				v1TriggerData.Data.monthlyDate.Days = (uint)num;
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["DaysOfMonth"] = num;
				}
			}
			OnNotifyPropertyChanged("DaysOfMonth");
		}
	}

	[DefaultValue(0)]
	public MonthsOfTheYear MonthsOfYear
	{
		get
		{
			if (v2Trigger == null)
			{
				return v1TriggerData.Data.monthlyDOW.Months;
			}
			return (MonthsOfTheYear)((IMonthlyTrigger)v2Trigger).MonthsOfYear;
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyTrigger)v2Trigger).MonthsOfYear = (short)value;
			}
			else
			{
				v1TriggerData.Data.monthlyDOW.Months = value;
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["MonthsOfYear"] = (short)value;
				}
			}
			OnNotifyPropertyChanged("MonthsOfYear");
		}
	}

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	[XmlIgnore]
	public TimeSpan RandomDelay
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault("RandomDelay", TimeSpan.Zero);
			}
			return Task.StringToTimeSpan(((IMonthlyTrigger)v2Trigger).RandomDelay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyTrigger)v2Trigger).RandomDelay = Task.TimeSpanToString(value);
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["RandomDelay"] = value;
			}
			OnNotifyPropertyChanged("RandomDelay");
		}
	}

	[DefaultValue(false)]
	[XmlIgnore]
	public bool RunOnLastDayOfMonth
	{
		get
		{
			return ((IMonthlyTrigger)v2Trigger)?.RunOnLastDayOfMonth ?? GetUnboundValueOrDefault("RunOnLastDayOfMonth", def: false);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyTrigger)v2Trigger).RunOnLastDayOfMonth = value;
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["RunOnLastDayOfMonth"] = value;
			}
			OnNotifyPropertyChanged("RunOnLastDayOfMonth");
		}
	}

	TimeSpan ITriggerDelay.Delay
	{
		get
		{
			return RandomDelay;
		}
		set
		{
			RandomDelay = value;
		}
	}

	public MonthlyTrigger(int dayOfMonth = 1, MonthsOfTheYear monthsOfYear = MonthsOfTheYear.AllMonths)
		: base(TaskTriggerType.Monthly)
	{
		if (dayOfMonth < 1 || dayOfMonth > 32)
		{
			throw new ArgumentOutOfRangeException("dayOfMonth");
		}
		if (!monthsOfYear.IsValidFlagValue())
		{
			throw new ArgumentOutOfRangeException("monthsOfYear");
		}
		if (dayOfMonth == 32)
		{
			DaysOfMonth = new int[0];
			RunOnLastDayOfMonth = true;
		}
		else
		{
			DaysOfMonth = new int[1] { dayOfMonth };
		}
		MonthsOfYear = monthsOfYear;
	}

	internal MonthlyTrigger([NotNull] ITaskTrigger iTrigger)
		: base(iTrigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunMonthly)
	{
		if (v1TriggerData.Data.monthlyDate.Months == (MonthsOfTheYear)0)
		{
			v1TriggerData.Data.monthlyDate.Months = MonthsOfTheYear.AllMonths;
		}
		if (v1TriggerData.Data.monthlyDate.Days == 0)
		{
			v1TriggerData.Data.monthlyDate.Days = 1u;
		}
	}

	internal MonthlyTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public override void CopyProperties(Trigger sourceTrigger)
	{
		base.CopyProperties(sourceTrigger);
		if (sourceTrigger is MonthlyTrigger monthlyTrigger)
		{
			DaysOfMonth = monthlyTrigger.DaysOfMonth;
			MonthsOfYear = monthlyTrigger.MonthsOfYear;
			try
			{
				RunOnLastDayOfMonth = monthlyTrigger.RunOnLastDayOfMonth;
			}
			catch
			{
			}
		}
		if (sourceTrigger is MonthlyDOWTrigger monthlyDOWTrigger)
		{
			MonthsOfYear = monthlyDOWTrigger.MonthsOfYear;
		}
	}

	public override bool Equals(Trigger other)
	{
		if (other is MonthlyTrigger monthlyTrigger && base.Equals(monthlyTrigger) && ListsEqual(DaysOfMonth, monthlyTrigger.DaysOfMonth) && MonthsOfYear == monthlyTrigger.MonthsOfYear && v1Trigger == null)
		{
			return RunOnLastDayOfMonth == monthlyTrigger.RunOnLastDayOfMonth;
		}
		return false;
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		CalendarTrigger.ReadXml(reader, this, ReadMyXml);
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		CalendarTrigger.WriteXml(writer, this, WriteMyXml);
	}

	protected override string V2GetTriggerString()
	{
		string text = string.Join(Resources.ListSeparator, Array.ConvertAll(DaysOfMonth, (int i) => i.ToString()));
		if (RunOnLastDayOfMonth)
		{
			text = text + ((text.Length == 0) ? "" : Resources.ListSeparator) + Resources.WWLastWeek;
		}
		string @string = TaskEnumGlobalizer.GetString(MonthsOfYear);
		return string.Format(Resources.TriggerMonthly1, Trigger.AdjustToLocal(base.StartBoundary), text, @string);
	}

	private static int IndicesToMask(int[] indices)
	{
		if (indices == null || indices.Length == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (int num2 in indices)
		{
			if (num2 < 1 || num2 > 31)
			{
				throw new ArgumentException("Days must be in the range 1..31");
			}
			num |= 1 << num2 - 1;
		}
		return num;
	}

	private static bool ListsEqual<T>(ICollection<T> left, ICollection<T> right) where T : IComparable
	{
		if (left == null && right == null)
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		if (left.Count != right.Count)
		{
			return false;
		}
		List<T> list = new List<T>(left);
		List<T> list2 = new List<T>(right);
		list.Sort();
		list2.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].CompareTo(list2[i]) != 0)
			{
				return false;
			}
		}
		return true;
	}

	private static int[] MaskToIndices(int mask)
	{
		int num = 0;
		for (int i = 0; mask >> i > 0; i++)
		{
			num += 1 & (mask >> i);
		}
		int[] array = new int[num];
		num = 0;
		for (int j = 0; mask >> j > 0; j++)
		{
			if ((1 & (mask >> j)) == 1)
			{
				array[num++] = j + 1;
			}
		}
		return array;
	}

	private void ReadMyXml([NotNull] XmlReader reader)
	{
		reader.ReadStartElement("ScheduleByMonth");
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			string localName = reader.LocalName;
			if (!(localName == "DaysOfMonth"))
			{
				if (localName == "Months")
				{
					reader.Read();
					MonthsOfYear = (MonthsOfTheYear)0;
					while (reader.MoveToContent() == XmlNodeType.Element)
					{
						try
						{
							MonthsOfYear |= (MonthsOfTheYear)Enum.Parse(typeof(MonthsOfTheYear), reader.LocalName);
						}
						catch
						{
							throw new XmlException("Invalid months of the year element.");
						}
						reader.Read();
					}
					reader.ReadEndElement();
				}
				else
				{
					reader.Skip();
				}
				continue;
			}
			reader.Read();
			List<int> list = new List<int>();
			while (reader.MoveToContent() == XmlNodeType.Element)
			{
				if (reader.LocalName != "Day")
				{
					continue;
				}
				string text = reader.ReadElementContentAsString();
				if (!text.Equals("Last", StringComparison.InvariantCultureIgnoreCase))
				{
					int num = int.Parse(text);
					if (num >= 1 && num <= 31)
					{
						list.Add(num);
					}
				}
			}
			DaysOfMonth = list.ToArray();
			reader.ReadEndElement();
		}
		reader.ReadEndElement();
	}

	private void WriteMyXml([NotNull] XmlWriter writer)
	{
		writer.WriteStartElement("ScheduleByMonth");
		writer.WriteStartElement("DaysOfMonth");
		int[] daysOfMonth = DaysOfMonth;
		foreach (int num in daysOfMonth)
		{
			writer.WriteElementString("Day", num.ToString());
		}
		writer.WriteEndElement();
		writer.WriteStartElement("Months");
		foreach (MonthsOfTheYear value in Enum.GetValues(typeof(MonthsOfTheYear)))
		{
			if (value != MonthsOfTheYear.AllMonths && (MonthsOfYear & value) == value)
			{
				writer.WriteElementString(value.ToString(), null);
			}
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}
}
