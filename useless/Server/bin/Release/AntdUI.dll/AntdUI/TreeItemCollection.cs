using System.Windows.Forms;

namespace AntdUI;

public class TreeItemCollection : iCollection<TreeItem>
{
	public TreeItemCollection(Tree it)
	{
		BindData(it);
	}

	public TreeItemCollection(TreeItem it)
	{
		BindData(it);
	}

	internal TreeItemCollection BindData(Tree it)
	{
		Tree it2 = it;
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

	internal TreeItemCollection BindData(TreeItem it)
	{
		TreeItem it2 = it;
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
