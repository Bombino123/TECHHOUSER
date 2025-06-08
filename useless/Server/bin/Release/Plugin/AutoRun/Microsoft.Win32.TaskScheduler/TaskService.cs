using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.Fluent;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[Description("Provides access to the Task Scheduler service.")]
[ToolboxItem(true)]
[ComVisible(true)]
public sealed class TaskService : Component, ISupportInitialize, ISerializable
{
	public delegate void ComHandlerUpdate(short percentage, string message);

	public struct ConnectionToken
	{
		internal int token;

		internal ConnectionToken(int value)
		{
			token = value;
		}
	}

	private static class ConnectionDataManager
	{
		public static List<ConnectionData> connections = new List<ConnectionData>
		{
			new ConnectionData(null)
		};

		public static TaskService InstanceFromToken(ConnectionToken token)
		{
			ConnectionData connectionData;
			lock (connections)
			{
				connectionData = connections[(token.token < connections.Count) ? token.token : 0];
			}
			return new TaskService(connectionData.TargetServer, connectionData.UserName, connectionData.UserAccountDomain, connectionData.UserPassword, connectionData.ForceV1);
		}

		public static ConnectionToken TokenFromInstance(string targetServer, string userName = null, string accountDomain = null, string password = null, bool forceV1 = false)
		{
			lock (connections)
			{
				ConnectionData connectionData = new ConnectionData(targetServer, userName, accountDomain, password, forceV1);
				for (int i = 0; i < connections.Count; i++)
				{
					if (connections[i].Equals(connectionData))
					{
						return new ConnectionToken(i);
					}
				}
				connections.Add(connectionData);
				return new ConnectionToken(connections.Count - 1);
			}
		}
	}

	private class ComHandlerThread
	{
		private class TaskHandlerStatus : ITaskHandlerStatus
		{
			private readonly Action<int> OnCompleted;

			private readonly ComHandlerUpdate OnUpdate;

			public TaskHandlerStatus(Action<int> onCompleted, ComHandlerUpdate onUpdate)
			{
				OnCompleted = onCompleted;
				OnUpdate = onUpdate;
			}

			public void TaskCompleted([In][MarshalAs(UnmanagedType.Error)] int taskErrCode)
			{
				OnCompleted?.Invoke(taskErrCode);
			}

			public void UpdateStatus([In] short percentComplete, [In][MarshalAs(UnmanagedType.BStr)] string statusMessage)
			{
				OnUpdate?.Invoke(percentComplete, statusMessage);
			}
		}

		public int ReturnCode;

		private readonly AutoResetEvent completed = new AutoResetEvent(initialState: false);

		private readonly string Data;

		private readonly Type objType;

		private readonly TaskHandlerStatus status;

		private readonly int Timeout;

		public ComHandlerThread(Guid clsid, string data, int millisecondsTimeout, ComHandlerUpdate onUpdate, Action<int> onComplete)
		{
			ComHandlerThread comHandlerThread = this;
			objType = Type.GetTypeFromCLSID(clsid, throwOnError: true);
			Data = data;
			Timeout = millisecondsTimeout;
			status = new TaskHandlerStatus(delegate(int i)
			{
				comHandlerThread.completed.Set();
				onComplete?.Invoke(i);
			}, onUpdate);
		}

		public Thread Start()
		{
			Thread thread = new Thread(ThreadProc);
			thread.Start();
			return thread;
		}

		private void ThreadProc()
		{
			completed.Reset();
			object obj = null;
			try
			{
				obj = Activator.CreateInstance(objType);
			}
			catch
			{
			}
			if (obj == null)
			{
				return;
			}
			ITaskHandler taskHandler = null;
			try
			{
				taskHandler = (ITaskHandler)obj;
			}
			catch
			{
			}
			try
			{
				if (taskHandler != null)
				{
					taskHandler.Start(status, Data);
					completed.WaitOne(Timeout);
					taskHandler.Stop(out ReturnCode);
				}
			}
			finally
			{
				if (taskHandler != null)
				{
					Marshal.ReleaseComObject(taskHandler);
				}
				Marshal.ReleaseComObject(obj);
			}
		}
	}

