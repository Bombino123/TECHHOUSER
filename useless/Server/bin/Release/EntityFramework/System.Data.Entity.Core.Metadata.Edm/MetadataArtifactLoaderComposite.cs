using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataArtifactLoaderComposite : MetadataArtifactLoader, IEnumerable<MetadataArtifactLoader>, IEnumerable
{
	private readonly ReadOnlyCollection<MetadataArtifactLoader> _children;

	public override string Path => string.Empty;

	public override bool IsComposite => true;

	public MetadataArtifactLoaderComposite(List<MetadataArtifactLoader> children)
	{
		_children = new ReadOnlyCollection<MetadataArtifactLoader>(new List<MetadataArtifactLoader>(children));
	}

	public override List<string> GetOriginalPaths()
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoader child in _children)
		{
			list.AddRange(child.GetOriginalPaths());
		}
		return list;
	}

	public override List<string> GetOriginalPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoader child in _children)
		{
			list.AddRange(child.GetOriginalPaths(spaceToGet));
		}
		return list;
	}

	public override List<string> GetPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoader child in _children)
		{
			list.AddRange(child.GetPaths(spaceToGet));
		}
		return list;
	}

	public override List<string> GetPaths()
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoader child in _children)
		{
			list.AddRange(child.GetPaths());
		}
		return list;
	}

	public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
	{
		List<XmlReader> list = new List<XmlReader>();
		foreach (MetadataArtifactLoader child in _children)
		{
			list.AddRange(child.GetReaders(sourceDictionary));
		}
		return list;
	}

	public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
	{
		List<XmlReader> list = new List<XmlReader>();
		foreach (MetadataArtifactLoader child in _children)
		{
			list.AddRange(child.CreateReaders(spaceToGet));
		}
		return list;
	}

	public IEnumerator<MetadataArtifactLoader> GetEnumerator()
	{
		return _children.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _children.GetEnumerator();
	}
}
