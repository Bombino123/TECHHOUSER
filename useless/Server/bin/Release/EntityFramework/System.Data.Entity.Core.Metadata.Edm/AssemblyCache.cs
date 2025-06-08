using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class AssemblyCache
{
	private static readonly Dictionary<Assembly, ImmutableAssemblyCacheEntry> _globalAssemblyCache = new Dictionary<Assembly, ImmutableAssemblyCacheEntry>();

	private static readonly object _assemblyCacheLock = new object();

	internal static LockedAssemblyCache AcquireLockedAssemblyCache()
	{
		return new LockedAssemblyCache(_assemblyCacheLock, _globalAssemblyCache);
	}

	internal static void LoadAssembly(Assembly assembly, bool loadReferencedAssemblies, KnownAssembliesSet knownAssemblies, out Dictionary<string, EdmType> typesInLoading, out List<EdmItemError> errors)
	{
		object loaderCookie = null;
		LoadAssembly(assembly, loadReferencedAssemblies, knownAssemblies, null, null, ref loaderCookie, out typesInLoading, out errors);
	}

	internal static void LoadAssembly(Assembly assembly, bool loadReferencedAssemblies, KnownAssembliesSet knownAssemblies, EdmItemCollection edmItemCollection, Action<string> logLoadMessage, ref object loaderCookie, out Dictionary<string, EdmType> typesInLoading, out List<EdmItemError> errors)
	{
		typesInLoading = null;
		errors = null;
		using LockedAssemblyCache lockedAssemblyCache = AcquireLockedAssemblyCache();
		ObjectItemLoadingSessionData objectItemLoadingSessionData = new ObjectItemLoadingSessionData(knownAssemblies, lockedAssemblyCache, edmItemCollection, logLoadMessage, loaderCookie);
		LoadAssembly(assembly, loadReferencedAssemblies, objectItemLoadingSessionData);
		loaderCookie = objectItemLoadingSessionData.LoaderCookie;
		objectItemLoadingSessionData.CompleteSession();
		if (objectItemLoadingSessionData.EdmItemErrors.Count == 0)
		{
			EdmValidator edmValidator = new EdmValidator();
			edmValidator.SkipReadOnlyItems = true;
			edmValidator.Validate(objectItemLoadingSessionData.TypesInLoading.Values, objectItemLoadingSessionData.EdmItemErrors);
			if (objectItemLoadingSessionData.EdmItemErrors.Count == 0)
			{
				if (ObjectItemAssemblyLoader.IsAttributeLoader(objectItemLoadingSessionData.ObjectItemAssemblyLoaderFactory))
				{
					UpdateCache(lockedAssemblyCache, objectItemLoadingSessionData.AssembliesLoaded);
				}
				else if (objectItemLoadingSessionData.EdmItemCollection != null && ObjectItemAssemblyLoader.IsConventionLoader(objectItemLoadingSessionData.ObjectItemAssemblyLoaderFactory))
				{
					UpdateCache(objectItemLoadingSessionData.EdmItemCollection, objectItemLoadingSessionData.AssembliesLoaded);
				}
			}
		}
		if (objectItemLoadingSessionData.TypesInLoading.Count > 0)
		{
			foreach (EdmType value in objectItemLoadingSessionData.TypesInLoading.Values)
			{
				value.SetReadOnly();
			}
		}
		typesInLoading = objectItemLoadingSessionData.TypesInLoading;
		errors = objectItemLoadingSessionData.EdmItemErrors;
	}

	private static void LoadAssembly(Assembly assembly, bool loadReferencedAssemblies, ObjectItemLoadingSessionData loadingData)
	{
		bool flag = false;
		if (loadingData.KnownAssemblies.TryGetKnownAssembly(assembly, loadingData.ObjectItemAssemblyLoaderFactory, loadingData.EdmItemCollection, out var entry))
		{
			flag = !entry.ReferencedAssembliesAreLoaded && loadReferencedAssemblies;
		}
		else
		{
			ObjectItemAssemblyLoader.CreateLoader(assembly, loadingData).Load();
			flag = loadReferencedAssemblies;
		}
		if (!flag)
		{
			return;
		}
		if ((entry == null && loadingData.KnownAssemblies.TryGetKnownAssembly(assembly, loadingData.ObjectItemAssemblyLoaderFactory, loadingData.EdmItemCollection, out entry)) || entry != null)
		{
			entry.ReferencedAssembliesAreLoaded = true;
		}
		foreach (Assembly nonSystemReferencedAssembly in MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(assembly))
		{
			LoadAssembly(nonSystemReferencedAssembly, loadReferencedAssemblies, loadingData);
		}
	}

	private static void UpdateCache(EdmItemCollection edmItemCollection, Dictionary<Assembly, MutableAssemblyCacheEntry> assemblies)
	{
		foreach (KeyValuePair<Assembly, MutableAssemblyCacheEntry> assembly in assemblies)
		{
			edmItemCollection.ConventionalOcCache.AddAssemblyToOcCacheFromAssemblyCache(assembly.Key, new ImmutableAssemblyCacheEntry(assembly.Value));
		}
	}

	private static void UpdateCache(LockedAssemblyCache lockedAssemblyCache, Dictionary<Assembly, MutableAssemblyCacheEntry> assemblies)
	{
		foreach (KeyValuePair<Assembly, MutableAssemblyCacheEntry> assembly in assemblies)
		{
			lockedAssemblyCache.Add(assembly.Key, new ImmutableAssemblyCacheEntry(assembly.Value));
		}
	}
}
