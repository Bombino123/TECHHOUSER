using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace AntdUI;

[Description("StackPanel 堆栈布局")]
[ToolboxItem(true)]
[DefaultProperty("Vertical")]
[Designer(typeof(IControlDesigner))]
public class StackPanel : IControl
{
	internal class StackLayout : LayoutEngine
	{
		public bool Vertical { get; set; }

		public string? ItemSize { get; set; }

		public int Gap { get; set; }

		public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			if (container is StackPanel stackPanel && ((Control)stackPanel).IsHandleCreated && ((ArrangedElementCollection)((Control)stackPanel).Controls).Count > 0)
			{
				if (stackPanel.PauseLayout)
				{
					return false;
				}
				List<Control> list = new List<Control>(((ArrangedElementCollection)((Control)stackPanel).Controls).Count);
				foreach (Control item in (ArrangedElementCollection)((Control)stackPanel).Controls)
				{
					Control val = item;
					if (val.Visible)
					{
						list.Insert(0, val);
					}
				}
				if (list.Count > 0)
				{
					Rectangle displayRectangle = ((Control)stackPanel).DisplayRectangle;
					int num = 0;
					num = ((ItemSize == null || string.IsNullOrEmpty(ItemSize)) ? HandLayout(stackPanel, list, displayRectangle) : ((ItemSize.EndsWith("%") && float.TryParse(ItemSize.TrimEnd(new char[1] { '%' }), out var result)) ? HandLayout(stackPanel, list, displayRectangle, (int)Math.Round((float)(Vertical ? displayRectangle.Height : displayRectangle.Width) * (result / 100f))) : ((!int.TryParse(ItemSize, out var result2)) ? HandLayoutFill(list, displayRectangle) : HandLayout(stackPanel, list, displayRectangle, (int)Math.Round((float)result2 * Config.Dpi)))));
					if (stackPanel.ScrollBar != null)
					{
						bool show = stackPanel.ScrollBar.Show;
						stackPanel.ScrollBar.SetVrSize(num);
						if (show != stackPanel.ScrollBar.Show)
						{
							((Control)stackPanel).BeginInvoke((Delegate)new Action(stackPanel.IOnSizeChanged));
						}
					}
				}
			}
			return false;
		}

		private int HandLayout(StackPanel parent, List<Control> controls, Rectangle rect)
		{
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			int count = controls.Count;
			int num = 0;
			int num2 = 0;
			int result = 0;
			int num3 = 0;
			if (parent.ScrollBar != null)
			{
				num = parent.ScrollBar.Value;
			}
			if (Gap > 0 && count > 1)
			{
				num3 = (int)Math.Round((float)Gap * Config.Dpi);
			}
			Padding margin;
			if (Vertical)
			{
				foreach (Control control in controls)
				{
					Point location = rect.Location;
					margin = control.Margin;
					int left = ((Padding)(ref margin)).Left;
					int num4 = -num;
					margin = control.Margin;
					location.Offset(left, num4 + ((Padding)(ref margin)).Top + num2);
					control.Location = location;
					int width = rect.Width;
					margin = control.Margin;
					control.Width = width - ((Padding)(ref margin)).Horizontal;
					int num5 = num2;
					int num6 = control.Height + num3;
					margin = control.Margin;
					num2 = num5 + (num6 + ((Padding)(ref margin)).Vertical);
					result = location.Y + num + control.Height;
				}
			}
			else
			{
				foreach (Control control2 in controls)
				{
					Point location2 = rect.Location;
					int num7 = -num;
					margin = control2.Margin;
					int dx = num7 + ((Padding)(ref margin)).Left + num2;
					margin = control2.Margin;
					location2.Offset(dx, ((Padding)(ref margin)).Top);
					control2.Location = location2;
					int height = rect.Height;
					margin = control2.Margin;
					control2.Height = height - ((Padding)(ref margin)).Vertical;
					int num8 = num2;
					int num9 = control2.Width + num3;
					margin = control2.Margin;
					num2 = num8 + (num9 + ((Padding)(ref margin)).Horizontal);
					result = control2.Left + num + control2.Width;
				}
			}
			return result;
		}

		private int HandLayout(StackPanel parent, List<Control> controls, Rectangle rect, int size)
		{
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			int count = controls.Count;
			int num = 0;
			int num2 = 0;
			int result = 0;
			int num3 = 0;
			if (parent.ScrollBar != null)
			{
				num = parent.ScrollBar.Value;
			}
			if (Gap > 0 && count > 1)
			{
				num3 = (int)Math.Round((float)Gap * Config.Dpi);
			}
			Padding margin;
			if (Vertical)
			{
				foreach (Control control in controls)
				{
					Point location = rect.Location;
					margin = control.Margin;
					int left = ((Padding)(ref margin)).Left;
					int num4 = -num;
					margin = control.Margin;
					location.Offset(left, num4 + ((Padding)(ref margin)).Top + num2);
					control.Location = location;
					int width = rect.Width;
					margin = control.Margin;
					control.Size = new Size(width - ((Padding)(ref margin)).Horizontal, size);
					int num5 = num2;
					int num6 = control.Height + num3;
					margin = control.Margin;
					num2 = num5 + (num6 + ((Padding)(ref margin)).Vertical);
					result = location.Y + num + control.Height;
				}
			}
			else
			{
				foreach (Control control2 in controls)
				{
					Point location2 = rect.Location;
					int num7 = -num;
					margin = control2.Margin;
					int dx = num7 + ((Padding)(ref margin)).Left + num2;
					margin = control2.Margin;
					location2.Offset(dx, ((Padding)(ref margin)).Top);
					control2.Location = location2;
					int height = rect.Height;
					margin = control2.Margin;
					control2.Size = new Size(size, height - ((Padding)(ref margin)).Vertical);
					int num8 = num2;
					int num9 = control2.Width + num3;
					margin = control2.Margin;
					num2 = num8 + (num9 + ((Padding)(ref margin)).Horizontal);
					result = control2.Left + num + control2.Width;
				}
			}
			return result;
		}

