using System.Windows.Forms;

namespace AntdUI;

public class MenuItemCollection : iCollection<MenuItem>
{
	public MenuItemCollection(Menu it)
	{
		BindData(it);
	}

	public MenuItemCollection(MenuItem it)
	{
		BindData(it);
	}

	internal MenuItemCollection BindData(Menu it)
	{
		Menu it2 = it;
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

	internal MenuItemCollection BindData(MenuItem it)
	{
		MenuItem it2 = it;
		action = delegate(bool render)
		{
			if (it2.PARENT != null)
			{
				if (render)
				{
					it2.PARENT.ChangeList();
				}
				((Control)it2.PARENT).Invalidate();
			}
		};
		return this;
	}
}
