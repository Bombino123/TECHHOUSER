using System.Data.Entity.ModelConfiguration.Configuration.Properties.Index;

namespace System.Data.Entity.ModelConfiguration.Configuration;

public class IndexConfiguration
{
	private readonly System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration _configuration;

	internal IndexConfiguration(System.Data.Entity.ModelConfiguration.Configuration.Properties.Index.IndexConfiguration configuration)
	{
		_configuration = configuration;
	}

	public IndexConfiguration IsUnique()
	{
		return IsUnique(unique: true);
	}

	public IndexConfiguration IsUnique(bool unique)
	{
		_configuration.IsUnique = unique;
		return this;
	}

	public IndexConfiguration IsClustered()
	{
		return IsClustered(clustered: true);
	}

	public IndexConfiguration IsClustered(bool clustered)
	{
		_configuration.IsClustered = clustered;
		return this;
	}

	public IndexConfiguration HasName(string name)
	{
		_configuration.Name = name;
		return this;
	}
}
