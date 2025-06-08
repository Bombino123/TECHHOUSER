using System.Data.Entity.Core.Mapping;

namespace System.Data.Entity.Infrastructure.MappingViews;

public abstract class DbMappingViewCacheFactory
{
	public abstract DbMappingViewCache Create(string conceptualModelContainerName, string storeModelContainerName);

	internal DbMappingViewCache Create(EntityContainerMapping mapping)
	{
		return Create(mapping.EdmEntityContainer.Name, mapping.StorageEntityContainer.Name);
	}
}
