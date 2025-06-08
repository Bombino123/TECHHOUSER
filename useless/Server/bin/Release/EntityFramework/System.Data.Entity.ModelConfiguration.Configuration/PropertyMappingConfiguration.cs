using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class PropertyMappingConfiguration
{
	private readonly System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration _configuration;

	internal System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration Configuration => _configuration;

	internal PropertyMappingConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration configuration)
	{
		_configuration = configuration;
	}

	public PropertyMappingConfiguration HasColumnName(string columnName)
	{
		Configuration.ColumnName = columnName;
		return this;
	}

	public PropertyMappingConfiguration HasColumnAnnotation(string name, object value)
	{
		Check.NotEmpty(name, "name");
		Configuration.SetAnnotation(name, value);
		return this;
	}
}
