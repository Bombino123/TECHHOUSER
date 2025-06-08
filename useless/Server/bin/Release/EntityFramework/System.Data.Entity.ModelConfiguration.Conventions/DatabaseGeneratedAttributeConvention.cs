using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class DatabaseGeneratedAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<DatabaseGeneratedAttribute>
{
	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, DatabaseGeneratedAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		configuration.HasDatabaseGeneratedOption(attribute.DatabaseGeneratedOption);
	}
}
