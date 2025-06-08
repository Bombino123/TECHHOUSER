namespace AntdUI;

public class StringsEventArgs : VEventArgs<string[]>
{
	public StringsEventArgs(string[] value)
		: base(value)
	{
	}
}
