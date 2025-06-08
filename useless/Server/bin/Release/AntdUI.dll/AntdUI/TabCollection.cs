using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace AntdUI;

public class TabCollection : iCollection<TabPage>
{
	public TabCollection(Tabs it)
	{
		BindData(it);
	}

	internal TabCollection BindData(Tabs it)
	{
		Tabs it2 = it;
		action = delegate(bool render)
		{
			if (render)
			{
				it2.LoadLayout(r: false);
			}
			((Control)it2).Invalidate();
		};
		action_add = delegate(TabPage item)
		{
			item.PARENT = it2;
			item.SetDock(((ArrangedElementCollection)it2.Controls).Count == 0);
			it2.Controls.Add((Control)(object)item);
		};
		action_del = delegate(TabPage item, int index)
		{
			it2.Controls.Remove((Control)(object)item);
			if (index == -1)
			{
				it2.SelectedIndex = 0;
			}
			else
			{
				int selectedIndex = it2.SelectedIndex;
				if (selectedIndex == index)
				{
					int num = index - 1;
					if (num > -1)
					{
						it2.SelectedIndex = num;
					}
					else
					{
						it2.ShowPage(num);
					}
				}
				else if (selectedIndex > index)
				{
					it2.SelectedIndex = selectedIndex - 1;
				}
			}
		};
		return this;
	}
}
