using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Resources;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataArtifactLoaderCompositeResource : MetadataArtifactLoader
{
	private readonly ReadOnlyCollection<MetadataArtifactLoaderResource> _children;

	private readonly string _originalPath;

	public override string Path => _originalPath;

	public override bool IsComposite => true;

	internal MetadataArtifactLoaderCompositeResource(string originalPath, string assemblyName, string resourceName, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
	{
		_originalPath = originalPath;
		_children = new ReadOnlyCollection<MetadataArtifactLoaderResource>(LoadResources(assemblyName, resourceName, uriRegistry, resolver));
	}

	public override List<string> GetOriginalPaths(DataSpace spaceToGet)
	{
		return GetOriginalPaths();
	}

	public override List<string> GetPaths(DataSpace spaceToGet)
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoaderResource child in _children)
		{
			list.AddRange(child.GetPaths(spaceToGet));
		}
		return list;
	}

	public override List<string> GetPaths()
	{
		List<string> list = new List<string>();
		foreach (MetadataArtifactLoaderResource child in _children)
		{
			list.AddRange(child.GetPaths());
		}
		return list;
	}

	public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
	{
		List<XmlReader> list = new List<XmlReader>();
		foreach (MetadataArtifactLoaderResource child in _children)
		{
			list.AddRange(child.GetReaders(sourceDictionary));
		}
		return list;
	}

	public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
	{
		List<XmlReader> list = new List<XmlReader>();
		foreach (MetadataArtifactLoaderResource child in _children)
		{
			list.AddRange(child.CreateReaders(spaceToGet));
		}
		return list;
	}

	private static List<MetadataArtifactLoaderResource> LoadResources(string assemblyName, string resourceName, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
	{
		List<MetadataArtifactLoaderResource> list = new List<MetadataArtifactLoaderResource>();
		if (assemblyName == MetadataArtifactLoader.wildcard)
		{
			foreach (Assembly wildcardAssembly in resolver.GetWildcardAssemblies())
			{
				if (AssemblyContainsResource(wildcardAssembly, ref resourceName))
				{
					LoadResourcesFromAssembly(wildcardAssembly, resourceName, uriRegistry, list);
				}
			}
		}
		else
		{
			LoadResourcesFromAssembly(ResolveAssemblyName(assemblyName, resolver), resourceName, uriRegistry, list);
		}
		if (resourceName != null && list.Count == 0)
		{
			throw new MetadataException(Strings.UnableToLoadResource);
		}
		return list;
	}

	private static bool AssemblyContainsResource(Assembly assembly, ref string resourceName)
	{
		if (resourceName == null)
		{
			return true;
		}
		string[] manifestResourceNamesForAssembly = GetManifestResourceNamesForAssembly(assembly);
		foreach (string text in manifestResourceNamesForAssembly)
		{
			if (string.Equals(resourceName, text, StringComparison.OrdinalIgnoreCase))
			{
				resourceName = text;
				return true;
			}
		}
		return false;
	}

	private static void LoadResourcesFromAssembly(Assembly assembly, string resourceName, ICollection<string> uriRegistry, List<MetadataArtifactLoaderResource> loaders)
	{
		if (resourceName == null)
		{
			LoadAllResourcesFromAssembly(assembly, uriRegistry, loaders);
			return;
		}
		if (AssemblyContainsResource(assembly, ref resourceName))
		{
			CreateAndAddSingleResourceLoader(assembly, resourceName, uriRegistry, loaders);
			return;
		}
		throw new MetadataException(Strings.UnableToLoadResource);
	}

	private static void LoadAllResourcesFromAssembly(Assembly assembly, ICollection<string> uriRegistry, List<MetadataArtifactLoaderResource> loaders)
	{
		string[] manifestResourceNamesForAssembly = GetManifestResourceNamesForAssembly(assembly);
		foreach (string resourceName in manifestResourceNamesForAssembly)
		{
			CreateAndAddSingleResourceLoader(assembly, resourceName, uriRegistry, loaders);
		}
	}

	private static void CreateAndAddSingleResourceLoader(Assembly assembly, string resourceName, ICollection<string> uriRegistry, List<MetadataArtifactLoaderResource> loaders)
	{
		string item = CreateResPath(assembly, resourceName);
		if (!uriRegistry.Contains(item))
		{
			loaders.Add(new MetadataArtifactLoaderResource(assembly, resourceName, uriRegistry));
		}
	}

	internal static string CreateResPath(Assembly assembly, string resourceName)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", MetadataArtifactLoader.resPathPrefix, assembly.FullName, MetadataArtifactLoader.resPathSeparator, resourceName);
	}

	internal static string[] GetManifestResourceNamesForAssembly(Assembly assembly)
	{
		if (assembly.IsDynamic)
		{
			return new string[0];
		}
		return assembly.GetManifestResourceNames();
	}

	private static Assembly ResolveAssemblyName(string assemblyName, MetadataArtifactAssemblyResolver resolver)
	{
		AssemblyName referenceName = new AssemblyName(assemblyName);
		if (!resolver.TryResolveAssemblyReference(referenceName, out var assembly))
		{
			throw new FileNotFoundException(Strings.UnableToResolveAssembly(assemblyName));
		}
		return assembly;
	}

	internal static MetadataArtifactLoader CreateResourceLoader(string path, ExtensionCheck extensionCheck, string validExtension, ICollection<string> uriRegistry, MetadataArtifactAssemblyResolver resolver)
	{
		string assemblyName = null;
		string resourceName = null;
		ParseResourcePath(path, out assemblyName, out resourceName);
		bool num = assemblyName != null && (resourceName == null || assemblyName.Trim() == MetadataArtifactLoader.wildcard);
		ValidateExtension(extensionCheck, validExtension, resourceName);
		if (num)
		{
			return new MetadataArtifactLoaderCompositeResource(path, assemblyName, resourceName, uriRegistry, resolver);
		}
		return new MetadataArtifactLoaderResource(ResolveAssemblyName(assemblyName, resolver), resourceName, uriRegistry);
	}

	private static void ValidateExtension(ExtensionCheck extensionCheck, string validExtension, string resourceName)
	{
		if (resourceName == null)
		{
			return;
		}
		switch (extensionCheck)
		{
		case ExtensionCheck.Specific:
			MetadataArtifactLoader.CheckArtifactExtension(resourceName, validExtension);
			break;
		case ExtensionCheck.All:
			if (!MetadataArtifactLoader.IsValidArtifact(resourceName))
			{
				throw new MetadataException(Strings.InvalidMetadataPath);
			}
			break;
		}
	}

	private static void ParseResourcePath(string path, out string assemblyName, out string resourceName)
	{
		int length = MetadataArtifactLoader.resPathPrefix.Length;
		string[] array = path.Substring(length).Split(new string[2]
		{
			MetadataArtifactLoader.resPathSeparator,
			MetadataArtifactLoader.altPathSeparator
		}, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0 || array.Length > 2)
		{
			throw new MetadataException(Strings.InvalidMetadataPath);
		}
		if (array.Length >= 1)
		{
			assemblyName = array[0];
		}
		else
		{
			assemblyName = null;
		}
		if (array.Length == 2)
		{
			resourceName = array[1];
		}
		else
		{
			resourceName = null;
		}
	}
}
