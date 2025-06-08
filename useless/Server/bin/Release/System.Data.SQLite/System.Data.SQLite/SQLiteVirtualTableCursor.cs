namespace System.Data.SQLite;

public class SQLiteVirtualTableCursor : ISQLiteNativeHandle, IDisposable
{
	protected static readonly int InvalidRowIndex;

	private int rowIndex;

	private SQLiteVirtualTable table;

	private int indexNumber;

	private string indexString;

	private SQLiteValue[] values;

	private IntPtr nativeHandle;

	private bool disposed;

	public virtual SQLiteVirtualTable Table
	{
		get
		{
			CheckDisposed();
			return table;
		}
	}

	public virtual int IndexNumber
	{
		get
		{
			CheckDisposed();
			return indexNumber;
		}
	}

	public virtual string IndexString
	{
		get
		{
			CheckDisposed();
			return indexString;
		}
	}

	public virtual SQLiteValue[] Values
	{
		get
		{
			CheckDisposed();
			return values;
		}
	}

	public virtual IntPtr NativeHandle
	{
		get
		{
			CheckDisposed();
			return nativeHandle;
		}
		internal set
		{
			nativeHandle = value;
		}
	}

	public SQLiteVirtualTableCursor(SQLiteVirtualTable table)
		: this()
	{
		this.table = table;
	}

	private SQLiteVirtualTableCursor()
	{
		rowIndex = InvalidRowIndex;
	}

	protected virtual int TryPersistValues(SQLiteValue[] values)
	{
		int num = 0;
		if (values != null)
		{
			foreach (SQLiteValue sQLiteValue in values)
			{
				if (sQLiteValue != null && sQLiteValue.Persist())
				{
					num++;
				}
			}
		}
		return num;
	}

	public virtual void Filter(int indexNumber, string indexString, SQLiteValue[] values)
	{
		CheckDisposed();
		if (values != null && TryPersistValues(values) != values.Length)
		{
			throw new SQLiteException("failed to persist one or more values");
		}
		this.indexNumber = indexNumber;
		this.indexString = indexString;
		this.values = values;
	}

	public virtual int GetRowIndex()
	{
		return rowIndex;
	}

	public virtual void NextRowIndex()
	{
		rowIndex++;
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
			throw new ObjectDisposedException(typeof(SQLiteVirtualTableCursor).Name);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
		}
	}

	~SQLiteVirtualTableCursor()
	{
		Dispose(disposing: false);
	}
}
