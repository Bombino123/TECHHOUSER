using System;
using System.Drawing;

namespace AntdUI;

public class VirtualPanelArgs : EventArgs
{
	public VirtualPanel Panel { get; private set; }

	public Rectangle Rect { get; private set; }

	public int Radius { get; private set; }

	public VirtualPanelArgs(VirtualPanel panel, Rectangle rect, int radius)
	{
		Panel = panel;
		Rect = rect;
		Radius = radius;
	}
}
