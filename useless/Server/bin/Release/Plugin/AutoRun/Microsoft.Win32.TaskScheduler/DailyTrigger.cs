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
public sealed class DailyTrigger : Trigger, ICalendarTrigger, ITriggerDelay, IXmlSerializable
{
	[DefaultValue(1)]
	public short DaysInterval
	{
		get
		{
			if (v2Trigger != null)
			{
				return ((IDailyTrigger)v2Trigger).DaysInterval;
			}
			return (short)v1TriggerData.Data.daily.DaysInterval;
		}
		set
		{
			if (v2Trigger != null)
			{
				((IDailyTrigger)v2Trigger).DaysInterval = value;
			}
			else
			{
				v1TriggerData.Data.daily.DaysInterval = (ushort)value;
				if (v1Trigger != null)
				{
					SetV1TriggerData();
				}
				else
				{
					unboundValues["DaysInterval"] = value;
				}
			}
			OnNotifyPropertyChanged("DaysInterval");
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
			return Task.StringToTimeSpan(((IDailyTrigger)v2Trigger).RandomDelay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IDailyTrigger)v2Trigger).RandomDelay = Task.TimeSpanToString(value);
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

	public DailyTrigger(short daysInterval = 1)
		: base(TaskTriggerType.Daily)
	{
		DaysInterval = daysInterval;
	}

	internal DailyTrigger([NotNull] ITaskTrigger iTrigger)
		: base(iTrigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.RunDaily)
	{
		if (v1TriggerData.Data.daily.DaysInterval == 0)
		{
			v1TriggerData.Data.daily.DaysInterval = 1;
		}
	}

	internal DailyTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public override void CopyProperties(Trigger sourceTrigger)
	{
		base.CopyProperties(sourceTrigger);
		if (sourceTrigger is DailyTrigger dailyTrigger)
		{
			DaysInterval = dailyTrigger.DaysInterval;
		}
	}

	public override bool Equals(Trigger other)
	{
		if (other is DailyTrigger dailyTrigger && base.Equals(dailyTrigger))
		{
			return DaysInterval == dailyTrigger.DaysInterval;
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
		if (DaysInterval != 1)
		{
			return string.Format(Resources.TriggerDaily2, Trigger.AdjustToLocal(base.StartBoundary), DaysInterval);
		}
		return string.Format(Resources.TriggerDaily1, Trigger.AdjustToLocal(base.StartBoundary));
	}

	private void ReadMyXml(XmlReader reader)
	{
		reader.ReadStartElement("ScheduleByDay");
		if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DaysInterval")
		{
			DaysInterval = (short)reader.ReadElementContentAs(typeof(short), null);
		}
		reader.Read();
		reader.ReadEndElement();
	}

	private void WriteMyXml(XmlWriter writer)
	{
		writer.WriteStartElement("ScheduleByDay");
		writer.WriteElementString("DaysInterval", DaysInterval.ToString());
		writer.WriteEndElement();
	}
}
