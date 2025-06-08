using System.Windows.Forms;

namespace AntdUI;

public class SegmentedItemEventArgs : VMEventArgs<SegmentedItem>
{
	public SegmentedItemEventArgs(SegmentedItem item, MouseEventArgs e)
		: base(item, e)
	{
	}
}
