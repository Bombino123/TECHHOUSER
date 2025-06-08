using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DbConfigurationTypeAttribute : Attribute
{
	private readonly Type _configurationType;

	public Type ConfigurationType => _configurationType;

	public DbConfigurationTypeAttribute(Type configurationType)
	{
		Check.NotNull(configurationType, "configurationType");
		_configurationType = configurationType;
	}

	public DbConfigurationTypeAttribute(string configurationTypeName)
	{
		Check.NotEmpty(configurationTypeName, "configurationTypeName");
		try
		{
			_configurationType = Type.GetType(configurationTypeName, throwOnError: true);
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException(Strings.DbConfigurationTypeInAttributeNotFound(configurationTypeName), innerException);
		}
	}
}
