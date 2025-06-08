using System;

namespace AntdUI;

public class TreeCheckedEventArgs : EventArgs
{
	public TreeItem Item { get; private set; }

	public bool Value { get; private set; }

	public TreeCheckedEventArgs(TreeItem item, bool value)
	{
		Item = item;
		Value = value;
	}
}
