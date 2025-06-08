using System.Windows.Forms;

namespace AntdUI;

public class BreadcrumbItemCollection : iCollection<BreadcrumbItem>
{
	public BreadcrumbItemCollection(Breadcrumb it)
	{
		BindData(it);
	}

	internal BreadcrumbItemCollection BindData(Breadcrumb it)
	{
		Breadcrumb it2 = it;
		action = delegate(bool render)
		{
			if (render)
			{
				it2.ChangeItems();
			}
			((Control)it2).Invalidate();
		};
		return this;
	}
}
