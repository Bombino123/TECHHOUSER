using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Data.SQLite;

internal sealed class SQLite3_UTF16 : SQLite3
{
	private bool disposed;

	internal SQLite3_UTF16(SQLiteDateFormats fmt, DateTimeKind kind, string fmtString, IntPtr db, string fileName, bool ownHandle)
		: base(fmt, kind, fmtString, db, fileName, ownHandle)
	{
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLite3_UTF16).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			_ = disposed;
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}

	public override string ToString(IntPtr b, int nbytelen)
	{
		CheckDisposed();
		return UTF16ToString(b, nbytelen);
	}

	public static string UTF16ToString(IntPtr b, int nbytelen)
	{
		if (nbytelen == 0 || b == IntPtr.Zero)
		{
			return string.Empty;
		}
		if (nbytelen == -1)
		{
			return Marshal.PtrToStringUni(b);
		}
		return Marshal.PtrToStringUni(b, nbytelen / 2);
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
		_usePool = usePool;
		if (_usePool && !SQLite3.IsAllowedToUsePool(openFlags))
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
				typeof(SQLite3_UTF16),
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
				SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_open16_interop(SQLiteConvert.ToUTF8(strFilename), SQLiteConvert.ToUTF8(vfsName), openFlags, num, ref zero);
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
				typeof(SQLite3_UTF16),
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

	internal override void Bind_DateTime(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, DateTime dt)
	{
		switch (_datetimeFormat)
		{
		case SQLiteDateFormats.Ticks:
		case SQLiteDateFormats.JulianDay:
		case SQLiteDateFormats.UnixEpoch:
			base.Bind_DateTime(stmt, flags, index, dt);
			return;
		}
		if (HelperMethods.LogBind(flags))
		{
			SQLite3.LogBind(stmt?._sqlite_stmt, index, dt);
		}
		Bind_Text(stmt, flags, index, ToString(dt));
	}

	internal override void Bind_Text(SQLiteStatement stmt, SQLiteConnectionFlags flags, int index, string value)
	{
		SQLiteStatementHandle sqlite_stmt = stmt._sqlite_stmt;
		if (HelperMethods.LogBind(flags))
		{
			SQLite3.LogBind(sqlite_stmt, index, value);
		}
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3_bind_text16(sqlite_stmt, index, value, value.Length * 2, (IntPtr)(-1));
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, GetLastError());
		}
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
		return ToDateTime(GetText(stmt, index));
	}

	internal override string ColumnName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_name16_interop(stmt._sqlite_stmt, index, ref len);
		if (intPtr == IntPtr.Zero)
		{
			throw new SQLiteException(SQLiteErrorCode.NoMem, GetLastError());
		}
		return UTF16ToString(intPtr, len);
	}

	internal override string ColumnType(SQLiteStatement stmt, int index, ref TypeAffinity nAffinity)
	{
		int len = 0;
		IntPtr intPtr = UnsafeNativeMethods.sqlite3_column_decltype16_interop(stmt._sqlite_stmt, index, ref len);
		nAffinity = ColumnAffinity(stmt, index);
		if (intPtr != IntPtr.Zero && (len > 0 || len == -1))
		{
			string text = UTF16ToString(intPtr, len);
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

	internal override string GetText(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override string ColumnOriginalName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override string ColumnDatabaseName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override string ColumnTableName(SQLiteStatement stmt, int index)
	{
		int len = 0;
		return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16_interop(stmt._sqlite_stmt, index, ref len), len);
	}

	internal override string GetParamValueText(IntPtr ptr)
	{
		int len = 0;
		return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16_interop(ptr, ref len), len);
	}

	internal override void ReturnError(IntPtr context, string value)
	{
		UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length * 2);
	}

	internal override void ReturnText(IntPtr context, string value)
	{
		UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length * 2, (IntPtr)(-1));
	}
}
