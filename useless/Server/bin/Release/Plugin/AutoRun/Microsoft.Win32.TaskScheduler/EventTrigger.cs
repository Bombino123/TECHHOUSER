using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = false)]
[ComVisible(true)]
public sealed class EventTrigger : Trigger, ITriggerDelay
{
	private NamedValueCollection nvc;

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Delay
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault("Delay", TimeSpan.Zero);
			}
			return Task.StringToTimeSpan(((IEventTrigger)v2Trigger).Delay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IEventTrigger)v2Trigger).Delay = Task.TimeSpanToString(value);
			}
			else
			{
				unboundValues["Delay"] = value;
			}
			OnNotifyPropertyChanged("Delay");
		}
	}

	[DefaultValue(null)]
	public string Subscription
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault<string>("Subscription");
			}
			return ((IEventTrigger)v2Trigger).Subscription;
		}
		set
		{
			if (v2Trigger != null)
			{
				((IEventTrigger)v2Trigger).Subscription = value;
			}
			else
			{
				unboundValues["Subscription"] = value;
			}
			OnNotifyPropertyChanged("Subscription");
		}
	}

	[XmlArray]
	[XmlArrayItem("Value", typeof(NameValuePair))]
	public NamedValueCollection ValueQueries => nvc ?? (nvc = ((v2Trigger == null) ? new NamedValueCollection() : new NamedValueCollection(((IEventTrigger)v2Trigger).ValueQueries)));

	public EventTrigger()
		: base(TaskTriggerType.Event)
	{
	}

	public EventTrigger(string log, string source, int? eventId)
		: this()
	{
		SetBasic(log, source, eventId);
	}

	internal EventTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public static string BuildQuery(string log, string source, int? eventId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (string.IsNullOrEmpty(log))
		{
			throw new ArgumentNullException("log");
		}
		stringBuilder.AppendFormat("<QueryList><Query Id=\"0\" Path=\"{0}\"><Select Path=\"{0}\">*", log);
		bool flag = !string.IsNullOrEmpty(source);
		bool hasValue = eventId.HasValue;
		if (flag || hasValue)
		{
			stringBuilder.Append("[System[");
			if (flag)
			{
				stringBuilder.AppendFormat("Provider[@Name='{0}']", source);
			}
			if (flag && hasValue)
			{
				stringBuilder.Append(" and ");
			}
			if (hasValue)
			{
				stringBuilder.AppendFormat("EventID={0}", eventId.Value);
			}
			stringBuilder.Append("]]");
		}
		stringBuilder.Append("</Select></Query></QueryList>");
		return stringBuilder.ToString();
	}

	public override void CopyProperties(Trigger sourceTrigger)
	{
		base.CopyProperties(sourceTrigger);
		if (sourceTrigger is EventTrigger eventTrigger)
		{
			Subscription = eventTrigger.Subscription;
			eventTrigger.ValueQueries.CopyTo(ValueQueries);
		}
	}

	public override bool Equals(Trigger other)
	{
		if (other is EventTrigger eventTrigger && base.Equals(eventTrigger))
		{
			return Subscription == eventTrigger.Subscription;
		}
		return false;
	}

	public bool GetBasic(out string log, out string source, out int? eventId)
	{
		log = (source = null);
		eventId = null;
		if (!string.IsNullOrEmpty(Subscription))
		{
			using MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(Subscription));
			using XmlTextReader xmlTextReader = new XmlTextReader(input)
			{
				WhitespaceHandling = WhitespaceHandling.None
			};
			try
			{
				xmlTextReader.MoveToContent();
				xmlTextReader.ReadStartElement("QueryList");
				if (xmlTextReader.Name == "Query" && xmlTextReader.MoveToAttribute("Path"))
				{
					string value = xmlTextReader.Value;
					if (xmlTextReader.MoveToElement() && xmlTextReader.ReadToDescendant("Select") && value.Equals(xmlTextReader["Path"], StringComparison.InvariantCultureIgnoreCase))
					{
						Match match = Regex.Match(xmlTextReader.ReadString(), "\\*(?:\\[System\\[(?:Provider\\[\\@Name='(?<s>[^']+)'\\])?(?:\\s+and\\s+)?(?:EventID=(?<e>\\d+))?\\]\\])", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
						if (match.Success)
						{
							log = value;
							if (match.Groups["s"].Success)
							{
								source = match.Groups["s"].Value;
							}
							if (match.Groups["e"].Success)
							{
								eventId = Convert.ToInt32(match.Groups["e"].Value);
							}
							return true;
						}
					}
				}
			}
			catch
			{
			}
		}
		return false;
	}

	public void SetBasic([NotNull] string log, string source, int? eventId)
	{
		ValueQueries.Clear();
		Subscription = BuildQuery(log, source, eventId);
	}

	internal override void Bind(ITaskDefinition iTaskDef)
	{
		base.Bind(iTaskDef);
		nvc?.Bind(((IEventTrigger)v2Trigger).ValueQueries);
	}

	protected override string V2GetTriggerString()
	{
		if (!GetBasic(out var log, out var source, out var eventId))
		{
			return Resources.TriggerEvent1;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(Resources.TriggerEventBasic1, log);
		if (!string.IsNullOrEmpty(source))
		{
			stringBuilder.AppendFormat(Resources.TriggerEventBasic2, source);
		}
		if (eventId.HasValue)
		{
			stringBuilder.AppendFormat(Resources.TriggerEventBasic3, eventId.Value);
		}
		return stringBuilder.ToString();
	}
}
