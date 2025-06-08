using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public sealed class TaskCollection : IReadOnlyList<Task>, IReadOnlyCollection<Task>, IEnumerable<Task>, IEnumerable, IDisposable
{
	internal class V1TaskEnumerator : IEnumerator<Task>, IDisposable, IEnumerator
	{
		private readonly Regex filter;

		private readonly TaskService svc;

		private readonly IEnumWorkItems wienum;

		private string curItem;

		private ITaskScheduler ts;

		public Task Current => new Task(svc, ICurrent);

		object IEnumerator.Current => Current;

		internal int Count
		{
			get
			{
				int num = 0;
				Reset();
				while (MoveNext())
				{
					num++;
				}
				Reset();
				return num;
			}
		}

		internal ITask ICurrent => TaskService.GetTask(ts, curItem);

		internal V1TaskEnumerator(TaskService svc, Regex filter = null)
		{
			this.svc = svc;
			this.filter = filter;
			ts = svc.v1TaskScheduler;
			wienum = ts?.Enum();
			Reset();
		}

		public void Dispose()
		{
			if (wienum != null)
			{
				Marshal.ReleaseComObject(wienum);
			}
			ts = null;
		}

		public bool MoveNext()
		{
			IntPtr Names = IntPtr.Zero;
			bool flag = false;
			do
			{
				curItem = null;
				uint Fetched = 0u;
				try
				{
					wienum?.Next(1u, out Names, out Fetched);
					if (Fetched != 1)
					{
						break;
					}
					using (CoTaskMemString coTaskMemString = new CoTaskMemString(Marshal.ReadIntPtr(Names)))
					{
						curItem = coTaskMemString.ToString();
					}
					if (curItem != null && curItem.EndsWith(".job", StringComparison.InvariantCultureIgnoreCase))
					{
						curItem = curItem.Remove(curItem.Length - 4);
					}
					goto IL_00bb;
				}
				catch
				{
					goto IL_00bb;
				}
				finally
				{
					Marshal.FreeCoTaskMem(Names);
					Names = IntPtr.Zero;
				}
				IL_00bb:
				if (filter != null && curItem != null && !filter.IsMatch(curItem))
				{
					continue;
				}
				ITask o = null;
				try
				{
					o = ICurrent;
					flag = true;
				}
				catch
				{
					flag = false;
				}
				finally
				{
					Marshal.ReleaseComObject(o);
				}
			}
			while (!flag);
			return curItem != null;
		}

		public void Reset()
		{
			curItem = null;
			wienum?.Reset();
		}
	}

	private class V2TaskEnumerator : ComEnumerator<Task, IRegisteredTask>
	{
		private readonly Regex filter;

		internal V2TaskEnumerator(TaskFolder folder, IRegisteredTaskCollection iTaskColl, Regex filter = null)
			: base((Func<int>)(() => iTaskColl.Count), (Func<object, IRegisteredTask>)((object o) => iTaskColl[o]), (Func<IRegisteredTask, Task>)((IRegisteredTask o) => Task.CreateTask(folder.TaskService, o)))
		{
			this.filter = filter;
		}

		public override bool MoveNext()
		{
			bool flag;
			for (flag = base.MoveNext(); flag && filter != null && !filter.IsMatch(iEnum?.Current?.Name ?? ""); flag = base.MoveNext())
			{
			}
			return flag;
		}
	}

	private readonly TaskFolder fld;

	private readonly TaskService svc;

	private readonly IRegisteredTaskCollection v2Coll;

	private Regex filter;

	private ITaskScheduler v1TS;

	public int Count
	{
		get
		{
			int num = 0;
			if (v2Coll != null)
			{
				V2TaskEnumerator v2TaskEnumerator = new V2TaskEnumerator(fld, v2Coll, filter);
				while (v2TaskEnumerator.MoveNext())
				{
					num++;
				}
				return num;
			}
			return new V1TaskEnumerator(svc, filter).Count;
		}
	}

	private Regex Filter
	{
		get
		{
			return filter;
		}
		set
		{
			string text = value?.ToString().TrimStart(new char[1] { '^' }).TrimEnd(new char[1] { '$' }) ?? string.Empty;
			if (text == string.Empty || text == "*")
			{
				filter = null;
			}
			else if (value != null && value.ToString().TrimEnd(new char[1] { '$' }).EndsWith("\\.job", StringComparison.InvariantCultureIgnoreCase))
			{
				filter = new Regex(value.ToString().Replace("\\.job", ""));
			}
			else
			{
				filter = value;
			}
		}
	}

	public Task this[int index]
	{
		get
		{
			int num = 0;
			IEnumerator<Task> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (num++ == index)
				{
					return enumerator.Current;
				}
			}
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public Task this[string name]
	{
		get
		{
			if (v2Coll != null)
			{
				return Task.CreateTask(svc, v2Coll[name]);
			}
			Task task = svc.GetTask(name);
			if (task != null)
			{
				return task;
			}
			throw new ArgumentOutOfRangeException("name");
		}
	}

	internal TaskCollection([NotNull] TaskService svc, Regex filter = null)
	{
		this.svc = svc;
		Filter = filter;
		v1TS = svc.v1TaskScheduler;
	}

	internal TaskCollection([NotNull] TaskFolder folder, [NotNull] IRegisteredTaskCollection iTaskColl, Regex filter = null)
	{
		svc = folder.TaskService;
		Filter = filter;
		fld = folder;
		v2Coll = iTaskColl;
	}

	public void Dispose()
	{
		v1TS = null;
		if (v2Coll != null)
		{
			Marshal.ReleaseComObject(v2Coll);
		}
	}

	public bool Exists([NotNull] string taskName)
	{
		try
		{
			if (v2Coll != null)
			{
				return v2Coll[taskName] != null;
			}
			return svc.GetTask(taskName) != null;
		}
		catch
		{
		}
		return false;
	}

	public IEnumerator<Task> GetEnumerator()
	{
		if (v1TS != null)
		{
			return new V1TaskEnumerator(svc, filter);
		}
		return new V2TaskEnumerator(fld, v2Coll, filter);
	}

	public override string ToString()
	{
		return $"TaskCollection; Count: {Count}";
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
