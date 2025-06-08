using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = false)]
[PublicAPI]
[ComVisible(true)]
public class Task : IDisposable, IComparable, IComparable<Task>, INotifyPropertyChanged
{
	private class DefDoc
	{
		private readonly XmlDocument doc;

		public Version Version
		{
			get
			{
				try
				{
					return new Version(doc["Task"].Attributes["version"].Value);
				}
				catch
				{
					throw new InvalidOperationException("Task definition does not contain a version.");
				}
			}
			set
			{
				XmlElement xmlElement = doc["Task"];
				if (xmlElement != null)
				{
					xmlElement.Attributes["version"].Value = value.ToString(2);
				}
			}
		}

		public string Xml => doc.OuterXml;

		public DefDoc(string xml)
		{
			doc = new XmlDocument();
			doc.LoadXml(xml);
		}

		public bool Contains(string tag, string defaultVal = null, bool removeIfFound = false)
		{
			XmlNodeList elementsByTagName = doc.GetElementsByTagName(tag);
			while (elementsByTagName.Count > 0)
			{
				XmlNode xmlNode = elementsByTagName[0];
				if (xmlNode.InnerText != defaultVal || !removeIfFound || xmlNode.ParentNode == null)
				{
					return true;
				}
				xmlNode.ParentNode?.RemoveChild(xmlNode);
				elementsByTagName = doc.GetElementsByTagName(tag);
			}
			return false;
		}

		public void RemoveTag(string tag)
		{
			XmlNodeList elementsByTagName = doc.GetElementsByTagName(tag);
			while (elementsByTagName.Count > 0)
			{
				XmlNode xmlNode = elementsByTagName[0];
				xmlNode.ParentNode?.RemoveChild(xmlNode);
				elementsByTagName = doc.GetElementsByTagName(tag);
			}
		}
	}

	internal const AccessControlSections defaultAccessControlSections = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group;

	internal const SecurityInfos defaultSecurityInfosSections = SecurityInfos.Owner | SecurityInfos.Group | SecurityInfos.DiscretionaryAcl;

	internal ITask v1Task;

	private static readonly int osLibMinorVer = GetOSLibraryMinorVersion();

	private static readonly DateTime v2InvalidDate = new DateTime(1899, 12, 30);

	private readonly IRegisteredTask v2Task;

	private TaskDefinition myTD;

	[NotNull]
	public TaskDefinition Definition => myTD ?? (myTD = ((v2Task != null) ? new TaskDefinition(GetV2Definition(TaskService, v2Task, throwError: true)) : new TaskDefinition(v1Task, Name)));

	public bool Enabled
	{
		get
		{
			return v2Task?.Enabled ?? Definition.Settings.Enabled;
		}
		set
		{
			if (v2Task != null)
			{
				v2Task.Enabled = value;
			}
			else
			{
				Definition.Settings.Enabled = value;
				Definition.V1Save(null);
			}
			OnNotifyPropertyChanged("Enabled");
		}
	}

	[NotNull]
	public TaskFolder Folder
	{
		get
		{
			if (v2Task == null)
			{
				return TaskService.RootFolder;
			}
			string directoryName = System.IO.Path.GetDirectoryName(v2Task.Path);
			if (string.IsNullOrEmpty(directoryName) || directoryName == "\\")
			{
				return TaskService.RootFolder;
			}
			return TaskService.GetFolder(directoryName) ?? throw new DirectoryNotFoundException();
		}
	}

	public bool IsActive
	{
		get
		{
			DateTime now = DateTime.Now;
			if (!Definition.Settings.Enabled)
			{
				return false;
			}
			foreach (Trigger trigger in Definition.Triggers)
			{
				if (trigger.Enabled && !(now < trigger.StartBoundary) && !(now > trigger.EndBoundary) && (!(trigger is ICalendarTrigger) || DateTime.MinValue != NextRunTime || trigger is TimeTrigger))
				{
					return true;
				}
			}
			return false;
		}
	}

	public DateTime LastRunTime
	{
		get
		{
			if (v2Task == null)
			{
				return v1Task.GetMostRecentRunTime();
			}
			DateTime lastRunTime = v2Task.LastRunTime;
			if (!(lastRunTime == v2InvalidDate))
			{
				return lastRunTime;
			}
			return DateTime.MinValue;
		}
	}

