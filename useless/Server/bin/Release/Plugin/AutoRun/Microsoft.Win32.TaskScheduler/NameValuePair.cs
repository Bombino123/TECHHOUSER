using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Microsoft.Win32.TaskScheduler.V2Interop;

namespace Microsoft.Win32.TaskScheduler;

[PublicAPI]
[ComVisible(true)]
public class NameValuePair : IXmlSerializable, INotifyPropertyChanged, ICloneable, IEquatable<NameValuePair>, IEquatable<ITaskNamedValuePair>
{
	private readonly ITaskNamedValuePair v2Pair;

	private string name;

	private string value;

	[XmlIgnore]
	internal bool AttributedXmlFormat { get; set; } = true;


	[NotNull]
	public string Name
	{
		get
		{
			if (v2Pair != null)
			{
				return v2Pair.Name;
			}
			return name;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException("Name");
			}
			if (v2Pair == null)
			{
				name = value;
			}
			else
			{
				v2Pair.Name = value;
			}
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
		}
	}

	[NotNull]
	public string Value
	{
		get
		{
			if (v2Pair != null)
			{
				return v2Pair.Value;
			}
			return value;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException("Value");
			}
			if (v2Pair == null)
			{
				this.value = value;
			}
			else
			{
				v2Pair.Value = value;
			}
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public NameValuePair()
	{
	}

	internal NameValuePair([NotNull] ITaskNamedValuePair iPair)
	{
		v2Pair = iPair;
	}

	internal NameValuePair([NotNull] string name, [NotNull] string value)
	{
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
		{
			throw new ArgumentException("Both name and value must be non-empty strings.");
		}
		this.name = name;
		this.value = value;
	}

	[NotNull]
	public NameValuePair Clone()
	{
		return new NameValuePair(Name, Value);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public override bool Equals(object obj)
	{
		if (obj is ITaskNamedValuePair other)
		{
			return ((IEquatable<ITaskNamedValuePair>)this).Equals(other);
		}
		if (obj is NameValuePair other2)
		{
			return Equals(other2);
		}
		return base.Equals(obj);
	}

	public bool Equals([NotNull] NameValuePair other)
	{
		if (other.Name == Name)
		{
			return other.Value == Value;
		}
		return false;
	}

	bool IEquatable<ITaskNamedValuePair>.Equals(ITaskNamedValuePair other)
	{
		if (other.Name == Name)
		{
			return other.Value == Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return new
		{
			A = Name,
			B = Value
		}.GetHashCode();
	}

	public override string ToString()
	{
		return Name + "=" + Value;
	}

	public static implicit operator NameValuePair(KeyValuePair<string, string> kvp)
	{
		return new NameValuePair(kvp.Key, kvp.Value);
	}

	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Value")
		{
			Name = reader.GetAttribute("name");
			Value = reader.ReadString();
			reader.Read();
		}
		else
		{
			reader.ReadStartElement();
			XmlSerializationHelper.ReadObjectProperties(reader, this);
			reader.ReadEndElement();
		}
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		if (AttributedXmlFormat)
		{
			writer.WriteAttributeString("name", Name);
			writer.WriteString(Value);
		}
		else
		{
			XmlSerializationHelper.WriteObjectProperties(writer, this);
		}
	}
}
