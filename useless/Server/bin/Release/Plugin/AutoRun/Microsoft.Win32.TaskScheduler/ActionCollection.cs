using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Properties;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("Actions", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[PublicAPI]
[ComVisible(true)]
public sealed class ActionCollection : IList<Action>, ICollection<Action>, IEnumerable<Action>, IEnumerable, IDisposable, IXmlSerializable, IList, ICollection, INotifyCollectionChanged, INotifyPropertyChanged
{
	internal const int MaxActions = 32;

	private const string IndexerName = "Item[]";

	private static readonly string psV2IdRegex = "(?:; )?PowerShellConversion=(?<v>0|1)";

	private bool inV2set;

	private PowerShellActionPlatformOption psConvert = PowerShellActionPlatformOption.Version2;

	private readonly List<Action> v1Actions;

	private ITask v1Task;

	private readonly IActionCollection v2Coll;

	private ITaskDefinition v2Def;

	[XmlAttribute]
	[DefaultValue(null)]
	[CanBeNull]
	public string Context
	{
		get
		{
			if (v2Coll != null)
			{
				return v2Coll.Context;
			}
			return v1Task.GetDataItem("ActionCollectionContext");
		}
		set
		{
			if (v2Coll != null)
			{
				v2Coll.Context = value;
			}
			else
			{
				v1Task.SetDataItem("ActionCollectionContext", value);
			}
			OnNotifyPropertyChanged("Context");
		}
	}

	[DefaultValue(typeof(PowerShellActionPlatformOption), "Version2")]
	public PowerShellActionPlatformOption PowerShellConversion
	{
		get
		{
			return psConvert;
		}
		set
		{
			if (psConvert == value)
			{
				return;
			}
			psConvert = value;
			if (v1Task != null)
			{
				v1Task.SetDataItem("PowerShellConversion", value.ToString());
			}
			if (v2Def != null)
			{
				if (!string.IsNullOrEmpty(v2Def.Data))
				{
					v2Def.Data = Regex.Replace(v2Def.Data, psV2IdRegex, "");
				}
				if (!SupportV2Conversion)
				{
					v2Def.Data = string.Format("{0}; {1}=0", v2Def.Data, "PowerShellConversion");
				}
			}
			OnNotifyPropertyChanged("PowerShellConversion");
		}
	}

	public string XmlText
	{
		get
		{
			if (v2Coll != null)
			{
				return v2Coll.XmlText;
			}
			return XmlSerializationHelper.WriteObjectToXmlText(this);
		}
		set
		{
			if (v2Coll != null)
			{
				v2Coll.XmlText = value;
			}
			else
			{
				XmlSerializationHelper.ReadObjectFromXmlText(value, this);
			}
			OnNotifyPropertyChanged("XmlText");
		}
	}

	public int Count
	{
		get
		{
			if (v2Coll == null)
			{
				return v1Actions.Count;
			}
			return v2Coll.Count;
		}
	}

	bool IList.IsFixedSize => false;

	bool ICollection<Action>.IsReadOnly => false;

	bool IList.IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	private bool SupportV1Conversion => (PowerShellConversion & PowerShellActionPlatformOption.Version1) != 0;

	private bool SupportV2Conversion => (PowerShellConversion & PowerShellActionPlatformOption.Version2) != 0;

	[NotNull]
	public Action this[string actionId]
	{
		get
		{
			if (string.IsNullOrEmpty(actionId))
			{
				throw new ArgumentNullException("actionId");
			}
			Action action = Find((Action a) => string.Equals(a.Id, actionId));
			if (action != null)
			{
				return action;
			}
			throw new ArgumentOutOfRangeException("actionId");
		}
		set
		{
			if (value == null)
			{
				throw new NullReferenceException();
			}
			if (string.IsNullOrEmpty(actionId))
			{
				throw new ArgumentNullException("actionId");
			}
			int num = IndexOf(actionId);
			value.Id = actionId;
			if (num >= 0)
			{
				this[num] = value;
			}
			else
			{
				Add(value);
			}
		}
	}

	[NotNull]
	public Action this[int index]
	{
		get
		{
			if (v2Coll != null)
			{
				return Action.CreateAction(v2Coll[++index]);
			}
			if (v1Task != null)
			{
				if (SupportV1Conversion)
				{
					return v1Actions[index];
				}
				if (index == 0)
				{
					return v1Actions[0];
				}
			}
			throw new ArgumentOutOfRangeException();
		}
		set
		{
			if (index < 0 || Count <= index)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index is not a valid index in the ActionCollection");
			}
			object oldItem = this[index].Clone();
			if (v2Coll != null)
			{
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
			}
			else
			{
				v1Actions[index] = value;
				SaveV1Actions();
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
			this[index] = (Action)value;
		}
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public event PropertyChangedEventHandler PropertyChanged;

	internal ActionCollection([NotNull] ITask task)
	{
		v1Task = task;
		v1Actions = GetV1Actions();
		PowerShellConversion = Action.TryParse(v1Task.GetDataItem("PowerShellConversion"), psConvert | PowerShellActionPlatformOption.Version2);
	}

	internal ActionCollection([NotNull] ITaskDefinition iTaskDef)
	{
		v2Def = iTaskDef;
		v2Coll = iTaskDef.Actions;
		Match match;
		if (iTaskDef.Data != null && (match = Regex.Match(iTaskDef.Data, psV2IdRegex)).Success)
		{
			bool flag = false;
			try
			{
				flag = bool.Parse(match.Groups["v"].Value);
			}
			catch
			{
				try
				{
					flag = int.Parse(match.Groups["v"].Value) == 1;
				}
				catch
				{
				}
			}
			if (flag)
			{
				psConvert |= PowerShellActionPlatformOption.Version2;
			}
			else
			{
				psConvert &= ~PowerShellActionPlatformOption.Version2;
			}
		}
		UnconvertUnsupportedActions();
	}

	[NotNull]
	public TAction Add<TAction>([NotNull] TAction action) where TAction : Action
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (v2Def != null)
		{
			action.Bind(v2Def);
		}
		else
		{
			if (!SupportV1Conversion && (v1Actions.Count >= 1 || !(action is ExecAction)))
			{
				throw new NotV1SupportedException("Only a single ExecAction is supported unless the PowerShellConversion property includes the Version1 value.");
			}
			v1Actions.Add(action);
			SaveV1Actions();
		}
		OnNotifyPropertyChanged("Count");
		OnNotifyPropertyChanged("Item[]");
		this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, action));
		return action;
	}

	[NotNull]
	public ExecAction Add([NotNull] string path, [CanBeNull] string arguments = null, [CanBeNull] string workingDirectory = null)
	{
		return Add(new ExecAction(path, arguments, workingDirectory));
	}

	[NotNull]
	public Action AddNew(TaskActionType actionType)
	{
		if (Count >= 32)
		{
			throw new ArgumentOutOfRangeException("actionType", "A maximum of 32 actions is allowed within a single task.");
		}
		if (v1Task != null)
		{
			if (!SupportV1Conversion && (v1Actions.Count >= 1 || actionType != 0))
			{
				throw new NotV1SupportedException("Only a single ExecAction is supported unless the PowerShellConversion property includes the Version1 value.");
			}
			return Action.CreateAction(v1Task);
		}
		return Action.CreateAction(v2Coll.Create(actionType));
	}

	public void AddRange([ItemNotNull][NotNull] IEnumerable<Action> actions)
	{
		if (actions == null)
		{
			throw new ArgumentNullException("actions");
		}
		if (v1Task != null)
		{
			List<Action> list = new List<Action>(actions);
			bool flag = list.Count == 1 && list[0].ActionType == TaskActionType.Execute;
			if (!SupportV1Conversion && (v1Actions.Count + list.Count > 1 || !flag))
			{
				throw new NotV1SupportedException("Only a single ExecAction is supported unless the PowerShellConversion property includes the Version1 value.");
			}
			v1Actions.AddRange(actions);
			SaveV1Actions();
			return;
		}
		foreach (Action action in actions)
		{
			Add(action);
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
			v1Actions.Clear();
			SaveV1Actions();
		}
		OnNotifyPropertyChanged("Count");
		OnNotifyPropertyChanged("Item[]");
		this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public bool Contains([NotNull] Action item)
	{
		return Find((Action a) => a.Equals(item)) != null;
	}

	public bool ContainsType(Type actionType)
	{
		return Find((Action a) => a.GetType() == actionType) != null;
	}

	public void CopyTo(Action[] array, int arrayIndex)
	{
		CopyTo(0, array, arrayIndex, Count);
	}

	public void CopyTo(int index, [NotNull] Action[] array, int arrayIndex, int count)
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
			array[arrayIndex + i] = (Action)this[index + i].Clone();
		}
	}

	public void Dispose()
	{
		v1Task = null;
		v2Def = null;
		if (v2Coll != null)
		{
			Marshal.ReleaseComObject(v2Coll);
		}
	}

	public Action Find(Predicate<Action> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		using (IEnumerator<Action> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Action current = enumerator.Current;
				if (match(current))
				{
					return current;
				}
			}
		}
		return null;
	}

	public int FindIndexOf(int startIndex, int count, [NotNull] Predicate<Action> match)
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

	public int FindIndexOf([NotNull] Predicate<Action> match)
	{
		return FindIndexOf(0, Count, match);
	}

	public IEnumerator<Action> GetEnumerator()
	{
		if (v2Coll != null)
		{
			return new ComEnumerator<Action, IAction>(() => v2Coll.Count, (int i) => v2Coll[i], Action.CreateAction);
		}
		return v1Actions.GetEnumerator();
	}

	public int IndexOf(Action item)
	{
		return FindIndexOf((Action a) => a.Equals(item));
	}

	public int IndexOf(string actionId)
	{
		if (string.IsNullOrEmpty(actionId))
		{
			throw new ArgumentNullException("actionId");
		}
		return FindIndexOf((Action a) => string.Equals(a.Id, actionId));
	}

	public void Insert(int index, [NotNull] Action action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (v2Coll != null)
		{
			Action[] array = new Action[Count - index];
			if (Count != index)
			{
				CopyTo(index, array, 0, Count - index);
				for (int num = Count - 1; num >= index; num--)
				{
					RemoveAt(num);
				}
			}
			Add(action);
			if (Count != index)
			{
				for (int i = 0; i < array.Length; i++)
				{
					Add(array[i]);
				}
			}
		}
		else
		{
			if (!SupportV1Conversion && (index > 0 || !(action is ExecAction)))
			{
				throw new NotV1SupportedException("Only a single ExecAction is supported unless the PowerShellConversion property includes the Version1 value.");
			}
			v1Actions.Insert(index, action);
			SaveV1Actions();
		}
		if (!inV2set)
		{
			OnNotifyPropertyChanged("Count");
			OnNotifyPropertyChanged("Item[]");
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, action, index));
		}
	}

	public bool Remove([NotNull] Action item)
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
			throw new ArgumentOutOfRangeException("index", index, "Failed to remove action. Index out of range.");
		}
		object changedItem = this[index].Clone();
		if (v2Coll != null)
		{
			v2Coll.Remove(++index);
		}
		else
		{
			v1Actions.RemoveAt(index);
			SaveV1Actions();
		}
		if (!inV2set)
		{
			OnNotifyPropertyChanged("Count");
			OnNotifyPropertyChanged("Item[]");
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItem, index));
		}
	}

	[NotNull]
	[ItemNotNull]
	public Action[] ToArray()
	{
		Action[] array = new Action[Count];
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
			return Resources.MultipleActions;
		}
		return string.Empty;
	}

	void ICollection<Action>.Add(Action item)
	{
		Add(item);
	}

	int IList.Add(object value)
	{
		Add((Action)value);
		return Count - 1;
	}

	bool IList.Contains(object value)
	{
		return Contains((Action)value);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array != null && array.Rank != 1)
		{
			throw new RankException("Multi-dimensional arrays are not supported.");
		}
		Action[] array2 = new Action[Count];
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
		return IndexOf((Action)value);
	}

	void IList.Insert(int index, object value)
	{
		Insert(index, (Action)value);
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		reader.ReadStartElement(XmlSerializationHelper.GetElementName(this), "http://schemas.microsoft.com/windows/2004/02/mit/task");
		Context = reader.GetAttribute("Context");
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			Action action = Action.CreateAction(Action.TryParse((reader.LocalName == "Exec") ? "Execute" : reader.LocalName, TaskActionType.Execute));
			XmlSerializationHelper.ReadObject(reader, action);
			Add(action);
		}
		reader.ReadEndElement();
	}

	void IList.Remove(object value)
	{
		Remove((Action)value);
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		using IEnumerator<Action> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Action current = enumerator.Current;
			XmlSerializationHelper.WriteObject(writer, current);
		}
	}

	internal void ConvertUnsupportedActions()
	{
		if (TaskService.LibraryVersion.Minor <= 3 || !SupportV2Conversion)
		{
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			Action action = this[i];
			if (action is IBindAsExecAction && !(action is ComHandlerAction))
			{
				this[i] = ExecAction.ConvertToPowerShellAction(action);
			}
		}
	}

	private List<Action> GetV1Actions()
	{
		List<Action> list = new List<Action>();
		if (v1Task != null && v1Task.GetDataItem("ActionType") != "EMPTY")
		{
			ExecAction execAction = new ExecAction(v1Task);
			string[] array = execAction.ParsePowerShellItems();
			if (array != null)
			{
				if (array.Length == 2 && array[0] == "MULTIPLE")
				{
					PowerShellConversion |= PowerShellActionPlatformOption.Version1;
					foreach (Match item in Regex.Matches(array[1], "<# (?<id>\\w+):(?<t>\\w+) #>\\s*(?<c>[^<#]*)\\s*"))
					{
						Action action = Action.ActionFromScript(item.Groups["t"].Value, item.Groups["c"].Value);
						if (action != null)
						{
							if (item.Groups["id"].Value != "NO_ID")
							{
								action.Id = item.Groups["id"].Value;
							}
							list.Add(action);
						}
					}
				}
				else
				{
					list.Add(Action.ConvertFromPowerShellAction(execAction));
				}
			}
			else if (!string.IsNullOrEmpty(execAction.Path))
			{
				list.Add(execAction);
			}
		}
		return list;
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void SaveV1Actions()
	{
		if (v1Task == null)
		{
			throw new ArgumentNullException("v1Task");
		}
		if (v1Actions.Count == 0)
		{
			v1Task.SetApplicationName(string.Empty);
			v1Task.SetParameters(string.Empty);
			v1Task.SetWorkingDirectory(string.Empty);
			v1Task.SetDataItem("ActionId", null);
			v1Task.SetDataItem("ActionType", "EMPTY");
			return;
		}
		if (v1Actions.Count == 1)
		{
			if (!SupportV1Conversion && v1Actions[0].ActionType != 0)
			{
				throw new NotV1SupportedException("Only a single ExecAction is supported unless the PowerShellConversion property includes the Version1 value.");
			}
			v1Task.SetDataItem("ActionType", null);
			v1Actions[0].Bind(v1Task);
			return;
		}
		if (!SupportV1Conversion)
		{
			throw new NotV1SupportedException("Only a single ExecAction is supported unless the PowerShellConversion property includes the Version1 value.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Action v1Action in v1Actions)
		{
			stringBuilder.Append(string.Format("<# {0}:{1} #> {2} ", v1Action.Id ?? "NO_ID", v1Action.ActionType, v1Action.GetPowerShellCommand()));
		}
		ExecAction.CreatePowerShellAction("MULTIPLE", stringBuilder.ToString()).Bind(v1Task);
		v1Task.SetDataItem("ActionId", null);
		v1Task.SetDataItem("ActionType", "MULTIPLE");
	}

	private void UnconvertUnsupportedActions()
	{
		if (TaskService.LibraryVersion.Minor <= 3)
		{
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			if (this[i] is ExecAction execAction)
			{
				Action action = Action.ConvertFromPowerShellAction(execAction);
				if (action != null)
				{
					this[i] = action;
				}
			}
		}
	}
}
