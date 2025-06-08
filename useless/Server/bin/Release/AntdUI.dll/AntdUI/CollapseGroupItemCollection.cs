using System.Windows.Forms;

namespace AntdUI;

public class CollapseGroupItemCollection : iCollection<CollapseGroupItem>
{
	public CollapseGroupItemCollection(CollapseGroup it)
	{
		BindData(it);
	}

	public CollapseGroupItemCollection(CollapseGroupItem it)
	{
		BindData(it);
	}

	internal CollapseGroupItemCollection BindData(CollapseGroup it)
	{
		CollapseGroup it2 = it;
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

	internal CollapseGroupItemCollection BindData(CollapseGroupItem it)
	{
		CollapseGroupItem it2 = it;
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
