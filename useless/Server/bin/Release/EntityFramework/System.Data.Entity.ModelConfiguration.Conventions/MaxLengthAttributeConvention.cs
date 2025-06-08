using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class MaxLengthAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<MaxLengthAttribute>
{
	private const int MaxLengthIndicator = -1;

	public override void Apply(ConventionPrimitivePropertyConfiguration configuration, MaxLengthAttribute attribute)
	{
		Check.NotNull(configuration, "configuration");
		Check.NotNull(attribute, "attribute");
		PropertyInfo clrPropertyInfo = configuration.ClrPropertyInfo;
		if (attribute.Length == 0 || attribute.Length < -1)
		{
			throw Error.MaxLengthAttributeConvention_InvalidMaxLength(clrPropertyInfo.Name, clrPropertyInfo.ReflectedType);
		}
		if (attribute.Length == -1)
		{
			configuration.IsMaxLength();
		}
		else
		{
			configuration.HasMaxLength(attribute.Length);
		}
	}
}
