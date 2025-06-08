using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MetadataCache
{
	private const string DataDirectory = "|datadirectory|";

	private const string MetadataPathSeparator = "|";

	private const string SemicolonSeparator = ";";

	public static readonly MetadataCache Instance = new MetadataCache();

	private Memoizer<string, List<MetadataArtifactLoader>> _artifactLoaderCache = new Memoizer<string, List<MetadataArtifactLoader>>(SplitPaths, null);

	private readonly ConcurrentDictionary<string, MetadataWorkspace> _cachedWorkspaces = new ConcurrentDictionary<string, MetadataWorkspace>();

	private static List<MetadataArtifactLoader> SplitPaths(string paths)
	{
		HashSet<string> uriRegistry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<string> list = new List<string>();
		for (int num = paths.IndexOf("|datadirectory|", StringComparison.OrdinalIgnoreCase); num != -1; num = paths.IndexOf("|datadirectory|", StringComparison.OrdinalIgnoreCase))
		{
			int num2 = ((num == 0) ? (-1) : paths.LastIndexOf("|", num - 1, StringComparison.Ordinal)) + 1;
			int num3 = paths.IndexOf("|", num + "|datadirectory|".Length, StringComparison.Ordinal);
			if (num3 == -1)
			{
				list.Add(paths.Substring(num2));
				paths = paths.Remove(num2);
				break;
			}
			list.Add(paths.Substring(num2, num3 - num2));
			paths = paths.Remove(num2, num3 - num2);
		}
		string[] array = paths.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
		if (list.Count > 0)
		{
			list.AddRange(array);
			array = list.ToArray();
		}
		List<MetadataArtifactLoader> list2 = new List<MetadataArtifactLoader>();
		List<MetadataArtifactLoader> list3 = new List<MetadataArtifactLoader>();
		List<MetadataArtifactLoader> list4 = new List<MetadataArtifactLoader>();
		List<MetadataArtifactLoader> list5 = new List<MetadataArtifactLoader>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
			if (array[i].Length > 0)
			{
				MetadataArtifactLoader item = MetadataArtifactLoader.Create(array[i], MetadataArtifactLoader.ExtensionCheck.All, null, uriRegistry);
				if (array[i].EndsWith(".csdl", StringComparison.OrdinalIgnoreCase))
				{
					list2.Add(item);
				}
				else if (array[i].EndsWith(".msl", StringComparison.OrdinalIgnoreCase))
				{
					list3.Add(item);
				}
				else if (array[i].EndsWith(".ssdl", StringComparison.OrdinalIgnoreCase))
				{
					list4.Add(item);
				}
				else
				{
					list5.Add(item);
				}
			}
		}
		list5.AddRange(list4);
		list5.AddRange(list3);
		list5.AddRange(list2);
		return list5;
	}

	public MetadataWorkspace GetMetadataWorkspace(DbConnectionOptions effectiveConnectionOptions)
	{
		MetadataArtifactLoader artifactLoader = GetArtifactLoader(effectiveConnectionOptions);
		string cacheKey = CreateMetadataCacheKey(artifactLoader.GetPaths(), effectiveConnectionOptions["provider"]);
		return GetMetadataWorkspace(cacheKey, artifactLoader);
	}

	public MetadataArtifactLoader GetArtifactLoader(DbConnectionOptions effectiveConnectionOptions)
	{
		string text = effectiveConnectionOptions["metadata"];
		if (!string.IsNullOrEmpty(text))
		{
			List<MetadataArtifactLoader> list = _artifactLoaderCache.Evaluate(text);
			return MetadataArtifactLoader.Create(ShouldRecalculateMetadataArtifactLoader(list) ? SplitPaths(text) : list);
		}
		return MetadataArtifactLoader.Create(new List<MetadataArtifactLoader>());
	}

	public MetadataWorkspace GetMetadataWorkspace(string cacheKey, MetadataArtifactLoader artifactLoader)
	{
		return _cachedWorkspaces.GetOrAdd(cacheKey, delegate
		{
			EdmItemCollection edmItemCollection = LoadEdmItemCollection(artifactLoader);
			Lazy<StorageMappingItemCollection> mappingLoader = new Lazy<StorageMappingItemCollection>(() => LoadStoreCollection(edmItemCollection, artifactLoader));
			return new MetadataWorkspace(() => edmItemCollection, () => mappingLoader.Value.StoreItemCollection, () => mappingLoader.Value);
		});
	}

	public void Clear()
	{
		_cachedWorkspaces.Clear();
		Interlocked.CompareExchange(ref _artifactLoaderCache, new Memoizer<string, List<MetadataArtifactLoader>>(SplitPaths, null), _artifactLoaderCache);
	}

	private static StorageMappingItemCollection LoadStoreCollection(EdmItemCollection edmItemCollection, MetadataArtifactLoader loader)
	{
		List<XmlReader> xmlReaders = loader.CreateReaders(DataSpace.SSpace);
		StoreItemCollection storeCollection;
		try
		{
			storeCollection = new StoreItemCollection(xmlReaders, loader.GetPaths(DataSpace.SSpace));
		}
		finally
		{
			Helper.DisposeXmlReaders(xmlReaders);
		}
		List<XmlReader> xmlReaders2 = loader.CreateReaders(DataSpace.CSSpace);
		try
		{
			return new StorageMappingItemCollection(edmItemCollection, storeCollection, xmlReaders2, loader.GetPaths(DataSpace.CSSpace));
		}
		finally
		{
			Helper.DisposeXmlReaders(xmlReaders2);
		}
	}

	private static EdmItemCollection LoadEdmItemCollection(MetadataArtifactLoader loader)
	{
		List<XmlReader> xmlReaders = loader.CreateReaders(DataSpace.CSpace);
		try
		{
			return new EdmItemCollection(xmlReaders, loader.GetPaths(DataSpace.CSpace));
		}
		finally
		{
			Helper.DisposeXmlReaders(xmlReaders);
		}
	}

	private static bool ShouldRecalculateMetadataArtifactLoader(IEnumerable<MetadataArtifactLoader> loaders)
	{
		return loaders.Any((MetadataArtifactLoader loader) => loader.GetType() == typeof(MetadataArtifactLoaderCompositeFile));
	}

	private static string CreateMetadataCacheKey(IList<string> paths, string providerName)
	{
		int resultCount = 0;
		CreateMetadataCacheKeyWithCount(paths, providerName, buildResult: false, ref resultCount, out var result);
		CreateMetadataCacheKeyWithCount(paths, providerName, buildResult: true, ref resultCount, out result);
		return result;
	}

	private static void CreateMetadataCacheKeyWithCount(IList<string> paths, string providerName, bool buildResult, ref int resultCount, out string result)
	{
		StringBuilder stringBuilder = (buildResult ? new StringBuilder(resultCount) : null);
		resultCount = 0;
		if (!string.IsNullOrEmpty(providerName))
		{
			resultCount += providerName.Length + 1;
			if (buildResult)
			{
				stringBuilder.Append(providerName);
				stringBuilder.Append(";");
			}
		}
		if (paths != null)
		{
			for (int i = 0; i < paths.Count; i++)
			{
				if (paths[i].Length <= 0)
				{
					continue;
				}
				if (i > 0)
				{
					resultCount++;
					if (buildResult)
					{
						stringBuilder.Append("|");
					}
				}
				resultCount += paths[i].Length;
				if (buildResult)
				{
					stringBuilder.Append(paths[i]);
				}
			}
			resultCount++;
			if (buildResult)
			{
				stringBuilder.Append(";");
			}
		}
		result = (buildResult ? stringBuilder.ToString() : null);
	}
}
