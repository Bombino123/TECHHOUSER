using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[XmlRoot("Task", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = false)]
[XmlSchemaProvider("GetV1SchemaFile")]
[PublicAPI]
[ComVisible(true)]
public sealed class TaskDefinition : IDisposable, IXmlSerializable, INotifyPropertyChanged
{
	internal const string tns = "http://schemas.microsoft.com/windows/2004/02/mit/task";

	internal string v1Name = string.Empty;

	internal ITask v1Task;

	internal ITaskDefinition v2Def;

	private ActionCollection actions;

	private TaskPrincipal principal;

	private TaskRegistrationInfo regInfo;

	private TaskSettings settings;

	private TriggerCollection triggers;

	[XmlArrayItem(ElementName = "Exec", IsNullable = true, Type = typeof(ExecAction))]
	[XmlArrayItem(ElementName = "ShowMessage", IsNullable = true, Type = typeof(ShowMessageAction))]
	[XmlArrayItem(ElementName = "ComHandler", IsNullable = true, Type = typeof(ComHandlerAction))]
	[XmlArrayItem(ElementName = "SendEmail", IsNullable = true, Type = typeof(EmailAction))]
	[XmlArray]
	[NotNull]
	[ItemNotNull]
	public ActionCollection Actions => actions ?? (actions = ((v2Def != null) ? new ActionCollection(v2Def) : new ActionCollection(v1Task)));

	[CanBeNull]
	public string Data
	{
		get
		{
			if (v2Def == null)
			{
				return v1Task.GetDataItem("Data");
			}
			return v2Def.Data;
		}
		set
		{
			if (v2Def != null)
			{
				v2Def.Data = value;
			}
			else
			{
				v1Task.SetDataItem("Data", value);
			}
			OnNotifyPropertyChanged("Data");
		}
	}

	[XmlIgnore]
	public TaskCompatibility LowestSupportedVersion => GetLowestSupportedVersion();

	[XmlArrayItem(ElementName = "BootTrigger", IsNullable = true, Type = typeof(BootTrigger))]
	[XmlArrayItem(ElementName = "CalendarTrigger", IsNullable = true, Type = typeof(CalendarTrigger))]
	[XmlArrayItem(ElementName = "IdleTrigger", IsNullable = true, Type = typeof(IdleTrigger))]
	[XmlArrayItem(ElementName = "LogonTrigger", IsNullable = true, Type = typeof(LogonTrigger))]
	[XmlArrayItem(ElementName = "TimeTrigger", IsNullable = true, Type = typeof(TimeTrigger))]
	[XmlArray]
	[NotNull]
	[ItemNotNull]
	public TriggerCollection Triggers => triggers ?? (triggers = ((v2Def != null) ? new TriggerCollection(v2Def) : new TriggerCollection(v1Task)));

	[XmlIgnore]
	public string XmlText
	{
		get
		{
			if (v2Def == null)
			{
				return XmlSerializationHelper.WriteObjectToXmlText(this);
			}
			return v2Def.XmlText;
		}
		set
		{
			if (v2Def != null)
			{
				v2Def.XmlText = value;
			}
			else
			{
				XmlSerializationHelper.ReadObjectFromXmlText(value, this);
			}
			OnNotifyPropertyChanged("XmlText");
		}
	}

	[NotNull]
	public TaskPrincipal Principal => principal ?? (principal = ((v2Def != null) ? new TaskPrincipal(v2Def.Principal, () => XmlText) : new TaskPrincipal(v1Task)));

	public TaskRegistrationInfo RegistrationInfo => regInfo ?? (regInfo = ((v2Def != null) ? new TaskRegistrationInfo(v2Def.RegistrationInfo) : new TaskRegistrationInfo(v1Task)));

	[NotNull]
	public TaskSettings Settings => settings ?? (settings = ((v2Def != null) ? new TaskSettings(v2Def.Settings) : new TaskSettings(v1Task)));

	public event PropertyChangedEventHandler PropertyChanged;

