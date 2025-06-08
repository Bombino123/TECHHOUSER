using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

[Description("CollapseGroup 折叠分组面板")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ItemClick")]
public class CollapseGroup : IControl
{
	public class ItemClickEventArgs : VMEventArgs<CollapseGroupSub>
	{
		public Rectangle Rect { get; private set; }

		public ItemClickEventArgs(CollapseGroupSub item, Rectangle rect, MouseEventArgs e)
			: base(item, e)
		{
			Rect = rect;
		}
	}

	public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

	private Color? fore;

	private int radius = 6;

	private int columnCount = 6;

	private CollapseGroupItemCollection? items;

	private bool pauseLayout;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private readonly StringFormat s_c = Helper.SF((StringAlignment)0, (StringAlignment)1);

	private readonly StringFormat s_l = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private object? MDown;

	[Description("悬停背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? BackHover { get; set; }

	[Description("激活背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? BackActive { get; set; }

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
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

	[Description("激活字体颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	public Color? ForeActive { get; set; }

	[Description("只保持一个子菜单的展开")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Unique { get; set; }

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(6)]
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

	[Description("列数量")]
	[Category("外观")]
	[DefaultValue(6)]
	public int ColumnCount
	{
		get
		{
			return columnCount;
		}
		set
		{
			if (columnCount != value)
			{
				columnCount = value;
				ChangeList();
				((Control)this).Invalidate();
				OnPropertyChanged("ColumnCount");
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	public CollapseGroupItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new CollapseGroupItemCollection(this);
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

	[Description("点击项事件")]
	[Category("行为")]
	public event ItemClickEventHandler? ItemClick;

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		((Control)this).OnSizeChanged(e);
	}

	internal Rectangle ChangeList()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (pauseLayout || items == null || items.Count == 0 || clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return clientRectangle;
		}
		Rectangle rect = ((Control)this).ClientRectangle.DeflateRect(((Control)this).Padding);
		int y = rect.Y;
		Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int num = (int)(4f * Config.Dpi);
			int num2 = (rect.Width - num * (columnCount - 1)) / columnCount;
			int icon_size = num2 / 2;
			int num3 = size.Height + num * 2;
			foreach (CollapseGroupItem item in items)
			{
				item.PARENT = this;
				item.SetRect(g, new Rectangle(rect.X, y, rect.Width, num3), size.Height, num);
				y += num3;
				if (item.CanExpand)
				{
					int num4 = y;
					ChangeList(g, rect, item, item.Sub, ref y, size.Height, num2, icon_size, num);
					item.SubY = num4;
					item.SubHeight = y - num4;
					if ((item.Expand || item.ExpandThread) && item.ExpandProg > 0f)
					{
						item.ExpandHeight = y - num4;
						y = num4 + (int)(item.ExpandHeight * item.ExpandProg);
					}
					else if (!item.Expand)
					{
						y = num4;
					}
				}
			}
		});
		ScrollBar.SetVrSize(0, y);
		return clientRectangle;
	}

	private void ChangeList(Canvas g, Rectangle rect, CollapseGroupItem Parent, CollapseGroupSubCollection items, ref int y, int font_height, int csize, int icon_size, int gap)
	{
		int num = 0;
		int num2 = 0;
		foreach (CollapseGroupSub item in items)
		{
			item.PARENT = this;
			item.PARENTITEM = Parent;
			int num3 = g.MeasureString(item.Text, ((Control)this).Font, csize, s_c).Height - font_height;
			if (num3 > 0 && num2 < num3)
			{
				num2 = num3;
			}
			item.SetRect(g, new Rectangle(rect.X + (csize + gap) * num, y, csize, csize), font_height, num3, icon_size);
			num++;
			if (num > columnCount - 1)
			{
				y += csize + gap + num2;
				num = (num2 = 0);
			}
		}
		if (num > 0)
		{
			y += csize + gap + num2;
		}
	}

	public CollapseGroup()
	{
		ScrollBar = new ScrollBar(this, enabledY: true, enabledX: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
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
		float num = (float)radius * Config.Dpi;
		int valueX = ScrollBar.ValueX;
		int valueY = ScrollBar.ValueY;
		canvas.TranslateTransform(-valueX, -valueY);
		SolidBrush val = new SolidBrush(fore ?? Colour.TextBase.Get("CollapseGroup"));
		try
		{
			SolidBrush val2 = new SolidBrush(ForeActive ?? Colour.Primary.Get("CollapseGroup"));
			try
			{
				SolidBrush val3 = new SolidBrush(BackHover ?? Colour.FillSecondary.Get("CollapseGroup"));
				try
				{
					SolidBrush val4 = new SolidBrush(BackActive ?? Colour.PrimaryBg.Get("CollapseGroup"));
					try
					{
						SolidBrush val5 = new SolidBrush(Colour.TextQuaternary.Get("CollapseGroup"));
						try
						{
							PaintItem(canvas, clientRectangle, valueX, valueY, items, val, val2, val3, val4, val5, num);
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
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
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas);
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintItem(Canvas g, Rectangle rect, int sx, int sy, CollapseGroupItemCollection items, SolidBrush fore, SolidBrush fore_active, SolidBrush hover, SolidBrush active, SolidBrush brush_TextQuaternary, float radius)
	{
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		foreach (CollapseGroupItem item in items)
		{
			if (!item.Show)
			{
				continue;
			}
			PaintArrow(g, item, fore, sx, sy);
			g.String(item.Text, ((Control)this).Font, (Brush)(object)fore, item.txt_rect, s_l);
			if ((!item.Expand && !item.ExpandThread) || item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			if (item.ExpandThread)
			{
				g.SetClip(new RectangleF(rect.X, item.rect.Bottom, rect.Width, item.ExpandHeight * item.ExpandProg));
			}
			foreach (CollapseGroupSub item2 in item.items)
			{
				if (!item2.Show)
				{
					continue;
				}
				if (item2.Enabled)
				{
					if (item2.Select)
					{
						PaintBack(g, item2, active, radius);
						if (item2.AnimationHover)
						{
							SolidBrush val = new SolidBrush(Helper.ToColorN(item2.AnimationHoverValue, hover.Color));
							try
							{
								PaintBack(g, item2, val, radius);
							}
							finally
							{
								((IDisposable)val)?.Dispose();
							}
						}
						else if (item2.Hover)
						{
							PaintBack(g, item2, hover, radius);
						}
						if (item2.Icon != null)
						{
							g.Image(item2.Icon, item2.ico_rect);
						}
						else if (item2.IconSvg != null)
						{
							g.GetImgExtend(item2.IconSvg, item2.ico_rect, fore_active.Color);
						}
						g.String(item2.Text, ((Control)this).Font, (Brush)(object)fore_active, item2.txt_rect, s_c);
						continue;
					}
					if (item2.AnimationHover)
					{
						SolidBrush val2 = new SolidBrush(Helper.ToColorN(item2.AnimationHoverValue, hover.Color));
						try
						{
							PaintBack(g, item2, val2, radius);
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
					else if (item2.Hover)
					{
						PaintBack(g, item2, hover, radius);
					}
					if (item2.Icon != null)
					{
						g.Image(item2.Icon, item2.ico_rect);
					}
					else if (item2.IconSvg != null)
					{
						g.GetImgExtend(item2.IconSvg, item2.ico_rect, fore.Color);
					}
					g.String(item2.Text, ((Control)this).Font, (Brush)(object)fore, item2.txt_rect, s_c);
				}
				else
				{
					if (item2.Icon != null)
					{
						g.Image(item2.Icon, item2.ico_rect);
					}
					else if (item2.IconSvg != null)
					{
						g.GetImgExtend(item2.IconSvg, item2.ico_rect, brush_TextQuaternary.Color);
					}
					g.String(item2.Text, ((Control)this).Font, (Brush)(object)brush_TextQuaternary, item2.txt_rect, s_c);
				}
			}
			g.ResetClip();
		}
	}

	private void PaintBack(Canvas g, CollapseGroupSub sub, SolidBrush brush, float radius)
	{
		if (radius > 0f)
		{
			GraphicsPath val = sub.rect.RoundPath(radius);
			try
			{
				g.Fill((Brush)(object)brush, val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Fill((Brush)(object)brush, sub.rect);
	}

	internal PointF[] PaintArrow(RectangleF rect)
	{
		float num = rect.Height * 0.15f;
		float num2 = rect.Height * 0.2f;
		float num3 = rect.Height * 0.26f;
		return new PointF[3]
		{
			new PointF(rect.X + num, rect.Y + rect.Height / 2f),
			new PointF(rect.X + rect.Width * 0.4f, rect.Y + (rect.Height - num3)),
			new PointF(rect.X + rect.Width - num2, rect.Y + num2)
		};
	}

	private void PaintArrow(Canvas g, CollapseGroupItem item, SolidBrush color, int sx, int sy)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Pen val = new Pen((Brush)(object)color, 2f);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			if (item.ExpandThread)
			{
				g.DrawLines(val, item.arr_rect.TriangleLines(0f - (1f - 2f * item.ExpandProg), 0.4f));
			}
			else if (item.Expand)
			{
				g.DrawLines(val, item.arr_rect.TriangleLines(1f, 0.4f));
			}
			else
			{
				g.DrawLines(val, item.arr_rect.TriangleLines(-1f, 0.4f));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintArrow(Canvas g, CollapseGroupItem item, Pen pen, int sx, int sy, float rotate)
	{
		int num = item.arr_rect.Width / 2;
		g.TranslateTransform(item.arr_rect.X + num, item.arr_rect.Y + num);
		g.RotateTransform(rotate);
		g.DrawLines(pen, new Rectangle(-num, -num, item.arr_rect.Width, item.arr_rect.Height).TriangleLines(-1f, 0.4f));
		g.ResetTransform();
		g.TranslateTransform(-sx, -sy);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
		MDown = null;
		if (!ScrollBar.MouseDownY(e.Location) || !ScrollBar.MouseDownX(e.Location) || items == null || items.Count == 0)
		{
			return;
		}
		int valueY = ScrollBar.ValueY;
		OnTouchDown(e.X, e.Y);
		foreach (CollapseGroupItem item in items)
		{
			if (item.rect.Contains(e.X, e.Y + valueY))
			{
				MDown = item;
				break;
			}
			if (!item.Expand || item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			foreach (CollapseGroupSub item2 in item.items)
			{
				if (item2.Show && item2.Enabled && item2.rect.Contains(e.X, e.Y + valueY))
				{
					MDown = item2;
					return;
				}
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (ScrollBar.MouseMoveY(e.Location) && ScrollBar.MouseMoveX(e.Location))
		{
			int num = 0;
			if (items == null || items.Count == 0)
			{
				return;
			}
			int valueY = ScrollBar.ValueY;
			if (OnTouchMove(e.X, e.Y))
			{
				foreach (CollapseGroupItem item in items)
				{
					if (item.rect.Contains(e.X, e.Y + valueY))
					{
						num++;
					}
					else
					{
						if (!item.Expand || item.items == null || item.items.Count <= 0)
						{
							continue;
						}
						foreach (CollapseGroupSub item2 in item.items)
						{
							item2.Hover = item2.Show && item2.Enabled && item2.rect.Contains(e.X, e.Y + valueY);
							if (item2.Hover)
							{
								num++;
							}
						}
					}
				}
			}
			SetCursor(num > 0);
		}
		else
		{
			ILeave();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (!ScrollBar.MouseUpY() || !ScrollBar.MouseUpX() || !OnTouchUp() || items == null || items.Count == 0 || MDown == null)
		{
			return;
		}
		int valueY = ScrollBar.ValueY;
		foreach (CollapseGroupItem item in items)
		{
			if (item == MDown)
			{
				if (item.rect.Contains(e.X, e.Y + valueY))
				{
					item.Expand = !item.Expand;
					if (item.Expand && Unique)
					{
						foreach (CollapseGroupItem item2 in items)
						{
							if (item2 != item)
							{
								item2.Expand = false;
							}
						}
					}
				}
				MDown = null;
				break;
			}
			if (item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			foreach (CollapseGroupSub item3 in item.items)
			{
				if (MDown == item3)
				{
					if (item3.rect.Contains(e.X, e.Y + valueY))
					{
						item3.Select = true;
						this.ItemClick?.Invoke(this, new ItemClickEventArgs(item3, new Rectangle(item3.rect.X, item3.rect.Y - valueY, item3.rect.Width, item3.rect.Height), e));
					}
					MDown = null;
					return;
				}
			}
		}
	}

	public void IUSelect()
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (CollapseGroupItem item in items)
		{
			if (item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			foreach (CollapseGroupSub item2 in item.items)
			{
				item2.Select = false;
			}
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar.Leave();
		ILeave();
	}

	private void ILeave()
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (CollapseGroupItem item in items)
		{
			if (item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			foreach (CollapseGroupSub item2 in item.items)
			{
				item2.Hover = false;
			}
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		ScrollBar.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	protected override bool OnTouchScrollX(int value)
	{
		return ScrollBar.MouseWheelX(value);
	}

	protected override bool OnTouchScrollY(int value)
	{
		return ScrollBar.MouseWheelY(value);
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}
}
