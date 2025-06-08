using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public sealed class IdleSettings : IDisposable, INotifyPropertyChanged
{
	private readonly IIdleSettings v2Settings;

	private ITask v1Task;

	[DefaultValue(typeof(TimeSpan), "00:10:00")]
	[XmlElement("Duration")]
	public TimeSpan IdleDuration
	{
		get
		{
			if (v2Settings != null)
			{
				return Task.StringToTimeSpan(v2Settings.IdleDuration);
			}
			v1Task.GetIdleWait(out var _, out var DeadlineMinutes);
			return TimeSpan.FromMinutes((int)DeadlineMinutes);
		}
		set
		{
			if (v2Settings != null)
			{
				if (value != TimeSpan.Zero && value < TimeSpan.FromMinutes(1.0))
				{
					throw new ArgumentOutOfRangeException("IdleDuration");
				}
				v2Settings.IdleDuration = Task.TimeSpanToString(value);
			}
			else
			{
				v1Task.SetIdleWait((ushort)WaitTimeout.TotalMinutes, (ushort)value.TotalMinutes);
			}
			OnNotifyPropertyChanged("IdleDuration");
		}
	}

	[DefaultValue(false)]
	public bool RestartOnIdle
	{
		get
		{
			return v2Settings?.RestartOnIdle ?? v1Task.HasFlags(TaskFlags.RestartOnIdleResume);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.RestartOnIdle = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.RestartOnIdleResume, value);
			}
			OnNotifyPropertyChanged("RestartOnIdle");
		}
	}

	[DefaultValue(true)]
	public bool StopOnIdleEnd
	{
		get
		{
			return v2Settings?.StopOnIdleEnd ?? v1Task.HasFlags(TaskFlags.KillOnIdleEnd);
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.StopOnIdleEnd = value;
			}
			else
			{
				v1Task.SetFlags(TaskFlags.KillOnIdleEnd, value);
			}
			OnNotifyPropertyChanged("StopOnIdleEnd");
		}
	}

	[DefaultValue(typeof(TimeSpan), "01:00:00")]
	public TimeSpan WaitTimeout
	{
		get
		{
			if (v2Settings != null)
			{
				return Task.StringToTimeSpan(v2Settings.WaitTimeout);
			}
			v1Task.GetIdleWait(out var IdleMinutes, out var _);
			return TimeSpan.FromMinutes((int)IdleMinutes);
		}
		set
		{
			if (v2Settings != null)
			{
				if (value != TimeSpan.Zero && value < TimeSpan.FromMinutes(1.0))
				{
					throw new ArgumentOutOfRangeException("WaitTimeout");
				}
				v2Settings.WaitTimeout = ((value == TimeSpan.Zero) ? "PT0S" : Task.TimeSpanToString(value));
			}
			else
			{
				v1Task.SetIdleWait((ushort)value.TotalMinutes, (ushort)IdleDuration.TotalMinutes);
			}
			OnNotifyPropertyChanged("WaitTimeout");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal IdleSettings([NotNull] IIdleSettings iSettings)
	{
		v2Settings = iSettings;
	}

	internal IdleSettings([NotNull] ITask iTask)
	{
		v1Task = iTask;
	}

	public void Dispose()
	{
		if (v2Settings != null)
		{
			Marshal.ReleaseComObject(v2Settings);
		}
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

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
