using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class StringLengthAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<StringLengthAttribute>
{
	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, StringLengthAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		if (attribute.MaximumLength < 1)
		{
			PropertyInfo clrPropertyInfo = configuration.ClrPropertyInfo;
			throw Error.StringLengthAttributeConvention_InvalidMaximumLength(clrPropertyInfo.Name, clrPropertyInfo.ReflectedType);
		}
		configuration.HasMaxLength(attribute.MaximumLength);
	}
}
