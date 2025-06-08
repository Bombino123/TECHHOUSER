using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace AntdUI;

[Description("FlowPanel 流动布局")]
[ToolboxItem(true)]
[DefaultProperty("Align")]
[Designer(typeof(IControlDesigner))]
public class FlowPanel : IControl
{
	internal class FlowLayout : LayoutEngine
	{
		public int Gap { get; set; }

		public TAlignFlow Align { get; set; }

		public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			if (container is FlowPanel flowPanel && ((Control)flowPanel).IsHandleCreated && ((ArrangedElementCollection)((Control)flowPanel).Controls).Count > 0)
			{
				if (flowPanel.PauseLayout)
				{
					return false;
				}
				List<Control> list = new List<Control>(((ArrangedElementCollection)((Control)flowPanel).Controls).Count);
				foreach (Control item in (ArrangedElementCollection)((Control)flowPanel).Controls)
				{
					Control val = item;
					if (val.Visible)
					{
						list.Insert(0, val);
					}
				}
				if (list.Count > 0)
				{
					int vrSize = HandLayout(flowPanel, list);
					if (flowPanel.ScrollBar != null)
					{
						bool show = flowPanel.ScrollBar.Show;
						float num = flowPanel.ScrollBar.Max;
						flowPanel.ScrollBar.SetVrSize(vrSize);
						if (show != flowPanel.ScrollBar.Show || num != (float)flowPanel.ScrollBar.Max)
						{
							((Control)flowPanel).BeginInvoke((Delegate)new Action(flowPanel.IOnSizeChanged));
						}
					}
				}
			}
			return false;
		}

		private int HandLayout(FlowPanel parent, List<Control> controls)
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			Rectangle displayRectangle = ((Control)parent).DisplayRectangle;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int result = 0;
			int num4 = 0;
			if (parent.ScrollBar != null)
			{
				num = parent.ScrollBar.Value;
			}
			if (Gap > 0 && controls.Count > 1)
			{
				num4 = (int)Math.Round((float)Gap * Config.Dpi);
			}
			List<CP> list = new List<CP>();
			List<CP> list2 = new List<CP>(controls.Count);
			int num5 = 0;
			foreach (Control control in controls)
			{
				Point location = displayRectangle.Location;
				int num6 = num2 + control.Width;
				Padding margin = control.Margin;
				if (num6 + ((Padding)(ref margin)).Horizontal > displayRectangle.Width)
				{
					if (list.Count > 0)
					{
						if (Align == TAlignFlow.LeftCenter || Align == TAlignFlow.Center || Align == TAlignFlow.RightCenter)
						{
							int num7 = (displayRectangle.Width - num2 + num4) / 2;
							num5 = num7;
							foreach (CP item2 in list)
							{
								item2.Point = new Point(item2.Point.X + num7, item2.Point.Y);
							}
						}
						else if (Align == TAlignFlow.Right)
						{
							int num8 = displayRectangle.Width - num2 + num4;
							num5 = num8;
							foreach (CP item3 in list)
							{
								item3.Point = new Point(item3.Point.X + num8, item3.Point.Y);
							}
						}
					}
					list.Clear();
					num2 = 0;
					int num9 = num3;
					int num10 = control.Height + num4;
					margin = control.Margin;
					num3 = num9 + (num10 + ((Padding)(ref margin)).Vertical);
				}
				margin = control.Margin;
				int dx = ((Padding)(ref margin)).Left + num2;
				int num11 = -num;
				margin = control.Margin;
				location.Offset(dx, num11 + ((Padding)(ref margin)).Top + num3);
				CP item = new CP(control, location);
				list2.Add(item);
				list.Add(item);
				int num12 = num2;
				int num13 = control.Width + num4;
				margin = control.Margin;
				num2 = num12 + (num13 + ((Padding)(ref margin)).Horizontal);
				result = location.Y + num + control.Height;
			}
			if (list.Count > 0)
			{
				if (Align == TAlignFlow.LeftCenter)
				{
					if (num5 > 0)
					{
						foreach (CP item4 in list)
						{
							item4.Point = new Point(item4.Point.X + num5, item4.Point.Y);
						}
					}
				}
				else if (Align == TAlignFlow.RightCenter)
				{
					int num14 = displayRectangle.X + (displayRectangle.Width - num2 + num4) - num5;
					foreach (CP item5 in list)
					{
						item5.Point = new Point(item5.Point.X + num14, item5.Point.Y);
					}
				}
				else if (Align == TAlignFlow.Center)
				{
					int num15 = (displayRectangle.Width - num2 + num4) / 2;
					foreach (CP item6 in list)
					{
						item6.Point = new Point(item6.Point.X + num15, item6.Point.Y);
					}
				}
				else if (Align == TAlignFlow.Right)
				{
					int num16 = displayRectangle.Width - num2 + num4;
					foreach (CP item7 in list)
					{
						item7.Point = new Point(item7.Point.X + num16, item7.Point.Y);
					}
				}
			}
			list.Clear();
			((Control)parent).SuspendLayout();
			foreach (CP item8 in list2)
			{
				item8.Control.Location = item8.Point;
			}
			((Control)parent).ResumeLayout();
			return result;
		}
	}

	private class CP
	{
		public Control Control { get; set; }

		public Point Point { get; set; }

		public CP(Control control, Point point)
		{
			Control = control;
			Point = point;
		}
	}

	private bool autoscroll;

	[Browsable(false)]
	public ScrollBar? ScrollBar;

	private bool pauseLayout;

	private FlowLayout layoutengine = new FlowLayout();

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
				result.Width -= ScrollBar.SIZE;
			}
			return result;
		}
	}

	[Description("布局方向")]
	[Category("外观")]
	[DefaultValue(TAlignFlow.LeftCenter)]
	public TAlignFlow Align
	{
		get
		{
			return layoutengine.Align;
		}
		set
		{
			if (layoutengine.Align != value)
			{
				layoutengine.Align = value;
				if (((Control)this).IsHandleCreated)
				{
					IOnSizeChanged();
				}
				OnPropertyChanged("Align");
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

	protected override void OnHandleCreated(EventArgs e)
	{
		IOnSizeChanged();
		((Control)this).OnHandleCreated(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		ScrollBar?.Paint(e.Graphics.High());
		((Control)this).OnPaint(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		ScrollBar?.SizeChange(clientRectangle);
		((Control)this).OnSizeChanged(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (ScrollBar == null || !ScrollBar.MouseDown(e.Location))
		{
			((Control)this).OnMouseDown(e);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (ScrollBar == null || !ScrollBar.MouseMove(e.Location))
		{
			((Control)this).OnMouseMove(e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		ScrollBar?.MouseUp();
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
		ScrollBar?.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar?.Dispose();
		base.Dispose(disposing);
	}
}
