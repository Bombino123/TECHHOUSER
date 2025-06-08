using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Breadcrumb 面包屑")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ItemClick")]
public class Breadcrumb : IControl
{
	private int gap = 12;

	private Color? fore;

	private int radius = 4;

	private BreadcrumbItemCollection? items;

	private bool pauseLayout;

	private Rectangle[] hs = new Rectangle[0];

	private readonly StringFormat s_f = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(12)]
	public int Gap
	{
		get
		{
			return gap;
		}
		set
		{
			if (gap != value)
			{
				gap = value;
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("Gap");
			}
		}
	}

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
				OnPropertyChanged("ForeColor");
			}
		}
	}

	[Description("激活文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(4)]
	public int Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				radius = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	[DefaultValue(null)]
	public BreadcrumbItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new BreadcrumbItemCollection(this);
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
					ChangeItems();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	[Description("点击项时发生")]
	[Category("行为")]
	public event BreadcrumbItemEventHandler? ItemClick;

	protected override void OnSizeChanged(EventArgs e)
	{
		ChangeItems();
		((Control)this).OnSizeChanged(e);
	}

	protected override void OnMarginChanged(EventArgs e)
	{
		ChangeItems();
		((Control)this).OnMarginChanged(e);
	}

	protected override void OnPaddingChanged(EventArgs e)
	{
		ChangeItems();
		((Control)this).OnPaddingChanged(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		ChangeItems();
		((Control)this).OnFontChanged(e);
	}

	internal void ChangeItems()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (items == null || items.Count == 0 || pauseLayout)
		{
			return;
		}
		Rectangle rect2 = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		if (rect2.Width == 0 || rect2.Height == 0)
		{
			return;
		}
		Rectangle rect = rect2.PaddingRect(((Control)this).Margin);
		hs = Helper.GDI(delegate(Canvas g)
		{
			List<Rectangle> list = new List<Rectangle>(items.Count);
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int num = (int)(4f * Config.Dpi);
			int num2 = num * 2;
			int num3 = (int)((float)size.Height * 0.8f);
			int num4 = size.Height + num;
			int y = rect.Y + (rect.Height - num4) / 2;
			int y2 = rect.Y + (rect.Height - num3) / 2;
			int num5 = (int)((float)gap * Config.Dpi);
			int num6 = 0;
			int num7 = 0;
			foreach (BreadcrumbItem item in items)
			{
				item.PARENT = this;
				if (item.Text == null || string.IsNullOrEmpty(item.Text))
				{
					Rectangle rectangle = new Rectangle(rect.X + num6, y, num3 + num2, num4);
					if (item.HasIcon)
					{
						Rectangle rect3 = (item.RectText = rectangle);
						item.Rect = rect3;
						item.RectImg = new Rectangle(rectangle.X + num, y2, num3, num3);
					}
					else
					{
						Rectangle rect3 = (item.RectText = rectangle);
						item.Rect = rect3;
					}
				}
				else
				{
					Size size2 = g.MeasureString(item.Text, ((Control)this).Font);
					if (item.HasIcon)
					{
						int num8 = num3 + num2;
						Rectangle rectangle5 = (item.Rect = new Rectangle(rect.X + num6, y, size2.Width + num + num3 + num2, num4));
						item.RectImg = new Rectangle(rectangle5.X + num, y2, num3, num3);
						item.RectText = new Rectangle(item.RectImg.Right + num, rectangle5.Y, rectangle5.Width - num8 - num2, rectangle5.Height);
					}
					else
					{
						Rectangle rect3 = (item.RectText = new Rectangle(rect.X + num6, y, size2.Width + num2, num4));
						item.Rect = rect3;
					}
				}
				num6 += item.Rect.Width + num5;
				if (num7 > 0)
				{
					list.Add(new Rectangle(num7 - num5 + num, y, num5, num4));
				}
				num7 = num6;
			}
			return list.ToArray();
		});
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		if (items == null || items.Count == 0)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		float num = (float)radius * Config.Dpi;
		SolidBrush val = new SolidBrush(fore ?? Colour.TextSecondary.Get("Breadcrumb"));
		try
		{
			SolidBrush val2 = new SolidBrush(ForeActive ?? Colour.Text.Get("Breadcrumb"));
			try
			{
				Rectangle[] array = hs;
				foreach (Rectangle rect in array)
				{
					canvas.String("/", ((Control)this).Font, (Brush)(object)val, rect, s_f);
				}
				for (int j = 0; j < items.Count; j++)
				{
					BreadcrumbItem breadcrumbItem = items[j];
					if (breadcrumbItem == null)
					{
						continue;
					}
					if (j == items.Count - 1)
					{
						PaintImg(canvas, breadcrumbItem, val2.Color, breadcrumbItem.IconSvg, breadcrumbItem.Icon);
						canvas.String(breadcrumbItem.Text, ((Control)this).Font, (Brush)(object)val2, breadcrumbItem.RectText, s_f);
					}
					else if (breadcrumbItem.Hover)
					{
						GraphicsPath val3 = breadcrumbItem.Rect.RoundPath(num);
						try
						{
							canvas.Fill(Colour.FillSecondary.Get("Breadcrumb"), val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
						PaintImg(canvas, breadcrumbItem, val2.Color, breadcrumbItem.IconSvg, breadcrumbItem.Icon);
						canvas.String(breadcrumbItem.Text, ((Control)this).Font, (Brush)(object)val2, breadcrumbItem.RectText, s_f);
					}
					else
					{
						PaintImg(canvas, breadcrumbItem, val.Color, breadcrumbItem.IconSvg, breadcrumbItem.Icon);
						canvas.String(breadcrumbItem.Text, ((Control)this).Font, (Brush)(object)val, breadcrumbItem.RectText, s_f);
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private bool PaintImg(Canvas g, BreadcrumbItem it, Color color, string? svg, Image? bmp)
	{
		if (svg != null)
		{
			if (g.GetImgExtend(svg, it.RectImg, color))
			{
				return false;
			}
		}
		else if (bmp != null)
		{
			g.Image(bmp, it.RectImg);
			return false;
		}
		return true;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (BreadcrumbItem item in items)
		{
			bool flag = item.Rect.Contains(e.Location);
			if (item.Hover != flag)
			{
				item.Hover = flag;
				num2++;
			}
			if (item.Hover)
			{
				num++;
			}
		}
		SetCursor(num > 0);
		if (num2 > 0)
		{
			((Control)this).Invalidate();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (BreadcrumbItem item in items)
		{
			if (item.Hover)
			{
				item.Hover = false;
				num++;
			}
		}
		if (num > 0)
		{
			((Control)this).Invalidate();
		}
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (BreadcrumbItem item in items)
		{
			if (item.Hover)
			{
				item.Hover = false;
				num++;
			}
		}
		if (num > 0)
		{
			((Control)this).Invalidate();
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		((Control)this).OnMouseClick(e);
		if (items == null || items.Count == 0)
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
			BreadcrumbItem breadcrumbItem = items[i];
			if (breadcrumbItem != null && breadcrumbItem.Rect.Contains(e.Location))
			{
				this.ItemClick?.Invoke(this, new BreadcrumbItemEventArgs(breadcrumbItem, e));
				break;
			}
		}
	}
}
