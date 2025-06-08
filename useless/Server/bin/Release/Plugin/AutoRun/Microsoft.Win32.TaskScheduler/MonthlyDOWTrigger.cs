using System;
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
public sealed class MonthlyDOWTrigger : Trigger, ICalendarTrigger, ITriggerDelay, IXmlSerializable
{
	[DefaultValue(0)]
	public DaysOfTheWeek DaysOfWeek
	{
		get
		{
			if (v2Trigger == null)
			{
				return v1TriggerData.Data.monthlyDOW.DaysOfTheWeek;
			}
			return (DaysOfTheWeek)((IMonthlyDOWTrigger)v2Trigger).DaysOfWeek;
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyDOWTrigger)v2Trigger).DaysOfWeek = (short)value;
			}
			else
			{
				v1TriggerData.Data.monthlyDOW.DaysOfTheWeek = value;
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["DaysOfWeek"] = (short)value;
				}
			}
			OnNotifyPropertyChanged("DaysOfWeek");
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
			return (MonthsOfTheYear)((IMonthlyDOWTrigger)v2Trigger).MonthsOfYear;
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyDOWTrigger)v2Trigger).MonthsOfYear = (short)value;
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
			return Task.StringToTimeSpan(((IMonthlyDOWTrigger)v2Trigger).RandomDelay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyDOWTrigger)v2Trigger).RandomDelay = Task.TimeSpanToString(value);
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
	public bool RunOnLastWeekOfMonth
	{
		get
		{
			return ((IMonthlyDOWTrigger)v2Trigger)?.RunOnLastWeekOfMonth ?? GetUnboundValueOrDefault("RunOnLastWeekOfMonth", def: false);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IMonthlyDOWTrigger)v2Trigger).RunOnLastWeekOfMonth = value;
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["RunOnLastWeekOfMonth"] = value;
			}
			OnNotifyPropertyChanged("RunOnLastWeekOfMonth");
		}
	}

	[DefaultValue(0)]
	public WhichWeek WeeksOfMonth
	{
		get
		{
			if (v2Trigger == null)
			{
				if (v1Trigger == null)
				{
					return GetUnboundValueOrDefault("WeeksOfMonth", WhichWeek.FirstWeek);
				}
				return v1TriggerData.Data.monthlyDOW.V2WhichWeek;
			}
			WhichWeek whichWeek = (WhichWeek)((IMonthlyDOWTrigger)v2Trigger).WeeksOfMonth;
			if (((IMonthlyDOWTrigger)v2Trigger).RunOnLastWeekOfMonth)
			{
				whichWeek |= WhichWeek.LastWeek;
			}
			return whichWeek;
		}
		set
		{
			if (value.IsFlagSet(WhichWeek.LastWeek))
			{
				RunOnLastWeekOfMonth = true;
			}
			if (v2Trigger != null)
			{
				((IMonthlyDOWTrigger)v2Trigger).WeeksOfMonth = (short)value;
			}
			else
			{
				try
				{
					v1TriggerData.Data.monthlyDOW.V2WhichWeek = value;
				}
				catch (NotV1SupportedException)
				{
					if (v1Trigger != null)
					{
						throw;
					}
				}
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["WeeksOfMonth"] = (short)value;
				}
			}
			OnNotifyPropertyChanged("WeeksOfMonth");
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

	public MonthlyDOWTrigger(DaysOfTheWeek daysOfWeek = DaysOfTheWeek.Sunday, MonthsOfTheYear monthsOfYear = MonthsOfTheYear.AllMonths, WhichWeek weeksOfMonth = WhichWeek.FirstWeek)
		: base(TaskTriggerType.MonthlyDOW)
	{
		DaysOfWeek = daysOfWeek;
		MonthsOfYear = monthsOfYear;
		WeeksOfMonth = weeksOfMonth;
	}

	internal MonthlyDOWTrigger([NotNull] ITaskTrigger iTrigger)
		: base(iTrigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunMonthlyDOW)
	{
		if (v1TriggerData.Data.monthlyDOW.Months == (MonthsOfTheYear)0)
		{
			v1TriggerData.Data.monthlyDOW.Months = MonthsOfTheYear.AllMonths;
		}
		if (v1TriggerData.Data.monthlyDOW.DaysOfTheWeek == (DaysOfTheWeek)0)
		{
			v1TriggerData.Data.monthlyDOW.DaysOfTheWeek = DaysOfTheWeek.Sunday;
		}
	}

	internal MonthlyDOWTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public override void CopyProperties(Trigger sourceTrigger)
	{
		base.CopyProperties(sourceTrigger);
		if (sourceTrigger is MonthlyDOWTrigger monthlyDOWTrigger)
		{
			DaysOfWeek = monthlyDOWTrigger.DaysOfWeek;
			MonthsOfYear = monthlyDOWTrigger.MonthsOfYear;
			try
			{
				RunOnLastWeekOfMonth = monthlyDOWTrigger.RunOnLastWeekOfMonth;
			}
			catch
			{
			}
			WeeksOfMonth = monthlyDOWTrigger.WeeksOfMonth;
		}
		if (sourceTrigger is MonthlyTrigger monthlyTrigger)
		{
			MonthsOfYear = monthlyTrigger.MonthsOfYear;
		}
	}

	public override bool Equals(Trigger other)
	{
		if (other is MonthlyDOWTrigger monthlyDOWTrigger && base.Equals(other) && DaysOfWeek == monthlyDOWTrigger.DaysOfWeek && MonthsOfYear == monthlyDOWTrigger.MonthsOfYear && WeeksOfMonth == monthlyDOWTrigger.WeeksOfMonth && v1Trigger == null)
		{
			return RunOnLastWeekOfMonth == monthlyDOWTrigger.RunOnLastWeekOfMonth;
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
		string @string = TaskEnumGlobalizer.GetString(WeeksOfMonth);
		string string2 = TaskEnumGlobalizer.GetString(DaysOfWeek);
		string string3 = TaskEnumGlobalizer.GetString(MonthsOfYear);
		return string.Format(Resources.TriggerMonthlyDOW1, Trigger.AdjustToLocal(base.StartBoundary), @string, string2, string3);
	}

	private void ReadMyXml([NotNull] XmlReader reader)
	{
		reader.ReadStartElement("ScheduleByMonthDayOfWeek");
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			switch (reader.LocalName)
			{
			case "Weeks":
				reader.Read();
				while (reader.MoveToContent() == XmlNodeType.Element)
				{
					if (reader.LocalName == "Week")
					{
						string text = reader.ReadElementContentAsString();
						if (text == "Last")
						{
							WeeksOfMonth = WhichWeek.LastWeek;
							continue;
						}
						WeeksOfMonth = int.Parse(text) switch
						{
							1 => WhichWeek.FirstWeek, 
							2 => WhichWeek.SecondWeek, 
							3 => WhichWeek.ThirdWeek, 
							4 => WhichWeek.FourthWeek, 
							_ => throw new XmlException("Week element must contain a 1-4 or Last as content."), 
						};
					}
				}
				reader.ReadEndElement();
				break;
			case "DaysOfWeek":
				reader.Read();
				DaysOfWeek = (DaysOfTheWeek)0;
				while (reader.MoveToContent() == XmlNodeType.Element)
				{
					try
					{
						DaysOfWeek |= (DaysOfTheWeek)Enum.Parse(typeof(DaysOfTheWeek), reader.LocalName);
					}
					catch
					{
						throw new XmlException("Invalid days of the week element.");
					}
					reader.Read();
				}
				reader.ReadEndElement();
				break;
			case "Months":
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
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.ReadEndElement();
	}

	private void WriteMyXml([NotNull] XmlWriter writer)
	{
		writer.WriteStartElement("ScheduleByMonthDayOfWeek");
		writer.WriteStartElement("Weeks");
		if ((WeeksOfMonth & WhichWeek.FirstWeek) == WhichWeek.FirstWeek)
		{
			writer.WriteElementString("Week", "1");
		}
		if ((WeeksOfMonth & WhichWeek.SecondWeek) == WhichWeek.SecondWeek)
		{
			writer.WriteElementString("Week", "2");
		}
		if ((WeeksOfMonth & WhichWeek.ThirdWeek) == WhichWeek.ThirdWeek)
		{
			writer.WriteElementString("Week", "3");
		}
		if ((WeeksOfMonth & WhichWeek.FourthWeek) == WhichWeek.FourthWeek)
		{
			writer.WriteElementString("Week", "4");
		}
		if ((WeeksOfMonth & WhichWeek.LastWeek) == WhichWeek.LastWeek)
		{
			writer.WriteElementString("Week", "Last");
		}
		writer.WriteEndElement();
		writer.WriteStartElement("DaysOfWeek");
		foreach (DaysOfTheWeek value in Enum.GetValues(typeof(DaysOfTheWeek)))
		{
			if (value != DaysOfTheWeek.AllDays && (DaysOfWeek & value) == value)
			{
				writer.WriteElementString(value.ToString(), null);
			}
		}
		writer.WriteEndElement();
		writer.WriteStartElement("Months");
		foreach (MonthsOfTheYear value2 in Enum.GetValues(typeof(MonthsOfTheYear)))
		{
			if (value2 != MonthsOfTheYear.AllMonths && (MonthsOfYear & value2) == value2)
			{
				writer.WriteElementString(value2.ToString(), null);
			}
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}
}
