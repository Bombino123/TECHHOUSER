using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("VirtualPanel 虚拟容器")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ItemClick")]
public class VirtualPanel : IControl, IEventListener
{
	private class RItem
	{
		public bool wrap { get; set; }

		public int use_x { get; set; }

		public int use_y { get; set; }

		public int h { get; set; }

		public List<VirtualItem> cel { get; set; }

		public RItem(int usex, int usey, int height, List<VirtualItem> cell, bool _wrap = false)
		{
			use_x = usex;
			use_y = usey;
			h = height;
			cel = new List<VirtualItem>(cell);
			wrap = _wrap;
		}
	}

	private VirtualCollection? items;

	private int radius = 6;

	private int shadow;

	private Color? shadowColor;

	private int shadowOffsetX;

	private int shadowOffsetY;

	private float shadowOpacity = 0.1f;

	private float shadowOpacityHover = 0.3f;

	private TAlignMini shadowAlign;

	private int gap;

	private bool isEmpty;

	private string? emptyText;

	private bool wrap = true;

	private bool waterfall;

	private TAlignItems alignitems = TAlignItems.Start;

	private TJustifyContent justifycontent = TJustifyContent.Start;

	private TAlignContent aligncontent = TAlignContent.Start;

	private bool pauseLayout;

	[Browsable(false)]
	public ScrollBar ScrollBar;

	internal int CellCount = -1;

	private Action<VirtualItem> invalidate;

	private StringFormat stringCenter = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	public Control? BlurBar;

	private ManualResetEvent _event = new ManualResetEvent(initialState: false);

	private Dictionary<string, Bitmap> shadow_dir_tmp = new Dictionary<string, Bitmap>();

	private VirtualItem? MDown;

