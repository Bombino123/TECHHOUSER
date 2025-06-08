using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm;

internal abstract class AssemblyCacheEntry
{
	internal abstract IList<EdmType> TypesInAssembly { get; }

	internal abstract IList<Assembly> ClosureAssemblies { get; }

	internal bool TryGetEdmType(string typeName, out EdmType edmType)
	{
		edmType = null;
		foreach (EdmType item in TypesInAssembly)
		{
			if (item.Identity == typeName)
			{
				edmType = item;
				break;
			}
		}
		return edmType != null;
	}

	internal bool ContainsType(string typeName)
	{
		EdmType edmType = null;
		return TryGetEdmType(typeName, out edmType);
	}
}
