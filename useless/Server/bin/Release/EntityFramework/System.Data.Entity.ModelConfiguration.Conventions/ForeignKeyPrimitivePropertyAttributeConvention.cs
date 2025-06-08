using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Mappers;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ForeignKeyPrimitivePropertyAttributeConvention : PropertyAttributeConfigurationConvention<ForeignKeyAttribute>
{
	public override void Apply(PropertyInfo memberInfo, ConventionTypeConfiguration configuration, ForeignKeyAttribute attribute)
	{
		Check.NotNull(memberInfo, "memberInfo");
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		if (memberInfo.IsValidEdmScalarProperty())
		{
			PropertyInfo propertyInfo = (from pi in new PropertyFilter().GetProperties(configuration.ClrType, declaredOnly: false)
				where pi.Name.Equals(attribute.Name, StringComparison.Ordinal)
				select pi).SingleOrDefault();
			if (propertyInfo == null)
			{
				throw Error.ForeignKeyAttributeConvention_InvalidNavigationProperty(memberInfo.Name, configuration.ClrType, attribute.Name);
			}
			configuration.NavigationProperty(propertyInfo).HasConstraint(delegate(ForeignKeyConstraintConfiguration fk)
			{
				fk.AddColumn(memberInfo);
			});
		}
	}
}
