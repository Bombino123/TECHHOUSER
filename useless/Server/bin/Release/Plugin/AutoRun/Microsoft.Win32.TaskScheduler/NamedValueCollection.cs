using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public sealed class NamedValueCollection : IDisposable, ICollection<NameValuePair>, IEnumerable<NameValuePair>, IEnumerable, IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, INotifyCollectionChanged, INotifyPropertyChanged
{
	private ITaskNamedValueCollection v2Coll;

	private readonly List<NameValuePair> unboundDict;

	[XmlIgnore]
	internal bool AttributedXmlFormat { get; set; } = true;


	public int Count => v2Coll?.Count ?? unboundDict.Count;

	[ItemNotNull]
	[NotNull]
	public ICollection<string> Names
	{
		get
		{
			if (v2Coll == null)
			{
				return unboundDict.ConvertAll((NameValuePair p) => p.Name);
			}
			List<string> list = new List<string>(v2Coll.Count);
			using IEnumerator<NameValuePair> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				NameValuePair current = enumerator.Current;
				list.Add(current.Name);
			}
			return list;
		}
	}

	[ItemNotNull]
	[NotNull]
	public ICollection<string> Values
	{
		get
		{
			if (v2Coll == null)
			{
				return unboundDict.ConvertAll((NameValuePair p) => p.Value);
			}
			List<string> list = new List<string>(v2Coll.Count);
			using IEnumerator<NameValuePair> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				NameValuePair current = enumerator.Current;
				list.Add(current.Value);
			}
			return list;
		}
	}

	[NotNull]
	public string this[int index]
	{
		get
		{
			if (v2Coll != null)
			{
				return v2Coll[++index].Value;
			}
			return unboundDict[index].Value;
		}
	}

	public string this[string name]
	{
		[CanBeNull]
		get
		{
			TryGetValue(name, out var value);
			return value;
		}
		[NotNull]
		set
		{
			NameValuePair oldItem = null;
			NameValuePair nameValuePair = new NameValuePair(name, value);
			int num;
			if (v2Coll == null)
			{
				num = unboundDict.FindIndex((NameValuePair p) => p.Name == name);
				if (num == -1)
				{
					unboundDict.Add(nameValuePair);
				}
				else
				{
					oldItem = unboundDict[num];
					unboundDict[num] = nameValuePair;
				}
			}
			else
			{
				KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[Count];
				((ICollection<KeyValuePair<string, string>>)this).CopyTo(array, 0);
				num = Array.FindIndex(array, (KeyValuePair<string, string> p) => p.Key == name);
				if (num == -1)
				{
					v2Coll.Create(name, value);
				}
				else
				{
					oldItem = array[num];
					array[num] = new KeyValuePair<string, string>(name, value);
					v2Coll.Clear();
					KeyValuePair<string, string>[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						KeyValuePair<string, string> keyValuePair = array2[i];
						v2Coll.Create(keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
			if (num == -1)
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, nameValuePair));
			}
			else
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, nameValuePair, oldItem, num));
			}
		}
	}

	bool ICollection<NameValuePair>.IsReadOnly => false;

	ICollection<string> IDictionary<string, string>.Keys => Names;

	bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public event PropertyChangedEventHandler PropertyChanged;

	internal NamedValueCollection([NotNull] ITaskNamedValueCollection iColl)
	{
		v2Coll = iColl;
	}

	internal NamedValueCollection()
	{
		unboundDict = new List<NameValuePair>(5);
	}

	internal void Bind([NotNull] ITaskNamedValueCollection iTaskNamedValueCollection)
	{
		v2Coll = iTaskNamedValueCollection;
		v2Coll.Clear();
		foreach (NameValuePair item in unboundDict)
		{
			v2Coll.Create(item.Name, item.Value);
		}
	}

	public void CopyTo([NotNull] NamedValueCollection destCollection)
	{
		if (v2Coll != null)
		{
			for (int i = 1; i <= Count; i++)
			{
				destCollection.Add(v2Coll[i].Name, v2Coll[i].Value);
			}
			return;
		}
		foreach (NameValuePair item in unboundDict)
		{
			destCollection.Add(item.Name, item.Value);
		}
	}

	public void Dispose()
	{
		if (v2Coll != null)
		{
			Marshal.ReleaseComObject(v2Coll);
		}
	}

	public void Add([NotNull] NameValuePair item)
	{
		if (v2Coll != null)
		{
			v2Coll.Create(item.Name, item.Value);
		}
		else
		{
			unboundDict.Add(item);
		}
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
	}

	public void Add([NotNull] string name, [NotNull] string value)
	{
		Add(new NameValuePair(name, value));
	}

	public void AddRange([ItemNotNull][NotNull] IEnumerable<NameValuePair> items)
	{
		if (v2Coll != null)
		{
			foreach (NameValuePair item in items)
			{
				v2Coll.Create(item.Name, item.Value);
			}
		}
		else
		{
			unboundDict.AddRange(items);
		}
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
	}

	public void Clear()
	{
		if (v2Coll != null)
		{
			v2Coll.Clear();
		}
		else
		{
			unboundDict.Clear();
		}
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public IEnumerator<NameValuePair> GetEnumerator()
	{
		if (v2Coll == null)
		{
			return unboundDict.GetEnumerator();
		}
		return new ComEnumerator<NameValuePair, ITaskNamedValuePair>(() => v2Coll.Count, (int i) => v2Coll[i], (ITaskNamedValuePair o) => new NameValuePair(o));
	}

	private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
		{
			foreach (NameValuePair newItem in e.NewItems)
			{
				newItem.AttributedXmlFormat = AttributedXmlFormat;
			}
		}
		this.CollectionChanged?.Invoke(this, e);
	}

	public bool Remove([NotNull] string name)
	{
		int i = -1;
		NameValuePair changedItem = null;
		try
		{
			if (v2Coll == null)
			{
				i = unboundDict.FindIndex((NameValuePair p) => p.Name == name);
				if (i != -1)
				{
					changedItem = unboundDict[i];
					unboundDict.RemoveAt(i);
				}
				return i != -1;
			}
			for (i = 0; i < v2Coll.Count; i++)
			{
				if (name == v2Coll[i].Name)
				{
					changedItem = new NameValuePair(v2Coll[i]).Clone();
					v2Coll.Remove(i);
					return true;
				}
			}
			i = -1;
		}
		finally
		{
			if (i != -1)
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem, i));
			}
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		NameValuePair changedItem;
		if (v2Coll != null)
		{
			changedItem = new NameValuePair(v2Coll[index]).Clone();
			v2Coll.Remove(index);
		}
		else
		{
			changedItem = unboundDict[index];
			unboundDict.RemoveAt(index);
		}
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem, index));
	}

	public bool TryGetValue(string name, out string value)
	{
		if (v2Coll != null)
		{
			using (IEnumerator<NameValuePair> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NameValuePair current = enumerator.Current;
					if (string.CompareOrdinal(current.Name, name) == 0)
					{
						value = current.Value;
						return true;
					}
				}
			}
			value = null;
			return false;
		}
		NameValuePair nameValuePair = unboundDict.Find((NameValuePair p) => p.Name == name);
		value = nameValuePair?.Value;
		return nameValuePair != null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	bool ICollection<NameValuePair>.Contains(NameValuePair item)
	{
		if (v2Coll == null)
		{
			return unboundDict.Contains(item);
		}
		using (IEnumerator<NameValuePair> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				NameValuePair current = enumerator.Current;
				if (object.Equals(item, current))
				{
					return true;
				}
			}
		}
		return false;
	}

	void ICollection<NameValuePair>.CopyTo(NameValuePair[] array, int arrayIndex)
	{
		if (v2Coll == null)
		{
			unboundDict.CopyTo(array, arrayIndex);
			return;
		}
		if (array.Length - arrayIndex < v2Coll.Count)
		{
			throw new ArgumentException("Items in collection exceed available items in destination array.");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentException("Array index must be 0 or greater.", "arrayIndex");
		}
		for (int i = 0; i < v2Coll.Count; i++)
		{
			array[i + arrayIndex] = new NameValuePair(v2Coll[i]);
		}
	}

	bool ICollection<NameValuePair>.Remove(NameValuePair item)
	{
		int i = -1;
		try
		{
			if (v2Coll == null)
			{
				if ((i = unboundDict.IndexOf(item)) != -1)
				{
					return unboundDict.Remove(item);
				}
			}
			else
			{
				for (i = 0; i < v2Coll.Count; i++)
				{
					if (item.Equals(v2Coll[i]))
					{
						v2Coll.Remove(i);
						return true;
					}
				}
			}
			i = -1;
		}
		finally
		{
			if (i != -1)
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, i));
			}
		}
		return false;
	}

	bool IDictionary<string, string>.ContainsKey(string key)
	{
		return Names.Contains(key);
	}

	void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
	{
		Add(item.Key, item.Value);
	}

	bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
	{
		return ((ICollection<NameValuePair>)this).Contains(new NameValuePair(item.Key, item.Value));
	}

	void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
	{
		if (array.Length < Count + arrayIndex)
		{
			throw new ArgumentOutOfRangeException("array", "Array has insufficient capacity to support copy.");
		}
		foreach (KeyValuePair<string, string> item in (IEnumerable<KeyValuePair<string, string>>)this)
		{
			array[arrayIndex++] = item;
		}
	}

	bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
	{
		return ((ICollection<NameValuePair>)this).Remove(new NameValuePair(item.Key, item.Value));
	}

	IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
	{
		using IEnumerator<NameValuePair> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			NameValuePair current = enumerator.Current;
			yield return new KeyValuePair<string, string>(current.Name, current.Value);
		}
	}
}
