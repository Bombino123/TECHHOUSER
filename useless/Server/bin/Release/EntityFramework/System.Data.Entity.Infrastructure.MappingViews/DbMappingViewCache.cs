using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Infrastructure.MappingViews;

public abstract class DbMappingViewCache
{
	public abstract string MappingHashValue { get; }

	public abstract DbMappingView GetView(EntitySetBase extent);
}
