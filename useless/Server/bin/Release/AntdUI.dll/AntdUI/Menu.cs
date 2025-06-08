using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Menu 导航菜单")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("SelectChanged")]
public class Menu : IControl, SubLayeredForm
{
	private Color? fore;

	private int radius = 6;

	private bool round;

	private TAMode theme;

	private float iconratio = 1.2f;

	private TMenuMode mode;

	private bool collapsed;

	private MenuItemCollection? items;

	private bool pauseLayout;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private int collapseWidth;

	private int collapsedWidth;

	private readonly StringFormat SL = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private MenuItem? MDown;

	private int hoveindexold = -1;

	private TooltipForm? tooltipForm;

	private ILayeredForm? subForm;

	internal int select_x;

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
				OnPropertyChanged("ForeColor");
			}
		}
	}

	[Description("激活字体颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive { get; set; }

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
				OnPropertyChanged("Round");
			}
		}
	}

	[Description("色彩模式")]
	[Category("外观")]
	[DefaultValue(TAMode.Auto)]
	public TAMode Theme
	{
		get
		{
			return theme;
		}
		set
		{
			if (theme != value)
			{
				theme = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Theme");
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? Gap { get; set; }

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(1.2f)]
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
				OnPropertyChanged("IconRatio");
			}
		}
	}

	[Description("菜单类型")]
	[Category("外观")]
	[DefaultValue(TMenuMode.Inline)]
	public TMenuMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				if (((Control)this).IsHandleCreated)
				{
					ChangeList();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Mode");
			}
		}
	}

	[Description("触发下拉的行为")]
	[Category("行为")]
	[DefaultValue(Trigger.Hover)]
	public Trigger Trigger { get; set; } = Trigger.Hover;


	[Description("常规缩进")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Indent { get; set; }

	[Description("只保持一个子菜单的展开")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Unique { get; set; }

	[Description("显示子菜单背景")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ShowSubBack { get; set; }

	[Description("自动折叠")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool AutoCollapse { get; set; }

	[Description("是否折叠")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Collapsed
	{
		get
		{
			return collapsed;
		}
		set
		{
			if (collapsed != value)
			{
				collapsed = value;
				if (((Control)this).IsHandleCreated)
				{
					ChangeList();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Collapsed");
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("菜单集合")]
	[Category("数据")]
	public MenuItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new MenuItemCollection(this);
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

	public int CollapseWidth => collapseWidth;

	public int CollapsedWidth => collapsedWidth;

	[Description("Select 属性值更改时发生")]
	[Category("行为")]
	public event SelectEventHandler? SelectChanged;

	public void SelectIndex(int i1, bool focus = true)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		if (items.ListExceed(i1))
		{
			((Control)this).Invalidate();
			return;
		}
		MenuItem menuItem = items[i1];
		menuItem.Select = true;
		OnSelectIndexChanged(menuItem);
		if (focus && ScrollBar.ShowY)
		{
			ScrollBar.ValueY = menuItem.rect.Y;
		}
		((Control)this).Invalidate();
	}

	public void SelectIndex(int i1, int i2, bool focus = true)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		if (items.ListExceed(i1))
		{
			((Control)this).Invalidate();
			return;
		}
		MenuItem menuItem = items[i1];
		if (menuItem.items.ListExceed(i2))
		{
			((Control)this).Invalidate();
			return;
		}
		MenuItem menuItem2 = menuItem.Sub[i2];
		bool select = (menuItem2.Select = true);
		menuItem.Select = select;
		OnSelectIndexChanged(menuItem2);
		if (focus && ScrollBar.ShowY)
		{
			ScrollBar.ValueY = menuItem2.rect.Y;
		}
		((Control)this).Invalidate();
	}

	public void SelectIndex(int i1, int i2, int i3, bool focus = true)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		if (items.ListExceed(i1))
		{
			((Control)this).Invalidate();
			return;
		}
		MenuItem menuItem = items[i1];
		if (menuItem.items.ListExceed(i2))
		{
			((Control)this).Invalidate();
			return;
		}
		MenuItem menuItem2 = menuItem.Sub[i2];
		if (menuItem2.items.ListExceed(i3))
		{
			((Control)this).Invalidate();
			return;
		}
		MenuItem menuItem3 = menuItem2.Sub[i3];
		bool flag2 = (menuItem3.Select = true);
		bool select = (menuItem2.Select = flag2);
		menuItem.Select = select;
		OnSelectIndexChanged(menuItem3);
		if (focus && ScrollBar.ShowY)
		{
			ScrollBar.ValueY = menuItem3.rect.Y;
		}
		((Control)this).Invalidate();
	}

	public int GetSelectIndex(MenuItem item)
	{
		if (items != null)
		{
			return items.IndexOf(item);
		}
		return -1;
	}

	public void Select(MenuItem item, bool focus = true)
	{
		if (items != null && items.Count != 0)
		{
			IUSelect(items);
			Select(item, focus, items);
		}
	}

	private void Select(MenuItem item, bool focus, MenuItemCollection items)
	{
		foreach (MenuItem item2 in items)
		{
			if (item2 == item)
			{
				item2.Select = true;
				OnSelectIndexChanged(item2);
				if (focus && ScrollBar.ShowY)
				{
					ScrollBar.ValueY = item2.rect.Y;
				}
				break;
			}
			if (item2.items != null && item2.items.Count > 0)
			{
				Select(item, focus, item2.items);
			}
		}
	}

	public void Remove(MenuItem item)
	{
		if (items != null && items.Count != 0)
		{
			Remove(item, items);
		}
	}

	private void Remove(MenuItem item, MenuItemCollection items)
	{
		foreach (MenuItem item2 in items)
		{
			if (item2 == item)
			{
				items.Remove(item2);
				break;
			}
			if (item2.items != null && item2.items.Count > 0)
			{
				Remove(item, item2.items);
			}
		}
	}

	internal void OnSelectIndexChanged(MenuItem item)
	{
		this.SelectChanged?.Invoke(this, new MenuSelectEventArgs(item));
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		if (GetSelectItem(out List<MenuItem> list) != null)
		{
			foreach (MenuItem item in list)
			{
				item.Select = true;
			}
		}
		((Control)this).OnHandleCreated(e);
	}

	public MenuItem? GetSelectItem()
	{
		List<MenuItem> list = new List<MenuItem>(0);
		return GetSelectItem(ref list, items);
	}

	public MenuItem? GetSelectItem(out List<MenuItem> list)
	{
		list = new List<MenuItem>(0);
		return GetSelectItem(ref list, items);
	}

	private MenuItem? GetSelectItem(ref List<MenuItem> list, MenuItemCollection? items)
	{
		if (items == null || items.Count == 0)
		{
			return null;
		}
		foreach (MenuItem item in items)
		{
			List<MenuItem> list2 = new List<MenuItem>(list.Count + 1);
			list2.AddRange(list);
			list2.Add(item);
			MenuItem selectItem = GetSelectItem(ref list2, item.Sub);
			if (selectItem == null)
			{
				if (item.Select)
				{
					list = list2;
					return item;
				}
				continue;
			}
			list = list2;
			return selectItem;
		}
		return null;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		Rectangle rect = ChangeList();
		ScrollBar.SizeChange(rect);
		((Control)this).OnFontChanged(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (((Control)this).IsHandleCreated)
		{
			Rectangle rect = ChangeList();
			ScrollBar.SizeChange(rect);
		}
		((Control)this).OnSizeChanged(e);
	}

	internal Rectangle ChangeList()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Rectangle _rect = ((Control)this).ClientRectangle;
		if (_rect.Width == 0 || _rect.Height == 0 || pauseLayout || items == null || items.Count == 0)
		{
			return _rect;
		}
		Rectangle rect = _rect.PaddingRect(((Control)this).Padding);
		int y = 0;
		int icon_count = 0;
		Helper.GDI(delegate(Canvas g)
		{
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			MenuItemCollection menuItemCollection = items;
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int num = (int)Math.Ceiling((float)size.Height * iconratio);
			int num2 = num / 2;
			int num3 = num2 / 2;
			int gapy = ((!Gap.HasValue) ? num3 : ((int)((float?)Gap * Config.Dpi).Value));
			int height = size.Height + num2 * 2;
			if (mode == TMenuMode.Horizontal)
			{
				ChangeListHorizontal(rect, g, menuItemCollection, 0, num, num2, num3);
			}
			else
			{
				Menu menu = this;
				int num4 = num * 2 + num2 + num3;
				Padding padding = ((Control)this).Padding;
				menu.collapseWidth = num4 + ((Padding)(ref padding)).Horizontal;
				Menu menu2 = this;
				int num5 = ChangeList(rect, g, null, menuItemCollection, ref y, ref icon_count, height, num, num2, gapy, 0);
				padding = ((Control)this).Padding;
				menu2.collapsedWidth = num5 + ((Padding)(ref padding)).Horizontal;
				if (AutoCollapse)
				{
					if (icon_count > 0)
					{
						collapsed = collapsedWidth >= _rect.Width;
					}
					else
					{
						collapsed = false;
					}
				}
				if (collapsed)
				{
					ChangeUTitle(menuItemCollection);
				}
			}
		});
		ScrollBar.SetVrSize(y);
		return _rect;
	}

	private int ChangeList(Rectangle rect, Canvas g, MenuItem? Parent, MenuItemCollection items, ref int y, ref int icon_count, int height, int icon_size, int gap, int gapy, int depth)
	{
		int num = 0;
		foreach (MenuItem item in items)
		{
			item.PARENT = this;
			item.PARENTITEM = Parent;
			if (item.HasIcon)
			{
				icon_count++;
			}
			item.SetRect(depth, Indent, new Rectangle(rect.X, rect.Y + y, rect.Width, height), icon_size, gap);
			if (!item.Visible)
			{
				continue;
			}
			int num2 = g.MeasureString(item.Text, item.Font ?? ((Control)this).Font).Width + gap * 4 + icon_size + item.arr_rect.Width;
			if (num2 > num)
			{
				num = num2;
			}
			y += height + gapy;
			if (mode != 0 || !item.CanExpand)
			{
				continue;
			}
			if (!collapsed)
			{
				int num3 = y;
				int num4 = ChangeList(rect, g, item, item.Sub, ref y, ref icon_count, height, icon_size, gap, gapy, depth + 1);
				if (num4 > num)
				{
					num = num4;
				}
				item.SubY = num3 - gapy / 2;
				item.SubHeight = y - num3;
				if ((item.Expand || item.ExpandThread) && item.ExpandProg > 0f)
				{
					item.ExpandHeight = y - num3;
					y = num3 + (int)Math.Ceiling((float)item.ExpandHeight * item.ExpandProg);
				}
				else if (!item.Expand)
				{
					y = num3;
				}
			}
			else
			{
				int num5 = y;
				int num6 = ChangeList(rect, g, item, item.Sub, ref y, ref icon_count, height, icon_size, gap, gapy, depth + 1);
				if (num6 > num)
				{
					num = num6;
				}
				y = num5;
			}
		}
		return num;
	}

	private void ChangeListHorizontal(Rectangle rect, Canvas g, MenuItemCollection items, int x, int icon_size, int gap, int gapI)
	{
		foreach (MenuItem item in items)
		{
			item.PARENT = this;
			int num = ((!item.HasIcon) ? (g.MeasureString(item.Text, item.Font ?? ((Control)this).Font).Width + gap * 2) : (g.MeasureString(item.Text, item.Font ?? ((Control)this).Font).Width + gap * 3 + icon_size));
			item.SetRectNoArr(0, new Rectangle(rect.X + x, rect.Y, num, rect.Height), icon_size, gap);
			if (item.Visible)
			{
				x += num;
			}
		}
	}

	private void ChangeUTitle(MenuItemCollection items)
	{
		foreach (MenuItem item in items)
		{
			Rectangle rect = item.Rect;
			item.ico_rect = new Rectangle(rect.X + (rect.Width - item.ico_rect.Width) / 2, item.ico_rect.Y, item.ico_rect.Width, item.ico_rect.Height);
			if (item.Visible && item.CanExpand)
			{
				ChangeUTitle(item.Sub);
			}
		}
	}

	public Menu()
	{
		ScrollBar = new ScrollBar(this);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected O, but got Unknown
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
		int value = ScrollBar.Value;
		canvas.TranslateTransform(0f, -value);
		Color baseColor = Colour.TextBase.Get("Menu", theme);
		Color fore_enabled = Colour.TextQuaternary.Get("Menu", theme);
		Color color;
		Color back_hover;
		Color fore_active;
		Color back_active;
		if (Config.IsDark || theme == TAMode.Dark)
		{
			color = fore ?? Colour.Text.Get("Menu", theme);
			back_hover = (fore_active = ForeActive ?? Colour.TextBase.Get("Menu", theme));
			back_active = BackActive ?? Colour.Primary.Get("Menu", theme);
		}
		else
		{
			color = fore ?? Colour.TextBase.Get("Menu", theme);
			fore_active = ForeActive ?? Colour.Primary.Get("Menu", theme);
			back_hover = BackHover ?? Colour.FillSecondary.Get("Menu", theme);
			back_active = BackActive ?? Colour.PrimaryBg.Get("Menu", theme);
		}
		float num = (float)radius * Config.Dpi;
		SolidBrush val = new SolidBrush(Colour.FillQuaternary.Get("Menu", theme));
		try
		{
			PaintItems(canvas, clientRectangle, value, items, color, fore_active, fore_enabled, back_hover, back_active, num, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas, baseColor);
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintItems(Canvas g, Rectangle rect, int sy, MenuItemCollection items, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius, SolidBrush sub_bg)
	{
		foreach (MenuItem item in items)
		{
			item.show = item.Show && item.Visible && (float)item.rect.Y > (float)(sy - rect.Height) - (item.Expand ? item.SubHeight : 0f) && item.rect.Bottom < sy + rect.Height + item.rect.Height;
			if (!item.show)
			{
				continue;
			}
			PaintIt(g, item, fore, fore_active, fore_enabled, back_hover, back_active, radius);
			if (!collapsed && (item.Expand || item.ExpandThread) && item.items != null && item.items.Count > 0)
			{
				if (ShowSubBack)
				{
					g.Fill((Brush)(object)sub_bg, new RectangleF(rect.X, item.SubY, rect.Width, item.SubHeight));
				}
				GraphicsState state = g.Save();
				if (item.ExpandThread)
				{
					g.SetClip(new RectangleF(rect.X, item.rect.Bottom, rect.Width, (float)item.ExpandHeight * item.ExpandProg));
				}
				PaintItemExpand(g, rect, sy, item.items, fore, fore_active, fore_enabled, back_hover, back_active, radius);
				g.Restore(state);
			}
		}
	}

	private void PaintItemExpand(Canvas g, Rectangle rect, float sy, MenuItemCollection items, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		foreach (MenuItem item in items)
		{
			item.show = item.Show && item.Visible && (float)item.rect.Y > sy - (float)rect.Height - (item.Expand ? item.SubHeight : 0f) && (float)item.rect.Bottom < sy + (float)rect.Height + (float)item.rect.Height;
			if (!item.show)
			{
				continue;
			}
			PaintIt(g, item, fore, fore_active, fore_enabled, back_hover, back_active, radius);
			if (!item.Expand || item.items == null || item.items.Count <= 0)
			{
				continue;
			}
			PaintItemExpand(g, rect, sy, item.items, fore, fore_active, fore_enabled, back_hover, back_active, radius);
			if (item.ExpandThread)
			{
				SolidBrush val = new SolidBrush(((Control)this).BackColor);
				try
				{
					g.Fill((Brush)(object)val, new RectangleF(rect.X, (float)item.rect.Bottom + (float)item.ExpandHeight * item.ExpandProg, rect.Width, item.ExpandHeight));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
	}

	private void PaintIt(Canvas g, MenuItem it, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius)
	{
		if (collapsed)
		{
			PaintItemMini(g, it, fore, fore_active, fore_enabled, back_hover, back_active, radius);
		}
		else
		{
			PaintItem(g, it, fore, fore_active, fore_enabled, back_hover, back_active, radius);
		}
	}

	private void PaintItemMini(Canvas g, MenuItem it, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius)
	{
		if (it.Enabled)
		{
			if (Config.IsDark || theme == TAMode.Dark)
			{
				if (it.Select)
				{
					PaintBack(g, back_active, it.rect, radius);
					PaintIcon(g, it, fore_active);
				}
				else if (it.AnimationHover)
				{
					PaintIcon(g, it, fore);
					PaintIcon(g, it, Helper.ToColorN(it.AnimationHoverValue, back_hover));
				}
				else if (it.Hover)
				{
					PaintIcon(g, it, back_hover);
				}
				else
				{
					PaintIcon(g, it, fore);
				}
			}
			else if (it.Select)
			{
				PaintBack(g, back_active, it.rect, radius);
				PaintIcon(g, it, fore_active);
			}
			else
			{
				if (it.AnimationHover)
				{
					PaintBack(g, Helper.ToColorN(it.AnimationHoverValue, back_hover), it.rect, radius);
				}
				else if (it.Hover)
				{
					PaintBack(g, back_hover, it.rect, radius);
				}
				PaintIcon(g, it, fore);
			}
		}
		else
		{
			if (it.Select)
			{
				PaintBack(g, back_active, it.rect, radius);
			}
			PaintIcon(g, it, fore_enabled);
		}
	}

	private void PaintItem(Canvas g, MenuItem it, Color fore, Color fore_active, Color fore_enabled, Color back_hover, Color back_active, float radius)
	{
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		if (it.Enabled)
		{
			if (Config.IsDark || theme == TAMode.Dark)
			{
				if (it.Select)
				{
					if (it.CanExpand)
					{
						if (mode == TMenuMode.Horizontal || mode == TMenuMode.Vertical)
						{
							PaintBack(g, back_active, it.rect, radius);
						}
						PaintTextIconExpand(g, it, fore_active);
					}
					else
					{
						PaintBack(g, back_active, it.rect, radius);
						PaintTextIcon(g, it, fore_active);
					}
				}
				else if (it.AnimationHover)
				{
					PaintTextIconExpand(g, it, fore);
					PaintTextIconExpand(g, it, Helper.ToColorN(it.AnimationHoverValue, back_hover));
				}
				else if (it.Hover)
				{
					PaintTextIconExpand(g, it, back_hover);
				}
				else
				{
					PaintTextIconExpand(g, it, fore);
				}
			}
			else if (it.Select)
			{
				if (it.CanExpand)
				{
					if (mode == TMenuMode.Horizontal || mode == TMenuMode.Vertical)
					{
						PaintBack(g, back_active, it.rect, radius);
					}
					PaintTextIconExpand(g, it, fore_active);
				}
				else
				{
					PaintBack(g, back_active, it.rect, radius);
					PaintTextIcon(g, it, fore_active);
				}
			}
			else
			{
				if (it.AnimationHover)
				{
					PaintBack(g, Helper.ToColorN(it.AnimationHoverValue, back_hover), it.rect, radius);
				}
				else if (it.Hover)
				{
					PaintBack(g, back_hover, it.rect, radius);
				}
				PaintTextIconExpand(g, it, fore);
			}
			return;
		}
		if (it.Select)
		{
			if (it.CanExpand)
			{
				if (mode == TMenuMode.Horizontal || mode == TMenuMode.Vertical)
				{
					PaintBack(g, back_active, it.rect, radius);
				}
				Pen val = new Pen(fore_active, 2f);
				try
				{
					LineCap startCap = (LineCap)2;
					val.EndCap = (LineCap)2;
					val.StartCap = startCap;
					g.DrawLines(val, it.arr_rect.TriangleLines(it.ArrowProg, 0.4f));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				PaintBack(g, back_active, it.rect, radius);
			}
		}
		else if (it.CanExpand)
		{
			Pen val2 = new Pen(fore_enabled, 2f);
			try
			{
				LineCap startCap = (LineCap)2;
				val2.EndCap = (LineCap)2;
				val2.StartCap = startCap;
				g.DrawLines(val2, it.arr_rect.TriangleLines(it.ArrowProg, 0.4f));
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		PaintTextIcon(g, it, fore_enabled);
	}

	private void PaintTextIcon(Canvas g, MenuItem it, Color fore)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(fore);
		try
		{
			g.String(it.Text, it.Font ?? ((Control)this).Font, (Brush)(object)val, it.txt_rect, SL);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		PaintIcon(g, it, fore);
	}

	private void PaintTextIconExpand(Canvas g, MenuItem it, Color fore)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (it.CanExpand)
		{
			if (mode == TMenuMode.Inline)
			{
				Pen val = new Pen(fore, 2f);
				try
				{
					LineCap startCap = (LineCap)2;
					val.EndCap = (LineCap)2;
					val.StartCap = startCap;
					g.DrawLines(val, it.arr_rect.TriangleLines(it.ArrowProg, 0.4f));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else if (mode == TMenuMode.Vertical)
			{
				Pen val2 = new Pen(fore, 2f);
				try
				{
					LineCap startCap = (LineCap)2;
					val2.EndCap = (LineCap)2;
					val2.StartCap = startCap;
					g.DrawLines(val2, TAlignMini.Right.TriangleLines(it.arr_rect, 0.4f));
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}
		SolidBrush val3 = new SolidBrush(fore);
		try
		{
			g.String(it.Text, it.Font ?? ((Control)this).Font, (Brush)(object)val3, it.txt_rect, SL);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		PaintIcon(g, it, fore);
	}

	private void PaintIcon(Canvas g, MenuItem it, Color fore)
	{
		if (it.Select)
		{
			int num = 0;
			if (it.IconActive != null)
			{
				g.Image(it.IconActive, it.ico_rect);
				num++;
			}
			if (it.IconActiveSvg != null && g.GetImgExtend(it.IconActiveSvg, it.ico_rect, fore))
			{
				num++;
			}
			if (num > 0)
			{
				return;
			}
		}
		if (it.Icon != null)
		{
			g.Image(it.Icon, it.ico_rect);
		}
		if (it.IconSvg != null)
		{
			g.GetImgExtend(it.IconSvg, it.ico_rect, fore);
		}
	}

	private void PaintBack(Canvas g, Color color, Rectangle rect, float radius)
	{
		if (Round || radius > 0f)
		{
			GraphicsPath val = rect.RoundPath(radius, Round);
			try
			{
				g.Fill(color, val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Fill(color, rect);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
		if (!ScrollBar.MouseDown(e.Location) || items == null || items.Count == 0)
		{
			return;
		}
		OnTouchDown(e.X, e.Y);
		foreach (MenuItem item in items)
		{
			if (IMouseDown(items, item, e.Location))
			{
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (!ScrollBar.MouseUp() || !OnTouchUp() || items == null || items.Count == 0 || MDown == null)
		{
			return;
		}
		foreach (MenuItem item in items)
		{
			List<MenuItem> list = new List<MenuItem> { item };
			if (IMouseUp(items, item, list, e.Location, MDown))
			{
				break;
			}
		}
	}

	private bool IMouseDown(MenuItemCollection items, MenuItem item, Point point)
	{
		if (item.Visible)
		{
			bool canExpand = item.CanExpand;
			if (item.Enabled && item.Contains(point, 0, ScrollBar.Value, out var _))
			{
				MDown = item;
				return true;
			}
			if (canExpand && item.Expand && !collapsed)
			{
				foreach (MenuItem item2 in item.Sub)
				{
					if (IMouseDown(items, item2, point))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IMouseUp(MenuItemCollection items, MenuItem item, List<MenuItem> list, Point point, MenuItem MDown)
	{
		if (item.Visible)
		{
			bool canExpand = item.CanExpand;
			if (MDown == item)
			{
				if (item.Enabled && item.Contains(point, 0, ScrollBar.Value, out var _))
				{
					if (canExpand)
					{
						if ((mode == TMenuMode.Horizontal || mode == TMenuMode.Vertical) && Trigger == Trigger.Click && item.items != null && item.items.Count > 0)
						{
							if (subForm == null)
							{
								Rectangle rectangle = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
								Rectangle rect = item.Rect;
								Rectangle rect_read = new Rectangle(rectangle.X + rect.X, rectangle.Y + rect.Y, rect.Width, rect.Height);
								select_x = 0;
								subForm = new LayeredFormMenuDown(this, radius, rect_read, item.items);
								((Form)subForm).Show((IWin32Window)(object)this);
							}
							else
							{
								subForm.IClose();
								subForm = null;
							}
						}
						else
						{
							item.Expand = !item.Expand;
						}
					}
					else
					{
						IUSelect(items);
						if (list.Count > 1)
						{
							foreach (MenuItem item2 in list)
							{
								item2.Select = true;
							}
						}
						item.Select = true;
						OnSelectIndexChanged(item);
						((Control)this).Invalidate();
					}
				}
				return true;
			}
			if (canExpand && item.Expand && !collapsed)
			{
				foreach (MenuItem item3 in item.Sub)
				{
					List<MenuItem> list2 = new List<MenuItem>(list.Count + 1);
					list2.AddRange(list);
					list2.Add(item3);
					if (IMouseUp(items, item3, list2, point, MDown))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (ScrollBar.MouseMove(e.Location))
		{
			if (!OnTouchMove(e.X, e.Y) || items == null || items.Count == 0)
			{
				return;
			}
			int count = 0;
			int hand = 0;
			if (collapsed)
			{
				int num = 0;
				int num2 = -1;
				foreach (MenuItem item in items)
				{
					if (item.show)
					{
						if (item.Contains(e.Location, 0, ScrollBar.Value, out var change))
						{
							num2 = num;
							hand++;
						}
						if (change)
						{
							count++;
						}
					}
					num++;
				}
				if (num2 != hoveindexold)
				{
					hoveindexold = num2;
					ILayeredForm? layeredForm = subForm;
					if (layeredForm != null)
					{
						((Form)layeredForm).Close();
					}
					subForm = null;
					TooltipForm? obj = tooltipForm;
					if (obj != null)
					{
						((Form)obj).Close();
					}
					tooltipForm = null;
					if (num2 > -1)
					{
						Rectangle rectangle = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
						MenuItem menuItem = items[num2];
						if (menuItem == null)
						{
							return;
						}
						Rectangle rect = menuItem.Rect;
						Rectangle rectangle2 = new Rectangle(rectangle.X + rect.X, rectangle.Y + rect.Y, rect.Width, rect.Height);
						if (menuItem.items != null && menuItem.items.Count > 0)
						{
							select_x = 0;
							subForm = new LayeredFormMenuDown(this, radius, rectangle2, menuItem.items);
							((Form)subForm).Show((IWin32Window)(object)this);
						}
						else if (menuItem.Text != null)
						{
							if (tooltipForm == null)
							{
								tooltipForm = new TooltipForm((Control)(object)this, rectangle2, menuItem.Text, new TooltipConfig
								{
									Font = (menuItem.Font ?? ((Control)this).Font),
									ArrowAlign = TAlign.Right
								});
								((Form)tooltipForm).Show((IWin32Window)(object)this);
							}
							else
							{
								tooltipForm.SetText(rectangle2, menuItem.Text);
							}
						}
					}
				}
			}
			else if (mode == TMenuMode.Inline)
			{
				foreach (MenuItem item2 in items)
				{
					IMouseMove(item2, e.Location, ref count, ref hand);
				}
			}
			else
			{
				int num3 = 0;
				int num4 = -1;
				foreach (MenuItem item3 in items)
				{
					if (item3.show)
					{
						if (item3.Contains(e.Location, 0, ScrollBar.Value, out var change2))
						{
							num4 = num3;
							hand++;
						}
						if (change2)
						{
							count++;
						}
					}
					num3++;
				}
				if (num4 != hoveindexold)
				{
					hoveindexold = num4;
					ILayeredForm? layeredForm2 = subForm;
					if (layeredForm2 != null)
					{
						((Form)layeredForm2).Close();
					}
					subForm = null;
					TooltipForm? obj2 = tooltipForm;
					if (obj2 != null)
					{
						((Form)obj2).Close();
					}
					tooltipForm = null;
					if (num4 > -1)
					{
						Rectangle rectangle3 = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
						MenuItem menuItem2 = items[num4];
						if (menuItem2 == null)
						{
							return;
						}
						Rectangle rect2 = menuItem2.Rect;
						Rectangle rect_read = new Rectangle(rectangle3.X + rect2.X, rectangle3.Y + rect2.Y, rect2.Width, rect2.Height);
						if (Trigger == Trigger.Hover && menuItem2.items != null && menuItem2.items.Count > 0)
						{
							select_x = 0;
							subForm = new LayeredFormMenuDown(this, radius, rect_read, menuItem2.items);
							((Form)subForm).Show((IWin32Window)(object)this);
						}
					}
				}
			}
			SetCursor(hand > 0);
			if (count > 0)
			{
				((Control)this).Invalidate();
			}
		}
		else
		{
			ILeave();
		}
	}

	private void IMouseMove(MenuItem it, Point point, ref int count, ref int hand)
	{
		if (!it.show)
		{
			return;
		}
		if (it.Contains(point, 0, ScrollBar.Value, out var change))
		{
			hand++;
		}
		if (change)
		{
			count++;
		}
		if (it.items == null || it.items.Count <= 0)
		{
			return;
		}
		foreach (MenuItem item in it.items)
		{
			IMouseMove(item, point, ref count, ref hand);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		hoveindexold = -1;
		TooltipForm? obj = tooltipForm;
		if (obj != null)
		{
			((Form)obj).Close();
		}
		tooltipForm = null;
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
		foreach (MenuItem item in items)
		{
			ILeave(item, ref count);
		}
		if (count > 0)
		{
			((Control)this).Invalidate();
		}
	}

	private void ILeave(MenuItem it, ref int count)
	{
		if (it.Hover)
		{
			count++;
		}
		it.Hover = false;
		if (it.items == null || it.items.Count <= 0)
		{
			return;
		}
		foreach (MenuItem item in it.items)
		{
			ILeave(item, ref count);
		}
	}

	public void USelect()
	{
		if (items != null && items.Count != 0)
		{
			IUSelect(items);
		}
	}

	[Obsolete("use USelect")]
	public void IUSelect()
	{
		if (items != null)
		{
			IUSelect(items);
		}
	}

	private void IUSelect(MenuItemCollection items)
	{
		foreach (MenuItem item in items)
		{
			IUSelect(item);
		}
	}

	private void IUSelect(MenuItem it)
	{
		it.Select = false;
		if (it.items == null || it.items.Count <= 0)
		{
			return;
		}
		foreach (MenuItem item in it.items)
		{
			IUSelect(item);
		}
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	internal void DropDownChange(MenuItem value)
	{
		select_x = 0;
		subForm = null;
		if (items == null || items.Count == 0)
		{
			return;
		}
		IUSelect(items);
		foreach (MenuItem item in items)
		{
			List<MenuItem> list = new List<MenuItem> { item };
			if (IDropDownChange(items, item, list, value))
			{
				return;
			}
		}
		((Control)this).Invalidate();
	}

	private bool IDropDownChange(MenuItemCollection items, MenuItem item, List<MenuItem> list, MenuItem value)
	{
		bool canExpand = item.CanExpand;
		if (item.Enabled && item == value)
		{
			if (canExpand)
			{
				item.Expand = !item.Expand;
			}
			else
			{
				IUSelect(items);
				if (list.Count > 1)
				{
					foreach (MenuItem item2 in list)
					{
						item2.Select = true;
					}
				}
				item.Select = true;
				OnSelectIndexChanged(item);
				((Control)this).Invalidate();
			}
			return true;
		}
		if (canExpand)
		{
			foreach (MenuItem item3 in item.Sub)
			{
				List<MenuItem> list2 = new List<MenuItem>(list.Count + 1);
				list2.AddRange(list);
				list2.Add(item3);
				if (IDropDownChange(items, item3, list2, value))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}
}
