using System.Collections;
using System.Collections.Generic;

namespace System.Data.SQLite;

internal abstract class SQLiteChangeSetEnumerator : IEnumerator<ISQLiteChangeSetMetadataItem>, IDisposable, IEnumerator
{
	private SQLiteChangeSetIterator iterator;

	private bool disposed;

	public ISQLiteChangeSetMetadataItem Current
	{
		get
		{
			CheckDisposed();
			return new SQLiteChangeSetMetadataItem(iterator);
		}
	}

	object IEnumerator.Current
	{
		get
		{
			CheckDisposed();
			return Current;
		}
	}

	public SQLiteChangeSetEnumerator(SQLiteChangeSetIterator iterator)
	{
		SetIterator(iterator);
	}

	private void CheckIterator()
	{
		if (iterator == null)
		{
			throw new InvalidOperationException("iterator unavailable");
		}
		iterator.CheckHandle();
	}

	private void SetIterator(SQLiteChangeSetIterator iterator)
	{
		this.iterator = iterator;
	}

	private void CloseIterator()
	{
		if (iterator != null)
		{
			iterator.Dispose();
			iterator = null;
		}
	}

	protected void ResetIterator(SQLiteChangeSetIterator iterator)
	{
		CloseIterator();
		SetIterator(iterator);
	}

	public bool MoveNext()
	{
		CheckDisposed();
		CheckIterator();
		return iterator.Next();
	}

	public virtual void Reset()
	{
		CheckDisposed();
		throw new NotImplementedException();
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
			throw new ObjectDisposedException(typeof(SQLiteChangeSetEnumerator).Name);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing)
			{
				CloseIterator();
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteChangeSetEnumerator()
	{
		Dispose(disposing: false);
	}
}
