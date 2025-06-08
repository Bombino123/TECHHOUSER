using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Spatial;

internal static class SpatialHelpers
{
	internal static object GetSpatialValue(MetadataWorkspace workspace, DbDataReader reader, TypeUsage columnType, int columnOrdinal)
	{
		DbSpatialDataReader dbSpatialDataReader = CreateSpatialDataReader(workspace, reader);
		if (Helper.IsGeographicType((PrimitiveType)columnType.EdmType))
		{
			return dbSpatialDataReader.GetGeography(columnOrdinal);
		}
		return dbSpatialDataReader.GetGeometry(columnOrdinal);
	}

	internal static async Task<object> GetSpatialValueAsync(MetadataWorkspace workspace, DbDataReader reader, TypeUsage columnType, int columnOrdinal, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		DbSpatialDataReader dbSpatialDataReader = CreateSpatialDataReader(workspace, reader);
		if (Helper.IsGeographicType((PrimitiveType)columnType.EdmType))
		{
			return await dbSpatialDataReader.GetGeographyAsync(columnOrdinal, cancellationToken).WithCurrentCulture();
		}
		return await dbSpatialDataReader.GetGeometryAsync(columnOrdinal, cancellationToken).WithCurrentCulture();
	}

	internal static DbSpatialDataReader CreateSpatialDataReader(MetadataWorkspace workspace, DbDataReader reader)
	{
		StoreItemCollection storeItemCollection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
		return storeItemCollection.ProviderFactory.GetProviderServices().GetSpatialDataReader(reader, storeItemCollection.ProviderManifestToken) ?? throw new ProviderIncompatibleException(Strings.ProviderDidNotReturnSpatialServices);
	}
}
