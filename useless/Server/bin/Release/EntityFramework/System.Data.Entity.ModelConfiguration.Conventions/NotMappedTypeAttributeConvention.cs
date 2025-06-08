using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class NotMappedTypeAttributeConvention : TypeAttributeConfigurationConvention<NotMappedAttribute>
{
	public override void Apply(ConventionTypeConfiguration configuration, NotMappedAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.Ignore();
	}
}
