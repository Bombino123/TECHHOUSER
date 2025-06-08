using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Microsoft.Win32.TaskScheduler;

internal static class XmlSerializationHelper
{
	public delegate bool PropertyConversionHandler([NotNull] PropertyInfo pi, object obj, ref object value);

	public static object GetDefaultValue([NotNull] PropertyInfo prop)
	{
		object[] customAttributes = prop.GetCustomAttributes(typeof(DefaultValueAttribute), inherit: true);
		if (customAttributes.Length != 0)
		{
			return ((DefaultValueAttribute)customAttributes[0]).Value;
		}
		if (prop.PropertyType.IsValueType)
		{
			return Activator.CreateInstance(prop.PropertyType);
		}
		return null;
	}

	private static bool GetPropertyValue(object obj, [NotNull] string property, ref object outVal)
	{
		PropertyInfo propertyInfo = obj?.GetType().GetProperty(property);
		if (propertyInfo != null)
		{
			outVal = propertyInfo.GetValue(obj, null);
			return true;
		}
		return false;
	}

	private static bool GetAttributeValue(Type objType, Type attrType, string property, bool inherit, ref object outVal)
	{
		object[] customAttributes = objType.GetCustomAttributes(attrType, inherit);
		if (customAttributes.Length != 0)
		{
			return GetPropertyValue(customAttributes[0], property, ref outVal);
		}
		return false;
	}

	private static bool GetAttributeValue([NotNull] PropertyInfo propInfo, Type attrType, string property, bool inherit, ref object outVal)
	{
		return GetPropertyValue(Attribute.GetCustomAttribute(propInfo, attrType, inherit), property, ref outVal);
	}

	private static bool IsStandardType(Type type)
	{
		if (!type.IsPrimitive && !(type == typeof(DateTime)) && !(type == typeof(DateTimeOffset)) && !(type == typeof(decimal)) && !(type == typeof(Guid)) && !(type == typeof(TimeSpan)) && !(type == typeof(string)))
		{
			return type.IsEnum;
		}
		return true;
	}

