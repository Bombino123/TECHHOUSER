using System.IO;

namespace System.Data.SQLite;

internal sealed class SQLiteStreamChangeSetEnumerator : SQLiteChangeSetEnumerator
{
	private bool disposed;

	public SQLiteStreamChangeSetEnumerator(Stream stream, SQLiteConnectionFlags connectionFlags)
		: base(SQLiteStreamChangeSetIterator.Create(stream, connectionFlags))
	{
	}

	public SQLiteStreamChangeSetEnumerator(Stream stream, SQLiteConnectionFlags connectionFlags, SQLiteChangeSetStartFlags startFlags)
		: base(SQLiteStreamChangeSetIterator.Create(stream, connectionFlags, startFlags))
	{
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteStreamChangeSetEnumerator).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
