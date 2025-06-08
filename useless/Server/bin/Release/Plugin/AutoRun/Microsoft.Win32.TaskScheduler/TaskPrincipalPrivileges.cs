using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public sealed class TaskPrincipalPrivileges : IList<TaskPrincipalPrivilege>, ICollection<TaskPrincipalPrivilege>, IEnumerable<TaskPrincipalPrivilege>, IEnumerable
{
	public sealed class TaskPrincipalPrivilegesEnumerator : IEnumerator<TaskPrincipalPrivilege>, IDisposable, IEnumerator
	{
		private readonly IPrincipal2 v2Principal2;

		private int cur;

		public TaskPrincipalPrivilege Current { get; private set; }

		object IEnumerator.Current => Current;

		internal TaskPrincipalPrivilegesEnumerator(IPrincipal2 iPrincipal2 = null)
		{
			v2Principal2 = iPrincipal2;
			Reset();
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (v2Principal2 != null && cur < v2Principal2.RequiredPrivilegeCount)
			{
				cur++;
				Current = (TaskPrincipalPrivilege)Enum.Parse(typeof(TaskPrincipalPrivilege), v2Principal2[cur]);
				return true;
			}
			Current = (TaskPrincipalPrivilege)0;
			return false;
		}

		public void Reset()
		{
			cur = 0;
			Current = (TaskPrincipalPrivilege)0;
		}
	}

	private readonly IPrincipal2 v2Principal2;

	public int Count => v2Principal2?.RequiredPrivilegeCount ?? 0;

	public bool IsReadOnly => false;

	public TaskPrincipalPrivilege this[int index]
	{
		get
		{
			if (v2Principal2 != null)
			{
				return (TaskPrincipalPrivilege)Enum.Parse(typeof(TaskPrincipalPrivilege), v2Principal2[index + 1]);
			}
			throw new IndexOutOfRangeException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	internal TaskPrincipalPrivileges(IPrincipal2 iPrincipal2 = null)
	{
		v2Principal2 = iPrincipal2;
	}

	public void Add(TaskPrincipalPrivilege item)
	{
		if (v2Principal2 != null)
		{
			v2Principal2.AddRequiredPrivilege(item.ToString());
			return;
		}
		throw new NotSupportedPriorToException(TaskCompatibility.V2_1);
	}

	public bool Contains(TaskPrincipalPrivilege item)
	{
		return IndexOf(item) != -1;
	}

	public void CopyTo(TaskPrincipalPrivilege[] array, int arrayIndex)
	{
		using IEnumerator<TaskPrincipalPrivilege> enumerator = GetEnumerator();
		for (int i = arrayIndex; i < array.Length; i++)
		{
			if (!enumerator.MoveNext())
			{
				break;
			}
			array[i] = enumerator.Current;
		}
	}

	public IEnumerator<TaskPrincipalPrivilege> GetEnumerator()
	{
		return new TaskPrincipalPrivilegesEnumerator(v2Principal2);
	}

	public int IndexOf(TaskPrincipalPrivilege item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (item == this[i])
			{
				return i;
			}
		}
		return -1;
	}

	void ICollection<TaskPrincipalPrivilege>.Clear()
	{
		throw new NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void IList<TaskPrincipalPrivilege>.Insert(int index, TaskPrincipalPrivilege item)
	{
		throw new NotImplementedException();
	}

	bool ICollection<TaskPrincipalPrivilege>.Remove(TaskPrincipalPrivilege item)
	{
		throw new NotImplementedException();
	}

	void IList<TaskPrincipalPrivilege>.RemoveAt(int index)
	{
		throw new NotImplementedException();
	}
}
