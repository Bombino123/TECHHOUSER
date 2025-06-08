using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public abstract class TypeAttributeConfigurationConvention<TAttribute> : Convention where TAttribute : Attribute
{
	private readonly AttributeProvider _attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

	protected TypeAttributeConfigurationConvention()
	{
		Types().Having((Type t) => _attributeProvider.GetAttributes(t).OfType<TAttribute>()).Configure(delegate(ConventionTypeConfiguration configuration, IEnumerable<TAttribute> attributes)
		{
			foreach (TAttribute attribute in attributes)
			{
				Apply(configuration, attribute);
			}
		});
	}

	public abstract void Apply(ConventionTypeConfiguration configuration, TAttribute attribute);
}
