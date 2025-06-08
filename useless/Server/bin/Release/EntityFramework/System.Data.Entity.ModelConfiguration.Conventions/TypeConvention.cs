using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal class TypeConvention : TypeConventionBase
{
	private readonly Action<ConventionTypeConfiguration> _entityConfigurationAction;

	internal Action<ConventionTypeConfiguration> EntityConfigurationAction => _entityConfigurationAction;

	public TypeConvention(IEnumerable<Func<Type, bool>> predicates, Action<ConventionTypeConfiguration> entityConfigurationAction)
		: base(predicates)
	{
		_entityConfigurationAction = entityConfigurationAction;
	}

	protected override void ApplyCore(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, modelConfiguration));
	}

	protected override void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration));
	}

	protected override void ApplyCore(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration));
	}
}
internal class TypeConvention<T> : TypeConventionBase where T : class
{
	private static readonly Func<Type, bool> _ofTypePredicate = (Type t) => typeof(T).IsAssignableFrom(t);

	private readonly Action<ConventionTypeConfiguration<T>> _entityConfigurationAction;

	internal Action<ConventionTypeConfiguration<T>> EntityConfigurationAction => _entityConfigurationAction;

	internal static Func<Type, bool> OfTypePredicate => _ofTypePredicate;

	public TypeConvention(IEnumerable<Func<Type, bool>> predicates, Action<ConventionTypeConfiguration<T>> entityConfigurationAction)
		: base(predicates.Prepend(_ofTypePredicate))
	{
		_entityConfigurationAction = entityConfigurationAction;
	}

	protected override void ApplyCore(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, modelConfiguration));
	}

	protected override void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration));
	}

	protected override void ApplyCore(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration));
	}
}
