namespace System.Data.SQLite;

public class BusyEventArgs : EventArgs
{
	public readonly IntPtr UserData;

	public readonly int Count;

	public SQLiteBusyReturnCode ReturnCode;

	private BusyEventArgs()
	{
		UserData = IntPtr.Zero;
		Count = 0;
		ReturnCode = SQLiteBusyReturnCode.Retry;
	}

	internal BusyEventArgs(IntPtr pUserData, int count, SQLiteBusyReturnCode returnCode)
		: this()
	{
		UserData = pUserData;
		Count = count;
		ReturnCode = returnCode;
	}
}
