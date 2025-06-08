using System.Windows.Forms;

namespace AntdUI;

public class SegmentedItemCollection : iCollection<SegmentedItem>
{
	public SegmentedItemCollection(Segmented it)
	{
		BindData(it);
	}

	internal SegmentedItemCollection BindData(Segmented it)
	{
		Segmented it2 = it;
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
