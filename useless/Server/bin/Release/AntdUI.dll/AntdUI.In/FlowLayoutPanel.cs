using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AntdUI.In;

public class FlowLayoutPanel : FlowLayoutPanel
{
	private ScrollY scrollY;

	private bool scrollYVisible;

	private const int WM_PAINT = 15;

	private const int WM_ERASEBKGND = 20;

	private const int WM_NCCALCSIZE = 131;

	private const int WM_MOUSEWHEEL = 522;

	private const int SB_SHOW_VERT = 1;

	private const int SB_SHOW_BOTH = 3;

	private bool ScrollYVisible
	{
		get
		{
			return scrollYVisible;
		}
		set
		{
			if (scrollYVisible != value)
			{
				scrollYVisible = value;
				if (!value)
				{
					scrollY.SetVrSize(0f, 0);
				}
				((Control)this).OnSizeChanged(EventArgs.Empty);
			}
		}
	}

	public override Rectangle DisplayRectangle
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			Rectangle result = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
			if (scrollYVisible)
			{
				return new Rectangle(result.X, result.Y - ((ScrollProperties)((ScrollableControl)this).VerticalScroll).Value, result.Width - scrollY.Rect.Width, result.Height);
			}
			return result;
		}
	}

	public FlowLayoutPanel()
	{
		((Control)this).SetStyle((ControlStyles)204818, true);
		((Control)this).UpdateStyles();
		scrollY = new ScrollY((FlowLayoutPanel)(object)this);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		LoadScroll();
		((Control)this).OnSizeChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		LoadScroll();
		if (ScrollYVisible)
		{
			scrollY.Paint(e.Graphics.High());
		}
		((Control)this).OnPaint(e);
	}

	private void LoadScroll()
	{
		ScrollYVisible = ((ScrollProperties)((ScrollableControl)this).VerticalScroll).Visible;
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (ScrollYVisible)
		{
			scrollY.SizeChange(clientRectangle);
			if (ScrollYVisible)
			{
				scrollY.SetVrSize(((ScrollProperties)((ScrollableControl)this).VerticalScroll).Maximum, clientRectangle.Height);
				scrollY.Value = ((ScrollProperties)((ScrollableControl)this).VerticalScroll).Value;
			}
		}
	}

	protected override void WndProc(ref Message m)
	{
		switch (((Message)(ref m)).Msg)
		{
		case 15:
		case 20:
		case 131:
			if (!((Component)this).DesignMode && ((ScrollableControl)this).AutoScroll)
			{
				ShowScrollBar(((Control)this).Handle, 3, bShow: false);
			}
			break;
		case 522:
			ShowScrollBar(((Control)this).Handle, 3, bShow: false);
			break;
		}
		((ScrollableControl)this).WndProc(ref m);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (scrollY.MouseDown(e.Location, delegate(float value)
		{
			((ScrollProperties)((ScrollableControl)this).VerticalScroll).Value = (int)value;
		}))
		{
			((Control)this).OnMouseDown(e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		scrollY.MouseUp(e.Location);
		((Control)this).OnMouseUp(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		scrollY.Leave();
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (scrollY.MouseMove(e.Location, delegate(float value)
		{
			((ScrollProperties)((ScrollableControl)this).VerticalScroll).Value = (int)value;
		}))
		{
			((Control)this).OnMouseMove(e);
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
}
