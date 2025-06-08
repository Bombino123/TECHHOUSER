using System;
using System.Collections.Generic;

namespace dnlib.DotNet.Resources;

public sealed class ResourceElementSet
{
	internal const string DeserializingResourceReaderTypeNameRegex = "^System\\.Resources\\.Extensions\\.DeserializingResourceReader,\\s*System\\.Resources\\.Extensions";

	internal const string ResourceReaderTypeNameRegex = "^System\\.Resources\\.ResourceReader,\\s*mscorlib";

	private readonly Dictionary<string, ResourceElement> dict = new Dictionary<string, ResourceElement>(StringComparer.Ordinal);

	public string ResourceReaderTypeName { get; }

	public string ResourceSetTypeName { get; }

	public ResourceReaderType ReaderType { get; }

	public int FormatVersion { get; internal set; }

	public int Count => dict.Count;

	public IEnumerable<ResourceElement> ResourceElements => dict.Values;

	internal ResourceElementSet(string resourceReaderTypeName, string resourceSetTypeName, ResourceReaderType readerType)
	{
		ResourceReaderTypeName = resourceReaderTypeName;
		ResourceSetTypeName = resourceSetTypeName;
		ReaderType = readerType;
	}

	public void Add(ResourceElement elem)
	{
		dict[elem.Name] = elem;
	}

	public static ResourceElementSet CreateForDeserializingResourceReader(Version extensionAssemblyVersion, int formatVersion = 2)
	{
		string text = "System.Resources.Extensions, Version=" + extensionAssemblyVersion.ToString(4) + ", Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
		return new ResourceElementSet("System.Resources.Extensions.DeserializingResourceReader, " + text, "System.Resources.Extensions.RuntimeResourceSet, " + text, ResourceReaderType.DeserializingResourceReader)
		{
			FormatVersion = formatVersion
		};
	}

	public static ResourceElementSet CreateForResourceReader(ModuleDef module, int formatVersion = 2)
	{
		string text = ((!(module.CorLibTypes.AssemblyRef.Name == "mscorlib")) ? "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" : module.CorLibTypes.AssemblyRef.FullName);
		return new ResourceElementSet("System.Resources.ResourceReader, " + text, "System.Resources.RuntimeResourceSet", ResourceReaderType.ResourceReader)
		{
			FormatVersion = formatVersion
		};
	}

	public static ResourceElementSet CreateForResourceReader(Version mscorlibVersion, int formatVersion = 2)
	{
		return new ResourceElementSet("System.Resources.ResourceReader, mscorlib, Version=" + mscorlibVersion.ToString(4) + ", Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Resources.RuntimeResourceSet", ResourceReaderType.ResourceReader)
		{
			FormatVersion = formatVersion
		};
	}

	public ResourceElementSet Clone()
	{
		return new ResourceElementSet(ResourceReaderTypeName, ResourceSetTypeName, ReaderType)
		{
			FormatVersion = FormatVersion
		};
	}
}
