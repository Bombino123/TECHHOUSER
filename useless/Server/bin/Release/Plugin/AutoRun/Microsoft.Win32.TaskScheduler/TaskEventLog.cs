using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class TaskEventLog : IEnumerable<TaskEvent>, IEnumerable
{
	private const string TSEventLogPath = "Microsoft-Windows-TaskScheduler/Operational";

	private static readonly bool IsVistaOrLater = Environment.OSVersion.Version.Major >= 6;

	public long Count
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			EventLogReader val = new EventLogReader(Query);
			try
			{
				long num = 64L;
				long num2 = 0L;
				long num3 = num;
				while (val.ReadEvent() != null)
				{
					val.Seek(SeekOrigin.Begin, num2 += num);
				}
				bool flag = false;
				while (num2 > 0 && num3 >= 1)
				{
					num2 = ((!flag) ? (num2 - (num3 /= 2)) : (num2 + (num3 /= 2)));
					val.Seek(SeekOrigin.Begin, num2);
					flag = val.ReadEvent() != null;
				}
				return flag ? (num2 + 1) : num2;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public bool Enabled
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			if (!IsVistaOrLater)
			{
				return false;
			}
			EventLogConfiguration val = new EventLogConfiguration("Microsoft-Windows-TaskScheduler/Operational", Query.Session);
			try
			{
				return val.IsEnabled;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			if (!IsVistaOrLater)
			{
				throw new NotSupportedException("Task history not available on systems prior to Windows Vista and Windows Server 2008.");
			}
			EventLogConfiguration val = new EventLogConfiguration("Microsoft-Windows-TaskScheduler/Operational", Query.Session);
			try
			{
				if (val.IsEnabled != value)
				{
					val.IsEnabled = value;
					val.SaveChanges();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	[DefaultValue(false)]
	public bool EnumerateInReverse { get; set; }

	internal EventLogQuery Query { get; private set; }

	public TaskEventLog([CanBeNull] string taskPath)
		: this(".", taskPath)
	{
		Initialize(".", BuildQuery(taskPath), revDir: true);
	}

	public TaskEventLog([NotNull] string machineName, [CanBeNull] string taskPath, string domain = null, string user = null, string password = null)
	{
		Initialize(machineName, BuildQuery(taskPath), revDir: true, domain, user, password);
	}

	public TaskEventLog(DateTime startTime, string taskName = null, string machineName = null, string domain = null, string user = null, string password = null)
	{
		int[] eventIDs = new int[16]
		{
			100, 102, 103, 107, 108, 109, 111, 117, 118, 119,
			120, 121, 122, 123, 124, 125
		};
		Initialize(machineName, BuildQuery(taskName, eventIDs, startTime), revDir: false, domain, user, password);
	}

	public TaskEventLog(string taskName = null, int[] eventIDs = null, DateTime? startTime = null, string machineName = null, string domain = null, string user = null, string password = null)
	{
		Initialize(machineName, BuildQuery(taskName, eventIDs, startTime), revDir: true, domain, user, password);
	}

	public TaskEventLog(string taskName = null, int[] eventIDs = null, int[] levels = null, DateTime? startTime = null, string machineName = null, string domain = null, string user = null, string password = null)
	{
		Initialize(machineName, BuildQuery(taskName, eventIDs, startTime, levels), revDir: true, domain, user, password);
	}

	internal static string BuildQuery(string taskName = null, int[] eventIDs = null, DateTime? startTime = null, int[] levels = null)
	{
		StringBuilder stringBuilder = new StringBuilder("*");
		if (eventIDs != null && eventIDs.Length != 0)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(" and ");
			}
			stringBuilder.AppendFormat("({0})", string.Join(" or ", Array.ConvertAll(eventIDs, (int i) => $"EventID={i}")));
		}
		if (levels != null && levels.Length != 0)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(" and ");
			}
			stringBuilder.AppendFormat("({0})", string.Join(" or ", Array.ConvertAll(levels, (int i) => $"Level={i}")));
		}
		if (startTime.HasValue)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(" and ");
			}
			stringBuilder.AppendFormat("TimeCreated[@SystemTime>='{0}']", XmlConvert.ToString(startTime.Value, XmlDateTimeSerializationMode.RoundtripKind));
		}
		if (stringBuilder.Length > 1)
		{
			stringBuilder.Insert(1, "[System[Provider[@Name='Microsoft-Windows-TaskScheduler'] and ");
			stringBuilder.Append(']');
		}
		if (!string.IsNullOrEmpty(taskName))
		{
			if (stringBuilder.Length == 1)
			{
				stringBuilder.Append('[');
			}
			else
			{
				stringBuilder.Append("] and *[");
			}
			stringBuilder.AppendFormat("EventData[Data[@Name='TaskName']='{0}']", taskName);
		}
		if (stringBuilder.Length > 1)
		{
			stringBuilder.Append(']');
		}
		return $"<QueryList>  <Query Id=\"0\" Path=\"Microsoft-Windows-TaskScheduler/Operational\">    <Select Path=\"Microsoft-Windows-TaskScheduler/Operational\">{stringBuilder}</Select>  </Query></QueryList>";
	}

	private void Initialize(string machineName, string query, bool revDir, string domain = null, string user = null, string password = null)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		if (!IsVistaOrLater)
		{
			throw new NotSupportedException("Enumeration of task history not available on systems prior to Windows Vista and Windows Server 2008.");
		}
		SecureString secureString = null;
		if (password != null)
		{
			secureString = new SecureString();
			foreach (char c in password)
			{
				secureString.AppendChar(c);
			}
		}
		Query = new EventLogQuery("Microsoft-Windows-TaskScheduler/Operational", (PathType)1, query)
		{
			ReverseDirection = revDir
		};
		if (machineName != null && machineName != "." && !machineName.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
		{
			Query.Session = new EventLogSession(machineName, domain, user, secureString, (SessionAuthentication)0);
		}
	}

	IEnumerator<TaskEvent> IEnumerable<TaskEvent>.GetEnumerator()
	{
		return GetEnumerator(EnumerateInReverse);
	}

	[NotNull]
	public TaskEventEnumerator GetEnumerator(bool reverse)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		Query.ReverseDirection = !reverse;
		return new TaskEventEnumerator(new EventLogReader(Query));
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator(reverse: false);
	}
}
