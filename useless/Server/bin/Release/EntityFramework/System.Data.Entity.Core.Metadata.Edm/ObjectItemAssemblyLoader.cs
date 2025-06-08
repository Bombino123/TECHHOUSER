using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class ObjectItemAssemblyLoader
{
	private readonly ObjectItemLoadingSessionData _sessionData;

	private readonly Assembly _assembly;

	private readonly AssemblyCacheEntry _cacheEntry;

	protected ObjectItemLoadingSessionData SessionData => _sessionData;

	protected Assembly SourceAssembly => _assembly;

	protected AssemblyCacheEntry CacheEntry => _cacheEntry;

	protected ObjectItemAssemblyLoader(Assembly assembly, AssemblyCacheEntry cacheEntry, ObjectItemLoadingSessionData sessionData)
	{
		_assembly = assembly;
		_cacheEntry = cacheEntry;
		_sessionData = sessionData;
	}

	internal virtual void Load()
	{
		AddToAssembliesLoaded();
		LoadTypesFromAssembly();
		AddToKnownAssemblies();
		LoadClosureAssemblies();
	}

	protected abstract void AddToAssembliesLoaded();

	protected abstract void LoadTypesFromAssembly();

	protected virtual void LoadClosureAssemblies()
	{
		LoadAssemblies(CacheEntry.ClosureAssemblies, SessionData);
	}

	internal virtual void OnLevel1SessionProcessing()
	{
	}

	internal virtual void OnLevel2SessionProcessing()
	{
	}

	internal static ObjectItemAssemblyLoader CreateLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
	{
		if (sessionData.KnownAssemblies.Contains(assembly, sessionData.ObjectItemAssemblyLoaderFactory, sessionData.EdmItemCollection))
		{
			return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
		}
		if (sessionData.LockedAssemblyCache.TryGetValue(assembly, out var cacheEntry))
		{
			if (sessionData.ObjectItemAssemblyLoaderFactory == null)
			{
				if (cacheEntry.TypesInAssembly.Count != 0)
				{
					sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemAttributeAssemblyLoader.Create;
				}
			}
			else if (sessionData.ObjectItemAssemblyLoaderFactory != new Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>(ObjectItemAttributeAssemblyLoader.Create))
			{
				sessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_AttributeAssemblyReferenced(assembly.FullName)));
			}
			return new ObjectItemCachedAssemblyLoader(assembly, cacheEntry, sessionData);
		}
		if (sessionData.EdmItemCollection != null && sessionData.EdmItemCollection.ConventionalOcCache.TryGetConventionalOcCacheFromAssemblyCache(assembly, out cacheEntry))
		{
			sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemConventionAssemblyLoader.Create;
			return new ObjectItemCachedAssemblyLoader(assembly, cacheEntry, sessionData);
		}
		if (sessionData.ObjectItemAssemblyLoaderFactory == null)
		{
			if (ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(assembly))
			{
				sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemAttributeAssemblyLoader.Create;
			}
			else if (ObjectItemConventionAssemblyLoader.SessionContainsConventionParameters(sessionData))
			{
				sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemConventionAssemblyLoader.Create;
			}
		}
		if (sessionData.ObjectItemAssemblyLoaderFactory != null)
		{
			return sessionData.ObjectItemAssemblyLoaderFactory(assembly, sessionData);
		}
		return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
	}

	internal static bool IsAttributeLoader(object loaderCookie)
	{
		return IsAttributeLoader(loaderCookie as Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>);
	}

	internal static bool IsAttributeLoader(Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> loaderFactory)
	{
		if (loaderFactory == null)
		{
			return false;
		}
		return loaderFactory == new Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>(ObjectItemAttributeAssemblyLoader.Create);
	}

	internal static bool IsConventionLoader(Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> loaderFactory)
	{
		if (loaderFactory == null)
		{
			return false;
		}
		return loaderFactory == new Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>(ObjectItemConventionAssemblyLoader.Create);
	}

	protected virtual void AddToKnownAssemblies()
	{
		_sessionData.KnownAssemblies.Add(_assembly, new KnownAssemblyEntry(CacheEntry, SessionData.EdmItemCollection != null));
	}

	protected static void LoadAssemblies(IEnumerable<Assembly> assemblies, ObjectItemLoadingSessionData sessionData)
	{
		foreach (Assembly assembly in assemblies)
		{
			CreateLoader(assembly, sessionData).Load();
		}
	}

	protected static bool TryGetPrimitiveType(Type type, out PrimitiveType primitiveType)
	{
		return ClrProviderManifest.Instance.TryGetPrimitiveType(Nullable.GetUnderlyingType(type) ?? type, out primitiveType);
	}
}
