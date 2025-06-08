using System.Runtime.InteropServices;
using System.Threading;

namespace System.Data.SQLite;

internal sealed class SQLiteBackupHandle : CriticalHandle
{
	private SQLiteConnectionHandle cnn;

	public override bool IsInvalid => handle == IntPtr.Zero;

	public static implicit operator IntPtr(SQLiteBackupHandle backup)
	{
		return backup?.handle ?? IntPtr.Zero;
	}

	internal SQLiteBackupHandle(SQLiteConnectionHandle cnn, IntPtr backup)
		: this()
	{
		this.cnn = cnn;
		SetHandle(backup);
	}

	private SQLiteBackupHandle()
		: base(IntPtr.Zero)
	{
	}

	protected override bool ReleaseHandle()
	{
		try
		{
			IntPtr intPtr = Interlocked.Exchange(ref handle, IntPtr.Zero);
			if (intPtr != IntPtr.Zero)
			{
				SQLiteBase.FinishBackup(cnn, intPtr);
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
