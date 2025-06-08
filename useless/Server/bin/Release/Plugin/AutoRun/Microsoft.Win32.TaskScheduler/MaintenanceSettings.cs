using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = false)]
[PublicAPI]
[ComVisible(true)]
public sealed class MaintenanceSettings : IDisposable, INotifyPropertyChanged
{
	private readonly ITaskSettings3 iSettings;

	private IMaintenanceSettings iMaintSettings;

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Deadline
	{
		get
		{
			if (iMaintSettings == null)
			{
				return TimeSpan.Zero;
			}
			return Task.StringToTimeSpan(iMaintSettings.Deadline);
		}
		set
		{
			if (iSettings != null)
			{
				if (iMaintSettings == null && value != TimeSpan.Zero)
				{
					iMaintSettings = iSettings.CreateMaintenanceSettings();
				}
				if (iMaintSettings != null)
				{
					iMaintSettings.Deadline = Task.TimeSpanToString(value);
				}
				OnNotifyPropertyChanged("Deadline");
				return;
			}
			throw new NotSupportedPriorToException(TaskCompatibility.V2_2);
		}
	}

	[DefaultValue(false)]
	public bool Exclusive
	{
		get
		{
			if (iMaintSettings != null)
			{
				return iMaintSettings.Exclusive;
			}
			return false;
		}
		set
		{
			if (iSettings != null)
			{
				if (iMaintSettings == null && value)
				{
					iMaintSettings = iSettings.CreateMaintenanceSettings();
				}
				if (iMaintSettings != null)
				{
					iMaintSettings.Exclusive = value;
				}
				OnNotifyPropertyChanged("Exclusive");
				return;
			}
			throw new NotSupportedPriorToException(TaskCompatibility.V2_2);
		}
	}

	[DefaultValue(typeof(TimeSpan), "00:00:00")]
	public TimeSpan Period
	{
		get
		{
			if (iMaintSettings == null)
			{
				return TimeSpan.Zero;
			}
			return Task.StringToTimeSpan(iMaintSettings.Period);
		}
		set
		{
			if (iSettings != null)
			{
				if (iMaintSettings == null && value != TimeSpan.Zero)
				{
					iMaintSettings = iSettings.CreateMaintenanceSettings();
				}
				if (iMaintSettings != null)
				{
					iMaintSettings.Period = Task.TimeSpanToString(value);
				}
				OnNotifyPropertyChanged("Period");
				return;
			}
			throw new NotSupportedPriorToException(TaskCompatibility.V2_2);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal MaintenanceSettings([CanBeNull] ITaskSettings3 iSettings3)
	{
		iSettings = iSettings3;
		if (iSettings3 != null)
		{
			iMaintSettings = iSettings.MaintenanceSettings;
		}
	}

	public void Dispose()
	{
		if (iMaintSettings != null)
		{
			Marshal.ReleaseComObject(iMaintSettings);
		}
	}

	public override string ToString()
	{
		if (iMaintSettings == null)
		{
			return base.ToString();
		}
		return DebugHelper.GetDebugString(this);
	}

	internal bool IsSet()
	{
		if (iMaintSettings != null)
		{
			if (iMaintSettings.Period == null && iMaintSettings.Deadline == null)
			{
				return iMaintSettings.Exclusive;
			}
			return true;
		}
		return false;
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
