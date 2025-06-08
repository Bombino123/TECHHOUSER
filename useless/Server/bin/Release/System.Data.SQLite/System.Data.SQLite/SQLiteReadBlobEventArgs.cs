namespace System.Data.SQLite;

public class SQLiteReadBlobEventArgs : SQLiteReadEventArgs
{
	private bool readOnly;

	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			readOnly = value;
		}
	}

	internal SQLiteReadBlobEventArgs(bool readOnly)
	{
		this.readOnly = readOnly;
	}
}