	internal TaskDefinition([NotNull] ITask iTask, string name)
	{
		v1Task = iTask;
		v1Name = name;
	}

	internal TaskDefinition([NotNull] ITaskDefinition iDef)
	{
		v2Def = iDef;
	}

	public static XmlSchemaComplexType GetV1SchemaFile([NotNull] XmlSchemaSet xs)
	{
		XmlSchema xmlSchema;
		using (Stream input = Assembly.GetAssembly(typeof(TaskDefinition)).GetManifestResourceStream("Microsoft.Win32.TaskScheduler.V1.TaskSchedulerV1Schema.xsd"))
		{
			xmlSchema = (XmlSchema)new XmlSerializer(typeof(XmlSchema)).Deserialize(XmlReader.Create(input));
			xs.Add(xmlSchema);
		}
		XmlQualifiedName name = new XmlQualifiedName("taskType", "http://schemas.microsoft.com/windows/2004/02/mit/task");
		return (XmlSchemaComplexType)xmlSchema.SchemaTypes[name];
	}

	public bool CanUseUnifiedSchedulingEngine(bool throwExceptionWithDetails = false, Version taskSchedulerVersion = null)
	{
		Version version = taskSchedulerVersion ?? TaskService.LibraryVersion;
		if (version < TaskServiceVersion.V1_3)
		{
			return false;
		}
		InvalidOperationException ex = new InvalidOperationException
		{
			HelpLink = "http://msdn.microsoft.com/en-us/library/windows/desktop/aa384138(v=vs.85).aspx"
		};
		bool flag = false;
		if (Settings.NetworkSettings.Id != Guid.Empty && version >= TaskServiceVersion.V1_5)
		{
			flag = true;
			if (!throwExceptionWithDetails)
			{
				return false;
			}
			TryAdd(ex.Data, "Settings.NetworkSettings.Id", "!= Guid.Empty");
		}
		if (!Actions.PowerShellConversion.IsFlagSet(PowerShellActionPlatformOption.Version2))
		{
			for (int i = 0; i < Actions.Count; i++)
			{
				Action action = Actions[i];
				if (!(action is EmailAction))
				{
					if (action is ShowMessageAction)
					{
						flag = true;
						if (!throwExceptionWithDetails)
						{
							return false;
						}
						TryAdd(ex.Data, $"Actions[{i}]", "== typeof(ShowMessageAction)");
					}
				}
				else
				{
					flag = true;
					if (!throwExceptionWithDetails)
					{
						return false;
					}
					TryAdd(ex.Data, $"Actions[{i}]", "== typeof(EmailAction)");
				}
			}
		}
		if (version == TaskServiceVersion.V1_3)
		{
			for (int j = 0; j < Triggers.Count; j++)
			{
				Trigger trigger;
				try
				{
					trigger = Triggers[j];
				}
				catch
				{
					if (!throwExceptionWithDetails)
					{
						return false;
					}
					TryAdd(ex.Data, $"Triggers[{j}]", "is irretrievable.");
					continue;
				}
				if (!(trigger is MonthlyTrigger))
				{
					if (trigger is MonthlyDOWTrigger)
					{
						flag = true;
						if (!throwExceptionWithDetails)
						{
							return false;
						}
						TryAdd(ex.Data, $"Triggers[{j}]", "== typeof(MonthlyDOWTrigger)");
					}
				}
				else
				{
					flag = true;
					if (!throwExceptionWithDetails)
					{
						return false;
					}
					TryAdd(ex.Data, $"Triggers[{j}]", "== typeof(MonthlyTrigger)");
				}
				if (trigger.ExecutionTimeLimit != TimeSpan.Zero)
				{
					flag = true;
					if (!throwExceptionWithDetails)
					{
						return false;
					}
					TryAdd(ex.Data, $"Triggers[{j}].ExecutionTimeLimit", "!= TimeSpan.Zero");
				}
			}
		}
		if (flag && throwExceptionWithDetails)
		{
			throw ex;
		}
		return !flag;
	}

