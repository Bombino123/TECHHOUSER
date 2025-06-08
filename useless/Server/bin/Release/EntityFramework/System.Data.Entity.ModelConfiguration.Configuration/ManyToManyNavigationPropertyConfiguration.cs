using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> where TEntityType : class where TTargetEntityType : class
{
	private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

	internal ManyToManyNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
	{
		_navigationPropertyConfiguration = navigationPropertyConfiguration;
	}

	public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> Map(Action<ManyToManyAssociationMappingConfiguration> configurationAction)
	{
		Check.NotNull(configurationAction, "configurationAction");
		ManyToManyAssociationMappingConfiguration manyToManyAssociationMappingConfiguration = new ManyToManyAssociationMappingConfiguration();
		configurationAction(manyToManyAssociationMappingConfiguration);
		_navigationPropertyConfiguration.AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration;
		return this;
	}

	public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> MapToStoredProcedures()
	{
		if (_navigationPropertyConfiguration.ModificationStoredProceduresConfiguration == null)
		{
			_navigationPropertyConfiguration.ModificationStoredProceduresConfiguration = new ModificationStoredProceduresConfiguration();
		}
		return this;
	}

	public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> MapToStoredProcedures(Action<ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType>> modificationStoredProcedureMappingConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureMappingConfigurationAction, "modificationStoredProcedureMappingConfigurationAction");
		ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType> manyToManyModificationStoredProceduresConfiguration = new ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType>();
		modificationStoredProcedureMappingConfigurationAction(manyToManyModificationStoredProceduresConfiguration);
		if (_navigationPropertyConfiguration.ModificationStoredProceduresConfiguration == null)
		{
			_navigationPropertyConfiguration.ModificationStoredProceduresConfiguration = manyToManyModificationStoredProceduresConfiguration.Configuration;
		}
		else
		{
			_navigationPropertyConfiguration.ModificationStoredProceduresConfiguration.Merge(manyToManyModificationStoredProceduresConfiguration.Configuration, allowOverride: true);
		}
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
