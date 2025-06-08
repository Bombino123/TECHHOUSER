using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataArtifactLoaderCompositeFile : MetadataArtifactLoader
{
	private ReadOnlyCollection<MetadataArtifactLoaderFile> _csdlChildren;

	private ReadOnlyCollection<MetadataArtifactLoaderFile> _ssdlChildren;

	private ReadOnlyCollection<MetadataArtifactLoaderFile> _mslChildren;

	private readonly string _path;

	private readonly ICollection<string> _uriRegistry;

	public override string Path => _path;

	public override bool IsComposite => true;

	internal ReadOnlyCollection<MetadataArtifactLoaderFile> CsdlChildren
	{
		get
		{
			LoadCollections();
			return _csdlChildren;
		}
	}

	internal ReadOnlyCollection<MetadataArtifactLoaderFile> SsdlChildren
	{
		get
		{
			LoadCollections();
			return _ssdlChildren;
		}
	}

	internal ReadOnlyCollection<MetadataArtifactLoaderFile> MslChildren
	{
		get
		{
			LoadCollections();
			return _mslChildren;
		}
	}

	public MetadataArtifactLoaderCompositeFile(string path, ICollection<string> uriRegistry)
	{
		_path = path;
		_uriRegistry = uriRegistry;
	}

	private void LoadCollections()
	{
		if (_csdlChildren == null)
		{
			ReadOnlyCollection<MetadataArtifactLoaderFile> value = new ReadOnlyCollection<MetadataArtifactLoaderFile>(GetArtifactsInDirectory(_path, ".csdl", _uriRegistry));
			Interlocked.CompareExchange(ref _csdlChildren, value, null);
		}
		if (_ssdlChildren == null)
		{
			ReadOnlyCollection<MetadataArtifactLoaderFile> value2 = new ReadOnlyCollection<MetadataArtifactLoaderFile>(GetArtifactsInDirectory(_path, ".ssdl", _uriRegistry));
			Interlocked.CompareExchange(ref _ssdlChildren, value2, null);
		}
		if (_mslChildren == null)
		{
			ReadOnlyCollection<MetadataArtifactLoaderFile> value3 = new ReadOnlyCollection<MetadataArtifactLoaderFile>(GetArtifactsInDirectory(_path, ".msl", _uriRegistry));
			Interlocked.CompareExchange(ref _mslChildren, value3, null);
		}
	}

	public override List<string> GetOriginalPaths(DataSpace spaceToGet)
	{
		return GetOriginalPaths();
	}

	public override List<string> GetPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		if (!TryGetListForSpace(spaceToGet, out var files))
		{
			return list;
		}
		foreach (MetadataArtifactLoaderFile item in files)
		{
			list.AddRange(item.GetPaths(spaceToGet));
		}
		return list;
	}

	private bool TryGetListForSpace(DataSpace spaceToGet, out IList<MetadataArtifactLoaderFile> files)
	{
		switch (spaceToGet)
		{
		case DataSpace.CSpace:
			files = CsdlChildren;
			return true;
		case DataSpace.SSpace:
			files = SsdlChildren;
			return true;
		case DataSpace.CSSpace:
			files = MslChildren;
			return true;
		default:
			files = null;
			return false;
		}
	}

	public override List<string> GetPaths()
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoaderFile csdlChild in CsdlChildren)
		{
			list.AddRange(csdlChild.GetPaths());
		}
		foreach (MetadataArtifactLoaderFile ssdlChild in SsdlChildren)
		{
			list.AddRange(ssdlChild.GetPaths());
		}
		foreach (MetadataArtifactLoaderFile mslChild in MslChildren)
		{
			list.AddRange(mslChild.GetPaths());
		}
		return list;
	}

	public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
	{
		List<XmlReader> list = new List<XmlReader>();
		foreach (MetadataArtifactLoaderFile csdlChild in CsdlChildren)
		{
			list.AddRange(csdlChild.GetReaders(sourceDictionary));
		}
		foreach (MetadataArtifactLoaderFile ssdlChild in SsdlChildren)
		{
			list.AddRange(ssdlChild.GetReaders(sourceDictionary));
		}
		foreach (MetadataArtifactLoaderFile mslChild in MslChildren)
		{
			list.AddRange(mslChild.GetReaders(sourceDictionary));
		}
		return list;
	}

	public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
	{
		List<XmlReader> list = new List<XmlReader>();
		if (!TryGetListForSpace(spaceToGet, out var files))
		{
			return list;
		}
		foreach (MetadataArtifactLoaderFile item in files)
		{
			list.AddRange(item.CreateReaders(spaceToGet));
		}
		return list;
	}

	private static List<MetadataArtifactLoaderFile> GetArtifactsInDirectory(string directory, string extension, ICollection<string> uriRegistry)
	{
		List<MetadataArtifactLoaderFile> list = new List<MetadataArtifactLoaderFile>();
		string[] files = Directory.GetFiles(directory, MetadataArtifactLoader.wildcard + extension, SearchOption.TopDirectoryOnly);
		foreach (string text in files)
		{
			string text2 = System.IO.Path.Combine(directory, text);
			if (!uriRegistry.Contains(text2) && text.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(new MetadataArtifactLoaderFile(text2, uriRegistry));
			}
		}
		return list;
	}
}
