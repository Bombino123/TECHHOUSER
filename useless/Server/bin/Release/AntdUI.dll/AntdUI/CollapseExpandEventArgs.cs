namespace AntdUI;

public class CollapseExpandEventArgs : VEventArgs<CollapseItem>
{
	public bool Expand { get; private set; }

	public CollapseExpandEventArgs(CollapseItem value, bool expand)
		: base(value)
	{
		Expand = expand;
	}
}
