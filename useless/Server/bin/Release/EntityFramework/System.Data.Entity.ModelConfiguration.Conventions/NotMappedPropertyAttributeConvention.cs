using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class NotMappedPropertyAttributeConvention : PropertyAttributeConfigurationConvention<NotMappedAttribute>
{
	public override void Apply(PropertyInfo memberInfo, ConventionTypeConfiguration configuration, NotMappedAttribute attribute)
	{
		Check.NotNull(memberInfo, "memberInfo");
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.Ignore(memberInfo);
	}
}
