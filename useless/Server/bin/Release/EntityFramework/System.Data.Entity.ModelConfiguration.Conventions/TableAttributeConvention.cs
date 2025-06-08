using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class TableAttributeConvention : TypeAttributeConfigurationConvention<TableAttribute>
{
	public override void Apply(ConventionTypeConfiguration configuration, TableAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		if (string.IsNullOrWhiteSpace(attribute.Schema))
		{
			configuration.ToTable(attribute.Name);
		}
		else
		{
			configuration.ToTable(attribute.Name, attribute.Schema);
		}
	}
}
