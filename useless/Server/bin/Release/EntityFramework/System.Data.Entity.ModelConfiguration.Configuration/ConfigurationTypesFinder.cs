using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration.Types;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ConfigurationTypesFinder
{
	private readonly ConfigurationTypeActivator _activator;

	private readonly ConfigurationTypeFilter _filter;

	public ConfigurationTypesFinder()
		: this(new ConfigurationTypeActivator(), new ConfigurationTypeFilter())
	{
	}

	public ConfigurationTypesFinder(ConfigurationTypeActivator activator, ConfigurationTypeFilter filter)
	{
		_activator = activator;
		_filter = filter;
	}

	public virtual void AddConfigurationTypesToModel(IEnumerable<Type> types, ModelConfiguration modelConfiguration)
	{
		foreach (Type type in types)
		{
			if (_filter.IsEntityTypeConfiguration(type))
			{
				modelConfiguration.Add(_activator.Activate<EntityTypeConfiguration>(type));
			}
			else if (_filter.IsComplexTypeConfiguration(type))
			{
				modelConfiguration.Add(_activator.Activate<ComplexTypeConfiguration>(type));
			}
		}
	}
}
