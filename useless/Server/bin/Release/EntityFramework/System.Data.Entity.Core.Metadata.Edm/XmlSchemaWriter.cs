using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class XmlSchemaWriter
{
	protected XmlWriter _xmlWriter;

	protected double _version;

	internal void WriteComment(string comment)
	{
		if (!string.IsNullOrEmpty(comment))
		{
			_xmlWriter.WriteComment(comment);
		}
	}

	internal virtual void WriteEndElement()
	{
		_xmlWriter.WriteEndElement();
	}

	protected static string GetQualifiedTypeName(string prefix, string typeName)
	{
		return new StringBuilder().Append(prefix).Append(".").Append(typeName)
			.ToString();
	}

	internal static string GetLowerCaseStringFromBoolValue(bool value)
	{
		if (!value)
		{
			return "false";
		}
		return "true";
	}
}