	public int LastTaskResult
	{
		get
		{
			if (v2Task != null)
			{
				return v2Task.LastTaskResult;
			}
			return (int)v1Task.GetExitCode();
		}
	}

	public DateTime NextRunTime
	{
		get
		{
			if (v2Task == null)
			{
				return v1Task.GetNextRunTime();
			}
			DateTime nextRunTime = v2Task.NextRunTime;
			if (nextRunTime != DateTime.MinValue && nextRunTime != v2InvalidDate)
			{
				if (!(nextRunTime == v2InvalidDate))
				{
					return nextRunTime;
				}
				return DateTime.MinValue;
			}
			DateTime[] runTimes = GetRunTimes(DateTime.Now, DateTime.MaxValue, 1u);
			nextRunTime = ((runTimes.Length != 0) ? runTimes[0] : DateTime.MinValue);
			if (!(nextRunTime == v2InvalidDate))
			{
				return nextRunTime;
			}
			return DateTime.MinValue;
		}
	}

	public bool ReadOnly { get; internal set; }

	[Obsolete("This property will be removed in deference to the GetAccessControl, GetSecurityDescriptorSddlForm, SetAccessControl and SetSecurityDescriptorSddlForm methods.")]
	public GenericSecurityDescriptor SecurityDescriptor
	{
		get
		{
			return new RawSecurityDescriptor(GetSecurityDescriptorSddlForm());
		}
		set
		{
			SetSecurityDescriptorSddlForm(value.GetSddlForm(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group));
		}
	}

	public virtual TaskState State
	{
		get
		{
			if (v2Task != null)
			{
				return v2Task.State;
			}
			V1Reactivate();
			if (!Enabled)
			{
				return TaskState.Disabled;
			}
			switch (v1Task.GetStatus())
			{
			case TaskStatus.Ready:
			case TaskStatus.NeverRun:
			case TaskStatus.NoMoreRuns:
			case TaskStatus.Terminated:
				return TaskState.Ready;
			case TaskStatus.Running:
				return TaskState.Running;
			case TaskStatus.Disabled:
				return TaskState.Disabled;
			default:
				return TaskState.Unknown;
			}
		}
	}

	public TaskService TaskService { get; }

	[NotNull]
	public string Name
	{
		get
		{
			if (v2Task == null)
			{
				return System.IO.Path.GetFileNameWithoutExtension(GetV1Path(v1Task));
			}
			return v2Task.Name;
		}
	}

	public int NumberOfMissedRuns => (v2Task ?? throw new NotV1SupportedException()).NumberOfMissedRuns;

	[NotNull]
	public string Path
	{
		get
		{
			if (v2Task == null)
			{
				return "\\" + Name;
			}
			return v2Task.Path;
		}
	}

