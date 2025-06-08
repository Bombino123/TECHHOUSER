using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class PropertyConventionWithHavingConfiguration<T> where T : class
{
	private readonly ConventionsConfiguration _conventionsConfiguration;

	private readonly IEnumerable<Func<PropertyInfo, bool>> _predicates;

	private readonly Func<PropertyInfo, T> _capturingPredicate;

	internal ConventionsConfiguration ConventionsConfiguration => _conventionsConfiguration;

	internal IEnumerable<Func<PropertyInfo, bool>> Predicates => _predicates;

	internal Func<PropertyInfo, T> CapturingPredicate => _capturingPredicate;

	internal PropertyConventionWithHavingConfiguration(ConventionsConfiguration conventionsConfiguration, IEnumerable<Func<PropertyInfo, bool>> predicates, Func<PropertyInfo, T> capturingPredicate)
	{
		_conventionsConfiguration = conventionsConfiguration;
		_predicates = predicates;
		_capturingPredicate = capturingPredicate;
	}

	public void Configure(Action<ConventionPrimitivePropertyConfiguration, T> propertyConfigurationAction)
	{
		Check.NotNull(propertyConfigurationAction, "propertyConfigurationAction");
		_conventionsConfiguration.Add(new PropertyConventionWithHaving<T>(_predicates, _capturingPredicate, propertyConfigurationAction));
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
