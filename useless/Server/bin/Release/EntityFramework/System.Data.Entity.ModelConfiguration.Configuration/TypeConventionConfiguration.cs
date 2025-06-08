using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class TypeConventionConfiguration
{
	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly IEnumerable<Func<Type, bool>> _predicates;

	internal ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	internal IEnumerable<Func<Type, bool>> Predicates => _predicates;

	internal TypeConventionConfiguration(ConventionsConfiguration conventionsConfiguration)
		: this(conventionsConfiguration, Enumerable.Empty<Func<Type, bool>>())
	{
	}

	private TypeConventionConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<Type, bool>> predicates)
	{
		_conventionsConfiguration = conventionsConfiguration;
		_predicates = predicates;
	}

	public TypeConventionConfiguration Where(Func<Type, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return new TypeConventionConfiguration(_conventionsConfiguration, IEnumerableExtensions.Append(_predicates, predicate));
	}

	public TypeConventionWithHavingConfiguration<T> Having<T>(Func<Type, T> capturingPredicate) where T : class
	{
		Check.NotNull(capturingPredicate, "capturingPredicate");
		return new TypeConventionWithHavingConfiguration<T>(_conventionsConfiguration, _predicates, capturingPredicate);
	}

	public void Configure(Action<ConventionTypeConfiguration> entityConfigurationAction)
	{
		Check.NotNull(entityConfigurationAction, "entityConfigurationAction");
		_conventionsConfiguration.Add(new TypeConvention(_predicates, entityConfigurationAction));
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
public class TypeConventionConfiguration<T> where T : class
{
	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly IEnumerable<Func<Type, bool>> _predicates;

	internal ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	internal IEnumerable<Func<Type, bool>> Predicates => _predicates;

	internal TypeConventionConfiguration(ConventionsConfiguration conventionsConfiguration)
		: this(conventionsConfiguration, Enumerable.Empty<Func<Type, bool>>())
	{
	}

	private TypeConventionConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<Type, bool>> predicates)
	{
		_conventionsConfiguration = conventionsConfiguration;
		_predicates = predicates;
	}

	public TypeConventionConfiguration<T> Where(Func<Type, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return new TypeConventionConfiguration<T>(_conventionsConfiguration, IEnumerableExtensions.Append(_predicates, predicate));
	}

	public TypeConventionWithHavingConfiguration<T, TValue> Having<TValue>(Func<Type, TValue> capturingPredicate) where TValue : class
	{
		Check.NotNull(capturingPredicate, "capturingPredicate");
		return new TypeConventionWithHavingConfiguration<T, TValue>(_conventionsConfiguration, _predicates, capturingPredicate);
	}

	public void Configure(Action<ConventionTypeConfiguration<T>> entityConfigurationAction)
	{
		Check.NotNull(entityConfigurationAction, "entityConfigurationAction");
		_conventionsConfiguration.Add(new TypeConvention<T>(_predicates, entityConfigurationAction));
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
