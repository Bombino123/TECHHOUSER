using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public abstract class PropertyAttributeConfigurationConvention<TAttribute> : Convention where TAttribute : Attribute
{
	private readonly AttributeProvider _attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

	protected PropertyAttributeConfigurationConvention()
	{
		Types().Configure(delegate(ConventionTypeConfiguration ec)
		{
			foreach (PropertyInfo instanceProperty in ec.ClrType.GetInstanceProperties())
			{
				IList<Attribute> list = (IList<Attribute>)_attributeProvider.GetAttributes(instanceProperty);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] is TAttribute attribute)
					{
						Apply(instanceProperty, ec, attribute);
					}
				}
			}
		});
	}

	public abstract void Apply(PropertyInfo memberInfo, ConventionTypeConfiguration configuration, TAttribute attribute);
}
