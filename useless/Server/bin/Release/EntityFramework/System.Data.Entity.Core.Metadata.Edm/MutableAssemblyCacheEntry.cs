using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class MutableAssemblyCacheEntry : AssemblyCacheEntry
{
	private readonly List<EdmType> _typesInAssembly = new List<EdmType>();

	private readonly List<Assembly> _closureAssemblies = new List<Assembly>();

	internal override IList<EdmType> TypesInAssembly => _typesInAssembly;

	internal override IList<Assembly> ClosureAssemblies => _closureAssemblies;
}
