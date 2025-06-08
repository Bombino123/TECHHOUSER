using System;
using System.Collections.Generic;

namespace SharpDX;

public class DisposeCollector : DisposeBase
{
	private List<object> disposables;

	public int Count => disposables.Count;

	public void DisposeAndClear(bool disposeManagedResources = true)
	{
		if (disposables == null)
		{
			return;
		}
		for (int num = disposables.Count - 1; num >= 0; num--)
		{
			object obj = disposables[num];
			if (obj is IDisposable disposable)
			{
				if (disposeManagedResources)
				{
					disposable.Dispose();
				}
			}
			else
			{
				Utilities.FreeMemory((IntPtr)obj);
			}
			disposables.RemoveAt(num);
		}
		disposables.Clear();
	}

	protected override void Dispose(bool disposeManagedResources)
	{
		DisposeAndClear(disposeManagedResources);
		disposables = null;
	}

	public T Collect<T>(T toDispose)
	{
		if (!(toDispose is IDisposable) && !(toDispose is IntPtr))
		{
			throw new ArgumentException("Argument must be IDisposable or IntPtr");
		}
		if (toDispose is IntPtr && !Utilities.IsMemoryAligned((IntPtr)(object)toDispose))
		{
			throw new ArgumentException("Memory pointer is invalid. Memory must have been allocated with Utilties.AllocateMemory");
		}
		if (!object.Equals(toDispose, default(T)))
		{
			if (disposables == null)
			{
				disposables = new List<object>();
			}
			if (!disposables.Contains(toDispose))
			{
				disposables.Add(toDispose);
			}
		}
		return toDispose;
	}

	public void RemoveAndDispose<T>(ref T objectToDispose)
	{
		if (disposables != null)
		{
			Remove(objectToDispose);
			if (objectToDispose is IDisposable disposable)
			{
				disposable.Dispose();
			}
			else
			{
				Utilities.FreeMemory((IntPtr)(object)objectToDispose);
			}
			objectToDispose = default(T);
		}
	}

	public void Remove<T>(T toDisposeArg)
	{
		if (disposables != null && disposables.Contains(toDisposeArg))
		{
			disposables.Remove(toDisposeArg);
		}
	}
}
