using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public class TaskEventArgs : EventArgs
{
	private readonly TaskService taskService;

	public Task Task
	{
		get
		{
			try
			{
				return taskService?.GetTask(TaskPath);
			}
			catch
			{
				return null;
			}
		}
	}

	[NotNull]
	public TaskEvent TaskEvent { get; }

	public string TaskName => Path.GetFileName(TaskPath);

	public string TaskPath { get; }

	internal TaskEventArgs([NotNull] TaskEvent evt, TaskService ts = null)
	{
		TaskEvent = evt;
		TaskPath = evt.TaskPath;
		taskService = ts;
	}
}
