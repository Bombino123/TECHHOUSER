using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Tree 树形控件")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("SelectChanged")]
public class Tree : IControl
{
	private Color? fore;

	private float iconratio = 1f;

	private int radius = 6;

	private int _gap = 8;

	private bool round;

	private bool checkable;

	private bool blockNode;

	private TreeItemCollection? items;

	private TreeItem? selectItem;

	private bool pauseLayout;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private float check_radius;

	private readonly StringFormat s_c = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_l = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private TreeItem? MDown;

	private bool doubleClick;

	[Description("悬停背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackHover { get; set; }

	[Description("激活背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackActive { get; set; }

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

	[Description("激活字体颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(1f)]
	public float IconRatio
	{
		get
		{
			return iconratio;
		}
		set
		{
			if (iconratio != value)
			{
				iconratio = value;
				if (((Control)this).IsHandleCreated)
				{
					ChangeList();
					((Control)this).Invalidate();
				}
			}
		}
	}

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
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(8)]
	public int Gap
	{
		get
		{
			return _gap;
		}
		set
		{
			if (_gap != value)
			{
				_gap = value;
				ChangeList();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("圆角样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Round
	{
		get
		{
			return round;
		}
		set
		{
			if (round != value)
			{
				round = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("节点前添加 Checkbox 复选框")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Checkable
	{
		get
		{
			return checkable;
		}
		set
		{
			if (checkable != value)
			{
				checkable = value;
				ChangeList();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("Checkable 状态下节点选择完全受控（父子节点选中状态不再关联）")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool CheckStrictly { get; set; } = true;


	[Description("节点占据一行")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool BlockNode
	{
		get
		{
			return blockNode;
		}
		set
		{
			if (blockNode != value)
			{
				blockNode = value;
				ChangeList();
				((Control)this).Invalidate();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	public TreeItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new TreeItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Browsable(false)]
	[Description("选择项")]
	[Category("数据")]
	[DefaultValue(null)]
	public TreeItem? SelectItem
	{
		get
		{
			return selectItem;
		}
		set
		{
			selectItem = value;
			if (value == null)
			{
				USelect();
			}
			else
			{
				Select(value);
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
					ChangeList();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	[Description("Select 属性值更改时发生")]
	[Category("行为")]
	public event TreeSelectEventHandler? SelectChanged;

	[Description("Checked 属性值更改时发生")]
	[Category("行为")]
	public event TreeCheckedEventHandler? CheckedChanged;

	[Description("点击项事件")]
	[Category("行为")]
	public event TreeSelectEventHandler? NodeMouseClick;

	[Description("双击项事件")]
	[Category("行为")]
	public event TreeSelectEventHandler? NodeMouseDoubleClick;

	[Description("移动项事件")]
	[Category("行为")]
	public event TreeHoverEventHandler? NodeMouseMove;

	internal void OnNodeMouseMove(TreeItem item, bool hover)
	{
		if (this.NodeMouseMove != null)
		{
			int valueX = ScrollBar.ValueX;
			int valueY = ScrollBar.ValueY;
			this.NodeMouseMove(this, new TreeHoverEventArgs(item, item.Rect("Text", valueX, valueY), hover));
		}
	}

	internal void OnSelectChanged(TreeItem item, MouseEventArgs args)
	{
		if (this.SelectChanged != null)
		{
			int valueX = ScrollBar.ValueX;
			int valueY = ScrollBar.ValueY;
			this.SelectChanged(this, new TreeSelectEventArgs(item, item.Rect("Text", valueX, valueY), args));
		}
	}

	internal void OnNodeMouseClick(TreeItem item, MouseEventArgs args)
	{
		if (this.NodeMouseClick != null)
		{
			int valueX = ScrollBar.ValueX;
			int valueY = ScrollBar.ValueY;
			this.NodeMouseClick(this, new TreeSelectEventArgs(item, item.Rect("Text", valueX, valueY), args));
		}
	}

	internal void OnNodeMouseDoubleClick(TreeItem item, MouseEventArgs args)
	{
		if (this.NodeMouseDoubleClick != null)
		{
			int valueX = ScrollBar.ValueX;
			int valueY = ScrollBar.ValueY;
			this.NodeMouseDoubleClick(this, new TreeSelectEventArgs(item, item.Rect("Text", valueX, valueY), args));
		}
	}

	internal void OnCheckedChanged(TreeItem item, bool value)
	{
		this.CheckedChanged?.Invoke(this, new TreeCheckedEventArgs(item, value));
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		((Control)this).OnSizeChanged(e);
	}

	internal Rectangle ChangeList()
	{
		Rectangle rect = ((Control)this).ClientRectangle;
		if (pauseLayout || items == null || items.Count == 0 || rect.Width == 0 || rect.Height == 0)
		{
			return rect;
		}
		int x = 0;
		int y = 0;
		bool has = HasSub(items);
		Helper.GDI(delegate(Canvas g)
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Invalid comparison between Unknown and I4
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Invalid comparison between Unknown and I4
			int num = (int)((float)g.MeasureString("龍Qq", ((Control)this).Font).Height * iconratio);
			int num2 = (int)((float)_gap * Config.Dpi);
			int gapI = num2 / 2;
			int height = num + num2 * 2;
			check_radius = (float)num * 0.2f;
			if (CheckStrictly && has && items[0].PARENT == null && items[0].PARENTITEM == null)
			{
				List<TreeItem> dir = new List<TreeItem>();
				TestSub(ref dir, items);
				foreach (TreeItem item in dir)
				{
					int num3 = 0;
					foreach (TreeItem item2 in item.Sub)
					{
						if ((int)item2.CheckState == 1 || (int)item2.CheckState == 2)
						{
							num3++;
						}
					}
					if (num3 > 0)
					{
						item.CheckState = (CheckState)((num3 == item.Sub.Count) ? 1 : 2);
					}
					else
					{
						item.CheckState = (CheckState)0;
					}
				}
			}
			ChangeList(g, rect, null, items, has, ref x, ref y, height, num, num2, gapI, 0, expand: true);
		});
		ScrollBar.SetVrSize(x, y);
		return rect;
	}

	private bool HasSub(TreeItemCollection items)
	{
		foreach (TreeItem item in items)
		{
			if (item.CanExpand)
			{
				return true;
			}
		}
		return false;
	}

	private void TestSub(ref List<TreeItem> dir, TreeItemCollection items)
	{
		foreach (TreeItem item in items)
		{
			if (item.CanExpand)
			{
				dir.Insert(0, item);
				TestSub(ref dir, item.Sub);
			}
		}
	}

	private void ChangeList(Canvas g, Rectangle rect, TreeItem? Parent, TreeItemCollection items, bool has_sub, ref int x, ref int y, int height, int icon_size, int gap, int gapI, int depth, bool expand)
	{
		foreach (TreeItem item in items)
		{
			item.PARENT = this;
			item.PARENTITEM = Parent;
			item.SetRect(g, ((Control)this).Font, depth, checkable, blockNode, has_sub, new Rectangle(0, y, rect.Width, height), icon_size, gap);
			if (expand && item.txt_rect.Right > x)
			{
				x = item.txt_rect.Right;
			}
			if (!item.Show || !item.Visible)
			{
				continue;
			}
			y += height + gapI;
			if (item.CanExpand)
			{
				int num = y;
				ChangeList(g, rect, item, item.Sub, has_sub, ref x, ref y, height, icon_size, gap, gapI, depth + 1, expand && item.Expand);
				item.SubY = num - gapI / 2;
				item.SubHeight = y - num;
				if ((item.Expand || item.ExpandThread) && item.ExpandProg > 0f)
				{
					item.ExpandHeight = y - num;
					y = num + (int)(item.ExpandHeight * item.ExpandProg);
				}
				else if (!item.Expand)
				{
					y = num;
				}
			}
		}
	}

	public Tree()
	{
		ScrollBar = new ScrollBar(this, enabledY: true, enabledX: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
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
		int valueX = ScrollBar.ValueX;
		int valueY = ScrollBar.ValueY;
		canvas.TranslateTransform(-valueX, -valueY);
		float num = (float)radius * Config.Dpi;
		SolidBrush val = new SolidBrush(fore ?? Colour.TextBase.Get("Tree"));
		try
		{
			SolidBrush val2 = new SolidBrush(ForeActive ?? Colour.Primary.Get("Tree"));
			try
			{
				SolidBrush val3 = new SolidBrush(BackHover ?? Colour.FillSecondary.Get("Tree"));
				try
				{
					SolidBrush val4 = new SolidBrush(BackActive ?? Colour.PrimaryBg.Get("Tree"));
					try
					{
						SolidBrush val5 = new SolidBrush(Colour.TextTertiary.Get("Tree"));
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

	private void PaintItem(Canvas g, Rectangle rect, int sx, int sy, TreeItemCollection items, SolidBrush fore, SolidBrush fore_active, SolidBrush hover, SolidBrush active, SolidBrush brushTextTertiary, float radius)
	{
		foreach (TreeItem item in items)
		{
			item.show = item.Show && item.Visible && (float)item.rect.Y > (float)(sy - rect.Height) - (item.Expand ? item.SubHeight : 0f) && item.rect.Bottom < sy + rect.Height + item.rect.Height;
			if (!item.show)
			{
				continue;
			}
			PaintItem(g, item, fore, fore_active, hover, active, brushTextTertiary, radius, sx, sy);
			if ((item.Expand || item.ExpandThread) && item.items != null && item.items.Count > 0)
			{
				GraphicsState state = g.Save();
				if (item.ExpandThread)
				{
					g.SetClip(new RectangleF(rect.X, item.rect.Bottom, rect.Width, item.ExpandHeight * item.ExpandProg));
				}
				PaintItem(g, rect, sx, sy, item.items, fore, fore_active, hover, active, brushTextTertiary, radius);
				g.Restore(state);
			}
		}
	}

	private void PaintItem(Canvas g, TreeItem item, SolidBrush fore, SolidBrush fore_active, SolidBrush hover, SolidBrush active, SolidBrush brushTextTertiary, float radius, int sx, int sy)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Invalid comparison between Unknown and I4
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Invalid comparison between Unknown and I4
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Invalid comparison between Unknown and I4
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Invalid comparison between Unknown and I4
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Expected O, but got Unknown
		if (item.Select)
		{
			if (blockNode)
			{
				g.ResetTransform();
				g.TranslateTransform(0f, -sy);
				PaintBack(g, active, item.rect, radius);
				g.TranslateTransform(-sx, 0f);
			}
			else
			{
				PaintBack(g, active, item.rect, radius);
			}
			if (item.CanExpand)
			{
				PaintArrow(g, item, fore_active, sx, sy);
			}
			PaintItemText(g, item, fore_active, brushTextTertiary);
		}
		else
		{
			if (item.Back.HasValue)
			{
				SolidBrush val = new SolidBrush(item.Back.Value);
				try
				{
					PaintBack(g, val, item.rect, radius);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			if (blockNode)
			{
				g.ResetTransform();
				g.TranslateTransform(0f, -sy);
				if (item.AnimationHover)
				{
					SolidBrush val2 = new SolidBrush(Helper.ToColorN(item.AnimationHoverValue, hover.Color));
					try
					{
						PaintBack(g, val2, item.rect, radius);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				else if (item.Hover)
				{
					PaintBack(g, hover, item.rect, radius);
				}
				g.TranslateTransform(-sx, 0f);
			}
			else if (item.AnimationHover)
			{
				SolidBrush val3 = new SolidBrush(Helper.ToColorN(item.AnimationHoverValue, hover.Color));
				try
				{
					PaintBack(g, val3, item.rect, radius);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			else if (item.Hover)
			{
				PaintBack(g, hover, item.rect, radius);
			}
			if (item.CanExpand)
			{
				PaintArrow(g, item, fore, sx, sy);
			}
			if (item.Enabled)
			{
				PaintItemText(g, item, fore, brushTextTertiary);
			}
			else
			{
				SolidBrush val4 = new SolidBrush(Colour.TextQuaternary.Get("Tree"));
				try
				{
					PaintItemText(g, item, val4, brushTextTertiary);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
		}
		if (!checkable)
		{
			return;
		}
		GraphicsPath val5 = item.check_rect.RoundPath(check_radius, round: false);
		try
		{
			float width = 2f * Config.Dpi;
			if (item.Enabled)
			{
				if (item.AnimationCheck)
				{
					float alpha = 255f * item.AnimationCheckValue;
					if ((int)item.CheckState == 2 || ((int)item.checkStateOld == 2 && !item.Checked))
					{
						g.Draw(Colour.BorderColor.Get("Tree"), width, val5);
						g.Fill(Helper.ToColor(alpha, Colour.Primary.Get("Tree")), PaintBlock(item.check_rect));
						return;
					}
					_ = item.check_rect.Width;
					g.Fill(Helper.ToColor(alpha, Colour.Primary.Get("Tree")), val5);
					g.DrawLines(Helper.ToColor(alpha, Colour.BgBase.Get("Tree")), 3f * Config.Dpi, PaintArrow(item.check_rect));
					if (item.Checked)
					{
						float num = (float)item.check_rect.Height + (float)item.check_rect.Height * item.AnimationCheckValue;
						SolidBrush val6 = new SolidBrush(Helper.ToColor(100f * (1f - item.AnimationCheckValue), Colour.Primary.Get("Tree")));
						try
						{
							g.FillEllipse((Brush)(object)val6, new RectangleF((float)item.check_rect.X + ((float)item.check_rect.Width - num) / 2f, (float)item.check_rect.Y + ((float)item.check_rect.Height - num) / 2f, num, num));
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					g.Draw(Colour.Primary.Get("Tree"), 2f * Config.Dpi, val5);
				}
				else if ((int)item.CheckState == 2)
				{
					g.Draw(Colour.BorderColor.Get("Tree"), width, val5);
					g.Fill(Colour.Primary.Get("Tree"), PaintBlock(item.check_rect));
				}
				else if (item.Checked)
				{
					g.Fill(Colour.Primary.Get("Tree"), val5);
					g.DrawLines(Colour.BgBase.Get("Tree"), width, PaintArrow(item.check_rect));
				}
				else
				{
					g.Draw(Colour.BorderColor.Get("Tree"), width, val5);
				}
			}
			else
			{
				g.Fill(Colour.FillQuaternary.Get("Tree"), val5);
				if ((int)item.CheckState == 2)
				{
					g.Fill(Colour.TextQuaternary.Get("Tree"), PaintBlock(item.check_rect));
				}
				else if (item.Checked)
				{
					g.DrawLines(Colour.TextQuaternary.Get("Tree"), width, PaintArrow(item.check_rect));
				}
				g.Draw(Colour.BorderColorDisable.Get("Tree"), width, val5);
			}
		}
		finally
		{
			((IDisposable)val5)?.Dispose();
		}
	}

	private void PaintItemText(Canvas g, TreeItem item, SolidBrush fore, SolidBrush brushTextTertiary)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		Color color = fore.Color;
		if (item.Fore.HasValue)
		{
			color = item.Fore.Value;
			SolidBrush val = new SolidBrush(color);
			try
			{
				g.String(item.Text, ((Control)this).Font, (Brush)(object)val, item.txt_rect, blockNode ? s_l : s_c);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			g.String(item.Text, ((Control)this).Font, (Brush)(object)fore, item.txt_rect, blockNode ? s_l : s_c);
		}
		if (item.SubTitle != null)
		{
			g.String(item.SubTitle, ((Control)this).Font, (Brush)(object)brushTextTertiary, item.subtxt_rect, s_l);
		}
		if (item.Icon != null)
		{
			g.Image(item.Icon, item.ico_rect);
		}
		if (item.IconSvg != null)
		{
			g.GetImgExtend(item.IconSvg, item.ico_rect, color);
		}
	}

	internal RectangleF PaintBlock(RectangleF rect)
	{
		float num = rect.Height * 0.2f;
		float num2 = num * 2f;
		return new RectangleF(rect.X + num, rect.Y + num, rect.Width - num2, rect.Height - num2);
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

	private void PaintArrow(Canvas g, TreeItem item, SolidBrush color, int sx, int sy)
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
				PaintArrow(g, item, val, sx, sy, -90f + 90f * item.ExpandProg);
			}
			else if (item.Expand)
			{
				g.DrawLines(val, item.arr_rect.TriangleLines(-1f, 0.4f));
			}
			else
			{
				PaintArrow(g, item, val, sx, sy, -90f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintArrow(Canvas g, TreeItem item, Pen pen, int sx, int sy, float rotate)
	{
		int num = item.arr_rect.Width / 2;
		g.TranslateTransform(item.arr_rect.X + num, item.arr_rect.Y + num);
		g.RotateTransform(rotate);
		g.DrawLines(pen, new Rectangle(-num, -num, item.arr_rect.Width, item.arr_rect.Height).TriangleLines(-1f, 0.4f));
		g.ResetTransform();
		g.TranslateTransform(-sx, -sy);
	}

	private void PaintBack(Canvas g, SolidBrush brush, Rectangle rect, float radius)
	{
		if (round || radius > 0f)
		{
			GraphicsPath val = rect.RoundPath(radius, round);
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
		g.Fill((Brush)(object)brush, rect);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
		doubleClick = e.Clicks > 1;
		MDown = null;
		if (!ScrollBar.MouseDownY(e.Location) || !ScrollBar.MouseDownX(e.Location) || items == null || items.Count == 0)
		{
			return;
		}
		OnTouchDown(e.X, e.Y);
		foreach (TreeItem item in items)
		{
			if (IMouseDown(e, item))
			{
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (!ScrollBar.MouseUpY() || !ScrollBar.MouseUpX() || !OnTouchUp() || items == null || items.Count == 0 || MDown == null)
		{
			return;
		}
		foreach (TreeItem item in items)
		{
			if (IMouseUp(e, item, MDown))
			{
				break;
			}
		}
	}

	private bool IMouseDown(MouseEventArgs e, TreeItem item)
	{
		if (item.Contains(e.X, e.Y, ScrollBar.ValueX, ScrollBar.ValueY, checkable, blockNode) > 0)
		{
			MDown = item;
			return true;
		}
		if (item.CanExpand && item.Expand)
		{
			foreach (TreeItem item2 in item.Sub)
			{
				if (IMouseDown(e, item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IMouseUp(MouseEventArgs e, TreeItem item, TreeItem MDown)
	{
		bool canExpand = item.CanExpand;
		if (MDown == item)
		{
			int num = item.Contains(e.X, e.Y, ScrollBar.ValueX, ScrollBar.ValueY, checkable, blockNode);
			if (num > 0)
			{
				if (blockNode)
				{
					if (canExpand)
					{
						item.Expand = !item.Expand;
					}
					selectItem = item;
					item.Select = true;
					OnSelectChanged(item, e);
					((Control)this).Invalidate();
				}
				else if (num == 3 && item.Enabled)
				{
					item.Checked = !item.Checked;
					if (CheckStrictly)
					{
						SetCheck(item, item.Checked);
						SetCheckStrictly(item.PARENTITEM);
					}
				}
				else if (num == 2 && canExpand)
				{
					item.Expand = !item.Expand;
				}
				else
				{
					selectItem = item;
					item.Select = true;
					OnSelectChanged(item, e);
					((Control)this).Invalidate();
				}
				if (doubleClick)
				{
					OnNodeMouseDoubleClick(item, e);
				}
				OnNodeMouseClick(item, e);
			}
			return true;
		}
		if (canExpand && item.Expand)
		{
			foreach (TreeItem item2 in item.Sub)
			{
				if (IMouseUp(e, item2, MDown))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SetCheck(TreeItem item, bool value)
	{
		if (item.items == null || item.items.Count <= 0)
		{
			return;
		}
		foreach (TreeItem item2 in item.items)
		{
			item2.Checked = value;
			SetCheck(item2, value);
		}
	}

	private void SetCheckStrictly(TreeItem? item)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		if (item == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (TreeItem item2 in item.Sub)
		{
			if ((int)item2.CheckState == 1)
			{
				num2++;
				num++;
			}
			else if ((int)item2.CheckState == 2)
			{
				num++;
			}
		}
		if (num > 0)
		{
			item.CheckState = (CheckState)((num2 == item.Sub.Count) ? 1 : 2);
		}
		else
		{
			item.CheckState = (CheckState)0;
		}
		SetCheckStrictly(item.PARENTITEM);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (ScrollBar.MouseMoveY(e.Location) && ScrollBar.MouseMoveX(e.Location))
		{
			if (!OnTouchMove(e.X, e.Y) || items == null || items.Count == 0)
			{
				return;
			}
			int hand = 0;
			foreach (TreeItem item in items)
			{
				IMouseMove(item, e.X, e.Y, ref hand);
			}
			SetCursor(hand > 0);
		}
		else
		{
			ILeave();
		}
	}

	private void IMouseMove(TreeItem item, int x, int y, ref int hand)
	{
		if (!item.show)
		{
			return;
		}
		if (item.Contains(x, y, ScrollBar.ValueX, ScrollBar.ValueY, checkable, blockNode) > 0)
		{
			hand++;
		}
		if (item.items == null)
		{
			return;
		}
		foreach (TreeItem item2 in item.items)
		{
			IMouseMove(item2, x, y, ref hand);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar.Leave();
		ILeave();
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ScrollBar.Leave();
		ILeave();
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

	private void ILeave()
	{
		SetCursor(val: false);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int count = 0;
		foreach (TreeItem item in items)
		{
			ILeave(item, ref count);
		}
		if (count > 0)
		{
			((Control)this).Invalidate();
		}
	}

	private void ILeave(TreeItem item, ref int count)
	{
		selectItem = null;
		if (item.Hover)
		{
			count++;
		}
		item.Hover = false;
		if (item.items == null)
		{
			return;
		}
		foreach (TreeItem item2 in item.items)
		{
			ILeave(item2, ref count);
		}
	}

	private void IUSelect(TreeItem item)
	{
		item.Select = false;
		if (item.items == null)
		{
			return;
		}
		foreach (TreeItem item2 in item.items)
		{
			IUSelect(item2);
		}
	}

	public bool Select(TreeItem item)
	{
		return Select(items, item);
	}

	private bool Select(TreeItemCollection? items, TreeItem item)
	{
		if (items == null || items.Count == 0)
		{
			return false;
		}
		foreach (TreeItem item2 in items)
		{
			if (item2 == item)
			{
				selectItem = item;
				item2.Select = true;
				return true;
			}
			if (Select(item2.items, item))
			{
				return true;
			}
		}
		return false;
	}

	public void USelect()
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (TreeItem item in items)
		{
			IUSelect(item);
		}
	}

	[Obsolete("use USelect")]
	public void IUSelect()
	{
		USelect();
	}

	public void ExpandAll(bool value = true)
	{
		if (items != null && items.Count > 0)
		{
			ExpandAll(items, value);
		}
	}

	public void ExpandAll(TreeItemCollection items, bool value = true)
	{
		if (items == null || items.Count <= 0)
		{
			return;
		}
		foreach (TreeItem item in items)
		{
			item.Expand = value;
			ExpandAll(item.Sub, value);
		}
	}

	public List<TreeItem> GetCheckeds(bool Indeterminate = true)
	{
		if (items == null)
		{
			return new List<TreeItem>(0);
		}
		return GetCheckeds(items, Indeterminate);
	}

	private List<TreeItem> GetCheckeds(TreeItemCollection items, bool Indeterminate)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		List<TreeItem> list = new List<TreeItem>();
		if (Indeterminate)
		{
			foreach (TreeItem item in items)
			{
				if ((int)item.CheckState != 0)
				{
					list.Add(item);
				}
				if (item.items != null && item.items.Count > 0)
				{
					List<TreeItem> checkeds = GetCheckeds(item.items, Indeterminate);
					if (checkeds.Count > 0)
					{
						list.AddRange(checkeds);
					}
				}
			}
		}
		else
		{
			foreach (TreeItem item2 in items)
			{
				if (item2.Checked)
				{
					list.Add(item2);
				}
				if (item2.items != null && item2.items.Count > 0)
				{
					List<TreeItem> checkeds2 = GetCheckeds(item2.items, Indeterminate);
					if (checkeds2.Count > 0)
					{
						list.AddRange(checkeds2);
					}
				}
			}
		}
		return list;
	}

	public void SetCheckeds()
	{
		if (items != null)
		{
			List<TreeItem> checkeds = GetCheckeds();
			SetCheckeds(checkeds.Count == 0);
		}
	}

	public void SetCheckeds(bool check)
	{
		if (items != null)
		{
			SetCheckeds(items, check);
		}
	}

	public void SetCheckeds(TreeItemCollection items, bool check)
	{
		foreach (TreeItem item in items)
		{
			item.Checked = check;
			if (item.items != null && item.items.Count > 0)
			{
				SetCheckeds(item.items, check);
			}
		}
	}

	public void Focus(TreeItem item)
	{
		if (ScrollBar.ShowY)
		{
			ScrollBar.ValueY = item.rect.Y - (int)((float)_gap * Config.Dpi);
		}
		((Control)this).Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}
}
