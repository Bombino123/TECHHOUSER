using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class TypeConventionWithHavingConfiguration<T> where T : class
{
	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly IEnumerable<Func<Type, bool>> _predicates;

	private readonly Func<Type, T> _capturingPredicate;

	internal ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	internal IEnumerable<Func<Type, bool>> Predicates => _predicates;

	internal Func<Type, T> CapturingPredicate => _capturingPredicate;

	internal TypeConventionWithHavingConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<Type, bool>> predicates, Func<Type, T> capturingPredicate)
	{
		_conventionsConfiguration = conventionsConfiguration;
		_predicates = predicates;
		_capturingPredicate = capturingPredicate;
	}

	public void Configure(Action<ConventionTypeConfiguration, T> entityConfigurationAction)
	{
		Check.NotNull(entityConfigurationAction, "entityConfigurationAction");
		_conventionsConfiguration.Add(new TypeConventionWithHaving<T>(_predicates, _capturingPredicate, entityConfigurationAction));
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
public class TypeConventionWithHavingConfiguration<T, TValue> where T : class where TValue : class
{
	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly IEnumerable<Func<Type, bool>> _predicates;

	private readonly Func<Type, TValue> _capturingPredicate;

	internal ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	internal IEnumerable<Func<Type, bool>> Predicates => _predicates;

	internal Func<Type, TValue> CapturingPredicate => _capturingPredicate;

	internal TypeConventionWithHavingConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<Type, bool>> predicates, Func<Type, TValue> capturingPredicate)
	{
		_conventionsConfiguration = conventionsConfiguration;
		_predicates = predicates;
		_capturingPredicate = capturingPredicate;
	}

	public void Configure(Action<ConventionTypeConfiguration<T>, TValue> entityConfigurationAction)
	{
		Check.NotNull(entityConfigurationAction, "entityConfigurationAction");
		_conventionsConfiguration.Add(new TypeConventionWithHaving<T, TValue>(_predicates, _capturingPredicate, entityConfigurationAction));
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
