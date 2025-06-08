using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class ObjectItemCachedAssemblyLoader : ObjectItemAssemblyLoader
{
	private new ImmutableAssemblyCacheEntry CacheEntry => (ImmutableAssemblyCacheEntry)base.CacheEntry;

	internal ObjectItemCachedAssemblyLoader(Assembly assembly, ImmutableAssemblyCacheEntry cacheEntry, ObjectItemLoadingSessionData sessionData)
		: base(assembly, cacheEntry, sessionData)
	{
	}

	protected override void AddToAssembliesLoaded()
	{
	}

	protected override void LoadTypesFromAssembly()
	{
		foreach (EdmType item in CacheEntry.TypesInAssembly)
		{
			if (!base.SessionData.TypesInLoading.ContainsKey(item.Identity))
			{
				base.SessionData.TypesInLoading.Add(item.Identity, item);
			}
		}
	}
}
