namespace System.Data.SQLite;

internal sealed class SQLiteChangeSetMetadataItem : ISQLiteChangeSetMetadataItem, IDisposable
{
	private SQLiteChangeSetIterator iterator;

	private string tableName;

	private int? numberOfColumns;

	private SQLiteAuthorizerActionCode? operationCode;

	private bool? indirect;

	private bool[] primaryKeyColumns;

	private int? numberOfForeignKeyConflicts;

	private bool disposed;

	public string TableName
	{
		get
		{
			CheckDisposed();
			PopulateOperationMetadata();
			return tableName;
		}
	}

	public int NumberOfColumns
	{
		get
		{
			CheckDisposed();
			PopulateOperationMetadata();
			return numberOfColumns.Value;
		}
	}

	public SQLiteAuthorizerActionCode OperationCode
	{
		get
		{
			CheckDisposed();
			PopulateOperationMetadata();
			return operationCode.Value;
		}
	}

	public bool Indirect
	{
		get
		{
			CheckDisposed();
			PopulateOperationMetadata();
			return indirect.Value;
		}
	}

	public bool[] PrimaryKeyColumns
	{
		get
		{
			CheckDisposed();
			PopulatePrimaryKeyColumns();
			return primaryKeyColumns;
		}
	}

	public int NumberOfForeignKeyConflicts
	{
		get
		{
			CheckDisposed();
			PopulateNumberOfForeignKeyConflicts();
			return numberOfForeignKeyConflicts.Value;
		}
	}

	public SQLiteChangeSetMetadataItem(SQLiteChangeSetIterator iterator)
	{
		this.iterator = iterator;
	}

	private void CheckIterator()
	{
		if (iterator == null)
		{
			throw new InvalidOperationException("iterator unavailable");
		}
		iterator.CheckHandle();
	}

	private void PopulateOperationMetadata()
	{
		if (tableName == null || !numberOfColumns.HasValue || !operationCode.HasValue || !indirect.HasValue)
		{
			CheckIterator();
			IntPtr pTblName = IntPtr.Zero;
			SQLiteAuthorizerActionCode op = SQLiteAuthorizerActionCode.None;
			int bIndirect = 0;
			int nColumns = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_op(iterator.GetIntPtr(), ref pTblName, ref nColumns, ref op, ref bIndirect);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_op");
			}
			tableName = SQLiteString.StringFromUtf8IntPtr(pTblName);
			numberOfColumns = nColumns;
			operationCode = op;
			indirect = bIndirect != 0;
		}
	}

	private void PopulatePrimaryKeyColumns()
	{
		if (primaryKeyColumns != null)
		{
			return;
		}
		CheckIterator();
		IntPtr pPrimaryKeys = IntPtr.Zero;
		int nColumns = 0;
		SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_pk(iterator.GetIntPtr(), ref pPrimaryKeys, ref nColumns);
		if (sQLiteErrorCode != 0)
		{
			throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_pk");
		}
		byte[] array = SQLiteBytes.FromIntPtr(pPrimaryKeys, nColumns);
		if (array != null)
		{
			primaryKeyColumns = new bool[nColumns];
			for (int i = 0; i < array.Length; i++)
			{
				primaryKeyColumns[i] = array[i] != 0;
			}
		}
	}

	private void PopulateNumberOfForeignKeyConflicts()
	{
		if (!numberOfForeignKeyConflicts.HasValue)
		{
			CheckIterator();
			int conflicts = 0;
			SQLiteErrorCode sQLiteErrorCode = UnsafeNativeMethods.sqlite3changeset_fk_conflicts(iterator.GetIntPtr(), ref conflicts);
			if (sQLiteErrorCode != 0)
			{
				throw new SQLiteException(sQLiteErrorCode, "sqlite3changeset_fk_conflicts");
			}
			numberOfForeignKeyConflicts = conflicts;
		}
	}

	public SQLiteValue GetOldValue(int columnIndex)
	{
		CheckDisposed();
		CheckIterator();
		IntPtr pValue = IntPtr.Zero;
		UnsafeNativeMethods.sqlite3changeset_old(iterator.GetIntPtr(), columnIndex, ref pValue);
		return SQLiteValue.FromIntPtr(pValue);
	}

	public SQLiteValue GetNewValue(int columnIndex)
	{
		CheckDisposed();
		CheckIterator();
		IntPtr pValue = IntPtr.Zero;
		UnsafeNativeMethods.sqlite3changeset_new(iterator.GetIntPtr(), columnIndex, ref pValue);
		return SQLiteValue.FromIntPtr(pValue);
	}

	public SQLiteValue GetConflictValue(int columnIndex)
	{
		CheckDisposed();
		CheckIterator();
		IntPtr pValue = IntPtr.Zero;
		UnsafeNativeMethods.sqlite3changeset_conflict(iterator.GetIntPtr(), columnIndex, ref pValue);
		return SQLiteValue.FromIntPtr(pValue);
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
			throw new ObjectDisposedException(typeof(SQLiteChangeSetMetadataItem).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing && iterator != null)
			{
				iterator = null;
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteChangeSetMetadataItem()
	{
		Dispose(disposing: false);
	}
}
