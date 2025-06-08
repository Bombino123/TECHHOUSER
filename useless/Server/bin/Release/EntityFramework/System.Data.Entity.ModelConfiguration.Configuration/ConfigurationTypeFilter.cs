using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

internal class ConfigurationTypeFilter
{
	public virtual bool IsEntityTypeConfiguration(Type type)
	{
		return IsStructuralTypeConfiguration(type, typeof(EntityTypeConfiguration<>));
	}

	public virtual bool IsComplexTypeConfiguration(Type type)
	{
		return IsStructuralTypeConfiguration(type, typeof(ComplexTypeConfiguration<>));
	}

	private static bool IsStructuralTypeConfiguration(Type type, Type structuralTypeConfiguration)
	{
		if (!type.IsAbstract())
		{
			return type.TryGetElementType(structuralTypeConfiguration) != null;
		}
		return false;
	}
}
