using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using System.Security.Principal;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public sealed class TaskEvent : IComparable<TaskEvent>
{
	public class EventDataValues
	{
		private readonly EventLogRecord rec;

		public string this[string propertyName]
		{
			get
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Expected O, but got Unknown
				EventLogPropertySelector val = new EventLogPropertySelector((IEnumerable<string>)new string[1] { "Event/EventData/Data[@Name='" + propertyName + "']" });
				try
				{
					return rec.GetPropertyValues(val)[0].ToString();
				}
				catch
				{
				}
				return null;
			}
		}

		internal EventDataValues(EventLogRecord eventRec)
		{
			rec = eventRec;
		}
	}

	public Guid? ActivityId { get; internal set; }

	public EventDataValues DataValues { get; }

	public int EventId { get; internal set; }

	public EventRecord EventRecord { get; internal set; }

	public StandardTaskEventId StandardEventId
	{
		get
		{
			if (Enum.IsDefined(typeof(StandardTaskEventId), EventId))
			{
				return (StandardTaskEventId)EventId;
			}
			return StandardTaskEventId.Unknown;
		}
	}

	public string Level { get; internal set; }

	public string OpCode { get; internal set; }

	public int? ProcessId { get; internal set; }

	public long? RecordId { get; internal set; }

	public string TaskCategory { get; internal set; }

	public string TaskPath { get; internal set; }

	public DateTime? TimeCreated { get; internal set; }

	public SecurityIdentifier UserId { get; internal set; }

	public byte? Version { get; internal set; }

	internal TaskEvent([NotNull] EventRecord rec)
	{
		EventId = rec.Id;
		EventRecord = rec;
		Version = rec.Version;
		TaskCategory = rec.TaskDisplayName;
		OpCode = rec.OpcodeDisplayName;
		TimeCreated = rec.TimeCreated;
		RecordId = rec.RecordId;
		ActivityId = rec.ActivityId;
		Level = rec.LevelDisplayName;
		UserId = rec.UserId;
		ProcessId = rec.ProcessId;
		object taskPath;
		if (rec.Properties.Count <= 0)
		{
			taskPath = null;
		}
		else
		{
			EventProperty obj = rec.Properties[0];
			taskPath = ((obj == null) ? null : obj.Value?.ToString());
		}
		TaskPath = (string)taskPath;
		DataValues = new EventDataValues((EventLogRecord)(object)((rec is EventLogRecord) ? rec : null));
	}

	internal TaskEvent([NotNull] string taskPath, StandardTaskEventId id, DateTime time)
	{
		EventId = (int)id;
		TaskPath = taskPath;
		TimeCreated = time;
	}

	[Obsolete("Use the DataVales property instead.")]
	public string GetDataValue(string name)
	{
		return DataValues?[name];
	}

	public override string ToString()
	{
		EventRecord eventRecord = EventRecord;
		return ((eventRecord != null) ? eventRecord.FormatDescription() : null) ?? TaskPath;
	}

	public int CompareTo(TaskEvent other)
	{
		int num = string.Compare(TaskPath, other.TaskPath, StringComparison.Ordinal);
		if (num == 0 && EventRecord != null)
		{
			num = string.Compare(ActivityId.ToString(), other.ActivityId.ToString(), StringComparison.Ordinal);
			if (num == 0)
			{
				num = Convert.ToInt32(RecordId - other.RecordId);
			}
		}
		return num;
	}
}
