using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> where TEntityType : class where TTargetEntityType : class
{
	private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

	internal ManyNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
	{
		navigationPropertyConfiguration.Reset();
		_navigationPropertyConfiguration = navigationPropertyConfiguration;
		_navigationPropertyConfiguration.RelationshipMultiplicity = RelationshipMultiplicity.Many;
	}

	public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> WithMany(Expression<Func<TTargetEntityType, ICollection<TEntityType>>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithMany();
	}

	public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> WithMany()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.Many;
		return new ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType>(_navigationPropertyConfiguration);
	}

	public DependentNavigationPropertyConfiguration<TTargetEntityType> WithRequired(Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithRequired();
	}

	public DependentNavigationPropertyConfiguration<TTargetEntityType> WithRequired()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.One;
		return new DependentNavigationPropertyConfiguration<TTargetEntityType>(_navigationPropertyConfiguration);
	}

	public DependentNavigationPropertyConfiguration<TTargetEntityType> WithOptional(Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithOptional();
	}

	public DependentNavigationPropertyConfiguration<TTargetEntityType> WithOptional()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.ZeroOrOne;
		return new DependentNavigationPropertyConfiguration<TTargetEntityType>(_navigationPropertyConfiguration);
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
