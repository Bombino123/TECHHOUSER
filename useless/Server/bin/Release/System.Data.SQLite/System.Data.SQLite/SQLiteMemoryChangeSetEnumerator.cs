namespace System.Data.SQLite;

internal sealed class SQLiteMemoryChangeSetEnumerator : SQLiteChangeSetEnumerator
{
	private byte[] rawData;

	private SQLiteChangeSetStartFlags flags;

	private bool disposed;

	public SQLiteMemoryChangeSetEnumerator(byte[] rawData)
		: base(SQLiteMemoryChangeSetIterator.Create(rawData))
	{
		this.rawData = rawData;
		flags = SQLiteChangeSetStartFlags.None;
	}

	public SQLiteMemoryChangeSetEnumerator(byte[] rawData, SQLiteChangeSetStartFlags flags)
		: base(SQLiteMemoryChangeSetIterator.Create(rawData, flags))
	{
		this.rawData = rawData;
		this.flags = flags;
	}

	public override void Reset()
	{
		CheckDisposed();
		SQLiteMemoryChangeSetIterator sQLiteMemoryChangeSetIterator = ((flags == SQLiteChangeSetStartFlags.None) ? SQLiteMemoryChangeSetIterator.Create(rawData) : SQLiteMemoryChangeSetIterator.Create(rawData, flags));
		ResetIterator(sQLiteMemoryChangeSetIterator);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteMemoryChangeSetEnumerator).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposed)
			{
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}
}
