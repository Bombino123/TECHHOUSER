using System.Windows.Forms;

namespace AntdUI;

public class StepsItemCollection : iCollection<StepsItem>
{
	public StepsItemCollection(Steps it)
	{
		BindData(it);
	}

	internal StepsItemCollection BindData(Steps it)
	{
		Steps it2 = it;
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
