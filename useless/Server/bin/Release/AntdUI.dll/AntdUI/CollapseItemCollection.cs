using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class CollapseItemCollection : iCollection<CollapseItem>
{
	public CollapseItemCollection(Collapse it)
	{
		BindData(it);
	}

	internal CollapseItemCollection BindData(Collapse it)
	{
		Collapse it2 = it;
		action = delegate(bool render)
		{
			if (render)
			{
				it2.LoadLayout(r: false);
			}
			((Control)it2).Invalidate();
		};
		action_add = delegate(CollapseItem item)
		{
			item.PARENT = it2;
			((Control)item).Location = new Point(-((Control)item).Width, -((Control)item).Height);
			it2.Controls.Add((Control)(object)item);
		};
		action_del = delegate(CollapseItem item, int index)
		{
			it2.Controls.Remove((Control)(object)item);
		};
		return this;
	}
}
