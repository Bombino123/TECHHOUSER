using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal class PropertyConvention : PropertyConventionBase
{
	private readonly Action<ConventionPrimitivePropertyConfiguration> _propertyConfigurationAction;

	internal Action<ConventionPrimitivePropertyConfiguration> PropertyConfigurationAction => _propertyConfigurationAction;

	public PropertyConvention(IEnumerable<Func<PropertyInfo, bool>> predicates, Action<ConventionPrimitivePropertyConfiguration> propertyConfigurationAction)
		: base(predicates)
	{
		_propertyConfigurationAction = propertyConfigurationAction;
	}

	protected override void ApplyCore(PropertyInfo memberInfo, Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_propertyConfigurationAction(new ConventionPrimitivePropertyConfiguration(memberInfo, configuration));
	}
}