	public void Dispose()
	{
		regInfo = null;
		triggers = null;
		settings = null;
		principal = null;
		actions = null;
		if (v2Def != null)
		{
			Marshal.ReleaseComObject(v2Def);
		}
		v1Task = null;
	}

	public bool Validate(bool throwException = false)
	{
		InvalidOperationException ex = new InvalidOperationException();
		if (Settings.UseUnifiedSchedulingEngine)
		{
			try
			{
				CanUseUnifiedSchedulingEngine(throwException);
			}
			catch (InvalidOperationException ex2)
			{
				foreach (DictionaryEntry datum in ex2.Data)
				{
					TryAdd(ex.Data, (datum.Key as ICloneable)?.Clone() ?? datum.Key, (datum.Value as ICloneable)?.Clone() ?? datum.Value);
				}
			}
		}
		if (Settings.Compatibility >= TaskCompatibility.V2_2)
		{
			TimeSpan timeSpan = TimeSpan.FromDays(1.0);
			if (Settings.MaintenanceSettings.IsSet() && (Settings.MaintenanceSettings.Period < timeSpan || Settings.MaintenanceSettings.Deadline < timeSpan || Settings.MaintenanceSettings.Deadline <= Settings.MaintenanceSettings.Period))
			{
				TryAdd(ex.Data, "Settings.MaintenanceSettings", "Period or Deadline must be at least 1 day and Deadline must be greater than Period.");
			}
		}
		List<TaskCompatibilityEntry> list = new List<TaskCompatibilityEntry>();
		if (GetLowestSupportedVersion(list) > Settings.Compatibility)
		{
			foreach (TaskCompatibilityEntry item in list)
			{
				TryAdd(ex.Data, item.Property, item.Reason);
			}
		}
		bool startWhenAvailable = Settings.StartWhenAvailable;
		bool flag = Settings.DeleteExpiredTaskAfter != TimeSpan.Zero;
		bool flag2 = Settings.Compatibility < TaskCompatibility.V2;
		bool flag3 = false;
		for (int i = 0; i < Triggers.Count; i++)
		{
			Trigger trigger;
			try
			{
				trigger = Triggers[i];
			}
			catch
			{
				TryAdd(ex.Data, $"Triggers[{i}]", "is irretrievable.");
				continue;
			}
			if (startWhenAvailable && trigger.Repetition.Duration != TimeSpan.Zero && trigger.EndBoundary == DateTime.MaxValue)
			{
				TryAdd(ex.Data, "Settings.StartWhenAvailable", "== true requires time-based tasks with an end boundary or time-based tasks that are set to repeat infinitely.");
			}
			if (flag2 && trigger.Repetition.Interval != TimeSpan.Zero && trigger.Repetition.Interval >= trigger.Repetition.Duration)
			{
				TryAdd(ex.Data, "Trigger.Repetition.Interval", ">= Trigger.Repetition.Duration under Task Scheduler 1.0.");
			}
			if (trigger.EndBoundary <= trigger.StartBoundary)
			{
				TryAdd(ex.Data, "Trigger.StartBoundary", ">= Trigger.EndBoundary is not allowed.");
			}
			if (flag && trigger.EndBoundary != DateTime.MaxValue)
			{
				flag3 = true;
			}
		}
		if (flag && !flag3)
		{
			TryAdd(ex.Data, "Settings.DeleteExpiredTaskAfter", "!= TimeSpan.Zero requires at least one trigger with an end boundary.");
		}
		if (throwException && ex.Data.Count > 0)
		{
			throw ex;
		}
		return ex.Data.Count == 0;
	}

	public static TaskDefinition operator +(TaskDefinition definition, Trigger trigger)
	{
		definition.Triggers.Add(trigger);
		return definition;
	}

	public static TaskDefinition operator +(TaskDefinition definition, Action action)
	{
		definition.Actions.Add(action);
		return definition;
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		reader.ReadStartElement(XmlSerializationHelper.GetElementName(this), "http://schemas.microsoft.com/windows/2004/02/mit/task");
		XmlSerializationHelper.ReadObjectProperties(reader, this);
		reader.ReadEndElement();
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		XmlSerializationHelper.WriteObjectProperties(writer, this);
	}

