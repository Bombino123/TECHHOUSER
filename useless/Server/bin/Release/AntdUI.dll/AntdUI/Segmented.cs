using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Segmented 分段控制器")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("SelectIndexChanged")]
public class Segmented : IControl
{
	private TAlignMini barPosition;

	private float barsize = 3f;

	private int barpadding;

	private bool vertical;

	private bool full;

	private int radius = 6;

	private float? iconratio;

	private float icongap = 0.2f;

	private bool round;

	private TAlignMini iconalign = TAlignMini.Top;

	private int igap;

	private Color? back;

	private Color? backactive;

	private Color? fore;

	private Color? foreactive;

	private RightToLeft rightToLeft;

	private SegmentedItemCollection? items;

	private int _select = -1;

	private bool AnimationBar;

	private RectangleF AnimationBarValue;

	private ITask? ThreadBar;

	private RectangleF TabSelectRect;

	private bool pauseLayout;

	private readonly StringFormat s_f = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private Rectangle Rect;

	[Description("原装背景颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color OriginalBackColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			((Control)this).BackColor = value;
		}
	}

	[Description("线条位置")]
	[Category("条")]
	[DefaultValue(TAlignMini.None)]
	public TAlignMini BarPosition
	{
		get
		{
			return barPosition;
		}
		set
		{
			if (barPosition != value)
			{
				barPosition = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BarPosition");
			}
		}
	}

	[Description("条大小")]
	[Category("条")]
	[DefaultValue(3f)]
	public float BarSize
	{
		get
		{
			return barsize;
		}
		set
		{
			if (barsize != value)
			{
				barsize = value;
				if (barPosition != 0)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BarSize");
			}
		}
	}

	[Description("条边距")]
	[Category("条")]
	[DefaultValue(0)]
	public int BarPadding
	{
		get
		{
			return barpadding;
		}
		set
		{
			if (barpadding != value)
			{
				barpadding = value;
				if (barPosition != 0)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BarPadding");
			}
		}
	}

	[Description("条圆角")]
	[Category("条")]
	[DefaultValue(0)]
	public int BarRadius { get; set; }

