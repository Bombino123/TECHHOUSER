namespace System.Data.SQLite;

public class LogEventArgs : EventArgs
{
	public readonly object ErrorCode;

	public readonly string Message;

	public readonly object Data;

	internal LogEventArgs(IntPtr pUserData, object errorCode, string message, object data)
	{
		ErrorCode = errorCode;
		Message = message;
		Data = data;
	}
}
