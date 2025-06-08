using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

[Obsolete("The mechanism to provide pre-generated views has changed. Implement a class that derives from System.Data.Entity.Infrastructure.MappingViews.DbMappingViewCache and has a parameterless constructor, then associate it with a type that derives from DbContext or ObjectContext by using System.Data.Entity.Infrastructure.MappingViews.DbMappingViewCacheTypeAttribute.", true)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EntityViewGenerationAttribute : Attribute
{
	private readonly Type m_viewGenType;

	public Type ViewGenerationType => m_viewGenType;

	public EntityViewGenerationAttribute(Type viewGenerationType)
	{
		Check.NotNull(viewGenerationType, "viewGenerationType");
		m_viewGenType = viewGenerationType;
	}
}
