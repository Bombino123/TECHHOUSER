using System.Collections;
using System.Configuration;
using System.Data.Entity.Internal.ConfigFile;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.Design;

public class AppConfigReader
{
	private readonly Configuration _configuration;

	public AppConfigReader(Configuration configuration)
	{
		Check.NotNull<Configuration>(configuration, "configuration");
		_configuration = configuration;
	}

	public string GetProviderServices(string invariantName)
	{
		return (from ProviderElement p in (IEnumerable)((EntityFrameworkSection)(object)_configuration.GetSection("entityFramework"))?.Providers
			where p.InvariantName == invariantName
			select p.ProviderTypeName).FirstOrDefault();
	}
}