	private int isdouclick;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	public VirtualCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new VirtualCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
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
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("阴影")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (shadow != value)
			{
				shadow = value;
				DisposeShadow();
				LoadLayout();
				OnPropertyChanged("Shadow");
			}
		}
	}

	[Description("阴影颜色")]
	[Category("阴影")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (!(shadowColor == value))
			{
				shadowColor = value;
				DisposeShadow();
				LoadLayout();
				OnPropertyChanged("ShadowColor");
			}
		}
	}

	[Description("阴影偏移X")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetX
	{
		get
		{
			return shadowOffsetX;
		}
		set
		{
			if (shadowOffsetX != value)
			{
				shadowOffsetX = value;
				DisposeShadow();
				LoadLayout();
				OnPropertyChanged("ShadowOffsetX");
			}
		}
	}

	[Description("阴影偏移Y")]
	[Category("阴影")]
	[DefaultValue(0)]
	public int ShadowOffsetY
	{
		get
		{
			return shadowOffsetY;
		}
		set
		{
			if (shadowOffsetY != value)
			{
				shadowOffsetY = value;
				DisposeShadow();
				LoadLayout();
				OnPropertyChanged("ShadowOffsetY");
			}
		}
	}

	[Description("阴影透明度")]
	[Category("阴影")]
	[DefaultValue(0.1f)]
	public float ShadowOpacity
	{
		get
		{
			return shadowOpacity;
		}
		set
		{
			if (shadowOpacity != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacity = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOpacity");
			}
		}
	}

	[Description("阴影透明度动画使能")]
	[Category("阴影")]
	[DefaultValue(false)]
	public bool ShadowOpacityAnimation { get; set; }

	[Description("悬停阴影后透明度")]
	[Category("阴影")]
	[DefaultValue(0.3f)]
	public float ShadowOpacityHover
	{
		get
		{
			return shadowOpacityHover;
		}
		set
		{
			if (shadowOpacityHover != value)
			{
				if (value < 0f)
				{
					value = 0f;
				}
				else if (value > 1f)
				{
					value = 1f;
				}
				shadowOpacityHover = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ShadowOpacityHover");
			}
		}
	}

	[Description("阴影方向")]
	[Category("阴影")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini ShadowAlign
	{
		get
		{
			return shadowAlign;
		}
		set
		{
			if (shadowAlign != value)
			{
				shadowAlign = value;
				DisposeShadow();
				LoadLayout();
				OnPropertyChanged("ShadowAlign");
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
			return gap;
		}
		set
		{
			if (gap != value)
			{
				gap = value;
				LoadLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("是否显示空样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Empty { get; set; }

	[Description("数据为空显示文字")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? EmptyText
	{
		get
		{
			return emptyText;
		}
		set
		{
			if (!(emptyText == value))
			{
				emptyText = value;
				((Control)this).Invalidate();
				OnPropertyChanged("EmptyText");
			}
		}
	}

	[Description("数据为空显示图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? EmptyImage { get; set; }

	[Description("换行")]
	[Category("布局")]
	[DefaultValue(true)]
	public bool Wrap
	{
		get
		{
			return wrap;
		}
		set
		{
			if (wrap != value)
			{
				wrap = value;
				LoadLayout();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("瀑布流")]
	[Category("布局")]
	[DefaultValue(false)]
	public bool Waterfall
	{
		get
		{
			return waterfall;
		}
		set
		{
			if (waterfall != value)
			{
				waterfall = value;
				LoadLayout();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("侧轴(纵轴)对齐方式")]
	[Category("布局")]
	[DefaultValue(TAlignItems.Start)]
	public TAlignItems AlignItems
	{
		get
		{
			return alignitems;
		}
		set
		{
			if (alignitems != value)
			{
				alignitems = value;
				LoadLayout();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("主轴(横轴)对齐方式")]
	[Category("布局")]
	[DefaultValue(TJustifyContent.Start)]
	public TJustifyContent JustifyContent
	{
		get
		{
			return justifycontent;
		}
		set
		{
			if (justifycontent != value)
			{
				justifycontent = value;
				LoadLayout();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("没有占用交叉轴上所有可用的空间时对齐容器内的各项(垂直)")]
	[Category("布局")]
	[DefaultValue(TAlignContent.Start)]
	public TAlignContent AlignContent
	{
		get
		{
			return aligncontent;
		}
		set
		{
			if (aligncontent != value)
			{
				aligncontent = value;
				LoadLayout();
				((Control)this).Invalidate();
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
					LoadLayout();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	[Description("点击项时发生")]
	[Category("行为")]
	public event VirtualItemEventHandler? ItemClick;

	public VirtualPanel()
	{
		ScrollBar = new ScrollBar(this);
		invalidate = delegate(VirtualItem it)
		{
			((Control)this).Invalidate(new Rectangle(it.RECT.X - ScrollBar.ValueX, it.RECT.Y - ScrollBar.ValueY, it.RECT.Width, it.RECT.Height));
		};
		Thread thread = new Thread(LongTask);
		thread.IsBackground = true;
		thread.Start();
	}

	private void DisposeShadow()
	{
		if (items == null || items.Count == 0 || shadow_dir_tmp.Count == 0)
		{
			return;
		}
		lock (shadow_dir_tmp)
		{
			foreach (KeyValuePair<string, Bitmap> item in shadow_dir_tmp)
			{
				((Image)item.Value).Dispose();
			}
			shadow_dir_tmp.Clear();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		CellCount = -1;
		LoadLayout();
		((Control)this).OnSizeChanged(e);
	}

	public void LoadLayout()
	{
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		if (items == null || items.Count == 0)
		{
			ScrollBar.Value = 0;
		}
		else
		{
			if (pauseLayout)
			{
				return;
			}
			List<VirtualItem> list = new List<VirtualItem>(items.Count);
			foreach (VirtualItem item in items)
			{
				item.SHOW = false;
				if (item.Visible)
				{
					list.Add(item);
				}
			}
			if (list.Count > 0)
			{
				isEmpty = false;
				int vrSize = HandLayout(list);
				ScrollBar.SetVrSize(vrSize);
			}
			else
			{
				isEmpty = true;
			}
		}
	}

	private int HandLayout(List<VirtualItem> items)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		List<VirtualItem> items2 = items;
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return 0;
		}
		ScrollBar.SizeChange(clientRectangle);
		Rectangle rect = clientRectangle.PaddingRect(((Control)this).Padding);
		return Helper.GDI(delegate(Canvas g)
		{
			int num = (int)Math.Round((float)Gap * Config.Dpi);
			int num2 = rect.X;
			int num3 = rect.Y + num;
			int num4 = 0;
			int num5 = 0;
			int num6 = (int)((float)Shadow * Config.Dpi);
			int num7 = num6 * 2;
			int num8 = (int)((float)radius * Config.Dpi);
			List<RItem> list = new List<RItem>();
			List<VirtualItem> list2 = new List<VirtualItem>(items2.Count);
			foreach (VirtualItem item in items2)
			{
				Size size = item.Size(g, new VirtualPanelArgs(this, rect, num8));
				item.WIDTH = size.Width;
				item.HEIGHT = size.Height;
				item.invalidate = invalidate;
			}
			if (waterfall && CellCount == -1)
			{
				List<int> list3 = new List<int>(items2.Count);
				int num9 = 0;
				foreach (VirtualItem item2 in items2)
				{
					if (num2 + item2.WIDTH >= rect.Width)
					{
						num2 = rect.X;
						num3 += num5 + num;
						if (num9 > 0)
						{
							list3.Add(num9);
						}
						num9 = 0;
					}
					num2 += item2.WIDTH + num;
					num9++;
				}
				num2 = rect.X;
				num3 = rect.Y + num;
				if (list3.Count > 0)
				{
					CellCount = list3.Max();
				}
			}
			foreach (VirtualItem item3 in items2)
			{
				if (list2.Count > 0 && num2 + item3.WIDTH >= rect.Width)
				{
					list.Add(new RItem(num2, num3, num5, list2, _wrap: true));
					list2.Clear();
					num2 = rect.X;
					num3 += num5 + num;
					num5 = 0;
				}
				if (num5 < item3.HEIGHT)
				{
					num5 = item3.HEIGHT;
				}
				if (item3 is VirtualShadowItem virtualShadowItem)
				{
					item3.SetRECT(num2 + num6, num3 + num6, item3.WIDTH - num7, item3.HEIGHT - num7);
					virtualShadowItem.SetRECTS(num2, num3, item3.WIDTH, item3.HEIGHT);
				}
				else
				{
					item3.SetRECT(num2, num3, item3.WIDTH, item3.HEIGHT);
				}
				num2 += item3.WIDTH + num;
				num4 = num3 + item3.HEIGHT + num;
				item3.SHOW = true;
				list2.Add(item3);
			}
			if (list2.Count > 0)
			{
				list.Add(new RItem(num2, num3, num5, list2));
				num4 = num3 + num5 + num;
			}
			list2.Clear();
			if (num4 > rect.Height)
			{
				rect.Height = num4;
			}
			switch (justifycontent)
			{
			case TJustifyContent.End:
				foreach (RItem item4 in list)
				{
					int x4 = rect.Width - item4.use_x + num;
					HandLayout(rect, item4.cel, x4, 0);
				}
				break;
			case TJustifyContent.SpaceBetween:
				foreach (RItem item5 in list)
				{
					if (item5.cel.Count > 1)
					{
						int num22 = item5.cel.Count;
						int num23 = 0;
						if (CellCount == -1 || CellCount <= item5.cel.Count)
						{
							num23 = item5.cel.Sum((VirtualItem a) => a.RECT.Width);
						}
						else
						{
							num22 = CellCount;
							num23 = item5.cel.Sum((VirtualItem a) => a.RECT.Width) + (CellCount - item5.cel.Count) * item5.cel[0].RECT.Width;
						}
						int num24 = (rect.Width - num23) / (num22 - 1);
						int num25 = rect.X;
						foreach (VirtualItem item6 in item5.cel)
						{
							HandLayout(rect, item6, num25 - item6.RECT.X, 0);
							num25 += item6.RECT.Width + num24;
						}
					}
				}
				break;
			case TJustifyContent.SpaceEvenly:
				if (waterfall)
				{
					foreach (RItem item7 in list)
					{
						int num15 = item7.cel.Count;
						int num16 = 0;
						if (CellCount == -1 || CellCount <= item7.cel.Count)
						{
							num16 = item7.cel.Sum((VirtualItem a) => a.RECT.Width);
						}
						else
						{
							num15 = CellCount;
							num16 = item7.cel.Sum((VirtualItem a) => a.RECT.Width) + (CellCount - item7.cel.Count) * item7.cel[0].RECT.Width;
						}
						int num17 = (rect.Width - num16) / (num15 + 1);
						int num18 = rect.X + num17;
						foreach (VirtualItem item8 in item7.cel)
						{
							HandLayout(rect, item8, num18 - item8.RECT.X, 0);
							num18 += item8.RECT.Width + num17;
						}
					}
				}
				else
				{
					foreach (RItem item9 in list)
					{
						if (item9.cel.Count > 1)
						{
							int count = item9.cel.Count;
							int num19 = (num19 = item9.cel.Sum((VirtualItem a) => a.RECT.Width));
							int num20 = (rect.Width - num19) / (count + 1);
							int num21 = rect.X + num20;
							foreach (VirtualItem item10 in item9.cel)
							{
								HandLayout(rect, item10, num21 - item10.RECT.X, 0);
								num21 += item10.RECT.Width + num20;
							}
						}
						else
						{
							int x3 = (rect.Width - item9.use_x + num) / 2;
							HandLayout(rect, item9.cel, x3, 0);
						}
					}
				}
				break;
			case TJustifyContent.SpaceAround:
				foreach (RItem item11 in list)
				{
					if (item11.cel.Count > 1)
					{
						int num10 = item11.cel.Sum((VirtualItem a) => a.RECT.Width);
						int num11 = rect.Width - num10;
						int num12 = num11 / item11.cel.Count;
						int num13 = num11 / (item11.cel.Count + 1);
						int num14 = rect.X + num13 / 2;
						foreach (VirtualItem item12 in item11.cel)
						{
							HandLayout(rect, item12, num14 - item12.RECT.X, 0);
							num14 += item12.RECT.Width + num12;
						}
					}
					else
					{
						int x2 = (rect.Width - item11.use_x + num) / 2;
						HandLayout(rect, item11.cel, x2, 0);
					}
				}
				break;
			default:
				foreach (RItem item13 in list)
				{
					int x = (rect.Width - item13.use_x + num) / 2;
					HandLayout(rect, item13.cel, x, 0);
				}
				break;
			case TJustifyContent.Start:
				break;
			}
			switch (aligncontent)
			{
			case TAlignContent.End:
			{
				int y3 = rect.Height - GetTotalHeight(list);
				foreach (RItem item14 in list)
				{
					HandLayout(rect, item14.cel, 0, y3);
				}
				break;
			}
			case TAlignContent.SpaceBetween:
				if (list.Count > 1)
				{
					int totalHeight2 = GetTotalHeight(list);
					int num30 = (rect.Height - totalHeight2) / (list.Count - 1);
					int num31 = rect.Y;
					foreach (RItem item15 in list)
					{
						foreach (VirtualItem item16 in item15.cel)
						{
							HandLayout(rect, item16, 0, num31 - item16.RECT.Y);
						}
						num31 += item15.h + num30;
					}
				}
				break;
			case TAlignContent.SpaceEvenly:
				if (list.Count > 1)
				{
					int totalHeight3 = GetTotalHeight(list);
					int num32 = (rect.Height - totalHeight3) / (list.Count + 1);
					int num33 = rect.Y + num32;
					foreach (RItem item17 in list)
					{
						foreach (VirtualItem item18 in item17.cel)
						{
							HandLayout(rect, item18, 0, num33 - item18.RECT.Y);
						}
						num33 += item17.h + num32;
					}
				}
				else
				{
					int y4 = (rect.Height - GetTotalHeight(list)) / 2;
					foreach (RItem item19 in list)
					{
						HandLayout(rect, item19.cel, 0, y4);
					}
				}
				break;
			case TAlignContent.SpaceAround:
				if (list.Count > 1)
				{
					int totalHeight = GetTotalHeight(list);
					int num26 = rect.Height - totalHeight;
					int num27 = num26 / list.Count;
					int num28 = num26 / (list.Count + 1);
					int num29 = rect.Y + num28 / 2;
					foreach (RItem item20 in list)
					{
						foreach (VirtualItem item21 in item20.cel)
						{
							HandLayout(rect, item21, 0, num29 - item21.RECT.Y);
						}
						num29 += item20.h + num27;
					}
				}
				else
				{
					int y2 = (rect.Height - GetTotalHeight(list)) / 2;
					foreach (RItem item22 in list)
					{
						HandLayout(rect, item22.cel, 0, y2);
					}
				}
				break;
			default:
			{
				int y = (rect.Height - GetTotalHeight(list)) / 2;
				foreach (RItem item23 in list)
				{
					HandLayout(rect, item23.cel, 0, y);
				}
				break;
			}
			case TAlignContent.Start:
				break;
			}
			if (waterfall)
			{
				num4 = WaterfallLayout(rect, list);
			}
			else
			{
				switch (alignitems)
				{
				case TAlignItems.End:
					foreach (RItem item24 in list)
					{
						foreach (VirtualItem item25 in item24.cel)
						{
							int y6 = item24.h - item25.RECT.Height;
							HandLayout(rect, item25, 0, y6);
						}
					}
					break;
				default:
					foreach (RItem item26 in list)
					{
						foreach (VirtualItem item27 in item26.cel)
						{
							int y5 = (item26.h - item27.RECT.Height) / 2;
							HandLayout(rect, item27, 0, y5);
						}
					}
					break;
				case TAlignItems.Start:
					break;
				}
			}
			return num4 + num * 2;
		});
	}

	private int WaterfallLayout(Rectangle rect, List<RItem> rows)
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		switch (justifycontent)
		{
		case TJustifyContent.Start:
			WaterfallLayoutStart(rect, rows);
			break;
		case TJustifyContent.End:
			WaterfallLayoutEnd(rect, rows);
			break;
		default:
			WaterfallLayoutCenter(rect, rows);
			break;
		}
		int num = 0;
		foreach (RItem row in rows)
		{
			foreach (VirtualItem item in row.cel)
			{
				if (item is VirtualShadowItem virtualShadowItem)
				{
					if (num < virtualShadowItem.RECT_S.Bottom)
					{
						num = virtualShadowItem.RECT_S.Bottom;
					}
				}
				else if (num < item.RECT.Bottom)
				{
					num = item.RECT.Bottom;
				}
			}
		}
		int num2 = num;
		Padding padding = ((Control)this).Padding;
		return num2 + ((Padding)(ref padding)).Bottom + gap;
	}

	private void WaterfallLayoutStart(Rectangle rect, List<RItem> rows)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(rows[0].cel.Count);
		for (int i = 1; i < rows.Count; i++)
		{
			RItem rItem = rows[i];
			RItem rItem2 = rows[i - 1];
			if (rItem2.cel.Count < rItem.cel.Count)
			{
				continue;
			}
			for (int j = 0; j < rItem.cel.Count; j++)
			{
				VirtualItem d = rItem.cel[j];
				VirtualItem virtualItem = rItem2.cel[j];
				if (virtualItem.HEIGHT < rItem2.h)
				{
					int num = rItem2.h - virtualItem.HEIGHT;
					if (dictionary.ContainsKey(j))
					{
						dictionary[j] += num;
					}
					else
					{
						dictionary.Add(j, num);
					}
				}
				if (dictionary.TryGetValue(j, out var value))
				{
					HandLayout(rect, d, 0, -value);
				}
			}
		}
	}

	private void WaterfallLayoutEnd(Rectangle rect, List<RItem> rows)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(rows[0].cel.Count);
		for (int i = 1; i < rows.Count; i++)
		{
			RItem rItem = rows[i];
			RItem rItem2 = rows[i - 1];
			if (rItem2.cel.Count == rItem.cel.Count)
			{
				for (int j = 0; j < rItem.cel.Count; j++)
				{
					VirtualItem d = rItem.cel[j];
					VirtualItem virtualItem = rItem2.cel[j];
					if (virtualItem.HEIGHT < rItem2.h)
					{
						int num = rItem2.h - virtualItem.HEIGHT;
						if (dictionary.ContainsKey(j))
						{
							dictionary[j] += num;
						}
						else
						{
							dictionary.Add(j, num);
						}
					}
					if (dictionary.TryGetValue(j, out var value))
					{
						HandLayout(rect, d, 0, -value);
					}
				}
			}
			else
			{
				if (rItem2.cel.Count <= rItem.cel.Count)
				{
					continue;
				}
				for (int k = 0; k < rItem.cel.Count; k++)
				{
					int num2 = rItem2.cel.Count - rItem.cel.Count + k;
					VirtualItem d2 = rItem.cel[k];
					VirtualItem virtualItem2 = rItem2.cel[num2];
					if (virtualItem2.HEIGHT < rItem2.h)
					{
						int num3 = rItem2.h - virtualItem2.HEIGHT;
						if (dictionary.ContainsKey(num2))
						{
							dictionary[num2] += num3;
						}
						else
						{
							dictionary.Add(num2, num3);
						}
					}
					if (dictionary.TryGetValue(num2, out var value2))
					{
						HandLayout(rect, d2, 0, -value2);
					}
				}
			}
		}
	}

	private void WaterfallLayoutCenter(Rectangle rect, List<RItem> rows)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>(rows[0].cel.Count);
		for (int i = 1; i < rows.Count; i++)
		{
			RItem rItem = rows[i];
			RItem rItem2 = rows[i - 1];
			if (rItem2.cel.Count == rItem.cel.Count)
			{
				for (int j = 0; j < rItem.cel.Count; j++)
				{
					VirtualItem d = rItem.cel[j];
					VirtualItem virtualItem = rItem2.cel[j];
					if (virtualItem.HEIGHT < rItem2.h)
					{
						int num = rItem2.h - virtualItem.HEIGHT;
						if (dictionary.ContainsKey(j))
						{
							dictionary[j] += num;
						}
						else
						{
							dictionary.Add(j, num);
						}
					}
					if (dictionary.TryGetValue(j, out var value))
					{
						HandLayout(rect, d, 0, -value);
					}
				}
			}
			else
			{
				if (rItem2.cel.Count <= rItem.cel.Count)
				{
					continue;
				}
				List<int> list = new List<int>(rItem.cel.Count);
				foreach (VirtualItem item in rItem.cel)
				{
					int num2 = -1;
					int num3 = 0;
					for (int k = 0; k < rItem2.cel.Count; k++)
					{
						if (!list.Contains(k) && (rItem2.cel[k].RECT.Y < num2 || num2 == -1))
						{
							num3 = k;
							num2 = rItem2.cel[k].RECT.Y;
						}
					}
					list.Add(num3);
					VirtualItem virtualItem2 = rItem2.cel[num3];
					if (virtualItem2.HEIGHT < rItem2.h)
					{
						int num4 = rItem2.h - virtualItem2.HEIGHT;
						if (dictionary.ContainsKey(num3))
						{
							dictionary[num3] += num4;
						}
						else
						{
							dictionary.Add(num3, num4);
						}
					}
					if (dictionary.TryGetValue(num3, out var value2))
					{
						HandLayout(rect, item, virtualItem2.RECT.X - item.RECT.X, -value2);
					}
				}
			}
		}
	}

	private int GetTotalHeight(List<RItem> rows)
	{
		int num = 0;
		foreach (RItem row in rows)
		{
			num += row.h;
		}
		return num;
	}

	private void HandLayout(Rectangle rect, List<VirtualItem> d, int x, int y)
	{
		if (x == 0 && y == 0)
		{
			return;
		}
		foreach (VirtualItem item in d)
		{
			if (item.WIDTH == rect.Width)
			{
				if (y != 0)
				{
					if (item is VirtualShadowItem virtualShadowItem)
					{
						virtualShadowItem.RECT_S.Offset(0, y);
					}
					item.RECT.Offset(0, y);
				}
			}
			else
			{
				if (item is VirtualShadowItem virtualShadowItem2)
				{
					virtualShadowItem2.RECT_S.Offset(x, y);
				}
				item.RECT.Offset(x, y);
			}
		}
	}

	private void HandLayout(Rectangle rect, VirtualItem d, int x, int y)
	{
		if (d.WIDTH == rect.Width)
		{
			x = 0;
		}
		if (x != 0 || y != 0)
		{
			if (d is VirtualShadowItem virtualShadowItem)
			{
				virtualShadowItem.RECT_S.Offset(x, y);
			}
			d.RECT.Offset(x, y);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (items == null || items.Count == 0 || isEmpty)
		{
			if (Empty)
			{
				PaintEmpty(e.Graphics.High(), ((Control)this).ClientRectangle);
			}
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		int value = ScrollBar.Value;
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		clientRectangle.Offset(0, value);
		canvas.TranslateTransform(0f, -value);
		int num = (int)((float)radius * Config.Dpi);
		foreach (VirtualItem item in items)
		{
			if (item.SHOW)
			{
				item.SHOW_RECT = clientRectangle.Contains(clientRectangle.X, item.RECT.Y) || clientRectangle.Contains(clientRectangle.X, item.RECT.Bottom);
				if (item.SHOW_RECT)
				{
					GraphicsState state = canvas.Save();
					if (item is VirtualShadowItem it)
					{
						DrawShadow(it, canvas, num);
					}
					canvas.TranslateTransform(item.RECT.X, item.RECT.Y);
					item.Paint(canvas, new VirtualPanelArgs(this, new Rectangle(0, 0, item.RECT.Width, item.RECT.Height), num));
					canvas.Restore(state);
				}
			}
			else
			{
				item.SHOW_RECT = false;
			}
		}
		canvas.ResetTransform();
		ScrollBar.Paint(canvas);
		if (Config.Animation && BlurBar != null)
		{
			_event.SetWait();
		}
		((Control)this).OnPaint(e);
	}

	private void PaintEmpty(Canvas g, Rectangle rect)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(Colour.Text.Get("VirtualPanel"));
		try
		{
			string text = EmptyText ?? Localization.Get("NoData", "暂无数据");
			if (EmptyImage == null)
			{
				g.String(text, ((Control)this).Font, (Brush)(object)val, rect, stringCenter);
				return;
			}
			int num = (int)((float)gap * Config.Dpi);
			Size size = g.MeasureString(text, ((Control)this).Font);
			Rectangle rect2 = new Rectangle(rect.X + (rect.Width - EmptyImage.Width) / 2, rect.Y + (rect.Height - EmptyImage.Height) / 2 - size.Height, EmptyImage.Width, EmptyImage.Height);
			Rectangle rect3 = new Rectangle(rect.X, rect2.Bottom + num, rect.Width, size.Height);
			g.Image(EmptyImage, rect2);
			g.String(text, ((Control)this).Font, (Brush)(object)val, rect3, stringCenter);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void LongTask()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		while (!_event.Wait())
		{
			try
			{
				if (items != null && items.Count > 0 && BlurBar != null)
				{
					int value = ScrollBar.Value;
					int height = BlurBar.Height;
					if (value > height)
					{
						value -= height;
						Rectangle clientRectangle = ((Control)this).ClientRectangle;
						Bitmap val = new Bitmap(clientRectangle.Width, height);
						using (Canvas canvas = Graphics.FromImage((Image)(object)val).HighLay())
						{
							clientRectangle.Offset(0, value);
							canvas.TranslateTransform(0f, -value);
							int num = (int)((float)radius * Config.Dpi);
							foreach (VirtualItem item in items)
							{
								if (item.SHOW && item.SHOW_RECT)
								{
									if (item is VirtualShadowItem it)
									{
										DrawShadow(it, canvas, num);
									}
									item.Paint(canvas, new VirtualPanelArgs(this, item.RECT, num));
								}
							}
							canvas.ResetTransform();
							SolidBrush val2 = new SolidBrush(Color.FromArgb(45, BlurBar.BackColor));
							try
							{
								canvas.Fill((Brush)(object)val2, 0, 0, ((Image)val).Width, ((Image)val).Height);
							}
							finally
							{
								((IDisposable)val2)?.Dispose();
							}
						}
						Helper.Blur(val, height * 6);
						IBlurBar(BlurBar, val);
					}
					else
					{
						IBlurBar(BlurBar, null);
					}
				}
				else if (BlurBar != null)
				{
					IBlurBar(BlurBar, null);
				}
				_event.ResetWait();
			}
			catch
			{
				break;
			}
		}
	}

	private void IBlurBar(Control BlurBar, Bitmap? bmp)
	{
		Control BlurBar2 = BlurBar;
		Bitmap bmp2 = bmp;
		((Control)this).Invoke((Delegate)(Action)delegate
		{
			Image backgroundImage = BlurBar2.BackgroundImage;
			if (backgroundImage != null)
			{
				backgroundImage.Dispose();
			}
			BlurBar2.BackgroundImage = (Image)(object)bmp2;
		});
	}

	protected override void Dispose(bool disposing)
	{
		BlurBar = null;
		_event?.WaitDispose();
		base.Dispose(disposing);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		LoadLayout();
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.THEME && Config.Animation && BlurBar != null)
		{
			_event.SetWait();
		}
	}

	private void DrawShadow(VirtualShadowItem it, Canvas g, float radius)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		if (shadow <= 0)
		{
			return;
		}
		string key = it.RECT_S.Width + "_" + it.RECT_S.Height;
		lock (shadow_dir_tmp)
		{
			if (!shadow_dir_tmp.ContainsKey(key))
			{
				int num = (int)((float)Shadow * Config.Dpi);
				GraphicsPath val = new Rectangle(num, num, it.RECT.Width, it.RECT.Height).RoundPath(radius, shadowAlign);
				try
				{
					shadow_dir_tmp.Add(key, val.PaintShadow(it.RECT_S.Width, it.RECT_S.Height, shadowColor ?? Colour.TextBase.Get("VirtualPanel"), num));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			if (!shadow_dir_tmp.TryGetValue(key, out Bitmap value))
			{
				return;
			}
			ImageAttributes val2 = new ImageAttributes();
			try
			{
				ColorMatrix val3 = new ColorMatrix();
				if (it.AnimationHover)
				{
					val3.Matrix33 = it.AnimationHoverValue;
				}
				else if (it.Hover)
				{
					val3.Matrix33 = shadowOpacityHover;
				}
				else
				{
					val3.Matrix33 = shadowOpacity;
				}
				val2.SetColorMatrix(val3, (ColorMatrixFlag)0, (ColorAdjustType)1);
				g.Image((Image)(object)value, new Rectangle(it.RECT_S.X + shadowOffsetX, it.RECT_S.Y + shadowOffsetY, it.RECT_S.Width, it.RECT_S.Height), 0, 0, ((Image)value).Width, ((Image)value).Height, (GraphicsUnit)2, val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		((Control)this).OnMouseDown(e);
		MDown = null;
		if (!ScrollBar.MouseDown(e.Location) || items == null || items.Count == 0)
		{
			return;
		}
		OnTouchDown(e.X, e.Y);
		int x = e.X;
		int y = e.Y + ScrollBar.Value;
		foreach (VirtualItem item in items)
		{
			if (item.SHOW && item.SHOW_RECT && item.CanClick && item.RECT.Contains(x, y))
			{
				isdouclick = e.Clicks;
				MDown = item;
				break;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (!ScrollBar.MouseMove(e.Location) || !OnTouchMove(e.X, e.Y) || items == null || items.Count == 0)
		{
			return;
		}
		int x = e.X;
		int y = e.Y + ScrollBar.Value;
		int num = 0;
		int num2 = 0;
		foreach (VirtualItem item in items)
		{
			if (item.SHOW && item.SHOW_RECT && item.CanClick && item.RECT.Contains(x, y))
			{
				if (item.MouseMove(this, new VirtualPanelMouseArgs(item, item.RECT, x, y, e)))
				{
					num2++;
				}
				if (!item.Hover)
				{
					item.Hover = true;
					num++;
					SetHover(item, value: true);
				}
			}
			else if (item.Hover)
			{
				item.MouseLeave(this, new VirtualPanelMouseArgs(item, item.RECT, x, y, e));
				item.Hover = false;
				num++;
				SetHover(item, value: false);
			}
		}
		if (num > 0)
		{
			((Control)this).Invalidate();
		}
		SetCursor(num2 > 0);
	}

	private void SetHover(VirtualItem it, bool value)
	{
		if (!base.Enabled || !ShadowOpacityAnimation || shadow <= 0 || !(shadowOpacityHover > 0f))
		{
			return;
		}
		VirtualShadowItem virtualShadow = it as VirtualShadowItem;
		if (virtualShadow == null || !(shadowOpacityHover > shadowOpacity))
		{
			return;
		}
		if (Config.Animation)
		{
			virtualShadow.ThreadHover?.Dispose();
			virtualShadow.AnimationHover = true;
			float addvalue = shadowOpacityHover / 12f;
			if (value)
			{
				virtualShadow.ThreadHover = new ITask((Control)(object)this, delegate
				{
					virtualShadow.AnimationHoverValue = virtualShadow.AnimationHoverValue.Calculate(addvalue);
					if (virtualShadow.AnimationHoverValue >= shadowOpacityHover)
					{
						virtualShadow.AnimationHoverValue = shadowOpacityHover;
						return false;
					}
					((Control)this).Invalidate();
					return true;
				}, 20, delegate
				{
					virtualShadow.AnimationHover = false;
					((Control)this).Invalidate();
				});
				return;
			}
			virtualShadow.ThreadHover = new ITask((Control)(object)this, delegate
			{
				virtualShadow.AnimationHoverValue = virtualShadow.AnimationHoverValue.Calculate(0f - addvalue);
				if (virtualShadow.AnimationHoverValue <= shadowOpacity)
				{
					virtualShadow.AnimationHoverValue = shadowOpacity;
					return false;
				}
				((Control)this).Invalidate();
				return true;
			}, 20, delegate
			{
				virtualShadow.AnimationHover = false;
				((Control)this).Invalidate();
			});
		}
		else
		{
			((Control)this).Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (ScrollBar.MouseUp() && OnTouchUp() && MDown != null)
		{
			int x = e.X;
			int y = e.Y + ScrollBar.Value;
			if (MDown.RECT.Contains(x, y))
			{
				this.ItemClick?.Invoke(this, new VirtualItemEventArgs(MDown, e));
				MDown.MouseClick(this, new VirtualPanelMouseArgs(MDown, MDown.RECT, x, y, e, isdouclick));
			}
		}
		MDown = null;
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

	private void ILeave()
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (VirtualItem item in items)
		{
			if (item.Hover)
			{
				item.Hover = false;
				num++;
				SetHover(item, value: false);
			}
		}
		if (num > 0)
		{
			((Control)this).Invalidate();
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
}
