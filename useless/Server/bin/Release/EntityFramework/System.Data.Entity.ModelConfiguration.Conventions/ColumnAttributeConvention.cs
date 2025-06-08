using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ColumnAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<ColumnAttribute>
{
	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, ColumnAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		if (!string.IsNullOrWhiteSpace(attribute.Name))
		{
			configuration.HasColumnName(attribute.Name);
		}
		if (!string.IsNullOrWhiteSpace(attribute.TypeName))
		{
			configuration.HasColumnType(attribute.TypeName);
		}
		if (attribute.Order >= 0)
		{
			configuration.HasColumnOrder(attribute.Order);
		}
	}
}
