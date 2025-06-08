using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[XmlType(IncludeInSchema = false)]
[PublicAPI]
[ComVisible(true)]
public sealed class NetworkSettings : IDisposable, INotifyPropertyChanged
{
	private readonly INetworkSettings v2Settings;

	[DefaultValue(typeof(Guid), "00000000-0000-0000-0000-000000000000")]
	public Guid Id
	{
		get
		{
			string text = null;
			if (v2Settings != null)
			{
				text = v2Settings.Id;
			}
			if (!string.IsNullOrEmpty(text))
			{
				return new Guid(text);
			}
			return Guid.Empty;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.Id = ((value == Guid.Empty) ? null : value.ToString());
				OnNotifyPropertyChanged("Id");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	[DefaultValue(null)]
	public string Name
	{
		get
		{
			return v2Settings?.Name;
		}
		set
		{
			if (v2Settings != null)
			{
				v2Settings.Name = value;
				OnNotifyPropertyChanged("Name");
				return;
			}
			throw new NotV1SupportedException();
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal NetworkSettings([CanBeNull] INetworkSettings iSettings)
	{
		v2Settings = iSettings;
	}

	public void Dispose()
	{
		if (v2Settings != null)
		{
			Marshal.ReleaseComObject(v2Settings);
		}
	}

	public override string ToString()
	{
		if (v2Settings != null)
		{
			return DebugHelper.GetDebugString(this);
		}
		return base.ToString();
	}

	internal bool IsSet()
	{
		if (v2Settings != null)
		{
			if (string.IsNullOrEmpty(v2Settings.Id))
			{
				return !string.IsNullOrEmpty(v2Settings.Name);
			}
			return true;
		}
		return false;
	}

	private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
