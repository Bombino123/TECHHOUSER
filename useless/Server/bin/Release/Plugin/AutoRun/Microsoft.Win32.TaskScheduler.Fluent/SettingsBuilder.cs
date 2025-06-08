using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.TaskScheduler.Fluent;

[ComVisible(true)]
public class SettingsBuilder : BaseBuilder
{
	internal SettingsBuilder(BuilderInfo taskBuilder)
		: base(taskBuilder)
	{
	}

	public SettingsBuilder AllowingStartIfOnBatteries()
	{
		base.TaskDef.Settings.DisallowStartIfOnBatteries = false;
		return this;
	}

	public SettingsBuilder AllowingStartOnRemoteAppSession()
	{
		base.TaskDef.Settings.DisallowStartOnRemoteAppSession = false;
		return this;
	}

	public SettingsBuilder DataIs(string data)
	{
		base.TaskDef.Data = data;
		return this;
	}

	public SettingsBuilder DeletingTaskAfter(TimeSpan duration)
	{
		base.TaskDef.Settings.DeleteExpiredTaskAfter = duration;
		return this;
	}

	public SettingsBuilder DisallowingDemandStart()
	{
		base.TaskDef.Settings.AllowDemandStart = false;
		return this;
	}

	public SettingsBuilder DisallowingHardTerminate()
	{
		base.TaskDef.Settings.AllowHardTerminate = false;
		return this;
	}

	public SettingsBuilder ExecutingAtMost(TimeSpan duration)
	{
		base.TaskDef.Settings.ExecutionTimeLimit = duration;
		return this;
	}

	public SettingsBuilder InstancesAre(TaskInstancesPolicy policy)
	{
		base.TaskDef.Settings.MultipleInstances = policy;
		return this;
	}

	public SettingsBuilder NotStoppingIfGoingOnBatteries()
	{
		base.TaskDef.Settings.StopIfGoingOnBatteries = true;
		return this;
	}

	public SettingsBuilder OnlyIfIdle()
	{
		base.TaskDef.Settings.RunOnlyIfIdle = true;
		return this;
	}

	public SettingsBuilder OnlyIfNetworkAvailable()
	{
		base.TaskDef.Settings.RunOnlyIfNetworkAvailable = true;
		return this;
	}

	public SettingsBuilder PriorityIs(ProcessPriorityClass priority)
	{
		base.TaskDef.Settings.Priority = priority;
		return this;
	}

	public SettingsBuilder RestartingEvery(TimeSpan interval)
	{
		base.TaskDef.Settings.RestartInterval = interval;
		return this;
	}

	public SettingsBuilder StartingWhenAvailable()
	{
		base.TaskDef.Settings.StartWhenAvailable = true;
		return this;
	}

	public SettingsBuilder WakingToRun()
	{
		base.TaskDef.Settings.WakeToRun = true;
		return this;
	}
}
