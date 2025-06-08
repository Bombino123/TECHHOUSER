using System.Windows.Forms;

namespace AntdUI;

public class TabsItemEventArgs : VMEventArgs<TabPage>
{
	public Tabs.IStyle Style { get; private set; }

	public TabsItemEventArgs(TabPage item, Tabs.IStyle style, MouseEventArgs e)
		: base(item, e)
	{
		Style = style;
	}
}
