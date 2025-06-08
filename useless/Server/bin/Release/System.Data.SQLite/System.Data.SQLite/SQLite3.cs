using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace System.Data.SQLite;

internal class SQLite3 : SQLiteBase
{
	private static object syncRoot = new object();

	private IntPtr dbName = IntPtr.Zero;

	internal const string PublicKey = "002400000480000094000000060200000024000052534131000400000100010005a288de5687c4e1b621ddff5d844727418956997f475eb829429e411aff3e93f97b70de698b972640925bdd44280df0a25a843266973704137cbb0e7441c1fe7cae4e2440ae91ab8cde3933febcb1ac48dd33b40e13c421d8215c18a4349a436dd499e3c385cc683015f886f6c10bd90115eb2bd61b67750839e3a19941dc9c";

	internal const string DesignerVersion = "1.0.115.5";

	private const string PoolHashFileNamePrefix = "SQLITE_POOL_HASH:";

	protected internal SQLiteConnectionHandle _sql;

	protected string _fileName;

	protected string _returnToFileName;

	protected int _maxPoolSize;

	protected SQLiteConnectionFlags _flags;

	private bool _setLogCallback;

	protected bool _usePool;

	private bool _returnToPool;

	protected int _poolVersion;

	private int _cancelCount;

	private bool _buildingSchema;

	protected Dictionary<SQLiteFunctionAttribute, SQLiteFunction> _functions;

	protected string _shimExtensionFileName;

	protected bool? _shimIsLoadNeeded;

	protected string _shimExtensionProcName = "sqlite3_vtshim_init";

	protected Dictionary<string, SQLiteModule> _modules;

	private bool _forceLogPrepare;

	private bool disposed;

	private static bool? have_errstr = null;

	private static bool? have_stmt_readonly = null;

	private static bool? forceLogLifecycle = null;

	private const int MINIMUM_PAGE_SIZE = 512;

	private const int MAXIMUM_PAGE_SIZE = 65536;

	private const int PAGE_SIZE_OFFSET = 16;

	internal override bool ForceLogPrepare => _forceLogPrepare;

	internal override string Version => SQLiteVersion;

	internal override int VersionNumber => SQLiteVersionNumber;

