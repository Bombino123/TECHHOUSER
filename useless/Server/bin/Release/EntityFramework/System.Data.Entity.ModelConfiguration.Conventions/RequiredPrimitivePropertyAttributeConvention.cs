using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class RequiredPrimitivePropertyAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<RequiredAttribute>
{
	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, RequiredAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.IsRequired();
	}
}
