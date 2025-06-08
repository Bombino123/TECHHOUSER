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
public sealed class RegistrationTrigger : Trigger, ITriggerDelay
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
			return Task.StringToTimeSpan(((IRegistrationTrigger)v2Trigger).Delay);
		}
		set
		{
			if (v2Trigger != null)
			{
				((IRegistrationTrigger)v2Trigger).Delay = Task.TimeSpanToString(value);
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

	public RegistrationTrigger()
		: base(TaskTriggerType.Registration)
	{
	}

	internal RegistrationTrigger([NotNull] ITrigger iTrigger)
		: base(iTrigger)
	{
	}

	protected override string V2GetTriggerString()
	{
		return Resources.TriggerRegistration1;
	}
}
