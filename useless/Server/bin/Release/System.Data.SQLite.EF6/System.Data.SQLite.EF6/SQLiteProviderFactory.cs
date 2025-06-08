using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Globalization;

namespace System.Data.SQLite.EF6;

public sealed class SQLiteProviderFactory : DbProviderFactory, IServiceProvider, IDisposable
{
	public static readonly SQLiteProviderFactory Instance = new SQLiteProviderFactory();

	private bool disposed;

	public override DbCommand CreateCommand()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		CheckDisposed();
		return (DbCommand)new SQLiteCommand();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		CheckDisposed();
		return (DbCommandBuilder)new SQLiteCommandBuilder();
	}

	public override DbConnection CreateConnection()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		CheckDisposed();
		return (DbConnection)new SQLiteConnection();
	}

	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		CheckDisposed();
		return (DbConnectionStringBuilder)new SQLiteConnectionStringBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		CheckDisposed();
		return (DbDataAdapter)new SQLiteDataAdapter();
	}

	public override DbParameter CreateParameter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		CheckDisposed();
		return (DbParameter)new SQLiteParameter();
	}

	public object GetService(Type serviceType)
	{
		if (serviceType == typeof(ISQLiteSchemaExtensions) || serviceType == typeof(DbProviderServices))
		{
			object instance = SQLiteProviderServices.Instance;
			if (SQLite3.ForceLogLifecycle())
			{
				SQLiteLog.LogMessage(HelperMethods.StringFormat((IFormatProvider)CultureInfo.CurrentCulture, "Success of \"{0}\" from SQLiteProviderFactory.GetService(\"{1}\")...", new object[2]
				{
					(instance != null) ? instance.ToString() : "<null>",
					(serviceType != null) ? serviceType.ToString() : "<null>"
				}));
			}
			return instance;
		}
		if (SQLite3.ForceLogLifecycle())
		{
			SQLiteLog.LogMessage(HelperMethods.StringFormat((IFormatProvider)CultureInfo.CurrentCulture, "Failure of SQLiteProviderFactory.GetService(\"{0}\")...", new object[1] { (serviceType != null) ? serviceType.ToString() : "<null>" }));
		}
		return null;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteProviderFactory).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
		}
	}

	~SQLiteProviderFactory()
	{
		Dispose(disposing: false);
	}
}