	internal static Dictionary<string, string> GetV1TaskDataDictionary(ITask v1Task)
	{
		object v1TaskData = GetV1TaskData(v1Task);
		Dictionary<string, string> dictionary = ((!(v1TaskData is string)) ? (v1TaskData as Dictionary<string, string>) : new Dictionary<string, string>(2)
		{
			{
				"Data",
				v1TaskData.ToString()
			},
			{
				"Documentation",
				v1TaskData.ToString()
			}
		});
		return dictionary ?? new Dictionary<string, string>();
	}

	internal static void SetV1TaskData(ITask v1Task, object value)
	{
		if (value == null)
		{
			v1Task.SetWorkItemData(0, null);
			return;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, value);
		v1Task.SetWorkItemData((ushort)memoryStream.Length, memoryStream.ToArray());
	}

	internal void V1Save(string newName)
	{
		if (v1Task == null)
		{
			return;
		}
		Triggers.Bind();
		IPersistFile persistFile = (IPersistFile)v1Task;
		if (string.IsNullOrEmpty(newName) || newName == v1Name)
		{
			try
			{
				persistFile.Save(null, fRemember: false);
				persistFile = null;
				return;
			}
			catch
			{
			}
		}
		persistFile.GetCurFile(out string ppszFileName);
		File.Delete(ppszFileName);
		string? directoryName = Path.GetDirectoryName(ppszFileName);
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		ppszFileName = directoryName + directorySeparatorChar + newName + Path.GetExtension(ppszFileName);
		File.Delete(ppszFileName);
		persistFile.Save(ppszFileName, fRemember: true);
	}

	private static object GetV1TaskData(ITask v1Task)
	{
		IntPtr Data = IntPtr.Zero;
		try
		{
			v1Task.GetWorkItemData(out var DataLen, out Data);
			if (DataLen == 0)
			{
				return null;
			}
			byte[] array = new byte[DataLen];
			Marshal.Copy(Data, array, 0, DataLen);
			MemoryStream serializationStream = new MemoryStream(array, writable: false);
			return new BinaryFormatter().Deserialize(serializationStream);
		}
		catch
		{
		}
		finally
		{
			if (Data != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(Data);
			}
		}
		return null;
	}

	private static void TryAdd(IDictionary d, object k, object v)
	{
		if (!d.Contains(k))
		{
			d.Add(k, v);
		}
	}

