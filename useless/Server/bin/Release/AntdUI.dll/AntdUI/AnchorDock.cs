using System.Windows.Forms;

namespace AntdUI;

internal class AnchorDock
{
	public DockStyle Dock { get; set; }

	public AnchorStyles Anchor { get; set; }

	public AnchorDock(Control control)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Dock = control.Dock;
		Anchor = control.Anchor;
		if (control.Visible)
		{
			control.Dock = (DockStyle)0;
			control.Anchor = (AnchorStyles)5;
		}
	}
}
