using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[ComVisible(true)]
public sealed class TaskFolderCollection : ICollection<TaskFolder>, IEnumerable<TaskFolder>, IEnumerable, IDisposable, INotifyCollectionChanged, INotifyPropertyChanged
{
	private const string IndexerName = "Item[]";

	private readonly TaskFolder parent;

	private readonly TaskFolder[] v1FolderList;

	private readonly ITaskFolderCollection v2FolderList;

	public int Count => v2FolderList?.Count ?? v1FolderList.Length;

	bool ICollection<TaskFolder>.IsReadOnly => false;

	public TaskFolder this[int index]
	{
		get
		{
			if (v2FolderList != null)
			{
				return new TaskFolder(parent.TaskService, v2FolderList[++index]);
			}
			return v1FolderList[index];
		}
	}

	public TaskFolder this[[NotNull] string path]
	{
		get
		{
			try
			{
				if (v2FolderList != null)
				{
					return parent.GetFolder(path);
				}
				if (v1FolderList != null && v1FolderList.Length != 0 && (path == string.Empty || path == "\\"))
				{
					return v1FolderList[0];
				}
			}
			catch
			{
			}
			throw new ArgumentException("Path not found", "path");
		}
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public event PropertyChangedEventHandler PropertyChanged;

	internal TaskFolderCollection()
	{
		v1FolderList = new TaskFolder[0];
	}

	internal TaskFolderCollection([NotNull] TaskFolder folder, [NotNull] ITaskFolderCollection iCollection)
	{
		parent = folder;
		v2FolderList = iCollection;
	}

	public void Add([NotNull] TaskFolder item)
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		if (v2FolderList != null)
		{
			for (int num = v2FolderList.Count; num > 0; num--)
			{
				parent.DeleteFolder(v2FolderList[num].Name, exceptionOnNotExists: false);
			}
			OnNotifyPropertyChanged("Count");
			OnNotifyPropertyChanged("Item[]");
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}

	public bool Contains([NotNull] TaskFolder item)
	{
		if (v2FolderList != null)
		{
			for (int num = v2FolderList.Count; num > 0; num--)
			{
				if (string.Equals(item.Path, v2FolderList[num].Path, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
		return item.Path == "\\";
	}

	public void CopyTo(TaskFolder[] array, int arrayIndex)
	{
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (v2FolderList != null)
		{
			if (arrayIndex + Count > array.Length)
			{
				throw new ArgumentException();
			}
			using IEnumerator<TaskFolder> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				TaskFolder current = enumerator.Current;
				array[arrayIndex++] = current;
			}
			return;
		}
		if (arrayIndex + v1FolderList.Length > array.Length)
		{
			throw new ArgumentException();
		}
		v1FolderList.CopyTo(array, arrayIndex);
	}

	public void Dispose()
	{
		if (v1FolderList != null && v1FolderList.Length != 0)
		{
			v1FolderList[0].Dispose();
			v1FolderList[0] = null;
		}
		if (v2FolderList != null)
		{
			Marshal.ReleaseComObject(v2FolderList);
		}
	}

	public bool Exists([NotNull] string path)
	{
		try
		{
			parent.GetFolder(path);
			return true;
		}
		catch
		{
		}
		return false;
	}

	public IEnumerator<TaskFolder> GetEnumerator()
	{
		if (v2FolderList != null)
		{
			return new ComEnumerator<TaskFolder, ITaskFolder>(() => v2FolderList.Count, (object o) => v2FolderList[o], (ITaskFolder o) => new TaskFolder(parent.TaskService, o));
		}
		return Array.AsReadOnly(v1FolderList).GetEnumerator();
	}

	public bool Remove([NotNull] TaskFolder item)
	{
		if (v2FolderList != null)
		{
			for (int num = v2FolderList.Count; num > 0; num--)
			{
				if (string.Equals(item.Path, v2FolderList[num].Path, StringComparison.CurrentCultureIgnoreCase))
				{
					try
					{
						parent.DeleteFolder(v2FolderList[num].Name);
						OnNotifyPropertyChanged("Count");
						OnNotifyPropertyChanged("Item[]");
						this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, num));
					}
					catch
					{
						return false;
					}
					return true;
				}
			}
		}
		return false;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
