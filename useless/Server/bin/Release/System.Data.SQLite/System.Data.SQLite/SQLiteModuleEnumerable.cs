using System.Collections;
using System.Globalization;

namespace System.Data.SQLite;

public class SQLiteModuleEnumerable : SQLiteModuleCommon
{
	private IEnumerable enumerable;

	private bool objectIdentity;

	private bool disposed;

	public SQLiteModuleEnumerable(string name, IEnumerable enumerable)
		: this(name, enumerable, objectIdentity: false)
	{
	}

	public SQLiteModuleEnumerable(string name, IEnumerable enumerable, bool objectIdentity)
		: base(name)
	{
		if (enumerable == null)
		{
			throw new ArgumentNullException("enumerable");
		}
		this.enumerable = enumerable;
		this.objectIdentity = objectIdentity;
	}

	protected virtual SQLiteErrorCode CursorEndOfEnumeratorError(SQLiteVirtualTableCursor cursor)
	{
		SetCursorError(cursor, "already hit end of enumerator");
		return SQLiteErrorCode.Error;
	}

	public override SQLiteErrorCode Create(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
	{
		CheckDisposed();
		if (DeclareTable(connection, GetSqlForDeclareTable(), ref error) == SQLiteErrorCode.Ok)
		{
			table = new SQLiteVirtualTable(arguments);
			return SQLiteErrorCode.Ok;
		}
		return SQLiteErrorCode.Error;
	}

	public override SQLiteErrorCode Connect(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
	{
		CheckDisposed();
		if (DeclareTable(connection, GetSqlForDeclareTable(), ref error) == SQLiteErrorCode.Ok)
		{
			table = new SQLiteVirtualTable(arguments);
			return SQLiteErrorCode.Ok;
		}
		return SQLiteErrorCode.Error;
	}

	public override SQLiteErrorCode BestIndex(SQLiteVirtualTable table, SQLiteIndex index)
	{
		CheckDisposed();
		if (!table.BestIndex(index))
		{
			SetTableError(table, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "failed to select best index for virtual table \"{0}\"", table.TableName));
			return SQLiteErrorCode.Error;
		}
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Disconnect(SQLiteVirtualTable table)
	{
		CheckDisposed();
		table.Dispose();
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Destroy(SQLiteVirtualTable table)
	{
		CheckDisposed();
		table.Dispose();
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		cursor = new SQLiteVirtualTableCursorEnumerator(table, enumerable.GetEnumerator());
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Close(SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		if (!(cursor is SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator))
		{
			return CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
		}
		sQLiteVirtualTableCursorEnumerator.Close();
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Filter(SQLiteVirtualTableCursor cursor, int indexNumber, string indexString, SQLiteValue[] values)
	{
		CheckDisposed();
		if (!(cursor is SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator))
		{
			return CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
		}
		sQLiteVirtualTableCursorEnumerator.Filter(indexNumber, indexString, values);
		sQLiteVirtualTableCursorEnumerator.Reset();
		sQLiteVirtualTableCursorEnumerator.MoveNext();
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Next(SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		if (!(cursor is SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator))
		{
			return CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
		}
		if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
		{
			return CursorEndOfEnumeratorError(cursor);
		}
		sQLiteVirtualTableCursorEnumerator.MoveNext();
		return SQLiteErrorCode.Ok;
	}

	public override bool Eof(SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		if (!(cursor is SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator))
		{
			return ResultCodeToEofResult(CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator)));
		}
		return sQLiteVirtualTableCursorEnumerator.EndOfEnumerator;
	}

	public override SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index)
	{
		CheckDisposed();
		if (!(cursor is SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator))
		{
			return CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
		}
		if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
		{
			return CursorEndOfEnumeratorError(cursor);
		}
		object current = sQLiteVirtualTableCursorEnumerator.Current;
		if (current != null)
		{
			context.SetString(GetStringFromObject(cursor, current));
		}
		else
		{
			context.SetNull();
		}
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode RowId(SQLiteVirtualTableCursor cursor, ref long rowId)
	{
		CheckDisposed();
		if (!(cursor is SQLiteVirtualTableCursorEnumerator sQLiteVirtualTableCursorEnumerator))
		{
			return CursorTypeMismatchError(cursor, typeof(SQLiteVirtualTableCursorEnumerator));
		}
		if (sQLiteVirtualTableCursorEnumerator.EndOfEnumerator)
		{
			return CursorEndOfEnumeratorError(cursor);
		}
		object current = sQLiteVirtualTableCursorEnumerator.Current;
		rowId = GetRowIdFromObject(cursor, current);
		return SQLiteErrorCode.Ok;
	}

	public override SQLiteErrorCode Update(SQLiteVirtualTable table, SQLiteValue[] values, ref long rowId)
	{
		CheckDisposed();
		SetTableError(table, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "virtual table \"{0}\" is read-only", table.TableName));
		return SQLiteErrorCode.Error;
	}

	public override SQLiteErrorCode Rename(SQLiteVirtualTable table, string newName)
	{
		CheckDisposed();
		if (!table.Rename(newName))
		{
			SetTableError(table, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "failed to rename virtual table from \"{0}\" to \"{1}\"", table.TableName, newName));
			return SQLiteErrorCode.Error;
		}
		return SQLiteErrorCode.Ok;
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteModuleEnumerable).Name);
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
}
