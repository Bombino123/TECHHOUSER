using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V1Interop;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlRoot("RegistrationInfo", Namespace = "http://schemas.microsoft.com/windows/2004/02/mit/task", IsNullable = true)]
[PublicAPI]
[ComVisible(true)]
public sealed class TaskRegistrationInfo : IDisposable, IXmlSerializable, INotifyPropertyChanged
{
	private readonly IRegistrationInfo v2RegInfo;

	private ITask v1Task;

	[DefaultValue(null)]
	public string Author
	{
		get
		{
			if (v2RegInfo == null)
			{
				return v1Task.GetCreator();
			}
			return v2RegInfo.Author;
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.Author = value;
			}
			else
			{
				v1Task.SetCreator(value);
			}
			OnNotifyPropertyChanged("Author");
		}
	}

	[DefaultValue(typeof(DateTime), "0001-01-01T00:00:00")]
	public DateTime Date
	{
		get
		{
			if (v2RegInfo != null)
			{
				if (DateTime.TryParse(v2RegInfo.Date, Trigger.DefaultDateCulture, DateTimeStyles.AssumeLocal, out var result))
				{
					return result;
				}
			}
			else
			{
				string v1Path = Task.GetV1Path(v1Task);
				if (!string.IsNullOrEmpty(v1Path) && File.Exists(v1Path))
				{
					return File.GetLastWriteTime(v1Path);
				}
			}
			return DateTime.MinValue;
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.Date = ((value == DateTime.MinValue) ? null : value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFK", Trigger.DefaultDateCulture));
			}
			else
			{
				string v1Path = Task.GetV1Path(v1Task);
				if (!string.IsNullOrEmpty(v1Path) && File.Exists(v1Path))
				{
					File.SetLastWriteTime(v1Path, value);
				}
			}
			OnNotifyPropertyChanged("Date");
		}
	}

	[DefaultValue(null)]
	public string Description
	{
		get
		{
			if (v2RegInfo == null)
			{
				return v1Task.GetComment();
			}
			return FixCrLf(v2RegInfo.Description);
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.Description = value;
			}
			else
			{
				v1Task.SetComment(value);
			}
			OnNotifyPropertyChanged("Description");
		}
	}

	[DefaultValue(null)]
	public string Documentation
	{
		get
		{
			if (v2RegInfo == null)
			{
				return v1Task.GetDataItem("Documentation");
			}
			return FixCrLf(v2RegInfo.Documentation);
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.Documentation = value;
			}
			else
			{
				v1Task.SetDataItem("Documentation", value);
			}
			OnNotifyPropertyChanged("Documentation");
		}
	}

	[XmlIgnore]
	public GenericSecurityDescriptor SecurityDescriptor
	{
		get
		{
			return new RawSecurityDescriptor(SecurityDescriptorSddlForm);
		}
		set
		{
			SecurityDescriptorSddlForm = value?.GetSddlForm(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}
	}

	[DefaultValue(null)]
	[XmlIgnore]
	public string SecurityDescriptorSddlForm
	{
		get
		{
			object obj = null;
			if (v2RegInfo != null)
			{
				obj = v2RegInfo.SecurityDescriptor;
			}
			return obj?.ToString();
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.SecurityDescriptor = value;
				OnNotifyPropertyChanged("SecurityDescriptorSddlForm");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(null)]
	public string Source
	{
		get
		{
			if (v2RegInfo == null)
			{
				return v1Task.GetDataItem("Source");
			}
			return v2RegInfo.Source;
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.Source = value;
			}
			else
			{
				v1Task.SetDataItem("Source", value);
			}
			OnNotifyPropertyChanged("Source");
		}
	}

	[DefaultValue(null)]
	public string URI
	{
		get
		{
			string text = ((v2RegInfo != null) ? v2RegInfo.URI : v1Task.GetDataItem("URI"));
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return null;
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.URI = value;
			}
			else
			{
				v1Task.SetDataItem("URI", value);
			}
			OnNotifyPropertyChanged("URI");
		}
	}

	[DefaultValueEx(typeof(Version), "1.0")]
	public Version Version
	{
		get
		{
			string text = ((v2RegInfo != null) ? v2RegInfo.Version : v1Task.GetDataItem("Version"));
			if (text != null)
			{
				try
				{
					return new Version(text);
				}
				catch
				{
				}
			}
			return new Version(1, 0);
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.Version = value?.ToString();
			}
			else
			{
				v1Task.SetDataItem("Version", value.ToString());
			}
			OnNotifyPropertyChanged("Version");
		}
	}

	[XmlIgnore]
	public string XmlText
	{
		get
		{
			if (v2RegInfo == null)
			{
				return XmlSerializationHelper.WriteObjectToXmlText(this);
			}
			return v2RegInfo.XmlText;
		}
		set
		{
			if (v2RegInfo != null)
			{
				v2RegInfo.XmlText = value;
			}
			else
			{
				XmlSerializationHelper.ReadObjectFromXmlText(value, this);
			}
			OnNotifyPropertyChanged("XmlText");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal TaskRegistrationInfo([NotNull] IRegistrationInfo iRegInfo)
	{
		v2RegInfo = iRegInfo;
	}

	internal TaskRegistrationInfo([NotNull] ITask iTask)
	{
		v1Task = iTask;
	}

	public void Dispose()
	{
		v1Task = null;
		if (v2RegInfo != null)
		{
			Marshal.ReleaseComObject(v2RegInfo);
		}
	}

	public override string ToString()
	{
		if (v2RegInfo != null || v1Task != null)
		{
			return DebugHelper.GetDebugString(this);
		}
		return base.ToString();
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		if (!reader.IsEmptyElement)
		{
			reader.ReadStartElement(XmlSerializationHelper.GetElementName(this), "http://schemas.microsoft.com/windows/2004/02/mit/task");
			XmlSerializationHelper.ReadObjectProperties(reader, this, ProcessVersionXml);
			reader.ReadEndElement();
		}
		else
		{
			reader.Skip();
		}
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		XmlSerializationHelper.WriteObjectProperties(writer, this, ProcessVersionXml);
	}

	internal static string FixCrLf(string text)
	{
		if (text != null)
		{
			return Regex.Replace(text, "(?<!\r)\n|\r(?!\n)", "\r\n");
		}
		return null;
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private bool ProcessVersionXml(PropertyInfo pi, object obj, ref object value)
	{
		if (pi.Name != "Version" || value == null)
		{
			return false;
		}
		if (value is Version)
		{
			value = value.ToString();
		}
		else if (value is string)
		{
			value = new Version(value.ToString());
		}
		return true;
	}
}
