using System.Data.Entity.Utilities;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Infrastructure;

public class DefaultDbModelStore : DbModelStore
{
	private const string FileExtension = ".edmx";

	private readonly string _directory;

	public string Directory => _directory;

	public DefaultDbModelStore(string directory)
	{
		Check.NotEmpty(directory, "directory");
		_directory = directory;
	}

	public override DbCompiledModel TryLoad(Type contextType)
	{
		return LoadXml(contextType, delegate(XmlReader reader)
		{
			string defaultSchema = GetDefaultSchema(contextType);
			return EdmxReader.Read(reader, defaultSchema);
		});
	}

	public override XDocument TryGetEdmx(Type contextType)
	{
		return LoadXml(contextType, (Func<XmlReader, XDocument>)XDocument.Load);
	}

	internal T LoadXml<T>(Type contextType, Func<XmlReader, T> xmlReaderDelegate)
	{
		string filePath = GetFilePath(contextType);
		if (!File.Exists(filePath))
		{
			return default(T);
		}
		if (!FileIsValid(contextType, filePath))
		{
			File.Delete(filePath);
			return default(T);
		}
		using XmlReader arg = XmlReader.Create(filePath);
		return xmlReaderDelegate(arg);
	}

	public override void Save(Type contextType, DbModel model)
	{
		using XmlWriter writer = XmlWriter.Create(GetFilePath(contextType), new XmlWriterSettings
		{
			Indent = true
		});
		EdmxWriter.WriteEdmx(model, writer);
	}

	protected virtual string GetFilePath(Type contextType)
	{
		string path = contextType.FullName + ".edmx";
		return Path.Combine(_directory, path);
	}

	protected virtual bool FileIsValid(Type contextType, string filePath)
	{
		DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(contextType.Assembly.Location);
		return File.GetLastWriteTimeUtc(filePath) >= lastWriteTimeUtc;
	}
}
