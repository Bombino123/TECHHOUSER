using System.Collections.Generic;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataArtifactLoaderFile : MetadataArtifactLoader, IComparable
{
	private readonly bool _alreadyLoaded;

	private readonly string _path;

	public override string Path => _path;

	public MetadataArtifactLoaderFile(string path, ICollection<string> uriRegistry)
	{
		_path = path;
		_alreadyLoaded = uriRegistry.Contains(_path);
		if (!_alreadyLoaded)
		{
			uriRegistry.Add(_path);
		}
	}

	public int CompareTo(object obj)
	{
		if (obj is MetadataArtifactLoaderFile metadataArtifactLoaderFile)
		{
			return string.Compare(_path, metadataArtifactLoaderFile._path, StringComparison.OrdinalIgnoreCase);
		}
		return -1;
	}

	public override bool Equals(object obj)
	{
		return CompareTo(obj) == 0;
	}

	public override int GetHashCode()
	{
		return _path.GetHashCode();
	}

	public override List<string> GetPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		if (!_alreadyLoaded && MetadataArtifactLoader.IsArtifactOfDataSpace(_path, spaceToGet))
		{
			list.Add(_path);
		}
		return list;
	}

	public override List<string> GetPaths()
	{
		List<string> list = new List<string>();
		if (!_alreadyLoaded)
		{
			list.Add(_path);
		}
		return list;
	}

	public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
	{
		List<XmlReader> list = new List<XmlReader>();
		if (!_alreadyLoaded)
		{
			XmlReader xmlReader = CreateXmlReader();
			list.Add(xmlReader);
			sourceDictionary?.Add(this, xmlReader);
		}
		return list;
	}

	public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
	{
		List<XmlReader> list = new List<XmlReader>();
		if (!_alreadyLoaded && MetadataArtifactLoader.IsArtifactOfDataSpace(_path, spaceToGet))
		{
			XmlReader item = CreateXmlReader();
			list.Add(item);
		}
		return list;
	}

	private XmlReader CreateXmlReader()
	{
		XmlReaderSettings xmlReaderSettings = Schema.CreateEdmStandardXmlReaderSettings();
		xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
		return XmlReader.Create(_path, xmlReaderSettings);
	}
}
