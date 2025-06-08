namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class KnownAssemblyEntry
{
	private readonly AssemblyCacheEntry _cacheEntry;

	internal AssemblyCacheEntry CacheEntry => _cacheEntry;

	public bool ReferencedAssembliesAreLoaded { get; set; }

	public bool SeenWithEdmItemCollection { get; set; }

	internal KnownAssemblyEntry(AssemblyCacheEntry cacheEntry, bool seenWithEdmItemCollection)
	{
		_cacheEntry = cacheEntry;
		ReferencedAssembliesAreLoaded = false;
		SeenWithEdmItemCollection = seenWithEdmItemCollection;
	}

	public bool HaveSeenInCompatibleContext(object loaderCookie, EdmItemCollection itemCollection)
	{
		if (!SeenWithEdmItemCollection && itemCollection != null)
		{
			return ObjectItemAssemblyLoader.IsAttributeLoader(loaderCookie);
		}
		return true;
	}
}
