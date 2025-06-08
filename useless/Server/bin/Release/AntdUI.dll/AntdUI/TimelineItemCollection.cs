using System.Windows.Forms;

namespace AntdUI;

public class TimelineItemCollection : iCollection<TimelineItem>
{
	public TimelineItemCollection(Timeline it)
	{
		BindData(it);
	}

	internal TimelineItemCollection BindData(Timeline it)
	{
		Timeline it2 = it;
		action = delegate(bool render)
		{
			if (render)
			{
				it2.ChangeList();
			}
			((Control)it2).Invalidate();
		};
		return this;
	}
}
