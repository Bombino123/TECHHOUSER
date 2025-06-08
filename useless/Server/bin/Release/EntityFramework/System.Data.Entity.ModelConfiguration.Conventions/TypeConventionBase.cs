using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

internal abstract class TypeConventionBase : IConfigurationConvention<Type, EntityTypeConfiguration>, IConvention, IConfigurationConvention<Type, ComplexTypeConfiguration>, IConfigurationConvention<Type>
{
	private readonly IEnumerable<Func<Type, bool>> _predicates;

	internal IEnumerable<Func<Type, bool>> Predicates => _predicates;

	protected TypeConventionBase(IEnumerable<Func<Type, bool>> predicates)
	{
		_predicates = predicates;
	}

	public void Apply(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		if (_predicates.All((Func<Type, bool> p) => p(memberInfo)))
		{
			ApplyCore(memberInfo, modelConfiguration);
		}
	}

	protected abstract void ApplyCore(Type memberInfo, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration);

	public void Apply(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		if (_predicates.All((Func<Type, bool> p) => p(memberInfo)))
		{
			ApplyCore(memberInfo, configuration, modelConfiguration);
		}
	}

	protected abstract void ApplyCore(Type memberInfo, Func<EntityTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration);

	public void Apply(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		if (_predicates.All((Func<Type, bool> p) => p(memberInfo)))
		{
			ApplyCore(memberInfo, configuration, modelConfiguration);
		}
	}

	protected abstract void ApplyCore(Type memberInfo, Func<ComplexTypeConfiguration> configuration, System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration);
}
