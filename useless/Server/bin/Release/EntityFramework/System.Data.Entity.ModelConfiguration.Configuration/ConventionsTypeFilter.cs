using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ConventionsTypeFilter
{
	public virtual bool IsConvention(Type conventionType)
	{
		if (!IsConfigurationConvention(conventionType) && !IsConceptualModelConvention(conventionType) && !IsConceptualToStoreMappingConvention(conventionType))
		{
			return IsStoreModelConvention(conventionType);
		}
		return true;
	}

	public static bool IsConfigurationConvention(Type conventionType)
	{
		if (!typeof(IConfigurationConvention).IsAssignableFrom(conventionType) && !typeof(Convention).IsAssignableFrom(conventionType) && !conventionType.GetGenericTypeImplementations(typeof(IConfigurationConvention<>)).Any())
		{
			return conventionType.GetGenericTypeImplementations(typeof(IConfigurationConvention<, >)).Any();
		}
		return true;
	}

	public static bool IsConceptualModelConvention(Type conventionType)
	{
		return conventionType.GetGenericTypeImplementations(typeof(IConceptualModelConvention<>)).Any();
	}

	public static bool IsStoreModelConvention(Type conventionType)
	{
		return conventionType.GetGenericTypeImplementations(typeof(IStoreModelConvention<>)).Any();
	}

	public static bool IsConceptualToStoreMappingConvention(Type conventionType)
	{
		return typeof(IDbMappingConvention).IsAssignableFrom(conventionType);
	}
}
