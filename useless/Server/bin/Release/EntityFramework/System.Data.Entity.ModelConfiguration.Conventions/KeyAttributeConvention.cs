using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class KeyAttributeConvention : Convention
{
	private readonly AttributeProvider _attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

	internal override void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(PropertyInfo propertyInfo, Func<TStructuralTypeConfiguration> structuralTypeConfiguration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		if (typeof(TStructuralTypeConfiguration) == typeof(EntityTypeConfiguration) && _attributeProvider.GetAttributes(propertyInfo).OfType<KeyAttribute>().Any())
		{
			EntityTypeConfiguration entityTypeConfiguration = (EntityTypeConfiguration)(object)structuralTypeConfiguration();
			if (propertyInfo.IsValidEdmScalarProperty())
			{
				entityTypeConfiguration.Key(propertyInfo);
			}
		}
	}
}
