using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class DefaultAssemblyResolver : MetadataArtifactAssemblyResolver
{
	internal sealed class AssemblyComparer : IEqualityComparer<Assembly>
	{
		private static readonly AssemblyComparer _instance = new AssemblyComparer();

		public static AssemblyComparer Instance => _instance;

		private AssemblyComparer()
		{
		}

		public bool Equals(Assembly x, Assembly y)
		{
			AssemblyName assemblyName = new AssemblyName(x.FullName);
			AssemblyName assemblyName2 = new AssemblyName(y.FullName);
			if ((object)x != y)
			{
				if (AssemblyName.ReferenceMatchesDefinition(assemblyName, assemblyName2))
				{
					return AssemblyName.ReferenceMatchesDefinition(assemblyName2, assemblyName);
				}
				return false;
			}
			return true;
		}

		public int GetHashCode(Assembly assembly)
		{
			return assembly.FullName.GetHashCode();
		}
	}

	internal override bool TryResolveAssemblyReference(AssemblyName referenceName, out Assembly assembly)
	{
		assembly = ResolveAssembly(referenceName);
		return assembly != null;
	}

	internal override IEnumerable<Assembly> GetWildcardAssemblies()
	{
		return GetAllDiscoverableAssemblies();
	}

	internal virtual Assembly ResolveAssembly(AssemblyName referenceName)
	{
		Assembly assembly = null;
		foreach (Assembly alreadyLoadedNonSystemAssembly in GetAlreadyLoadedNonSystemAssemblies())
		{
			if (AssemblyName.ReferenceMatchesDefinition(referenceName, new AssemblyName(alreadyLoadedNonSystemAssembly.FullName)))
			{
				return alreadyLoadedNonSystemAssembly;
			}
		}
		if (assembly == null)
		{
			assembly = MetadataAssemblyHelper.SafeLoadReferencedAssembly(referenceName);
			if (assembly != null)
			{
				return assembly;
			}
		}
		TryFindWildcardAssemblyMatch(referenceName, out assembly);
		return assembly;
	}

	private static bool TryFindWildcardAssemblyMatch(AssemblyName referenceName, out Assembly assembly)
	{
		foreach (Assembly allDiscoverableAssembly in GetAllDiscoverableAssemblies())
		{
			if (AssemblyName.ReferenceMatchesDefinition(referenceName, new AssemblyName(allDiscoverableAssembly.FullName)))
			{
				assembly = allDiscoverableAssembly;
				return true;
			}
		}
		assembly = null;
		return false;
	}

	private static IEnumerable<Assembly> GetAlreadyLoadedNonSystemAssemblies()
	{
		return from a in AppDomain.CurrentDomain.GetAssemblies()
			where a != null && !MetadataAssemblyHelper.ShouldFilterAssembly(a)
			select a;
	}

	private static IEnumerable<Assembly> GetAllDiscoverableAssemblies()
	{
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		HashSet<Assembly> hashSet = new HashSet<Assembly>(AssemblyComparer.Instance);
		foreach (Assembly alreadyLoadedNonSystemAssembly in GetAlreadyLoadedNonSystemAssemblies())
		{
			hashSet.Add(alreadyLoadedNonSystemAssembly);
		}
		AspProxy aspProxy = new AspProxy();
		if (!aspProxy.IsAspNetEnvironment())
		{
			if (entryAssembly == null)
			{
				return hashSet;
			}
			hashSet.Add(entryAssembly);
			{
				foreach (Assembly nonSystemReferencedAssembly in MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(entryAssembly))
				{
					hashSet.Add(nonSystemReferencedAssembly);
				}
				return hashSet;
			}
		}
		if (aspProxy.HasBuildManagerType())
		{
			IEnumerable<Assembly> buildManagerReferencedAssemblies = aspProxy.GetBuildManagerReferencedAssemblies();
			if (buildManagerReferencedAssemblies != null)
			{
				foreach (Assembly item in buildManagerReferencedAssemblies)
				{
					if (!MetadataAssemblyHelper.ShouldFilterAssembly(item))
					{
						hashSet.Add(item);
					}
				}
			}
		}
		return hashSet.Where((Assembly a) => a != null);
	}
}
