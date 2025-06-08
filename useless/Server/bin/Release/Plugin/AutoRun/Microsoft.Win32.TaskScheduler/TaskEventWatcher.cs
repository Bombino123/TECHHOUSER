using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

[Serializable]
[DefaultEvent("EventRecorded")]
[DefaultProperty("Folder")]
[ToolboxItem(true)]
[PublicAPI]
[ComVisible(true)]
public class TaskEventWatcher : Component, ISupportInitialize
{
	[Serializable]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[PublicAPI]
	public class EventFilter
	{
		private string filter = "*";

		private int[] ids;

		private int[] levels;

		private readonly TaskEventWatcher parent;

		[DefaultValue(null)]
		[Category("Filter")]
		[Description("An array of event identifiers to use when filtering.")]
		public int[] EventIds
		{
			get
			{
				return ids;
			}
			set
			{
				if (ids != value)
				{
					ids = value;
					parent.Restart();
				}
			}
		}

		[DefaultValue(null)]
		[Category("Filter")]
		[Description("An array of event levels to use when filtering.")]
		public int[] EventLevels
		{
			get
			{
				return levels;
			}
			set
			{
				if (levels != value)
				{
					levels = value;
					parent.Restart();
				}
			}
		}

		[DefaultValue("*")]
		[Category("Filter")]
		[Description("A task name, which can utilize wildcards, for filtering.")]
		public string TaskName
		{
			get
			{
				return filter;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					value = "*";
				}
				if (string.Compare(filter, value, StringComparison.OrdinalIgnoreCase) != 0)
				{
					filter = value;
					Wildcard = ((value.IndexOfAny(new char[2] { '?', '*' }) == -1) ? null : new Wildcard(value));
					parent.Restart();
				}
			}
		}

		internal Wildcard Wildcard { get; private set; } = new Wildcard("*");


		internal EventFilter([NotNull] TaskEventWatcher parent)
		{
			this.parent = parent;
		}

		public override string ToString()
		{
			return filter + ((levels == null) ? "" : " +levels") + ((ids == null) ? "" : " +id's");
		}

