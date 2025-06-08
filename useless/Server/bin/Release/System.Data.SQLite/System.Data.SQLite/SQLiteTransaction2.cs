using System.Threading;

namespace System.Data.SQLite;

public sealed class SQLiteTransaction2 : SQLiteTransaction
{
	private int _beginLevel;

	private string _savePointName;

	private bool disposed;

	internal SQLiteTransaction2(SQLiteConnection connection, bool deferredLock)
		: base(connection, deferredLock)
	{
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteTransaction2).Name);
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing && IsValid(throwError: false))
			{
				IssueRollback(throwError: false);
			}
		}
		finally
		{
			base.Dispose(disposing);
			disposed = true;
		}
	}

	public override void Commit()
	{
		CheckDisposed();
		IsValid(throwError: true);
		if (_beginLevel == 0)
		{
			using (SQLiteCommand sQLiteCommand = _cnn.CreateCommand())
			{
				sQLiteCommand.CommandText = "COMMIT;";
				sQLiteCommand.ExecuteNonQuery();
			}
			_cnn._transactionLevel = 0;
			_cnn = null;
			return;
		}
		using (SQLiteCommand sQLiteCommand2 = _cnn.CreateCommand())
		{
			if (string.IsNullOrEmpty(_savePointName))
			{
				throw new SQLiteException("Cannot commit, unknown SAVEPOINT");
			}
			sQLiteCommand2.CommandText = $"RELEASE {_savePointName};";
			sQLiteCommand2.ExecuteNonQuery();
		}
		_cnn._transactionLevel--;
		_cnn = null;
	}

	protected override void Begin(bool deferredLock)
	{
		int beginLevel;
		if ((beginLevel = _cnn._transactionLevel++) == 0)
		{
			try
			{
				using SQLiteCommand sQLiteCommand = _cnn.CreateCommand();
				if (!deferredLock)
				{
					sQLiteCommand.CommandText = "BEGIN IMMEDIATE;";
				}
				else
				{
					sQLiteCommand.CommandText = "BEGIN;";
				}
				sQLiteCommand.ExecuteNonQuery();
				_beginLevel = beginLevel;
				return;
			}
			catch (SQLiteException)
			{
				_cnn._transactionLevel--;
				_cnn = null;
				throw;
			}
		}
		try
		{
			using SQLiteCommand sQLiteCommand2 = _cnn.CreateCommand();
			_savePointName = GetSavePointName();
			sQLiteCommand2.CommandText = $"SAVEPOINT {_savePointName};";
			sQLiteCommand2.ExecuteNonQuery();
			_beginLevel = beginLevel;
		}
		catch (SQLiteException)
		{
			_cnn._transactionLevel--;
			_cnn = null;
			throw;
		}
	}

	protected override void IssueRollback(bool throwError)
	{
		SQLiteConnection sQLiteConnection = Interlocked.Exchange(ref _cnn, null);
		if (sQLiteConnection == null)
		{
			return;
		}
		if (_beginLevel == 0)
		{
			try
			{
				using (SQLiteCommand sQLiteCommand = sQLiteConnection.CreateCommand())
				{
					sQLiteCommand.CommandText = "ROLLBACK;";
					sQLiteCommand.ExecuteNonQuery();
				}
				sQLiteConnection._transactionLevel = 0;
				return;
			}
			catch
			{
				if (throwError)
				{
					throw;
				}
				return;
			}
		}
		try
		{
			using (SQLiteCommand sQLiteCommand2 = sQLiteConnection.CreateCommand())
			{
				if (string.IsNullOrEmpty(_savePointName))
				{
					throw new SQLiteException("Cannot rollback, unknown SAVEPOINT");
				}
				sQLiteCommand2.CommandText = $"ROLLBACK TO {_savePointName};";
				sQLiteCommand2.ExecuteNonQuery();
			}
			sQLiteConnection._transactionLevel--;
		}
		catch
		{
			if (throwError)
			{
				throw;
			}
		}
	}

	private string GetSavePointName()
	{
		return $"sqlite_dotnet_savepoint_{++_cnn._transactionSequence}";
	}
}
