using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ComplexTypeAttributeConvention : TypeAttributeConfigurationConvention<ComplexTypeAttribute>
{
	public override void Apply(ConventionTypeConfiguration configuration, ComplexTypeAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.IsComplexType();
	}
}
