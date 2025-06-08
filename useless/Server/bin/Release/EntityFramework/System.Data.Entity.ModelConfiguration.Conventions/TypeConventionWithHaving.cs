using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal class TypeConventionWithHaving<T> : TypeConventionWithHavingBase<T> where T : class
{
	private readonly Action<ConventionTypeConfiguration, T> _entityConfigurationAction;

	internal Action<ConventionTypeConfiguration, T> EntityConfigurationAction => _entityConfigurationAction;

	public TypeConventionWithHaving(IEnumerable<Func<Type, bool>> predicates, Func<Type, T> capturingPredicate, Action<ConventionTypeConfiguration, T> entityConfigurationAction)
		: base(predicates, capturingPredicate)
	{
		_entityConfigurationAction = entityConfigurationAction;
	}

	protected override void InvokeAction(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, T value)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, modelConfiguration), value);
	}

	protected override void InvokeAction(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, T value)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration), value);
	}

	protected override void InvokeAction(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, T value)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration), value);
	}
}
internal class TypeConventionWithHaving<T, TValue> : TypeConventionWithHavingBase<TValue> where T : class where TValue : class
{
	private readonly Action<ConventionTypeConfiguration<T>, TValue> _entityConfigurationAction;

	internal Action<ConventionTypeConfiguration<T>, TValue> EntityConfigurationAction => _entityConfigurationAction;

	public TypeConventionWithHaving(IEnumerable<Func<Type, bool>> predicates, Func<Type, TValue> capturingPredicate, Action<ConventionTypeConfiguration<T>, TValue> entityConfigurationAction)
		: base(predicates.Prepend(TypeConvention<T>.OfTypePredicate), capturingPredicate)
	{
		_entityConfigurationAction = entityConfigurationAction;
	}

	protected override void InvokeAction(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, TValue value)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, modelConfiguration), value);
	}

	protected override void InvokeAction(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, TValue value)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration), value);
	}

	protected override void InvokeAction(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, TValue value)
	{
		_entityConfigurationAction(new ConventionTypeConfiguration<T>(memberInfo, configuration, modelConfiguration), value);
	}
}
