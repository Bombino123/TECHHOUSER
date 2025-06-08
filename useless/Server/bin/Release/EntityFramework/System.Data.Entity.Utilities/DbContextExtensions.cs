using System.Data.Entity.Infrastructure;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Utilities;

internal static class DbContextExtensions
{
	public static XDocument GetModel(this DbContext context)
	{
		return GetModel(delegate(XmlWriter w)
		{
			EdmxWriter.WriteEdmx(context, w);
		});
	}

	public static XDocument GetModel(Action<XmlWriter> writeXml)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (XmlWriter obj = XmlWriter.Create(memoryStream, new XmlWriterSettings
		{
			Indent = true
		}))
		{
			writeXml(obj);
		}
		memoryStream.Position = 0L;
		return XDocument.Load((Stream)memoryStream);
	}
}
