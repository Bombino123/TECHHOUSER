using System.Windows.Forms;

namespace AntdUI;

public class TimelineItemEventArgs : VMEventArgs<TimelineItem>
{
	public TimelineItemEventArgs(TimelineItem item, MouseEventArgs e)
		: base(item, e)
	{
	}
}