	public string Xml
	{
		get
		{
			if (v2Task == null)
			{
				return Definition.XmlText;
			}
			return v2Task.Xml;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal Task([NotNull] TaskService svc, [NotNull] ITask iTask)
	{
		TaskService = svc;
		v1Task = iTask;
		ReadOnly = false;
	}

	internal Task([NotNull] TaskService svc, [NotNull] IRegisteredTask iTask, ITaskDefinition iDef = null)
	{
		TaskService = svc;
		v2Task = iTask;
		if (iDef != null)
		{
			myTD = new TaskDefinition(iDef);
		}
		ReadOnly = false;
	}

	public int CompareTo(Task other)
	{
		return string.Compare(Path, other?.Path, StringComparison.InvariantCulture);
	}

	public void Dispose()
	{
		if (v2Task != null)
		{
			Marshal.ReleaseComObject(v2Task);
		}
		v1Task = null;
	}

	public void Export([NotNull] string outputFileName)
	{
		File.WriteAllText(outputFileName, Xml, Encoding.Unicode);
	}

	public TaskSecurity GetAccessControl()
	{
		return GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	public TaskSecurity GetAccessControl(AccessControlSections includeSections)
	{
		return new TaskSecurity(this, includeSections);
	}

	[NotNull]
	[ItemNotNull]
	public RunningTaskCollection GetInstances()
	{
		if (v2Task == null)
		{
			throw new NotV1SupportedException();
		}
		return new RunningTaskCollection(TaskService, v2Task.GetInstances(0));
	}

	public DateTime GetLastRegistrationTime()
	{
		DateTime date = Definition.RegistrationInfo.Date;
		if (date != DateTime.MinValue)
		{
			return date;
		}
		TaskEventLog taskEventLog = new TaskEventLog(Path, new int[1] { 106 }, null, TaskService.TargetServer, TaskService.UserAccountDomain, TaskService.UserName, TaskService.UserPassword);
		if (!taskEventLog.Enabled)
		{
			return date;
		}
		foreach (TaskEvent item in (IEnumerable<TaskEvent>)taskEventLog)
		{
			if (item.TimeCreated.HasValue)
			{
				return item.TimeCreated.Value;
			}
		}
		return date;
	}

	[NotNull]
	public DateTime[] GetRunTimes(DateTime start, DateTime end, uint count = 0u)
	{
		NativeMethods.SYSTEMTIME Begin = start;
		NativeMethods.SYSTEMTIME End = end;
		IntPtr TaskTimes = IntPtr.Zero;
		DateTime[] result = new DateTime[0];
		try
		{
			if (v2Task != null)
			{
				v2Task.GetRunTimes(ref Begin, ref End, ref count, ref TaskTimes);
			}
			else
			{
				ushort Count = (ushort)((count != 0 && count <= 1440) ? ((ushort)count) : 1440);
				v1Task.GetRunTimes(ref Begin, ref End, ref Count, ref TaskTimes);
				count = Count;
			}
			result = InteropUtil.ToArray<NativeMethods.SYSTEMTIME, DateTime>(TaskTimes, (int)count);
		}
		catch (Exception)
		{
		}
		finally
		{
			Marshal.FreeCoTaskMem(TaskTimes);
		}
		return result;
	}

	public string GetSecurityDescriptorSddlForm(SecurityInfos includeSections = SecurityInfos.Owner | SecurityInfos.Group | SecurityInfos.DiscretionaryAcl)
	{
		if (v2Task == null)
		{
			throw new NotV1SupportedException();
		}
		return v2Task.GetSecurityDescriptor((int)includeSections);
	}

	public void RegisterChanges()
	{
		if (Definition.Principal.RequiresPassword())
		{
			throw new SecurityException("Tasks which have been registered previously with stored passwords must use the TaskFolder.RegisterTaskDefinition method for updates.");
		}
		if (v2Task != null)
		{
			TaskService.GetFolder(System.IO.Path.GetDirectoryName(Path)).RegisterTaskDefinition(Name, Definition, TaskCreation.Update, null, null, Definition.Principal.LogonType);
		}
		else
		{
			TaskService.RootFolder.RegisterTaskDefinition(Name, Definition);
		}
	}

	public RunningTask Run(params string[] parameters)
	{
		if (v2Task != null)
		{
			if (parameters.Length > 32)
			{
				throw new ArgumentOutOfRangeException("parameters", "A maximum of 32 values is allowed.");
			}
			if (TaskService.HighestSupportedVersion < TaskServiceVersion.V1_5 && parameters.Any((string p) => (p?.Length ?? 0) >= 260))
			{
				throw new ArgumentOutOfRangeException("parameters", "On systems prior to Windows 10, all individual parameters must be less than 260 characters.");
			}
			IRunningTask runningTask = v2Task.Run((parameters.Length == 0) ? null : parameters);
			if (runningTask == null)
			{
				return null;
			}
			return new RunningTask(TaskService, v2Task, runningTask);
		}
		v1Task.Run();
		return new RunningTask(TaskService, v1Task);
	}

	public RunningTask RunEx(TaskRunFlags flags, int sessionID, string user, params string[] parameters)
	{
		if (v2Task == null)
		{
			throw new NotV1SupportedException();
		}
		if (parameters == null || parameters.Any((string s) => s == null))
		{
			throw new ArgumentNullException("parameters", "The array and none of the values passed as parameters may be `null`.");
		}
		if (parameters.Length > 32)
		{
			throw new ArgumentOutOfRangeException("parameters", "A maximum of 32 parameters can be supplied to RunEx.");
		}
		if (TaskService.HighestSupportedVersion < TaskServiceVersion.V1_5 && parameters.Any((string p) => (p?.Length ?? 0) >= 260))
		{
			throw new ArgumentOutOfRangeException("parameters", "On systems prior to Windows 10, no individual parameter may be more than 260 characters.");
		}
		IRunningTask runningTask = v2Task.RunEx((parameters.Length == 0) ? null : parameters, (int)flags, sessionID, user);
		if (runningTask == null)
		{
			return null;
		}
		return new RunningTask(TaskService, v2Task, runningTask);
	}

	public void SetAccessControl([NotNull] TaskSecurity taskSecurity)
	{
		taskSecurity.Persist(this);
	}

	public void SetSecurityDescriptorSddlForm([NotNull] string sddlForm, TaskSetSecurityOptions options = TaskSetSecurityOptions.None)
	{
		if (v2Task != null)
		{
			v2Task.SetSecurityDescriptor(sddlForm, (int)options);
			return;
		}
		throw new NotV1SupportedException();
	}

	public bool ShowEditor()
	{
		try
		{
			Type type = ReflectionHelper.LoadType("Microsoft.Win32.TaskScheduler.TaskEditDialog", "Microsoft.Win32.TaskSchedulerEditor.dll");
			if (type != null)
			{
				return ReflectionHelper.InvokeMethod<int>(type, new object[3] { this, true, true }, "ShowDialog", new object[0]) == 1;
			}
		}
		catch
		{
		}
		return false;
	}

	public void ShowPropertyPage()
	{
		if (v1Task != null)
		{
			v1Task.EditWorkItem(IntPtr.Zero, 0u);
			return;
		}
		throw new NotV2SupportedException();
	}

	public void Stop()
	{
		if (v2Task != null)
		{
			v2Task.Stop(0);
		}
		else
		{
			v1Task.Terminate();
		}
	}

	public override string ToString()
	{
		return Name;
	}

	int IComparable.CompareTo(object other)
	{
		return CompareTo(other as Task);
	}

	internal static Task CreateTask(TaskService svc, IRegisteredTask iTask, bool throwError = false)
	{
		ITaskDefinition v2Definition = GetV2Definition(svc, iTask, throwError);
		if (v2Definition != null || !svc.AllowReadOnlyTasks)
		{
			return new Task(svc, iTask, v2Definition);
		}
		v2Definition = GetV2StrippedDefinition(svc, iTask);
		return new Task(svc, iTask, v2Definition)
		{
			ReadOnly = true
		};
	}

	internal static int GetOSLibraryMinorVersion()
	{
		return TaskService.LibraryVersion.Minor;
	}

	[NotNull]
	internal static string GetV1Path(ITask v1Task)
	{
		((IPersistFile)v1Task).GetCurFile(out string ppszFileName);
		return ppszFileName ?? string.Empty;
	}

	internal static ITaskDefinition GetV2Definition(TaskService svc, IRegisteredTask iTask, bool throwError = false)
	{
		Version version = new Version();
		try
		{
			DefDoc defDoc = new DefDoc(iTask.Xml);
			version = defDoc.Version;
			if (version.Minor > osLibMinorVer)
			{
				int num = version.Minor;
				if (!defDoc.Contains("Volatile", "false", removeIfFound: true) && !defDoc.Contains("MaintenanceSettings"))
				{
					num = 3;
				}
				if (!defDoc.Contains("UseUnifiedSchedulingEngine", "false", removeIfFound: true) && !defDoc.Contains("DisallowStartOnRemoteAppSession", "false", removeIfFound: true) && !defDoc.Contains("RequiredPrivileges") && !defDoc.Contains("ProcessTokenSidType", "Default", removeIfFound: true))
				{
					num = 2;
				}
				if (!defDoc.Contains("DisplayName") && !defDoc.Contains("GroupId") && !defDoc.Contains("RunLevel", "LeastPrivilege", removeIfFound: true) && !defDoc.Contains("SecurityDescriptor") && !defDoc.Contains("Source") && !defDoc.Contains("URI") && !defDoc.Contains("AllowStartOnDemand", "true", removeIfFound: true) && !defDoc.Contains("AllowHardTerminate", "true", removeIfFound: true) && !defDoc.Contains("MultipleInstancesPolicy", "IgnoreNew", removeIfFound: true) && !defDoc.Contains("NetworkSettings") && !defDoc.Contains("StartWhenAvailable", "false", removeIfFound: true) && !defDoc.Contains("SendEmail") && !defDoc.Contains("ShowMessage") && !defDoc.Contains("ComHandler") && !defDoc.Contains("EventTrigger") && !defDoc.Contains("SessionStateChangeTrigger") && !defDoc.Contains("RegistrationTrigger") && !defDoc.Contains("RestartOnFailure") && !defDoc.Contains("LogonType", "None", removeIfFound: true))
				{
					num = 1;
				}
				if (num > osLibMinorVer && throwError)
				{
					throw new InvalidOperationException($"The current version of the native library (1.{osLibMinorVer}) does not support the original or minimum version of the \"{iTask.Name}\" task ({version}/1.{num}). This is likely due to attempting to read the remote tasks of a newer version of Windows from a down-level client.");
				}
				if (num != version.Minor)
				{
					defDoc.Version = new Version(1, num);
					ITaskDefinition taskDefinition = svc.v2TaskService.NewTask(0u);
					taskDefinition.XmlText = defDoc.Xml;
					return taskDefinition;
				}
			}
			return iTask.Definition;
		}
		catch (COMException ex)
		{
			if (throwError)
			{
				if (ex.ErrorCode == -2147216616 && version.Minor != osLibMinorVer)
				{
					throw new InvalidOperationException($"The current version of the native library (1.{osLibMinorVer}) does not support the version of the \"{iTask.Name}\" task ({version})");
				}
				throw;
			}
		}
		catch
		{
			if (throwError)
			{
				throw;
			}
		}
		return null;
	}

	internal static ITaskDefinition GetV2StrippedDefinition(TaskService svc, IRegisteredTask iTask)
	{
		try
		{
			DefDoc defDoc = new DefDoc(iTask.Xml);
			if (defDoc.Version.Minor > osLibMinorVer)
			{
				if (osLibMinorVer < 4)
				{
					defDoc.RemoveTag("Volatile");
					defDoc.RemoveTag("MaintenanceSettings");
				}
				if (osLibMinorVer < 3)
				{
					defDoc.RemoveTag("UseUnifiedSchedulingEngine");
					defDoc.RemoveTag("DisallowStartOnRemoteAppSession");
					defDoc.RemoveTag("RequiredPrivileges");
					defDoc.RemoveTag("ProcessTokenSidType");
				}
				if (osLibMinorVer < 2)
				{
					defDoc.RemoveTag("DisplayName");
					defDoc.RemoveTag("GroupId");
					defDoc.RemoveTag("RunLevel");
					defDoc.RemoveTag("SecurityDescriptor");
					defDoc.RemoveTag("Source");
					defDoc.RemoveTag("URI");
					defDoc.RemoveTag("AllowStartOnDemand");
					defDoc.RemoveTag("AllowHardTerminate");
					defDoc.RemoveTag("MultipleInstancesPolicy");
					defDoc.RemoveTag("NetworkSettings");
					defDoc.RemoveTag("StartWhenAvailable");
					defDoc.RemoveTag("SendEmail");
					defDoc.RemoveTag("ShowMessage");
					defDoc.RemoveTag("ComHandler");
					defDoc.RemoveTag("EventTrigger");
					defDoc.RemoveTag("SessionStateChangeTrigger");
					defDoc.RemoveTag("RegistrationTrigger");
					defDoc.RemoveTag("RestartOnFailure");
					defDoc.RemoveTag("LogonType");
				}
				defDoc.RemoveTag("WnfStateChangeTrigger");
				defDoc.Version = new Version(1, osLibMinorVer);
				ITaskDefinition taskDefinition = svc.v2TaskService.NewTask(0u);
				taskDefinition.XmlText = defDoc.Xml;
				return taskDefinition;
			}
		}
		catch (Exception)
		{
		}
		return iTask.Definition;
	}

	internal static TimeSpan StringToTimeSpan(string input)
	{
		if (!string.IsNullOrEmpty(input))
		{
			try
			{
				return XmlConvert.ToTimeSpan(input);
			}
			catch
			{
			}
		}
		return TimeSpan.Zero;
	}

	internal static string TimeSpanToString(TimeSpan span)
	{
		if (span != TimeSpan.Zero)
		{
			try
			{
				return XmlConvert.ToString(span);
			}
			catch
			{
			}
		}
		return null;
	}

	internal void V1Reactivate()
	{
		ITask task = TaskService.GetTask(TaskService.v1TaskScheduler, Name);
		if (task != null)
		{
			v1Task = task;
		}
	}

	protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
