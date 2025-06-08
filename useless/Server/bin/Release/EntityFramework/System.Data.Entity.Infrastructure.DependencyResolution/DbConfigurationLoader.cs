using System.Data.Entity.Internal;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

internal class DbConfigurationLoader
{
	public virtual Type TryLoadFromConfig(AppConfig config)
	{
		string configurationTypeName = config.ConfigurationTypeName;
		if (string.IsNullOrWhiteSpace(configurationTypeName))
		{
			return null;
		}
		Type type;
		try
		{
			type = Type.GetType(configurationTypeName, throwOnError: true);
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException(Strings.DbConfigurationTypeNotFound(configurationTypeName), innerException);
		}
		if (!typeof(DbConfiguration).IsAssignableFrom(type))
		{
			throw new InvalidOperationException(Strings.CreateInstance_BadDbConfigurationType(type.ToString(), typeof(DbConfiguration).ToString()));
		}
		return type;
	}

	public virtual bool AppConfigContainsDbConfigurationType(AppConfig config)
	{
		return !string.IsNullOrWhiteSpace(config.ConfigurationTypeName);
	}
}
