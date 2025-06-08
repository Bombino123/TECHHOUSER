using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public sealed class TaskFolder : IDisposable, IComparable<TaskFolder>
{
	private ITaskScheduler v1List;

	private readonly ITaskFolder v2Folder;

	internal const string rootString = "\\";

	[NotNull]
	[ItemNotNull]
	public IEnumerable<Task> AllTasks => EnumerateFolderTasks(this);

	[NotNull]
	public string Name
	{
		get
		{
			if (v2Folder != null)
			{
				return v2Folder.Name;
			}
			return "\\";
		}
	}

	public TaskFolder Parent
	{
		get
		{
			if (v2Folder == null)
			{
				return null;
			}
			string directoryName = System.IO.Path.GetDirectoryName(v2Folder.Path);
			if (string.IsNullOrEmpty(directoryName))
			{
				return null;
			}
			return TaskService.GetFolder(directoryName);
		}
	}

	[NotNull]
	public string Path
	{
		get
		{
			if (v2Folder != null)
			{
				return v2Folder.Path;
			}
			return "\\";
		}
	}

	[Obsolete("This property will be removed in deference to the GetAccessControl, GetSecurityDescriptorSddlForm, SetAccessControl and SetSecurityDescriptorSddlForm methods.")]
	public GenericSecurityDescriptor SecurityDescriptor
	{
		get
		{
			return GetSecurityDescriptor();
		}
		set
		{
			SetSecurityDescriptor(value);
		}
	}

	[NotNull]
	[ItemNotNull]
	public TaskFolderCollection SubFolders
	{
		get
		{
			try
			{
				if (v2Folder != null)
				{
					return new TaskFolderCollection(this, v2Folder.GetFolders(0));
				}
			}
			catch
			{
			}
			return new TaskFolderCollection();
		}
	}

	[NotNull]
	[ItemNotNull]
	public TaskCollection Tasks => GetTasks();

	public TaskService TaskService { get; }

	internal TaskFolder([NotNull] TaskService svc)
	{
		TaskService = svc;
		v1List = svc.v1TaskScheduler;
	}

	internal TaskFolder([NotNull] TaskService svc, [NotNull] ITaskFolder iFldr)
	{
		TaskService = svc;
		v2Folder = iFldr;
	}

	public void Dispose()
	{
		if (v2Folder != null)
		{
			Marshal.ReleaseComObject(v2Folder);
		}
		v1List = null;
	}

	[NotNull]
	internal TaskFolder GetFolder([NotNull] string path)
	{
		if (v2Folder != null)
		{
			return new TaskFolder(TaskService, v2Folder.GetFolder(path));
		}
		throw new NotV1SupportedException();
	}

	int IComparable<TaskFolder>.CompareTo(TaskFolder other)
	{
		return string.Compare(Path, other.Path, ignoreCase: true);
	}

	[Obsolete("This method will be removed in deference to the CreateFolder(string, TaskSecurity) method.")]
	public TaskFolder CreateFolder([NotNull] string subFolderName, GenericSecurityDescriptor sd)
	{
		return CreateFolder(subFolderName, sd?.GetSddlForm(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group));
	}

	public TaskFolder CreateFolder([NotNull] string subFolderName, [NotNull] TaskSecurity folderSecurity)
	{
		if (folderSecurity == null)
		{
			throw new ArgumentNullException("folderSecurity");
		}
		return CreateFolder(subFolderName, folderSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group));
	}

	public TaskFolder CreateFolder([NotNull] string subFolderName, string sddlForm = null, bool exceptionOnExists = true)
	{
		if (v2Folder == null)
		{
			throw new NotV1SupportedException();
		}
		ITaskFolder taskFolder = null;
		try
		{
			taskFolder = v2Folder.CreateFolder(subFolderName, sddlForm);
		}
		catch (COMException ex)
		{
			switch (ex.ErrorCode & 0xFFFF)
			{
			case 183:
				if (exceptionOnExists)
				{
					throw;
				}
				try
				{
					taskFolder = v2Folder.GetFolder(subFolderName);
					if (taskFolder != null && sddlForm != null && sddlForm.Trim().Length > 0)
					{
						string securityDescriptor = taskFolder.GetSecurityDescriptor(7);
						if (string.Compare(sddlForm, securityDescriptor, StringComparison.OrdinalIgnoreCase) != 0)
						{
							throw new SecurityException("Security descriptor mismatch between specified credentials and credentials on existing folder by same name.");
						}
					}
				}
				catch
				{
					if (taskFolder != null)
					{
						Marshal.ReleaseComObject(taskFolder);
					}
					throw;
				}
				break;
			case 87:
			case 1305:
			case 1332:
			case 1336:
			case 1337:
			case 1338:
				throw new ArgumentException("Invalid SDDL form", "sddlForm", ex);
			default:
				throw;
			}
		}
		return new TaskFolder(TaskService, taskFolder);
	}

	public void DeleteFolder([NotNull] string subFolderName, bool exceptionOnNotExists = true)
	{
		if (v2Folder != null)
		{
			try
			{
				v2Folder.DeleteFolder(subFolderName, 0);
				return;
			}
			catch (Exception ex)
			{
				if ((!(ex is FileNotFoundException) && !(ex is DirectoryNotFoundException)) || exceptionOnNotExists)
				{
					throw;
				}
				return;
			}
		}
		throw new NotV1SupportedException();
	}

	public void DeleteTask([NotNull] string name, bool exceptionOnNotExists = true)
	{
		try
		{
			if (v2Folder != null)
			{
				v2Folder.DeleteTask(name, 0);
				return;
			}
			if (!name.EndsWith(".job", StringComparison.CurrentCultureIgnoreCase))
			{
				name += ".job";
			}
			v1List.Delete(name);
		}
		catch (FileNotFoundException)
		{
			if (exceptionOnNotExists)
			{
				throw;
			}
		}
	}

	[NotNull]
	[ItemNotNull]
	public IEnumerable<TaskFolder> EnumerateFolders(Predicate<TaskFolder> filter = null)
	{
		foreach (TaskFolder subFolder in SubFolders)
		{
			if (filter == null || filter(subFolder))
			{
				yield return subFolder;
			}
		}
	}

	[NotNull]
	[ItemNotNull]
	public IEnumerable<Task> EnumerateTasks(Predicate<Task> filter = null, bool recurse = false)
	{
		return EnumerateFolderTasks(this, filter, recurse);
	}

	public override bool Equals(object obj)
	{
		if (obj is TaskFolder taskFolder)
		{
			if (Path == taskFolder.Path && TaskService.TargetServer == taskFolder.TaskService.TargetServer)
			{
				return GetSecurityDescriptorSddlForm() == taskFolder.GetSecurityDescriptorSddlForm();
			}
			return false;
		}
		return false;
	}

	[NotNull]
	public TaskSecurity GetAccessControl()
	{
		return GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	[NotNull]
	public TaskSecurity GetAccessControl(AccessControlSections includeSections)
	{
		return new TaskSecurity(this, includeSections);
	}

	public override int GetHashCode()
	{
		return new
		{
			A = Path,
			B = TaskService.TargetServer,
			C = GetSecurityDescriptorSddlForm()
		}.GetHashCode();
	}

	[Obsolete("This method will be removed in deference to the GetAccessControl and GetSecurityDescriptorSddlForm methods.")]
	public GenericSecurityDescriptor GetSecurityDescriptor(SecurityInfos includeSections = SecurityInfos.Owner | SecurityInfos.Group | SecurityInfos.DiscretionaryAcl)
	{
		return new RawSecurityDescriptor(GetSecurityDescriptorSddlForm(includeSections));
	}

	public string GetSecurityDescriptorSddlForm(SecurityInfos includeSections = SecurityInfos.Owner | SecurityInfos.Group | SecurityInfos.DiscretionaryAcl)
	{
		if (v2Folder != null)
		{
			return v2Folder.GetSecurityDescriptor((int)includeSections);
		}
		throw new NotV1SupportedException();
	}

	[NotNull]
	[ItemNotNull]
	public TaskCollection GetTasks(Regex filter = null)
	{
		if (v2Folder != null)
		{
			return new TaskCollection(this, v2Folder.GetTasks(1), filter);
		}
		return new TaskCollection(TaskService, filter);
	}

	public Task ImportTask(string path, [NotNull] string xmlFile, bool overwriteExisting = true)
	{
		return RegisterTask(path, File.ReadAllText(xmlFile), overwriteExisting ? TaskCreation.CreateOrUpdate : TaskCreation.Create);
	}

	public Task RegisterTask(string path, [NotNull] string xmlText, TaskCreation createType = TaskCreation.CreateOrUpdate, string userId = null, string password = null, TaskLogonType logonType = TaskLogonType.S4U, string sddl = null)
	{
		if (v2Folder != null)
		{
			return Task.CreateTask(TaskService, v2Folder.RegisterTask(path, xmlText, (int)createType, userId, password, logonType, sddl));
		}
		TaskDefinition taskDefinition = TaskService.NewTask();
		XmlSerializationHelper.ReadObjectFromXmlText(xmlText, taskDefinition);
		return RegisterTaskDefinition(path, taskDefinition, createType, userId ?? taskDefinition.Principal.ToString(), password, (logonType == TaskLogonType.S4U) ? taskDefinition.Principal.LogonType : logonType, sddl);
	}

	public Task RegisterTaskDefinition(string path, [NotNull] TaskDefinition definition)
	{
		return RegisterTaskDefinition(path, definition, TaskCreation.CreateOrUpdate, definition.Principal.ToString(), null, definition.Principal.LogonType);
	}

	public Task RegisterTaskDefinition([NotNull] string path, [NotNull] TaskDefinition definition, TaskCreation createType, string userId, string password = null, TaskLogonType logonType = TaskLogonType.S4U, string sddl = null)
	{
		if (definition.Actions.Count < 1 || definition.Actions.Count > 32)
		{
			throw new ArgumentOutOfRangeException("Actions", "A task must be registered with at least one action and no more than 32 actions.");
		}
		if (userId == null)
		{
			userId = definition.Principal.Account;
		}
		if (userId == string.Empty)
		{
			userId = null;
		}
		User user = new User(userId);
		if (v2Folder != null)
		{
			definition.Actions.ConvertUnsupportedActions();
			switch (logonType)
			{
			case TaskLogonType.ServiceAccount:
				if (string.IsNullOrEmpty(userId) || !user.IsServiceAccount)
				{
					throw new ArgumentException("A valid system account name must be supplied for TaskLogonType.ServiceAccount. Valid entries are \"NT AUTHORITY\\SYSTEM\", \"SYSTEM\", \"NT AUTHORITY\\LOCALSERVICE\", or \"NT AUTHORITY\\NETWORKSERVICE\".", "userId");
				}
				if (password != null)
				{
					throw new ArgumentException("A password cannot be supplied when specifying TaskLogonType.ServiceAccount.", "password");
				}
				break;
			case TaskLogonType.Group:
				if (password != null)
				{
					throw new ArgumentException("A password cannot be supplied when specifying TaskLogonType.Group.", "password");
				}
				break;
			}
			if (definition.RegistrationInfo.Date == DateTime.MinValue)
			{
				definition.RegistrationInfo.Date = DateTime.Now;
			}
			IRegisteredTask registeredTask = v2Folder.RegisterTaskDefinition(path, definition.v2Def, (int)createType, userId ?? user.Name, password, logonType, sddl);
			if (createType == TaskCreation.ValidateOnly && registeredTask == null)
			{
				return null;
			}
			return Task.CreateTask(TaskService, registeredTask);
		}
		string text = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
		if (Regex.IsMatch(path, "[" + text + "]"))
		{
			throw new ArgumentOutOfRangeException("path", "Task names may not include any characters which are invalid for file names.");
		}
		if (Regex.IsMatch(path, "\\.[^" + text + "]{0,3}\\z"))
		{
			throw new ArgumentOutOfRangeException("path", "Task names ending with a period followed by three or fewer characters cannot be retrieved due to a bug in the native library.");
		}
		TaskFlags taskFlags = definition.v1Task.GetFlags();
		if (logonType == TaskLogonType.InteractiveTokenOrPassword && string.IsNullOrEmpty(password))
		{
			logonType = TaskLogonType.InteractiveToken;
		}
		switch (logonType)
		{
		case TaskLogonType.None:
		case TaskLogonType.S4U:
		case TaskLogonType.Group:
			throw new NotV1SupportedException("This LogonType is not supported on Task Scheduler 1.0.");
		case TaskLogonType.InteractiveToken:
			taskFlags |= TaskFlags.Interactive | TaskFlags.RunOnlyIfLoggedOn;
			definition.v1Task.SetAccountInformation(user.Name, IntPtr.Zero);
			break;
		case TaskLogonType.ServiceAccount:
			taskFlags &= ~(TaskFlags.Interactive | TaskFlags.RunOnlyIfLoggedOn);
			definition.v1Task.SetAccountInformation((string.IsNullOrEmpty(userId) || user.IsSystem) ? string.Empty : user.Name, IntPtr.Zero);
			break;
		case TaskLogonType.InteractiveTokenOrPassword:
		{
			taskFlags |= TaskFlags.Interactive;
			using (CoTaskMemString coTaskMemString2 = new CoTaskMemString(password))
			{
				definition.v1Task.SetAccountInformation(user.Name, coTaskMemString2.DangerousGetHandle());
			}
			break;
		}
		case TaskLogonType.Password:
		{
			using (CoTaskMemString coTaskMemString = new CoTaskMemString(password))
			{
				definition.v1Task.SetAccountInformation(user.Name, coTaskMemString.DangerousGetHandle());
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("logonType", logonType, null);
		}
		definition.v1Task.SetFlags(taskFlags);
		switch (createType)
		{
		case TaskCreation.Disable:
			definition.Settings.Enabled = false;
			goto case TaskCreation.Create;
		case TaskCreation.Create:
		case TaskCreation.Update:
		case TaskCreation.CreateOrUpdate:
			definition.V1Save(path);
			return new Task(TaskService, definition.v1Task);
		case TaskCreation.DontAddPrincipalAce:
			throw new NotV1SupportedException("Security settings are not available on Task Scheduler 1.0.");
		case TaskCreation.IgnoreRegistrationTriggers:
			throw new NotV1SupportedException("Registration triggers are not available on Task Scheduler 1.0.");
		case TaskCreation.ValidateOnly:
			throw new NotV1SupportedException("XML validation not available on Task Scheduler 1.0.");
		default:
			throw new ArgumentOutOfRangeException("createType", createType, null);
		}
	}

	public void SetAccessControl([NotNull] TaskSecurity taskSecurity)
	{
		taskSecurity.Persist(this);
	}

	[Obsolete("This method will be removed in deference to the SetAccessControl and SetSecurityDescriptorSddlForm methods.")]
	public void SetSecurityDescriptor([NotNull] GenericSecurityDescriptor sd, SecurityInfos includeSections = SecurityInfos.Owner | SecurityInfos.Group | SecurityInfos.DiscretionaryAcl)
	{
		SetSecurityDescriptorSddlForm(sd.GetSddlForm((AccessControlSections)includeSections));
	}

	public void SetSecurityDescriptorSddlForm([NotNull] string sddlForm, TaskSetSecurityOptions options = TaskSetSecurityOptions.None)
	{
		if (v2Folder != null)
		{
			v2Folder.SetSecurityDescriptor(sddlForm, (int)options);
			return;
		}
		throw new NotV1SupportedException();
	}

	public override string ToString()
	{
		return Path;
	}

	internal static IEnumerable<Task> EnumerateFolderTasks(TaskFolder folder, Predicate<Task> filter = null, bool recurse = true)
	{
		foreach (Task task in folder.Tasks)
		{
			if (filter == null || filter(task))
			{
				yield return task;
			}
		}
		if (!recurse)
		{
			yield break;
		}
		foreach (TaskFolder subFolder in folder.SubFolders)
		{
			foreach (Task item in EnumerateFolderTasks(subFolder, filter))
			{
				yield return item;
			}
		}
	}
}
