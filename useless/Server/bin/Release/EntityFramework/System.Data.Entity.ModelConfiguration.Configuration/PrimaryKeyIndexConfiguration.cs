using System.Data.Entity.ModelConfiguration.Configuration.Properties.Index;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class PrimaryKeyIndexConfiguration
{
	private readonly System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration _configuration;

	internal PrimaryKeyIndexConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration configuration)
	{
		_configuration = configuration;
	}

	public PrimaryKeyIndexConfiguration IsClustered()
	{
		return IsClustered(clustered: true);
	}

	public PrimaryKeyIndexConfiguration IsClustered(bool clustered)
	{
		_configuration.IsClustered = clustered;
		return this;
	}

	public PrimaryKeyIndexConfiguration HasName(string name)
	{
		_configuration.Name = name;
		return this;
	}
}
