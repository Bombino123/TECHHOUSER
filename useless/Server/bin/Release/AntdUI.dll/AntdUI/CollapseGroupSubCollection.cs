using System.Windows.Forms;

namespace AntdUI;

public class CollapseGroupSubCollection : iCollection<CollapseGroupSub>
{
	public CollapseGroupSubCollection(CollapseGroup it)
	{
		BindData(it);
	}

	public CollapseGroupSubCollection(CollapseGroupItem it)
	{
		BindData(it);
	}

	internal CollapseGroupSubCollection BindData(CollapseGroup it)
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

	internal CollapseGroupSubCollection BindData(CollapseGroupItem it)
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
