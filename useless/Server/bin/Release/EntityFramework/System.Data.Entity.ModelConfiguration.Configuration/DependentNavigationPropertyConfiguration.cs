using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class DependentNavigationPropertyConfiguration<TDependentEntityType> : ForeignKeyNavigationPropertyConfiguration where TDependentEntityType : class
{
	internal DependentNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
		: base(navigationPropertyConfiguration)
	{
	}

	public CascadableNavigationPropertyConfiguration HasForeignKey<TKey>(Expression<Func<TDependentEntityType, TKey>> foreignKeyExpression)
	{
		Check.NotNull(foreignKeyExpression, "foreignKeyExpression");
		base.NavigationPropertyConfiguration.Constraint = new ForeignKeyConstraintConfiguration(from p in foreignKeyExpression.GetSimplePropertyAccessList()
			select p.Single());
		return this;
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
