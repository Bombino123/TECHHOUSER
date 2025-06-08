using System;
using System.ComponentModel;
using System.Diagnostics;
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

[XmlRoot("Settings", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = true)]
[PublicAPI]
[ComVisible(true)]
public sealed class TaskSettings : IDisposable, IXmlSerializable, INotifyPropertyChanged
{
	private const uint InfiniteRunTimeV1 = uint.MaxValue;

	private readonly ITaskSettings v2Settings;

	private readonly ITaskSettings2 v2Settings2;

	private readonly ITaskSettings3 v2Settings3;

	private IdleSettings idleSettings;

	private MaintenanceSettings maintenanceSettings;

	private NetworkSettings networkSettings;

	private ITask v1Task;

	[DefaultValue(true)]
	[XmlElement("AllowStartOnDemand")]
	[XmlIgnore]
	public bool AllowDemandStart
	{
		get
		{
			if (v2Settings != null)
			{
				return v2Settings.AllowDemandStart;
			}
			return true;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.AllowDemandStart = value;
				OnNotifyPropertyChanged("AllowDemandStart");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(true)]
	[XmlIgnore]
	public bool AllowHardTerminate
	{
		get
		{
			if (v2Settings != null)
			{
				return v2Settings.AllowHardTerminate;
			}
			return true;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.AllowHardTerminate = value;
				OnNotifyPropertyChanged("AllowHardTerminate");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[XmlIgnore]
	public TaskCompatibility Compatibility
	{
		get
		{
			return v2Settings?.Compatibility ?? TaskCompatibility.V1;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.Compatibility = value;
			}
			else if (value != TaskCompatibility.V1)
			{
				throw new NotV1SupportedException();
			}
			OnNotifyPropertyChanged("Compatibility");
		}
	}

	[DefaultValue(typeof(TimeSpan), "12:00:00")]
	public TimeSpan DeleteExpiredTaskAfter
	{
		get
		{
			if (v2Settings != null)
			{
				if (!(v2Settings.DeleteExpiredTaskAfter == "PT0S"))
				{
					return Task.StringToTimeSpan(v2Settings.DeleteExpiredTaskAfter);
				}
				return TimeSpan.FromSeconds(1.0);
			}
			if (!v1Task.HasFlags(TaskFlags.DeleteWhenDone))
			{
				return TimeSpan.Zero;
			}
			return TimeSpan.FromSeconds(1.0);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.DeleteExpiredTaskAfter = ((value == TimeSpan.FromSeconds(1.0)) ? "PT0S" : Task.TimeSpanToString(value));
			}
			else
			{
				v1Task.SetFlags(TaskFlags.DeleteWhenDone, value >= TimeSpan.FromSeconds(1.0));
			}
			OnNotifyPropertyChanged("DeleteExpiredTaskAfter");
		}
	}

	[DefaultValue(true)]
	public bool DisallowStartIfOnBatteries
	{
		get
		{
			return v2Settings?.DisallowStartIfOnBatteries ?? v1Task.HasFlags(TaskFlags.DontStartIfOnBatteries);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.DisallowStartIfOnBatteries = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.DontStartIfOnBatteries, value);
			}
			OnNotifyPropertyChanged("DisallowStartIfOnBatteries");
		}
	}

	[DefaultValue(false)]
	[XmlIgnore]
	public bool DisallowStartOnRemoteAppSession
	{
		get
		{
			if (v2Settings2 != null)
			{
				return v2Settings2.DisallowStartOnRemoteAppSession;
			}
			if (v2Settings3 != null)
			{
				return v2Settings3.DisallowStartOnRemoteAppSession;
			}
			return false;
		}
		set
		{
			if (v2Settings2 != null)
			{
				v2Settings2.DisallowStartOnRemoteAppSession = value;
			}
			else
			{
				if (v2Settings3 == null)
				{
					throw new NotSupportedPriorToException(TaskCompatibility.V2_1);
				}
				v2Settings3.DisallowStartOnRemoteAppSession = value;
			}
			OnNotifyPropertyChanged("DisallowStartOnRemoteAppSession");
		}
	}

	[DefaultValue(true)]
	public bool Enabled
	{
		get
		{
			return v2Settings?.Enabled ?? (!v1Task.HasFlags(TaskFlags.Disabled));
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.Enabled = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.Disabled, !value);
			}
			OnNotifyPropertyChanged("Enabled");
		}
	}

	[DefaultValue(typeof(TimeSpan), "3")]
	public TimeSpan ExecutionTimeLimit
	{
		get
		{
			if (v2Settings != null)
			{
				return Task.StringToTimeSpan(v2Settings.ExecutionTimeLimit);
			}
			uint maxRunTime = v1Task.GetMaxRunTime();
			if (maxRunTime != uint.MaxValue)
			{
				return TimeSpan.FromMilliseconds(maxRunTime);
			}
			return TimeSpan.Zero;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.ExecutionTimeLimit = ((value == TimeSpan.Zero) ? "PT0S" : Task.TimeSpanToString(value));
			}
			else
			{
				uint maxRunTime = ((value == TimeSpan.Zero) ? uint.MaxValue : Convert.ToUInt32(value.TotalMilliseconds));
				v1Task.SetMaxRunTime(maxRunTime);
				if (value == TimeSpan.Zero && v1Task.GetMaxRunTime() != uint.MaxValue)
				{
					v1Task.SetMaxRunTime(4294967294u);
				}
			}
			OnNotifyPropertyChanged("ExecutionTimeLimit");
		}
	}

	[DefaultValue(false)]
	public bool Hidden
	{
		get
		{
			return v2Settings?.Hidden ?? v1Task.HasFlags(TaskFlags.Hidden);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.Hidden = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.Hidden, value);
			}
			OnNotifyPropertyChanged("Hidden");
		}
	}

	[XmlIgnore]
	[NotNull]
	public MaintenanceSettings MaintenanceSettings => maintenanceSettings ?? (maintenanceSettings = new MaintenanceSettings(v2Settings3));

	[DefaultValue(typeof(TaskInstancesPolicy), "IgnoreNew")]
	[XmlIgnore]
	public TaskInstancesPolicy MultipleInstances
	{
		get
		{
			return v2Settings?.MultipleInstances ?? TaskInstancesPolicy.IgnoreNew;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.MultipleInstances = value;
				OnNotifyPropertyChanged("MultipleInstances");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(typeof(ProcessPriorityClass), "Normal")]
	public ProcessPriorityClass Priority
	{
		get
		{
			if (v2Settings == null)
			{
				return (ProcessPriorityClass)v1Task.GetPriority();
			}
			return GetPriorityFromInt(v2Settings.Priority);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.Priority = GetPriorityAsInt(value);
			}
			else
			{
				if (value == ProcessPriorityClass.AboveNormal || value == ProcessPriorityClass.BelowNormal)
				{
					throw new NotV1SupportedException("Unsupported priority level on Task Scheduler 1.0.");
				}
				v1Task.SetPriority((uint)value);
			}
			OnNotifyPropertyChanged("Priority");
		}
	}

	[DefaultValue(0)]
	[XmlIgnore]
	public int RestartCount
	{
		get
		{
			return v2Settings?.RestartCount ?? 0;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.RestartCount = value;
				OnNotifyPropertyChanged("RestartCount");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	[XmlIgnore]
	public TimeSpan RestartInterval
	{
		get
		{
			if (v2Settings == null)
			{
				return TimeSpan.Zero;
			}
			return Task.StringToTimeSpan(v2Settings.RestartInterval);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.RestartInterval = Task.TimeSpanToString(value);
				OnNotifyPropertyChanged("RestartInterval");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(false)]
	public bool RunOnlyIfIdle
	{
		get
		{
			return v2Settings?.RunOnlyIfIdle ?? v1Task.HasFlags(TaskFlags.StartOnlyIfIdle);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.RunOnlyIfIdle = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.StartOnlyIfIdle, value);
			}
			OnNotifyPropertyChanged("RunOnlyIfIdle");
		}
	}

	[XmlIgnore]
	public bool RunOnlyIfLoggedOn
	{
		get
		{
			if (v2Settings == null)
			{
				return v1Task.HasFlags(TaskFlags.RunOnlyIfLoggedOn);
			}
			return true;
		}
		set
		{
			if (v1Task != null)
			{
				v1Task.SetFlags(TaskFlags.RunOnlyIfLoggedOn, value);
			}
			else if (v2Settings != null)
			{
				throw new NotV2SupportedException("Task Scheduler 2.0 (1.2) does not support setting this property. You must use an InteractiveToken in order to have the task run in the current user session.");
			}
			OnNotifyPropertyChanged("RunOnlyIfLoggedOn");
		}
	}

	[DefaultValue(false)]
	public bool RunOnlyIfNetworkAvailable
	{
		get
		{
			return v2Settings?.RunOnlyIfNetworkAvailable ?? v1Task.HasFlags(TaskFlags.RunIfConnectedToInternet);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.RunOnlyIfNetworkAvailable = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.RunIfConnectedToInternet, value);
			}
			OnNotifyPropertyChanged("RunOnlyIfNetworkAvailable");
		}
	}

	[DefaultValue(false)]
	[XmlIgnore]
	public bool StartWhenAvailable
	{
		get
		{
			if (v2Settings != null)
			{
				return v2Settings.StartWhenAvailable;
			}
			return false;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.StartWhenAvailable = value;
				OnNotifyPropertyChanged("StartWhenAvailable");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(true)]
	public bool StopIfGoingOnBatteries
	{
		get
		{
			return v2Settings?.StopIfGoingOnBatteries ?? v1Task.HasFlags(TaskFlags.KillIfGoingOnBatteries);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.StopIfGoingOnBatteries = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.KillIfGoingOnBatteries, value);
			}
			OnNotifyPropertyChanged("StopIfGoingOnBatteries");
		}
	}

	[DefaultValue(false)]
	[XmlIgnore]
	public bool UseUnifiedSchedulingEngine
	{
		get
		{
			if (v2Settings2 != null)
			{
				return v2Settings2.UseUnifiedSchedulingEngine;
			}
			if (v2Settings3 != null)
			{
				return v2Settings3.UseUnifiedSchedulingEngine;
			}
			return false;
		}
		set
		{
			if (v2Settings2 != null)
			{
				v2Settings2.UseUnifiedSchedulingEngine = value;
			}
			else
			{
				if (v2Settings3 == null)
				{
					throw new NotSupportedPriorToException(TaskCompatibility.V2_1);
				}
				v2Settings3.UseUnifiedSchedulingEngine = value;
			}
			OnNotifyPropertyChanged("UseUnifiedSchedulingEngine");
		}
	}

	[DefaultValue(false)]
	[XmlIgnore]
	public bool Volatile
	{
		get
		{
			if (v2Settings3 != null)
			{
				return v2Settings3.Volatile;
			}
			return false;
		}
		set
		{
			if (v2Settings3 != null)
			{
				v2Settings3.Volatile = value;
				OnNotifyPropertyChanged("Volatile");
				return;
			}
			throw new NotSupportedPriorToException(TaskCompatibility.V2_2);
		}
	}

	[DefaultValue(false)]
	public bool WakeToRun
	{
		get
		{
			return v2Settings?.WakeToRun ?? v1Task.HasFlags(TaskFlags.SystemRequired);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.WakeToRun = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.SystemRequired, value);
			}
			OnNotifyPropertyChanged("WakeToRun");
		}
	}

	[XmlIgnore]
	public string XmlText
	{
		get
		{
			if (v2Settings == null)
			{
				return XmlSerializationHelper.WriteObjectToXmlText(this);
			}
			return v2Settings.XmlText;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.XmlText = value;
			}
			else
			{
				XmlSerializationHelper.ReadObjectFromXmlText(value, this);
			}
			OnNotifyPropertyChanged("XmlText");
		}
	}

	[NotNull]
	public IdleSettings IdleSettings => idleSettings ?? (idleSettings = ((v2Settings != null) ? new IdleSettings(v2Settings.IdleSettings) : new IdleSettings(v1Task)));

	[XmlIgnore]
	[NotNull]
	public NetworkSettings NetworkSettings => networkSettings ?? (networkSettings = new NetworkSettings(v2Settings?.NetworkSettings));

	public event PropertyChangedEventHandler PropertyChanged;

	internal TaskSettings([NotNull] ITaskSettings iSettings)
	{
		v2Settings = iSettings;
		try
		{
			if (Environment.OSVersion.Version >= new Version(6, 1))
			{
				v2Settings2 = (ITaskSettings2)v2Settings;
			}
		}
		catch
		{
		}
		try
		{
			if (Environment.OSVersion.Version >= new Version(6, 2))
			{
				v2Settings3 = (ITaskSettings3)v2Settings;
			}
		}
		catch
		{
		}
	}

	internal TaskSettings([NotNull] ITask iTask)
	{
		v1Task = iTask;
	}

	public void Dispose()
	{
		if (v2Settings != null)
		{
			Marshal.ReleaseComObject(v2Settings);
		}
		idleSettings = null;
		networkSettings = null;
		v1Task = null;
	}

	public override string ToString()
	{
		if (v2Settings != null || v1Task != null)
		{
			return DebugHelper.GetDebugString(this);
		}
		return base.ToString();
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
			XmlSerializationHelper.ReadObjectProperties(reader, this, ConvertXmlProperty);
			reader.ReadEndElement();
		}
		else
		{
			reader.Skip();
		}
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		XmlSerializationHelper.WriteObjectProperties(writer, this, ConvertXmlProperty);
	}

	private bool ConvertXmlProperty(PropertyInfo pi, object obj, ref object value)
	{
		if (pi.Name == "Priority" && value != null)
		{
			if (value is int)
			{
				value = GetPriorityFromInt((int)value);
			}
			else if (value is ProcessPriorityClass)
			{
				value = GetPriorityAsInt((ProcessPriorityClass)value);
			}
			return true;
		}
		return false;
	}

	private int GetPriorityAsInt(ProcessPriorityClass value)
	{
		if (value <= (ProcessPriorityClass)10 && value >= (ProcessPriorityClass)0)
		{
			return (int)value;
		}
		int result = 7;
		switch (value)
		{
		case ProcessPriorityClass.AboveNormal:
			result = 3;
			break;
		case ProcessPriorityClass.High:
			result = 1;
			break;
		case ProcessPriorityClass.Idle:
			result = 10;
			break;
		case ProcessPriorityClass.Normal:
			result = 5;
			break;
		case ProcessPriorityClass.RealTime:
			result = 0;
			break;
		}
		return result;
	}

	private ProcessPriorityClass GetPriorityFromInt(int value)
	{
		switch (value)
		{
		case 0:
			return ProcessPriorityClass.RealTime;
		case 1:
			return ProcessPriorityClass.High;
		case 2:
		case 3:
			return ProcessPriorityClass.AboveNormal;
		case 4:
		case 5:
		case 6:
			return ProcessPriorityClass.Normal;
		default:
			return ProcessPriorityClass.BelowNormal;
		case 9:
		case 10:
			return ProcessPriorityClass.Idle;
		}
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
