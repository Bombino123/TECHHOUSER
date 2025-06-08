namespace System.Data.SQLite;

public class AuthorizerEventArgs : EventArgs
{
	public readonly IntPtr UserData;

	public readonly SQLiteAuthorizerActionCode ActionCode;

	public readonly string Argument1;

	public readonly string Argument2;

	public readonly string Database;

	public readonly string Context;

	public SQLiteAuthorizerReturnCode ReturnCode;

	private AuthorizerEventArgs()
	{
		UserData = IntPtr.Zero;
		ActionCode = SQLiteAuthorizerActionCode.None;
		Argument1 = null;
		Argument2 = null;
		Database = null;
		Context = null;
		ReturnCode = SQLiteAuthorizerReturnCode.Ok;
	}

	internal AuthorizerEventArgs(IntPtr pUserData, SQLiteAuthorizerActionCode actionCode, string argument1, string argument2, string database, string context, SQLiteAuthorizerReturnCode returnCode)
		: this()
	{
		UserData = pUserData;
		ActionCode = actionCode;
		Argument1 = argument1;
		Argument2 = argument2;
		Database = database;
		Context = context;
		ReturnCode = returnCode;
	}
}
