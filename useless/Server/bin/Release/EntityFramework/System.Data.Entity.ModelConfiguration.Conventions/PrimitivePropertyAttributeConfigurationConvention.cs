using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public abstract class PrimitivePropertyAttributeConfigurationConvention<TAttribute> : Convention where TAttribute : Attribute
{
	private readonly AttributeProvider _attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

	protected PrimitivePropertyAttributeConfigurationConvention()
	{
		Properties().Having((PropertyInfo pi) => _attributeProvider.GetAttributes(pi).OfType<TAttribute>()).Configure(delegate(ConventionPrimitivePropertyConfiguration configuration, IEnumerable<TAttribute> attributes)
		{
			foreach (TAttribute attribute in attributes)
			{
				Apply(configuration, attribute);
			}
		});
	}

	public abstract void Apply(ConventionPrimitivePropertyConfiguration configuration, TAttribute attribute);
}
