namespace AntdUI;

public class MenuSelectEventArgs : VEventArgs<MenuItem>
{
	public MenuSelectEventArgs(MenuItem value)
		: base(value)
	{
	}
}
