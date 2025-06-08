namespace AntdUI;

public class ClosingPageEventArgs : VEventArgs<TabPage>
{
	public ClosingPageEventArgs(TabPage value)
		: base(value)
	{
	}
}
