using System.Windows.Forms;

namespace AntdUI;

public class BreadcrumbItemEventArgs : VMEventArgs<BreadcrumbItem>
{
	public BreadcrumbItemEventArgs(BreadcrumbItem item, MouseEventArgs e)
		: base(item, e)
	{
	}
}
