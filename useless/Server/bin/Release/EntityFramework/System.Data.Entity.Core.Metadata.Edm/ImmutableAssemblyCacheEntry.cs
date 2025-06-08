using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class ImmutableAssemblyCacheEntry : AssemblyCacheEntry
{
	private readonly ReadOnlyCollection<EdmType> _typesInAssembly;

	private readonly ReadOnlyCollection<Assembly> _closureAssemblies;

	internal override IList<EdmType> TypesInAssembly => _typesInAssembly;

	internal override IList<Assembly> ClosureAssemblies => _closureAssemblies;

	internal ImmutableAssemblyCacheEntry(MutableAssemblyCacheEntry mutableEntry)
	{
		_typesInAssembly = new ReadOnlyCollection<EdmType>(new List<EdmType>(mutableEntry.TypesInAssembly));
		_closureAssemblies = new ReadOnlyCollection<Assembly>(new List<Assembly>(mutableEntry.ClosureAssemblies));
	}
}
