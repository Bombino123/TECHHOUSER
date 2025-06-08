using System.Windows.Forms;

namespace AntdUI;

public class SliderMarkItemCollection : iCollection<SliderMarkItem>
{
	public SliderMarkItemCollection(IControl it)
	{
		BindData(it);
	}

	internal SliderMarkItemCollection BindData(IControl it)
	{
		IControl it2 = it;
		action = delegate
		{
			((Control)it2).Invalidate();
		};
		return this;
	}
}
