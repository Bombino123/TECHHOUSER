using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.Data.SQLite;

public sealed class SQLiteFactory : DbProviderFactory, IDisposable, IServiceProvider
{
	private bool disposed;

	public static readonly SQLiteFactory Instance;

	private static readonly string DefaultTypeName;

	private static readonly BindingFlags DefaultBindingFlags;

	private static Type _dbProviderServicesType;

	private static object _sqliteServices;

	public event SQLiteLogEventHandler Log
	{
		add
		{
			CheckDisposed();
			SQLiteLog.Log += value;
		}
		remove
		{
			CheckDisposed();
			SQLiteLog.Log -= value;
		}
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
			throw new ObjectDisposedException(typeof(SQLiteFactory).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
		}
	}

	~SQLiteFactory()
	{
		Dispose(disposing: false);
	}

	public override DbCommand CreateCommand()
	{
		CheckDisposed();
		return new SQLiteCommand();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		CheckDisposed();
		return new SQLiteCommandBuilder();
	}

	public override DbConnection CreateConnection()
	{
		CheckDisposed();
		return new SQLiteConnection();
	}

	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		CheckDisposed();
		return new SQLiteConnectionStringBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		CheckDisposed();
		return new SQLiteDataAdapter();
	}

	public override DbParameter CreateParameter()
	{
		CheckDisposed();
		return new SQLiteParameter();
	}

	static SQLiteFactory()
	{
		Instance = new SQLiteFactory();
		DefaultTypeName = "System.Data.SQLite.Linq.SQLiteProviderServices, System.Data.SQLite.Linq, Version={0}, Culture=neutral, PublicKeyToken=db937bc2d44ff139";
		DefaultBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
		InitializeDbProviderServices();
	}

	internal static void PreInitialize()
	{
		UnsafeNativeMethods.Initialize();
		SQLiteLog.Initialize(typeof(SQLiteFactory).Name);
	}

	private static void InitializeDbProviderServices()
	{
		PreInitialize();
		string text = "4.0.0.0";
		_dbProviderServicesType = Type.GetType(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "System.Data.Common.DbProviderServices, System.Data.Entity, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089", text), throwOnError: false);
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == typeof(ISQLiteSchemaExtensions) || (_dbProviderServicesType != null && serviceType == _dbProviderServicesType))
		{
			object sQLiteProviderServicesInstance = GetSQLiteProviderServicesInstance();
			if (SQLite3.ForceLogLifecycle())
			{
				SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Success of \"{0}\" from SQLiteFactory.GetService(\"{1}\")...", (sQLiteProviderServicesInstance != null) ? sQLiteProviderServicesInstance.ToString() : "<null>", (serviceType != null) ? serviceType.ToString() : "<null>"));
			}
			return sQLiteProviderServicesInstance;
		}
		if (SQLite3.ForceLogLifecycle())
		{
			SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Failure of SQLiteFactory.GetService(\"{0}\")...", (serviceType != null) ? serviceType.ToString() : "<null>"));
		}
		return null;
	}

	[ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
	private object GetSQLiteProviderServicesInstance()
	{
		if (_sqliteServices == null)
		{
			string settingValue = UnsafeNativeMethods.GetSettingValue("TypeName_SQLiteProviderServices", null);
			Version version = GetType().Assembly.GetName().Version;
			settingValue = ((settingValue == null) ? HelperMethods.StringFormat(CultureInfo.InvariantCulture, DefaultTypeName, version) : HelperMethods.StringFormat(CultureInfo.InvariantCulture, settingValue, version));
			Type type = Type.GetType(settingValue, throwOnError: false);
			if (type != null)
			{
				FieldInfo field = type.GetField("Instance", DefaultBindingFlags);
				if (field != null)
				{
					_sqliteServices = field.GetValue(null);
				}
			}
		}
		return _sqliteServices;
	}
}
