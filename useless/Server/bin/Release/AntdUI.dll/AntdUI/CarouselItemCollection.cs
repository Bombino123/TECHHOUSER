using System.Windows.Forms;

namespace AntdUI;

public class CarouselItemCollection : iCollection<CarouselItem>
{
	public CarouselItemCollection(Carousel it)
	{
		BindData(it);
	}

	internal CarouselItemCollection BindData(Carousel it)
	{
		Carousel it2 = it;
		action = delegate
		{
			it2.ChangeImg();
			((Control)it2).Invalidate();
		};
		return this;
	}
}