	internal static string DefineConstants
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			IList<string> optionList = SQLiteDefineConstants.OptionList;
			if (optionList != null)
			{
				foreach (string item in optionList)
				{
					if (item != null)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(' ');
						}
						stringBuilder.Append(item);
					}
				}
			}
			return stringBuilder.ToString();
		}
	}

	internal static string SQLiteVersion => SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_libversion(), -1);

	internal static int SQLiteVersionNumber => UnsafeNativeMethods.sqlite3_libversion_number();

	internal static string SQLiteSourceId => SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_sourceid(), -1);

	internal static string SQLiteCompileOptions
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			IntPtr intPtr = UnsafeNativeMethods.sqlite3_compileoption_get(num++);
			while (intPtr != IntPtr.Zero)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(SQLiteConvert.UTF8ToString(intPtr, -1));
				intPtr = UnsafeNativeMethods.sqlite3_compileoption_get(num++);
			}
			return stringBuilder.ToString();
		}
	}

	internal static string InteropVersion => SQLiteConvert.UTF8ToString(UnsafeNativeMethods.interop_libversion(), -1);

	internal static string InteropSourceId => SQLiteConvert.UTF8ToString(UnsafeNativeMethods.interop_sourceid(), -1);

	internal static string InteropCompileOptions
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			IntPtr intPtr = UnsafeNativeMethods.interop_compileoption_get(num++);
			while (intPtr != IntPtr.Zero)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(SQLiteConvert.UTF8ToString(intPtr, -1));
				intPtr = UnsafeNativeMethods.interop_compileoption_get(num++);
			}
			return stringBuilder.ToString();
		}
	}

	internal override bool AutoCommit => SQLiteBase.IsAutocommit(_sql, _sql);

	internal override long LastInsertRowId => UnsafeNativeMethods.sqlite3_last_insert_rowid(_sql);

	internal override int Changes => UnsafeNativeMethods.sqlite3_changes_interop(_sql);

	internal override long MemoryUsed => StaticMemoryUsed;

	internal static long StaticMemoryUsed => UnsafeNativeMethods.sqlite3_memory_used();

	internal override long MemoryHighwater => StaticMemoryHighwater;

	internal static long StaticMemoryHighwater => UnsafeNativeMethods.sqlite3_memory_highwater(0);

	internal override bool OwnHandle
	{
		get
		{
			if (_sql == null)
			{
				throw new SQLiteException("no connection handle available");
			}
			return _sql.OwnHandle;
		}
	}

	internal override IDictionary<SQLiteFunctionAttribute, SQLiteFunction> Functions => _functions;

	internal SQLite3(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString, IntPtr db, string fileName, bool ownHandle)
		: base(fmt, kind, fmtString)
	{
		InitializeForceLogPrepare();
		if (db != IntPtr.Zero)
		{
			_sql = new SQLiteConnectionHandle(db, ownHandle);
			_fileName = fileName;
			_returnToFileName = fileName;
			SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, _sql, fileName, new object[7]
			{
				typeof(SQLite3),
				fmt,
				kind,
				fmtString,
				db,
				fileName,
				ownHandle
			}));
		}
	}

	private void InitializeForceLogPrepare()
	{
		if (UnsafeNativeMethods.GetSettingValue("SQLite_ForceLogPrepare", null) != null)
		{
			_forceLogPrepare = true;
		}
		else
		{
			_forceLogPrepare = false;
		}
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLite3).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposed)
			{
				DisposeModules();
				Close(disposing: true);
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}

	private void DisposeModules()
	{
		if (_modules == null)
		{
			return;
		}
		foreach (KeyValuePair<string, SQLiteModule> module in _modules)
		{
			module.Value?.Dispose();
		}
		_modules.Clear();
	}

	internal override void Close(bool disposing)
	{
		if (_sql == null)
		{
			return;
		}
		if (!_sql.OwnHandle)
		{
			_sql = null;
			return;
		}
		bool flag = HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.UnbindFunctionsOnClose);
		while (true)
		{
			if (_returnToPool || _usePool)
			{
				if (SQLiteBase.ResetConnection(_sql, _sql, !disposing) && UnhookNativeCallbacks(includeGlobal: true, !disposing))
				{
					if (flag)
					{
						SQLiteFunction.UnbindAllFunctions(this, _flags, registered: false);
					}
					DisposeModules();
					SQLiteConnectionPool.Add(_returnToFileName, _sql, _poolVersion);
					SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.ClosedToPool, null, null, null, null, _sql, _returnToFileName, new object[4]
					{
						typeof(SQLite3),
						!disposing,
						_returnToFileName,
						_poolVersion
					}));
					break;
				}
				_returnToFileName = _fileName;
				_returnToPool = false;
				_usePool = false;
				continue;
			}
			UnhookNativeCallbacks(disposing, !disposing);
			if (flag)
			{
				SQLiteFunction.UnbindAllFunctions(this, _flags, registered: false);
			}
			_sql.Dispose();
			FreeDbName(!disposing);
			break;
		}
		_sql = null;
	}

	private int GetCancelCount()
	{
		return Interlocked.CompareExchange(ref _cancelCount, 0, 0);
	}

	private bool ShouldThrowForCancel()
	{
		return GetCancelCount() > 0;
	}

	private int ResetCancelCount()
	{
		return Interlocked.CompareExchange(ref _cancelCount, 0, _cancelCount);
	}

	internal override void Cancel()
	{
		try
		{
		}
		finally
		{
			Interlocked.Increment(ref _cancelCount);
			UnsafeNativeMethods.sqlite3_interrupt(_sql);
		}
	}

	internal override void BindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteFunction function, SQLiteConnectionFlags flags)
	{
		if (functionAttribute == null)
		{
			throw new ArgumentNullException("functionAttribute");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		SQLiteFunction.BindFunction(this, functionAttribute, function, flags);
		if (_functions == null)
		{
			_functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();
		}
		_functions[functionAttribute] = function;
	}

	internal override bool UnbindFunction(SQLiteFunctionAttribute functionAttribute, SQLiteConnectionFlags flags)
	{
		if (functionAttribute == null)
		{
			throw new ArgumentNullException("functionAttribute");
		}
		if (_functions == null)
		{
			return false;
		}
		if (_functions.TryGetValue(functionAttribute, out var value) && SQLiteFunction.UnbindFunction(this, functionAttribute, value, flags) && _functions.Remove(functionAttribute))
		{
			return true;
		}
		return false;
	}

	internal override bool IsReadOnly(string name)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			if (name != null)
			{
				intPtr = SQLiteString.Utf8IntPtrFromString(name);
			}
			return UnsafeNativeMethods.sqlite3_db_readonly(_sql, intPtr) switch
			{
				-1 => throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "database \"{0}\" not found", name)), 
				0 => false, 
				_ => true, 
			};
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
			}
		}
	}

	internal override SQLiteErrorCode SetMemoryStatus(bool value)
	{
		return StaticSetMemoryStatus(value);
	}

	internal static SQLiteErrorCode StaticSetMemoryStatus(bool value)
	{
		return UnsafeNativeMethods.sqlite3_config_int(SQLiteConfigOpsEnum.SQLITE_CONFIG_MEMSTATUS, value ? 1 : 0);
	}

	internal override SQLiteErrorCode ReleaseMemory()
	{
		return UnsafeNativeMethods.sqlite3_db_release_memory(_sql);
	}

	internal static SQLiteErrorCode StaticReleaseMemory(int nBytes, bool reset, bool compact, ref int nFree, ref bool resetOk, ref uint nLargest)
	{
		SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Ok;
		int num = UnsafeNativeMethods.sqlite3_release_memory(nBytes);
		uint largest = 0u;
		bool flag = false;
		if (HelperMethods.IsWindows())
		{
			if (sQLiteErrorCode == SQLiteErrorCode.Ok && reset)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_reset_heap();
				if (sQLiteErrorCode == SQLiteErrorCode.Ok)
				{
					flag = true;
				}
			}
			if (sQLiteErrorCode == SQLiteErrorCode.Ok && compact)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_compact_heap(ref largest);
			}
		}
		else if (reset || compact)
		{
			sQLiteErrorCode = SQLiteErrorCode.NotFound;
		}
		nFree = num;
		nLargest = largest;
		resetOk = flag;
		return sQLiteErrorCode;
	}

	internal override SQLiteErrorCode Shutdown()
	{
		return StaticShutdown(directories: false);
	}

	internal static SQLiteErrorCode StaticShutdown(bool directories)
	{
		SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Ok;
		if (directories && HelperMethods.IsWindows())
		{
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_set_directory(1u, null);
			}
			if (sQLiteErrorCode == SQLiteErrorCode.Ok)
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_win32_set_directory(2u, null);
			}
		}
		if (sQLiteErrorCode == SQLiteErrorCode.Ok)
		{
			sQLiteErrorCode = UnsafeNativeMethods.sqlite3_shutdown();
		}
		return sQLiteErrorCode;
	}

	internal override bool IsOpen()
	{
		if (_sql != null && !_sql.IsInvalid)
		{
			return !_sql.IsClosed;
		}
		return false;
	}

	internal override string GetFileName(string dbName)
	{
		if (_sql == null)
		{
			return null;
		}
		return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_db_filename_bytes(_sql, SQLiteConvert.ToUTF8(dbName)), -1);
	}

	protected static bool IsAllowedToUsePool(SQLiteOpenFlagsEnum openFlags)
	{
		return openFlags == SQLiteOpenFlagsEnum.Default;
	}

	internal override void Open(string strFilename, string vfsName, SQLiteConnectionFlags connectionFlags, SQLiteOpenFlagsEnum openFlags, int maxPoolSize, bool usePool)
	{
		if (_sql != null)
		{
			Close(disposing: false);
		}
		if (_sql != null)
		{
			throw new SQLiteException("connection handle is still active");
		}
		_maxPoolSize = maxPoolSize;
		_returnToPool = false;
		_usePool = usePool;
		if (_usePool && !IsAllowedToUsePool(openFlags))
		{
			_usePool = false;
		}
		_fileName = strFilename;
		_returnToFileName = strFilename;
		_flags = connectionFlags;
		if (usePool)
		{
			_sql = SQLiteConnectionPool.Remove(strFilename, maxPoolSize, out _poolVersion);
			SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.OpenedFromPool, null, null, null, null, _sql, strFilename, new object[8]
			{
				typeof(SQLite3),
				strFilename,
				vfsName,
				connectionFlags,
				openFlags,
				maxPoolSize,
				usePool,
				_poolVersion
			}));
		}
		if (_sql == null)
		{
			try
			{
			}
			finally
			{
				IntPtr zero = IntPtr.Zero;
				uint id = (uint)Process.GetCurrentProcess().Id;
				zero = ((IntPtr.Size != 8) ? new IntPtr((int)(0x4407E41B | id)) : new IntPtr(-5197420724967971813L | id));
				int num = 0;
				if (!HelperMethods.HasFlags(connectionFlags, SQLiteConnectionFlags.NoExtensionFunctions))
				{
					num |= 1;
				}
				if (HelperMethods.HasFlags(connectionFlags, SQLiteConnectionFlags.NoCoreFunctions))
				{
					num |= 2;
				}
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_open_interop(SQLiteConvert.ToUTF8(strFilename), SQLiteConvert.ToUTF8(vfsName), openFlags, num, ref zero);
				if (sQLiteErrorCode != 0)
				{
					throw new SQLiteException(sQLiteErrorCode, null);
				}
				_sql = new SQLiteConnectionHandle(zero, ownHandle: true);
			}
			lock (_sql)
			{
			}
			SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, _sql, strFilename, new object[7]
			{
				typeof(SQLite3),
				strFilename,
				vfsName,
				connectionFlags,
				openFlags,
				maxPoolSize,
				usePool
			}));
		}
		if (!HelperMethods.HasFlags(connectionFlags, SQLiteConnectionFlags.NoBindFunctions))
		{
			if (_functions == null)
			{
				_functions = new Dictionary<SQLiteFunctionAttribute, SQLiteFunction>();
			}
			foreach (KeyValuePair<SQLiteFunctionAttribute, SQLiteFunction> item in SQLiteFunction.BindFunctions(this, connectionFlags))
			{
				_functions[item.Key] = item.Value;
			}
		}
		SetTimeout(0);
		GC.KeepAlive(_sql);
	}

	internal override void ClearPool()
	{
		SQLiteConnectionPool.ClearPool(_fileName);
	}

	internal override int CountPool()
	{
		Dictionary<string, int> counts = null;
		int openCount = 0;
		int closeCount = 0;
		int totalCount = 0;
		SQLiteConnectionPool.GetCounts(_fileName, ref counts, ref openCount, ref closeCount, ref totalCount);
		return totalCount;
	}

	internal override void SetTimeout(int nTimeoutMS)
	{
		IntPtr intPtr = _sql;
		if (intPtr == IntPtr.Zero)
		{
			throw new SQLiteException("no connection handle available");
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_busy_timeout(intPtr, nTimeoutMS);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override bool Step(SQLiteStatement stmt)
	{
		Random random = null;
		uint tickCount = (uint)Environment.TickCount;
		uint num = (uint)(stmt._command._commandTimeout * 1000);
		ResetCancelCount();
		SQLiteErrorCode sQLiteErrorCode;
		while (true)
		{
			try
			{
			}
			finally
			{
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_step(stmt._sqlite_stmt);
			}
			if (ShouldThrowForCancel())
			{
				break;
			}
			switch (sQLiteErrorCode)
			{
			case SQLiteErrorCode.Ok:
				continue;
			case SQLiteErrorCode.Interrupt:
				return false;
			case SQLiteErrorCode.Row:
				return true;
			case SQLiteErrorCode.Done:
				return false;
			}
			SQLiteErrorCode sQLiteErrorCode2 = Reset(stmt);
			switch (sQLiteErrorCode2)
			{
			case SQLiteErrorCode.Ok:
				throw new SQLiteException(sQLiteErrorCode, GetLastError());
			case SQLiteErrorCode.Busy:
			case SQLiteErrorCode.Locked:
				if (stmt._command != null)
				{
					if (random == null)
					{
						random = new Random();
					}
					if ((uint)(Environment.TickCount - (int)tickCount) > num)
					{
						throw new SQLiteException(sQLiteErrorCode2, GetLastError());
					}
					Thread.Sleep(random.Next(1, 150));
				}
				break;
			}
		}
		if (sQLiteErrorCode == SQLiteErrorCode.Ok || sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
		{
			sQLiteErrorCode = SQLiteErrorCode.Interrupt;
		}
		throw new SQLiteException(sQLiteErrorCode, null);
	}

	internal static string GetErrorString(SQLiteErrorCode rc)
	{
		try
		{
			if (!have_errstr.HasValue)
			{
				int sQLiteVersionNumber = SQLiteVersionNumber;
				have_errstr = sQLiteVersionNumber >= 3007015;
			}
			if (have_errstr.Value)
			{
				IntPtr intPtr = UnsafeNativeMethods.sqlite3_errstr(rc);
				if (intPtr != IntPtr.Zero)
				{
					return Marshal.PtrToStringAnsi(intPtr);
				}
			}
		}
		catch (EntryPointNotFoundException)
		{
		}
		return SQLiteBase.FallbackGetErrorString(rc);
	}

	internal override bool IsReadOnly(SQLiteStatement stmt)
	{
		try
		{
			if (!have_stmt_readonly.HasValue)
			{
				int sQLiteVersionNumber = SQLiteVersionNumber;
				have_stmt_readonly = sQLiteVersionNumber >= 3007004;
			}
			if (have_stmt_readonly.Value)
			{
				return UnsafeNativeMethods.sqlite3_stmt_readonly(stmt._sqlite_stmt) != 0;
			}
		}
		catch (EntryPointNotFoundException)
		{
		}
		return false;
	}

	internal override SQLiteErrorCode Reset(SQLiteStatement stmt)
	{
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_reset_interop(stmt._sqlite_stmt);
		switch (sQLiteErrorCode)
		{
		case SQLiteErrorCode.Schema:
		{
			string strRemain = null;
			using (SQLiteStatement sQLiteStatement = Prepare(null, stmt._sqlStatement, null, (uint)(stmt._command._commandTimeout * 1000), ref strRemain))
			{
				stmt._sqlite_stmt.Dispose();
				if (sQLiteStatement != null)
				{
					stmt._sqlite_stmt = sQLiteStatement._sqlite_stmt;
					sQLiteStatement._sqlite_stmt = null;
				}
				stmt.BindParameters();
			}
			return SQLiteErrorCode.Unknown;
		}
		case SQLiteErrorCode.Busy:
		case SQLiteErrorCode.Locked:
			return sQLiteErrorCode;
		default:
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		case SQLiteErrorCode.Ok:
			return sQLiteErrorCode;
		}
	}

	internal override string GetLastError()
	{
		return GetLastError(null);
	}

	internal override string GetLastError(string defValue)
	{
		string text = SQLiteBase.GetLastError(_sql, _sql);
		if (string.IsNullOrEmpty(text))
		{
			text = defValue;
		}
		return text;
	}

	internal static bool ForceLogLifecycle()
	{
		lock (syncRoot)
		{
			if (!forceLogLifecycle.HasValue)
			{
				if (UnsafeNativeMethods.GetSettingValue("SQLite_ForceLogLifecycle", null) != null)
				{
					forceLogLifecycle = true;
				}
				else
				{
					forceLogLifecycle = false;
				}
			}
			return forceLogLifecycle.Value;
		}
	}

	internal override SQLiteStatement Prepare(SQLiteConnection cnn, string strSql, SQLiteStatement previous, uint timeoutMS, ref string strRemain)
	{
		if (!string.IsNullOrEmpty(strSql))
		{
			strSql = strSql.Trim();
		}
		if (!string.IsNullOrEmpty(strSql))
		{
			string text = cnn?._baseSchemaName;
			if (!string.IsNullOrEmpty(text))
			{
				strSql = strSql.Replace(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "[{0}].", text), string.Empty);
				strSql = strSql.Replace(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "{0}.", text), string.Empty);
			}
		}
		SQLiteConnectionFlags flags = cnn?.Flags ?? SQLiteConnectionFlags.Default;
		if (_forceLogPrepare || HelperMethods.LogPrepare(flags))
		{
			if (strSql == null || strSql.Length == 0 || strSql.Trim().Length == 0)
			{
				SQLiteLog.LogMessage("Preparing {<nothing>}...");
			}
			else
			{
				SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Preparing {{{0}}}...", strSql));
			}
		}
		IntPtr zero = IntPtr.Zero;
		IntPtr ptrRemain = IntPtr.Zero;
		int nRemain = 0;
		SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Schema;
		int num = 0;
		int num2 = cnn?._prepareRetries ?? 3;
		byte[] array = SQLiteConvert.ToUTF8(strSql);
		string text2 = null;
		SQLiteStatement sQLiteStatement = null;
		Random random = null;
		uint tickCount = (uint)Environment.TickCount;
		ResetCancelCount();
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		IntPtr pSql = gCHandle.AddrOfPinnedObject();
		SQLiteStatementHandle sQLiteStatementHandle = null;
		try
		{
			while ((sQLiteErrorCode == SQLiteErrorCode.Schema || sQLiteErrorCode == SQLiteErrorCode.Locked || sQLiteErrorCode == SQLiteErrorCode.Busy) && num < num2)
			{
				try
				{
				}
				finally
				{
					zero = IntPtr.Zero;
					ptrRemain = IntPtr.Zero;
					nRemain = 0;
					sQLiteErrorCode = UnsafeNativeMethods.sqlite3_prepare_interop(_sql, pSql, array.Length - 1, ref zero, ref ptrRemain, ref nRemain);
					if (sQLiteErrorCode == SQLiteErrorCode.Ok && zero != IntPtr.Zero)
					{
						sQLiteStatementHandle?.Dispose();
						sQLiteStatementHandle = new SQLiteStatementHandle(_sql, zero);
					}
				}
				if (sQLiteStatementHandle != null)
				{
					SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteStatementHandle, strSql, new object[5]
					{
						typeof(SQLite3),
						cnn,
						strSql,
						previous,
						timeoutMS
					}));
				}
				if (ShouldThrowForCancel())
				{
					if (sQLiteErrorCode == SQLiteErrorCode.Ok || sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
					{
						sQLiteErrorCode = SQLiteErrorCode.Interrupt;
					}
					throw new SQLiteException(sQLiteErrorCode, null);
				}
				switch (sQLiteErrorCode)
				{
				case SQLiteErrorCode.Schema:
					num++;
					continue;
				case SQLiteErrorCode.Error:
					if (string.Compare(GetLastError(), "near \"TYPES\": syntax error", StringComparison.OrdinalIgnoreCase) == 0)
					{
						int num3 = strSql.IndexOf(';');
						if (num3 == -1)
						{
							num3 = strSql.Length - 1;
						}
						text2 = strSql.Substring(0, num3 + 1);
						strSql = strSql.Substring(num3 + 1);
						strRemain = string.Empty;
						while (sQLiteStatement == null && strSql.Length > 0)
						{
							sQLiteStatement = Prepare(cnn, strSql, previous, timeoutMS, ref strRemain);
							strSql = strRemain;
						}
						sQLiteStatement?.SetTypes(text2);
						return sQLiteStatement;
					}
					if (_buildingSchema || string.Compare(GetLastError(), 0, "no such table: TEMP.SCHEMA", 0, 26, StringComparison.OrdinalIgnoreCase) != 0)
					{
						continue;
					}
					strRemain = string.Empty;
					_buildingSchema = true;
					try
					{
						if (((IServiceProvider)SQLiteFactory.Instance).GetService(typeof(ISQLiteSchemaExtensions)) is ISQLiteSchemaExtensions iSQLiteSchemaExtensions)
						{
							iSQLiteSchemaExtensions.BuildTempSchema(cnn);
						}
						while (sQLiteStatement == null && strSql.Length > 0)
						{
							sQLiteStatement = Prepare(cnn, strSql, previous, timeoutMS, ref strRemain);
							strSql = strRemain;
						}
						return sQLiteStatement;
					}
					finally
					{
						_buildingSchema = false;
					}
				case SQLiteErrorCode.Busy:
				case SQLiteErrorCode.Locked:
					if (random == null)
					{
						random = new Random();
					}
					if ((uint)(Environment.TickCount - (int)tickCount) > timeoutMS)
					{
						throw new SQLiteException(sQLiteErrorCode, GetLastError());
					}
					Thread.Sleep(random.Next(1, 150));
					continue;
				default:
					continue;
				case SQLiteErrorCode.Interrupt:
					break;
				}
				break;
			}
			if (ShouldThrowForCancel())
			{
				if (sQLiteErrorCode == SQLiteErrorCode.Ok || sQLiteErrorCode == SQLiteErrorCode.Row || sQLiteErrorCode == SQLiteErrorCode.Done)
				{
					sQLiteErrorCode = SQLiteErrorCode.Interrupt;
				}
				throw new SQLiteException(sQLiteErrorCode, null);
			}
			switch (sQLiteErrorCode)
			{
			case SQLiteErrorCode.Interrupt:
				return null;
			default:
				throw new SQLiteException(sQLiteErrorCode, GetLastError());
			case SQLiteErrorCode.Ok:
				strRemain = SQLiteConvert.UTF8ToString(ptrRemain, nRemain);
				if (sQLiteStatementHandle != null)
				{
					sQLiteStatement = new SQLiteStatement(this, flags, sQLiteStatementHandle, strSql.Substring(0, strSql.Length - strRemain.Length), previous);
				}
				return sQLiteStatement;
			}
		}
		finally
		{
			gCHandle.Free();
		}
	}

	protected static void LogBind(SQLiteStatementHandle handle, int index)
	{
		IntPtr intPtr = handle;
		SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Binding statement {0} paramter #{1} as NULL...", intPtr, index));
	}

	protected static void LogBind(SQLiteStatementHandle handle, int index, ValueType value)
	{
		IntPtr intPtr = handle;
		SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", intPtr, index, value.GetType(), value));
	}

	private static string FormatDateTime(DateTime value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK"));
		stringBuilder.Append(' ');
		stringBuilder.Append(value.Kind);
		stringBuilder.Append(' ');
		stringBuilder.Append(value.Ticks);
		return stringBuilder.ToString();
	}

	protected static void LogBind(SQLiteStatementHandle handle, int index, DateTime value)
	{
		IntPtr intPtr = handle;
		SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", intPtr, index, typeof(DateTime), FormatDateTime(value)));
	}

	protected static void LogBind(SQLiteStatementHandle handle, int index, string value)
	{
		IntPtr intPtr = handle;
		SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", intPtr, index, typeof(string), (value != null) ? value : "<null>"));
	}

	private static string ToHexadecimalString(byte[] array)
	{
		if (array == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	protected static void LogBind(SQLiteStatementHandle handle, int index, byte[] value)
	{
		IntPtr intPtr = handle;
		SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Binding statement {0} paramter #{1} as type {2} with value {{{3}}}...", intPtr, index, typeof(byte[]), (value != null) ? ToHexadecimalString(value) : "<null>"));
	}

	internal override void Bind_Double(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, double value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_double(sqlite_stmt, index, value);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_Int32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, int value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int(sqlite_stmt, index, value);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_UInt32(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, uint value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		SQLiteErrorCode sQLiteErrorCode;
		if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.BindUInt32AsInt64))
		{
			long value2 = value;
			sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int64(sqlite_stmt, index, value2);
		}
		else
		{
			sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_uint(sqlite_stmt, index, value);
		}
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_Int64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, long value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int64(sqlite_stmt, index, value);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_UInt64(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, ulong value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_uint64(sqlite_stmt, index, value);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_Boolean(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, bool value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		int value2 = (value ? 1 : 0);
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_int(sqlite_stmt, index, value2);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, value);
		}
		byte[] array = SQLiteConvert.ToUTF8(value);
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, array);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text(sqlite_stmt, index, array, array.Length - 1, (IntPtr)(-1));
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, dt);
		}
		if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.BindDateTimeWithKind) && _datetimeKind != 0 && dt.Kind != 0 && dt.Kind != _datetimeKind)
		{
			if (_datetimeKind == DateTimeKind.Utc)
			{
				dt = dt.ToUniversalTime();
			}
			else if (_datetimeKind == DateTimeKind.Local)
			{
				dt = dt.ToLocalTime();
			}
		}
		switch (_datetimeFormat)
		{
		case SQLiteDateFormats.Ticks:
		{
			long ticks = dt.Ticks;
			if (_forceLogPrepare || HelperMethods.LogBind(flags))
			{
				LogBind(sqlite_stmt, index, ticks);
			}
			SQLiteErrorCode sQLiteErrorCode4 = UnsafeNativeMethods.sqlite3_bind_int64(sqlite_stmt, index, ticks);
			if (sQLiteErrorCode4 != 0)
			{
				throw new SQLiteException(sQLiteErrorCode4, GetLastError());
			}
			break;
		}
		case SQLiteDateFormats.JulianDay:
		{
			double num = SQLiteConvert.ToJulianDay(dt);
			if (_forceLogPrepare || HelperMethods.LogBind(flags))
			{
				LogBind(sqlite_stmt, index, num);
			}
			SQLiteErrorCode sQLiteErrorCode2 = UnsafeNativeMethods.sqlite3_bind_double(sqlite_stmt, index, num);
			if (sQLiteErrorCode2 != 0)
			{
				throw new SQLiteException(sQLiteErrorCode2, GetLastError());
			}
			break;
		}
		case SQLiteDateFormats.UnixEpoch:
		{
			long num2 = Convert.ToInt64(dt.Subtract(SQLiteConvert.UnixEpoch).TotalSeconds);
			if (_forceLogPrepare || HelperMethods.LogBind(flags))
			{
				LogBind(sqlite_stmt, index, num2);
			}
			SQLiteErrorCode sQLiteErrorCode3 = UnsafeNativeMethods.sqlite3_bind_int64(sqlite_stmt, index, num2);
			if (sQLiteErrorCode3 != 0)
			{
				throw new SQLiteException(sQLiteErrorCode3, GetLastError());
			}
			break;
		}
		default:
		{
			byte[] array = ToUTF8(dt);
			if (_forceLogPrepare || HelperMethods.LogBind(flags))
			{
				LogBind(sqlite_stmt, index, array);
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text(sqlite_stmt, index, array, array.Length - 1, (IntPtr)(-1));
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, GetLastError());
			}
			break;
		}
		}
	}

	internal override void Bind_Blob(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, byte[] blobData)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index, blobData);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_blob(sqlite_stmt, index, blobData, blobData.Length, (IntPtr)(-1));
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void Bind_Null(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			LogBind(sqlite_stmt, index);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_null(sqlite_stmt, index);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override int Bind_ParamCount(SQLiteStatement stmt, SQLiteConnectionFlags flags)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		int num = UnsafeNativeMethods.sqlite3_bind_parameter_count(sqlite_stmt);
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			IntPtr intPtr = sqlite_stmt;
			SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Statement {0} paramter count is {1}.", intPtr, num));
		}
		return num;
	}

	internal override string Bind_ParamName(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		int len = 0;
		string text = SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name_interop(sqlite_stmt, index, ref len), len);
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			IntPtr intPtr = sqlite_stmt;
			SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Statement {0} paramter #{1} name is {{{2}}}.", intPtr, index, text));
		}
		return text;
	}

	internal override int Bind_ParamIndex(SQLiteStatement stmt, SQLiteConnectionFlags flags, string paramName)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		int num = UnsafeNativeMethods.sqlite3_bind_parameter_index(sqlite_stmt, SQLiteConvert.ToUTF8(paramName));
		if (_forceLogPrepare || HelperMethods.LogBind(flags))
		{
			IntPtr intPtr = sqlite_stmt;
			SQLiteLog.LogMessage(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Statement {0} paramter index of name {{{1}}} is #{2}.", intPtr, paramName, num));
		}
		return num;
	}

	internal override int ColumnCount(SQLiteStatement stmt)
	{
		return UnsafeNativeMethods.sqlite3_column_count(stmt._sqlite_stmt);
	}

	internal override string ColumnName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_name_interop(stmt._sqlite_stmt, index, ref len);
		if (intPtr == IntPtr.Zero)
		{
			throw new SQLiteException(SQLiteErrorCode.NoMem, GetLastError());
		}
		return SQLiteConvert.UTF8ToString(intPtr, len);
	}

	internal override TypeAffinity ColumnAffinity(SQLiteStatement stmt, int index)
	{
		return UnsafeNativeMethods.sqlite3_column_type(stmt._sqlite_stmt, index);
	}

	internal override string ColumnType(SQLiteStatement stmt, int index, ref TypeAffinity nAffinity)
	{
		int len = 0;
		IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_decltype_interop(stmt._sqlite_stmt, index, ref len);
		nAffinity = ColumnAffinity(stmt, index);
		if (intPtr != IntPtr.Zero && (len > 0 || len == -1))
		{
			string text = SQLiteConvert.UTF8ToString(intPtr, len);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		string[] typeDefinitions = stmt.TypeDefinitions;
		if (typeDefinitions != null && index < typeDefinitions.Length && typeDefinitions[index] != null)
		{
			return typeDefinitions[index];
		}
		return string.Empty;
	}

	internal override int ColumnIndex(SQLiteStatement stmt, string columnName)
	{
		int num = ColumnCount(stmt);
		for (int i = 0; i < num; i++)
		{
			if (string.Compare(columnName, ColumnName(stmt, i), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override string ColumnTableName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override bool DoesTableExist(string dataBase, string table)
	{
		string dataType = null;
		string collateSequence = null;
		bool notNull = false;
		bool primaryKey = false;
		bool autoIncrement = false;
		return ColumnMetaData(dataBase, table, null, canThrow: false, ref dataType, ref collateSequence, ref notNull, ref primaryKey, ref autoIncrement);
	}

	internal override bool ColumnMetaData(string dataBase, string table, string column, bool canThrow, ref string dataType, ref string collateSequence, ref bool notNull, ref bool primaryKey, ref bool autoIncrement)
	{
		IntPtr ptrDataType = IntPtr.Zero;
		IntPtr ptrCollSeq = IntPtr.Zero;
		int notNull2 = 0;
		int primaryKey2 = 0;
		int autoInc = 0;
		int dtLen = 0;
		int csLen = 0;
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_table_column_metadata_interop(_sql, SQLiteConvert.ToUTF8(dataBase), SQLiteConvert.ToUTF8(table), SQLiteConvert.ToUTF8(column), ref ptrDataType, ref ptrCollSeq, ref notNull2, ref primaryKey2, ref autoInc, ref dtLen, ref csLen);
		if (canThrow && sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
		dataType = SQLiteConvert.UTF8ToString(ptrDataType, dtLen);
		collateSequence = SQLiteConvert.UTF8ToString(ptrCollSeq, csLen);
		notNull = notNull2 == 1;
		primaryKey = primaryKey2 == 1;
		autoIncrement = autoInc == 1;
		return sQLiteErrorCode == SQLiteErrorCode.Ok;
	}

	internal override object GetObject(SQLiteStatement stmt, int index)
	{
		switch (ColumnAffinity(stmt, index))
		{
		case TypeAffinity.Int64:
			return GetInt64(stmt, index);
		case TypeAffinity.Double:
			return GetDouble(stmt, index);
		case TypeAffinity.Text:
			return GetText(stmt, index);
		case TypeAffinity.Blob:
		{
			long bytes = GetBytes(stmt, index, 0, null, 0, 0);
			if (bytes > 0 && bytes <= int.MaxValue)
			{
				byte[] array = new byte[(int)bytes];
				GetBytes(stmt, index, 0, array, 0, (int)bytes);
				return array;
			}
			break;
		}
		case TypeAffinity.Null:
			return DBNull.Value;
		}
		throw new NotImplementedException();
	}

	internal override double GetDouble(SQLiteStatement stmt, int index)
	{
		return UnsafeNativeMethods.sqlite3_column_double(stmt._sqlite_stmt, index);
	}

	internal override bool GetBoolean(SQLiteStatement stmt, int index)
	{
		return SQLiteConvert.ToBoolean(GetObject(stmt, index), CultureInfo.InvariantCulture, viaFramework: false);
	}

	internal override sbyte GetSByte(SQLiteStatement stmt, int index)
	{
		return (sbyte)(GetInt32(stmt, index) & 0xFF);
	}

	internal override byte GetByte(SQLiteStatement stmt, int index)
	{
		return (byte)((uint)GetInt32(stmt, index) & 0xFFu);
	}

	internal override short GetInt16(SQLiteStatement stmt, int index)
	{
		return (short)(GetInt32(stmt, index) & 0xFFFF);
	}

	internal override ushort GetUInt16(SQLiteStatement stmt, int index)
	{
		return (ushort)((uint)GetInt32(stmt, index) & 0xFFFFu);
	}

	internal override int GetInt32(SQLiteStatement stmt, int index)
	{
		return UnsafeNativeMethods.sqlite3_column_int(stmt._sqlite_stmt, index);
	}

	internal override uint GetUInt32(SQLiteStatement stmt, int index)
	{
		return (uint)GetInt32(stmt, index);
	}

	internal override long GetInt64(SQLiteStatement stmt, int index)
	{
		return UnsafeNativeMethods.sqlite3_column_int64(stmt._sqlite_stmt, index);
	}

	internal override ulong GetUInt64(SQLiteStatement stmt, int index)
	{
		return (ulong)GetInt64(stmt, index);
	}

	internal override string GetText(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override DateTime GetDateTime(SQLiteStatement stmt, int index)
	{
		if (_datetimeFormat == SQLiteDateFormats.Ticks)
		{
			return SQLiteConvert.TicksToDateTime(GetInt64(stmt, index), _datetimeKind);
		}
		if (_datetimeFormat == SQLiteDateFormats.JulianDay)
		{
			return SQLiteConvert.ToDateTime(GetDouble(stmt, index), _datetimeKind);
		}
		if (_datetimeFormat == SQLiteDateFormats.UnixEpoch)
		{
			return SQLiteConvert.UnixEpochToDateTime(GetInt64(stmt, index), _datetimeKind);
		}
		int len = 0;
		return ToDateTime(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override long GetBytes(SQLiteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart, int nLength)
	{
		int num = UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index);
		if (bDest == null)
		{
			return num;
		}
		int num2 = nLength;
		if (num2 + nStart > bDest.Length)
		{
			num2 = bDest.Length - nStart;
		}
		if (num2 + nDataOffset > num)
		{
			num2 = num - nDataOffset;
		}
		if (num2 > 0)
		{
			Marshal.Copy((IntPtr)(UnsafeNativeMethods.sqlite3_column_blob(stmt._sqlite_stmt, index).ToInt64() + nDataOffset), bDest, nStart, num2);
		}
		else
		{
			num2 = 0;
		}
		return num2;
	}

	internal override char GetChar(SQLiteStatement stmt, int index)
	{
		return Convert.ToChar(GetUInt16(stmt, index));
	}

	internal override long GetChars(SQLiteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart, int nLength)
	{
		int num = nLength;
		string text = GetText(stmt, index);
		int length = text.Length;
		if (bDest == null)
		{
			return length;
		}
		if (num + nStart > bDest.Length)
		{
			num = bDest.Length - nStart;
		}
		if (num + nDataOffset > length)
		{
			num = length - nDataOffset;
		}
		if (num > 0)
		{
			text.CopyTo(nDataOffset, bDest, nStart, num);
		}
		else
		{
			num = 0;
		}
		return num;
	}

	internal override bool IsNull(SQLiteStatement stmt, int index)
	{
		return ColumnAffinity(stmt, index) == TypeAffinity.Null;
	}

	internal override int AggregateCount(IntPtr context)
	{
		return UnsafeNativeMethods.sqlite3_aggregate_count(context);
	}

	internal override SQLiteErrorCode CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal, bool canThrow)
	{
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_function_interop(_sql, SQLiteConvert.ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal, needCollSeq ? 1 : 0);
		if (sQLiteErrorCode == SQLiteErrorCode.Ok)
		{
			sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_function_interop(_sql, SQLiteConvert.ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal, needCollSeq ? 1 : 0);
		}
		if (canThrow && sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
		return sQLiteErrorCode;
	}

	internal override SQLiteErrorCode CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16, bool canThrow)
	{
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_collation(_sql, SQLiteConvert.ToUTF8(strCollation), 2, IntPtr.Zero, func16);
		if (sQLiteErrorCode == SQLiteErrorCode.Ok)
		{
			sQLiteErrorCode = UnsafeNativeMethods.sqlite3_create_collation(_sql, SQLiteConvert.ToUTF8(strCollation), 1, IntPtr.Zero, func);
		}
		if (canThrow && sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
		return sQLiteErrorCode;
	}

	internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2)
	{
		Encoding encoding = null;
		switch (enc)
		{
		case CollationEncodingEnum.UTF8:
			encoding = Encoding.UTF8;
			break;
		case CollationEncodingEnum.UTF16LE:
			encoding = Encoding.Unicode;
			break;
		case CollationEncodingEnum.UTF16BE:
			encoding = Encoding.BigEndianUnicode;
			break;
		}
		byte[] bytes = encoding.GetBytes(s1);
		byte[] bytes2 = encoding.GetBytes(s2);
		return UnsafeNativeMethods.sqlite3_context_collcompare_interop(context, bytes, bytes.Length, bytes2, bytes2.Length);
	}

	internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2)
	{
		Encoding encoding = null;
		switch (enc)
		{
		case CollationEncodingEnum.UTF8:
			encoding = Encoding.UTF8;
			break;
		case CollationEncodingEnum.UTF16LE:
			encoding = Encoding.Unicode;
			break;
		case CollationEncodingEnum.UTF16BE:
			encoding = Encoding.BigEndianUnicode;
			break;
		}
		byte[] bytes = encoding.GetBytes(c1);
		byte[] bytes2 = encoding.GetBytes(c2);
		return UnsafeNativeMethods.sqlite3_context_collcompare_interop(context, bytes, bytes.Length, bytes2, bytes2.Length);
	}

	internal override CollationSequence GetCollationSequence(SQLiteFunction func, IntPtr context)
	{
		CollationSequence result = default(CollationSequence);
		int len = 0;
		int type = 0;
		int enc = 0;
		IntPtr nativestring = UnsafeNativeMethods.sqlite3_context_collseq_interop(context, ref type, ref enc, ref len);
		result.Name = SQLiteConvert.UTF8ToString(nativestring, len);
		result.Type = (CollationTypeEnum)type;
		result._func = func;
		result.Encoding = (CollationEncodingEnum)enc;
		return result;
	}

	internal override long GetParamValueBytes(IntPtr p, int nDataOffset, byte[] bDest, int nStart, int nLength)
	{
		int num = UnsafeNativeMethods.sqlite3_value_bytes(p);
		if (bDest == null)
		{
			return num;
		}
		int num2 = nLength;
		if (num2 + nStart > bDest.Length)
		{
			num2 = bDest.Length - nStart;
		}
		if (num2 + nDataOffset > num)
		{
			num2 = num - nDataOffset;
		}
		if (num2 > 0)
		{
			Marshal.Copy((IntPtr)(UnsafeNativeMethods.sqlite3_value_blob(p).ToInt64() + nDataOffset), bDest, nStart, num2);
		}
		else
		{
			num2 = 0;
		}
		return num2;
	}

	internal override double GetParamValueDouble(IntPtr ptr)
	{
		return UnsafeNativeMethods.sqlite3_value_double(ptr);
	}

	internal override int GetParamValueInt32(IntPtr ptr)
	{
		return UnsafeNativeMethods.sqlite3_value_int(ptr);
	}

	internal override long GetParamValueInt64(IntPtr ptr)
	{
		return UnsafeNativeMethods.sqlite3_value_int64(ptr);
	}

	internal override string GetParamValueText(IntPtr ptr)
	{
		int len = 0;
		return SQLiteConvert.UTF8ToString(UnsafeNativeMethods.sqlite3_value_text_interop(ptr, ref len), len);
	}

	internal override TypeAffinity GetParamValueType(IntPtr ptr)
	{
		return UnsafeNativeMethods.sqlite3_value_type(ptr);
	}

	internal override void ReturnBlob(IntPtr context, byte[] value)
	{
		UnsafeNativeMethods.sqlite3_result_blob(context, value, value.Length, (IntPtr)(-1));
	}

	internal override void ReturnDouble(IntPtr context, double value)
	{
		UnsafeNativeMethods.sqlite3_result_double(context, value);
	}

	internal override void ReturnError(IntPtr context, string value)
	{
		UnsafeNativeMethods.sqlite3_result_error(context, SQLiteConvert.ToUTF8(value), value.Length);
	}

	internal override void ReturnInt32(IntPtr context, int value)
	{
		UnsafeNativeMethods.sqlite3_result_int(context, value);
	}

	internal override void ReturnInt64(IntPtr context, long value)
	{
		UnsafeNativeMethods.sqlite3_result_int64(context, value);
	}

	internal override void ReturnNull(IntPtr context)
	{
		UnsafeNativeMethods.sqlite3_result_null(context);
	}

	internal override void ReturnText(IntPtr context, string value)
	{
		byte[] array = SQLiteConvert.ToUTF8(value);
		UnsafeNativeMethods.sqlite3_result_text(context, SQLiteConvert.ToUTF8(value), array.Length - 1, (IntPtr)(-1));
	}

	private string GetShimExtensionFileName(ref bool isLoadNeeded)
	{
		if (_shimIsLoadNeeded.HasValue)
		{
			isLoadNeeded = _shimIsLoadNeeded.Value;
		}
		else
		{
			isLoadNeeded = HelperMethods.IsWindows();
		}
		string shimExtensionFileName = _shimExtensionFileName;
		if (shimExtensionFileName != null)
		{
			return shimExtensionFileName;
		}
		return UnsafeNativeMethods.GetNativeLibraryFileNameOnly();
	}

	internal override void CreateModule(SQLiteModule module, SQLiteConnectionFlags flags)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		if (HelperMethods.NoLogModule(flags))
		{
			module.LogErrors = HelperMethods.LogModuleError(flags);
			module.LogExceptions = HelperMethods.LogModuleException(flags);
		}
		if (_sql == null)
		{
			throw new SQLiteException("connection has an invalid handle");
		}
		bool isLoadNeeded = false;
		string shimExtensionFileName = GetShimExtensionFileName(ref isLoadNeeded);
		if (isLoadNeeded)
		{
			if (shimExtensionFileName == null)
			{
				throw new SQLiteException("the file name for the \"vtshim\" extension is unknown");
			}
			if (_shimExtensionProcName == null)
			{
				throw new SQLiteException("the entry point for the \"vtshim\" extension is unknown");
			}
			SetLoadExtension(bOnOff: true);
			LoadExtension(shimExtensionFileName, _shimExtensionProcName);
		}
		if (module.CreateDisposableModule(_sql))
		{
			if (_modules == null)
			{
				_modules = new Dictionary<string, SQLiteModule>();
			}
			_modules.Add(module.Name, module);
			if (_usePool)
			{
				_usePool = false;
			}
			return;
		}
		throw new SQLiteException(GetLastError());
	}

	internal override void DisposeModule(SQLiteModule module, SQLiteConnectionFlags flags)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		module.Dispose();
	}

	internal override IntPtr AggregateContext(IntPtr context)
	{
		return UnsafeNativeMethods.sqlite3_aggregate_context(context, 1);
	}

	internal override SQLiteErrorCode DeclareVirtualTable(SQLiteModule module, string strSql, ref string error)
	{
		if (_sql == null)
		{
			error = "connection has an invalid handle";
			return SQLiteErrorCode.Error;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = SQLiteString.Utf8IntPtrFromString(strSql);
			SQLiteErrorCode num = UnsafeNativeMethods.sqlite3_declare_vtab(_sql, intPtr);
			if (num == SQLiteErrorCode.Ok && module != null)
			{
				module.Declared = true;
			}
			if (num != 0)
			{
				error = GetLastError();
			}
			return num;
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
			}
		}
	}

	internal override SQLiteErrorCode DeclareVirtualFunction(SQLiteModule module, int argumentCount, string name, ref string error)
	{
		if (_sql == null)
		{
			error = "connection has an invalid handle";
			return SQLiteErrorCode.Error;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = SQLiteString.Utf8IntPtrFromString(name);
			SQLiteErrorCode num = UnsafeNativeMethods.sqlite3_overload_function(_sql, intPtr, argumentCount);
			if (num != 0)
			{
				error = GetLastError();
			}
			return num;
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				SQLiteMemory.Free(intPtr);
				intPtr = IntPtr.Zero;
			}
		}
	}

	private static string GetStatusDbOpsNames()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] names = Enum.GetNames(typeof(SQLiteStatusOpsEnum));
		foreach (string value in names)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetLimitOpsNames()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] names = Enum.GetNames(typeof(SQLiteLimitOpsEnum));
		foreach (string value in names)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetConfigDbOpsNames()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] names = Enum.GetNames(typeof(SQLiteConfigDbOpsEnum));
		foreach (string value in names)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	internal override SQLiteErrorCode GetStatusParameter(SQLiteStatusOpsEnum option, bool reset, ref int current, ref int highwater)
	{
		if (!Enum.IsDefined(typeof(SQLiteStatusOpsEnum), option))
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "unrecognized status option, must be: {0}", GetStatusDbOpsNames()));
		}
		return UnsafeNativeMethods.sqlite3_db_status(_sql, option, ref current, ref highwater, reset ? 1 : 0);
	}

	internal override int SetLimitOption(SQLiteLimitOpsEnum option, int value)
	{
		if (!Enum.IsDefined(typeof(SQLiteLimitOpsEnum), option))
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "unrecognized limit option, must be: {0}", GetLimitOpsNames()));
		}
		return UnsafeNativeMethods.sqlite3_limit(_sql, option, value);
	}

	internal override SQLiteErrorCode SetConfigurationOption(SQLiteConfigDbOpsEnum option, object value)
	{
		if (!Enum.IsDefined(typeof(SQLiteConfigDbOpsEnum), option))
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "unrecognized configuration option, must be: {0}", GetConfigDbOpsNames()));
		}
		switch (option)
		{
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_NONE:
			return SQLiteErrorCode.Ok;
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_MAINDBNAME:
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is string))
			{
				throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "configuration value type mismatch, must be of type {0}", typeof(string)));
			}
			SQLiteErrorCode sQLiteErrorCode = SQLiteErrorCode.Error;
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = SQLiteString.Utf8IntPtrFromString((string)value);
				if (intPtr == IntPtr.Zero)
				{
					throw new SQLiteException(SQLiteErrorCode.NoMem, "cannot allocate database name");
				}
				sQLiteErrorCode = UnsafeNativeMethods.sqlite3_db_config_charptr(_sql, option, intPtr);
				if (sQLiteErrorCode == SQLiteErrorCode.Ok)
				{
					FreeDbName(canThrow: true);
					dbName = intPtr;
					intPtr = IntPtr.Zero;
				}
			}
			finally
			{
				if (sQLiteErrorCode != 0 && intPtr != IntPtr.Zero)
				{
					SQLiteMemory.Free(intPtr);
					intPtr = IntPtr.Zero;
				}
			}
			return sQLiteErrorCode;
		}
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LOOKASIDE:
			if (!(value is object[] array))
			{
				throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "configuration value type mismatch, must be of type {0}", typeof(object[])));
			}
			if (!(array[0] is IntPtr))
			{
				throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "configuration element zero (0) type mismatch, must be of type {0}", typeof(IntPtr)));
			}
			if (!(array[1] is int))
			{
				throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "configuration element one (1) type mismatch, must be of type {0}", typeof(int)));
			}
			if (!(array[2] is int))
			{
				throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "configuration element two (2) type mismatch, must be of type {0}", typeof(int)));
			}
			return UnsafeNativeMethods.sqlite3_db_config_intptr_two_ints(_sql, option, (IntPtr)array[0], (int)array[1], (int)array[2]);
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FKEY:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_TRIGGER:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_FTS3_TOKENIZER:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_NO_CKPT_ON_CLOSE:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_QPSG:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_TRIGGER_EQP:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_RESET_DATABASE:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DEFENSIVE:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_WRITABLE_SCHEMA:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LEGACY_ALTER_TABLE:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DQS_DML:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_DQS_DDL:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_VIEW:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_LEGACY_FILE_FORMAT:
		case SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_TRUSTED_SCHEMA:
		{
			if (!(value is bool))
			{
				throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "configuration value type mismatch, must be of type {0}", typeof(bool)));
			}
			int result = 0;
			return UnsafeNativeMethods.sqlite3_db_config_int_refint(_sql, option, ((bool)value) ? 1 : 0, ref result);
		}
		default:
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.CurrentCulture, "unsupported configuration option {0}", option));
		}
	}

	internal override void SetLoadExtension(bool bOnOff)
	{
		SQLiteErrorCode sQLiteErrorCode = ((SQLiteVersionNumber < 3013000) ? UnsafeNativeMethods.sqlite3_enable_load_extension(_sql, bOnOff ? (-1) : 0) : SetConfigurationOption(SQLiteConfigDbOpsEnum.SQLITE_DBCONFIG_ENABLE_LOAD_EXTENSION, bOnOff));
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override void LoadExtension(string fileName, string procName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		IntPtr pError = IntPtr.Zero;
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(fileName + "\0");
			byte[] procName2 = null;
			if (procName != null)
			{
				procName2 = Encoding.UTF8.GetBytes(procName + "\0");
			}
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_load_extension(_sql, bytes, procName2, ref pError);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, SQLiteConvert.UTF8ToString(pError, -1));
			}
		}
		finally
		{
			if (pError != IntPtr.Zero)
			{
				UnsafeNativeMethods.sqlite3_free(pError);
				pError = IntPtr.Zero;
			}
		}
	}

	internal override void SetExtendedResultCodes(bool bOnOff)
	{
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_extended_result_codes(_sql, bOnOff ? (-1) : 0);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override SQLiteErrorCode ResultCode()
	{
		return UnsafeNativeMethods.sqlite3_errcode(_sql);
	}

	internal override SQLiteErrorCode ExtendedResultCode()
	{
		return UnsafeNativeMethods.sqlite3_extended_errcode(_sql);
	}

	internal override void LogMessage(SQLiteErrorCode iErrCode, string zMessage)
	{
		StaticLogMessage(iErrCode, zMessage);
	}

	internal static void StaticLogMessage(SQLiteErrorCode iErrCode, string zMessage)
	{
		UnsafeNativeMethods.sqlite3_log(iErrCode, SQLiteConvert.ToUTF8(zMessage));
	}

	private static int GetLegacyDatabasePageSize(SQLiteConnection connection, string fileName, byte[] passwordBytes, int? pageSize)
	{
		if (pageSize.HasValue)
		{
			return pageSize.Value;
		}
		using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
		byte[] array = new byte[512];
		int num = array.Length;
		int num2 = fileStream.Read(array, 0, num);
		if (num2 != num)
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "read {0} encrypted page bytes, expected {1} (1)", num2, num));
		}
		byte[] outputBytes = null;
		DecryptLegacyDatabasePage(connection, passwordBytes, array, ref outputBytes);
		if (outputBytes == null)
		{
			throw new SQLiteException("failed to decrypt page (1)");
		}
		int num3 = outputBytes.Length;
		if (num3 != num)
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "got {0} decrypted page bytes, expected {1} (1)", num3, num));
		}
		int num4 = 18;
		if (num3 < num4)
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "got {0} decrypted page bytes, need at least {1}", num3, num4));
		}
		return (outputBytes[16] << 8) | outputBytes[17];
	}

	private static void DecryptLegacyDatabasePage(SQLiteConnection connection, byte[] passwordBytes, byte[] inputBytes, ref byte[] outputBytes)
	{
		using SQLiteCommand sQLiteCommand = connection.CreateCommand();
		sQLiteCommand.CommandText = "SELECT cryptoapi_decrypt(?, ?);";
		SQLiteParameter sQLiteParameter = sQLiteCommand.CreateParameter();
		sQLiteParameter.ParameterName = "dataBlob";
		sQLiteParameter.DbType = DbType.Binary;
		sQLiteParameter.Value = inputBytes;
		sQLiteCommand.Parameters.Add(sQLiteParameter);
		sQLiteParameter = sQLiteCommand.CreateParameter();
		sQLiteParameter.ParameterName = "passwordBlob";
		sQLiteParameter.DbType = DbType.Binary;
		sQLiteParameter.Value = passwordBytes;
		sQLiteCommand.Parameters.Add(sQLiteParameter);
		using SQLiteDataReader sQLiteDataReader = sQLiteCommand.ExecuteReader();
		List<byte> list = null;
		while (sQLiteDataReader.Read())
		{
			if (sQLiteDataReader[0] is byte[] array)
			{
				if (list == null)
				{
					list = new List<byte>(array.Length);
				}
				list.AddRange(array);
			}
		}
		if (list != null)
		{
			outputBytes = list.ToArray();
		}
	}

	private static void DecryptLegacyDatabasePage(SQLiteConnection connection, FileStream inputStream, FileStream outputStream, int pageSize, byte[] passwordBytes, byte[] inputBytes, ref long totalReadCount, ref long totalWriteCount)
	{
		if (inputBytes == null)
		{
			throw new SQLiteException("invalid input buffer");
		}
		int num = inputBytes.Length;
		if (num != pageSize)
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "input buffer is sized {0} bytes, need {1}", num, pageSize));
		}
		Array.Clear(inputBytes, 0, num);
		int num2 = inputStream.Read(inputBytes, 0, pageSize);
		if (num2 != pageSize)
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "read {0} encrypted page bytes, expected {1} (2)", num2, pageSize));
		}
		totalReadCount += num2;
		byte[] outputBytes = null;
		DecryptLegacyDatabasePage(connection, passwordBytes, inputBytes, ref outputBytes);
		if (outputBytes == null)
		{
			throw new SQLiteException("failed to decrypt page (2)");
		}
		int num3 = outputBytes.Length;
		if (num3 != num)
		{
			throw new SQLiteException(HelperMethods.StringFormat(CultureInfo.InvariantCulture, "got {0} decrypted page bytes, expected {1} (2)", num3, num));
		}
		outputStream.Write(outputBytes, 0, num3);
		totalWriteCount += num3;
	}

	internal static string DecryptLegacyDatabase(string fileName, byte[] passwordBytes, int? pageSize, SQLiteProgressEventHandler progress)
	{
		SQLiteExtra.Verify(null);
		if (string.IsNullOrEmpty(fileName))
		{
			throw new SQLiteException("invalid file name");
		}
		if (!File.Exists(fileName))
		{
			throw new SQLiteException("named file does not exist");
		}
		using SQLiteConnection sQLiteConnection = new SQLiteConnection();
		if (progress != null)
		{
			sQLiteConnection.Progress += progress;
		}
		sQLiteConnection.ConnectionString = SQLiteCommand.DefaultConnectionString;
		sQLiteConnection.Open();
		sQLiteConnection.EnableExtensions(enable: true);
		sQLiteConnection.LoadExtension(UnsafeNativeMethods.GetNativeModuleFileName(), "sqlite3_cryptoapi_init");
		int legacyDatabasePageSize = GetLegacyDatabasePageSize(sQLiteConnection, fileName, passwordBytes, pageSize);
		if (legacyDatabasePageSize == 0)
		{
			throw new SQLiteException("page size cannot be zero");
		}
		if (legacyDatabasePageSize < 512 || legacyDatabasePageSize > 65536)
		{
			throw new SQLiteException("page size out-of-range");
		}
		if (((uint)legacyDatabasePageSize & (true ? 1u : 0u)) != 0)
		{
			throw new SQLiteException("page size is odd");
		}
		long length = new FileInfo(fileName).Length;
		if (length % legacyDatabasePageSize != 0L)
		{
			throw new SQLiteException("data not multiple of page size");
		}
		using FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
		string text = string.Format("{0}-decrypted-{1}-{2}", fileName, DateTime.UtcNow.ToString("yyyy_MM_dd"), Environment.TickCount.ToString("X"));
		using (FileStream outputStream = new FileStream(text, FileMode.CreateNew, FileAccess.Write, FileShare.None))
		{
			byte[] inputBytes = new byte[legacyDatabasePageSize];
			long totalReadCount = 0L;
			long totalWriteCount = 0L;
			while (totalReadCount < length)
			{
				DecryptLegacyDatabasePage(sQLiteConnection, inputStream, outputStream, legacyDatabasePageSize, passwordBytes, inputBytes, ref totalReadCount, ref totalWriteCount);
			}
			if (totalReadCount != length)
			{
				throw new SQLiteException("encrypted data was not totally read");
			}
			if (totalWriteCount != totalReadCount)
			{
				throw new SQLiteException("decrypted data was not totally written");
			}
		}
		return text;
	}

	private static void ZeroPassword(byte[] passwordBytes)
	{
		if (passwordBytes != null)
		{
			for (int i = 0; i < passwordBytes.Length; i++)
			{
				byte b = (passwordBytes[i] = (byte)((i + 1) % 255));
				passwordBytes[i] ^= b;
			}
		}
	}

	private static byte[] CalculatePoolHash(string fileName, byte[] passwordBytes, bool asText)
	{
		try
		{
			using SHA512 sHA = SHA512.Create();
			sHA.Initialize();
			byte[] bytes;
			if (fileName != null)
			{
				bytes = Encoding.Unicode.GetBytes(fileName);
				sHA.TransformBlock(bytes, 0, bytes.Length, null, 0);
			}
			if (passwordBytes != null)
			{
				sHA.TransformBlock(passwordBytes, 0, passwordBytes.Length, null, 0);
			}
			bytes = BitConverter.GetBytes(asText);
			sHA.TransformFinalBlock(bytes, 0, bytes.Length);
			return sHA.Hash;
		}
		catch (Exception)
		{
		}
		return null;
	}

	private static string CalculatePoolFileName(string fileName, byte[] poolHash)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return null;
		}
		if (poolHash == null)
		{
			return null;
		}
		string text = "SQLITE_POOL_HASH:";
		string text2 = HelperMethods.StringFormat(CultureInfo.InvariantCulture, ":{0}", Convert.ToBase64String(poolHash));
		if (fileName.StartsWith(text, StringComparison.Ordinal))
		{
			return null;
		}
		return HelperMethods.StringFormat(CultureInfo.InvariantCulture, "{0}:{1}{2}", text, fileName, text2);
	}

	private bool TryToUsePool(int maxPoolSize, string fileName, byte[] passwordBytes, bool asText, ref string returnToFileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return false;
		}
		byte[] array = CalculatePoolHash(fileName, passwordBytes, asText);
		if (array == null)
		{
			return false;
		}
		string text = CalculatePoolFileName(fileName, array);
		if (text == null)
		{
			return false;
		}
		bool flag = false;
		SQLiteConnectionHandle sQLiteConnectionHandle = null;
		try
		{
			sQLiteConnectionHandle = SQLiteConnectionPool.Remove(text, maxPoolSize, out var version);
			if (sQLiteConnectionHandle != null)
			{
				SQLiteConnectionPool.ClearPool(fileName);
				SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.OpenedFromPool, null, null, null, null, sQLiteConnectionHandle, text, new object[1] { fileName }));
				if (_sql != null)
				{
					UnhookNativeCallbacks(includeGlobal: false, canThrow: true);
					_sql.Dispose();
					_sql = null;
					FreeDbName(canThrow: true);
				}
				_fileName = text;
				_sql = sQLiteConnectionHandle;
				_poolVersion = version;
				flag = true;
			}
		}
		finally
		{
			if (!flag && sQLiteConnectionHandle != null)
			{
				sQLiteConnectionHandle.Dispose();
				sQLiteConnectionHandle = null;
			}
		}
		returnToFileName = text;
		return flag;
	}

	internal override void SetPassword(byte[] passwordBytes, bool asText)
	{
		string returnToFileName = null;
		if (_usePool && TryToUsePool(_maxPoolSize, _fileName, passwordBytes, asText, ref returnToFileName))
		{
			if (returnToFileName != null)
			{
				_returnToFileName = returnToFileName;
			}
			_returnToPool = true;
			return;
		}
		SQLiteExtra.Verify(null);
		int keylen = (asText ? (-1) : ((passwordBytes != null) ? passwordBytes.Length : 0));
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_key(_sql, passwordBytes, keylen);
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.HidePassword))
		{
			ZeroPassword(passwordBytes);
		}
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
		if (_usePool)
		{
			if (returnToFileName != null)
			{
				_returnToFileName = returnToFileName;
			}
			_usePool = false;
			_returnToPool = true;
		}
	}

	internal override void ChangePassword(byte[] newPasswordBytes, bool asText)
	{
		SQLiteExtra.Verify(null);
		int keylen = (asText ? (-1) : ((newPasswordBytes != null) ? newPasswordBytes.Length : 0));
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_rekey(_sql, newPasswordBytes, keylen);
		if (HelperMethods.HasFlags(_flags, SQLiteConnectionFlags.HidePassword))
		{
			ZeroPassword(newPasswordBytes);
		}
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
		if (_usePool)
		{
			_returnToFileName = _fileName;
			_usePool = false;
			_returnToPool = false;
		}
	}

	internal override void SetBusyHook(SQLiteBusyCallback func)
	{
		UnsafeNativeMethods.sqlite3_busy_handler(_sql, func, IntPtr.Zero);
	}

	internal override void SetProgressHook(int nOps, SQLiteProgressCallback func)
	{
		UnsafeNativeMethods.sqlite3_progress_handler(_sql, nOps, func, IntPtr.Zero);
	}

	internal override void SetAuthorizerHook(SQLiteAuthorizerCallback func)
	{
		UnsafeNativeMethods.sqlite3_set_authorizer(_sql, func, IntPtr.Zero);
	}

	internal override void SetUpdateHook(SQLiteUpdateCallback func)
	{
		UnsafeNativeMethods.sqlite3_update_hook(_sql, func, IntPtr.Zero);
	}

	internal override void SetCommitHook(SQLiteCommitCallback func)
	{
		UnsafeNativeMethods.sqlite3_commit_hook(_sql, func, IntPtr.Zero);
	}

	internal override void SetTraceCallback(SQLiteTraceCallback func)
	{
		UnsafeNativeMethods.sqlite3_trace(_sql, func, IntPtr.Zero);
	}

	internal override void SetTraceCallback2(SQLiteTraceFlags mask, SQLiteTraceCallback2 func)
	{
		UnsafeNativeMethods.sqlite3_trace_v2(_sql, mask, func, IntPtr.Zero);
	}

	internal override void SetRollbackHook(SQLiteRollbackCallback func)
	{
		UnsafeNativeMethods.sqlite3_rollback_hook(_sql, func, IntPtr.Zero);
	}

	internal override SQLiteErrorCode SetLogCallback(SQLiteLogCallback func)
	{
		SQLiteErrorCode num = UnsafeNativeMethods.sqlite3_config_log(SQLiteConfigOpsEnum.SQLITE_CONFIG_LOG, func, IntPtr.Zero);
		if (num == SQLiteErrorCode.Ok)
		{
			_setLogCallback = func != null;
		}
		return num;
	}

	private static void AppendError(StringBuilder builder, string message)
	{
		builder?.AppendLine(message);
	}

	private bool UnhookNativeCallbacks(bool includeGlobal, bool canThrow)
	{
		bool flag = true;
		SQLiteErrorCode errorCode = SQLiteErrorCode.Ok;
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			SetRollbackHook(null);
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset rollback hook");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		try
		{
			if (UnsafeNativeMethods.sqlite3_libversion_number() >= 3014000)
			{
				SetTraceCallback2(SQLiteTraceFlags.SQLITE_TRACE_NONE, null);
			}
			else
			{
				SetTraceCallback(null);
			}
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset trace callback");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		try
		{
			SetCommitHook(null);
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset commit hook");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		try
		{
			SetUpdateHook(null);
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset update hook");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		try
		{
			SetAuthorizerHook(null);
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset authorizer hook");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		try
		{
			SetBusyHook(null);
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset busy hook");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		try
		{
			SetProgressHook(0, null);
		}
		catch (Exception)
		{
			AppendError(stringBuilder, "failed to unset progress hook");
			errorCode = SQLiteErrorCode.Error;
			flag = false;
		}
		if (includeGlobal && _setLogCallback)
		{
			try
			{
				SQLiteErrorCode sQLiteErrorCode = SetLogCallback(null);
				if (sQLiteErrorCode != 0)
				{
					AppendError(stringBuilder, "could not unset log callback");
					errorCode = sQLiteErrorCode;
					flag = false;
				}
			}
			catch (Exception)
			{
				AppendError(stringBuilder, "failed to unset log callback");
				errorCode = SQLiteErrorCode.Error;
				flag = false;
			}
		}
		if (!flag && canThrow)
		{
			throw new SQLiteException(errorCode, stringBuilder.ToString());
		}
		return flag;
	}

	private bool FreeDbName(bool canThrow)
	{
		try
		{
			if (dbName != IntPtr.Zero)
			{
				SQLiteMemory.Free(dbName);
				dbName = IntPtr.Zero;
			}
			return true;
		}
		catch (Exception)
		{
			if (canThrow)
			{
				throw;
			}
		}
		return false;
	}

	internal override SQLiteBackup InitializeBackup(SQLiteConnection destCnn, string destName, string sourceName)
	{
		if (destCnn == null)
		{
			throw new ArgumentNullException("destCnn");
		}
		if (destName == null)
		{
			throw new ArgumentNullException("destName");
		}
		if (sourceName == null)
		{
			throw new ArgumentNullException("sourceName");
		}
		SQLiteConnectionHandle sql = ((destCnn._sql as SQLite3) ?? throw new ArgumentException("Destination connection has no wrapper.", "destCnn"))._sql;
		if (sql == null)
		{
			throw new ArgumentException("Destination connection has an invalid handle.", "destCnn");
		}
		SQLiteConnectionHandle sql2 = _sql;
		if (sql2 == null)
		{
			throw new InvalidOperationException("Source connection has an invalid handle.");
		}
		byte[] zDestName = SQLiteConvert.ToUTF8(destName);
		byte[] zSourceName = SQLiteConvert.ToUTF8(sourceName);
		SQLiteBackupHandle sQLiteBackupHandle = null;
		try
		{
		}
		finally
		{
			IntPtr intPtr = UnsafeNativeMethods.sqlite3_backup_init(sql, zDestName, sql2, zSourceName);
			if (intPtr == IntPtr.Zero)
			{
				SQLiteErrorCode sQLiteErrorCode = ResultCode();
				if (sQLiteErrorCode != 0)
				{
					throw new SQLiteException(sQLiteErrorCode, GetLastError());
				}
				throw new SQLiteException("failed to initialize backup");
			}
			sQLiteBackupHandle = new SQLiteBackupHandle(sql, intPtr);
		}
		SQLiteConnection.OnChanged(null, new ConnectionEventArgs(SQLiteConnectionEventType.NewCriticalHandle, null, null, null, null, sQLiteBackupHandle, null, new object[4]
		{
			typeof(SQLite3),
			destCnn,
			destName,
			sourceName
		}));
		return new SQLiteBackup(this, sQLiteBackupHandle, sql, zDestName, sql2, zSourceName);
	}

	internal override bool StepBackup(SQLiteBackup backup, int nPage, ref bool retry)
	{
		retry = false;
		if (backup == null)
		{
			throw new ArgumentNullException("backup");
		}
		IntPtr intPtr = backup._sqlite_backup ?? throw new InvalidOperationException("Backup object has an invalid handle.");
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("Backup object has an invalid handle pointer.");
		}
		SQLiteErrorCode sQLiteErrorCode = (backup._stepResult = UnsafeNativeMethods.sqlite3_backup_step(intPtr, nPage));
		switch (sQLiteErrorCode)
		{
		case SQLiteErrorCode.Ok:
			return true;
		case SQLiteErrorCode.Busy:
			retry = true;
			return true;
		case SQLiteErrorCode.Locked:
			retry = true;
			return true;
		case SQLiteErrorCode.Done:
			return false;
		default:
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override int RemainingBackup(SQLiteBackup backup)
	{
		if (backup == null)
		{
			throw new ArgumentNullException("backup");
		}
		IntPtr intPtr = backup._sqlite_backup ?? throw new InvalidOperationException("Backup object has an invalid handle.");
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("Backup object has an invalid handle pointer.");
		}
		return UnsafeNativeMethods.sqlite3_backup_remaining(intPtr);
	}

	internal override int PageCountBackup(SQLiteBackup backup)
	{
		if (backup == null)
		{
			throw new ArgumentNullException("backup");
		}
		IntPtr intPtr = backup._sqlite_backup ?? throw new InvalidOperationException("Backup object has an invalid handle.");
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("Backup object has an invalid handle pointer.");
		}
		return UnsafeNativeMethods.sqlite3_backup_pagecount(intPtr);
	}

	internal override void FinishBackup(SQLiteBackup backup)
	{
		if (backup == null)
		{
			throw new ArgumentNullException("backup");
		}
		SQLiteBackupHandle obj = backup._sqlite_backup ?? throw new InvalidOperationException("Backup object has an invalid handle.");
		IntPtr intPtr = obj;
		if (intPtr == IntPtr.Zero)
		{
			throw new InvalidOperationException("Backup object has an invalid handle pointer.");
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_backup_finish_interop(intPtr);
		obj.SetHandleAsInvalid();
		if (sQLiteErrorCode != 0 && sQLiteErrorCode != backup._stepResult)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
	}

	internal override bool IsInitialized()
	{
		return StaticIsInitialized();
	}

	internal static bool StaticIsInitialized()
	{
		lock (syncRoot)
		{
			bool internalEnabled = SQLiteLog.InternalEnabled;
			SQLiteLog.InternalEnabled = false;
			try
			{
				return UnsafeNativeMethods.sqlite3_config_none(SQLiteConfigOpsEnum.SQLITE_CONFIG_NONE) == SQLiteErrorCode.Misuse;
			}
			finally
			{
				SQLiteLog.InternalEnabled = internalEnabled;
			}
		}
	}

	internal override object GetValue(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, SQLiteType typ)
	{
		TypeAffinity typeAffinity = typ.Affinity;
		if (typeAffinity == TypeAffinity.Null)
		{
			return DBNull.Value;
		}
		Type type = null;
		if (typ.Type != DbType.Object)
		{
			type = SQLiteConvert.SQLiteTypeToType(typ);
			typeAffinity = SQLiteConvert.TypeToAffinity(type, flags);
		}
		if (HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetAllAsText))
		{
			return GetText(stmt, index);
		}
		switch (typeAffinity)
		{
		case TypeAffinity.Blob:
		{
			if (typ.Type == DbType.Guid && typ.Affinity == TypeAffinity.Text)
			{
				return new Guid(GetText(stmt, index));
			}
			int num = (int)GetBytes(stmt, index, 0, null, 0, 0);
			byte[] array = new byte[num];
			GetBytes(stmt, index, 0, array, 0, num);
			if (typ.Type == DbType.Guid && num == 16)
			{
				return new Guid(array);
			}
			return array;
		}
		case TypeAffinity.DateTime:
			return GetDateTime(stmt, index);
		case TypeAffinity.Double:
			if (type == null)
			{
				return GetDouble(stmt, index);
			}
			return Convert.ChangeType(GetDouble(stmt, index), type, HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetInvariantDouble) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
		case TypeAffinity.Int64:
			if (type == null)
			{
				return GetInt64(stmt, index);
			}
			if (type == typeof(bool))
			{
				return GetBoolean(stmt, index);
			}
			if (type == typeof(sbyte))
			{
				return GetSByte(stmt, index);
			}
			if (type == typeof(byte))
			{
				return GetByte(stmt, index);
			}
			if (type == typeof(short))
			{
				return GetInt16(stmt, index);
			}
			if (type == typeof(ushort))
			{
				return GetUInt16(stmt, index);
			}
			if (type == typeof(int))
			{
				return GetInt32(stmt, index);
			}
			if (type == typeof(uint))
			{
				return GetUInt32(stmt, index);
			}
			if (type == typeof(long))
			{
				return GetInt64(stmt, index);
			}
			if (type == typeof(ulong))
			{
				return GetUInt64(stmt, index);
			}
			return Convert.ChangeType(GetInt64(stmt, index), type, HelperMethods.HasFlags(flags, SQLiteConnectionFlags.GetInvariantInt64) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
		default:
			return GetText(stmt, index);
		}
	}

	internal override int GetCursorForTable(SQLiteStatement stmt, int db, int rootPage)
	{
		return UnsafeNativeMethods.sqlite3_table_cursor_interop(stmt._sqlite_stmt, db, rootPage);
	}

	internal override long GetRowIdForCursor(SQLiteStatement stmt, int cursor)
	{
		long rowid = 0L;
		if (UnsafeNativeMethods.sqlite3_cursor_rowid_interop(stmt._sqlite_stmt, cursor, ref rowid) == SQLiteErrorCode.Ok)
		{
			return rowid;
		}
		return 0L;
	}

	internal override void GetIndexColumnExtendedInfo(string database, string index, string column, ref int sortMode, ref int onError, ref string collationSequence)
	{
		IntPtr Collation = IntPtr.Zero;
		int colllen = 0;
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_index_column_info_interop(_sql, SQLiteConvert.ToUTF8(database), SQLiteConvert.ToUTF8(index), SQLiteConvert.ToUTF8(column), ref sortMode, ref onError, ref Collation, ref colllen);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, null);
		}
		collationSequence = SQLiteConvert.UTF8ToString(Collation, colllen);
	}

	internal override SQLiteErrorCode FileControl(string zDbName, int op, IntPtr pArg)
	{
		return UnsafeNativeMethods.sqlite3_file_control(_sql, (zDbName != null) ? SQLiteConvert.ToUTF8(zDbName) : null, op, pArg);
	}
}
