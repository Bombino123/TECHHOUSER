using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Mappers;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class InversePropertyAttributeConvention : PropertyAttributeConfigurationConvention<InversePropertyAttribute>
{
	public override void Apply(PropertyInfo memberInfo, ConventionTypeConfiguration configuration, InversePropertyAttribute attribute)
	{
		Check.NotNull(memberInfo, "memberInfo");
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		if (memberInfo.IsValidEdmNavigationProperty())
		{
			Type targetType = memberInfo.PropertyType.GetTargetType();
			PropertyInfo inverseNavigationProperty = new PropertyFilter().GetProperties(targetType, declaredOnly: false).SingleOrDefault((PropertyInfo p) => string.Equals(p.Name, attribute.Property, StringComparison.OrdinalIgnoreCase));
			if (inverseNavigationProperty == null)
			{
				throw Error.InversePropertyAttributeConvention_PropertyNotFound(attribute.Property, targetType, memberInfo.Name, memberInfo.ReflectedType);
			}
			if (memberInfo == inverseNavigationProperty)
			{
				throw Error.InversePropertyAttributeConvention_SelfInverseDetected(memberInfo.Name, memberInfo.ReflectedType);
			}
			configuration.NavigationProperty(memberInfo).HasInverseNavigationProperty((PropertyInfo p) => inverseNavigationProperty);
		}
	}
}
