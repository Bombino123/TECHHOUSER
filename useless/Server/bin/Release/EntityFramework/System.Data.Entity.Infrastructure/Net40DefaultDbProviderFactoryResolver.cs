using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure;

internal class Net40DefaultDbProviderFactoryResolver : IDbProviderFactoryResolver
{
	private readonly ConcurrentDictionary<Type, DbProviderFactory> _cache = new ConcurrentDictionary<Type, DbProviderFactory>(new KeyValuePair<Type, DbProviderFactory>[1]
	{
		new KeyValuePair<Type, DbProviderFactory>(typeof(EntityConnection), EntityProviderFactory.Instance)
	});

	private readonly ProviderRowFinder _finder;

	public Net40DefaultDbProviderFactoryResolver()
		: this(new ProviderRowFinder())
	{
	}

	public Net40DefaultDbProviderFactoryResolver(ProviderRowFinder finder)
	{
		_finder = finder;
	}

	public DbProviderFactory ResolveProviderFactory(DbConnection connection)
	{
		Check.NotNull(connection, "connection");
		return GetProviderFactory(connection, DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>());
	}

	public DbProviderFactory GetProviderFactory(DbConnection connection, IEnumerable<DataRow> dataRows)
	{
		Type type = connection.GetType();
		return _cache.GetOrAdd(type, delegate(Type t)
		{
			DataRow obj = _finder.FindRow(t, (DataRow r) => ExactMatch(r, t), dataRows) ?? _finder.FindRow(null, (DataRow r) => ExactMatch(r, t), dataRows) ?? _finder.FindRow(t, (DataRow r) => AssignableMatch(r, t), dataRows) ?? _finder.FindRow(null, (DataRow r) => AssignableMatch(r, t), dataRows);
			if (obj == null)
			{
				throw new NotSupportedException(Strings.ProviderNotFound(connection.ToString()));
			}
			return DbProviderFactories.GetFactory(obj);
		});
	}

	private static bool ExactMatch(DataRow row, Type connectionType)
	{
		return DbProviderFactories.GetFactory(row).CreateConnection().GetType() == connectionType;
	}

	private static bool AssignableMatch(DataRow row, Type connectionType)
	{
		return connectionType.IsInstanceOfType(DbProviderFactories.GetFactory(row).CreateConnection());
	}
}
