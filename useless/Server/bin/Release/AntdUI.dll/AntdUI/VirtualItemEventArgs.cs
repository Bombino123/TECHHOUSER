using System.Windows.Forms;

namespace AntdUI;

public class VirtualItemEventArgs : VMEventArgs<VirtualItem>
{
	public VirtualItemEventArgs(VirtualItem item, MouseEventArgs e)
		: base(item, e)
	{
	}
}