		private int HandLayoutFill(List<Control> controls, Rectangle rect)
		{
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			int count = controls.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (Gap > 0 && count > 1)
			{
				num3 = (int)Math.Round((float)Gap * Config.Dpi);
			}
			Padding margin;
			if (Vertical)
			{
				int num4 = (rect.Height - num3 * (count - 1)) / count;
				foreach (Control control in controls)
				{
					Point location = rect.Location;
					margin = control.Margin;
					int dx = ((Padding)(ref margin)).Left + num;
					margin = control.Margin;
					location.Offset(dx, ((Padding)(ref margin)).Top + num2);
					control.Location = location;
					int width = rect.Width;
					margin = control.Margin;
					int width2 = width - ((Padding)(ref margin)).Horizontal;
					margin = control.Margin;
					control.Size = new Size(width2, num4 - ((Padding)(ref margin)).Vertical);
					num2 += num4 + num3;
				}
			}
			else
			{
				int num5 = (rect.Width - num3 * (count - 1)) / count;
				foreach (Control control2 in controls)
				{
					Point location2 = rect.Location;
					margin = control2.Margin;
					int dx2 = ((Padding)(ref margin)).Left + num;
					margin = control2.Margin;
					location2.Offset(dx2, ((Padding)(ref margin)).Top + num2);
					control2.Location = location2;
					margin = control2.Margin;
					int width3 = num5 - ((Padding)(ref margin)).Horizontal;
					int height = rect.Height;
					margin = control2.Margin;
					control2.Size = new Size(width3, height - ((Padding)(ref margin)).Vertical);
					num += num5 + num3;
				}
			}
			return 0;
		}
	}

	private bool autoscroll;

	[Browsable(false)]
	public ScrollBar? ScrollBar;

	private bool pauseLayout;

	private StackLayout layoutengine = new StackLayout();

	[Description("是否显示滚动条")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool AutoScroll
	{
		get
		{
			return autoscroll;
		}
		set
		{
			if (autoscroll != value)
			{
				autoscroll = value;
				if (autoscroll)
				{
					ScrollBar = new ScrollBar(this);
				}
				else
				{
					ScrollBar = null;
				}
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("AutoScroll");
			}
		}
	}

	public override Rectangle DisplayRectangle
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			Rectangle result = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
			if (ScrollBar != null && ScrollBar.Show)
			{
				if (ScrollBar.EnabledY)
				{
					result.Width -= ScrollBar.SIZE;
				}
				else
				{
					result.Height -= ScrollBar.SIZE;
				}
			}
			return result;
		}
	}

	[Description("是否垂直方向")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Vertical
	{
		get
		{
			return layoutengine.Vertical;
		}
		set
		{
			if (layoutengine.Vertical != value)
			{
				layoutengine.Vertical = value;
				if (autoscroll)
				{
					ScrollBar = new ScrollBar(this);
				}
				else
				{
					ScrollBar = null;
				}
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("Vertical");
			}
		}
	}

	[Description("内容大小")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? ItemSize
	{
		get
		{
			return layoutengine.ItemSize;
		}
		set
		{
			if (!(layoutengine.ItemSize == value))
			{
				layoutengine.ItemSize = value;
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("ItemSize");
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Gap
	{
		get
		{
			return layoutengine.Gap;
		}
		set
		{
			if (layoutengine.Gap != value)
			{
				layoutengine.Gap = value;
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("Gap");
			}
		}
	}

	[Browsable(false)]
	[Description("暂停布局")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool PauseLayout
	{
		get
		{
			return pauseLayout;
		}
		set
		{
			if (pauseLayout != value)
			{
				pauseLayout = value;
				if (!value)
				{
					((Control)this).Invalidate();
					IOnSizeChanged();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	public override LayoutEngine LayoutEngine => (LayoutEngine)(object)layoutengine;

	protected override void OnPaint(PaintEventArgs e)
	{
		ScrollBar?.Paint(e.Graphics.High());
		((Control)this).OnPaint(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		IOnSizeChanged();
		((Control)this).OnHandleCreated(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		ScrollBar?.SizeChange(clientRectangle);
		((Control)this).OnSizeChanged(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (ScrollBar != null && ScrollBar.MouseDown(e.Location))
		{
			OnTouchDown(e.X, e.Y);
		}
		else
		{
			((Control)this).OnMouseDown(e);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (ScrollBar == null || !ScrollBar.MouseMove(e.Location) || !OnTouchMove(e.X, e.Y))
		{
			((Control)this).OnMouseMove(e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		ScrollBar?.MouseUp();
		OnTouchUp();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar?.Leave();
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ScrollBar?.Leave();
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (ScrollBar != null && ScrollBar.EnabledY)
		{
			ScrollBar.MouseWheel(e.Delta);
		}
		base.OnMouseWheel(e);
	}

	protected override bool OnTouchScrollX(int value)
	{
		if (ScrollBar != null && ScrollBar.EnabledX)
		{
			return ScrollBar.MouseWheelX(value);
		}
		return false;
	}

	protected override bool OnTouchScrollY(int value)
	{
		if (ScrollBar != null && ScrollBar.EnabledY)
		{
			return ScrollBar.MouseWheelY(value);
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar?.Dispose();
		base.Dispose(disposing);
	}
}
