using System.Collections.Generic;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Resources;
using System.IO;
using System.Reflection;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataArtifactLoaderResource : MetadataArtifactLoader, IComparable
{
	private readonly bool _alreadyLoaded;

	private readonly Assembly _assembly;

	private readonly string _resourceName;

	public override string Path => MetadataArtifactLoaderCompositeResource.CreateResPath(_assembly, _resourceName);

	internal MetadataArtifactLoaderResource(Assembly assembly, string resourceName, ICollection<string> uriRegistry)
	{
		_assembly = assembly;
		_resourceName = resourceName;
		string item = MetadataArtifactLoaderCompositeResource.CreateResPath(_assembly, _resourceName);
		_alreadyLoaded = uriRegistry.Contains(item);
		if (!_alreadyLoaded)
		{
			uriRegistry.Add(item);
		}
	}

	public int CompareTo(object obj)
	{
		if (obj is MetadataArtifactLoaderResource metadataArtifactLoaderResource)
		{
			return string.Compare(Path, metadataArtifactLoaderResource.Path, StringComparison.OrdinalIgnoreCase);
		}
		return -1;
	}

	public override bool Equals(object obj)
	{
		return CompareTo(obj) == 0;
	}

	public override int GetHashCode()
	{
		return Path.GetHashCode();
	}

	public override List<string> GetPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		if (!_alreadyLoaded && MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
		{
			list.Add(Path);
		}
		return list;
	}

	public override List<string> GetPaths()
	{
		List<string> list = new List<string>();
		if (!_alreadyLoaded)
		{
			list.Add(Path);
		}
		return list;
	}

	public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
	{
		List<XmlReader> list = new List<XmlReader>();
		if (!_alreadyLoaded)
		{
			XmlReader xmlReader = CreateReader();
			list.Add(xmlReader);
			sourceDictionary?.Add(this, xmlReader);
		}
		return list;
	}

	private XmlReader CreateReader()
	{
		Stream input = LoadResource();
		XmlReaderSettings xmlReaderSettings = Schema.CreateEdmStandardXmlReaderSettings();
		xmlReaderSettings.CloseInput = true;
		xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
		return XmlReader.Create(input, xmlReaderSettings);
	}

	public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
	{
		List<XmlReader> list = new List<XmlReader>();
		if (!_alreadyLoaded && MetadataArtifactLoader.IsArtifactOfDataSpace(Path, spaceToGet))
		{
			XmlReader item = CreateReader();
			list.Add(item);
		}
		return list;
	}

	private Stream LoadResource()
	{
		if (TryCreateResourceStream(out var resourceStream))
		{
			return resourceStream;
		}
		throw new MetadataException(Strings.UnableToLoadResource);
	}

	private bool TryCreateResourceStream(out Stream resourceStream)
	{
		resourceStream = _assembly.GetManifestResourceStream(_resourceName);
		return resourceStream != null;
	}
}
