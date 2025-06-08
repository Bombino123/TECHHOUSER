using System.Data.Entity.Core.Objects;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.MappingViews;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DbMappingViewCacheTypeAttribute : Attribute
{
	private readonly Type _contextType;

	private readonly Type _cacheType;

	internal Type ContextType => _contextType;

	internal Type CacheType => _cacheType;

	public DbMappingViewCacheTypeAttribute(Type contextType, Type cacheType)
	{
		Check.NotNull(contextType, "contextType");
		Check.NotNull(cacheType, "cacheType");
		if (!contextType.IsSubclassOf(typeof(ObjectContext)) && !contextType.IsSubclassOf(typeof(DbContext)))
		{
			throw new ArgumentException(Strings.DbMappingViewCacheTypeAttribute_InvalidContextType(contextType), "contextType");
		}
		if (!cacheType.IsSubclassOf(typeof(DbMappingViewCache)))
		{
			throw new ArgumentException(Strings.Generated_View_Type_Super_Class(cacheType), "cacheType");
		}
		_contextType = contextType;
		_cacheType = cacheType;
	}

	public DbMappingViewCacheTypeAttribute(Type contextType, string cacheTypeName)
	{
		Check.NotNull(contextType, "contextType");
		Check.NotEmpty(cacheTypeName, "cacheTypeName");
		if (!contextType.IsSubclassOf(typeof(ObjectContext)) && !contextType.IsSubclassOf(typeof(DbContext)))
		{
			throw new ArgumentException(Strings.DbMappingViewCacheTypeAttribute_InvalidContextType(contextType), "contextType");
		}
		_contextType = contextType;
		try
		{
			_cacheType = Type.GetType(cacheTypeName, throwOnError: true);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException(Strings.DbMappingViewCacheTypeAttribute_CacheTypeNotFound(cacheTypeName), "cacheTypeName", innerException);
		}
	}
}
