using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = false)]
[PublicAPI]
[ComVisible(true)]
public sealed class RunningTask : Task
{
	private readonly IRunningTask v2RunningTask;

	public uint EnginePID
	{
		get
		{
			if (v2RunningTask != null)
			{
				return v2RunningTask.EnginePID;
			}
			throw new NotV1SupportedException();
		}
	}

	public string CurrentAction
	{
		get
		{
			if (v2RunningTask == null)
			{
				return v1Task.GetApplicationName();
			}
			return v2RunningTask.CurrentAction;
		}
	}

	public Guid InstanceGuid
	{
		get
		{
			if (v2RunningTask == null)
			{
				return Guid.Empty;
			}
			return new Guid(v2RunningTask.InstanceGuid);
		}
	}

	public override TaskState State => v2RunningTask?.State ?? base.State;

	internal RunningTask([NotNull] TaskService svc, [NotNull] IRegisteredTask iTask, [NotNull] IRunningTask iRunningTask)
		: base(svc, iTask)
	{
		v2RunningTask = iRunningTask;
	}

	internal RunningTask([NotNull] TaskService svc, [NotNull] ITask iTask)
		: base(svc, iTask)
	{
	}

	public new void Dispose()
	{
		base.Dispose();
		if (v2RunningTask != null)
		{
			Marshal.ReleaseComObject(v2RunningTask);
		}
	}

	public void Refresh()
	{
		try
		{
			v2RunningTask?.Refresh();
		}
		catch (COMException ex) when (ex.ErrorCode == -2147216629)
		{
			throw new InvalidOperationException("The current task is no longer running.", ex);
		}
	}
}
