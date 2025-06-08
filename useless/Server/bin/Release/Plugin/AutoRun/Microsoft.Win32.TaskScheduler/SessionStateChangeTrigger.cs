using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = false)]
[ComVisible(true)]
public sealed class SessionStateChangeTrigger : Trigger, ITriggerDelay, ITriggerUserId
{
	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Delay
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault("Delay", TimeSpan.Zero);
			}
			return Task.StringToTimeSpan(((ISessionStateChangeTrigger)v2Trigger).Delay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((ISessionStateChangeTrigger)v2Trigger).Delay = Task.TimeSpanToString(value);
			}
			else
			{
				unboundValues["Delay"] = value;
			}
			OnNotifyPropertyChanged("Delay");
		}
	}

	[DefaultValue(1)]
	public TaskSessionStateChangeType StateChange
	{
		get
		{
			return ((ISessionStateChangeTrigger)v2Trigger)?.StateChange ?? GetUnboundValueOrDefault("StateChange", TaskSessionStateChangeType.ConsoleConnect);
		}
		set
		{
			if (v2Trigger != null)
			{
				((ISessionStateChangeTrigger)v2Trigger).StateChange = value;
			}
			else
			{
				unboundValues["StateChange"] = value;
			}
			OnNotifyPropertyChanged("StateChange");
		}
	}

	[DefaultValue(null)]
	public string UserId
	{
		get
		{
			if (v2Trigger == null)
			{
				return GetUnboundValueOrDefault<string>("UserId");
			}
			return ((ISessionStateChangeTrigger)v2Trigger).UserId;
		}
		set
		{
			if (v2Trigger != null)
			{
				((ISessionStateChangeTrigger)v2Trigger).UserId = value;
			}
			else
			{
				unboundValues["UserId"] = value;
			}
			OnNotifyPropertyChanged("UserId");
		}
	}

	public SessionStateChangeTrigger()
		: base(TaskTriggerType.SessionStateChange)
	{
	}

	public SessionStateChangeTrigger(TaskSessionStateChangeType stateChange, string userId = null)
		: this()
	{
		StateChange = stateChange;
		UserId = userId;
	}

	internal SessionStateChangeTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	public override void CopyProperties(Trigger sourceTrigger)
	{
		base.CopyProperties(sourceTrigger);
		if (sourceTrigger is SessionStateChangeTrigger sessionStateChangeTrigger && !StateChangeIsSet())
		{
			StateChange = sessionStateChangeTrigger.StateChange;
		}
	}

	public override bool Equals(Trigger other)
	{
		if (other is SessionStateChangeTrigger sessionStateChangeTrigger && base.Equals(sessionStateChangeTrigger))
		{
			return StateChange == sessionStateChangeTrigger.StateChange;
		}
		return false;
	}

	protected override string V2GetTriggerString()
	{
		string? @string = Resources.ResourceManager.GetString("TriggerSession" + StateChange);
		string arg = (string.IsNullOrEmpty(UserId) ? Resources.TriggerAnyUser : UserId);
		if (StateChange != TaskSessionStateChangeType.SessionLock && StateChange != TaskSessionStateChangeType.SessionUnlock)
		{
			arg = string.Format(Resources.TriggerSessionUserSession, arg);
		}
		return string.Format(@string, arg);
	}

	private bool StateChangeIsSet()
	{
		if (v2Trigger == null)
		{
			return unboundValues?.ContainsKey("StateChange") ?? false;
		}
		return true;
	}
}
