using System;
using System.Drawing;

namespace AntdUI;

public abstract class VirtualItem
{
	public bool SHOW;

	public bool SHOW_RECT;

	public Rectangle RECT;

	internal Action<VirtualItem>? invalidate;

	internal int WIDTH;

	internal int HEIGHT;

	public bool Visible { get; set; } = true;


	public bool CanClick { get; set; } = true;


	public bool Hover { get; set; }

	public object? Tag { get; set; }

	public abstract Size Size(Canvas g, VirtualPanelArgs e);

	public abstract void Paint(Canvas g, VirtualPanelArgs e);

	public virtual bool MouseMove(VirtualPanel sender, VirtualPanelMouseArgs e)
	{
		return true;
	}

	public virtual void MouseLeave(VirtualPanel sender, VirtualPanelMouseArgs e)
	{
	}

	public virtual void MouseClick(VirtualPanel sender, VirtualPanelMouseArgs e)
	{
	}

	public void Invalidate()
	{
		invalidate?.Invoke(this);
	}

	internal void SetRECT(int x, int y, int w, int h)
	{
		RECT.Width = w;
		RECT.Height = h;
		RECT.X = x;
		RECT.Y = y;
	}
}
