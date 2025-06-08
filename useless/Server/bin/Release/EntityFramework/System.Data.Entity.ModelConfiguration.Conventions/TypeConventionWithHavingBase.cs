using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal abstract class TypeConventionWithHavingBase<T> : TypeConventionBase where T : class
{
	private readonly Func<Type, T> _capturingPredicate;

	internal Func<Type, T> CapturingPredicate => _capturingPredicate;

	public TypeConventionWithHavingBase(IEnumerable<Func<Type, bool>> predicates, Func<Type, T> capturingPredicate)
		: base(predicates)
	{
		_capturingPredicate = capturingPredicate;
	}

	protected override void ApplyCore(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		T val = _capturingPredicate(memberInfo);
		if (val != null)
		{
			InvokeAction(memberInfo, modelConfiguration, val);
		}
	}

	protected abstract void InvokeAction(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration configuration, T value);

	protected sealed override void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		T val = _capturingPredicate(memberInfo);
		if (val != null)
		{
			InvokeAction(memberInfo, configuration, modelConfiguration, val);
		}
	}

	protected abstract void InvokeAction(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, T value);

	protected override void ApplyCore(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		T val = _capturingPredicate(memberInfo);
		if (val != null)
		{
			InvokeAction(memberInfo, configuration, modelConfiguration, val);
		}
	}

	protected abstract void InvokeAction(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration, T value);
}
