using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class TimestampAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<TimestampAttribute>
{
	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, TimestampAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.IsRowVersion();
	}
}
