#define TRACE
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Transactions;

namespace System.Data.SQLite;

public sealed class SQLiteConnection : DbConnection, ICloneable, IDisposable
{
	internal const DbType BadDbType = (DbType)(-1);

	internal const string DefaultBaseSchemaName = "sqlite_default_schema";

	private const string MemoryFileName = ":memory:";

	internal const IsolationLevel DeferredIsolationLevel = IsolationLevel.ReadCommitted;

	internal const IsolationLevel ImmediateIsolationLevel = IsolationLevel.Serializable;

	private const SQLiteConnectionFlags FallbackDefaultFlags = SQLiteConnectionFlags.Default;

	private const SQLiteSynchronousEnum DefaultSynchronous = SQLiteSynchronousEnum.Default;

	private const SQLiteJournalModeEnum DefaultJournalMode = SQLiteJournalModeEnum.Default;

	private const IsolationLevel DefaultIsolationLevel = IsolationLevel.Serializable;

	internal const SQLiteDateFormats DefaultDateTimeFormat = SQLiteDateFormats.ISO8601;

	internal const DateTimeKind DefaultDateTimeKind = DateTimeKind.Unspecified;

	internal const string DefaultDateTimeFormatString = null;

	private const string DefaultDataSource = null;

	private const string DefaultUri = null;

	private const string DefaultFullUri = null;

	private const string DefaultTextPassword = null;

	private const string DefaultHexPassword = null;

	private const string DefaultPassword = null;

	private const int DefaultVersion = 3;

	private const int DefaultPageSize = 4096;

	private const int DefaultMaxPageCount = 0;

	private const int DefaultCacheSize = -2000;

	private const int DefaultMaxPoolSize = 100;

	private const int DefaultConnectionTimeout = 30;

	private const int DefaultBusyTimeout = 0;

	private const int DefaultWaitTimeout = 30000;

	private const bool DefaultNoDefaultFlags = false;

	private const bool DefaultNoSharedFlags = false;

	private const bool DefaultFailIfMissing = false;

	private const bool DefaultReadOnly = false;

	internal const bool DefaultBinaryGUID = true;

	private const bool DefaultUseUTF16Encoding = false;

	private const bool DefaultToFullPath = true;

	private const bool DefaultPooling = false;

	private const bool DefaultLegacyFormat = false;

	private const bool DefaultForeignKeys = false;

	private const bool DefaultRecursiveTriggers = false;

	private const bool DefaultEnlist = true;

	private const bool DefaultSetDefaults = true;

	internal const int DefaultPrepareRetries = 3;

	private static readonly DbType? _DefaultDbType = null;

	private const string _DefaultTypeName = null;

	private const string DefaultVfsName = null;

	private const int DefaultProgressOps = 0;

	private const int SQLITE_FCNTL_CHUNK_SIZE = 6;

	private const int SQLITE_FCNTL_WIN32_AV_RETRY = 9;

	private const string _dataDirectory = "|DataDirectory|";

	private static string _defaultCatalogName = "main";

	private static string _defaultMasterTableName = "sqlite_master";

	private static string _temporaryCatalogName = "temp";

	private static string _temporaryMasterTableName = "sqlite_temp_master";

	private static readonly Assembly _assembly = typeof(SQLiteConnection).Assembly;

	private static readonly object _syncRoot = new object();

	private static SQLiteConnectionFlags _sharedFlags;

	[ThreadStatic]
	private static SQLiteConnection _lastConnectionInOpen;

	private ConnectionState _connectionState;

	private string _connectionString;

	internal int _transactionLevel;

	internal int _transactionSequence;

	internal bool _noDispose;

	private bool _disposing;

	private IsolationLevel _defaultIsolation;

	internal readonly object _enlistmentSyncRoot = new object();

	internal SQLiteEnlistment _enlistment;

	internal SQLiteDbTypeMap _typeNames;

	private SQLiteTypeCallbacksMap _typeCallbacks;

	internal SQLiteBase _sql;

	private string _dataSource;

	private byte[] _password;

	private bool _passwordWasText;

	internal string _baseSchemaName;

	private SQLiteConnectionFlags _flags;

	private Dictionary<string, object> _cachedSettings;

	private DbType? _defaultDbType;

	private string _defaultTypeName;

	private string _vfsName;

	private int _defaultTimeout;

	private int _busyTimeout;

	private int _waitTimeout;

	internal int _prepareRetries;

	private int _progressOps;

	private bool _parseViaFramework;

	internal bool _binaryGuid;

	internal int _version;

	private SQLiteBusyCallback _busyCallback;

	private SQLiteProgressCallback _progressCallback;

	private SQLiteAuthorizerCallback _authorizerCallback;

	private SQLiteUpdateCallback _updateCallback;

	private SQLiteCommitCallback _commitCallback;

	private SQLiteTraceCallback _traceCallback;

	private SQLiteRollbackCallback _rollbackCallback;

	private bool disposed;

	public static ISQLiteConnectionPool ConnectionPool
	{
		get
		{
			return SQLiteConnectionPool.GetConnectionPool();
		}
		set
		{
			SQLiteConnectionPool.SetConnectionPool(value);
		}
	}

	public int PoolCount
	{
		get
		{
			if (_sql == null)
			{
				return 0;
			}
			return _sql.CountPool();
		}
	}

	[RefreshProperties(RefreshProperties.All)]
	[DefaultValue("")]
	[Editor("SQLite.Designer.SQLiteConnectionStringEditor, SQLite.Designer, Version=1.0.115.5, Culture=neutral, PublicKeyToken=db937bc2d44ff139", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public override string ConnectionString
	{
		get
		{
			CheckDisposed();
			return _connectionString;
		}
		set
		{
			CheckDisposed();
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (_connectionState != 0)
			{
				throw new InvalidOperationException();
			}
			_connectionString = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string DataSource
	{
		get
		{
			CheckDisposed();
			return _dataSource;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string FileName
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for getting file name.");
			}
			return _sql.GetFileName(GetDefaultCatalogName());
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string Database
	{
		get
		{
			CheckDisposed();
			return GetDefaultCatalogName();
		}
	}

	public int DefaultTimeout
	{
		get
		{
			CheckDisposed();
			return _defaultTimeout;
		}
		set
		{
			CheckDisposed();
			_defaultTimeout = value;
		}
	}

	public int BusyTimeout
	{
		get
		{
			CheckDisposed();
			return _busyTimeout;
		}
		set
		{
			CheckDisposed();
			_busyTimeout = value;
		}
	}

	public int WaitTimeout
	{
		get
		{
			CheckDisposed();
			return _waitTimeout;
		}
		set
		{
			CheckDisposed();
			_waitTimeout = value;
		}
	}

	public int PrepareRetries
	{
		get
		{
			CheckDisposed();
			return _prepareRetries;
		}
		set
		{
			CheckDisposed();
			_prepareRetries = value;
		}
	}

	public int ProgressOps
	{
		get
		{
			CheckDisposed();
			return _progressOps;
		}
		set
		{
			CheckDisposed();
			_progressOps = value;
		}
	}

	public bool ParseViaFramework
	{
		get
		{
			CheckDisposed();
			return _parseViaFramework;
		}
		set
		{
			CheckDisposed();
			_parseViaFramework = value;
		}
	}

	public SQLiteConnectionFlags Flags
	{
		get
		{
			CheckDisposed();
			return _flags;
		}
		set
		{
			CheckDisposed();
			_flags = value;
		}
	}

	public DbType? DefaultDbType
	{
		get
		{
			CheckDisposed();
			return _defaultDbType;
		}
		set
		{
			CheckDisposed();
			_defaultDbType = value;
		}
	}

	public string DefaultTypeName
	{
		get
		{
			CheckDisposed();
			return _defaultTypeName;
		}
		set
		{
			CheckDisposed();
			_defaultTypeName = value;
		}
	}

	public string VfsName
	{
		get
		{
			CheckDisposed();
			return _vfsName;
		}
		set
		{
			CheckDisposed();
			_vfsName = value;
		}
	}

	public bool OwnHandle
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for checking handle.");
			}
			return _sql.OwnHandle;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string ServerVersion
	{
		get
		{
			CheckDisposed();
			return SQLiteVersion;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long LastInsertRowId
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for getting last insert rowid.");
			}
			return _sql.LastInsertRowId;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Changes
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for getting number of changes.");
			}
			return _sql.Changes;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool AutoCommit
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for getting autocommit mode.");
			}
			return _sql.AutoCommit;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long MemoryUsed
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for getting memory used.");
			}
			return _sql.MemoryUsed;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long MemoryHighwater
	{
		get
		{
			CheckDisposed();
			if (_sql == null)
			{
				throw new InvalidOperationException("Database connection not valid for getting maximum memory used.");
			}
			return _sql.MemoryHighwater;
		}
	}

	public static string DefineConstants => SQLite3.DefineConstants;

	public static string SQLiteVersion => SQLite3.SQLiteVersion;

	public static string SQLiteSourceId => SQLite3.SQLiteSourceId;

	public static string SQLiteCompileOptions => SQLite3.SQLiteCompileOptions;

	public static string InteropVersion => SQLite3.InteropVersion;

	public static string InteropSourceId => SQLite3.InteropSourceId;

	public static string InteropCompileOptions => SQLite3.InteropCompileOptions;

	public static string ProviderVersion
	{
		get
		{
			if (!(_assembly != null))
			{
				return null;
			}
			return _assembly.GetName().Version.ToString();
		}
	}

	public static string ProviderSourceId
	{
		get
		{
			if (_assembly == null)
			{
				return null;
			}
			string text = null;
			if (_assembly.IsDefined(typeof(AssemblySourceIdAttribute), inherit: false))
			{
				text = ((AssemblySourceIdAttribute)_assembly.GetCustomAttributes(typeof(AssemblySourceIdAttribute), inherit: false)[0]).SourceId;
			}
			string text2 = null;
			if (_assembly.IsDefined(typeof(AssemblySourceTimeStampAttribute), inherit: false))
			{
				text2 = ((AssemblySourceTimeStampAttribute)_assembly.GetCustomAttributes(typeof(AssemblySourceTimeStampAttribute), inherit: false)[0]).SourceTimeStamp;
			}
			if (text != null || text2 != null)
			{
				if (text == null)
				{
					text = "0000000000000000000000000000000000000000";
				}
				if (text2 == null)
				{
					text2 = "0000-00-00 00:00:00 UTC";
				}
				return HelperMethods.StringFormat(CultureInfo.InvariantCulture, "{0} {1}", text, text2);
			}
			return null;
		}
	}

	public static SQLiteConnectionFlags DefaultFlags
	{
		get
		{
			string name = "DefaultFlags_SQLiteConnection";
			if (!TryGetLastCachedSetting(name, null, out var value))
			{
				value = UnsafeNativeMethods.GetSettingValue(name, null);
				SetLastCachedSetting(name, value);
			}
			if (value == null)
			{
				return SQLiteConnectionFlags.Default;
			}
			object obj = TryParseEnum(typeof(SQLiteConnectionFlags), value.ToString(), ignoreCase: true);
			if (obj is SQLiteConnectionFlags)
			{
				return (SQLiteConnectionFlags)obj;
			}
			return SQLiteConnectionFlags.Default;
		}
	}

	public static SQLiteConnectionFlags SharedFlags
	{
		get
		{
			lock (_syncRoot)
			{
				return _sharedFlags;
			}
		}
		set
		{
			lock (_syncRoot)
			{
				_sharedFlags = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override ConnectionState State
	{
		get
		{
			CheckDisposed();
			return _connectionState;
		}
	}

	protected override DbProviderFactory DbProviderFactory
	{
		get
		{
			DbProviderFactory instance = SQLiteFactory.Instance;
			if (SQLite3.ForceLogLifecycle())
			{
				SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Returning \"{0}\" from SQLiteConnection.DbProviderFactory...", (instance != null) ? instance.ToString() : "<null>"));
			}
			return instance;
		}
	}

	private static event SQLiteConnectionEventHandler _handlers;

	private event SQLiteBusyEventHandler _busyHandler;

	private event SQLiteProgressEventHandler _progressHandler;

	private event SQLiteAuthorizerEventHandler _authorizerHandler;

	private event SQLiteUpdateEventHandler _updateHandler;

	private event SQLiteCommitHandler _commitHandler;

	private event SQLiteTraceEventHandler _traceHandler;

	private event EventHandler _rollbackHandler;

	public override event StateChangeEventHandler StateChange;

	public static event SQLiteConnectionEventHandler Changed
	{
		add
		{
			lock (_syncRoot)
			{
				_handlers -= value;
				_handlers += value;
			}
		}
		remove
		{
			lock (_syncRoot)
			{
				_handlers -= value;
			}
		}
	}

	public event SQLiteBusyEventHandler Busy
	{
		add
		{
			CheckDisposed();
			if (this._busyHandler == null)
			{
				_busyCallback = BusyCallback;
				if (_sql != null)
				{
					_sql.SetBusyHook(_busyCallback);
				}
			}
			_busyHandler += value;
		}
		remove
		{
			CheckDisposed();
			_busyHandler -= value;
			if (this._busyHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetBusyHook(null);
				}
				_busyCallback = null;
			}
		}
	}

	public event SQLiteProgressEventHandler Progress
	{
		add
		{
			CheckDisposed();
			if (this._progressHandler == null)
			{
				_progressCallback = ProgressCallback;
				if (_sql != null)
				{
					_sql.SetProgressHook(_progressOps, _progressCallback);
				}
			}
			_progressHandler += value;
		}
		remove
		{
			CheckDisposed();
			_progressHandler -= value;
			if (this._progressHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetProgressHook(0, null);
				}
				_progressCallback = null;
			}
		}
	}

	public event SQLiteAuthorizerEventHandler Authorize
	{
		add
		{
			CheckDisposed();
			if (this._authorizerHandler == null)
			{
				_authorizerCallback = AuthorizerCallback;
				if (_sql != null)
				{
					_sql.SetAuthorizerHook(_authorizerCallback);
				}
			}
			_authorizerHandler += value;
		}
		remove
		{
			CheckDisposed();
			_authorizerHandler -= value;
			if (this._authorizerHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetAuthorizerHook(null);
				}
				_authorizerCallback = null;
			}
		}
	}

