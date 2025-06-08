using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Timeline 时间轴")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ItemClick")]
public class Timeline : IControl
{
	private Color? fore;

	private TimelineItemCollection? items;

	private bool pauseLayout;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private RectangleF[] splits = new RectangleF[0];

	private readonly StringFormat stringFormatLeft = Helper.SF((StringAlignment)1, (StringAlignment)0);

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeColor
	{
		get
		{
			return fore;
		}
		set
		{
			if (!(fore == value))
			{
				fore = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("描述字体")]
	[Category("外观")]
	[DefaultValue(null)]
	public Font? FontDescription { get; set; }

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? Gap { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	public TimelineItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new TimelineItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
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
					ChangeList();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	[Description("点击项时发生")]
	[Category("行为")]
	public event TimelineEventHandler? ItemClick;

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		((Control)this).OnSizeChanged(e);
	}

	internal Rectangle ChangeList()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Rectangle rect = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
		if (pauseLayout || items == null || items.Count == 0)
		{
			return rect;
		}
		if (rect.Width == 0 || rect.Height == 0)
		{
			return rect;
		}
		int y = rect.Y;
		Helper.GDI(delegate(Canvas g)
		{
			int height = g.MeasureString("龍Qq", ((Control)this).Font).Height;
			float num = (float)height * 0.136f;
			float num2 = num * 0.666f;
			float num3 = num2 * 2f;
			int num4 = (int)Math.Round(8f * Config.Dpi);
			int num5 = (int)Math.Round((double)height * 1.1);
			int num6 = (int)Math.Round((double)height * 0.846);
			int num7 = (int)Math.Round(((double?)((float?)Gap * Config.Dpi)) ?? ((double)height * 0.91));
			int num8 = (int)Math.Round((double)height * 0.636);
			int width = rect.Width - num8 - num6 - num5 * 2;
			y += num5;
			List<RectangleF> list = new List<RectangleF>(items.Count);
			int num9 = 0;
			Font font = FontDescription ?? ((Control)this).Font;
			int num10 = num4 * 2;
			foreach (TimelineItem item in items)
			{
				item.PARENT = this;
				item.pen_w = num;
				if (item.Visible)
				{
					Size size = g.MeasureString(item.Text, ((Control)this).Font, width);
					item.ico_rect = new Rectangle(rect.X + num5, y + (height - num8) / 2, num8, num8);
					item.txt_rect = new Rectangle(item.ico_rect.Right + num6, y, size.Width, size.Height);
					if (!string.IsNullOrEmpty(item.Description))
					{
						Size size2 = g.MeasureString(item.Description, font, width);
						item.description_rect = new Rectangle(item.txt_rect.X, item.txt_rect.Bottom + num4, size2.Width, size2.Height);
						y += num4 * 2 + size2.Height;
					}
					item.rect = new Rectangle(item.ico_rect.X - num4, y - num4, item.txt_rect.Width + num8 + num6 + num10, size.Height + num10);
					y += size.Height + num7;
					if (num9 > 0)
					{
						TimelineItem timelineItem = items[num9 - 1];
						if (timelineItem != null)
						{
							list.Add(new RectangleF((float)item.ico_rect.X + ((float)num8 - num2) / 2f, (float)timelineItem.ico_rect.Bottom + num3, num2, (float)(item.ico_rect.Y - timelineItem.ico_rect.Bottom) - num3 * 2f));
						}
					}
				}
				num9++;
			}
			splits = list.ToArray();
			y = y - num7 + num5;
		});
		ScrollBar.SetVrSize(y);
		return rect;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		ScrollBar.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	public Timeline()
	{
		ScrollBar = new ScrollBar(this);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		if (items == null || items.Count == 0)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		canvas.TranslateTransform(0f, -ScrollBar.Value);
		Color color = fore ?? Colour.Text.Get("Timeline");
		SolidBrush val = new SolidBrush(Colour.Split.Get("Timeline"));
		try
		{
			RectangleF[] array = splits;
			foreach (RectangleF rect in array)
			{
				canvas.Fill((Brush)(object)val, rect);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		Font font = FontDescription ?? ((Control)this).Font;
		SolidBrush val2 = new SolidBrush(color);
		try
		{
			SolidBrush val3 = new SolidBrush(Colour.TextTertiary.Get("Timeline"));
			try
			{
				SolidBrush val4 = new SolidBrush(Colour.BgBase.Get("Timeline"));
				try
				{
					foreach (TimelineItem item in items)
					{
						if (item.Visible)
						{
							canvas.String(item.Text, ((Control)this).Font, (Brush)(object)val2, item.txt_rect, stringFormatLeft);
							canvas.String(item.Description, font, (Brush)(object)val3, item.description_rect, stringFormatLeft);
							if (PaintIcon(canvas, item, color))
							{
								Color color2 = (item.Fill.HasValue ? item.Fill.Value : (item.Type switch
								{
									TTypeMini.Error => Colour.Error.Get("Timeline"), 
									TTypeMini.Success => Colour.Success.Get("Timeline"), 
									TTypeMini.Info => Colour.Info.Get("Timeline"), 
									TTypeMini.Warn => Colour.Warning.Get("Timeline"), 
									TTypeMini.Default => Colour.TextQuaternary.Get("Timeline"), 
									_ => Colour.Primary.Get("Timeline"), 
								}));
								canvas.FillEllipse((Brush)(object)val4, item.ico_rect);
								canvas.DrawEllipse(color2, item.pen_w, item.ico_rect);
							}
						}
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas);
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private bool PaintIcon(Canvas g, TimelineItem it, Color fore)
	{
		if (it.Icon != null)
		{
			g.Image(it.Icon, it.ico_rect);
			return false;
		}
		if (it.IconSvg != null && g.GetImgExtend(it.IconSvg, it.ico_rect, fore))
		{
			return false;
		}
		return true;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
		if (ScrollBar.MouseDown(e.Location))
		{
			OnTouchDown(e.X, e.Y);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		ScrollBar.MouseUp();
		OnTouchUp();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (!ScrollBar.MouseMove(e.Location) || items == null || items.Count == 0 || this.ItemClick == null)
		{
			return;
		}
		if (OnTouchMove(e.X, e.Y))
		{
			for (int i = 0; i < items.Count; i++)
			{
				TimelineItem timelineItem = items[i];
				if (timelineItem != null && timelineItem.rect.Contains(e.X, e.Y + ScrollBar.Value))
				{
					SetCursor(val: true);
					return;
				}
			}
		}
		SetCursor(val: false);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar.Leave();
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ScrollBar.Leave();
	}

	protected override bool OnTouchScrollX(int value)
	{
		return ScrollBar.MouseWheelX(value);
	}

	protected override bool OnTouchScrollY(int value)
	{
		return ScrollBar.MouseWheelY(value);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		((Control)this).OnMouseClick(e);
		if (items == null || items.Count == 0 || this.ItemClick == null)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			TimelineItem timelineItem = items[i];
			if (timelineItem != null && timelineItem.rect.Contains(e.X, e.Y + ScrollBar.Value))
			{
				this.ItemClick(this, new TimelineItemEventArgs(timelineItem, e));
				break;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}
}
