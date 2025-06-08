using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class PropertyConventionConfiguration
{
	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly IEnumerable<Func<PropertyInfo, bool>> _predicates;

	internal ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	internal IEnumerable<Func<PropertyInfo, bool>> Predicates => _predicates;

	internal PropertyConventionConfiguration(ConventionsConfiguration conventionsConfiguration)
		: this(conventionsConfiguration, Enumerable.Empty<Func<PropertyInfo, bool>>())
	{
	}

	private PropertyConventionConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<PropertyInfo, bool>> predicates)
	{
		_conventionsConfiguration = conventionsConfiguration;
		_predicates = predicates;
	}

	public PropertyConventionConfiguration Where(Func<PropertyInfo, bool> predicate)
	{
		Check.NotNull(predicate, "predicate");
		return new PropertyConventionConfiguration(_conventionsConfiguration, IEnumerableExtensions.Append(_predicates, predicate));
	}

	public PropertyConventionWithHavingConfiguration<T> Having<T>(Func<PropertyInfo, T> capturingPredicate) where T : class
	{
		Check.NotNull(capturingPredicate, "capturingPredicate");
		return new PropertyConventionWithHavingConfiguration<T>(_conventionsConfiguration, _predicates, capturingPredicate);
	}

	public void Configure(Action<ConventionPrimitivePropertyConfiguration> propertyConfigurationAction)
	{
		Check.NotNull(propertyConfigurationAction, "propertyConfigurationAction");
		_conventionsConfiguration.Add(new PropertyConvention(_predicates, propertyConfigurationAction));
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
