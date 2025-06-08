using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class RunningTaskCollection : IReadOnlyList<RunningTask>, IReadOnlyCollection<RunningTask>, IEnumerable<RunningTask>, IEnumerable, IDisposable
{
	private class V1RunningTaskEnumerator : IEnumerator<RunningTask>, IDisposable, IEnumerator
	{
		private readonly TaskService svc;

		private readonly TaskCollection.V1TaskEnumerator tEnum;

		public RunningTask Current => new RunningTask(svc, tEnum.ICurrent);

		object IEnumerator.Current => Current;

		internal V1RunningTaskEnumerator([NotNull] TaskService svc)
		{
			this.svc = svc;
			tEnum = new TaskCollection.V1TaskEnumerator(svc);
		}

		public void Dispose()
		{
			tEnum.Dispose();
		}

		public bool MoveNext()
		{
			if (tEnum.MoveNext())
			{
				Task current = tEnum.Current;
				if (current == null || current.State != TaskState.Running)
				{
					return MoveNext();
				}
				return true;
			}
			return false;
		}

		public void Reset()
		{
			tEnum.Reset();
		}
	}

	private readonly TaskService svc;

	private readonly IRunningTaskCollection v2Coll;

	public int Count
	{
		get
		{
			if (v2Coll != null)
			{
				return v2Coll.Count;
			}
			int num = 0;
			V1RunningTaskEnumerator v1RunningTaskEnumerator = new V1RunningTaskEnumerator(svc);
			while (v1RunningTaskEnumerator.MoveNext())
			{
				num++;
			}
			return num;
		}
	}

	public RunningTask this[int index]
	{
		get
		{
			if (v2Coll != null)
			{
				IRunningTask runningTask = v2Coll[++index];
				return new RunningTask(svc, TaskService.GetTask(svc.v2TaskService, runningTask.Path), runningTask);
			}
			int num = 0;
			V1RunningTaskEnumerator v1RunningTaskEnumerator = new V1RunningTaskEnumerator(svc);
			while (v1RunningTaskEnumerator.MoveNext())
			{
				if (num++ == index)
				{
					return v1RunningTaskEnumerator.Current;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	internal RunningTaskCollection([NotNull] TaskService svc)
	{
		this.svc = svc;
	}

	internal RunningTaskCollection([NotNull] TaskService svc, [NotNull] IRunningTaskCollection iTaskColl)
	{
		this.svc = svc;
		v2Coll = iTaskColl;
	}

	public void Dispose()
	{
		if (v2Coll != null)
		{
			Marshal.ReleaseComObject(v2Coll);
		}
	}

	public IEnumerator<RunningTask> GetEnumerator()
	{
		if (v2Coll != null)
		{
			return new ComEnumerator<RunningTask, IRunningTask>(() => v2Coll.Count, (object o) => v2Coll[o], delegate(IRunningTask o)
			{
				IRegisteredTask registeredTask = null;
				try
				{
					registeredTask = TaskService.GetTask(svc.v2TaskService, o.Path);
				}
				catch
				{
				}
				return (registeredTask != null) ? new RunningTask(svc, registeredTask, o) : null;
			});
		}
		return new V1RunningTaskEnumerator(svc);
	}

	public override string ToString()
	{
		return $"RunningTaskCollection; Count: {Count}";
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