	private class ConnectionData : IEquatable<ConnectionData>
	{
		public bool ForceV1;

		public string TargetServer;

		public string UserAccountDomain;

		public string UserName;

		public string UserPassword;

		public ConnectionData(string targetServer, string userName = null, string accountDomain = null, string password = null, bool forceV1 = false)
		{
			TargetServer = targetServer;
			UserAccountDomain = accountDomain;
			UserName = userName;
			UserPassword = password;
			ForceV1 = forceV1;
		}

		public bool Equals(ConnectionData other)
		{
			if (string.Equals(TargetServer, other.TargetServer, StringComparison.InvariantCultureIgnoreCase) && string.Equals(UserAccountDomain, other.UserAccountDomain, StringComparison.InvariantCultureIgnoreCase) && string.Equals(UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(UserPassword, other.UserPassword, StringComparison.InvariantCultureIgnoreCase))
			{
				return ForceV1 == other.ForceV1;
			}
			return false;
		}
	}

	private class VersionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (!(value is string version))
			{
				return base.ConvertFrom(context, culture, value);
			}
			return new Version(version);
		}
	}

	internal static readonly bool LibraryIsV2 = Environment.OSVersion.Version.Major >= 6;

	internal static readonly Guid PowerShellActionGuid = new Guid("dab4c1e3-cd12-46f1-96fc-3981143c9bab");

	private static Guid CLSID_Ctask = typeof(CTask).GUID;

	private static Guid IID_ITask = typeof(ITask).GUID;

	[ThreadStatic]
	private static TaskService instance;

	private static Version osLibVer;

	internal ITaskScheduler v1TaskScheduler;

	internal ITaskService v2TaskService;

	private bool connecting;

	private bool forceV1;

	private bool initializing;

	private Version maxVer;

	private bool maxVerSet;

	private string targetServer;

	private bool targetServerSet;

	private string userDomain;

	private bool userDomainSet;

	private string userName;

	private bool userNameSet;

	private string userPassword;

	private bool userPasswordSet;

	private WindowsImpersonatedIdentity v1Impersonation;

	public static TaskService Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new TaskService();
				instance.ServiceDisconnected += Instance_ServiceDisconnected;
			}
			return instance;
		}
	}

	[Browsable(false)]
	public static Version LibraryVersion { get; } = Instance.HighestSupportedVersion;


	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Allow tasks from later OS versions with new properties to be retrieved as read only tasks.")]
	public bool AllowReadOnlyTasks { get; set; }

	[Browsable(false)]
	[DefaultValue(null)]
	[Obsolete("This property has been superseded by the UserAccountDomin property and may not be available in future releases.")]
	public string ConnectedDomain
	{
		get
		{
			if (v2TaskService != null)
			{
				return v2TaskService.ConnectedDomain;
			}
			string[] array = v1Impersonation.Name.Split(new char[1] { '\\' });
			if (array.Length == 2)
			{
				return array[0];
			}
			return string.Empty;
		}
	}

	[Browsable(false)]
	[DefaultValue(null)]
	[Obsolete("This property has been superseded by the UserName property and may not be available in future releases.")]
	public string ConnectedUser
	{
		get
		{
			if (v2TaskService != null)
			{
				return v2TaskService.ConnectedUser;
			}
			string[] array = v1Impersonation.Name.Split(new char[1] { '\\' });
			if (array.Length == 2)
			{
				return array[1];
			}
			return array[0];
		}
	}

	[Category("Data")]
	[TypeConverter(typeof(VersionConverter))]
	[Description("Highest version of library that should be used.")]
	public Version HighestSupportedVersion
	{
		get
		{
			return maxVer;
		}
		set
		{
			if (value > GetLibraryVersionFromLocalOS())
			{
				throw new ArgumentOutOfRangeException("HighestSupportedVersion", "The value of HighestSupportedVersion cannot exceed that of the underlying Windows version library.");
			}
			maxVer = value;
			maxVerSet = true;
			bool flag = value <= TaskServiceVersion.V1_1;
			if (flag != forceV1)
			{
				forceV1 = flag;
				Connect();
			}
		}
	}

	[Browsable(false)]
	public TaskFolder RootFolder => GetFolder("\\");

	[Category("Data")]
	[DefaultValue(null)]
	[Description("The name of the computer to connect to.")]
	public string TargetServer
	{
		get
		{
			if (!ShouldSerializeTargetServer())
			{
				return null;
			}
			return targetServer;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, targetServer, StringComparison.OrdinalIgnoreCase) != 0)
			{
				targetServerSet = true;
				targetServer = value;
				Connect();
			}
		}
	}

	[Category("Data")]
	[DefaultValue(null)]
	[Description("The user account domain to be used when connecting.")]
	public string UserAccountDomain
	{
		get
		{
			if (!ShouldSerializeUserAccountDomain())
			{
				return null;
			}
			return userDomain;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, userDomain, StringComparison.OrdinalIgnoreCase) != 0)
			{
				userDomainSet = true;
				userDomain = value;
				Connect();
			}
		}
	}

	[Category("Data")]
	[DefaultValue(null)]
	[Description("The user name to be used when connecting.")]
	public string UserName
	{
		get
		{
			if (!ShouldSerializeUserName())
			{
				return null;
			}
			return userName;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, userName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				userNameSet = true;
				userName = value;
				Connect();
			}
		}
	}

	[Category("Data")]
	[DefaultValue(null)]
	[Description("The user password to be used when connecting.")]
	public string UserPassword
	{
		get
		{
			return userPassword;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.CompareOrdinal(value, userPassword) != 0)
			{
				userPasswordSet = true;
				userPassword = value;
				Connect();
			}
		}
	}

	[Browsable(false)]
	public IEnumerable<Task> AllTasks => RootFolder.AllTasks;

	[Browsable(false)]
	public bool Connected
	{
		get
		{
			if (v2TaskService == null || !v2TaskService.Connected)
			{
				return v1TaskScheduler != null;
			}
			return true;
		}
	}

	public ConnectionToken Token => ConnectionDataManager.TokenFromInstance(TargetServer, UserName, UserAccountDomain, UserPassword, forceV1);

	protected override bool CanRaiseEvents { get; }

	public event EventHandler ServiceConnected;

	public event EventHandler ServiceDisconnected;

	public TaskService()
	{
		ResetHighestSupportedVersion();
		Connect();
	}

	public TaskService(string targetServer, string userName = null, string accountDomain = null, string password = null, bool forceV1 = false)
	{
		BeginInit();
		TargetServer = targetServer;
		UserName = userName;
		UserAccountDomain = accountDomain;
		UserPassword = password;
		this.forceV1 = forceV1;
		ResetHighestSupportedVersion();
		EndInit();
	}

	private TaskService([NotNull] SerializationInfo info, StreamingContext context)
	{
		BeginInit();
		TargetServer = (string)info.GetValue("TargetServer", typeof(string));
		UserName = (string)info.GetValue("UserName", typeof(string));
		UserAccountDomain = (string)info.GetValue("UserAccountDomain", typeof(string));
		UserPassword = (string)info.GetValue("UserPassword", typeof(string));
		forceV1 = (bool)info.GetValue("forceV1", typeof(bool));
		ResetHighestSupportedVersion();
		EndInit();
	}

	public static TaskService CreateFromToken(ConnectionToken token)
	{
		return ConnectionDataManager.InstanceFromToken(token);
	}

	public static string GetDllResourceString([NotNull] string dllPath, int resourceId)
	{
		return $"$(@ {dllPath}, {resourceId})";
	}

	public static int RunComHandlerAction(Guid clsid, string data = null, int millisecondsTimeout = -1, ComHandlerUpdate onUpdate = null)
	{
		ComHandlerThread comHandlerThread = new ComHandlerThread(clsid, data, millisecondsTimeout, onUpdate, null);
		comHandlerThread.Start().Join();
		return comHandlerThread.ReturnCode;
	}

	public static void RunComHandlerActionAsync(Guid clsid, Action<int> onComplete, string data = null, int millisecondsTimeout = -1, ComHandlerUpdate onUpdate = null)
	{
		new ComHandlerThread(clsid, data, millisecondsTimeout, onUpdate, onComplete).Start();
	}

	public Task AddAutomaticMaintenanceTask([NotNull] string taskPathAndName, TimeSpan period, TimeSpan deadline, string executablePath, string arguments = null, string workingDirectory = null)
	{
		if (HighestSupportedVersion.Minor < 4)
		{
			throw new InvalidOperationException("Automatic Maintenance tasks are only supported on Windows 8/Server 2012 and later.");
		}
		TaskDefinition taskDefinition = NewTask();
		taskDefinition.Settings.UseUnifiedSchedulingEngine = true;
		taskDefinition.Settings.MaintenanceSettings.Period = period;
		taskDefinition.Settings.MaintenanceSettings.Deadline = deadline;
		taskDefinition.Actions.Add(executablePath, arguments, workingDirectory);
		return RootFolder.RegisterTaskDefinition(taskPathAndName, taskDefinition, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, "D:P(A;;FA;;;BA)(A;;FA;;;SY)(A;;FRFX;;;LS)");
	}

	public Task AddTask([NotNull] string path, [NotNull] Trigger trigger, [NotNull] Action action, string userId = null, string password = null, TaskLogonType logonType = TaskLogonType.InteractiveToken, string description = null)
	{
		TaskDefinition taskDefinition = NewTask();
		if (!string.IsNullOrEmpty(description))
		{
			taskDefinition.RegistrationInfo.Description = description;
		}
		taskDefinition.Triggers.Add(trigger);
		taskDefinition.Actions.Add(action);
		return RootFolder.RegisterTaskDefinition(path, taskDefinition, TaskCreation.CreateOrUpdate, userId, password, logonType);
	}

	public Task AddTask([NotNull] string path, QuickTriggerType trigger, [NotNull] string exePath, string arguments = null, string userId = null, string password = null, TaskLogonType logonType = TaskLogonType.InteractiveToken, string description = null)
	{
		return AddTask(path, trigger switch
		{
			QuickTriggerType.Boot => new BootTrigger(), 
			QuickTriggerType.Idle => new IdleTrigger(), 
			QuickTriggerType.Logon => new LogonTrigger(), 
			QuickTriggerType.TaskRegistration => new RegistrationTrigger(), 
			QuickTriggerType.Hourly => new DailyTrigger(1)
			{
				Repetition = new RepetitionPattern(TimeSpan.FromHours(1.0), TimeSpan.FromDays(1.0))
			}, 
			QuickTriggerType.Daily => new DailyTrigger(1), 
			QuickTriggerType.Weekly => new WeeklyTrigger(DaysOfTheWeek.Sunday, 1), 
			QuickTriggerType.Monthly => new MonthlyTrigger(), 
			_ => throw new ArgumentOutOfRangeException("trigger", trigger, null), 
		}, new ExecAction(exePath, arguments), userId, password, logonType, description);
	}

	public void BeginInit()
	{
		initializing = true;
	}

	public void EndInit()
	{
		initializing = false;
		Connect();
	}

	public override bool Equals(object obj)
	{
		if (obj is TaskService taskService)
		{
			if (taskService.TargetServer == TargetServer && taskService.UserAccountDomain == UserAccountDomain && taskService.UserName == UserName && taskService.UserPassword == UserPassword)
			{
				return taskService.forceV1 == forceV1;
			}
			return false;
		}
		return base.Equals(obj);
	}

	public Task[] FindAllTasks(Regex name, bool searchAllFolders = true)
	{
		List<Task> results = new List<Task>();
		FindTaskInFolder(RootFolder, name, ref results, searchAllFolders);
		return results.ToArray();
	}

	public Task[] FindAllTasks(Predicate<Task> filter, bool searchAllFolders = true)
	{
		if (filter == null)
		{
			filter = (Task t) => true;
		}
		List<Task> results = new List<Task>();
		FindTaskInFolder(RootFolder, filter, ref results, searchAllFolders);
		return results.ToArray();
	}

	public Task FindTask([NotNull] string name, bool searchAllFolders = true)
	{
		Task[] array = FindAllTasks(new Wildcard(name), searchAllFolders);
		if (array.Length != 0)
		{
			return array[0];
		}
		return null;
	}

	public TaskEventLog GetEventLog(string taskPath = null)
	{
		return new TaskEventLog(TargetServer, taskPath, UserAccountDomain, UserName, UserPassword);
	}

	public TaskFolder GetFolder(string folderName)
	{
		TaskFolder result = null;
		if (v2TaskService != null)
		{
			if (string.IsNullOrEmpty(folderName))
			{
				folderName = "\\";
			}
			try
			{
				ITaskFolder folder = v2TaskService.GetFolder(folderName);
				if (folder != null)
				{
					result = new TaskFolder(this, folder);
				}
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (FileNotFoundException)
			{
			}
		}
		else
		{
			if (!(folderName == "\\") && !string.IsNullOrEmpty(folderName))
			{
				throw new NotV1SupportedException("Folder other than the root (\\) was requested on a system only supporting Task Scheduler 1.0.");
			}
			result = new TaskFolder(this);
		}
		return result;
	}

	public override int GetHashCode()
	{
		return new
		{
			A = TargetServer,
			B = UserAccountDomain,
			C = UserName,
			D = UserPassword,
			E = forceV1
		}.GetHashCode();
	}

	public RunningTaskCollection GetRunningTasks(bool includeHidden = true)
	{
		if (v2TaskService != null)
		{
			try
			{
				return new RunningTaskCollection(this, v2TaskService.GetRunningTasks(includeHidden ? 1 : 0));
			}
			catch
			{
			}
		}
		return new RunningTaskCollection(this);
	}

	public Task GetTask([NotNull] string taskPath)
	{
		Task result = null;
		if (v2TaskService != null)
		{
			IRegisteredTask task = GetTask(v2TaskService, taskPath);
			if (task != null)
			{
				result = Task.CreateTask(this, task);
			}
		}
		else
		{
			taskPath = Path.GetFileNameWithoutExtension(taskPath);
			ITask task2 = GetTask(v1TaskScheduler, taskPath);
			if (task2 != null)
			{
				result = new Task(this, task2);
			}
		}
		return result;
	}

	public TaskDefinition NewTask()
	{
		if (v2TaskService != null)
		{
			return new TaskDefinition(v2TaskService.NewTask(0u));
		}
		string text = "Temp" + Guid.NewGuid().ToString("B");
		return new TaskDefinition(v1TaskScheduler.NewWorkItem(text, CLSID_Ctask, IID_ITask), text);
	}

	public TaskDefinition NewTaskFromFile([NotNull] string xmlFile)
	{
		TaskDefinition taskDefinition = NewTask();
		taskDefinition.XmlText = File.ReadAllText(xmlFile);
		return taskDefinition;
	}

	public void StartSystemTaskSchedulerManager()
	{
		if (Environment.UserInteractive)
		{
			Process.Start("control.exe", "schedtasks");
		}
	}

	[SecurityCritical]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TargetServer", TargetServer, typeof(string));
		info.AddValue("UserName", UserName, typeof(string));
		info.AddValue("UserAccountDomain", UserAccountDomain, typeof(string));
		info.AddValue("UserPassword", UserPassword, typeof(string));
		info.AddValue("forceV1", forceV1, typeof(bool));
	}

	internal static IRegisteredTask GetTask([NotNull] ITaskService iSvc, [NotNull] string name)
	{
		ITaskFolder taskFolder = null;
		try
		{
			taskFolder = iSvc.GetFolder("\\");
			return taskFolder.GetTask(name);
		}
		catch
		{
			return null;
		}
		finally
		{
			if (taskFolder != null)
			{
				Marshal.ReleaseComObject(taskFolder);
			}
		}
	}

	internal static ITask GetTask([NotNull] ITaskScheduler iSvc, [NotNull] string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		try
		{
			return iSvc.Activate(name, IID_ITask);
		}
		catch (UnauthorizedAccessException)
		{
			throw;
		}
		catch (ArgumentException)
		{
			return iSvc.Activate(name + ".job", IID_ITask);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (v2TaskService != null)
		{
			try
			{
				Marshal.ReleaseComObject(v2TaskService);
			}
			catch
			{
			}
			v2TaskService = null;
		}
		if (v1TaskScheduler != null)
		{
			try
			{
				Marshal.ReleaseComObject(v1TaskScheduler);
			}
			catch
			{
			}
			v1TaskScheduler = null;
		}
		if (v1Impersonation != null)
		{
			v1Impersonation.Dispose();
			v1Impersonation = null;
		}
		if (!connecting)
		{
			this.ServiceDisconnected?.Invoke(this, EventArgs.Empty);
		}
		base.Dispose(disposing);
	}

	private static Version GetLibraryVersionFromLocalOS()
	{
		if (osLibVer == null)
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				osLibVer = TaskServiceVersion.V1_1;
			}
			else if (Environment.OSVersion.Version.Minor == 0)
			{
				osLibVer = TaskServiceVersion.V1_2;
			}
			else if (Environment.OSVersion.Version.Minor == 1)
			{
				osLibVer = TaskServiceVersion.V1_3;
			}
			else
			{
				try
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "taskschd.dll"));
					if (versionInfo.FileBuildPart > 9600 && versionInfo.FileBuildPart <= 14393)
					{
						osLibVer = TaskServiceVersion.V1_5;
					}
					else if (versionInfo.FileBuildPart >= 15063)
					{
						osLibVer = TaskServiceVersion.V1_6;
					}
					else
					{
						osLibVer = TaskServiceVersion.V1_4;
					}
				}
				catch
				{
				}
			}
			if (osLibVer == null)
			{
				throw new NotSupportedException("The Task Scheduler library version for this system cannot be determined.");
			}
		}
		return osLibVer;
	}

	private static void Instance_ServiceDisconnected(object sender, EventArgs e)
	{
		instance?.Connect();
	}

	private void Connect()
	{
		ResetUnsetProperties();
		if (initializing || base.DesignMode)
		{
			return;
		}
		if ((!string.IsNullOrEmpty(userDomain) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userPassword)) || (string.IsNullOrEmpty(userDomain) && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userPassword)))
		{
			connecting = true;
			Dispose(disposing: true);
			if (LibraryIsV2 && !forceV1)
			{
				v2TaskService = (ITaskService)new TaskSchedulerClass();
				if (!string.IsNullOrEmpty(targetServer))
				{
					if (targetServer.StartsWith("\\"))
					{
						targetServer = targetServer.TrimStart(new char[1] { '\\' });
					}
					if (targetServer.Equals(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase))
					{
						targetServer = null;
					}
				}
				else
				{
					targetServer = null;
				}
				v2TaskService.Connect(targetServer, userName, userDomain, userPassword);
				targetServer = v2TaskService.TargetServer;
				userName = v2TaskService.ConnectedUser;
				userDomain = v2TaskService.ConnectedDomain;
				maxVer = GetV2Version();
			}
			else
			{
				v1Impersonation = new WindowsImpersonatedIdentity(userName, userDomain, userPassword);
				v1TaskScheduler = (ITaskScheduler)new CTaskScheduler();
				if (!string.IsNullOrEmpty(targetServer))
				{
					if (!targetServer.StartsWith("\\\\"))
					{
						targetServer = "\\\\" + targetServer;
					}
				}
				else
				{
					targetServer = null;
				}
				v1TaskScheduler.SetTargetComputer(targetServer);
				targetServer = v1TaskScheduler.GetTargetComputer();
				maxVer = TaskServiceVersion.V1_1;
			}
			this.ServiceConnected?.Invoke(this, EventArgs.Empty);
			connecting = false;
			return;
		}
		throw new ArgumentException("A username, password, and domain must be provided.");
	}

	private bool FindTaskInFolder([NotNull] TaskFolder fld, Regex taskName, ref List<Task> results, bool recurse = true)
	{
		results.AddRange(fld.GetTasks(taskName));
		if (recurse)
		{
			foreach (TaskFolder subFolder in fld.SubFolders)
			{
				if (FindTaskInFolder(subFolder, taskName, ref results))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool FindTaskInFolder([NotNull] TaskFolder fld, Predicate<Task> filter, ref List<Task> results, bool recurse = true)
	{
		foreach (Task task in fld.GetTasks())
		{
			try
			{
				if (filter(task))
				{
					results.Add(task);
				}
			}
			catch
			{
			}
		}
		if (recurse)
		{
			foreach (TaskFolder subFolder in fld.SubFolders)
			{
				if (FindTaskInFolder(subFolder, filter, ref results))
				{
					return true;
				}
			}
		}
		return false;
	}

	private Version GetV2Version()
	{
		uint highestVersion = v2TaskService.HighestVersion;
		return new Version((int)(highestVersion >> 16), (int)(highestVersion & 0xFFFF));
	}

	private void ResetHighestSupportedVersion()
	{
		maxVer = ((!Connected) ? GetLibraryVersionFromLocalOS() : ((v2TaskService != null) ? GetV2Version() : TaskServiceVersion.V1_1));
	}

	private void ResetUnsetProperties()
	{
		if (!maxVerSet)
		{
			ResetHighestSupportedVersion();
		}
		if (!targetServerSet)
		{
			targetServer = null;
		}
		if (!userDomainSet)
		{
			userDomain = null;
		}
		if (!userNameSet)
		{
			userName = null;
		}
		if (!userPasswordSet)
		{
			userPassword = null;
		}
	}

	private bool ShouldSerializeHighestSupportedVersion()
	{
		if (LibraryIsV2)
		{
			return maxVer <= TaskServiceVersion.V1_1;
		}
		return false;
	}

	private bool ShouldSerializeTargetServer()
	{
		if (targetServer != null)
		{
			return !targetServer.Trim(new char[1] { '\\' }).Equals(Environment.MachineName.Trim(new char[1] { '\\' }), StringComparison.InvariantCultureIgnoreCase);
		}
		return false;
	}

	private bool ShouldSerializeUserAccountDomain()
	{
		if (userDomain != null)
		{
			return !userDomain.Equals(Environment.UserDomainName, StringComparison.InvariantCultureIgnoreCase);
		}
		return false;
	}

	private bool ShouldSerializeUserName()
	{
		if (userName != null)
		{
			return !userName.Equals(Environment.UserName, StringComparison.InvariantCultureIgnoreCase);
		}
		return false;
	}

	public ActionBuilder Execute([NotNull] string path)
	{
		return new ActionBuilder(new BuilderInfo(this), path);
	}
}