	private static bool HasMembers([NotNull] object obj)
	{
		if (obj is IXmlSerializable)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
				((IXmlSerializable)obj).WriteXml(xmlTextWriter);
				xmlTextWriter.Flush();
				return memoryStream.Length > 3;
			}
		}
		PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (Attribute.IsDefined(propertyInfo, typeof(XmlIgnoreAttribute), inherit: false))
			{
				continue;
			}
			object value = propertyInfo.GetValue(obj, null);
			if (!object.Equals(value, GetDefaultValue(propertyInfo)))
			{
				if (IsStandardType(propertyInfo.PropertyType))
				{
					return true;
				}
				if (HasMembers(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetPropertyAttributeName([NotNull] PropertyInfo pi)
	{
		object outVal = null;
		string result = pi.Name;
		if (GetAttributeValue(pi, typeof(XmlAttributeAttribute), "AttributeName", inherit: false, ref outVal))
		{
			result = outVal.ToString();
		}
		return result;
	}

	public static string GetPropertyElementName([NotNull] PropertyInfo pi)
	{
		object outVal = null;
		string result = pi.Name;
		if (GetAttributeValue(pi, typeof(XmlElementAttribute), "ElementName", inherit: false, ref outVal))
		{
			result = outVal.ToString();
		}
		else if (GetAttributeValue(pi.PropertyType, typeof(XmlRootAttribute), "ElementName", inherit: true, ref outVal))
		{
			result = outVal.ToString();
		}
		return result;
	}

	public static bool WriteProperty([NotNull] XmlWriter writer, [NotNull] PropertyInfo pi, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		if (Attribute.IsDefined(pi, typeof(XmlIgnoreAttribute), inherit: false) || Attribute.IsDefined(pi, typeof(XmlAttributeAttribute), inherit: false))
		{
			return false;
		}
		object value = pi.GetValue(obj, null);
		object defaultValue = GetDefaultValue(pi);
		if ((value == null && defaultValue == null) || (value != null && value.Equals(defaultValue)))
		{
			return false;
		}
		Type type = pi.PropertyType;
		if (handler != null && handler(pi, obj, ref value))
		{
			type = value.GetType();
		}
		bool flag = IsStandardType(type);
		bool flag2 = pi.CanRead && pi.CanWrite;
		if (pi.CanRead)
		{
			_ = !pi.CanWrite;
		}
		else
			_ = 0;
		string propertyElementName = GetPropertyElementName(pi);
		if (flag && flag2)
		{
			string xmlValue = GetXmlValue(value, type);
			if (xmlValue != null)
			{
				writer.WriteElementString(propertyElementName, xmlValue);
			}
		}
		else if (!flag)
		{
			object outVal = null;
			if (type.GetInterface("IXmlSerializable") == null && GetAttributeValue(pi, typeof(XmlArrayAttribute), "ElementName", inherit: true, ref outVal) && type.GetInterface("IEnumerable") != null)
			{
				if (string.IsNullOrEmpty(outVal.ToString()))
				{
					outVal = propertyElementName;
				}
				writer.WriteStartElement(outVal.ToString());
				Attribute[] customAttributes = Attribute.GetCustomAttributes(pi, typeof(XmlArrayItemAttribute), inherit: true);
				Dictionary<Type, string> dictionary = new Dictionary<Type, string>(customAttributes.Length);
				Attribute[] array = customAttributes;
				for (int i = 0; i < array.Length; i++)
				{
					XmlArrayItemAttribute xmlArrayItemAttribute = (XmlArrayItemAttribute)array[i];
					dictionary.Add(xmlArrayItemAttribute.Type, xmlArrayItemAttribute.ElementName);
				}
				foreach (object item in (IEnumerable)value)
				{
					Type type2 = item.GetType();
					if (dictionary.TryGetValue(type2, out var value2))
					{
						if (IsStandardType(type2))
						{
							writer.WriteElementString(value2, GetXmlValue(item, type2));
						}
						else
						{
							WriteObject(writer, item, null, includeNS: false, value2);
						}
					}
				}
				writer.WriteEndElement();
			}
			else
			{
				WriteObject(writer, value);
			}
		}
		return false;
	}

	private static string GetXmlValue([NotNull] object value, Type propType)
	{
		string text = null;
		if (propType.IsEnum)
		{
			if (Attribute.IsDefined(propType, typeof(FlagsAttribute), inherit: false))
			{
				return Convert.ChangeType(value, Enum.GetUnderlyingType(propType)).ToString();
			}
			return value.ToString();
		}
		return propType.FullName switch
		{
			"System.Boolean" => XmlConvert.ToString((bool)value), 
			"System.Byte" => XmlConvert.ToString((byte)value), 
			"System.Char" => XmlConvert.ToString((char)value), 
			"System.DateTime" => XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind), 
			"System.DateTimeOffset" => XmlConvert.ToString((DateTimeOffset)value), 
			"System.Decimal" => XmlConvert.ToString((decimal)value), 
			"System.Double" => XmlConvert.ToString((double)value), 
			"System.Single" => XmlConvert.ToString((float)value), 
			"System.Guid" => XmlConvert.ToString((Guid)value), 
			"System.Int16" => XmlConvert.ToString((short)value), 
			"System.Int32" => XmlConvert.ToString((int)value), 
			"System.Int64" => XmlConvert.ToString((long)value), 
			"System.SByte" => XmlConvert.ToString((sbyte)value), 
			"System.TimeSpan" => XmlConvert.ToString((TimeSpan)value), 
			"System.UInt16" => XmlConvert.ToString((ushort)value), 
			"System.UInt32" => XmlConvert.ToString((uint)value), 
			"System.UInt64" => XmlConvert.ToString((ulong)value), 
			_ => (value == null) ? string.Empty : value.ToString(), 
		};
	}

	public static void WriteObjectAttributes([NotNull] XmlWriter writer, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (Attribute.IsDefined(propertyInfo, typeof(XmlAttributeAttribute), inherit: false))
			{
				WriteObjectAttribute(writer, propertyInfo, obj, handler);
			}
		}
	}

	public static void WriteObjectAttribute([NotNull] XmlWriter writer, [NotNull] PropertyInfo pi, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		object value = pi.GetValue(obj, null);
		object defaultValue = GetDefaultValue(pi);
		if ((value != null || defaultValue != null) && (value == null || !value.Equals(defaultValue)))
		{
			Type propType = pi.PropertyType;
			if (handler != null && handler(pi, obj, ref value))
			{
				propType = value.GetType();
			}
			writer.WriteAttributeString(GetPropertyAttributeName(pi), GetXmlValue(value, propType));
		}
	}

	public static void WriteObjectProperties([NotNull] XmlWriter writer, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo pi in properties)
		{
			WriteProperty(writer, pi, obj, handler);
		}
	}

	public static void WriteObject([NotNull] XmlWriter writer, [NotNull] object obj, PropertyConversionHandler handler = null, bool includeNS = false, string elemName = null)
	{
		if (obj == null)
		{
			return;
		}
		string localName = elemName ?? GetElementName(obj);
		if (HasMembers(obj))
		{
			if (includeNS)
			{
				writer.WriteStartElement(localName, GetTopLevelNamespace(obj));
			}
			else
			{
				writer.WriteStartElement(localName);
			}
			if (obj is IXmlSerializable)
			{
				((IXmlSerializable)obj).WriteXml(writer);
			}
			else
			{
				WriteObjectAttributes(writer, obj, handler);
				WriteObjectProperties(writer, obj, handler);
			}
			writer.WriteEndElement();
		}
	}

	public static string GetElementName([NotNull] object obj)
	{
		object outVal = null;
		if (!GetAttributeValue(obj.GetType(), typeof(XmlRootAttribute), "ElementName", inherit: true, ref outVal))
		{
			return obj.GetType().Name;
		}
		return outVal.ToString();
	}

	public static string GetTopLevelNamespace([NotNull] object obj)
	{
		object outVal = null;
		if (!GetAttributeValue(obj.GetType(), typeof(XmlRootAttribute), "Namespace", inherit: true, ref outVal))
		{
			return null;
		}
		return outVal.ToString();
	}

	public static void ReadObjectProperties([NotNull] XmlReader reader, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>(properties.Length);
		Dictionary<string, PropertyInfo> dictionary2 = new Dictionary<string, PropertyInfo>(properties.Length);
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!Attribute.IsDefined(propertyInfo, typeof(XmlIgnoreAttribute), inherit: false))
			{
				if (Attribute.IsDefined(propertyInfo, typeof(XmlAttributeAttribute), inherit: false))
				{
					dictionary.Add(GetPropertyAttributeName(propertyInfo), propertyInfo);
				}
				else
				{
					dictionary2.Add(GetPropertyElementName(propertyInfo), propertyInfo);
				}
			}
		}
		if (reader.HasAttributes)
		{
			for (int j = 0; j < reader.AttributeCount; j++)
			{
				reader.MoveToAttribute(j);
				if (dictionary.TryGetValue(reader.LocalName, out var value) && IsStandardType(value.PropertyType))
				{
					object obj2 = null;
					obj2 = ((!value.PropertyType.IsEnum) ? Convert.ChangeType(reader.Value, value.PropertyType) : Enum.Parse(value.PropertyType, reader.Value));
					handler?.Invoke(value, obj, ref obj2);
					value.SetValue(obj, obj2, null);
				}
			}
		}
		while (reader.MoveToContent() == XmlNodeType.Element)
		{
			object outVal = null;
			if (dictionary2.TryGetValue(reader.LocalName, out var value2))
			{
				TypeDescriptor.GetConverter(value2.PropertyType);
				if (IsStandardType(value2.PropertyType))
				{
					object obj3 = null;
					obj3 = (value2.PropertyType.IsEnum ? Enum.Parse(value2.PropertyType, reader.ReadElementContentAsString()) : ((!(value2.PropertyType == typeof(Guid))) ? reader.ReadElementContentAs(value2.PropertyType, null) : new GuidConverter().ConvertFromString(reader.ReadElementContentAsString())));
					handler?.Invoke(value2, obj, ref obj3);
					value2.SetValue(obj, obj3, null);
				}
				else if (value2.PropertyType == typeof(Version))
				{
					Version value3 = new Version(reader.ReadElementContentAsString());
					value2.SetValue(obj, value3, null);
				}
				else if (value2.PropertyType.GetInterface("IEnumerable") != null && value2.PropertyType.GetInterface("IXmlSerializable") == null && GetAttributeValue(value2, typeof(XmlArrayAttribute), "ElementName", inherit: true, ref outVal))
				{
					string name = (string.IsNullOrEmpty(outVal?.ToString()) ? value2.Name : outVal.ToString());
					reader.ReadStartElement(name);
					Attribute[] customAttributes = Attribute.GetCustomAttributes(value2, typeof(XmlArrayItemAttribute), inherit: true);
					Dictionary<string, Type> dictionary3 = new Dictionary<string, Type>(customAttributes.Length);
					Attribute[] array2 = customAttributes;
					for (int i = 0; i < array2.Length; i++)
					{
						XmlArrayItemAttribute xmlArrayItemAttribute = (XmlArrayItemAttribute)array2[i];
						dictionary3.Add(xmlArrayItemAttribute.ElementName, xmlArrayItemAttribute.Type);
					}
					List<object> list = new List<object>();
					while (reader.MoveToContent() == XmlNodeType.Element)
					{
						if (dictionary3.TryGetValue(reader.LocalName, out var value4))
						{
							object obj4;
							if (IsStandardType(value4))
							{
								obj4 = reader.ReadElementContentAs(value4, null);
							}
							else
							{
								obj4 = Activator.CreateInstance(value4);
								ReadObject(reader, obj4, handler);
							}
							if (obj4 != null)
							{
								list.Add(obj4);
							}
						}
					}
					reader.ReadEndElement();
					if (list.Count <= 0)
					{
						continue;
					}
					IEnumerable enumerable = list;
					Type type = typeof(object);
					if (dictionary3.Count == 1)
					{
						using Dictionary<string, Type>.ValueCollection.Enumerator enumerator = dictionary3.Values.GetEnumerator();
						if (enumerator.MoveNext())
						{
							type = enumerator.Current;
						}
					}
					bool flag = false;
					if (value2.PropertyType == enumerable.GetType() || (value2.PropertyType.IsArray && (value2.PropertyType.GetElementType() == typeof(object) || value2.PropertyType.GetElementType() == type)))
					{
						try
						{
							value2.SetValue(obj, enumerable, null);
							flag = true;
						}
						catch
						{
						}
					}
					if (!flag)
					{
						MethodInfo method = value2.PropertyType.GetMethod("AddRange", new Type[1] { typeof(IEnumerable) });
						if (method != null)
						{
							try
							{
								method.Invoke(value2.GetValue(obj, null), new object[1] { enumerable });
								flag = true;
							}
							catch
							{
							}
						}
					}
					if (!flag)
					{
						MethodInfo method2 = value2.PropertyType.GetMethod("Add", new Type[1] { typeof(object) });
						if (method2 != null)
						{
							try
							{
								foreach (object item in enumerable)
								{
									method2.Invoke(value2.GetValue(obj, null), new object[1] { item });
								}
								flag = true;
							}
							catch
							{
							}
						}
					}
					if (flag || !(type != typeof(object)))
					{
						continue;
					}
					MethodInfo method3 = value2.PropertyType.GetMethod("Add", new Type[1] { type });
					if (!(method3 != null))
					{
						continue;
					}
					try
					{
						foreach (object item2 in enumerable)
						{
							method3.Invoke(value2.GetValue(obj, null), new object[1] { item2 });
						}
						flag = true;
					}
					catch
					{
					}
				}
				else
				{
					object obj9 = value2.GetValue(obj, null) ?? Activator.CreateInstance(value2.PropertyType);
					if (obj9 == null)
					{
						throw new InvalidOperationException("Can't get instance of " + value2.PropertyType.Name + ".");
					}
					ReadObject(reader, obj9, handler);
				}
			}
			else
			{
				reader.Skip();
				reader.MoveToContent();
			}
		}
	}

	public static void ReadObject([NotNull] XmlReader reader, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		reader.MoveToContent();
		if (obj is IXmlSerializable)
		{
			((IXmlSerializable)obj).ReadXml(reader);
			return;
		}
		object outVal = null;
		string text = (GetAttributeValue(obj.GetType(), typeof(XmlRootAttribute), "ElementName", inherit: true, ref outVal) ? outVal.ToString() : obj.GetType().Name);
		if (reader.LocalName != text)
		{
			throw new XmlException("XML element name does not match object.");
		}
		if (!reader.IsEmptyElement)
		{
			reader.ReadStartElement();
			reader.MoveToContent();
			ReadObjectProperties(reader, obj, handler);
			reader.ReadEndElement();
		}
		else
		{
			reader.Skip();
		}
	}

	public static void ReadObjectFromXmlText([NotNull] string xml, [NotNull] object obj, PropertyConversionHandler handler = null)
	{
		using StringReader input = new StringReader(xml);
		using XmlReader xmlReader = XmlReader.Create(input);
		xmlReader.MoveToContent();
		ReadObject(xmlReader, obj, handler);
	}

	public static string WriteObjectToXmlText([NotNull] object obj, PropertyConversionHandler handler = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter writer = XmlWriter.Create(stringBuilder, new XmlWriterSettings
		{
			Indent = true
		}))
		{
			WriteObject(writer, obj, handler, includeNS: true);
		}
		return stringBuilder.ToString();
	}
}