	[Description("是否竖向")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Vertical
	{
		get
		{
			return vertical;
		}
		set
		{
			if (vertical != value)
			{
				vertical = value;
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("Vertical");
			}
		}
	}

	[Description("是否铺满")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Full
	{
		get
		{
			return full;
		}
		set
		{
			if (full != value)
			{
				full = value;
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("Full");
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
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(null)]
	public float? IconRatio
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
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	[Description("图标与文字间距比例")]
	[Category("外观")]
	[DefaultValue(0.2f)]
	public float IconGap
	{
		get
		{
			return icongap;
		}
		set
		{
			if (icongap != value)
			{
				icongap = value;
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("IconGap");
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

	[Description("图标对齐方向")]
	[Category("外观")]
	[DefaultValue(TAlignMini.Top)]
	public TAlignMini IconAlign
	{
		get
		{
			return iconalign;
		}
		set
		{
			if (iconalign != value)
			{
				iconalign = value;
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("IconAlign");
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
			return igap;
		}
		set
		{
			if (igap != value)
			{
				igap = value;
				ChangeItems();
				((Control)this).Invalidate();
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackColor
	{
		get
		{
			return back;
		}
		set
		{
			if (!(back == value))
			{
				back = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackColor");
			}
		}
	}

	[Description("悬停背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackHover { get; set; }

	[Description("激活背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackActive
	{
		get
		{
			return backactive;
		}
		set
		{
			if (!(backactive == value))
			{
				backactive = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackActive");
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

	[Description("悬停文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeHover { get; set; }

	[Description("激活文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeActive
	{
		get
		{
			return foreactive;
		}
		set
		{
			if (!(foreactive == value))
			{
				foreactive = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ForeActive");
			}
		}
	}

	[Description("反向")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public override RightToLeft RightToLeft
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return rightToLeft;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (rightToLeft != value)
			{
				rightToLeft = value;
				if (!full)
				{
					ChangeItems();
					((Control)this).Invalidate();
					OnPropertyChanged("RightToLeft");
				}
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	[DefaultValue(null)]
	public SegmentedItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new SegmentedItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Description("选择序号")]
	[Category("数据")]
	[DefaultValue(-1)]
	public int SelectIndex
	{
		get
		{
			return _select;
		}
		set
		{
			if (_select != value)
			{
				int select = _select;
				_select = value;
				this.SelectIndexChanged?.Invoke(this, new IntEventArgs(value));
				SetRect(select, _select);
				OnPropertyChanged("SelectIndex");
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
					ChangeItems();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PauseLayout");
			}
		}
	}

	[Browsable(true)]
	[Description("自动大小")]
	[Category("外观")]
	[DefaultValue(false)]
	public override bool AutoSize
	{
		get
		{
			return ((Control)this).AutoSize;
		}
		set
		{
			if (((Control)this).AutoSize != value)
			{
				((Control)this).AutoSize = value;
				BeforeAutoSize();
			}
		}
	}

	[Description("SelectIndex 属性值更改时发生")]
	[Category("行为")]
	public event IntEventHandler? SelectIndexChanged;

	[Description("SelectIndex 属性值更改前发生")]
	[Category("行为")]
	public event IntBoolEventHandler? SelectIndexChanging;

	[Description("点击项时发生")]
	[Category("行为")]
	public event SegmentedItemEventHandler? ItemClick;

	public Segmented()
	{
		((Control)this).BackColor = Color.Transparent;
	}

	protected override void Dispose(bool disposing)
	{
		ThreadBar?.Dispose();
		base.Dispose(disposing);
	}

	private void SetRect(int old, int value)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		if (items.ListExceed(value))
		{
			((Control)this).Invalidate();
			return;
		}
		SegmentedItem segmentedItem = items[value];
		if (items.ListExceed(old))
		{
			AnimationBarValue = (TabSelectRect = segmentedItem.Rect);
			((Control)this).Invalidate();
			return;
		}
		SegmentedItem segmentedItem2 = items[old];
		ThreadBar?.Dispose();
		RectangleF OldValue = segmentedItem2.Rect;
		RectangleF NewValue = segmentedItem.Rect;
		if (Config.Animation)
		{
			if (vertical)
			{
				if (OldValue.X != NewValue.X)
				{
					return;
				}
				AnimationBar = true;
				TabSelectRect = NewValue;
				float p_val2 = Math.Abs(NewValue.Y - AnimationBarValue.Y) * 0.09f;
				float p_w_val2 = Math.Abs(NewValue.Height - AnimationBarValue.Height) * 0.1f;
				float p_val4 = (NewValue.Y - AnimationBarValue.Y) * 0.5f;
				ThreadBar = new ITask((Control)(object)this, delegate
				{
					if (AnimationBarValue.Height != NewValue.Height)
					{
						if (NewValue.Height > OldValue.Height)
						{
							AnimationBarValue.Height += p_w_val2;
							if (AnimationBarValue.Height > NewValue.Height)
							{
								AnimationBarValue.Height = NewValue.Height;
							}
						}
						else
						{
							AnimationBarValue.Height -= p_w_val2;
							if (AnimationBarValue.Height < NewValue.Height)
							{
								AnimationBarValue.Height = NewValue.Height;
							}
						}
					}
					if (NewValue.Y > OldValue.Y)
					{
						if (AnimationBarValue.Y > p_val4)
						{
							AnimationBarValue.Y += p_val2 / 2f;
						}
						else
						{
							AnimationBarValue.Y += p_val2;
						}
						if (AnimationBarValue.Y > NewValue.Y)
						{
							AnimationBarValue.Y = NewValue.Y;
							((Control)this).Invalidate();
							return false;
						}
					}
					else
					{
						AnimationBarValue.Y -= p_val2;
						if (AnimationBarValue.Y < NewValue.Y)
						{
							AnimationBarValue.Y = NewValue.Y;
							((Control)this).Invalidate();
							return false;
						}
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationBarValue = NewValue;
					AnimationBar = false;
					((Control)this).Invalidate();
				});
			}
			else
			{
				if (OldValue.Y != NewValue.Y)
				{
					return;
				}
				AnimationBar = true;
				TabSelectRect = NewValue;
				float p_val = Math.Abs(NewValue.X - AnimationBarValue.X) * 0.09f;
				float p_w_val = Math.Abs(NewValue.Width - AnimationBarValue.Width) * 0.1f;
				float p_val3 = (NewValue.X - AnimationBarValue.X) * 0.5f;
				ThreadBar = new ITask((Control)(object)this, delegate
				{
					if (AnimationBarValue.Width != NewValue.Width)
					{
						if (NewValue.Width > OldValue.Width)
						{
							AnimationBarValue.Width += p_w_val;
							if (AnimationBarValue.Width > NewValue.Width)
							{
								AnimationBarValue.Width = NewValue.Width;
							}
						}
						else
						{
							AnimationBarValue.Width -= p_w_val;
							if (AnimationBarValue.Width < NewValue.Width)
							{
								AnimationBarValue.Width = NewValue.Width;
							}
						}
					}
					if (NewValue.X > OldValue.X)
					{
						if (AnimationBarValue.X > p_val3)
						{
							AnimationBarValue.X += p_val / 2f;
						}
						else
						{
							AnimationBarValue.X += p_val;
						}
						if (AnimationBarValue.X > NewValue.X)
						{
							AnimationBarValue.X = NewValue.X;
							((Control)this).Invalidate();
							return false;
						}
					}
					else
					{
						AnimationBarValue.X -= p_val;
						if (AnimationBarValue.X < NewValue.X)
						{
							AnimationBarValue.X = NewValue.X;
							((Control)this).Invalidate();
							return false;
						}
					}
					((Control)this).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationBarValue = NewValue;
					AnimationBar = false;
					((Control)this).Invalidate();
				});
			}
		}
		else
		{
			TabSelectRect = (AnimationBarValue = NewValue);
			((Control)this).Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Expected O, but got Unknown
		if (items == null || items.Count == 0)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		float num = (float)radius * Config.Dpi;
		GraphicsPath val = Rect.RoundPath(num, Round);
		try
		{
			canvas.Fill(back ?? Colour.BgLayout.Get("Segmented"), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		List<SegmentedItem> list = new List<SegmentedItem>(items.Count);
		int num2 = -1;
		for (int i = 0; i < items.Count; i++)
		{
			SegmentedItem segmentedItem = items[i];
			if (segmentedItem == null)
			{
				continue;
			}
			if (i == _select && !AnimationBar)
			{
				Color color = backactive ?? Colour.BgElevated.Get("Segmented");
				if (barPosition == TAlignMini.None)
				{
					GraphicsPath val2 = TabSelectRect.RoundPath(num, Round);
					try
					{
						canvas.Fill(color, val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				else
				{
					float barSize = BarSize * Config.Dpi;
					float num3 = (float)BarPadding * Config.Dpi;
					float barPadding = num3 * 2f;
					RectangleF barRect = GetBarRect(TabSelectRect, barSize, num3, barPadding);
					if (BarRadius > 0)
					{
						GraphicsPath val3 = barRect.RoundPath((float)BarRadius * Config.Dpi);
						try
						{
							canvas.Fill(color, val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					else
					{
						canvas.Fill(color, barRect);
					}
				}
			}
			else if (segmentedItem.Hover)
			{
				num2 = i;
				GraphicsPath val4 = segmentedItem.Rect.RoundPath(num, Round);
				try
				{
					canvas.Fill(BackHover ?? Colour.HoverBg.Get("Segmented"), val4);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			list.Add(segmentedItem);
		}
		if (AnimationBar)
		{
			Color color2 = backactive ?? Colour.BgElevated.Get("Segmented");
			if (barPosition == TAlignMini.None)
			{
				GraphicsPath val5 = AnimationBarValue.RoundPath(num, Round);
				try
				{
					canvas.Fill(color2, val5);
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
			}
			else
			{
				float barSize2 = BarSize * Config.Dpi;
				float num4 = (float)BarPadding * Config.Dpi;
				float barPadding2 = num4 * 2f;
				RectangleF barRect2 = GetBarRect(AnimationBarValue, barSize2, num4, barPadding2);
				if (BarRadius > 0)
				{
					GraphicsPath val6 = barRect2.RoundPath((float)BarRadius * Config.Dpi);
					try
					{
						canvas.Fill(color2, val6);
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
				}
				else
				{
					canvas.Fill(color2, barRect2);
				}
			}
		}
		bool enabled = base.Enabled;
		SolidBrush val7 = new SolidBrush(fore ?? Colour.TextSecondary.Get("Segmented"));
		try
		{
			SolidBrush val8 = new SolidBrush(Colour.TextQuaternary.Get("Segmented"));
			try
			{
				for (int j = 0; j < list.Count; j++)
				{
					SegmentedItem segmentedItem2 = list[j];
					if (j == _select)
					{
						if (enabled && segmentedItem2.Enabled)
						{
							Color color3 = foreactive ?? Colour.Text.Get("Segmented");
							if (PaintImg(canvas, segmentedItem2, color3, segmentedItem2.IconActiveSvg, segmentedItem2.IconActive))
							{
								PaintImg(canvas, segmentedItem2, color3, segmentedItem2.IconSvg, segmentedItem2.Icon);
							}
							canvas.String(segmentedItem2.Text, ((Control)this).Font, color3, segmentedItem2.RectText, s_f);
						}
						else
						{
							Color color4 = Colour.TextQuaternary.Get("Segmented");
							if (PaintImg(canvas, segmentedItem2, color4, segmentedItem2.IconActiveSvg, segmentedItem2.IconActive))
							{
								PaintImg(canvas, segmentedItem2, color4, segmentedItem2.IconSvg, segmentedItem2.Icon);
							}
							canvas.String(segmentedItem2.Text, ((Control)this).Font, color4, segmentedItem2.RectText, s_f);
						}
					}
					else if (enabled && segmentedItem2.Enabled)
					{
						if (j == num2)
						{
							Color color5 = ForeHover ?? Colour.HoverColor.Get("Segmented");
							PaintImg(canvas, segmentedItem2, color5, segmentedItem2.IconSvg, segmentedItem2.Icon);
							canvas.String(segmentedItem2.Text, ((Control)this).Font, color5, segmentedItem2.RectText, s_f);
						}
						else
						{
							PaintImg(canvas, segmentedItem2, val7.Color, segmentedItem2.IconSvg, segmentedItem2.Icon);
							canvas.String(segmentedItem2.Text, ((Control)this).Font, (Brush)(object)val7, segmentedItem2.RectText, s_f);
						}
					}
					else
					{
						PaintImg(canvas, segmentedItem2, val8.Color, segmentedItem2.IconSvg, segmentedItem2.Icon);
						canvas.String(segmentedItem2.Text, ((Control)this).Font, (Brush)(object)val8, segmentedItem2.RectText, s_f);
					}
					segmentedItem2.PaintBadge(((Control)this).Font, segmentedItem2.Rect, canvas);
				}
			}
			finally
			{
				((IDisposable)val8)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val7)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private bool PaintImg(Canvas g, SegmentedItem it, Color color, string? svg, Image? bmp)
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

	private RectangleF GetBarRect(RectangleF rect, float barSize, float barPadding, float barPadding2)
	{
		return barPosition switch
		{
			TAlignMini.Top => new RectangleF(rect.X + barPadding, rect.Y, rect.Width - barPadding2, barSize), 
			TAlignMini.Left => new RectangleF(rect.X, rect.Y + barPadding, barSize, rect.Height - barPadding2), 
			TAlignMini.Right => new RectangleF(rect.Right - barSize, rect.Y + barPadding, barSize, rect.Height - barPadding2), 
			_ => new RectangleF(rect.X + barPadding, rect.Bottom - barSize, rect.Width - barPadding2, barSize), 
		};
	}

	internal void ChangeItems()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (items == null || items.Count == 0)
		{
			_select = -1;
			return;
		}
		if (_select >= items.Count)
		{
			_select = items.Count - 1;
		}
		if (pauseLayout)
		{
			return;
		}
		Rectangle _rect = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		if (_rect.Width == 0 || _rect.Height == 0)
		{
			return;
		}
		Rectangle rect = _rect.PaddingRect(((Control)this).Margin);
		Helper.GDI(delegate(Canvas g)
		{
			//IL_159b: Unknown result type (might be due to invalid IL or missing references)
			//IL_15a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f32: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f37: Unknown result type (might be due to invalid IL or missing references)
			//IL_15e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_15e8: Invalid comparison between Unknown and I4
			//IL_0f71: Unknown result type (might be due to invalid IL or missing references)
			//IL_0f77: Invalid comparison between Unknown and I4
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int height = size.Height;
			int num = (int)((float)height * icongap);
			int num2 = (int)((float)igap * Config.Dpi);
			int num3 = (int)((float)size.Height * 0.6f);
			int num4 = num3 * 2;
			Padding margin;
			if (Full)
			{
				int count = items.Count;
				if (Vertical)
				{
					int num5 = (rect.Height - num2 * (count - 1)) / count;
					int num6 = 0;
					switch (iconalign)
					{
					case TAlignMini.Top:
					{
						int imgsize4 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
						foreach (SegmentedItem item in items)
						{
							item.PARENT = this;
							if (item.HasIcon && item.HasEmptyText)
							{
								item.SetIconNoText(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize4);
							}
							else
							{
								item.SetRectTopFull(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize4, height, num, g, ((Control)this).Font);
							}
							num6 += num5 + num2;
						}
						break;
					}
					case TAlignMini.Bottom:
					{
						int imgsize3 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
						foreach (SegmentedItem item2 in items)
						{
							item2.PARENT = this;
							if (item2.HasIcon && item2.HasEmptyText)
							{
								item2.SetIconNoText(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize3);
							}
							else
							{
								item2.SetRectBottomFull(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize3, height, num, g, ((Control)this).Font);
							}
							num6 += num5 + num2;
						}
						break;
					}
					case TAlignMini.Left:
					{
						int imgsize2 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
						foreach (SegmentedItem item3 in items)
						{
							item3.PARENT = this;
							if (item3.HasIcon && item3.HasEmptyText)
							{
								item3.SetIconNoText(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize2);
							}
							else
							{
								item3.SetRectLeft(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize2, num, num3);
							}
							num6 += num5 + num2;
						}
						break;
					}
					case TAlignMini.Right:
					{
						int imgsize = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
						foreach (SegmentedItem item4 in items)
						{
							item4.PARENT = this;
							if (item4.HasIcon && item4.HasEmptyText)
							{
								item4.SetIconNoText(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize);
							}
							else
							{
								item4.SetRectRight(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5), imgsize, num, num3);
							}
							num6 += num5 + num2;
						}
						break;
					}
					default:
						foreach (SegmentedItem item5 in items)
						{
							item5.PARENT = this;
							item5.SetRectNone(new Rectangle(rect.X, rect.Y + num6, rect.Width, num5));
							num6 += num5 + num2;
						}
						break;
					}
				}
				else
				{
					int num7 = (rect.Width - num2 * (count - 1)) / count;
					int num8 = 0;
					switch (iconalign)
					{
					case TAlignMini.Top:
					{
						int imgsize8 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
						foreach (SegmentedItem item6 in items)
						{
							item6.PARENT = this;
							if (item6.HasIcon && item6.HasEmptyText)
							{
								item6.SetIconNoText(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize8);
							}
							else
							{
								item6.SetRectTop(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize8, height, num);
							}
							num8 += num7 + num2;
						}
						break;
					}
					case TAlignMini.Bottom:
					{
						int imgsize7 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
						foreach (SegmentedItem item7 in items)
						{
							item7.PARENT = this;
							if (item7.HasIcon && item7.HasEmptyText)
							{
								item7.SetIconNoText(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize7);
							}
							else
							{
								item7.SetRectBottom(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize7, height, num);
							}
							num8 += num7 + num2;
						}
						break;
					}
					case TAlignMini.Left:
					{
						int imgsize6 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
						foreach (SegmentedItem item8 in items)
						{
							item8.PARENT = this;
							if (item8.HasIcon && item8.HasEmptyText)
							{
								item8.SetIconNoText(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize6);
							}
							else
							{
								item8.SetRectLeft(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize6, num, num3);
							}
							num8 += num7 + num2;
						}
						break;
					}
					case TAlignMini.Right:
					{
						int imgsize5 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
						foreach (SegmentedItem item9 in items)
						{
							item9.PARENT = this;
							if (item9.HasIcon && item9.HasEmptyText)
							{
								item9.SetIconNoText(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize5);
							}
							else
							{
								item9.SetRectRight(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height), imgsize5, num, num3);
							}
							num8 += num7 + num2;
						}
						break;
					}
					default:
						foreach (SegmentedItem item10 in items)
						{
							item10.PARENT = this;
							item10.SetRectNone(new Rectangle(rect.X + num8, rect.Y, num7, rect.Height));
							num8 += num7 + num2;
						}
						break;
					}
				}
				Rect = _rect;
			}
			else if (Vertical)
			{
				int num9 = 0;
				switch (iconalign)
				{
				case TAlignMini.Top:
				{
					int imgsize12 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
					int height6 = (int)Math.Ceiling((float)size.Height * 2.4f + (float)num4);
					foreach (SegmentedItem item11 in items)
					{
						item11.PARENT = this;
						if (item11.HasIcon && item11.HasEmptyText)
						{
							item11.SetIconNoText(new Rectangle(rect.X, rect.Y + num9, rect.Width, height6), imgsize12);
						}
						else
						{
							item11.SetRectTop(new Rectangle(rect.X, rect.Y + num9, rect.Width, height6), imgsize12, height, num, g, ((Control)this).Font);
						}
						num9 += item11.Rect.Height + num2;
					}
					break;
				}
				case TAlignMini.Bottom:
				{
					int imgsize11 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
					int height5 = (int)Math.Ceiling((float)size.Height * 2.4f + (float)num4);
					foreach (SegmentedItem item12 in items)
					{
						item12.PARENT = this;
						if (item12.HasIcon && item12.HasEmptyText)
						{
							item12.SetIconNoText(new Rectangle(rect.X, rect.Y + num9, rect.Width, height5), imgsize11);
						}
						else
						{
							item12.SetRectBottom(new Rectangle(rect.X, rect.Y + num9, rect.Width, height5), imgsize11, height, num, g, ((Control)this).Font);
						}
						num9 += item12.Rect.Height + num2;
					}
					break;
				}
				case TAlignMini.Left:
				{
					int imgsize10 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
					int height4 = size.Height + num4;
					foreach (SegmentedItem item13 in items)
					{
						item13.PARENT = this;
						if (item13.HasIcon && item13.HasEmptyText)
						{
							item13.SetIconNoText(new Rectangle(rect.X, rect.Y + num9, rect.Width, height4), imgsize10);
						}
						else
						{
							item13.SetRectLeft(new Rectangle(rect.X, rect.Y + num9, rect.Width, height4), imgsize10, num, num3);
						}
						num9 += item13.Rect.Height + num2;
					}
					break;
				}
				case TAlignMini.Right:
				{
					int imgsize9 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
					int height3 = size.Height + num4;
					foreach (SegmentedItem item14 in items)
					{
						item14.PARENT = this;
						if (item14.HasIcon && item14.HasEmptyText)
						{
							item14.SetIconNoText(new Rectangle(rect.X, rect.Y + num9, rect.Width, height3), imgsize9);
						}
						else
						{
							item14.SetRectRight(new Rectangle(rect.X, rect.Y + num9, rect.Width, height3), imgsize9, num, num3);
						}
						num9 += item14.Rect.Height + num2;
					}
					break;
				}
				default:
				{
					int height2 = size.Height + num3;
					foreach (SegmentedItem item15 in items)
					{
						item15.PARENT = this;
						item15.SetRectNone(new Rectangle(rect.X, rect.Y + num9, rect.Width, height2));
						num9 += item15.Rect.Height + num2;
					}
					break;
				}
				}
				Segmented segmented = this;
				int x = _rect.X;
				int y = _rect.Y;
				int width = _rect.Width;
				int num10 = num9 - num2;
				margin = ((Control)this).Margin;
				segmented.Rect = new Rectangle(x, y, width, num10 + ((Padding)(ref margin)).Vertical);
				if (Rect.Height < _rect.Height && (int)rightToLeft == 1)
				{
					int y2 = _rect.Bottom - Rect.Height;
					Rect.Y = y2;
					foreach (SegmentedItem item16 in items)
					{
						item16.SetOffset(0, y2);
					}
				}
			}
			else
			{
				int num11 = 0;
				switch (iconalign)
				{
				case TAlignMini.Top:
				{
					int num15 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
					foreach (SegmentedItem item17 in items)
					{
						item17.PARENT = this;
						if (item17.HasIcon && item17.HasEmptyText)
						{
							item17.SetIconNoText(new Rectangle(rect.X + num11, rect.Y, num15 + num4, rect.Height), num15);
						}
						else
						{
							Size size6 = g.MeasureString(item17.Text, ((Control)this).Font);
							item17.SetRectTop(new Rectangle(rect.X + num11, rect.Y, size6.Width + num4, rect.Height), num15, size6.Height, num);
						}
						num11 += item17.Rect.Width + num2;
					}
					break;
				}
				case TAlignMini.Bottom:
				{
					int num14 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.8f));
					foreach (SegmentedItem item18 in items)
					{
						item18.PARENT = this;
						if (item18.HasIcon && item18.HasEmptyText)
						{
							item18.SetIconNoText(new Rectangle(rect.X + num11, rect.Y, num14 + num4, rect.Height), num14);
						}
						else
						{
							Size size5 = g.MeasureString(item18.Text, ((Control)this).Font);
							item18.SetRectBottom(new Rectangle(rect.X + num11, rect.Y, size5.Width + num4, rect.Height), num14, size5.Height, num);
						}
						num11 += item18.Rect.Width + num2;
					}
					break;
				}
				case TAlignMini.Left:
				{
					int num13 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
					foreach (SegmentedItem item19 in items)
					{
						item19.PARENT = this;
						if (item19.HasIcon && item19.HasEmptyText)
						{
							item19.SetIconNoText(new Rectangle(rect.X + num11, rect.Y, num13 + num4, rect.Height), num13);
						}
						else
						{
							Size size4 = g.MeasureString(item19.Text, ((Control)this).Font);
							item19.SetRectLeft(new Rectangle(rect.X + num11, rect.Y, size4.Width + num13 + num + num4, rect.Height), num13, num, num3);
						}
						num11 += item19.Rect.Width + num2;
					}
					break;
				}
				case TAlignMini.Right:
				{
					int num12 = (int)((float)size.Height * iconratio.GetValueOrDefault(1.2f));
					foreach (SegmentedItem item20 in items)
					{
						item20.PARENT = this;
						if (item20.HasIcon && item20.HasEmptyText)
						{
							item20.SetIconNoText(new Rectangle(rect.X + num11, rect.Y, num12 + num4, rect.Height), num12);
						}
						else
						{
							Size size3 = g.MeasureString(item20.Text, ((Control)this).Font);
							item20.SetRectRight(new Rectangle(rect.X + num11, rect.Y, size3.Width + num12 + num + num4, rect.Height), num12, num, num3);
						}
						num11 += item20.Rect.Width + num2;
					}
					break;
				}
				default:
					foreach (SegmentedItem item21 in items)
					{
						item21.PARENT = this;
						Size size2 = g.MeasureString(item21.Text, ((Control)this).Font);
						item21.SetRectNone(new Rectangle(rect.X + num11, rect.Y, size2.Width + num4, rect.Height));
						num11 += item21.Rect.Width + num2;
					}
					break;
				}
				Segmented segmented2 = this;
				int x2 = _rect.X;
				int y3 = _rect.Y;
				int num16 = num11 - num2;
				margin = ((Control)this).Margin;
				segmented2.Rect = new Rectangle(x2, y3, num16 + ((Padding)(ref margin)).Horizontal, _rect.Height);
				if (Rect.Width < _rect.Width && (int)rightToLeft == 1)
				{
					int x3 = _rect.Right - Rect.Width;
					Rect.X = x3;
					foreach (SegmentedItem item22 in items)
					{
						item22.SetOffset(x3, 0);
					}
				}
			}
		});
		if (_select > -1)
		{
			SegmentedItem segmentedItem = items[_select];
			AnimationBarValue = (TabSelectRect = segmentedItem.Rect);
		}
	}

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

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (items == null || items.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (SegmentedItem item in items)
		{
			bool flag = item.Enabled && item.Rect.Contains(e.Location);
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
		foreach (SegmentedItem item in items)
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
		foreach (SegmentedItem item in items)
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
			SegmentedItem segmentedItem = items[i];
			if (segmentedItem != null && segmentedItem.Enabled && segmentedItem.Rect.Contains(e.Location))
			{
				bool flag = false;
				if (this.SelectIndexChanging == null)
				{
					flag = true;
				}
				else if (this.SelectIndexChanging(this, new IntEventArgs(i)))
				{
					flag = true;
				}
				if (flag)
				{
					SelectIndex = i;
				}
				this.ItemClick?.Invoke(this, new SegmentedItemEventArgs(segmentedItem, e));
				break;
			}
		}
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (((Control)this).AutoSize)
		{
			if (Vertical)
			{
				return new Size(((Control)this).GetPreferredSize(proposedSize).Width, Rect.Height);
			}
			return new Size(Rect.Width, ((Control)this).GetPreferredSize(proposedSize).Height);
		}
		return ((Control)this).GetPreferredSize(proposedSize);
	}

	protected override void OnResize(EventArgs e)
	{
		BeforeAutoSize();
		((Control)this).OnResize(e);
	}

	private bool BeforeAutoSize()
	{
		if (((Control)this).AutoSize)
		{
			if (((Control)this).InvokeRequired)
			{
				bool flag = false;
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					flag = BeforeAutoSize();
				});
				return flag;
			}
			if (Vertical)
			{
				int height = Rect.Height;
				if (((Control)this).Height == height)
				{
					return true;
				}
				((Control)this).Height = height;
			}
			else
			{
				int width = Rect.Width;
				if (((Control)this).Width == width)
				{
					return true;
				}
				((Control)this).Width = width;
			}
			return false;
		}
		return true;
	}
}
