using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class MetadataArtifactAssemblyResolver
{
	internal abstract bool TryResolveAssemblyReference(AssemblyName referenceName, out Assembly assembly);

	internal abstract IEnumerable<Assembly> GetWildcardAssemblies();
}
