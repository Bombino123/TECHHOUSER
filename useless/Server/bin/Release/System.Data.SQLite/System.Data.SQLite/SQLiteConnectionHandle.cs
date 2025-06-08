using System.Runtime.InteropServices;
using System.Threading;

namespace System.Data.SQLite;

internal sealed class SQLiteConnectionHandle : CriticalHandle
{
	private bool ownHandle;

	public bool OwnHandle => ownHandle;

	public override bool IsInvalid => handle == IntPtr.Zero;

	public static implicit operator IntPtr(SQLiteConnectionHandle db)
	{
		return db?.handle ?? IntPtr.Zero;
	}

	internal SQLiteConnectionHandle(IntPtr db, bool ownHandle)
		: this(ownHandle)
	{
		this.ownHandle = ownHandle;
		SetHandle(db);
	}

	private SQLiteConnectionHandle(bool ownHandle)
		: base(IntPtr.Zero)
	{
	}

	protected override bool ReleaseHandle()
	{
		if (!ownHandle)
		{
			return true;
		}
		try
		{
			IntPtr intPtr = Interlocked.Exchange(ref handle, IntPtr.Zero);
			if (intPtr != IntPtr.Zero)
			{
				SQLiteBase.CloseConnection(this, intPtr);
			}
		}
		catch (SQLiteException)
		{
		}
		finally
		{
			SetHandleAsInvalid();
		}
		return true;
	}
}
