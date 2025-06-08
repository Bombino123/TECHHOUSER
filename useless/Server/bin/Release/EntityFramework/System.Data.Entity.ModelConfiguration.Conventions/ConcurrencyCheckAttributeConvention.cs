using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ConcurrencyCheckAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<ConcurrencyCheckAttribute>
{
	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, ConcurrencyCheckAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.IsConcurrencyToken();
	}
}
