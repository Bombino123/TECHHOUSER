namespace System.Data.SQLite;

public class ProgressEventArgs : EventArgs
{
	public readonly IntPtr UserData;

	public SQLiteProgressReturnCode ReturnCode;

	private ProgressEventArgs()
	{
		UserData = IntPtr.Zero;
		ReturnCode = SQLiteProgressReturnCode.Continue;
	}

	internal ProgressEventArgs(IntPtr pUserData, SQLiteProgressReturnCode returnCode)
		: this()
	{
		UserData = pUserData;
		ReturnCode = returnCode;
	}
}
