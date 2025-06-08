using System.Collections.Generic;

namespace System.Data.SQLite;

public class SQLiteModuleNoop : SQLiteModule
{
	private Dictionary<string, SQLiteErrorCode> resultCodes;

	private bool disposed;

	public SQLiteModuleNoop(string name)
		: base(name)
	{
		resultCodes = new Dictionary<string, SQLiteErrorCode>();
	}

	protected virtual SQLiteErrorCode GetDefaultResultCode()
	{
		return SQLiteErrorCode.Ok;
	}

	protected virtual bool ResultCodeToEofResult(SQLiteErrorCode resultCode)
	{
		if (resultCode != 0)
		{
			return true;
		}
		return false;
	}

	protected virtual bool ResultCodeToFindFunctionResult(SQLiteErrorCode resultCode)
	{
		if (resultCode != 0)
		{
			return false;
		}
		return true;
	}

	protected virtual SQLiteErrorCode GetMethodResultCode(string methodName)
	{
		if (methodName == null || resultCodes == null)
		{
			return GetDefaultResultCode();
		}
		if (resultCodes != null && resultCodes.TryGetValue(methodName, out var value))
		{
			return value;
		}
		return GetDefaultResultCode();
	}

	protected virtual bool SetMethodResultCode(string methodName, SQLiteErrorCode resultCode)
	{
		if (methodName == null || resultCodes == null)
		{
			return false;
		}
		resultCodes[methodName] = resultCode;
		return true;
	}

	public override SQLiteErrorCode Create(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
	{
		CheckDisposed();
		return GetMethodResultCode("Create");
	}

	public override SQLiteErrorCode Connect(SQLiteConnection connection, IntPtr pClientData, string[] arguments, ref SQLiteVirtualTable table, ref string error)
	{
		CheckDisposed();
		return GetMethodResultCode("Connect");
	}

	public override SQLiteErrorCode BestIndex(SQLiteVirtualTable table, SQLiteIndex index)
	{
		CheckDisposed();
		return GetMethodResultCode("BestIndex");
	}

	public override SQLiteErrorCode Disconnect(SQLiteVirtualTable table)
	{
		CheckDisposed();
		return GetMethodResultCode("Disconnect");
	}

	public override SQLiteErrorCode Destroy(SQLiteVirtualTable table)
	{
		CheckDisposed();
		return GetMethodResultCode("Destroy");
	}

	public override SQLiteErrorCode Open(SQLiteVirtualTable table, ref SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		return GetMethodResultCode("Open");
	}

	public override SQLiteErrorCode Close(SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		return GetMethodResultCode("Close");
	}

	public override SQLiteErrorCode Filter(SQLiteVirtualTableCursor cursor, int indexNumber, string indexString, SQLiteValue[] values)
	{
		CheckDisposed();
		return GetMethodResultCode("Filter");
	}

	public override SQLiteErrorCode Next(SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		return GetMethodResultCode("Next");
	}

	public override bool Eof(SQLiteVirtualTableCursor cursor)
	{
		CheckDisposed();
		return ResultCodeToEofResult(GetMethodResultCode("Eof"));
	}

	public override SQLiteErrorCode Column(SQLiteVirtualTableCursor cursor, SQLiteContext context, int index)
	{
		CheckDisposed();
		return GetMethodResultCode("Column");
	}

	public override SQLiteErrorCode RowId(SQLiteVirtualTableCursor cursor, ref long rowId)
	{
		CheckDisposed();
		return GetMethodResultCode("RowId");
	}

	public override SQLiteErrorCode Update(SQLiteVirtualTable table, SQLiteValue[] values, ref long rowId)
	{
		CheckDisposed();
		return GetMethodResultCode("Update");
	}

	public override SQLiteErrorCode Begin(SQLiteVirtualTable table)
	{
		CheckDisposed();
		return GetMethodResultCode("Begin");
	}

	public override SQLiteErrorCode Sync(SQLiteVirtualTable table)
	{
		CheckDisposed();
		return GetMethodResultCode("Sync");
	}

	public override SQLiteErrorCode Commit(SQLiteVirtualTable table)
	{
		CheckDisposed();
		return GetMethodResultCode("Commit");
	}

	public override SQLiteErrorCode Rollback(SQLiteVirtualTable table)
	{
		CheckDisposed();
		return GetMethodResultCode("Rollback");
	}

	public override bool FindFunction(SQLiteVirtualTable table, int argumentCount, string name, ref SQLiteFunction function, ref IntPtr pClientData)
	{
		CheckDisposed();
		return ResultCodeToFindFunctionResult(GetMethodResultCode("FindFunction"));
	}

	public override SQLiteErrorCode Rename(SQLiteVirtualTable table, string newName)
	{
		CheckDisposed();
		return GetMethodResultCode("Rename");
	}

	public override SQLiteErrorCode Savepoint(SQLiteVirtualTable table, int savepoint)
	{
		CheckDisposed();
		return GetMethodResultCode("Savepoint");
	}

	public override SQLiteErrorCode Release(SQLiteVirtualTable table, int savepoint)
	{
		CheckDisposed();
		return GetMethodResultCode("Release");
	}

	public override SQLiteErrorCode RollbackTo(SQLiteVirtualTable table, int savepoint)
	{
		CheckDisposed();
		return GetMethodResultCode("RollbackTo");
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteModuleNoop).Name);
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
