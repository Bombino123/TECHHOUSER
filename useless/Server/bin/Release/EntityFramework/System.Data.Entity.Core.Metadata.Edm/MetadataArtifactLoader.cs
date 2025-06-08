using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Resources;
using System.IO;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class MetadataArtifactLoader
{
	public enum ExtensionCheck
	{
		None,
		Specific,
		All
	}

	protected static readonly string resPathPrefix = "res://";

	protected static readonly string resPathSeparator = "/";

	protected static readonly string altPathSeparator = "\\";

	protected static readonly string wildcard = "*";

	public abstract string Path { get; }

	public virtual bool IsComposite => false;

	public static MetadataArtifactLoader Create(string path, ExtensionCheck extensionCheck, string validExtension, ICollection<string> uriRegistry)
	{
		return Create(path, extensionCheck, validExtension, uriRegistry, new DefaultAssemblyResolver());
	}

	internal static MetadataArtifactLoader Create(string path, ExtensionCheck extensionCheck, string validExtension, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
	{
		if (PathStartsWithResPrefix(path))
		{
			return MetadataArtifactLoaderCompositeResource.CreateResourceLoader(path, extensionCheck, validExtension, uriRegistry, resolver);
		}
		string text = NormalizeFilePaths(path);
		if (Directory.Exists(text))
		{
			return new MetadataArtifactLoaderCompositeFile(text, uriRegistry);
		}
		if (File.Exists(text))
		{
			switch (extensionCheck)
			{
			case ExtensionCheck.Specific:
				CheckArtifactExtension(text, validExtension);
				break;
			case ExtensionCheck.All:
				if (!IsValidArtifact(text))
				{
					throw new MetadataException(Strings.InvalidMetadataPath);
				}
				break;
			}
			return new MetadataArtifactLoaderFile(text, uriRegistry);
		}
		throw new MetadataException(Strings.InvalidMetadataPath);
	}

	public static MetadataArtifactLoader Create(List<MetadataArtifactLoader> allCollections)
	{
		return new MetadataArtifactLoaderComposite(allCollections);
	}

	public static MetadataArtifactLoader CreateCompositeFromFilePaths(IEnumerable<string> filePaths, string validExtension)
	{
		return CreateCompositeFromFilePaths(filePaths, validExtension, new DefaultAssemblyResolver());
	}

	internal static MetadataArtifactLoader CreateCompositeFromFilePaths(IEnumerable<string> filePaths, string validExtension, MetadataArtifactAssemblyResolver resolver)
	{
		ExtensionCheck extensionCheck = ((!string.IsNullOrEmpty(validExtension)) ? ExtensionCheck.Specific : ExtensionCheck.All);
		List<MetadataArtifactLoader> list = new List<MetadataArtifactLoader>();
		HashSet<string> uriRegistry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string filePath in filePaths)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new MetadataException(Strings.NotValidInputPath, new ArgumentException(Strings.ADP_CollectionParameterElementIsNullOrEmpty("filePaths")));
			}
			string text = filePath.Trim();
			if (text.Length > 0)
			{
				list.Add(Create(text, extensionCheck, validExtension, uriRegistry, resolver));
			}
		}
		return Create(list);
	}

	public static MetadataArtifactLoader CreateCompositeFromXmlReaders(IEnumerable<XmlReader> xmlReaders)
	{
		List<MetadataArtifactLoader> list = new List<MetadataArtifactLoader>();
		foreach (XmlReader xmlReader in xmlReaders)
		{
			if (xmlReader == null)
			{
				throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("xmlReaders"));
			}
			list.Add(new MetadataArtifactLoaderXmlReaderWrapper(xmlReader));
		}
		return Create(list);
	}

	internal static void CheckArtifactExtension(string path, string validExtension)
	{
		string extension = GetExtension(path);
		if (!extension.Equals(validExtension, StringComparison.OrdinalIgnoreCase))
		{
			throw new MetadataException(Strings.InvalidFileExtension(path, extension, validExtension));
		}
	}

	public virtual List<string> GetOriginalPaths()
	{
		return new List<string>(new string[1] { Path });
	}

	public virtual List<string> GetOriginalPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		if (IsArtifactOfDataSpace(Path, spaceToGet))
		{
			list.Add(Path);
		}
		return list;
	}

	public abstract List<string> GetPaths();

	public abstract List<string> GetPaths(DataSpace spaceToGet);

	public List<XmlReader> GetReaders()
	{
		return GetReaders(null);
	}

	public abstract List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary);

	public abstract List<XmlReader> CreateReaders(DataSpace spaceToGet);

	internal static bool PathStartsWithResPrefix(string path)
	{
		return path.StartsWith(resPathPrefix, StringComparison.OrdinalIgnoreCase);
	}

	protected static bool IsCSpaceArtifact(string resource)
	{
		string extension = GetExtension(resource);
		if (!string.IsNullOrEmpty(extension))
		{
			return string.Compare(extension, ".csdl", StringComparison.OrdinalIgnoreCase) == 0;
		}
		return false;
	}

	protected static bool IsSSpaceArtifact(string resource)
	{
		string extension = GetExtension(resource);
		if (!string.IsNullOrEmpty(extension))
		{
			return string.Compare(extension, ".ssdl", StringComparison.OrdinalIgnoreCase) == 0;
		}
		return false;
	}

	protected static bool IsCSSpaceArtifact(string resource)
	{
		string extension = GetExtension(resource);
		if (!string.IsNullOrEmpty(extension))
		{
			return string.Compare(extension, ".msl", StringComparison.OrdinalIgnoreCase) == 0;
		}
		return false;
	}

	private static string GetExtension(string resource)
	{
		if (string.IsNullOrEmpty(resource))
		{
			return string.Empty;
		}
		int num = resource.LastIndexOf('.');
		if (num < 0)
		{
			return string.Empty;
		}
		return resource.Substring(num);
	}

	internal static bool IsValidArtifact(string resource)
	{
		string extension = GetExtension(resource);
		if (!string.IsNullOrEmpty(extension))
		{
			if (string.Compare(extension, ".csdl", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(extension, ".ssdl", StringComparison.OrdinalIgnoreCase) != 0)
			{
				return string.Compare(extension, ".msl", StringComparison.OrdinalIgnoreCase) == 0;
			}
			return true;
		}
		return false;
	}

	protected static bool IsArtifactOfDataSpace(string resource, DataSpace dataSpace)
	{
		return dataSpace switch
		{
			DataSpace.CSpace => IsCSpaceArtifact(resource), 
			DataSpace.SSpace => IsSSpaceArtifact(resource), 
			DataSpace.CSSpace => IsCSSpaceArtifact(resource), 
			_ => false, 
		};
	}

	internal static string NormalizeFilePaths(string path)
	{
		bool flag = true;
		if (!string.IsNullOrEmpty(path))
		{
			path = path.Trim();
			if (path.StartsWith("~", StringComparison.Ordinal))
			{
				path = new AspProxy().MapWebPath(path);
				flag = false;
			}
			if (path.Length == 2 && path[1] == System.IO.Path.VolumeSeparatorChar)
			{
				string text = path;
				char directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
				path = text + directorySeparatorChar;
			}
			else
			{
				string text2 = DbProviderServices.ExpandDataDirectory(path);
				if (!path.Equals(text2, StringComparison.Ordinal))
				{
					path = text2;
					flag = false;
				}
			}
		}
		try
		{
			if (flag)
			{
				path = System.IO.Path.GetFullPath(path);
			}
		}
		catch (ArgumentException innerException)
		{
			throw new MetadataException(Strings.NotValidInputPath, innerException);
		}
		catch (NotSupportedException innerException2)
		{
			throw new MetadataException(Strings.NotValidInputPath, innerException2);
		}
		catch (PathTooLongException)
		{
			throw new MetadataException(Strings.NotValidInputPath);
		}
		return path;
	}
}
