using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Spatial;

public abstract class DbSpatialDataReader
{
	public abstract DbGeography GetGeography(int ordinal);

	public virtual Task<DbGeography> GetGeographyAsync(int ordinal, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return TaskHelper.FromCancellation<DbGeography>();
		}
		try
		{
			return Task.FromResult(GetGeography(ordinal));
		}
		catch (Exception ex)
		{
			return TaskHelper.FromException<DbGeography>(ex);
		}
	}

	public abstract DbGeometry GetGeometry(int ordinal);

	public virtual Task<DbGeometry> GetGeometryAsync(int ordinal, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return TaskHelper.FromCancellation<DbGeometry>();
		}
		try
		{
			return Task.FromResult(GetGeometry(ordinal));
		}
		catch (Exception ex)
		{
			return TaskHelper.FromException<DbGeometry>(ex);
		}
	}

	public abstract bool IsGeographyColumn(int ordinal);

	public abstract bool IsGeometryColumn(int ordinal);
}
