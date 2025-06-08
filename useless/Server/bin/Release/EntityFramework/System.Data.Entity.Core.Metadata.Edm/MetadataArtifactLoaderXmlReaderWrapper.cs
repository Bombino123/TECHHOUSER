using System.Collections.Generic;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataArtifactLoaderXmlReaderWrapper : MetadataArtifactLoader, IComparable
{
	private readonly XmlReader _reader;

	private readonly string _resourceUri;

	public override string Path
	{
		get
		{
			if (string.IsNullOrEmpty(_resourceUri))
			{
				return string.Empty;
			}
			return _resourceUri;
		}
	}

	public MetadataArtifactLoaderXmlReaderWrapper(XmlReader xmlReader)
	{
		_reader = xmlReader;
		_resourceUri = xmlReader.BaseURI;
	}

	public int CompareTo(object obj)
	{
		if (obj is MetadataArtifactLoaderXmlReaderWrapper metadataArtifactLoaderXmlReaderWrapper)
		{
			if (_reader == metadataArtifactLoaderXmlReaderWrapper._reader)
			{
				return 0;
			}
			return -1;
		}
		return -1;
	}

	public override bool Equals(object obj)
	{
		return CompareTo(obj) == 0;
	}

	public override int GetHashCode()
	{
		return _reader.GetHashCode();
	}

	public override List<string> GetPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		if (MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
		{
			list.Add(Path);
		}
		return list;
	}

	public override List<string> GetPaths()
	{
		return new List<string>(new string[1] { Path });
	}

	public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
	{
		List<XmlReader> result = new List<XmlReader> { _reader };
		sourceDictionary?.Add(this, _reader);
		return result;
	}

	public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
	{
		List<XmlReader> list = new List<XmlReader>();
		if (MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
		{
			list.Add(_reader);
		}
		return list;
	}
}
