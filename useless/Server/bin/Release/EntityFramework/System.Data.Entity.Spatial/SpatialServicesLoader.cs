using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;

namespace System.Data.Entity.Spatial;

internal class SpatialServicesLoader
{
	private readonly IDbDependencyResolver _resolver;

	public SpatialServicesLoader(IDbDependencyResolver resolver)
	{
		_resolver = resolver;
	}

	public virtual DbSpatialServices LoadDefaultServices()
	{
		DbSpatialServices service = _resolver.GetService<DbSpatialServices>();
		if (service != null)
		{
			return service;
		}
		service = _resolver.GetService<DbSpatialServices>(new DbProviderInfo("System.Data.SqlClient", "2012"));
		if (service != null && service.NativeTypesAvailable)
		{
			return service;
		}
		return DefaultSpatialServices.Instance;
	}
}
