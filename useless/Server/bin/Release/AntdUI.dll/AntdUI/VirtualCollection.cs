using System.Windows.Forms;

namespace AntdUI;

public class VirtualCollection : iCollection<VirtualItem>
{
	public VirtualCollection(VirtualPanel it)
	{
		BindData(it);
	}

	internal VirtualCollection BindData(VirtualPanel it)
	{
		VirtualPanel it2 = it;
		action = delegate(bool render)
		{
			if (render)
			{
				it2.CellCount = -1;
				it2.LoadLayout();
			}
			((Control)it2).Invalidate();
		};
		return this;
	}
}
