using System.Collections.Generic;
using System.IO;

namespace System.Data.SQLite;

internal sealed class SQLiteSessionStreamManager : IDisposable
{
	private Dictionary<Stream, SQLiteStreamAdapter> streamAdapters;

	private SQLiteConnectionFlags flags;

	private bool disposed;

	public SQLiteSessionStreamManager(SQLiteConnectionFlags flags)
	{
		this.flags = flags;
		InitializeStreamAdapters();
	}

	private void InitializeStreamAdapters()
	{
		if (streamAdapters == null)
		{
			streamAdapters = new Dictionary<Stream, SQLiteStreamAdapter>();
		}
	}

	private void DisposeStreamAdapters()
	{
		if (streamAdapters == null)
		{
			return;
		}
		foreach (KeyValuePair<Stream, SQLiteStreamAdapter> streamAdapter in streamAdapters)
		{
			streamAdapter.Value?.Dispose();
		}
		streamAdapters.Clear();
		streamAdapters = null;
	}

	public SQLiteStreamAdapter GetAdapter(Stream stream)
	{
		CheckDisposed();
		if (stream == null)
		{
			return null;
		}
		if (streamAdapters.TryGetValue(stream, out var value))
		{
			return value;
		}
		value = new SQLiteStreamAdapter(stream, flags);
		streamAdapters.Add(stream, value);
		return value;
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
			throw new ObjectDisposedException(typeof(SQLiteSessionStreamManager).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing)
			{
				DisposeStreamAdapters();
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteSessionStreamManager()
	{
		Dispose(disposing: false);
	}
}
