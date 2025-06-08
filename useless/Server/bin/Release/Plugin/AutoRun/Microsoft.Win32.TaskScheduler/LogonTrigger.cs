using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class LogonTrigger : Trigger, ITriggerDelay, ITriggerUserId
{
	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	[XmlIgnore]
	public TimeSpan Delay
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault("Delay", TimeSpan.Zero);
			}
			return Task.StringToTimeSpan(((ILogonTrigger)v2Trigger).Delay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((ILogonTrigger)v2Trigger).Delay = Task.TimeSpanToString(value);
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["Delay"] = value;
			}
			OnNotifyPropertyChanged("Delay");
		}
	}

	[DefaultValue(null)]
	[XmlIgnore]
	public string UserId
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault<string>("UserId");
			}
			return ((ILogonTrigger)v2Trigger).UserId;
		}
		set
		{
			if (v2Trigger != null)
			{
				((ILogonTrigger)v2Trigger).UserId = value;
			}
			else
			{
				if (v1Trigger != null)
				{
					throw new NotV1SupportedException();
				}
				unboundValues["UserId"] = value;
			}
			OnNotifyPropertyChanged("UserId");
		}
	}

	public LogonTrigger()
		: base(TaskTriggerType.Logon)
	{
	}

	internal LogonTrigger([NotNull] ITaskTrigger iTrigger)
		: base(iTrigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.OnLogon)
	{
	}

	internal LogonTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	protected override string V2GetTriggerString()
	{
		string arg = (string.IsNullOrEmpty(UserId) ? Resources.TriggerAnyUser : UserId);
		return string.Format(Resources.TriggerLogon1, arg);
	}
}
