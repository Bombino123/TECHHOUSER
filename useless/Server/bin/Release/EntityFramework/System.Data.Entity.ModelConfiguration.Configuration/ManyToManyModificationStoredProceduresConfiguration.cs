using System.ComponentModel;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType> where TEntityType : class where TTargetEntityType : class
{
	private readonly ModificationStoredProceduresConfiguration _configuration = new ModificationStoredProceduresConfiguration();

	internal ModificationStoredProceduresConfiguration Configuration => _configuration;

	internal ManyToManyModificationStoredProceduresConfiguration()
	{
	}

	public ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType> Insert(Action<ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> manyToManyModificationStoredProcedureConfiguration = new ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>();
		modificationStoredProcedureConfigurationAction(manyToManyModificationStoredProcedureConfiguration);
		_configuration.Insert(manyToManyModificationStoredProcedureConfiguration.Configuration);
		return this;
	}

	public ManyToManyModificationStoredProceduresConfiguration<TEntityType, TTargetEntityType> Delete(Action<ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType> manyToManyModificationStoredProcedureConfiguration = new ManyToManyModificationStoredProcedureConfiguration<TEntityType, TTargetEntityType>();
		modificationStoredProcedureConfigurationAction(manyToManyModificationStoredProcedureConfiguration);
		_configuration.Delete(manyToManyModificationStoredProcedureConfiguration.Configuration);
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
