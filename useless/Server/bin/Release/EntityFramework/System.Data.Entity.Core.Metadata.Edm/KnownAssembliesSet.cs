using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class KnownAssembliesSet
{
	private readonly Dictionary<Assembly, KnownAssemblyEntry> _assemblies;

	internal IEnumerable<Assembly> Assemblies => _assemblies.Keys;

	internal KnownAssembliesSet()
	{
		_assemblies = new Dictionary<Assembly, KnownAssemblyEntry>();
	}

	internal KnownAssembliesSet(KnownAssembliesSet set)
	{
		_assemblies = new Dictionary<Assembly, KnownAssemblyEntry>(set._assemblies);
	}

	internal virtual bool TryGetKnownAssembly(Assembly assembly, object loaderCookie, EdmItemCollection itemCollection, out KnownAssemblyEntry entry)
	{
		if (!_assemblies.TryGetValue(assembly, out entry))
		{
			return false;
		}
		if (!entry.HaveSeenInCompatibleContext(loaderCookie, itemCollection))
		{
			return false;
		}
		return true;
	}

	public IEnumerable<KnownAssemblyEntry> GetEntries(object loaderCookie, EdmItemCollection itemCollection)
	{
		return _assemblies.Values.Where((KnownAssemblyEntry e) => e.HaveSeenInCompatibleContext(loaderCookie, itemCollection));
	}

	internal bool Contains(Assembly assembly, object loaderCookie, EdmItemCollection itemCollection)
	{
		KnownAssemblyEntry entry;
		return TryGetKnownAssembly(assembly, loaderCookie, itemCollection, out entry);
	}

	internal void Add(Assembly assembly, KnownAssemblyEntry knownAssemblyEntry)
	{
		if (_assemblies.TryGetValue(assembly, out var _))
		{
			_assemblies[assembly] = knownAssemblyEntry;
		}
		else
		{
			_assemblies.Add(assembly, knownAssemblyEntry);
		}
	}
}