	private TaskCompatibility GetLowestSupportedVersion(IList<TaskCompatibilityEntry> outputList = null)
	{
		TaskCompatibility taskCompatibility = TaskCompatibility.V1;
		List<TaskCompatibilityEntry> list = new List<TaskCompatibilityEntry>();
		if (Principal.GroupId != null)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Principal.GroupId", "cannot have a value."));
		}
		if (Principal.LogonType == TaskLogonType.Group || Principal.LogonType == TaskLogonType.None || Principal.LogonType == TaskLogonType.S4U)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Principal.LogonType", "cannot be Group, None or S4U."));
		}
		if (Principal.RunLevel == TaskRunLevel.Highest)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Principal.RunLevel", "cannot be set to Highest."));
		}
		if (RegistrationInfo.SecurityDescriptorSddlForm != null)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "RegistrationInfo.SecurityDescriptorSddlForm", "cannot have a value."));
		}
		if (!Settings.AllowDemandStart)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.AllowDemandStart", "must be true."));
		}
		if (!Settings.AllowHardTerminate)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.AllowHardTerminate", "must be true."));
		}
		if (Settings.MultipleInstances != TaskInstancesPolicy.IgnoreNew)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.MultipleInstances", "must be set to IgnoreNew."));
		}
		if (Settings.NetworkSettings.IsSet())
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.NetworkSetting", "cannot have a value."));
		}
		if (Settings.RestartCount != 0)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.RestartCount", "must be 0."));
		}
		if (Settings.RestartInterval != TimeSpan.Zero)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.RestartInterval", "must be 0 (TimeSpan.Zero)."));
		}
		if (Settings.StartWhenAvailable)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Settings.StartWhenAvailable", "must be false."));
		}
		if ((Actions.PowerShellConversion & PowerShellActionPlatformOption.Version1) != PowerShellActionPlatformOption.Version1 && (Actions.ContainsType(typeof(EmailAction)) || Actions.ContainsType(typeof(ShowMessageAction)) || Actions.ContainsType(typeof(ComHandlerAction))))
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Actions", "may only contain ExecAction types unless Actions.PowerShellConversion includes Version1."));
		}
		if ((Actions.PowerShellConversion & PowerShellActionPlatformOption.Version2) != PowerShellActionPlatformOption.Version2 && (Actions.ContainsType(typeof(EmailAction)) || Actions.ContainsType(typeof(ShowMessageAction))))
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_1, "Actions", "may only contain ExecAction and ComHanlderAction types unless Actions.PowerShellConversion includes Version2."));
		}
		try
		{
			if (Triggers.Find((Trigger t) => t is ITriggerDelay && ((ITriggerDelay)t).Delay != TimeSpan.Zero) != null)
			{
				list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain delays."));
			}
			if (Triggers.Find((Trigger t) => t.ExecutionTimeLimit != TimeSpan.Zero || t.Id != null) != null)
			{
				list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain an ExecutionTimeLimit or Id."));
			}
			if (Triggers.Find((Trigger t) => (t as LogonTrigger)?.UserId != null) != null)
			{
				list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain a LogonTrigger with a UserId."));
			}
			if (Triggers.Find((Trigger t) => (t is MonthlyDOWTrigger && ((MonthlyDOWTrigger)t).RunOnLastWeekOfMonth) || (t is MonthlyDOWTrigger && (((MonthlyDOWTrigger)t).WeeksOfMonth & (((MonthlyDOWTrigger)t).WeeksOfMonth - 1)) != 0)) != null)
			{
				list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain a MonthlyDOWTrigger with RunOnLastWeekOfMonth set or multiple WeeksOfMonth."));
			}
			if (Triggers.Find((Trigger t) => t is MonthlyTrigger && ((MonthlyTrigger)t).RunOnLastDayOfMonth) != null)
			{
				list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain a MonthlyTrigger with RunOnLastDayOfMonth set."));
			}
			if (Triggers.ContainsType(typeof(EventTrigger)) || Triggers.ContainsType(typeof(SessionStateChangeTrigger)) || Triggers.ContainsType(typeof(RegistrationTrigger)))
			{
				list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain EventTrigger, SessionStateChangeTrigger, or RegistrationTrigger types."));
			}
		}
		catch
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2, "Triggers", "cannot contain Custom triggers."));
		}
		if (Principal.ProcessTokenSidType != TaskProcessTokenSidType.Default)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_1, "Principal.ProcessTokenSidType", "must be Default."));
		}
		if (Principal.RequiredPrivileges.Count > 0)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_1, "Principal.RequiredPrivileges", "must be empty."));
		}
		if (Settings.DisallowStartOnRemoteAppSession)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_1, "Settings.DisallowStartOnRemoteAppSession", "must be false."));
		}
		if (Settings.UseUnifiedSchedulingEngine)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_1, "Settings.UseUnifiedSchedulingEngine", "must be false."));
		}
		if (Settings.MaintenanceSettings.IsSet())
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_2, "this.Settings.MaintenanceSettings", "must have no values set."));
		}
		if (Settings.Volatile)
		{
			list.Add(new TaskCompatibilityEntry(TaskCompatibility.V2_2, " this.Settings.Volatile", "must be false."));
		}
		foreach (TaskCompatibilityEntry item in list)
		{
			if (taskCompatibility < item.CompatibilityLevel)
			{
				taskCompatibility = item.CompatibilityLevel;
			}
			outputList?.Add(item);
		}
		return taskCompatibility;
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
