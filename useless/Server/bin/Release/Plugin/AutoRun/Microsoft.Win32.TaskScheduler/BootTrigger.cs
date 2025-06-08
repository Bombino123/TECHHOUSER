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
public sealed class BootTrigger : Trigger, ITriggerDelay
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
			return Task.StringToTimeSpan(((IBootTrigger)v2Trigger).Delay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IBootTrigger)v2Trigger).Delay = Task.TimeSpanToString(value);
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

	public BootTrigger()
		: base(TaskTriggerType.Boot)
	{
	}

	internal BootTrigger([NotNull] ITaskTrigger iTrigger)
		: base(iTrigger, Microsoft.Win32.TaskScheduler.V1Interop.TaskTriggerType.OnSystemStart)
	{
	}

	internal BootTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	protected override string V2GetTriggerString()
	{
		return Resources.TriggerBoot1;
	}
}
