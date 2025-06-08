namespace System.Data.SQLite;

public class SQLiteFunctionEx : SQLiteFunction
{
	private bool disposed;

	protected CollationSequence GetCollationSequence()
	{
		return _base.GetCollationSequence(this, _context);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteFunctionEx).Name);
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
