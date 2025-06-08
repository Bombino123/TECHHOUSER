using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Mapping;

public abstract class MappingItemCollection : ItemCollection
{
	internal MappingItemCollection(DataSpace dataSpace)
		: base(dataSpace)
	{
	}

	internal virtual bool TryGetMap(string identity, DataSpace typeSpace, out MappingBase map)
	{
		throw Error.NotSupported();
	}

	internal virtual MappingBase GetMap(GlobalItem item)
	{
		throw Error.NotSupported();
	}

	internal virtual bool TryGetMap(GlobalItem item, out MappingBase map)
	{
		throw Error.NotSupported();
	}

	internal virtual MappingBase GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
	{
		throw Error.NotSupported();
	}

	internal virtual bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out MappingBase map)
	{
		throw Error.NotSupported();
	}

	internal virtual MappingBase GetMap(string identity, DataSpace typeSpace)
	{
		throw Error.NotSupported();
	}
}
