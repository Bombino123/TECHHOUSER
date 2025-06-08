using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class CustomAssemblyResolver : MetadataArtifactAssemblyResolver
{
	private readonly Func<AssemblyName, Assembly> _referenceResolver;

	private readonly Func<IEnumerable<Assembly>> _wildcardAssemblyEnumerator;

	internal CustomAssemblyResolver(Func<IEnumerable<Assembly>> wildcardAssemblyEnumerator, Func<AssemblyName, Assembly> referenceResolver)
	{
		_wildcardAssemblyEnumerator = wildcardAssemblyEnumerator;
		_referenceResolver = referenceResolver;
	}

	internal override bool TryResolveAssemblyReference(AssemblyName referenceName, out Assembly assembly)
	{
		assembly = _referenceResolver(referenceName);
		return assembly != null;
	}

	internal override IEnumerable<Assembly> GetWildcardAssemblies()
	{
		return _wildcardAssemblyEnumerator() ?? throw new InvalidOperationException(Strings.WildcardEnumeratorReturnedNull);
	}
}
