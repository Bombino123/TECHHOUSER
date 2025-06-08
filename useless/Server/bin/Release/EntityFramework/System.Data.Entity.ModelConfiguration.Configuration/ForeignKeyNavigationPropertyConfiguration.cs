using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ForeignKeyNavigationPropertyConfiguration : CascadableNavigationPropertyConfiguration
{
	internal ForeignKeyNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
		: base(navigationPropertyConfiguration)
	{
	}

	public CascadableNavigationPropertyConfiguration Map(Action<ForeignKeyAssociationMappingConfiguration> configurationAction)
	{
		Check.NotNull(configurationAction, "configurationAction");
		base.NavigationPropertyConfiguration.Constraint = IndependentConstraintConfiguration.Instance;
		ForeignKeyAssociationMappingConfiguration foreignKeyAssociationMappingConfiguration = new ForeignKeyAssociationMappingConfiguration();
		configurationAction(foreignKeyAssociationMappingConfiguration);
		base.NavigationPropertyConfiguration.AssociationMappingConfiguration = foreignKeyAssociationMappingConfiguration;
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
