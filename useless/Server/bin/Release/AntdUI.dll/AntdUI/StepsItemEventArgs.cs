using System.Windows.Forms;

namespace AntdUI;

public class StepsItemEventArgs : VMEventArgs<StepsItem>
{
	public StepsItemEventArgs(StepsItem item, MouseEventArgs e)
		: base(item, e)
	{
	}
}
