using System.Globalization;

namespace System.Data.SQLite;

public class SQLiteModuleCommon : SQLiteModuleNoop
{
	private static readonly string declareSql = HelperMethods.StringFormat(CultureInfo.InvariantCulture, "CREATE TABLE {0}(x);", typeof(SQLiteModuleCommon).Name);

	private bool objectIdentity;

	private bool disposed;

	public SQLiteModuleCommon(string name)
		: this(name, objectIdentity: false)
	{
	}

	public SQLiteModuleCommon(string name, bool objectIdentity)
		: base(name)
	{
		this.objectIdentity = objectIdentity;
	}

	protected virtual string GetSqlForDeclareTable()
	{
		return declareSql;
	}

	protected virtual SQLiteErrorCode CursorTypeMismatchError(SQLiteVirtualTableCursor cursor, Type type)
	{
		if (type != null)
		{
			SetCursorError(cursor, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "not a \"{0}\" cursor", type));
		}
		else
		{
			SetCursorError(cursor, "cursor type mismatch");
		}
		return SQLiteErrorCode.Error;
	}

	protected virtual string GetStringFromObject(SQLiteVirtualTableCursor cursor, object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is string)
		{
			return (string)value;
		}
		return value.ToString();
	}

	protected virtual long MakeRowId(int rowIndex, int hashCode)
	{
		return ((long)rowIndex << 32) | (uint)hashCode;
	}

	protected virtual long GetRowIdFromObject(SQLiteVirtualTableCursor cursor, object value)
	{
		int rowIndex = cursor?.GetRowIndex() ?? 0;
		int hashCode = SQLiteMarshal.GetHashCode(value, objectIdentity);
		return MakeRowId(rowIndex, hashCode);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteModuleCommon).Name);
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
