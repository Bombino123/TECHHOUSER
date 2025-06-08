using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal class XmlExpressionDumper : ExpressionDumper
{
	private readonly XmlWriter _writer;

	internal static Encoding DefaultEncoding => Encoding.UTF8;

	internal XmlExpressionDumper(Stream stream)
		: this(stream, DefaultEncoding)
	{
	}

	internal XmlExpressionDumper(Stream stream, Encoding encoding)
	{
		_writer = XmlWriter.Create(stream, new XmlWriterSettings
		{
			CheckCharacters = false,
			Indent = true,
			Encoding = encoding
		});
		_writer.WriteStartDocument(standalone: true);
	}

	internal void Close()
	{
		_writer.WriteEndDocument();
		_writer.Flush();
		_writer.Close();
	}

	internal override void Begin(string name, Dictionary<string, object> attrs)
	{
		_writer.WriteStartElement(name);
		if (attrs == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> attr in attrs)
		{
			_writer.WriteAttributeString(attr.Key, (attr.Value == null) ? "" : attr.Value.ToString());
		}
	}

	internal override void End(string name)
	{
		_writer.WriteEndElement();
	}
}
