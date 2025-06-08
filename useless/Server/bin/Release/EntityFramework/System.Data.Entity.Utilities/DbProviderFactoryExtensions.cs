using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.EntityClient.Internal;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Resources;
using System.Data.SqlClient;
using System.Linq;

namespace System.Data.Entity.Utilities;

internal static class DbProviderFactoryExtensions
{
	public static string GetProviderInvariantName(this DbProviderFactory factory)
	{
		IEnumerable<DataRow> dataRows = DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>();
		DataRow dataRow = new ProviderRowFinder().FindRow(factory.GetType(), (DataRow r) => DbProviderFactories.GetFactory(r).GetType() == factory.GetType(), dataRows);
		if (dataRow == null)
		{
			if (factory.GetType() == typeof(SqlClientFactory))
			{
				return "System.Data.SqlClient";
			}
			throw new NotSupportedException(Strings.ProviderNameNotFound(factory));
		}
		return (string)dataRow[2];
	}

	internal static DbProviderServices GetProviderServices(this DbProviderFactory factory)
	{
		if (factory is EntityProviderFactory)
		{
			return EntityProviderServices.Instance;
		}
		IProviderInvariantName service = DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(factory);
		return DbConfiguration.DependencyResolver.GetService<DbProviderServices>(service.Name);
	}
}
