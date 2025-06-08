using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("Principals", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = true)]
[PublicAPI]
[ComVisible(true)]
public sealed class TaskPrincipal : IDisposable, IXmlSerializable, INotifyPropertyChanged
{
	private const string localSystemAcct = "SYSTEM";

	private readonly Microsoft.Win32.TaskScheduler.V2Interop.IPrincipal v2Principal;

	private readonly IPrincipal2 v2Principal2;

	private readonly Func<string> xmlFunc;

	private TaskPrincipalPrivileges reqPriv;

	private string v1CachedAcctInfo;

	private ITask v1Task;

	[DefaultValue(null)]
	[Browsable(false)]
	public string Account
	{
		get
		{
			try
			{
				string text = xmlFunc?.Invoke();
				if (!string.IsNullOrEmpty(text))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(text);
					XmlElement xmlElement = xmlDocument.DocumentElement?["Principals"]?["Principal"];
					if (xmlElement != null)
					{
						XmlElement xmlElement2 = xmlElement["UserId"] ?? xmlElement["GroupId"];
						if (xmlElement2 != null)
						{
							try
							{
								return User.FromSidString(xmlElement2.InnerText).Name;
							}
							catch
							{
								try
								{
									return new User(xmlElement2.InnerText).Name;
								}
								catch
								{
								}
							}
						}
					}
				}
				return new User(ToString()).Name;
			}
			catch
			{
				return null;
			}
		}
	}

	[DefaultValue(null)]
	public string DisplayName
	{
		get
		{
			if (v2Principal == null)
			{
				return v1Task.GetDataItem("PrincipalDisplayName");
			}
			return v2Principal.DisplayName;
		}
		set
		{
			if (v2Principal != null)
			{
				v2Principal.DisplayName = value;
			}
			else
			{
				v1Task.SetDataItem("PrincipalDisplayName", value);
			}
			OnNotifyPropertyChanged("DisplayName");
		}
	}

	[DefaultValue(null)]
	[XmlIgnore]
	public string GroupId
	{
		get
		{
			return v2Principal?.GroupId;
		}
		set
		{
			if (v2Principal != null)
			{
				if (string.IsNullOrEmpty(value))
				{
					value = null;
				}
				else
				{
					v2Principal.UserId = null;
					v2Principal.LogonType = TaskLogonType.Group;
				}
				v2Principal.GroupId = value;
				OnNotifyPropertyChanged("GroupId");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(null)]
	[XmlAttribute(AttributeName = "id", DataType = "ID")]
	public string Id
	{
		get
		{
			if (v2Principal == null)
			{
				return v1Task.GetDataItem("PrincipalId");
			}
			return v2Principal.Id;
		}
		set
		{
			if (v2Principal != null)
			{
				v2Principal.Id = value;
			}
			else
			{
				v1Task.SetDataItem("PrincipalId", value);
			}
			OnNotifyPropertyChanged("Id");
		}
	}

	[DefaultValue(typeof(TaskLogonType), "None")]
	public TaskLogonType LogonType
	{
		get
		{
			if (v2Principal != null)
			{
				return v2Principal.LogonType;
			}
			if (UserId == "SYSTEM")
			{
				return TaskLogonType.ServiceAccount;
			}
			if (v1Task.HasFlags(TaskFlags.RunOnlyIfLoggedOn))
			{
				return TaskLogonType.InteractiveToken;
			}
			return TaskLogonType.InteractiveTokenOrPassword;
		}
		set
		{
			if (v2Principal != null)
			{
				v2Principal.LogonType = value;
			}
			else
			{
				if (value == TaskLogonType.Group || value == TaskLogonType.None || value == TaskLogonType.S4U)
				{
					throw new NotV1SupportedException();
				}
				v1Task.SetFlags(TaskFlags.RunOnlyIfLoggedOn, value == TaskLogonType.InteractiveToken);
			}
			OnNotifyPropertyChanged("LogonType");
		}
	}

	[XmlIgnore]
	[DefaultValue(typeof(TaskProcessTokenSidType), "Default")]
	public TaskProcessTokenSidType ProcessTokenSidType
	{
		get
		{
			return v2Principal2?.ProcessTokenSidType ?? TaskProcessTokenSidType.Default;
		}
		set
		{
			if (v2Principal2 != null)
			{
				v2Principal2.ProcessTokenSidType = value;
				OnNotifyPropertyChanged("ProcessTokenSidType");
				return;
			}
			throw new NotSupportedPriorToException(TaskCompatibility.V2_1);
		}
	}

	[XmlIgnore]
	public TaskPrincipalPrivileges RequiredPrivileges => reqPriv ?? (reqPriv = new TaskPrincipalPrivileges(v2Principal2));

	[DefaultValue(typeof(TaskRunLevel), "LUA")]
	[XmlIgnore]
	public TaskRunLevel RunLevel
	{
		get
		{
			return v2Principal?.RunLevel ?? TaskRunLevel.LUA;
		}
		set
		{
			if (v2Principal != null)
			{
				v2Principal.RunLevel = value;
			}
			else if (value != 0)
			{
				throw new NotV1SupportedException();
			}
			OnNotifyPropertyChanged("RunLevel");
		}
	}

	[DefaultValue(null)]
	public string UserId
	{
		get
		{
			if (v2Principal != null)
			{
				return v2Principal.UserId;
			}
			if (v1CachedAcctInfo == null)
			{
				try
				{
					string text = v1Task.GetAccountInformation();
					v1CachedAcctInfo = (string.IsNullOrEmpty(text) ? "SYSTEM" : text);
				}
				catch
				{
					v1CachedAcctInfo = string.Empty;
				}
			}
			if (!(v1CachedAcctInfo == string.Empty))
			{
				return v1CachedAcctInfo;
			}
			return null;
		}
		set
		{
			if (v2Principal != null)
			{
				if (string.IsNullOrEmpty(value))
				{
					value = null;
				}
				else
				{
					v2Principal.GroupId = null;
				}
				v2Principal.UserId = value;
			}
			else
			{
				if (value.Equals("SYSTEM", StringComparison.CurrentCultureIgnoreCase))
				{
					value = "";
				}
				v1Task.SetAccountInformation(value, IntPtr.Zero);
				v1CachedAcctInfo = null;
			}
			OnNotifyPropertyChanged("UserId");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal TaskPrincipal([NotNull] Microsoft.Win32.TaskScheduler.V2Interop.IPrincipal iPrincipal, Func<string> defXml)
	{
		xmlFunc = defXml;
		v2Principal = iPrincipal;
		try
		{
			if (Environment.OSVersion.Version >= new Version(6, 1))
			{
				v2Principal2 = (IPrincipal2)v2Principal;
			}
		}
		catch
		{
		}
	}

	internal TaskPrincipal([NotNull] ITask iTask)
	{
		v1Task = iTask;
	}

	public static bool ValidateAccountForSidType(string acct, TaskProcessTokenSidType sidType)
	{
		string[] array = new string[4] { "NETWORK SERVICE", "LOCAL SERVICE", "S-1-5-19", "S-1-5-20" };
		if (sidType != TaskProcessTokenSidType.Default)
		{
			return Array.Find(array, (string id) => id.Equals(acct, StringComparison.InvariantCultureIgnoreCase)) != null;
		}
		return true;
	}

	public void Dispose()
	{
		if (v2Principal != null)
		{
			Marshal.ReleaseComObject(v2Principal);
		}
		v1Task = null;
	}

	public bool RequiresPassword()
	{
		if (LogonType != TaskLogonType.InteractiveTokenOrPassword && LogonType != TaskLogonType.Password)
		{
			if (LogonType == TaskLogonType.S4U && UserId != null)
			{
				return string.Compare(UserId, WindowsIdentity.GetCurrent().Name, StringComparison.OrdinalIgnoreCase) != 0;
			}
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		if (LogonType != TaskLogonType.Group)
		{
			return UserId;
		}
		return GroupId;
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		reader.ReadStartElement(XmlSerializationHelper.GetElementName(this), "http://schemas.microsoft.com/windows/2004/02/mit/task");
		if (reader.HasAttributes)
		{
			Id = reader.GetAttribute("id");
		}
		reader.Read();
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			if (reader.LocalName == "Principal")
			{
				reader.Read();
				XmlSerializationHelper.ReadObjectProperties(reader, this);
				reader.ReadEndElement();
			}
			else
			{
				reader.Skip();
			}
		}
		reader.ReadEndElement();
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		if (!string.IsNullOrEmpty(ToString()) || LogonType != 0)
		{
			writer.WriteStartElement("Principal");
			if (!string.IsNullOrEmpty(Id))
			{
				writer.WriteAttributeString("id", Id);
			}
			XmlSerializationHelper.WriteObjectProperties(writer, this);
			writer.WriteEndElement();
		}
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
