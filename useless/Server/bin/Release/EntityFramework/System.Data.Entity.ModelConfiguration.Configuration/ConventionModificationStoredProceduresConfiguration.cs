using System.ComponentModel;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class ConventionModificationStoredProceduresConfiguration
{
	private readonly Type _type;

	private readonly ModificationStoredProceduresConfiguration _configuration = new ModificationStoredProceduresConfiguration();

	internal ModificationStoredProceduresConfiguration Configuration => _configuration;

	internal ConventionModificationStoredProceduresConfiguration(Type type)
	{
		_type = type;
	}

	public ConventionModificationStoredProceduresConfiguration Insert(Action<ConventionInsertModificationStoredProcedureConfiguration> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		ConventionInsertModificationStoredProcedureConfiguration conventionInsertModificationStoredProcedureConfiguration = new ConventionInsertModificationStoredProcedureConfiguration(_type);
		modificationStoredProcedureConfigurationAction(conventionInsertModificationStoredProcedureConfiguration);
		_configuration.Insert(conventionInsertModificationStoredProcedureConfiguration.Configuration);
		return this;
	}

	public ConventionModificationStoredProceduresConfiguration Update(Action<ConventionUpdateModificationStoredProcedureConfiguration> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		ConventionUpdateModificationStoredProcedureConfiguration conventionUpdateModificationStoredProcedureConfiguration = new ConventionUpdateModificationStoredProcedureConfiguration(_type);
		modificationStoredProcedureConfigurationAction(conventionUpdateModificationStoredProcedureConfiguration);
		_configuration.Update(conventionUpdateModificationStoredProcedureConfiguration.Configuration);
		return this;
	}

	public ConventionModificationStoredProceduresConfiguration Delete(Action<ConventionDeleteModificationStoredProcedureConfiguration> modificationStoredProcedureConfigurationAction)
	{
		Check.NotNull(modificationStoredProcedureConfigurationAction, "modificationStoredProcedureConfigurationAction");
		ConventionDeleteModificationStoredProcedureConfiguration conventionDeleteModificationStoredProcedureConfiguration = new ConventionDeleteModificationStoredProcedureConfiguration(_type);
		modificationStoredProcedureConfigurationAction(conventionDeleteModificationStoredProcedureConfiguration);
		_configuration.Delete(conventionDeleteModificationStoredProcedureConfiguration.Configuration);
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
