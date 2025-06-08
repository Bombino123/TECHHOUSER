using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("Repetition", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = true)]
[TypeConverter(typeof(RepetitionPatternConverter))]
[ComVisible(true)]
public sealed class RepetitionPattern : IDisposable, IXmlSerializable, IEquatable<RepetitionPattern>, INotifyPropertyChanged
{
	private readonly Trigger pTrigger;

	private readonly IRepetitionPattern v2Pattern;

	private TimeSpan unboundInterval = TimeSpan.Zero;

	private TimeSpan unboundDuration = TimeSpan.Zero;

	private bool unboundStopAtDurationEnd;

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Duration
	{
		get
		{
			if (v2Pattern == null)
			{
				if (pTrigger == null)
				{
					return unboundDuration;
				}
				return TimeSpan.FromMinutes(pTrigger.v1TriggerData.MinutesDuration);
			}
			return Task.StringToTimeSpan(v2Pattern.Duration);
		}
		set
		{
			if (value.Ticks < 0 || (value != TimeSpan.Zero && value < TimeSpan.FromMinutes(1.0)))
			{
				throw new ArgumentOutOfRangeException("Duration");
			}
			if (v2Pattern != null)
			{
				v2Pattern.Duration = Task.TimeSpanToString(value);
			}
			else if (pTrigger != null)
			{
				pTrigger.v1TriggerData.MinutesDuration = (uint)value.TotalMinutes;
				Bind();
			}
			else
			{
				unboundDuration = value;
			}
			OnNotifyPropertyChanged("Duration");
		}
	}

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Interval
	{
		get
		{
			if (v2Pattern == null)
			{
				if (pTrigger == null)
				{
					return unboundInterval;
				}
				return TimeSpan.FromMinutes(pTrigger.v1TriggerData.MinutesInterval);
			}
			return Task.StringToTimeSpan(v2Pattern.Interval);
		}
		set
		{
			if (value.Ticks < 0 || ((v2Pattern != null || pTrigger == null) && value != TimeSpan.Zero && (value < TimeSpan.FromMinutes(1.0) || value > TimeSpan.FromDays(31.0))))
			{
				throw new ArgumentOutOfRangeException("Interval");
			}
			if (v2Pattern != null)
			{
				v2Pattern.Interval = Task.TimeSpanToString(value);
			}
			else if (pTrigger != null)
			{
				if (value != TimeSpan.Zero && value < TimeSpan.FromMinutes(1.0))
				{
					throw new ArgumentOutOfRangeException("Interval");
				}
				pTrigger.v1TriggerData.MinutesInterval = (uint)value.TotalMinutes;
				Bind();
			}
			else
			{
				unboundInterval = value;
			}
			OnNotifyPropertyChanged("Interval");
		}
	}

	[DefaultValue(false)]
	public bool StopAtDurationEnd
	{
		get
		{
			if (v2Pattern != null)
			{
				return v2Pattern.StopAtDurationEnd;
			}
			if (pTrigger != null)
			{
				return (pTrigger.v1TriggerData.Flags & TaskTriggerFlags.KillAtDurationEnd) == TaskTriggerFlags.KillAtDurationEnd;
			}
			return unboundStopAtDurationEnd;
		}
		set
		{
			if (v2Pattern != null)
			{
				v2Pattern.StopAtDurationEnd = value;
			}
			else if (pTrigger != null)
			{
				if (value)
				{
					pTrigger.v1TriggerData.Flags |= TaskTriggerFlags.KillAtDurationEnd;
				}
				else
				{
					pTrigger.v1TriggerData.Flags &= ~TaskTriggerFlags.KillAtDurationEnd;
				}
				Bind();
			}
			else
			{
				unboundStopAtDurationEnd = value;
			}
			OnNotifyPropertyChanged("StopAtDurationEnd");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public RepetitionPattern(TimeSpan interval, TimeSpan duration, bool stopAtDurationEnd = false)
	{
		Interval = interval;
		Duration = duration;
		StopAtDurationEnd = stopAtDurationEnd;
	}

	internal RepetitionPattern([NotNull] Trigger parent)
	{
		pTrigger = parent;
		if (pTrigger?.v2Trigger != null)
		{
			v2Pattern = pTrigger.v2Trigger.Repetition;
		}
	}

	public void Dispose()
	{
		if (v2Pattern != null)
		{
			Marshal.ReleaseComObject(v2Pattern);
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RepetitionPattern other))
		{
			return base.Equals(obj);
		}
		return Equals(other);
	}

	public bool Equals(RepetitionPattern other)
	{
		if (other != null && Duration == other.Duration && Interval == other.Interval)
		{
			return StopAtDurationEnd == other.StopAtDurationEnd;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return new
		{
			A = Duration,
			B = Interval,
			C = StopAtDurationEnd
		}.GetHashCode();
	}

	public bool IsSet()
	{
		if (v2Pattern != null)
		{
			if (!v2Pattern.StopAtDurationEnd && string.IsNullOrEmpty(v2Pattern.Duration))
			{
				return !string.IsNullOrEmpty(v2Pattern.Interval);
			}
			return true;
		}
		if (pTrigger != null)
		{
			if ((pTrigger.v1TriggerData.Flags & TaskTriggerFlags.KillAtDurationEnd) != TaskTriggerFlags.KillAtDurationEnd && pTrigger.v1TriggerData.MinutesDuration == 0)
			{
				return pTrigger.v1TriggerData.MinutesInterval != 0;
			}
			return true;
		}
		return false;
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		if (!reader.IsEmptyElement)
		{
			reader.ReadStartElement(XmlSerializationHelper.GetElementName(this), "http://schemas.microsoft.com/windows/2004/02/mit/task");
			XmlSerializationHelper.ReadObjectProperties(reader, this, ReadXmlConverter);
			reader.ReadEndElement();
		}
		else
		{
			reader.Skip();
		}
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		XmlSerializationHelper.WriteObjectProperties(writer, this);
	}

	internal void Bind()
	{
		if (pTrigger.v1Trigger != null)
		{
			pTrigger.SetV1TriggerData();
		}
		else if (pTrigger.v2Trigger != null)
		{
			if (pTrigger.v1TriggerData.MinutesInterval != 0)
			{
				v2Pattern.Interval = $"PT{pTrigger.v1TriggerData.MinutesInterval}M";
			}
			if (pTrigger.v1TriggerData.MinutesDuration != 0)
			{
				v2Pattern.Duration = $"PT{pTrigger.v1TriggerData.MinutesDuration}M";
			}
			v2Pattern.StopAtDurationEnd = (pTrigger.v1TriggerData.Flags & TaskTriggerFlags.KillAtDurationEnd) == TaskTriggerFlags.KillAtDurationEnd;
		}
	}

	internal void Set([NotNull] RepetitionPattern value)
	{
		Duration = value.Duration;
		Interval = value.Interval;
		StopAtDurationEnd = value.StopAtDurationEnd;
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private bool ReadXmlConverter(PropertyInfo pi, object obj, ref object value)
	{
		if (pi.Name != "Interval" || !(value is TimeSpan timeSpan) || timeSpan.Equals(TimeSpan.Zero) || Duration > timeSpan)
		{
			return false;
		}
		Duration = timeSpan.Add(TimeSpan.FromMinutes(1.0));
		return true;
	}
}
