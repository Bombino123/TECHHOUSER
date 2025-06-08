using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("Triggers", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[ComVisible(true)]
public sealed class TriggerCollection : IList<Trigger>, ICollection<Trigger>, IEnumerable<Trigger>, IEnumerable, IDisposable, IXmlSerializable, IList, ICollection, INotifyCollectionChanged, INotifyPropertyChanged
{
	private sealed class V1TriggerEnumerator : IEnumerator<Trigger>, IDisposable, IEnumerator
	{
		private short curItem = -1;

		private ITask iTask;

		public Trigger Current => Trigger.CreateTrigger(iTask.GetTrigger((ushort)curItem));

		object IEnumerator.Current => Current;

		internal V1TriggerEnumerator(ITask task)
		{
			iTask = task;
		}

		public void Dispose()
		{
			iTask = null;
		}

		public bool MoveNext()
		{
			return ++curItem < iTask.GetTriggerCount();
		}

		public void Reset()
		{
			curItem = -1;
		}
	}

	private const string IndexerName = "Item[]";

	private readonly ITriggerCollection v2Coll;

	private bool inV2set;

	private ITask v1Task;

	private ITaskDefinition v2Def;

	public int Count => v2Coll?.Count ?? v1Task.GetTriggerCount();

	bool IList.IsFixedSize => false;

	bool ICollection<Trigger>.IsReadOnly => false;

	bool IList.IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public Trigger this[[NotNull] string triggerId]
	{
		get
		{
			if (string.IsNullOrEmpty(triggerId))
			{
				throw new ArgumentNullException("triggerId");
			}
			using (IEnumerator<Trigger> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Trigger current = enumerator.Current;
					if (string.Equals(current.Id, triggerId))
					{
						return current;
					}
				}
			}
			throw new ArgumentOutOfRangeException("triggerId");
		}
		set
		{
			if (value == null)
			{
				throw new NullReferenceException();
			}
			if (string.IsNullOrEmpty(triggerId))
			{
				throw new ArgumentNullException("triggerId");
			}
			if (triggerId != value.Id)
			{
				throw new InvalidOperationException("Mismatching Id for trigger and lookup.");
			}
			int num = IndexOf(triggerId);
			if (num >= 0)
			{
				object oldItem = this[num].Clone();
				inV2set = true;
				try
				{
					RemoveAt(num);
					Insert(num, value);
				}
				finally
				{
					inV2set = true;
				}
				OnNotifyPropertyChanged("Item[]");
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, num));
			}
			else
			{
				Add(value);
			}
		}
	}

	public Trigger this[int index]
	{
		get
		{
			if (v2Coll != null)
			{
				return Trigger.CreateTrigger(v2Coll[++index], v2Def);
			}
			return Trigger.CreateTrigger(v1Task.GetTrigger((ushort)index));
		}
		set
		{
			if (index < 0 || Count <= index)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index is not a valid index in the TriggerCollection");
			}
			object oldItem = this[index].Clone();
			inV2set = true;
			try
			{
				Insert(index, value);
				RemoveAt(index + 1);
			}
			finally
			{
				inV2set = false;
			}
			OnNotifyPropertyChanged("Item[]");
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
		}
	}

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (Trigger)value;
		}
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public event PropertyChangedEventHandler PropertyChanged;

	internal TriggerCollection([NotNull] ITask iTask)
	{
		v1Task = iTask;
	}

	internal TriggerCollection([NotNull] ITaskDefinition iTaskDef)
	{
		v2Def = iTaskDef;
		v2Coll = v2Def.Triggers;
	}

	public TTrigger Add<TTrigger>([NotNull] TTrigger unboundTrigger) where TTrigger : Trigger
	{
		if (unboundTrigger == null)
		{
			throw new ArgumentNullException("unboundTrigger");
		}
		if (v2Def != null)
		{
			unboundTrigger.Bind(v2Def);
		}
		else
		{
			unboundTrigger.Bind(v1Task);
		}
		OnNotifyPropertyChanged("Count");
		OnNotifyPropertyChanged("Item[]");
		this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, unboundTrigger));
		return unboundTrigger;
	}

	public Trigger AddNew(TaskTriggerType taskTriggerType)
	{
		ushort NewTriggerIndex;
		if (v1Task != null)
		{
			return Trigger.CreateTrigger(v1Task.CreateTrigger(out NewTriggerIndex), Trigger.ConvertToV1TriggerType(taskTriggerType));
		}
		return Trigger.CreateTrigger(v2Coll.Create(taskTriggerType), v2Def);
	}

	public void AddRange([NotNull] IEnumerable<Trigger> triggers)
	{
		if (triggers == null)
		{
			throw new ArgumentNullException("triggers");
		}
		foreach (Trigger trigger in triggers)
		{
			Add(trigger);
		}
	}

	public void Clear()
	{
		if (v2Coll != null)
		{
			v2Coll.Clear();
		}
		else
		{
			inV2set = true;
			try
			{
				for (int num = Count - 1; num >= 0; num--)
				{
					RemoveAt(num);
				}
			}
			finally
			{
				inV2set = false;
			}
		}
		OnNotifyPropertyChanged("Count");
		OnNotifyPropertyChanged("Item[]");
		this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public bool Contains([NotNull] Trigger item)
	{
		return Find((Trigger a) => a.Equals(item)) != null;
	}

	public bool ContainsType(Type triggerType)
	{
		return Find((Trigger a) => a.GetType() == triggerType) != null;
	}

	public void CopyTo(Trigger[] array, int arrayIndex)
	{
		CopyTo(0, array, arrayIndex, Count);
	}

	public void CopyTo(int index, Trigger[] array, int arrayIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (count < 0 || count > Count - index)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (Count - index > array.Length - arrayIndex)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		for (int i = 0; i < count; i++)
		{
			array[arrayIndex + i] = (Trigger)this[index + i].Clone();
		}
	}

	public void Dispose()
	{
		if (v2Coll != null)
		{
			Marshal.ReleaseComObject(v2Coll);
		}
		v2Def = null;
		v1Task = null;
	}

	public Trigger Find([NotNull] Predicate<Trigger> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		using (IEnumerator<Trigger> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Trigger current = enumerator.Current;
				if (match(current))
				{
					return current;
				}
			}
		}
		return null;
	}

	public int FindIndexOf(int startIndex, int count, [NotNull] Predicate<Trigger> match)
	{
		if (startIndex < 0 || startIndex >= Count)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (startIndex + count > Count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			if (match(this[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public int FindIndexOf([NotNull] Predicate<Trigger> match)
	{
		return FindIndexOf(0, Count, match);
	}

	public IEnumerator<Trigger> GetEnumerator()
	{
		if (v1Task != null)
		{
			return new V1TriggerEnumerator(v1Task);
		}
		return new ComEnumerator<Trigger, ITrigger>(() => v2Coll.Count, (int i) => v2Coll[i], (ITrigger o) => Trigger.CreateTrigger(o, v2Def));
	}

	public int IndexOf([NotNull] Trigger item)
	{
		return FindIndexOf((Trigger a) => a.Equals(item));
	}

	public int IndexOf([NotNull] string triggerId)
	{
		if (string.IsNullOrEmpty(triggerId))
		{
			throw new ArgumentNullException(triggerId);
		}
		return FindIndexOf((Trigger a) => string.Equals(a.Id, triggerId));
	}

	public void Insert(int index, [NotNull] Trigger trigger)
	{
		if (trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		if (index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		Trigger[] array = new Trigger[Count - index];
		CopyTo(index, array, 0, Count - index);
		for (int num = Count - 1; num >= index; num--)
		{
			RemoveAt(num);
		}
		Add(trigger);
		Trigger[] array2 = array;
		foreach (Trigger unboundTrigger in array2)
		{
			Add(unboundTrigger);
		}
	}

	public bool Remove([NotNull] Trigger item)
	{
		int num = IndexOf(item);
		if (num != -1)
		{
			try
			{
				RemoveAt(num);
				return true;
			}
			catch
			{
			}
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", index, "Failed to remove Trigger. Index out of range.");
		}
		object changedItem = this[index].Clone();
		if (v2Coll != null)
		{
			v2Coll.Remove(++index);
		}
		else
		{
			v1Task.DeleteTrigger((ushort)index);
		}
		if (!inV2set)
		{
			OnNotifyPropertyChanged("Count");
			OnNotifyPropertyChanged("Item[]");
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem, index));
		}
	}

	public Trigger[] ToArray()
	{
		Trigger[] array = new Trigger[Count];
		CopyTo(array, 0);
		return array;
	}

	public override string ToString()
	{
		if (Count == 1)
		{
			return this[0].ToString();
		}
		if (Count > 1)
		{
			return Resources.MultipleTriggers;
		}
		return string.Empty;
	}

	void ICollection<Trigger>.Add(Trigger item)
	{
		Add(item);
	}

	int IList.Add(object value)
	{
		Add((Trigger)value);
		return Count - 1;
	}

	bool IList.Contains(object value)
	{
		return Contains((Trigger)value);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array != null && array.Rank != 1)
		{
			throw new RankException("Multi-dimensional arrays are not supported.");
		}
		Trigger[] array2 = new Trigger[Count];
		CopyTo(array2, 0);
		Array.Copy(array2, 0, array, index, Count);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	int IList.IndexOf(object value)
	{
		return IndexOf((Trigger)value);
	}

	void IList.Insert(int index, object value)
	{
		Insert(index, (Trigger)value);
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		reader.ReadStartElement(XmlSerializationHelper.GetElementName(this), "http://schemas.microsoft.com/windows/2004/02/mit/task");
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			switch (reader.LocalName)
			{
			case "BootTrigger":
				XmlSerializationHelper.ReadObject(reader, AddNew(TaskTriggerType.Boot));
				break;
			case "IdleTrigger":
				XmlSerializationHelper.ReadObject(reader, AddNew(TaskTriggerType.Idle));
				break;
			case "TimeTrigger":
				XmlSerializationHelper.ReadObject(reader, AddNew(TaskTriggerType.Time));
				break;
			case "LogonTrigger":
				XmlSerializationHelper.ReadObject(reader, AddNew(TaskTriggerType.Logon));
				break;
			case "CalendarTrigger":
				Add(CalendarTrigger.GetTriggerFromXml(reader));
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.ReadEndElement();
	}

	void IList.Remove(object value)
	{
		Remove((Trigger)value);
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		using IEnumerator<Trigger> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Trigger current = enumerator.Current;
			XmlSerializationHelper.WriteObject(writer, current);
		}
	}

	internal void Bind()
	{
		using IEnumerator<Trigger> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.SetV1TriggerData();
		}
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
