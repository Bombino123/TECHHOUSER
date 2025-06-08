using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Properties;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.ModelConfiguration.Conventions.Sets;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class Convention : IConvention
{
	private readonly ConventionsConfiguration _conventionsConfiguration = new ConventionsConfiguration(new ConventionSet());

	public Convention()
	{
	}

	internal Convention(ConventionsConfiguration conventionsConfiguration)
	{
		_conventionsConfiguration = conventionsConfiguration;
	}

	public TypeConventionConfiguration Types()
	{
		return new TypeConventionConfiguration(_conventionsConfiguration);
	}

	public TypeConventionConfiguration<T> Types<T>() where T : class
	{
		return new TypeConventionConfiguration<T>(_conventionsConfiguration);
	}

	public PropertyConventionConfiguration Properties()
	{
		return new PropertyConventionConfiguration(_conventionsConfiguration);
	}

	public PropertyConventionConfiguration Properties<T>()
	{
		if (!typeof(T).IsValidEdmScalarType())
		{
			throw Error.ModelBuilder_PropertyFilterTypeMustBePrimitive(typeof(T));
		}
		return new PropertyConventionConfiguration(_conventionsConfiguration).Where(delegate(PropertyInfo p)
		{
			p.PropertyType.TryUnwrapNullableType(out var underlyingType);
			return underlyingType == typeof(T);
		});
	}

	internal virtual void ApplyModelConfiguration(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);
	}

	internal virtual void ApplyModelConfiguration(Type type, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_conventionsConfiguration.ApplyModelConfiguration(type, modelConfiguration);
	}

	internal virtual void ApplyTypeConfiguration<TStructuralTypeConfiguration>(Type type, Func<TStructuralTypeConfiguration> structuralTypeConfiguration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration) where TStructuralTypeConfiguration : StructuralTypeConfiguration
	{
		_conventionsConfiguration.ApplyTypeConfiguration(type, structuralTypeConfiguration, modelConfiguration);
	}

	internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_conventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, modelConfiguration);
	}

	internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_conventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, propertyConfiguration, modelConfiguration);
	}

	internal virtual void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(PropertyInfo propertyInfo, Func<TStructuralTypeConfiguration> structuralTypeConfiguration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration) where TStructuralTypeConfiguration : StructuralTypeConfiguration
	{
		_conventionsConfiguration.ApplyPropertyTypeConfiguration(propertyInfo, structuralTypeConfiguration, modelConfiguration);
	}
}
