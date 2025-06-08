namespace System.Data.Entity.ModelConfiguration.Configuration;

public abstract class ModificationStoredProcedureConfigurationBase
{
	private readonly ModificationStoredProcedureConfiguration _configuration = new ModificationStoredProcedureConfiguration();

	internal ModificationStoredProcedureConfiguration Configuration => _configuration;

	internal ModificationStoredProcedureConfigurationBase()
	{
	}
}
