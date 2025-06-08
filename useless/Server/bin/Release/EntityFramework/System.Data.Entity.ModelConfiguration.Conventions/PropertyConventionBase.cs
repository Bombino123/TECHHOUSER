using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal abstract class PropertyConventionBase : IConfigurationConvention<PropertyInfo, System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration>, IConvention
{
	private readonly IEnumerable<Func<PropertyInfo, bool>> _predicates;

	internal IEnumerable<Func<PropertyInfo, bool>> Predicates => _predicates;

	public PropertyConventionBase(IEnumerable<Func<PropertyInfo, bool>> predicates)
	{
		_predicates = predicates;
	}

	public void Apply(PropertyInfo memberInfo, Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		if (_predicates.All((Func<PropertyInfo, bool> p) => p(memberInfo)))
		{
			ApplyCore(memberInfo, configuration, modelConfiguration);
		}
	}

	protected abstract void ApplyCore(PropertyInfo memberInfo, Func<System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration);
}