		internal bool ShouldSerialize()
		{
			if (ids == null && levels == null)
			{
				return filter != "*";
			}
			return true;
		}
	}

	private const string root = "\\";

	private const string star = "*";

	private static readonly TimeSpan MaxV1EventLapse = TimeSpan.FromSeconds(1.0);

	private bool disposed;

	private bool enabled;

	private string folder = "\\";

	private bool includeSubfolders;

	private bool initializing;

	private StandardTaskEventId lastId;

	private DateTime lastIdTime = DateTime.MinValue;

	private TaskService ts;

	private FileSystemWatcher v1Watcher;

	private EventLogWatcher watcher;

	private ISynchronizeInvoke synchronizingObject;

	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Indicates whether the component is enabled.")]
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (enabled == value)
			{
				return;
			}
			enabled = value;
			if (!IsSuspended())
			{
				if (enabled)
				{
					StartRaisingEvents();
				}
				else
				{
					StopRaisingEvents();
				}
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Behavior")]
	[Description("Indicates the filter for the watcher.")]
	public EventFilter Filter { get; }

	[DefaultValue("\\")]
	[Category("Behavior")]
	[Description("Indicates the folder to watch.")]
	public string Folder
	{
		get
		{
			return folder;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = "\\";
			}
			if (!value.EndsWith("\\"))
			{
				value += "\\";
			}
			if (string.Compare(folder, value, StringComparison.OrdinalIgnoreCase) != 0)
			{
				if ((base.DesignMode && (value.IndexOfAny(new char[2] { '*', '?' }) != -1 || value.IndexOfAny(Path.GetInvalidPathChars()) != -1)) || TaskService.GetFolder((value == "\\") ? value : value.TrimEnd(new char[1] { '\\' })) == null)
				{
					throw new ArgumentException("Invalid folder name: " + value);
				}
				folder = value;
			}
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Indicates whether to include events from subfolders.")]
	public bool IncludeSubfolders
	{
		get
		{
			return includeSubfolders;
		}
		set
		{
			if (includeSubfolders != value)
			{
				includeSubfolders = value;
				Restart();
			}
		}
	}

	[Browsable(false)]
	[DefaultValue(null)]
	public ISynchronizeInvoke SynchronizingObject
	{
		get
		{
			if (synchronizingObject == null && base.DesignMode && ((IDesignerHost)GetService(typeof(IDesignerHost)))?.RootComponent is ISynchronizeInvoke synchronizeInvoke)
			{
				synchronizingObject = synchronizeInvoke;
			}
			return synchronizingObject;
		}
		set
		{
			synchronizingObject = value;
		}
	}

	[Category("Connection")]
	[Description("The name of the computer to connect to.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string TargetServer
	{
		get
		{
			return TaskService.TargetServer;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, TaskService.TargetServer, StringComparison.OrdinalIgnoreCase) != 0)
			{
				TaskService.TargetServer = value;
				Restart();
			}
		}
	}

	[Category("Data")]
	[Description("The TaskService for this event watcher.")]
	public TaskService TaskService
	{
		get
		{
			return ts;
		}
		set
		{
			ts = value;
			Restart();
		}
	}

	[Category("Connection")]
	[Description("The user account domain to be used when connecting.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string UserAccountDomain
	{
		get
		{
			return TaskService.UserAccountDomain;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, TaskService.UserAccountDomain, StringComparison.OrdinalIgnoreCase) != 0)
			{
				TaskService.UserAccountDomain = value;
				Restart();
			}
		}
	}

	[Category("Connection")]
	[Description("The user name to be used when connecting.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string UserName
	{
		get
		{
			return TaskService.UserName;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, TaskService.UserName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				TaskService.UserName = value;
				Restart();
			}
		}
	}

	[Category("Connection")]
	[Description("The user password to be used when connecting.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string UserPassword
	{
		get
		{
			return TaskService.UserPassword;
		}
		set
		{
			if (value == null || value.Trim() == string.Empty)
			{
				value = null;
			}
			if (string.Compare(value, TaskService.UserPassword, StringComparison.OrdinalIgnoreCase) != 0)
			{
				TaskService.UserPassword = value;
				Restart();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	private bool IsHandleInvalid
	{
		get
		{
			if (!IsV1)
			{
				return watcher == null;
			}
			return v1Watcher == null;
		}
	}

	private static bool IsV1 => Environment.OSVersion.Version.Major < 6;

	[Category("Action")]
	[Description("Event recorded by a task or the task engine.")]
	public event EventHandler<TaskEventArgs> EventRecorded;

	public TaskEventWatcher()
		: this(TaskService.Instance)
	{
	}

	public TaskEventWatcher(string taskPath, TaskService taskService = null)
		: this(taskService ?? TaskService.Instance)
	{
		InitTask(taskPath);
	}

	public TaskEventWatcher([NotNull] Task task)
		: this(task?.TaskService)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		InitTask(task);
	}

	public TaskEventWatcher([NotNull] TaskFolder taskFolder, string taskFilter = "*", bool includeSubfolders = false)
		: this(taskFolder?.TaskService)
	{
		if (taskFolder == null)
		{
			throw new ArgumentNullException("taskFolder");
		}
		InitTask(taskFolder, taskFilter, includeSubfolders);
	}

	public TaskEventWatcher(string folder, string taskFilter, bool includeSubfolders, TaskService taskService = null)
		: this(taskService ?? TaskService.Instance)
	{
		InitTask(folder, taskFilter, includeSubfolders);
	}

	public TaskEventWatcher(string machineName, string taskPath, string domain = null, string user = null, string password = null)
		: this(new TaskService(machineName, user, domain, password))
	{
		InitTask(taskPath);
	}

	public TaskEventWatcher(string machineName, string folder, string taskFilter = "*", bool includeSubfolders = false, string domain = null, string user = null, string password = null)
		: this(new TaskService(machineName, user, domain, password))
	{
		InitTask(folder, taskFilter, includeSubfolders);
	}

	private TaskEventWatcher(TaskService ts)
	{
		TaskService = ts;
		Filter = new EventFilter(this);
	}

	public void BeginInit()
	{
		initializing = true;
		bool flag = enabled;
		StopRaisingEvents();
		enabled = flag;
		TaskService.BeginInit();
	}

	public void EndInit()
	{
		initializing = false;
		TaskService.EndInit();
		if (enabled)
		{
			StartRaisingEvents();
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				StopRaisingEvents();
				TaskService = null;
			}
			else
			{
				StopListening();
			}
		}
		finally
		{
			disposed = true;
			base.Dispose(disposing);
		}
	}

	protected virtual void OnEventRecorded(object sender, TaskEventArgs e)
	{
		EventHandler<TaskEventArgs> eventRecorded = this.EventRecorded;
		if (eventRecorded != null)
		{
			if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
			{
				SynchronizingObject.BeginInvoke(eventRecorded, new object[2] { this, e });
			}
			else
			{
				eventRecorded(sender, e);
			}
		}
	}

	private void InitTask([NotNull] Task task)
	{
		Filter.TaskName = task.Name;
		Folder = task.Folder.Path;
	}

	private void InitTask(TaskFolder taskFolder, string taskFilter, bool includeSubfolders)
	{
		this.includeSubfolders = includeSubfolders;
		Filter.TaskName = taskFilter;
		Folder = taskFolder?.Path;
	}

	private void InitTask(string taskFolder, string taskFilter, bool includeSubfolders)
	{
		this.includeSubfolders = includeSubfolders;
		Filter.TaskName = taskFilter;
		Folder = taskFolder;
	}

	private void InitTask(string taskPath)
	{
		Filter.TaskName = Path.GetFileNameWithoutExtension(taskPath);
		Folder = Path.GetDirectoryName(taskPath);
	}

	private bool IsSuspended()
	{
		if (!initializing)
		{
			return base.DesignMode;
		}
		return true;
	}

	private void ReleaseWatcher()
	{
		if (IsV1)
		{
			if (v1Watcher != null)
			{
				v1Watcher.EnableRaisingEvents = false;
				v1Watcher.Changed -= Watcher_DirectoryChanged;
				v1Watcher.Created -= Watcher_DirectoryChanged;
				v1Watcher.Deleted -= Watcher_DirectoryChanged;
				v1Watcher.Renamed -= Watcher_DirectoryChanged;
				v1Watcher = null;
			}
		}
		else if (watcher != null)
		{
			watcher.Enabled = false;
			watcher.EventRecordWritten -= Watcher_EventRecordWritten;
			watcher = null;
		}
	}

	private void ResetTaskService()
	{
		ts = TaskService.Instance;
	}

	private void Restart()
	{
		if (!IsSuspended() && enabled)
		{
			StopRaisingEvents();
			StartRaisingEvents();
		}
	}

	private void SetupWatcher()
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		ReleaseWatcher();
		string taskName = null;
		if (Filter.Wildcard == null)
		{
			taskName = Path.Combine(folder, Filter.TaskName);
		}
		if (IsV1)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.System));
			string path = ((directoryInfo.Parent != null) ? Path.Combine(directoryInfo.Parent.FullName, "Tasks") : "Tasks");
			v1Watcher = new FileSystemWatcher(path, "*.job")
			{
				NotifyFilter = (NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite)
			};
			v1Watcher.Changed += Watcher_DirectoryChanged;
			v1Watcher.Created += Watcher_DirectoryChanged;
			v1Watcher.Deleted += Watcher_DirectoryChanged;
			v1Watcher.Renamed += Watcher_DirectoryChanged;
		}
		else
		{
			TaskEventLog taskEventLog = new TaskEventLog(taskName, Filter.EventIds, Filter.EventLevels, DateTime.UtcNow, TargetServer, UserAccountDomain, UserName, UserPassword);
			taskEventLog.Query.ReverseDirection = false;
			watcher = new EventLogWatcher(taskEventLog.Query);
			watcher.EventRecordWritten += Watcher_EventRecordWritten;
		}
	}

	private bool ShouldSerializeFilter()
	{
		return Filter.ShouldSerialize();
	}

	private bool ShouldSerializeTaskService()
	{
		return !object.Equals(TaskService, TaskService.Instance);
	}

	private void StartRaisingEvents()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
		if (IsSuspended())
		{
			return;
		}
		enabled = true;
		SetupWatcher();
		if (IsV1)
		{
			try
			{
				v1Watcher.EnableRaisingEvents = true;
				return;
			}
			catch
			{
				return;
			}
		}
		try
		{
			watcher.Enabled = true;
		}
		catch
		{
		}
	}

	private void StopListening()
	{
		enabled = false;
		ReleaseWatcher();
	}

	private void StopRaisingEvents()
	{
		if (IsSuspended())
		{
			enabled = false;
		}
		else if (!IsHandleInvalid)
		{
			StopListening();
		}
	}

	private void Watcher_DirectoryChanged(object sender, FileSystemEventArgs e)
	{
		StandardTaskEventId standardTaskEventId = StandardTaskEventId.TaskUpdated;
		if (e.ChangeType == WatcherChangeTypes.Deleted)
		{
			standardTaskEventId = StandardTaskEventId.TaskDeleted;
		}
		else if (e.ChangeType == WatcherChangeTypes.Created)
		{
			standardTaskEventId = StandardTaskEventId.JobRegistered;
		}
		if (lastId != standardTaskEventId || !(DateTime.Now.Subtract(lastIdTime) <= MaxV1EventLapse))
		{
			OnEventRecorded(this, new TaskEventArgs(new TaskEvent(Path.Combine("\\", e.Name.Replace(".job", "")), standardTaskEventId, DateTime.Now), TaskService));
			lastId = standardTaskEventId;
			lastIdTime = DateTime.Now;
		}
	}

	private void Watcher_EventRecordWritten(object sender, EventRecordWrittenEventArgs e)
	{
		try
		{
			TaskEvent taskEvent = new TaskEvent(e.EventRecord);
			if (!string.IsNullOrEmpty(taskEvent.TaskPath))
			{
				int num = taskEvent.TaskPath.LastIndexOf('\\');
				string input = taskEvent.TaskPath.Substring(num + 1);
				string text = taskEvent.TaskPath.Substring(0, num + 1);
				if (string.IsNullOrEmpty(Filter.TaskName) || string.Compare(Filter.TaskName, taskEvent.TaskPath, StringComparison.OrdinalIgnoreCase) == 0 || ((Filter.Wildcard == null || Filter.Wildcard.IsMatch(input)) && (!IncludeSubfolders || text.StartsWith(folder, StringComparison.OrdinalIgnoreCase)) && (IncludeSubfolders || string.Compare(folder, text, StringComparison.OrdinalIgnoreCase) == 0)))
				{
					OnEventRecorded(this, new TaskEventArgs(taskEvent, TaskService));
				}
			}
		}
		catch (Exception)
		{
		}
	}
}
