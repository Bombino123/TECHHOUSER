using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ConfigurationTypeActivator
{
	public virtual TStructuralTypeConfiguration Activate<TStructuralTypeConfiguration>(Type type) where TStructuralTypeConfiguration : StructuralTypeConfiguration
	{
		if (type.GetDeclaredConstructor() == null)
		{
			throw new InvalidOperationException(Strings.CreateConfigurationType_NoParameterlessConstructor(type.Name));
		}
		return (TStructuralTypeConfiguration)typeof(StructuralTypeConfiguration<>).MakeGenericType(type.TryGetElementType(typeof(StructuralTypeConfiguration<>))).GetDeclaredProperty("Configuration").GetValue(Activator.CreateInstance(type, nonPublic: true), null);
	}
}