	public event SQLiteUpdateEventHandler Update
	{
		add
		{
			CheckDisposed();
			if (this._updateHandler == null)
			{
				_updateCallback = UpdateCallback;
				if (_sql != null)
				{
					_sql.SetUpdateHook(_updateCallback);
				}
			}
			_updateHandler += value;
		}
		remove
		{
			CheckDisposed();
			_updateHandler -= value;
			if (this._updateHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetUpdateHook(null);
				}
				_updateCallback = null;
			}
		}
	}

	public event SQLiteCommitHandler Commit
	{
		add
		{
			CheckDisposed();
			if (this._commitHandler == null)
			{
				_commitCallback = CommitCallback;
				if (_sql != null)
				{
					_sql.SetCommitHook(_commitCallback);
				}
			}
			_commitHandler += value;
		}
		remove
		{
			CheckDisposed();
			_commitHandler -= value;
			if (this._commitHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetCommitHook(null);
				}
				_commitCallback = null;
			}
		}
	}

	public event SQLiteTraceEventHandler Trace
	{
		add
		{
			CheckDisposed();
			if (this._traceHandler == null)
			{
				_traceCallback = TraceCallback;
				if (_sql != null)
				{
					_sql.SetTraceCallback(_traceCallback);
				}
			}
			_traceHandler += value;
		}
		remove
		{
			CheckDisposed();
			_traceHandler -= value;
			if (this._traceHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetTraceCallback(null);
				}
				_traceCallback = null;
			}
		}
	}

	public event EventHandler RollBack
	{
		add
		{
			CheckDisposed();
			if (this._rollbackHandler == null)
			{
				_rollbackCallback = RollbackCallback;
				if (_sql != null)
				{
					_sql.SetRollbackHook(_rollbackCallback);
				}
			}
			_rollbackHandler += value;
		}
		remove
		{
			CheckDisposed();
			_rollbackHandler -= value;
			if (this._rollbackHandler == null)
			{
				if (_sql != null)
				{
					_sql.SetRollbackHook(null);
				}
				_rollbackCallback = null;
			}
		}
	}

	private static string GetDefaultCatalogName()
	{
		return _defaultCatalogName;
	}

	private static bool IsDefaultCatalogName(string catalogName)
	{
		return string.Compare(catalogName, GetDefaultCatalogName(), StringComparison.OrdinalIgnoreCase) == 0;
	}

	private static string GetTemporaryCatalogName()
	{
		return _temporaryCatalogName;
	}

	private static bool IsTemporaryCatalogName(string catalogName)
	{
		return string.Compare(catalogName, GetTemporaryCatalogName(), StringComparison.OrdinalIgnoreCase) == 0;
	}

	private static string GetMasterTableName(bool temporary)
	{
		if (!temporary)
		{
			return _defaultMasterTableName;
		}
		return _temporaryMasterTableName;
	}

	public SQLiteConnection()
		: this((string)null)
	{
	}

	public SQLiteConnection(string connectionString)
		: this(connectionString, parseViaFramework: false)
	{
	}

	internal SQLiteConnection(IntPtr db, string fileName, bool ownHandle)
		: this()
	{
		_sql = new SQLite3(SQLiteDateFormats.ISO8601, DateTimeKind.Unspecified, null, db, fileName, ownHandle);
		_flags = SQLiteConnectionFlags.None;
		_connectionState = ((db != IntPtr.Zero) ? ConnectionState.Open : ConnectionState.Closed);
		_connectionString = null;
	}

	private void InitializeDefaults()
	{
		_defaultDbType = _DefaultDbType;
		_defaultTypeName = null;
		_vfsName = null;
		_defaultTimeout = 30;
		_busyTimeout = 0;
		_waitTimeout = 30000;
		_prepareRetries = 3;
		_progressOps = 0;
		_defaultIsolation = IsolationLevel.Serializable;
		_baseSchemaName = "sqlite_default_schema";
		_binaryGuid = true;
	}

	public SQLiteConnection(string connectionString, bool parseViaFramework)
	{
		_noDispose = false;
		UnsafeNativeMethods.Initialize();
		SQLiteLog.Initialize(typeof(SQLiteConnection).Name);
		_cachedSettings = new Dictionary<string, object>(new TypeNameStringComparer());
		_typeNames = new SQLiteDbTypeMap();
		_typeCallbacks = new SQLiteTypeCallbacksMap();
		_parseViaFramework = parseViaFramework;
		_flags = SQLiteConnectionFlags.None;
		InitializeDefaults();
		_connectionState = ConnectionState.Closed;
		_connectionString = null;
		if (connectionString != null)
		{
			ConnectionString = connectionString;
		}
	}

	public SQLiteConnection(SQLiteConnection connection)
		: this(connection.ConnectionString, connection.ParseViaFramework)
	{
		if (connection.State != ConnectionState.Open)
		{
			return;
		}
		Open();
		using DataTable dataTable = connection.GetSchema("Catalogs");
		foreach (DataRow row in dataTable.Rows)
		{
			string catalogName = row[0].ToString();
			if (!IsDefaultCatalogName(catalogName) && !IsTemporaryCatalogName(catalogName))
			{
				using SQLiteCommand sQLiteCommand = CreateCommand();
				sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "ATTACH DATABASE '{0}' AS [{1}]", row[1], row[0]);
				sQLiteCommand.ExecuteNonQuery();
			}
		}
	}

	private static SQLiteConnectionHandle GetNativeHandle(SQLiteConnection connection)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		SQLiteConnectionHandle obj = ((connection._sql as SQLite3) ?? throw new InvalidOperationException("Connection has no wrapper"))._sql ?? throw new InvalidOperationException("Connection has an invalid handle.");
		if (obj == IntPtr.Zero)
		{
			throw new InvalidOperationException("Connection has an invalid handle pointer.");
		}
		return obj;
	}

	internal static void OnChanged(SQLiteConnection connection, ConnectionEventArgs e)
	{
		if (connection == null || connection.CanRaiseEvents)
		{
			SQLiteConnectionEventHandler sQLiteConnectionEventHandler;
			lock (_syncRoot)
			{
				sQLiteConnectionEventHandler = ((SQLiteConnection._handlers == null) ? null : (SQLiteConnection._handlers.Clone() as SQLiteConnectionEventHandler));
			}
			sQLiteConnectionEventHandler?.Invoke(connection, e);
		}
	}

	public static object CreateHandle(IntPtr nativeHandle)
	{
		SQLiteConnectionHandle sQLiteConnectionHandle;
		try
		{
		}
		finally
		{
			sQLiteConnectionHandle = ((nativeHandle != IntPtr.Zero) ? new SQLiteConnectionHandle(nativeHandle, ownHandle: true) : null);
		}
		if (sQLiteConnectionHandle != null)
		{
			OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteConnectionHandle, null, new object[2]
			{
				typeof(SQLiteConnection),
				nativeHandle
			}));
		}
		return sQLiteConnectionHandle;
	}

	public void BackupDatabase(SQLiteConnection destination, string destinationName, string sourceName, int pages, SQLiteBackupCallback callback, int retryMilliseconds)
	{
		CheckDisposed();
		if (_connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException("Source database is not open.");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination._connectionState != ConnectionState.Open)
		{
			throw new ArgumentException("Destination database is not open.", "destination");
		}
		if (destinationName == null)
		{
			throw new ArgumentNullException("destinationName");
		}
		if (sourceName == null)
		{
			throw new ArgumentNullException("sourceName");
		}
		SQLiteBase sql = _sql;
		if (sql == null)
		{
			throw new InvalidOperationException("Connection object has an invalid handle.");
		}
		SQLiteBackup sQLiteBackup = null;
		try
		{
			sQLiteBackup = sql.InitializeBackup(destination, destinationName, sourceName);
			bool retry = false;
			while (sql.StepBackup(sQLiteBackup, pages, ref retry) && (callback == null || callback(this, sourceName, destination, destinationName, pages, sql.RemainingBackup(sQLiteBackup), sql.PageCountBackup(sQLiteBackup), retry)))
			{
				if (retry && retryMilliseconds >= 0)
				{
					Thread.Sleep(retryMilliseconds);
				}
				if (pages == 0)
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			if (HelperMethods.LogBackup(_flags))
			{
				SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception while backing up database: {0}", ex));
			}
			throw;
		}
		finally
		{
			if (sQLiteBackup != null)
			{
				sql.FinishBackup(sQLiteBackup);
			}
		}
	}

	public int ClearCachedSettings()
	{
		CheckDisposed();
		int result = -1;
		if (_cachedSettings != null)
		{
			result = _cachedSettings.Count;
			_cachedSettings.Clear();
		}
		return result;
	}

	internal bool TryGetCachedSetting(string name, object @default, out object value)
	{
		if (name == null || _cachedSettings == null)
		{
			value = @default;
			return false;
		}
		return _cachedSettings.TryGetValue(name, out value);
	}

	internal void SetCachedSetting(string name, object value)
	{
		if (name != null && _cachedSettings != null)
		{
			_cachedSettings[name] = value;
		}
	}

	public int ClearTypeMappings()
	{
		CheckDisposed();
		int result = -1;
		if (_typeNames != null)
		{
			result = _typeNames.Clear();
		}
		return result;
	}

	public Dictionary<string, object> GetTypeMappings()
	{
		CheckDisposed();
		Dictionary<string, object> dictionary = null;
		if (_typeNames != null)
		{
			dictionary = new Dictionary<string, object>(_typeNames.Count, _typeNames.Comparer);
			foreach (KeyValuePair<string, SQLiteDbTypeMapping> typeName in _typeNames)
			{
				SQLiteDbTypeMapping value = typeName.Value;
				object obj = null;
				object obj2 = null;
				object obj3 = null;
				if (value != null)
				{
					obj = value.typeName;
					obj2 = value.dataType;
					obj3 = value.primary;
				}
				dictionary.Add(typeName.Key, new object[3] { obj, obj2, obj3 });
			}
		}
		return dictionary;
	}

	public int AddTypeMapping(string typeName, DbType dataType, bool primary)
	{
		CheckDisposed();
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		int num = -1;
		if (_typeNames != null)
		{
			num = 0;
			if (primary && _typeNames.ContainsKey(dataType))
			{
				num += (_typeNames.Remove(dataType) ? 1 : 0);
			}
			if (_typeNames.ContainsKey(typeName))
			{
				num += (_typeNames.Remove(typeName) ? 1 : 0);
			}
			_typeNames.Add(new SQLiteDbTypeMapping(typeName, dataType, primary));
		}
		return num;
	}

	public int ClearTypeCallbacks()
	{
		CheckDisposed();
		int result = -1;
		if (_typeCallbacks != null)
		{
			result = _typeCallbacks.Count;
			_typeCallbacks.Clear();
		}
		return result;
	}

	public bool TryGetTypeCallbacks(string typeName, out SQLiteTypeCallbacks callbacks)
	{
		CheckDisposed();
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (_typeCallbacks == null)
		{
			callbacks = null;
			return false;
		}
		return _typeCallbacks.TryGetValue(typeName, out callbacks);
	}

	public bool SetTypeCallbacks(string typeName, SQLiteTypeCallbacks callbacks)
	{
		CheckDisposed();
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (_typeCallbacks == null)
		{
			return false;
		}
		if (callbacks == null)
		{
			return _typeCallbacks.Remove(typeName);
		}
		callbacks.TypeName = typeName;
		_typeCallbacks[typeName] = callbacks;
		return true;
	}

	public void BindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteFunction function)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for binding functions.");
		}
		_sql.BindFunction(functionAttribute, function, _flags);
	}

	public void BindFunction(SQLiteFunctionAttribute functionAttribute, Delegate callback1, Delegate callback2)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for binding functions.");
		}
		_sql.BindFunction(functionAttribute, new SQLiteDelegateFunction(callback1, callback2), _flags);
	}

	public bool UnbindFunction(SQLiteFunctionAttribute functionAttribute)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for unbinding functions.");
		}
		return _sql.UnbindFunction(functionAttribute, _flags);
	}

	public bool UnbindAllFunctions(bool registered)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for unbinding functions.");
		}
		return SQLiteFunction.UnbindAllFunctions(_sql, _flags, registered);
	}

	[Conditional("CHECK_STATE")]
	internal static void Check(SQLiteConnection connection)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		connection.CheckDisposed();
		if (connection._connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException("The connection is not open.");
		}
		SQLiteConnectionHandle obj = ((connection._sql as SQLite3) ?? throw new InvalidOperationException("The connection handle wrapper is null."))._sql ?? throw new InvalidOperationException("The connection handle is null.");
		if (obj.IsInvalid)
		{
			throw new InvalidOperationException("The connection handle is invalid.");
		}
		if (obj.IsClosed)
		{
			throw new InvalidOperationException("The connection handle is closed.");
		}
	}

	internal static SortedList<string, string> ParseConnectionString(string connectionString, bool parseViaFramework, bool allowNameOnly)
	{
		return ParseConnectionString(null, connectionString, parseViaFramework, allowNameOnly);
	}

	private static SortedList<string, string> ParseConnectionString(SQLiteConnection connection, string connectionString, bool parseViaFramework, bool allowNameOnly)
	{
		if (!parseViaFramework)
		{
			return ParseConnectionString(connection, connectionString, allowNameOnly);
		}
		return ParseConnectionStringViaFramework(connection, connectionString, strict: false);
	}

	private static string EscapeForConnectionString(string value, bool allowEquals)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (value.IndexOfAny(SQLiteConvert.SpecialChars) == -1)
		{
			return value;
		}
		int length = value.Length;
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			switch (c)
			{
			case '"':
			case '\'':
			case ';':
			case '\\':
				stringBuilder.Append('\\');
				stringBuilder.Append(c);
				break;
			case '=':
				if (allowEquals)
				{
					stringBuilder.Append(c);
					break;
				}
				throw new ArgumentException("equals sign character is not allowed here");
			default:
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	private static string BuildConnectionString(SortedList<string, string> opts)
	{
		if (opts == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> opt in opts)
		{
			stringBuilder.AppendFormat("{0}{1}{2}{3}", EscapeForConnectionString(opt.Key, allowEquals: false), '=', EscapeForConnectionString(opt.Value, allowEquals: true), ';');
		}
		return stringBuilder.ToString();
	}

	private void SetupSQLiteBase(SortedList<string, string> opts)
	{
		object obj = TryParseEnum(typeof(SQLiteDateFormats), FindKey(opts, "DateTimeFormat", SQLiteDateFormats.ISO8601.ToString()), ignoreCase: true);
		SQLiteDateFormats fmt = ((!(obj is SQLiteDateFormats)) ? SQLiteDateFormats.ISO8601 : ((SQLiteDateFormats)obj));
		obj = TryParseEnum(typeof(DateTimeKind), FindKey(opts, "DateTimeKind", DateTimeKind.Unspecified.ToString()), ignoreCase: true);
		DateTimeKind kind = ((obj is DateTimeKind) ? ((DateTimeKind)obj) : DateTimeKind.Unspecified);
		string fmtString = FindKey(opts, "DateTimeFormatString", null);
		if (SQLiteConvert.ToBoolean(FindKey(opts, "UseUTF16Encoding", false.ToString())))
		{
			_sql = new SQLite3_UTF16(fmt, kind, fmtString, IntPtr.Zero, null, ownHandle: false);
		}
		else
		{
			_sql = new SQLite3(fmt, kind, fmtString, IntPtr.Zero, null, ownHandle: false);
		}
	}

	public new void Dispose()
	{
		if (!_noDispose)
		{
			base.Dispose();
		}
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteConnection).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.TraceWarning) && _noDispose)
		{
			System.Diagnostics.Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "WARNING: Disposing of connection \"{0}\" with the no-dispose flag set.", _connectionString));
		}
		_disposing = true;
		try
		{
			if (!disposed)
			{
				Close();
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}

	public object Clone()
	{
		CheckDisposed();
		return new SQLiteConnection(this);
	}

	public static void CreateFile(string databaseFileName)
	{
		File.Create(databaseFileName).Close();
	}

	internal void OnStateChange(ConnectionState newState, ref StateChangeEventArgs eventArgs)
	{
		ConnectionState connectionState = _connectionState;
		_connectionState = newState;
		if (StateChange != null && newState != connectionState)
		{
			StateChangeEventArgs stateChangeEventArgs = new StateChangeEventArgs(connectionState, newState);
			StateChange(this, stateChangeEventArgs);
			eventArgs = stateChangeEventArgs;
		}
	}

	private static IsolationLevel GetFallbackDefaultIsolationLevel()
	{
		return IsolationLevel.Serializable;
	}

	internal IsolationLevel GetDefaultIsolationLevel()
	{
		return _defaultIsolation;
	}

	[Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
	public SQLiteTransaction BeginTransaction(IsolationLevel isolationLevel, bool deferredLock)
	{
		CheckDisposed();
		return (SQLiteTransaction)BeginDbTransaction((!deferredLock) ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted);
	}

	[Obsolete("Use one of the standard BeginTransaction methods, this one will be removed soon")]
	public SQLiteTransaction BeginTransaction(bool deferredLock)
	{
		CheckDisposed();
		return (SQLiteTransaction)BeginDbTransaction((!deferredLock) ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted);
	}

	public new SQLiteTransaction BeginTransaction(IsolationLevel isolationLevel)
	{
		CheckDisposed();
		return (SQLiteTransaction)BeginDbTransaction(isolationLevel);
	}

	public new SQLiteTransaction BeginTransaction()
	{
		CheckDisposed();
		return (SQLiteTransaction)BeginDbTransaction(_defaultIsolation);
	}

	protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
	{
		if (_connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException();
		}
		if (isolationLevel == IsolationLevel.Unspecified)
		{
			isolationLevel = _defaultIsolation;
		}
		isolationLevel = GetEffectiveIsolationLevel(isolationLevel);
		if (isolationLevel != IsolationLevel.Serializable && isolationLevel != IsolationLevel.ReadCommitted)
		{
			throw new ArgumentException("isolationLevel");
		}
		SQLiteTransaction sQLiteTransaction = ((!HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.AllowNestedTransactions)) ? new SQLiteTransaction(this, isolationLevel != IsolationLevel.Serializable) : new SQLiteTransaction2(this, isolationLevel != IsolationLevel.Serializable));
		OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.NewTransaction, null, sQLiteTransaction, null, null, null, null, null));
		return sQLiteTransaction;
	}

	public override void ChangeDatabase(string databaseName)
	{
		CheckDisposed();
		OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.ChangeDatabase, null, null, null, null, null, databaseName, null));
		throw new NotImplementedException();
	}

	public override void Close()
	{
		CheckDisposed();
		OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Closing, null, null, null, null, null, null, null));
		if (_sql != null)
		{
			lock (_enlistmentSyncRoot)
			{
				SQLiteEnlistment enlistment = _enlistment;
				_enlistment = null;
				if (enlistment != null)
				{
					SQLiteConnection sQLiteConnection = new SQLiteConnection();
					sQLiteConnection._sql = _sql;
					sQLiteConnection._transactionLevel = _transactionLevel;
					sQLiteConnection._transactionSequence = _transactionSequence;
					sQLiteConnection._enlistment = enlistment;
					sQLiteConnection._connectionState = _connectionState;
					sQLiteConnection._version = _version;
					SQLiteTransaction transaction = enlistment._transaction;
					if (transaction != null)
					{
						transaction._cnn = sQLiteConnection;
					}
					enlistment._disposeConnection = true;
					_sql = null;
				}
			}
			if (_sql != null)
			{
				_sql.Close(_disposing);
				_sql = null;
			}
			_transactionLevel = 0;
			_transactionSequence = 0;
		}
		StateChangeEventArgs eventArgs = null;
		OnStateChange(ConnectionState.Closed, ref eventArgs);
		OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Closed, eventArgs, null, null, null, null, null, null));
	}

	public static void ClearPool(SQLiteConnection connection)
	{
		if (connection._sql != null)
		{
			connection._sql.ClearPool();
		}
	}

	public static void ClearAllPools()
	{
		SQLiteConnectionPool.ClearAllPools();
	}

	public new SQLiteCommand CreateCommand()
	{
		CheckDisposed();
		return new SQLiteCommand(this);
	}

	protected override DbCommand CreateDbCommand()
	{
		return CreateCommand();
	}

	public ISQLiteSession CreateSession(string databaseName)
	{
		CheckDisposed();
		return new SQLiteSession(GetNativeHandle(this), _flags, databaseName);
	}

	public ISQLiteChangeSet CreateChangeSet(byte[] rawData)
	{
		CheckDisposed();
		return new SQLiteMemoryChangeSet(rawData, GetNativeHandle(this), _flags);
	}

	public ISQLiteChangeSet CreateChangeSet(byte[] rawData, SQLiteChangeSetStartFlags flags)
	{
		CheckDisposed();
		return new SQLiteMemoryChangeSet(rawData, GetNativeHandle(this), _flags, flags);
	}

	public ISQLiteChangeSet CreateChangeSet(Stream inputStream, Stream outputStream)
	{
		CheckDisposed();
		return new SQLiteStreamChangeSet(inputStream, outputStream, GetNativeHandle(this), _flags);
	}

	public ISQLiteChangeSet CreateChangeSet(Stream inputStream, Stream outputStream, SQLiteChangeSetStartFlags flags)
	{
		CheckDisposed();
		return new SQLiteStreamChangeSet(inputStream, outputStream, GetNativeHandle(this), _flags, flags);
	}

	public ISQLiteChangeGroup CreateChangeGroup()
	{
		CheckDisposed();
		return new SQLiteChangeGroup(_flags);
	}

	internal static string MapUriPath(string path)
	{
		if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
		{
			return path.Substring(7);
		}
		if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
		{
			return path.Substring(5);
		}
		if (path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		throw new InvalidOperationException("Invalid connection string: invalid URI");
	}

	private static bool ShouldUseLegacyConnectionStringParser(SQLiteConnection connection)
	{
		string name = "No_SQLiteConnectionNewParser";
		if (connection != null && connection.TryGetCachedSetting(name, null, out var value))
		{
			return value != null;
		}
		if (connection == null && TryGetLastCachedSetting(name, null, out value))
		{
			return value != null;
		}
		value = UnsafeNativeMethods.GetSettingValue(name, null);
		if (connection != null)
		{
			connection.SetCachedSetting(name, value);
		}
		else
		{
			SetLastCachedSetting(name, value);
		}
		return value != null;
	}

	private static SortedList<string, string> ParseConnectionString(string connectionString, bool allowNameOnly)
	{
		return ParseConnectionString(null, connectionString, allowNameOnly);
	}

	private static SortedList<string, string> ParseConnectionString(SQLiteConnection connection, string connectionString, bool allowNameOnly)
	{
		SortedList<string, string> sortedList = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
		string error = null;
		string[] array = ((!ShouldUseLegacyConnectionStringParser(connection)) ? SQLiteConvert.NewSplit(connectionString, ';', keepQuote: true, ref error) : SQLiteConvert.Split(connectionString, ';'));
		if (array == null)
		{
			throw new ArgumentException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Invalid ConnectionString format, cannot parse: {0}", (error != null) ? error : "could not split connection string into properties"));
		}
		int num = ((array != null) ? array.Length : 0);
		for (int i = 0; i < num; i++)
		{
			if (array[i] == null)
			{
				continue;
			}
			array[i] = array[i].Trim();
			if (array[i].Length == 0)
			{
				continue;
			}
			int num2 = array[i].IndexOf('=');
			if (num2 != -1)
			{
				sortedList.Add(UnwrapString(array[i].Substring(0, num2).Trim()), UnwrapString(array[i].Substring(num2 + 1).Trim()));
				continue;
			}
			if (allowNameOnly)
			{
				sortedList.Add(UnwrapString(array[i].Trim()), string.Empty);
				continue;
			}
			throw new ArgumentException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Invalid ConnectionString format for part \"{0}\", no equal sign found", array[i]));
		}
		return sortedList;
	}

	private static SortedList<string, string> ParseConnectionStringViaFramework(SQLiteConnection connection, string connectionString, bool strict)
	{
		DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
		dbConnectionStringBuilder.ConnectionString = connectionString;
		SortedList<string, string> sortedList = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string key in dbConnectionStringBuilder.Keys)
		{
			object obj = dbConnectionStringBuilder[key];
			string value = null;
			if (obj is string)
			{
				value = (string)obj;
			}
			else
			{
				if (strict)
				{
					throw new ArgumentException("connection property value is not a string", key);
				}
				if (obj != null)
				{
					value = obj.ToString();
				}
			}
			sortedList.Add(key, value);
		}
		return sortedList;
	}

	public override void EnlistTransaction(Transaction transaction)
	{
		CheckDisposed();
		bool flag;
		int waitTimeout;
		lock (_enlistmentSyncRoot)
		{
			flag = HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.WaitForEnlistmentReset);
			waitTimeout = _waitTimeout;
		}
		if (flag)
		{
			WaitForEnlistmentReset(waitTimeout, null);
		}
		lock (_enlistmentSyncRoot)
		{
			if (_enlistment == null || !(transaction == _enlistment._scope))
			{
				if (_enlistment != null)
				{
					throw new ArgumentException("Already enlisted in a transaction");
				}
				if (_transactionLevel > 0 && transaction != null)
				{
					throw new ArgumentException("Unable to enlist in transaction, a local transaction already exists");
				}
				if (transaction == null)
				{
					throw new ArgumentNullException("Unable to enlist in transaction, it is null");
				}
				bool flag2 = HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.StrictEnlistment);
				_enlistment = new SQLiteEnlistment(this, transaction, GetFallbackDefaultIsolationLevel(), flag2, flag2);
				OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.EnlistTransaction, null, null, null, null, null, null, new object[1] { _enlistment }));
			}
		}
	}

	public bool WaitForEnlistmentReset(int timeoutMilliseconds, bool? returnOnDisposed)
	{
		if (!returnOnDisposed.HasValue)
		{
			CheckDisposed();
		}
		else if (disposed)
		{
			return returnOnDisposed.Value;
		}
		if (timeoutMilliseconds < 0)
		{
			throw new ArgumentException("timeout cannot be negative");
		}
		int num;
		if (timeoutMilliseconds == 0)
		{
			num = 0;
		}
		else
		{
			num = Math.Min(timeoutMilliseconds / 10, 100);
			if (num == 0)
			{
				num = 100;
			}
		}
		DateTime utcNow = DateTime.UtcNow;
		while (true)
		{
			bool flag = Monitor.TryEnter(_enlistmentSyncRoot);
			try
			{
				if (flag && _enlistment == null)
				{
					return true;
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(_enlistmentSyncRoot);
					flag = false;
				}
			}
			if (num == 0)
			{
				return false;
			}
			double totalMilliseconds = DateTime.UtcNow.Subtract(utcNow).TotalMilliseconds;
			if (totalMilliseconds < 0.0 || totalMilliseconds >= (double)timeoutMilliseconds)
			{
				break;
			}
			Thread.Sleep(num);
		}
		return false;
	}

	internal static string FindKey(SortedList<string, string> items, string key, string defValue)
	{
		if (string.IsNullOrEmpty(key))
		{
			return defValue;
		}
		if (items.TryGetValue(key, out var value))
		{
			return value;
		}
		if (items.TryGetValue(key.Replace(" ", string.Empty), out value))
		{
			return value;
		}
		if (items.TryGetValue(key.Replace(" ", "_"), out value))
		{
			return value;
		}
		return defValue;
	}

	internal static object TryParseEnum(Type type, string value, bool ignoreCase)
	{
		if (!string.IsNullOrEmpty(value))
		{
			try
			{
				return Enum.Parse(type, value, ignoreCase);
			}
			catch
			{
			}
		}
		return null;
	}

	private static bool TryParseByte(string value, NumberStyles style, out byte result)
	{
		return byte.TryParse(value, style, null, out result);
	}

	public int SetLimitOption(SQLiteLimitOpsEnum option, int value)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for changing a limit option.");
		}
		return _sql.SetLimitOption(option, value);
	}

	public void SetConfigurationOption(SQLiteConfigDbOpsEnum option, object value)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for changing a configuration option.");
		}
		if (option == SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION && HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoLoadExtension))
		{
			throw new SQLiteException("Loading extensions is disabled for this database connection.");
		}
		SQLiteErrorCode sQLiteErrorCode = _sql.SetConfigurationOption(option, value);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, null);
		}
	}

	public void EnableExtensions(bool enable)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Database connection not valid for {0} extensions.", enable ? "enabling" : "disabling"));
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoLoadExtension))
		{
			throw new SQLiteException("Loading extensions is disabled for this database connection.");
		}
		_sql.SetLoadExtension(enable);
	}

	public void LoadExtension(string fileName)
	{
		CheckDisposed();
		LoadExtension(fileName, null);
	}

	public void LoadExtension(string fileName, string procName)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for loading extensions.");
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoLoadExtension))
		{
			throw new SQLiteException("Loading extensions is disabled for this database connection.");
		}
		_sql.LoadExtension(fileName, procName);
	}

	public void CreateModule(SQLiteModule module)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for creating modules.");
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoCreateModule))
		{
			throw new SQLiteException("Creating modules is disabled for this database connection.");
		}
		_sql.CreateModule(module, _flags);
	}

	internal static byte[] FromHexString(string text)
	{
		string error = null;
		return FromHexString(text, ref error);
	}

	internal static string ToHexString(byte[] array)
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.AppendFormat("{0:x2}", array[i]);
		}
		return stringBuilder.ToString();
	}

	private static byte[] FromHexString(string text, ref string error)
	{
		if (text == null)
		{
			error = "string is null";
			return null;
		}
		if (text.Length % 2 != 0)
		{
			error = "string contains an odd number of characters";
			return null;
		}
		byte[] array = new byte[text.Length / 2];
		for (int i = 0; i < text.Length; i += 2)
		{
			string text2 = text.Substring(i, 2);
			if (!TryParseByte(text2, NumberStyles.HexNumber, out array[i / 2]))
			{
				error = HelperMethods.StringFormat(CultureInfo.CurrentCulture, "string contains \"{0}\", which cannot be converted to a byte value", text2);
				return null;
			}
		}
		return array;
	}

	private bool GetDefaultPooling()
	{
		bool flag = false;
		if (flag)
		{
			if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoConnectionPool))
			{
				flag = false;
			}
			if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionPool))
			{
				flag = true;
			}
		}
		else
		{
			if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UseConnectionPool))
			{
				flag = true;
			}
			if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.NoConnectionPool))
			{
				flag = false;
			}
		}
		return flag;
	}

	private IsolationLevel GetEffectiveIsolationLevel(IsolationLevel isolationLevel)
	{
		if (!HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.MapIsolationLevels))
		{
			return isolationLevel;
		}
		switch (isolationLevel)
		{
		case IsolationLevel.Unspecified:
		case IsolationLevel.Chaos:
		case IsolationLevel.ReadUncommitted:
		case IsolationLevel.ReadCommitted:
			return IsolationLevel.ReadCommitted;
		case IsolationLevel.RepeatableRead:
		case IsolationLevel.Serializable:
		case IsolationLevel.Snapshot:
			return IsolationLevel.Serializable;
		default:
			return GetFallbackDefaultIsolationLevel();
		}
	}

	public override void Open()
	{
		CheckDisposed();
		_lastConnectionInOpen = this;
		OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Opening, null, null, null, null, null, null, null));
		if (_connectionState != 0)
		{
			throw new InvalidOperationException();
		}
		Close();
		SortedList<string, string> sortedList = ParseConnectionString(this, _connectionString, _parseViaFramework, allowNameOnly: false);
		string text = FindKey(sortedList, "Flags", null);
		object obj = ((text == null) ? null : TryParseEnum(typeof(SQLiteConnectionFlags), text, ignoreCase: true));
		bool flag = SQLiteConvert.ToBoolean(FindKey(sortedList, "NoDefaultFlags", false.ToString()));
		if (obj is SQLiteConnectionFlags)
		{
			_flags |= (SQLiteConnectionFlags)obj;
		}
		else if (!flag)
		{
			_flags |= DefaultFlags;
		}
		if (!SQLiteConvert.ToBoolean(FindKey(sortedList, "NoSharedFlags", false.ToString())))
		{
			lock (_syncRoot)
			{
				_flags |= _sharedFlags;
			}
		}
		bool flag2 = HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.HidePassword);
		SortedList<string, string> sortedList2 = sortedList;
		string text2 = _connectionString;
		if (flag2)
		{
			sortedList2 = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, string> item in sortedList)
			{
				if (!string.Equals(item.Key, "Password", StringComparison.OrdinalIgnoreCase) && !string.Equals(item.Key, "HexPassword", StringComparison.OrdinalIgnoreCase) && !string.Equals(item.Key, "TextPassword", StringComparison.OrdinalIgnoreCase))
				{
					sortedList2.Add(item.Key, item.Value);
				}
			}
			text2 = BuildConnectionString(sortedList2);
		}
		OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.ConnectionString, null, null, null, null, null, text2, new object[1] { sortedList2 }));
		text = FindKey(sortedList, "DefaultDbType", null);
		if (text != null)
		{
			obj = TryParseEnum(typeof(DbType), text, ignoreCase: true);
			_defaultDbType = ((obj is DbType) ? new DbType?((DbType)obj) : null);
		}
		if (_defaultDbType.HasValue && _defaultDbType.Value == (DbType)(-1))
		{
			_defaultDbType = null;
		}
		text = FindKey(sortedList, "DefaultTypeName", null);
		if (text != null)
		{
			_defaultTypeName = text;
		}
		text = FindKey(sortedList, "VfsName", null);
		if (text != null)
		{
			_vfsName = text;
		}
		bool flag3 = false;
		bool flag4 = false;
		if (Convert.ToInt32(FindKey(sortedList, "Version", SQLiteConvert.ToString(3)), CultureInfo.InvariantCulture) != 3)
		{
			throw new NotSupportedException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Only SQLite Version {0} is supported at this time", 3));
		}
		string text3 = FindKey(sortedList, "Data Source", null);
		if (string.IsNullOrEmpty(text3))
		{
			text3 = FindKey(sortedList, "Uri", null);
			if (string.IsNullOrEmpty(text3))
			{
				text3 = FindKey(sortedList, "FullUri", null);
				if (string.IsNullOrEmpty(text3))
				{
					throw new ArgumentException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Data Source cannot be empty.  Use {0} to open an in-memory database", ":memory:"));
				}
				flag4 = true;
			}
			else
			{
				text3 = MapUriPath(text3);
				flag3 = true;
			}
		}
		bool flag5 = string.Compare(text3, ":memory:", StringComparison.OrdinalIgnoreCase) == 0;
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.TraceWarning) && !flag3 && !flag4 && !flag5 && !string.IsNullOrEmpty(text3) && text3.StartsWith("\\", StringComparison.OrdinalIgnoreCase) && !text3.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
		{
			System.Diagnostics.Trace.WriteLine(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "WARNING: Detected a possibly malformed UNC database file name \"{0}\" that may have originally started with two backslashes; however, four leading backslashes may be required, e.g.: \"Data Source=\\\\\\{0};\"", text3));
		}
		if (!flag4)
		{
			if (flag5)
			{
				text3 = ":memory:";
			}
			else
			{
				bool toFullPath = SQLiteConvert.ToBoolean(FindKey(sortedList, "ToFullPath", true.ToString()));
				text3 = ExpandFileName(text3, toFullPath);
			}
		}
		try
		{
			bool usePool = SQLiteConvert.ToBoolean(FindKey(sortedList, "Pooling", GetDefaultPooling().ToString()));
			int maxPoolSize = Convert.ToInt32(FindKey(sortedList, "Max Pool Size", SQLiteConvert.ToString(100)), CultureInfo.InvariantCulture);
			text = FindKey(sortedList, "Default Timeout", null);
			if (text != null)
			{
				_defaultTimeout = Convert.ToInt32(text, CultureInfo.InvariantCulture);
			}
			text = FindKey(sortedList, "BusyTimeout", null);
			if (text != null)
			{
				_busyTimeout = Convert.ToInt32(text, CultureInfo.InvariantCulture);
			}
			text = FindKey(sortedList, "WaitTimeout", null);
			if (text != null)
			{
				_waitTimeout = Convert.ToInt32(text, CultureInfo.InvariantCulture);
			}
			text = FindKey(sortedList, "PrepareRetries", null);
			if (text != null)
			{
				_prepareRetries = Convert.ToInt32(text, CultureInfo.InvariantCulture);
			}
			text = FindKey(sortedList, "ProgressOps", null);
			if (text != null)
			{
				_progressOps = Convert.ToInt32(text, CultureInfo.InvariantCulture);
			}
			text = FindKey(sortedList, "Default IsolationLevel", null);
			if (text != null)
			{
				obj = TryParseEnum(typeof(IsolationLevel), text, ignoreCase: true);
				_defaultIsolation = ((obj is IsolationLevel) ? ((IsolationLevel)obj) : IsolationLevel.Serializable);
			}
			IsolationLevel effectiveIsolationLevel = GetEffectiveIsolationLevel(_defaultIsolation);
			if (effectiveIsolationLevel != IsolationLevel.Serializable && effectiveIsolationLevel != IsolationLevel.ReadCommitted)
			{
				throw new NotSupportedException("Invalid Default IsolationLevel specified");
			}
			text = FindKey(sortedList, "BaseSchemaName", null);
			if (text != null)
			{
				_baseSchemaName = text;
			}
			if (_sql == null)
			{
				SetupSQLiteBase(sortedList);
			}
			SQLiteOpenFlagsEnum sQLiteOpenFlagsEnum = SQLiteOpenFlagsEnum.None;
			if (!SQLiteConvert.ToBoolean(FindKey(sortedList, "FailIfMissing", false.ToString())))
			{
				sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.Create;
			}
			if (SQLiteConvert.ToBoolean(FindKey(sortedList, "Read Only", false.ToString())))
			{
				sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.ReadOnly;
				sQLiteOpenFlagsEnum &= ~SQLiteOpenFlagsEnum.Create;
			}
			else
			{
				sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.ReadWrite;
			}
			if (flag4)
			{
				sQLiteOpenFlagsEnum |= SQLiteOpenFlagsEnum.Uri;
			}
			_sql.Open(text3, _vfsName, _flags, sQLiteOpenFlagsEnum, maxPoolSize, usePool);
			text = FindKey(sortedList, "BinaryGUID", null);
			if (text != null)
			{
				_binaryGuid = SQLiteConvert.ToBoolean(text);
			}
			string text4 = FindKey(sortedList, "TextPassword", null);
			if (text4 != null)
			{
				byte[] array = Encoding.UTF8.GetBytes(text4);
				Array.Resize(ref array, array.Length + 1);
				_sql.SetPassword(array, asText: true);
				_passwordWasText = true;
			}
			else
			{
				string text5 = FindKey(sortedList, "HexPassword", null);
				if (text5 != null)
				{
					string error = null;
					byte[] array2 = FromHexString(text5, ref error);
					if (array2 == null)
					{
						throw new FormatException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Cannot parse 'HexPassword' property value into byte values: {0}", error));
					}
					_sql.SetPassword(array2, asText: false);
					_passwordWasText = false;
				}
				else
				{
					string text6 = FindKey(sortedList, "Password", null);
					if (text6 != null)
					{
						byte[] bytes = Encoding.UTF8.GetBytes(text6);
						_sql.SetPassword(bytes, asText: false);
						_passwordWasText = false;
					}
					else if (_password != null)
					{
						_sql.SetPassword(_password, _passwordWasText);
					}
					text6 = null;
				}
				text5 = null;
			}
			text4 = null;
			_password = null;
			if (flag2)
			{
				if (sortedList.ContainsKey("TextPassword"))
				{
					sortedList["TextPassword"] = string.Empty;
				}
				if (sortedList.ContainsKey("HexPassword"))
				{
					sortedList["HexPassword"] = string.Empty;
				}
				if (sortedList.ContainsKey("Password"))
				{
					sortedList["Password"] = string.Empty;
				}
				_connectionString = BuildConnectionString(sortedList);
			}
			if (!flag4)
			{
				_dataSource = Path.GetFileNameWithoutExtension(text3);
			}
			else
			{
				_dataSource = text3;
			}
			_version++;
			ConnectionState connectionState = _connectionState;
			_connectionState = ConnectionState.Open;
			try
			{
				text = FindKey(sortedList, "SetDefaults", null);
				if (text == null || SQLiteConvert.ToBoolean(text))
				{
					using SQLiteCommand sQLiteCommand = CreateCommand();
					if (_busyTimeout != 0)
					{
						sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA busy_timeout={0}", _busyTimeout);
						sQLiteCommand.ExecuteNonQuery();
					}
					if (!flag4 && !flag5)
					{
						text = FindKey(sortedList, "Page Size", null);
						if (text != null)
						{
							int num = Convert.ToInt32(text, CultureInfo.InvariantCulture);
							if (num != 4096)
							{
								sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA page_size={0}", num);
								sQLiteCommand.ExecuteNonQuery();
							}
						}
					}
					text = FindKey(sortedList, "Max Page Count", null);
					if (text != null)
					{
						int num = Convert.ToInt32(text, CultureInfo.InvariantCulture);
						if (num != 0)
						{
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA max_page_count={0}", num);
							sQLiteCommand.ExecuteNonQuery();
						}
					}
					text = FindKey(sortedList, "Legacy Format", null);
					if (text != null)
					{
						bool flag6 = SQLiteConvert.ToBoolean(text);
						if (flag6)
						{
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA legacy_file_format={0}", flag6 ? "ON" : "OFF");
							sQLiteCommand.ExecuteNonQuery();
						}
					}
					text = FindKey(sortedList, "Synchronous", null);
					if (text != null)
					{
						obj = TryParseEnum(typeof(SQLiteSynchronousEnum), text, ignoreCase: true);
						if (!(obj is SQLiteSynchronousEnum) || (SQLiteSynchronousEnum)obj != SQLiteSynchronousEnum.Default)
						{
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA synchronous={0}", text);
							sQLiteCommand.ExecuteNonQuery();
						}
					}
					text = FindKey(sortedList, "Cache Size", null);
					if (text != null)
					{
						int num = Convert.ToInt32(text, CultureInfo.InvariantCulture);
						if (num != -2000)
						{
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA cache_size={0}", num);
							sQLiteCommand.ExecuteNonQuery();
						}
					}
					text = FindKey(sortedList, "Journal Mode", null);
					if (text != null)
					{
						obj = TryParseEnum(typeof(SQLiteJournalModeEnum), text, ignoreCase: true);
						if (!(obj is SQLiteJournalModeEnum) || (SQLiteJournalModeEnum)obj != SQLiteJournalModeEnum.Default)
						{
							string format = "PRAGMA journal_mode={0}";
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, format, text);
							sQLiteCommand.ExecuteNonQuery();
						}
					}
					text = FindKey(sortedList, "Foreign Keys", null);
					if (text != null)
					{
						bool flag6 = SQLiteConvert.ToBoolean(text);
						if (flag6)
						{
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA foreign_keys={0}", flag6 ? "ON" : "OFF");
							sQLiteCommand.ExecuteNonQuery();
						}
					}
					text = FindKey(sortedList, "Recursive Triggers", null);
					if (text != null)
					{
						bool flag6 = SQLiteConvert.ToBoolean(text);
						if (flag6)
						{
							sQLiteCommand.CommandText = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA recursive_triggers={0}", flag6 ? "ON" : "OFF");
							sQLiteCommand.ExecuteNonQuery();
						}
					}
				}
				if (this._busyHandler != null)
				{
					_sql.SetBusyHook(_busyCallback);
				}
				if (this._progressHandler != null)
				{
					_sql.SetProgressHook(_progressOps, _progressCallback);
				}
				if (this._authorizerHandler != null)
				{
					_sql.SetAuthorizerHook(_authorizerCallback);
				}
				if (this._commitHandler != null)
				{
					_sql.SetCommitHook(_commitCallback);
				}
				if (this._updateHandler != null)
				{
					_sql.SetUpdateHook(_updateCallback);
				}
				if (this._rollbackHandler != null)
				{
					_sql.SetRollbackHook(_rollbackCallback);
				}
				Transaction current2 = Transaction.Current;
				if (current2 != null && SQLiteConvert.ToBoolean(FindKey(sortedList, "Enlist", true.ToString())))
				{
					EnlistTransaction(current2);
				}
				_connectionState = connectionState;
				StateChangeEventArgs eventArgs = null;
				OnStateChange(ConnectionState.Open, ref eventArgs);
				OnChanged(this, new ConnectionEventArgs(SQLiteConnectionEventType.Opened, eventArgs, null, null, null, null, text2, new object[1] { sortedList2 }));
			}
			catch
			{
				_connectionState = connectionState;
				throw;
			}
		}
		catch (Exception)
		{
			Close();
			throw;
		}
	}

	public SQLiteConnection OpenAndReturn()
	{
		CheckDisposed();
		Open();
		return this;
	}

	public void Cancel()
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for query cancellation.");
		}
		_sql.Cancel();
	}

	public bool IsReadOnly(string name)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for checking read-only status.");
		}
		return _sql.IsReadOnly(name);
	}

	public static void GetMemoryStatistics(ref IDictionary<string, long> statistics)
	{
		if (statistics == null)
		{
			statistics = new Dictionary<string, long>();
		}
		statistics["MemoryUsed"] = SQLite3.StaticMemoryUsed;
		statistics["MemoryHighwater"] = SQLite3.StaticMemoryHighwater;
	}

	public void ReleaseMemory()
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for releasing memory.");
		}
		SQLiteErrorCode sQLiteErrorCode = _sql.ReleaseMemory();
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, _sql.GetLastError("Could not release connection memory."));
		}
	}

	public static SQLiteErrorCode ReleaseMemory(int nBytes, bool reset, bool compact, ref int nFree, ref bool resetOk, ref uint nLargest)
	{
		return SQLite3.StaticReleaseMemory(nBytes, reset, compact, ref nFree, ref resetOk, ref nLargest);
	}

	public static SQLiteErrorCode SetMemoryStatus(bool value)
	{
		return SQLite3.StaticSetMemoryStatus(value);
	}

	private static bool TryGetLastCachedSetting(string name, object @default, out object value)
	{
		if (_lastConnectionInOpen == null)
		{
			value = @default;
			return false;
		}
		return _lastConnectionInOpen.TryGetCachedSetting(name, @default, out value);
	}

	private static void SetLastCachedSetting(string name, object value)
	{
		if (_lastConnectionInOpen != null)
		{
			_lastConnectionInOpen.SetCachedSetting(name, value);
		}
	}

	public SQLiteErrorCode Shutdown()
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for shutdown.");
		}
		_sql.Close(disposing: false);
		return _sql.Shutdown();
	}

	public static void Shutdown(bool directories, bool noThrow)
	{
		SQLiteErrorCode sQLiteErrorCode = SQLite3.StaticShutdown(directories);
		if (sQLiteErrorCode != 0 && !noThrow)
		{
			throw new SQLiteException(sQLiteErrorCode, null);
		}
	}

	public void SetExtendedResultCodes(bool bOnOff)
	{
		CheckDisposed();
		if (_sql != null)
		{
			_sql.SetExtendedResultCodes(bOnOff);
		}
	}

	public SQLiteErrorCode ResultCode()
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for getting result code.");
		}
		return _sql.ResultCode();
	}

	public SQLiteErrorCode ExtendedResultCode()
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for getting extended result code.");
		}
		return _sql.ExtendedResultCode();
	}

	public void LogMessage(SQLiteErrorCode iErrCode, string zMessage)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for logging message.");
		}
		_sql.LogMessage(iErrCode, zMessage);
	}

	public void LogMessage(int iErrCode, string zMessage)
	{
		CheckDisposed();
		if (_sql == null)
		{
			throw new InvalidOperationException("Database connection not valid for logging message.");
		}
		_sql.LogMessage((SQLiteErrorCode)iErrCode, zMessage);
	}

	public static string DecryptLegacyDatabase(string fileName, byte[] passwordBytes, int? pageSize, SQLiteProgressEventHandler progress)
	{
		return SQLite3.DecryptLegacyDatabase(fileName, passwordBytes, pageSize, progress);
	}

	public void ChangePassword(string newPassword)
	{
		CheckDisposed();
		if (!string.IsNullOrEmpty(newPassword))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(newPassword);
			ChangePassword(bytes);
		}
		else
		{
			ChangePassword((byte[])null);
		}
	}

	public void ChangePassword(byte[] newPassword)
	{
		CheckDisposed();
		if (_connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException("Database must be opened before changing the password.");
		}
		_sql.ChangePassword(newPassword, _passwordWasText);
	}

	public void SetPassword(string databasePassword)
	{
		CheckDisposed();
		if (!string.IsNullOrEmpty(databasePassword))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(databasePassword);
			SetPassword(bytes);
		}
		else
		{
			SetPassword((byte[])null);
		}
	}

	public void SetPassword(byte[] databasePassword)
	{
		CheckDisposed();
		if (_connectionState != 0)
		{
			throw new InvalidOperationException("Password can only be set before the database is opened.");
		}
		if (databasePassword != null && databasePassword.Length == 0)
		{
			databasePassword = null;
		}
		if (databasePassword != null && HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.HidePassword))
		{
			throw new InvalidOperationException("With 'HidePassword' enabled, passwords can only be set via the connection string.");
		}
		_password = databasePassword;
	}

	public SQLiteErrorCode SetAvRetry(ref int count, ref int interval)
	{
		CheckDisposed();
		if (_connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException("Database must be opened before changing the AV retry parameters.");
		}
		IntPtr intPtr = IntPtr.Zero;
		SQLiteErrorCode sQLiteErrorCode;
		try
		{
			intPtr = Marshal.AllocHGlobal(8);
			Marshal.WriteInt32(intPtr, 0, count);
			Marshal.WriteInt32(intPtr, 4, interval);
			sQLiteErrorCode = _sql.FileControl(null, 9, intPtr);
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				count = Marshal.ReadInt32(intPtr, 0);
				interval = Marshal.ReadInt32(intPtr, 4);
			}
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
		return sQLiteErrorCode;
	}

	public SQLiteErrorCode SetChunkSize(int size)
	{
		CheckDisposed();
		if (_connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException("Database must be opened before changing the chunk size.");
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.AllocHGlobal(4);
			Marshal.WriteInt32(intPtr, 0, size);
			return _sql.FileControl(null, 6, intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	private static string UnwrapString(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		int length = value.Length;
		if (value[0] == '"' && value[length - 1] == '"')
		{
			return value.Substring(1, length - 2);
		}
		if (value[0] == '\'' && value[length - 1] == '\'')
		{
			return value.Substring(1, length - 2);
		}
		return value;
	}

	private static string GetDataDirectory()
	{
		string text = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
		if (string.IsNullOrEmpty(text))
		{
			text = AppDomain.CurrentDomain.BaseDirectory;
		}
		return text;
	}

	private static string ExpandFileName(string sourceFile, bool toFullPath)
	{
		if (string.IsNullOrEmpty(sourceFile))
		{
			return sourceFile;
		}
		if (sourceFile.StartsWith("|DataDirectory|", StringComparison.OrdinalIgnoreCase))
		{
			string dataDirectory = GetDataDirectory();
			if (sourceFile.Length > "|DataDirectory|".Length && (sourceFile["|DataDirectory|".Length] == Path.DirectorySeparatorChar || sourceFile["|DataDirectory|".Length] == Path.AltDirectorySeparatorChar))
			{
				sourceFile = sourceFile.Remove("|DataDirectory|".Length, 1);
			}
			sourceFile = Path.Combine(dataDirectory, sourceFile.Substring("|DataDirectory|".Length));
		}
		if (toFullPath)
		{
			sourceFile = Path.GetFullPath(sourceFile);
		}
		return sourceFile;
	}

	public override DataTable GetSchema()
	{
		CheckDisposed();
		return GetSchema("MetaDataCollections", null);
	}

	public override DataTable GetSchema(string collectionName)
	{
		CheckDisposed();
		return GetSchema(collectionName, new string[0]);
	}

	public override DataTable GetSchema(string collectionName, string[] restrictionValues)
	{
		CheckDisposed();
		if (_connectionState != ConnectionState.Open)
		{
			throw new InvalidOperationException();
		}
		string[] array = new string[5];
		if (restrictionValues == null)
		{
			restrictionValues = new string[0];
		}
		restrictionValues.CopyTo(array, 0);
		switch (collectionName.ToUpper(CultureInfo.InvariantCulture))
		{
		case "METADATACOLLECTIONS":
			return Schema_MetaDataCollections();
		case "DATASOURCEINFORMATION":
			return Schema_DataSourceInformation();
		case "DATATYPES":
			return Schema_DataTypes();
		case "COLUMNS":
		case "TABLECOLUMNS":
			return Schema_Columns(array[0], array[2], array[3]);
		case "INDEXES":
			return Schema_Indexes(array[0], array[2], array[3]);
		case "TRIGGERS":
			return Schema_Triggers(array[0], array[2], array[3]);
		case "INDEXCOLUMNS":
			return Schema_IndexColumns(array[0], array[2], array[3], array[4]);
		case "TABLES":
			return Schema_Tables(array[0], array[2], array[3]);
		case "VIEWS":
			return Schema_Views(array[0], array[2]);
		case "VIEWCOLUMNS":
			return Schema_ViewColumns(array[0], array[2], array[3]);
		case "FOREIGNKEYS":
			return Schema_ForeignKeys(array[0], array[2], array[3]);
		case "CATALOGS":
			return Schema_Catalogs(array[0]);
		case "RESERVEDWORDS":
			return Schema_ReservedWords();
		default:
			throw new NotSupportedException();
		}
	}

	private static DataTable Schema_ReservedWords()
	{
		DataTable dataTable = new DataTable("ReservedWords");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("ReservedWord", typeof(string));
		dataTable.Columns.Add("MaximumVersion", typeof(string));
		dataTable.Columns.Add("MinimumVersion", typeof(string));
		dataTable.BeginLoadData();
		string[] array = SR.Keywords.Split(new char[1] { ',' });
		foreach (string value in array)
		{
			DataRow dataRow = dataTable.NewRow();
			dataRow[0] = value;
			dataTable.Rows.Add(dataRow);
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private static DataTable Schema_MetaDataCollections()
	{
		DataTable dataTable = new DataTable("MetaDataCollections");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("CollectionName", typeof(string));
		dataTable.Columns.Add("NumberOfRestrictions", typeof(int));
		dataTable.Columns.Add("NumberOfIdentifierParts", typeof(int));
		dataTable.BeginLoadData();
		StringReader stringReader = new StringReader(SR.MetaDataCollections);
		dataTable.ReadXml(stringReader);
		stringReader.Close();
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_DataSourceInformation()
	{
		DataTable dataTable = new DataTable("DataSourceInformation");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add(DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductName, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersion, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.DataSourceProductVersionNormalized, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.GroupByBehavior, typeof(int));
		dataTable.Columns.Add(DbMetaDataColumnNames.IdentifierPattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.IdentifierCase, typeof(int));
		dataTable.Columns.Add(DbMetaDataColumnNames.OrderByColumnsInSelect, typeof(bool));
		dataTable.Columns.Add(DbMetaDataColumnNames.ParameterMarkerFormat, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.ParameterMarkerPattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.ParameterNameMaxLength, typeof(int));
		dataTable.Columns.Add(DbMetaDataColumnNames.ParameterNamePattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierPattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.QuotedIdentifierCase, typeof(int));
		dataTable.Columns.Add(DbMetaDataColumnNames.StatementSeparatorPattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.StringLiteralPattern, typeof(string));
		dataTable.Columns.Add(DbMetaDataColumnNames.SupportedJoinOperators, typeof(int));
		dataTable.BeginLoadData();
		DataRow dataRow = dataTable.NewRow();
		dataRow.ItemArray = new object[17]
		{
			null, "SQLite", _sql.Version, _sql.Version, 3, "(^\\[\\p{Lo}\\p{Lu}\\p{Ll}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Nd}@$#_]*$)|(^\\[[^\\]\\0]|\\]\\]+\\]$)|(^\\\"[^\\\"\\0]|\\\"\\\"+\\\"$)", 1, false, "{0}", "@[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)",
			255, "^[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)", "(([^\\[]|\\]\\])*)", 1, ";", "'(([^']|'')*)'", 15
		};
		dataTable.Rows.Add(dataRow);
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_Columns(string strCatalog, string strTable, string strColumn)
	{
		DataTable dataTable = new DataTable("Columns");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("COLUMN_NAME", typeof(string));
		dataTable.Columns.Add("COLUMN_GUID", typeof(Guid));
		dataTable.Columns.Add("COLUMN_PROPID", typeof(long));
		dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
		dataTable.Columns.Add("COLUMN_HASDEFAULT", typeof(bool));
		dataTable.Columns.Add("COLUMN_DEFAULT", typeof(string));
		dataTable.Columns.Add("COLUMN_FLAGS", typeof(long));
		dataTable.Columns.Add("IS_NULLABLE", typeof(bool));
		dataTable.Columns.Add("DATA_TYPE", typeof(string));
		dataTable.Columns.Add("TYPE_GUID", typeof(Guid));
		dataTable.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
		dataTable.Columns.Add("CHARACTER_OCTET_LENGTH", typeof(int));
		dataTable.Columns.Add("NUMERIC_PRECISION", typeof(int));
		dataTable.Columns.Add("NUMERIC_SCALE", typeof(int));
		dataTable.Columns.Add("DATETIME_PRECISION", typeof(long));
		dataTable.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
		dataTable.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
		dataTable.Columns.Add("CHARACTER_SET_NAME", typeof(string));
		dataTable.Columns.Add("COLLATION_CATALOG", typeof(string));
		dataTable.Columns.Add("COLLATION_SCHEMA", typeof(string));
		dataTable.Columns.Add("COLLATION_NAME", typeof(string));
		dataTable.Columns.Add("DOMAIN_CATALOG", typeof(string));
		dataTable.Columns.Add("DOMAIN_NAME", typeof(string));
		dataTable.Columns.Add("DESCRIPTION", typeof(string));
		dataTable.Columns.Add("PRIMARY_KEY", typeof(bool));
		dataTable.Columns.Add("EDM_TYPE", typeof(string));
		dataTable.Columns.Add("AUTOINCREMENT", typeof(bool));
		dataTable.Columns.Add("UNIQUE", typeof(bool));
		dataTable.BeginLoadData();
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table' OR [type] LIKE 'view'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				if (!string.IsNullOrEmpty(strTable) && string.Compare(strTable, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}
				try
				{
					using SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", strCatalog, sQLiteDataReader.GetString(2)), this);
					using SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader(CommandBehavior.SchemaOnly);
					using DataTable dataTable2 = sQLiteDataReader2.GetSchemaTable(wantUniqueInfo: true, wantDefaultValue: true);
					foreach (DataRow row in dataTable2.Rows)
					{
						if (string.Compare(row[SchemaTableColumn.ColumnName].ToString(), strColumn, StringComparison.OrdinalIgnoreCase) == 0 || strColumn == null)
						{
							DataRow dataRow2 = dataTable.NewRow();
							dataRow2["NUMERIC_PRECISION"] = row[SchemaTableColumn.NumericPrecision];
							dataRow2["NUMERIC_SCALE"] = row[SchemaTableColumn.NumericScale];
							dataRow2["TABLE_NAME"] = sQLiteDataReader.GetString(2);
							dataRow2["COLUMN_NAME"] = row[SchemaTableColumn.ColumnName];
							dataRow2["TABLE_CATALOG"] = strCatalog;
							dataRow2["ORDINAL_POSITION"] = row[SchemaTableColumn.ColumnOrdinal];
							dataRow2["COLUMN_HASDEFAULT"] = row[SchemaTableOptionalColumn.DefaultValue] != DBNull.Value;
							dataRow2["COLUMN_DEFAULT"] = row[SchemaTableOptionalColumn.DefaultValue];
							dataRow2["IS_NULLABLE"] = row[SchemaTableColumn.AllowDBNull];
							dataRow2["DATA_TYPE"] = row["DataTypeName"].ToString().ToLower(CultureInfo.InvariantCulture);
							dataRow2["EDM_TYPE"] = SQLiteConvert.DbTypeToTypeName(this, (DbType)row[SchemaTableColumn.ProviderType], _flags).ToString().ToLower(CultureInfo.InvariantCulture);
							dataRow2["CHARACTER_MAXIMUM_LENGTH"] = row[SchemaTableColumn.ColumnSize];
							dataRow2["TABLE_SCHEMA"] = row[SchemaTableColumn.BaseSchemaName];
							dataRow2["PRIMARY_KEY"] = row[SchemaTableColumn.IsKey];
							dataRow2["AUTOINCREMENT"] = row[SchemaTableOptionalColumn.IsAutoIncrement];
							dataRow2["COLLATION_NAME"] = row["CollationType"];
							dataRow2["UNIQUE"] = row[SchemaTableColumn.IsUnique];
							dataTable.Rows.Add(dataRow2);
						}
					}
				}
				catch (SQLiteException)
				{
				}
			}
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_Indexes(string strCatalog, string strTable, string strIndex)
	{
		DataTable dataTable = new DataTable("Indexes");
		List<int> list = new List<int>();
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("INDEX_CATALOG", typeof(string));
		dataTable.Columns.Add("INDEX_SCHEMA", typeof(string));
		dataTable.Columns.Add("INDEX_NAME", typeof(string));
		dataTable.Columns.Add("PRIMARY_KEY", typeof(bool));
		dataTable.Columns.Add("UNIQUE", typeof(bool));
		dataTable.Columns.Add("CLUSTERED", typeof(bool));
		dataTable.Columns.Add("TYPE", typeof(int));
		dataTable.Columns.Add("FILL_FACTOR", typeof(int));
		dataTable.Columns.Add("INITIAL_SIZE", typeof(int));
		dataTable.Columns.Add("NULLS", typeof(int));
		dataTable.Columns.Add("SORT_BOOKMARKS", typeof(bool));
		dataTable.Columns.Add("AUTO_UPDATE", typeof(bool));
		dataTable.Columns.Add("NULL_COLLATION", typeof(int));
		dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
		dataTable.Columns.Add("COLUMN_NAME", typeof(string));
		dataTable.Columns.Add("COLUMN_GUID", typeof(Guid));
		dataTable.Columns.Add("COLUMN_PROPID", typeof(long));
		dataTable.Columns.Add("COLLATION", typeof(short));
		dataTable.Columns.Add("CARDINALITY", typeof(decimal));
		dataTable.Columns.Add("PAGES", typeof(int));
		dataTable.Columns.Add("FILTER_CONDITION", typeof(string));
		dataTable.Columns.Add("INTEGRATED", typeof(bool));
		dataTable.Columns.Add("INDEX_DEFINITION", typeof(string));
		dataTable.BeginLoadData();
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				bool flag = false;
				list.Clear();
				if (!string.IsNullOrEmpty(strTable) && string.Compare(sQLiteDataReader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}
				try
				{
					using SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].table_info([{1}])", strCatalog, sQLiteDataReader.GetString(2)), this);
					using SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader();
					while (sQLiteDataReader2.Read())
					{
						if (sQLiteDataReader2.GetInt32(5) != 0)
						{
							list.Add(sQLiteDataReader2.GetInt32(0));
							if (string.Compare(sQLiteDataReader2.GetString(2), "INTEGER", StringComparison.OrdinalIgnoreCase) == 0)
							{
								flag = true;
							}
						}
					}
				}
				catch (SQLiteException)
				{
				}
				if (list.Count == 1 && flag)
				{
					DataRow dataRow = dataTable.NewRow();
					dataRow["TABLE_CATALOG"] = strCatalog;
					dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
					dataRow["INDEX_CATALOG"] = strCatalog;
					dataRow["PRIMARY_KEY"] = true;
					dataRow["INDEX_NAME"] = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "{1}_PK_{0}", sQLiteDataReader.GetString(2), masterTableName);
					dataRow["UNIQUE"] = true;
					if (string.Compare((string)dataRow["INDEX_NAME"], strIndex, StringComparison.OrdinalIgnoreCase) == 0 || strIndex == null)
					{
						dataTable.Rows.Add(dataRow);
					}
					list.Clear();
				}
				try
				{
					using SQLiteCommand sQLiteCommand3 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].index_list([{1}])", strCatalog, sQLiteDataReader.GetString(2)), this);
					using SQLiteDataReader sQLiteDataReader3 = sQLiteCommand3.ExecuteReader();
					while (sQLiteDataReader3.Read())
					{
						if (string.Compare(sQLiteDataReader3.GetString(1), strIndex, StringComparison.OrdinalIgnoreCase) != 0 && strIndex != null)
						{
							continue;
						}
						DataRow dataRow = dataTable.NewRow();
						dataRow["TABLE_CATALOG"] = strCatalog;
						dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
						dataRow["INDEX_CATALOG"] = strCatalog;
						dataRow["INDEX_NAME"] = sQLiteDataReader3.GetString(1);
						dataRow["UNIQUE"] = SQLiteConvert.ToBoolean(sQLiteDataReader3.GetValue(2), CultureInfo.InvariantCulture, viaFramework: false);
						dataRow["PRIMARY_KEY"] = false;
						using (SQLiteCommand sQLiteCommand4 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{2}] WHERE [type] LIKE 'index' AND [name] LIKE '{1}'", strCatalog, sQLiteDataReader3.GetString(1).Replace("'", "''"), masterTableName), this))
						{
							using SQLiteDataReader sQLiteDataReader4 = sQLiteCommand4.ExecuteReader();
							if (sQLiteDataReader4.Read() && !sQLiteDataReader4.IsDBNull(4))
							{
								dataRow["INDEX_DEFINITION"] = sQLiteDataReader4.GetString(4);
							}
						}
						if (list.Count > 0 && sQLiteDataReader3.GetString(1).StartsWith("sqlite_autoindex_" + sQLiteDataReader.GetString(2), StringComparison.InvariantCultureIgnoreCase))
						{
							using SQLiteCommand sQLiteCommand5 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].index_info([{1}])", strCatalog, sQLiteDataReader3.GetString(1)), this);
							using SQLiteDataReader sQLiteDataReader5 = sQLiteCommand5.ExecuteReader();
							int num = 0;
							while (sQLiteDataReader5.Read())
							{
								if (!list.Contains(sQLiteDataReader5.GetInt32(1)))
								{
									num = 0;
									break;
								}
								num++;
							}
							if (num == list.Count)
							{
								dataRow["PRIMARY_KEY"] = true;
								list.Clear();
							}
						}
						dataTable.Rows.Add(dataRow);
					}
				}
				catch (SQLiteException)
				{
				}
			}
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_Triggers(string catalog, string table, string triggerName)
	{
		DataTable dataTable = new DataTable("Triggers");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("TRIGGER_NAME", typeof(string));
		dataTable.Columns.Add("TRIGGER_DEFINITION", typeof(string));
		dataTable.BeginLoadData();
		if (string.IsNullOrEmpty(table))
		{
			table = null;
		}
		if (string.IsNullOrEmpty(catalog))
		{
			catalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(catalog));
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{0}].[{1}] WHERE [type] LIKE 'trigger'", catalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				if ((string.Compare(sQLiteDataReader.GetString(1), triggerName, StringComparison.OrdinalIgnoreCase) == 0 || triggerName == null) && (table == null || string.Compare(table, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) == 0))
				{
					DataRow dataRow = dataTable.NewRow();
					dataRow["TABLE_CATALOG"] = catalog;
					dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
					dataRow["TRIGGER_NAME"] = sQLiteDataReader.GetString(1);
					dataRow["TRIGGER_DEFINITION"] = sQLiteDataReader.GetString(4);
					dataTable.Rows.Add(dataRow);
				}
			}
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_Tables(string strCatalog, string strTable, string strType)
	{
		DataTable dataTable = new DataTable("Tables");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("TABLE_TYPE", typeof(string));
		dataTable.Columns.Add("TABLE_ID", typeof(long));
		dataTable.Columns.Add("TABLE_ROOTPAGE", typeof(int));
		dataTable.Columns.Add("TABLE_DEFINITION", typeof(string));
		dataTable.BeginLoadData();
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{0}].[{1}] WHERE [type] LIKE 'table'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				string text = sQLiteDataReader.GetString(0);
				if (string.Compare(sQLiteDataReader.GetString(2), 0, "SQLITE_", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
				{
					text = "SYSTEM_TABLE";
				}
				if ((string.Compare(strType, text, StringComparison.OrdinalIgnoreCase) == 0 || strType == null) && (string.Compare(sQLiteDataReader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) == 0 || strTable == null))
				{
					DataRow dataRow = dataTable.NewRow();
					dataRow["TABLE_CATALOG"] = strCatalog;
					dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
					dataRow["TABLE_TYPE"] = text;
					dataRow["TABLE_ID"] = sQLiteDataReader.GetInt64(5);
					dataRow["TABLE_ROOTPAGE"] = sQLiteDataReader.GetInt32(3);
					dataRow["TABLE_DEFINITION"] = sQLiteDataReader.GetString(4);
					dataTable.Rows.Add(dataRow);
				}
			}
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_Views(string strCatalog, string strView)
	{
		DataTable dataTable = new DataTable("Views");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("VIEW_DEFINITION", typeof(string));
		dataTable.Columns.Add("CHECK_OPTION", typeof(bool));
		dataTable.Columns.Add("IS_UPDATABLE", typeof(bool));
		dataTable.Columns.Add("DESCRIPTION", typeof(string));
		dataTable.Columns.Add("DATE_CREATED", typeof(DateTime));
		dataTable.Columns.Add("DATE_MODIFIED", typeof(DateTime));
		dataTable.BeginLoadData();
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'view'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				if (string.Compare(sQLiteDataReader.GetString(1), strView, StringComparison.OrdinalIgnoreCase) == 0 || string.IsNullOrEmpty(strView))
				{
					string text = sQLiteDataReader.GetString(4).Replace('\r', ' ').Replace('\n', ' ')
						.Replace('\t', ' ');
					int num = CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, " AS ", CompareOptions.IgnoreCase);
					if (num > -1)
					{
						text = text.Substring(num + 4).Trim();
						DataRow dataRow = dataTable.NewRow();
						dataRow["TABLE_CATALOG"] = strCatalog;
						dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
						dataRow["IS_UPDATABLE"] = false;
						dataRow["VIEW_DEFINITION"] = text;
						dataTable.Rows.Add(dataRow);
					}
				}
			}
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_Catalogs(string strCatalog)
	{
		DataTable dataTable = new DataTable("Catalogs");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("CATALOG_NAME", typeof(string));
		dataTable.Columns.Add("DESCRIPTION", typeof(string));
		dataTable.Columns.Add("ID", typeof(long));
		dataTable.BeginLoadData();
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand("PRAGMA database_list", this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				if (string.Compare(sQLiteDataReader.GetString(1), strCatalog, StringComparison.OrdinalIgnoreCase) == 0 || strCatalog == null)
				{
					DataRow dataRow = dataTable.NewRow();
					dataRow["CATALOG_NAME"] = sQLiteDataReader.GetString(1);
					dataRow["DESCRIPTION"] = sQLiteDataReader.GetString(2);
					dataRow["ID"] = sQLiteDataReader.GetInt64(0);
					dataTable.Rows.Add(dataRow);
				}
			}
		}
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_DataTypes()
	{
		DataTable dataTable = new DataTable("DataTypes");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("TypeName", typeof(string));
		dataTable.Columns.Add("ProviderDbType", typeof(int));
		dataTable.Columns.Add("ColumnSize", typeof(long));
		dataTable.Columns.Add("CreateFormat", typeof(string));
		dataTable.Columns.Add("CreateParameters", typeof(string));
		dataTable.Columns.Add("DataType", typeof(string));
		dataTable.Columns.Add("IsAutoIncrementable", typeof(bool));
		dataTable.Columns.Add("IsBestMatch", typeof(bool));
		dataTable.Columns.Add("IsCaseSensitive", typeof(bool));
		dataTable.Columns.Add("IsFixedLength", typeof(bool));
		dataTable.Columns.Add("IsFixedPrecisionScale", typeof(bool));
		dataTable.Columns.Add("IsLong", typeof(bool));
		dataTable.Columns.Add("IsNullable", typeof(bool));
		dataTable.Columns.Add("IsSearchable", typeof(bool));
		dataTable.Columns.Add("IsSearchableWithLike", typeof(bool));
		dataTable.Columns.Add("IsLiteralSupported", typeof(bool));
		dataTable.Columns.Add("LiteralPrefix", typeof(string));
		dataTable.Columns.Add("LiteralSuffix", typeof(string));
		dataTable.Columns.Add("IsUnsigned", typeof(bool));
		dataTable.Columns.Add("MaximumScale", typeof(short));
		dataTable.Columns.Add("MinimumScale", typeof(short));
		dataTable.Columns.Add("IsConcurrencyType", typeof(bool));
		dataTable.BeginLoadData();
		StringReader stringReader = new StringReader(SR.DataTypes);
		dataTable.ReadXml(stringReader);
		stringReader.Close();
		dataTable.AcceptChanges();
		dataTable.EndLoadData();
		return dataTable;
	}

	private DataTable Schema_IndexColumns(string strCatalog, string strTable, string strIndex, string strColumn)
	{
		DataTable dataTable = new DataTable("IndexColumns");
		List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
		dataTable.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
		dataTable.Columns.Add("CONSTRAINT_NAME", typeof(string));
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("COLUMN_NAME", typeof(string));
		dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
		dataTable.Columns.Add("INDEX_NAME", typeof(string));
		dataTable.Columns.Add("COLLATION_NAME", typeof(string));
		dataTable.Columns.Add("SORT_MODE", typeof(string));
		dataTable.Columns.Add("CONFLICT_OPTION", typeof(int));
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		dataTable.BeginLoadData();
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				bool flag = false;
				list.Clear();
				if (!string.IsNullOrEmpty(strTable) && string.Compare(sQLiteDataReader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}
				try
				{
					using SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].table_info([{1}])", strCatalog, sQLiteDataReader.GetString(2)), this);
					using SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader();
					while (sQLiteDataReader2.Read())
					{
						if (sQLiteDataReader2.GetInt32(5) == 1)
						{
							list.Add(new KeyValuePair<int, string>(sQLiteDataReader2.GetInt32(0), sQLiteDataReader2.GetString(1)));
							if (string.Compare(sQLiteDataReader2.GetString(2), "INTEGER", StringComparison.OrdinalIgnoreCase) == 0)
							{
								flag = true;
							}
						}
					}
				}
				catch (SQLiteException)
				{
				}
				if (list.Count == 1 && flag)
				{
					DataRow dataRow = dataTable.NewRow();
					dataRow["CONSTRAINT_CATALOG"] = strCatalog;
					dataRow["CONSTRAINT_NAME"] = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "{1}_PK_{0}", sQLiteDataReader.GetString(2), masterTableName);
					dataRow["TABLE_CATALOG"] = strCatalog;
					dataRow["TABLE_NAME"] = sQLiteDataReader.GetString(2);
					dataRow["COLUMN_NAME"] = list[0].Value;
					dataRow["INDEX_NAME"] = dataRow["CONSTRAINT_NAME"];
					dataRow["ORDINAL_POSITION"] = 0;
					dataRow["COLLATION_NAME"] = "BINARY";
					dataRow["SORT_MODE"] = "ASC";
					dataRow["CONFLICT_OPTION"] = 2;
					if (string.IsNullOrEmpty(strIndex) || string.Compare(strIndex, (string)dataRow["INDEX_NAME"], StringComparison.OrdinalIgnoreCase) == 0)
					{
						dataTable.Rows.Add(dataRow);
					}
				}
				using SQLiteCommand sQLiteCommand3 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{2}] WHERE [type] LIKE 'index' AND [tbl_name] LIKE '{1}'", strCatalog, sQLiteDataReader.GetString(2).Replace("'", "''"), masterTableName), this);
				using SQLiteDataReader sQLiteDataReader3 = sQLiteCommand3.ExecuteReader();
				while (sQLiteDataReader3.Read())
				{
					int num = 0;
					if (!string.IsNullOrEmpty(strIndex) && string.Compare(strIndex, sQLiteDataReader3.GetString(1), StringComparison.OrdinalIgnoreCase) != 0)
					{
						continue;
					}
					try
					{
						using SQLiteCommand sQLiteCommand4 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].index_info([{1}])", strCatalog, sQLiteDataReader3.GetString(1)), this);
						using SQLiteDataReader sQLiteDataReader4 = sQLiteCommand4.ExecuteReader();
						while (sQLiteDataReader4.Read())
						{
							string text = (sQLiteDataReader4.IsDBNull(2) ? null : sQLiteDataReader4.GetString(2));
							DataRow dataRow = dataTable.NewRow();
							dataRow["CONSTRAINT_CATALOG"] = strCatalog;
							dataRow["CONSTRAINT_NAME"] = sQLiteDataReader3.GetString(1);
							dataRow["TABLE_CATALOG"] = strCatalog;
							dataRow["TABLE_NAME"] = sQLiteDataReader3.GetString(2);
							dataRow["COLUMN_NAME"] = text;
							dataRow["INDEX_NAME"] = sQLiteDataReader3.GetString(1);
							dataRow["ORDINAL_POSITION"] = num;
							string collationSequence = null;
							int sortMode = 0;
							int onError = 0;
							if (text != null)
							{
								_sql.GetIndexColumnExtendedInfo(strCatalog, sQLiteDataReader3.GetString(1), text, ref sortMode, ref onError, ref collationSequence);
							}
							if (!string.IsNullOrEmpty(collationSequence))
							{
								dataRow["COLLATION_NAME"] = collationSequence;
							}
							dataRow["SORT_MODE"] = ((sortMode == 0) ? "ASC" : "DESC");
							dataRow["CONFLICT_OPTION"] = onError;
							num++;
							if (strColumn == null || string.Compare(strColumn, text, StringComparison.OrdinalIgnoreCase) == 0)
							{
								dataTable.Rows.Add(dataRow);
							}
						}
					}
					catch (SQLiteException)
					{
					}
				}
			}
		}
		dataTable.EndLoadData();
		dataTable.AcceptChanges();
		return dataTable;
	}

	private DataTable Schema_ViewColumns(string strCatalog, string strView, string strColumn)
	{
		DataTable dataTable = new DataTable("ViewColumns");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("VIEW_CATALOG", typeof(string));
		dataTable.Columns.Add("VIEW_SCHEMA", typeof(string));
		dataTable.Columns.Add("VIEW_NAME", typeof(string));
		dataTable.Columns.Add("VIEW_COLUMN_NAME", typeof(string));
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("COLUMN_NAME", typeof(string));
		dataTable.Columns.Add("ORDINAL_POSITION", typeof(int));
		dataTable.Columns.Add("COLUMN_HASDEFAULT", typeof(bool));
		dataTable.Columns.Add("COLUMN_DEFAULT", typeof(string));
		dataTable.Columns.Add("COLUMN_FLAGS", typeof(long));
		dataTable.Columns.Add("IS_NULLABLE", typeof(bool));
		dataTable.Columns.Add("DATA_TYPE", typeof(string));
		dataTable.Columns.Add("CHARACTER_MAXIMUM_LENGTH", typeof(int));
		dataTable.Columns.Add("NUMERIC_PRECISION", typeof(int));
		dataTable.Columns.Add("NUMERIC_SCALE", typeof(int));
		dataTable.Columns.Add("DATETIME_PRECISION", typeof(long));
		dataTable.Columns.Add("CHARACTER_SET_CATALOG", typeof(string));
		dataTable.Columns.Add("CHARACTER_SET_SCHEMA", typeof(string));
		dataTable.Columns.Add("CHARACTER_SET_NAME", typeof(string));
		dataTable.Columns.Add("COLLATION_CATALOG", typeof(string));
		dataTable.Columns.Add("COLLATION_SCHEMA", typeof(string));
		dataTable.Columns.Add("COLLATION_NAME", typeof(string));
		dataTable.Columns.Add("PRIMARY_KEY", typeof(bool));
		dataTable.Columns.Add("EDM_TYPE", typeof(string));
		dataTable.Columns.Add("AUTOINCREMENT", typeof(bool));
		dataTable.Columns.Add("UNIQUE", typeof(bool));
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		dataTable.BeginLoadData();
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'view'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				if (!string.IsNullOrEmpty(strView) && string.Compare(strView, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}
				using SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", strCatalog, sQLiteDataReader.GetString(2)), this);
				string text = sQLiteDataReader.GetString(4).Replace('\r', ' ').Replace('\n', ' ')
					.Replace('\t', ' ');
				int num = CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, " AS ", CompareOptions.IgnoreCase);
				if (num < 0)
				{
					continue;
				}
				text = text.Substring(num + 4);
				using SQLiteCommand sQLiteCommand3 = new SQLiteCommand(text, this);
				using SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader(CommandBehavior.SchemaOnly);
				using SQLiteDataReader sQLiteDataReader3 = sQLiteCommand3.ExecuteReader(CommandBehavior.SchemaOnly);
				using DataTable dataTable3 = sQLiteDataReader2.GetSchemaTable(wantUniqueInfo: false, wantDefaultValue: false);
				using DataTable dataTable2 = sQLiteDataReader3.GetSchemaTable(wantUniqueInfo: false, wantDefaultValue: false);
				for (num = 0; num < dataTable2.Rows.Count; num++)
				{
					DataRow dataRow = dataTable3.Rows[num];
					DataRow dataRow2 = dataTable2.Rows[num];
					if (string.Compare(dataRow[SchemaTableColumn.ColumnName].ToString(), strColumn, StringComparison.OrdinalIgnoreCase) == 0 || strColumn == null)
					{
						DataRow dataRow3 = dataTable.NewRow();
						dataRow3["VIEW_CATALOG"] = strCatalog;
						dataRow3["VIEW_NAME"] = sQLiteDataReader.GetString(2);
						dataRow3["TABLE_CATALOG"] = strCatalog;
						dataRow3["TABLE_SCHEMA"] = dataRow2[SchemaTableColumn.BaseSchemaName];
						dataRow3["TABLE_NAME"] = dataRow2[SchemaTableColumn.BaseTableName];
						dataRow3["COLUMN_NAME"] = dataRow2[SchemaTableColumn.BaseColumnName];
						dataRow3["VIEW_COLUMN_NAME"] = dataRow[SchemaTableColumn.ColumnName];
						dataRow3["COLUMN_HASDEFAULT"] = dataRow[SchemaTableOptionalColumn.DefaultValue] != DBNull.Value;
						dataRow3["COLUMN_DEFAULT"] = dataRow[SchemaTableOptionalColumn.DefaultValue];
						dataRow3["ORDINAL_POSITION"] = dataRow[SchemaTableColumn.ColumnOrdinal];
						dataRow3["IS_NULLABLE"] = dataRow[SchemaTableColumn.AllowDBNull];
						dataRow3["DATA_TYPE"] = dataRow["DataTypeName"];
						dataRow3["EDM_TYPE"] = SQLiteConvert.DbTypeToTypeName(this, (DbType)dataRow[SchemaTableColumn.ProviderType], _flags).ToString().ToLower(CultureInfo.InvariantCulture);
						dataRow3["CHARACTER_MAXIMUM_LENGTH"] = dataRow[SchemaTableColumn.ColumnSize];
						dataRow3["TABLE_SCHEMA"] = dataRow[SchemaTableColumn.BaseSchemaName];
						dataRow3["PRIMARY_KEY"] = dataRow[SchemaTableColumn.IsKey];
						dataRow3["AUTOINCREMENT"] = dataRow[SchemaTableOptionalColumn.IsAutoIncrement];
						dataRow3["COLLATION_NAME"] = dataRow["CollationType"];
						dataRow3["UNIQUE"] = dataRow[SchemaTableColumn.IsUnique];
						dataTable.Rows.Add(dataRow3);
					}
				}
			}
		}
		dataTable.EndLoadData();
		dataTable.AcceptChanges();
		return dataTable;
	}

	private DataTable Schema_ForeignKeys(string strCatalog, string strTable, string strKeyName)
	{
		DataTable dataTable = new DataTable("ForeignKeys");
		dataTable.Locale = CultureInfo.InvariantCulture;
		dataTable.Columns.Add("CONSTRAINT_CATALOG", typeof(string));
		dataTable.Columns.Add("CONSTRAINT_SCHEMA", typeof(string));
		dataTable.Columns.Add("CONSTRAINT_NAME", typeof(string));
		dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
		dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
		dataTable.Columns.Add("TABLE_NAME", typeof(string));
		dataTable.Columns.Add("CONSTRAINT_TYPE", typeof(string));
		dataTable.Columns.Add("IS_DEFERRABLE", typeof(bool));
		dataTable.Columns.Add("INITIALLY_DEFERRED", typeof(bool));
		dataTable.Columns.Add("FKEY_ID", typeof(int));
		dataTable.Columns.Add("FKEY_FROM_COLUMN", typeof(string));
		dataTable.Columns.Add("FKEY_FROM_ORDINAL_POSITION", typeof(int));
		dataTable.Columns.Add("FKEY_TO_CATALOG", typeof(string));
		dataTable.Columns.Add("FKEY_TO_SCHEMA", typeof(string));
		dataTable.Columns.Add("FKEY_TO_TABLE", typeof(string));
		dataTable.Columns.Add("FKEY_TO_COLUMN", typeof(string));
		dataTable.Columns.Add("FKEY_ON_UPDATE", typeof(string));
		dataTable.Columns.Add("FKEY_ON_DELETE", typeof(string));
		dataTable.Columns.Add("FKEY_MATCH", typeof(string));
		if (string.IsNullOrEmpty(strCatalog))
		{
			strCatalog = GetDefaultCatalogName();
		}
		string masterTableName = GetMasterTableName(IsTemporaryCatalogName(strCatalog));
		dataTable.BeginLoadData();
		using (SQLiteCommand sQLiteCommand = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}] WHERE [type] LIKE 'table'", strCatalog, masterTableName), this))
		{
			using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
			while (sQLiteDataReader.Read())
			{
				if (!string.IsNullOrEmpty(strTable) && string.Compare(strTable, sQLiteDataReader.GetString(2), StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}
				try
				{
					using SQLiteCommandBuilder sQLiteCommandBuilder = new SQLiteCommandBuilder();
					using SQLiteCommand sQLiteCommand2 = new SQLiteCommand(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "PRAGMA [{0}].foreign_key_list([{1}])", strCatalog, sQLiteDataReader.GetString(2)), this);
					using SQLiteDataReader sQLiteDataReader2 = sQLiteCommand2.ExecuteReader();
					while (sQLiteDataReader2.Read())
					{
						DataRow dataRow = dataTable.NewRow();
						dataRow["CONSTRAINT_CATALOG"] = strCatalog;
						dataRow["CONSTRAINT_NAME"] = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "FK_{0}_{1}_{2}", sQLiteDataReader[2], sQLiteDataReader2.GetInt32(0), sQLiteDataReader2.GetInt32(1));
						dataRow["TABLE_CATALOG"] = strCatalog;
						dataRow["TABLE_NAME"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader.GetString(2));
						dataRow["CONSTRAINT_TYPE"] = "FOREIGN KEY";
						dataRow["IS_DEFERRABLE"] = false;
						dataRow["INITIALLY_DEFERRED"] = false;
						dataRow["FKEY_ID"] = sQLiteDataReader2[0];
						dataRow["FKEY_FROM_COLUMN"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader2[3].ToString());
						dataRow["FKEY_TO_CATALOG"] = strCatalog;
						dataRow["FKEY_TO_TABLE"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader2[2].ToString());
						dataRow["FKEY_TO_COLUMN"] = sQLiteCommandBuilder.UnquoteIdentifier(sQLiteDataReader2[4].ToString());
						dataRow["FKEY_FROM_ORDINAL_POSITION"] = sQLiteDataReader2[1];
						dataRow["FKEY_ON_UPDATE"] = ((sQLiteDataReader2.FieldCount > 5) ? sQLiteDataReader2[5] : string.Empty);
						dataRow["FKEY_ON_DELETE"] = ((sQLiteDataReader2.FieldCount > 6) ? sQLiteDataReader2[6] : string.Empty);
						dataRow["FKEY_MATCH"] = ((sQLiteDataReader2.FieldCount > 7) ? sQLiteDataReader2[7] : string.Empty);
						if (string.IsNullOrEmpty(strKeyName) || string.Compare(strKeyName, dataRow["CONSTRAINT_NAME"].ToString(), StringComparison.OrdinalIgnoreCase) == 0)
						{
							dataTable.Rows.Add(dataRow);
						}
					}
				}
				catch (SQLiteException)
				{
				}
			}
		}
		dataTable.EndLoadData();
		dataTable.AcceptChanges();
		return dataTable;
	}

	private SQLiteBusyReturnCode BusyCallback(IntPtr pUserData, int count)
	{
		try
		{
			BusyEventArgs busyEventArgs = new BusyEventArgs(pUserData, count, SQLiteBusyReturnCode.Retry);
			if (this._busyHandler != null)
			{
				this._busyHandler(this, busyEventArgs);
			}
			return busyEventArgs.ReturnCode;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Busy", ex));
				}
			}
			catch
			{
			}
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.StopOnException))
		{
			return SQLiteBusyReturnCode.Stop;
		}
		return SQLiteBusyReturnCode.Retry;
	}

	private SQLiteProgressReturnCode ProgressCallback(IntPtr pUserData)
	{
		try
		{
			ProgressEventArgs progressEventArgs = new ProgressEventArgs(pUserData, SQLiteProgressReturnCode.Continue);
			if (this._progressHandler != null)
			{
				this._progressHandler(this, progressEventArgs);
			}
			return progressEventArgs.ReturnCode;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Progress", ex));
				}
			}
			catch
			{
			}
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.InterruptOnException))
		{
			return SQLiteProgressReturnCode.Interrupt;
		}
		return SQLiteProgressReturnCode.Continue;
	}

	private SQLiteAuthorizerReturnCode AuthorizerCallback(IntPtr pUserData, SQLiteAuthorizerActionCode actionCode, IntPtr pArgument1, IntPtr pArgument2, IntPtr pDatabase, IntPtr pAuthContext)
	{
		try
		{
			AuthorizerEventArgs authorizerEventArgs = new AuthorizerEventArgs(pUserData, actionCode, SQLiteConvert.UTF8ToString(pArgument1, -1), SQLiteConvert.UTF8ToString(pArgument2, -1), SQLiteConvert.UTF8ToString(pDatabase, -1), SQLiteConvert.UTF8ToString(pAuthContext, -1), SQLiteAuthorizerReturnCode.Ok);
			if (this._authorizerHandler != null)
			{
				this._authorizerHandler(this, authorizerEventArgs);
			}
			return authorizerEventArgs.ReturnCode;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Authorize", ex));
				}
			}
			catch
			{
			}
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.DenyOnException))
		{
			return SQLiteAuthorizerReturnCode.Deny;
		}
		return SQLiteAuthorizerReturnCode.Ok;
	}

	private void UpdateCallback(IntPtr puser, int type, IntPtr database, IntPtr table, long rowid)
	{
		try
		{
			this._updateHandler(this, new UpdateEventArgs(SQLiteConvert.UTF8ToString(database, -1), SQLiteConvert.UTF8ToString(table, -1), (UpdateEventType)type, rowid));
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Update", ex));
				}
			}
			catch
			{
			}
		}
	}

	private void TraceCallback(IntPtr puser, IntPtr statement)
	{
		try
		{
			if (this._traceHandler != null)
			{
				this._traceHandler(this, new TraceEventArgs(SQLiteConvert.UTF8ToString(statement, -1)));
			}
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Trace", ex));
				}
			}
			catch
			{
			}
		}
	}

	private int CommitCallback(IntPtr parg)
	{
		try
		{
			CommitEventArgs commitEventArgs = new CommitEventArgs();
			if (this._commitHandler != null)
			{
				this._commitHandler(this, commitEventArgs);
			}
			return commitEventArgs.AbortTransaction ? 1 : 0;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Commit", ex));
				}
			}
			catch
			{
			}
		}
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.RollbackOnException))
		{
			return 1;
		}
		return 0;
	}

	private void RollbackCallback(IntPtr parg)
	{
		try
		{
			if (this._rollbackHandler != null)
			{
				this._rollbackHandler(this, EventArgs.Empty);
			}
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(_flags))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "Rollback", ex));
				}
			}
			catch
			{
			}
		}
	}
}
