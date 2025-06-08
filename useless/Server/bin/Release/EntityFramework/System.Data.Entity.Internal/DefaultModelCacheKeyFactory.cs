using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Internal;

internal sealed class DefaultModelCacheKeyFactory
{
	public IDbModelCacheKey Create(DbContext context)
	{
		Check.NotNull(context, "context");
		string customKey = null;
		if (context is IDbModelCacheKeyProvider dbModelCacheKeyProvider)
		{
			customKey = dbModelCacheKeyProvider.CacheKey;
		}
		return new DefaultModelCacheKey(context.GetType(), context.InternalContext.ProviderName, context.InternalContext.ProviderFactory.GetType(), customKey);
	}
}
