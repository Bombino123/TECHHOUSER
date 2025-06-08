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
public sealed class WeeklyTrigger : Trigger, ICalendarTrigger, ITriggerDelay, IXmlSerializable
{
	[DefaultValue(0)]
	public DaysOfTheWeek DaysOfWeek
	{
		get
		{
			if (v2Trigger == null)
			{
				return v1TriggerData.Data.weekly.DaysOfTheWeek;
			}
			return (DaysOfTheWeek)((IWeeklyTrigger)v2Trigger).DaysOfWeek;
		}
		set
		{
			if (v2Trigger != null)
			{
				((IWeeklyTrigger)v2Trigger).DaysOfWeek = (short)value;
			}
			else
			{
				v1TriggerData.Data.weekly.DaysOfTheWeek = value;
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
			return Task.StringToTimeSpan(((IWeeklyTrigger)v2Trigger).RandomDelay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IWeeklyTrigger)v2Trigger).RandomDelay = Task.TimeSpanToString(value);
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

	[DefaultValue(1)]
	public short WeeksInterval
	{
		get
		{
			return ((IWeeklyTrigger)v2Trigger)?.WeeksInterval ?? ((short)v1TriggerData.Data.weekly.WeeksInterval);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IWeeklyTrigger)v2Trigger).WeeksInterval = value;
			}
			else
			{
				v1TriggerData.Data.weekly.WeeksInterval = (ushort)value;
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["WeeksInterval"] = value;
				}
			}
			OnNotifyPropertyChanged("WeeksInterval");
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

	public WeeklyTrigger(DaysOfTheWeek daysOfWeek = DaysOfTheWeek.Sunday, short weeksInterval = 1)
		: base(TaskTriggerType.Weekly)
	{
		DaysOfWeek = daysOfWeek;
		WeeksInterval = weeksInterval;
	}

	internal WeeklyTrigger([NotNull] ITaskTrigger iTrigger)
		: base(iTrigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunWeekly)
	{
		if (v1TriggerData.Data.weekly.DaysOfTheWeek == (DaysOfTheWeek)0)
		{
			v1TriggerData.Data.weekly.DaysOfTheWeek = DaysOfTheWeek.Sunday;
		}
		if (v1TriggerData.Data.weekly.WeeksInterval == 0)
		{
			v1TriggerData.Data.weekly.WeeksInterval = 1;
		}
	}

	internal WeeklyTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public override void CopyProperties(Trigger sourceTrigger)
	{
		base.CopyProperties(sourceTrigger);
		if (sourceTrigger is WeeklyTrigger weeklyTrigger)
		{
			DaysOfWeek = weeklyTrigger.DaysOfWeek;
			WeeksInterval = weeklyTrigger.WeeksInterval;
		}
	}

	public override bool Equals(Trigger other)
	{
		if (other is WeeklyTrigger weeklyTrigger && base.Equals(weeklyTrigger) && DaysOfWeek == weeklyTrigger.DaysOfWeek)
		{
			return WeeksInterval == weeklyTrigger.WeeksInterval;
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
		string @string = TaskEnumGlobalizer.GetString(DaysOfWeek);
		return string.Format((WeeksInterval == 1) ? Resources.TriggerWeekly1Week : Resources.TriggerWeeklyMultWeeks, Trigger.AdjustToLocal(base.StartBoundary), @string, WeeksInterval);
	}

	private void ReadMyXml(XmlReader reader)
	{
		reader.ReadStartElement("ScheduleByWeek");
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			string localName = reader.LocalName;
			if (!(localName == "WeeksInterval"))
			{
				if (localName == "DaysOfWeek")
				{
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
				}
				else
				{
					reader.Skip();
				}
			}
			else
			{
				WeeksInterval = (short)reader.ReadElementContentAsInt();
			}
		}
		reader.ReadEndElement();
	}

	private void WriteMyXml(XmlWriter writer)
	{
		writer.WriteStartElement("ScheduleByWeek");
		if (WeeksInterval != 1)
		{
			writer.WriteElementString("WeeksInterval", WeeksInterval.ToString());
		}
		writer.WriteStartElement("DaysOfWeek");
		foreach (DaysOfTheWeek value in Enum.GetValues(typeof(DaysOfTheWeek)))
		{
			if (value != DaysOfTheWeek.AllDays && (DaysOfWeek & value) == value)
			{
				writer.WriteElementString(value.ToString(), null);
			}
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}
}
