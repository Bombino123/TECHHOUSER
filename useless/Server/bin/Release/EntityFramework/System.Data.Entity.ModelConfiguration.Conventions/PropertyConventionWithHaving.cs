using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal class PropertyConventionWithHaving<T> : PropertyConventionBase where T : class
{
	private readonly Func<PropertyInfo, T> _capturingPredicate;

	private readonly Action<ConventionPrimitivePropertyConfiguration, T> _propertyConfigurationAction;

	internal Func<PropertyInfo, T> CapturingPredicate => _capturingPredicate;

	internal Action<ConventionPrimitivePropertyConfiguration, T> PropertyConfigurationAction => _propertyConfigurationAction;

	public PropertyConventionWithHaving(IEnumerable<Func<PropertyInfo, bool>> predicates, Func<PropertyInfo, T> capturingPredicate, Action<ConventionPrimitivePropertyConfiguration, T> propertyConfigurationAction)
		: base(predicates)
	{
		_capturingPredicate = capturingPredicate;
		_propertyConfigurationAction = propertyConfigurationAction;
	}

	protected override void ApplyCore(PropertyInfo memberInfo, Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		T val = _capturingPredicate(memberInfo);
		if (val != null)
		{
			_propertyConfigurationAction(new ConventionPrimitivePropertyConfiguration(memberInfo, configuration), val);
		}
	}
}
