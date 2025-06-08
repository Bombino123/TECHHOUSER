using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public abstract class Action : IDisposable, ICloneable, IEquatable<Action>, INotifyPropertyChanged, IComparable, IComparable<Action>
{
	internal IAction iAction;

	internal ITask v1Task;

	protected readonly Dictionary<string, object> unboundValues = new Dictionary<string, object>();

	[XmlIgnore]
	public TaskActionType ActionType => iAction?.Type ?? InternalActionType;

	[DefaultValue(null)]
	[XmlAttribute(AttributeName = "id")]
	public virtual string Id
	{
		get
		{
			return GetProperty<string, IAction>("Id");
		}
		set
		{
			SetProperty<string, IAction>("Id", value);
		}
	}

	internal abstract TaskActionType InternalActionType { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	internal Action()
	{
	}

	internal Action([NotNull] IAction action)
	{
		iAction = action;
	}

	internal Action([NotNull] ITask iTask)
	{
		v1Task = iTask;
	}

	public static Action CreateAction(TaskActionType actionType)
	{
		return Activator.CreateInstance(GetObjectType(actionType)) as Action;
	}

	public object Clone()
	{
		Action action = CreateAction(ActionType);
		action.CopyProperties(this);
		return action;
	}

	public int CompareTo(Action obj)
	{
		return string.Compare(Id, obj?.Id, StringComparison.InvariantCulture);
	}

	public virtual void Dispose()
	{
		if (iAction != null)
		{
			Marshal.ReleaseComObject(iAction);
		}
	}

	public override bool Equals([CanBeNull] object obj)
	{
		if (obj is Action)
		{
			return Equals((Action)obj);
		}
		return base.Equals(obj);
	}

	public virtual bool Equals([NotNull] Action other)
	{
		if (ActionType == other.ActionType)
		{
			return Id == other.Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return new
		{
			A = ActionType,
			B = Id
		}.GetHashCode();
	}

	public override string ToString()
	{
		return Id;
	}

	public virtual string ToString([NotNull] CultureInfo culture)
	{
		using (new CultureSwitcher(culture))
		{
			return ToString();
		}
	}

	int IComparable.CompareTo(object obj)
	{
		return CompareTo(obj as Action);
	}

	internal static Action ActionFromScript(string actionType, string script)
	{
		return (Action)GetObjectType(TryParse(actionType, TaskActionType.Execute)).InvokeMember("FromPowerShellCommand", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[1] { script });
	}

	internal static Action ConvertFromPowerShellAction(ExecAction execAction)
	{
		string[] array = execAction.ParsePowerShellItems();
		if (array != null && array.Length == 2)
		{
			Action action = ActionFromScript(array[0], array[1]);
			if (action != null)
			{
				action.v1Task = execAction.v1Task;
				action.iAction = execAction.iAction;
				return action;
			}
		}
		return null;
	}

	internal static Action CreateAction(ITask iTask)
	{
		ExecAction execAction = new ExecAction(iTask);
		return ConvertFromPowerShellAction(execAction) ?? execAction;
	}

	internal static Action CreateAction(IAction iAction)
	{
		return Activator.CreateInstance(GetObjectType(iAction.Type), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[1] { iAction }, null) as Action;
	}

	internal static T TryParse<T>(string val, T defaultVal)
	{
		T result = defaultVal;
		if (val != null)
		{
			try
			{
				result = (T)Enum.Parse(typeof(T), val);
			}
			catch
			{
			}
		}
		return result;
	}

	internal virtual void Bind(ITask iTask)
	{
		if (Id != null)
		{
			iTask.SetDataItem("ActionId", Id);
		}
		IBindAsExecAction bindAsExecAction = this as IBindAsExecAction;
		if (bindAsExecAction != null)
		{
			iTask.SetDataItem("ActionType", InternalActionType.ToString());
		}
		unboundValues.TryGetValue("Path", out var value);
		iTask.SetApplicationName((bindAsExecAction != null) ? "powershell" : (value?.ToString() ?? string.Empty));
		unboundValues.TryGetValue("Arguments", out value);
		iTask.SetParameters((bindAsExecAction != null) ? ExecAction.BuildPowerShellCmd(ActionType.ToString(), GetPowerShellCommand()) : (value?.ToString() ?? string.Empty));
		unboundValues.TryGetValue("WorkingDirectory", out value);
		iTask.SetWorkingDirectory(value?.ToString() ?? string.Empty);
	}

	internal virtual void Bind(ITaskDefinition iTaskDef)
	{
		IActionCollection actions = iTaskDef.Actions;
		if (actions.Count >= 32)
		{
			throw new ArgumentOutOfRangeException("iTaskDef", "A maximum of 32 actions is allowed within a single task.");
		}
		CreateV2Action(actions);
		Marshal.ReleaseComObject(actions);
		foreach (string key in unboundValues.Keys)
		{
			try
			{
				ReflectionHelper.SetProperty(iAction, key, unboundValues[key]);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			catch
			{
			}
		}
		unboundValues.Clear();
	}

	internal virtual void CopyProperties([NotNull] Action sourceAction)
	{
		Id = sourceAction.Id;
	}

	internal abstract void CreateV2Action(IActionCollection iActions);

	internal abstract string GetPowerShellCommand();

	internal T GetProperty<T, TB>(string propName, T defaultValue = default(T))
	{
		if (iAction == null)
		{
			if (!unboundValues.TryGetValue(propName, out var value))
			{
				return defaultValue;
			}
			return (T)value;
		}
		return ReflectionHelper.GetProperty((TB)iAction, propName, defaultValue);
	}

	internal void OnPropertyChanged(string propName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	}

	internal void SetProperty<T, TB>(string propName, T value)
	{
		if (iAction == null)
		{
			if (object.Equals(value, default(T)))
			{
				unboundValues.Remove(propName);
			}
			else
			{
				unboundValues[propName] = value;
			}
		}
		else
		{
			ReflectionHelper.SetProperty((TB)iAction, propName, value);
		}
		OnPropertyChanged(propName);
	}

	[NotNull]
	private static Type GetObjectType(TaskActionType actionType)
	{
		return actionType switch
		{
			TaskActionType.ComHandler => typeof(ComHandlerAction), 
			TaskActionType.SendEmail => typeof(EmailAction), 
			TaskActionType.ShowMessage => typeof(ShowMessageAction), 
			_ => typeof(ExecAction), 
		};
	}
}
