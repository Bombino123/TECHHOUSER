using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class RequiredNavigationPropertyConfiguration<TEntityType, TTargetEntityType> where TEntityType : class where TTargetEntityType : class
{
	private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

	internal RequiredNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
	{
		navigationPropertyConfiguration.Reset();
		_navigationPropertyConfiguration = navigationPropertyConfiguration;
		_navigationPropertyConfiguration.RelationshipMultiplicity = RelationshipMultiplicity.One;
	}

	public DependentNavigationPropertyConfiguration<TEntityType> WithMany(Expression<Func<TTargetEntityType, ICollection<TEntityType>>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithMany();
	}

	public DependentNavigationPropertyConfiguration<TEntityType> WithMany()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.Many;
		return new DependentNavigationPropertyConfiguration<TEntityType>(_navigationPropertyConfiguration);
	}

	public ForeignKeyNavigationPropertyConfiguration WithOptional(Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithOptional();
	}

	public ForeignKeyNavigationPropertyConfiguration WithOptional()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.ZeroOrOne;
		return new ForeignKeyNavigationPropertyConfiguration(_navigationPropertyConfiguration);
	}

	public ForeignKeyNavigationPropertyConfiguration WithRequiredDependent(Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithRequiredDependent();
	}

	public ForeignKeyNavigationPropertyConfiguration WithRequiredDependent()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.One;
		_navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal = false;
		return new ForeignKeyNavigationPropertyConfiguration(_navigationPropertyConfiguration);
	}

	public ForeignKeyNavigationPropertyConfiguration WithRequiredPrincipal(Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
	{
		Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");
		_navigationPropertyConfiguration.InverseNavigationProperty = navigationPropertyExpression.GetSimplePropertyAccess().Single();
		return WithRequiredPrincipal();
	}

	public ForeignKeyNavigationPropertyConfiguration WithRequiredPrincipal()
	{
		_navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.One;
		_navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal = true;
		return new ForeignKeyNavigationPropertyConfiguration(_navigationPropertyConfiguration);
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
