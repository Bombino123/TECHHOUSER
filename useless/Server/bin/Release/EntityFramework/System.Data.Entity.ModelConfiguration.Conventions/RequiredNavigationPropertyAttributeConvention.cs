using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class RequiredNavigationPropertyAttributeConvention : Convention
{
	private readonly AttributeProvider _attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

	internal override void ApplyPropertyConfiguration(PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		if (propertyInfo.IsValidEdmNavigationProperty() && !propertyInfo.PropertyType.IsCollection() && _attributeProvider.GetAttributes(propertyInfo).OfType<RequiredAttribute>().Any())
		{
			NavigationPropertyConfiguration navigationPropertyConfiguration = (NavigationPropertyConfiguration)propertyConfiguration();
			if (!navigationPropertyConfiguration.RelationshipMultiplicity.HasValue)
			{
				navigationPropertyConfiguration.RelationshipMultiplicity = RelationshipMultiplicity.One;
			}
		}
	}
}
