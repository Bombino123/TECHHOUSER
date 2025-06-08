namespace System.Data.SQLite;

public class SQLiteReadValueEventArgs : SQLiteReadEventArgs
{
	private string methodName;

	private SQLiteReadEventArgs extraEventArgs;

	private SQLiteDataReaderValue value;

	public string MethodName => methodName;

	public SQLiteReadEventArgs ExtraEventArgs => extraEventArgs;

	public SQLiteDataReaderValue Value => value;

	internal SQLiteReadValueEventArgs(string methodName, SQLiteReadEventArgs extraEventArgs, SQLiteDataReaderValue value)
	{
		this.methodName = methodName;
		this.extraEventArgs = extraEventArgs;
		this.value = value;
	}
}
