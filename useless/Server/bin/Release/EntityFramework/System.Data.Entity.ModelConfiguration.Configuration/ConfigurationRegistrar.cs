using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConfigurationRegistrar
{
	private readonly ModelConfiguration _modelConfiguration;

	internal ConfigurationRegistrar(ModelConfiguration modelConfiguration)
	{
		_modelConfiguration = modelConfiguration;
	}

	public virtual ConfigurationRegistrar AddFromAssembly(Assembly assembly)
	{
		Check.NotNull(assembly, "assembly");
		new ConfigurationTypesFinder().AddConfigurationTypesToModel(assembly.GetAccessibleTypes(), _modelConfiguration);
		return this;
	}

	public virtual ConfigurationRegistrar Add<TEntityType>(EntityTypeConfiguration<TEntityType> entityTypeConfiguration) where TEntityType : class
	{
		Check.NotNull(entityTypeConfiguration, "entityTypeConfiguration");
		_modelConfiguration.Add((EntityTypeConfiguration)entityTypeConfiguration.Configuration);
		return this;
	}

	public virtual ConfigurationRegistrar Add<TComplexType>(ComplexTypeConfiguration<TComplexType> complexTypeConfiguration) where TComplexType : class
	{
		Check.NotNull(complexTypeConfiguration, "complexTypeConfiguration");
		_modelConfiguration.Add((ComplexTypeConfiguration)complexTypeConfiguration.Configuration);
		return this;
	}

	internal virtual IEnumerable<Type> GetConfiguredTypes()
	{
		return _modelConfiguration.ConfiguredTypes.ToList();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		return base.ToString();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Type GetType()
	{
		return base.GetType();
	}
}
