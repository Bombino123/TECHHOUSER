using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Pagination 分页")]
[ToolboxItem(true)]
[DefaultProperty("Current")]
[DefaultEvent("ValueChanged")]
public class Pagination : IControl
{
	private class ButtonLoad
	{
		internal Rectangle _rect;

		public Rectangle rect
		{
			get
			{
				return _rect;
			}
			set
			{
				_rect = value;
			}
		}

		public string key { get; set; }

		public int num { get; set; }

		public bool enabled { get; set; }

		public int prog { get; set; }

		public bool hover { get; set; }

		public ButtonLoad(Rectangle _rect, bool enable)
		{
			rect = _rect;
			key = "";
			enabled = enable;
		}

		public ButtonLoad(int _num, Rectangle _rect, bool enable)
		{
			num = _num;
			rect = _rect;
			key = _num.ToString();
			enabled = enable;
		}

		public ButtonLoad(int _num, Rectangle _rect, bool enable, int p)
		{
			num = _num;
			rect = _rect;
			key = _num.ToString();
			enabled = enable;
			prog = p;
		}
	}

	private int current = 1;

	private int total;

	private int pageSize = 10;

	private int _gap = 8;

	private bool showSizeChanger;

	private int[]? pageSizeOptions;

	private int sizeChangerWidth;

	private int pyr;

	private Color? fill;

	private int radius = 6;

	private float borderWidth = 1f;

	private RightToLeft rightToLeft;

	private string? textdesc;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private ButtonLoad[] buttons = new ButtonLoad[0];

	internal string? showTotal;

	internal Rectangle rect_text;

	private Input? input_SizeChanger;

	[Description("当前页数")]
	[Category("数据")]
	[DefaultValue(1)]
	public int Current
	{
		get
		{
			return current;
		}
		set
		{
			if (value < 1)
			{
				value = 1;
			}
			else if (value > PageTotal)
			{
				value = PageTotal;
			}
			if (current != value)
			{
				current = value;
				this.ValueChanged?.Invoke(this, new PagePageEventArgs(current, total, pageSize, PageTotal));
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("Current");
			}
		}
	}

	[Description("数据总数")]
	[Category("数据")]
	[DefaultValue(0)]
	public int Total
	{
		get
		{
			return total;
		}
		set
		{
			if (total != value)
			{
				total = value;
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("Total");
			}
		}
	}

	[Description("每页条数")]
	[Category("数据")]
	[DefaultValue(10)]
	public int PageSize
	{
		get
		{
			return pageSize;
		}
		set
		{
			if (pageSize != value)
			{
				pageSize = value;
				if (Math.Ceiling((double)total * 1.0 / (double)pageSize) < (double)current)
				{
					current = (int)Math.Ceiling((double)total * 1.0 / (double)pageSize);
				}
				this.ValueChanged?.Invoke(this, new PagePageEventArgs(current, total, pageSize, PageTotal));
				if (input_SizeChanger != null)
				{
					string text = Localization.Get("ItemsPerPage", "条/页");
					input_SizeChanger.Clear();
					input_SizeChanger.PlaceholderText = value + " " + text;
				}
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("PageSize");
			}
		}
	}

	[Description("最大显示总页数")]
	[Category("行为")]
	[DefaultValue(0)]
	public int MaxPageTotal { get; set; }

	[Description("总页数")]
	[Category("数据")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int PageTotal { get; private set; } = 1;


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
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("是否展示 PageSize 切换器")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ShowSizeChanger
	{
		get
		{
			return showSizeChanger;
		}
		set
		{
			if (showSizeChanger != value)
			{
				showSizeChanger = value;
				if (!value)
				{
					InputSizeChangerDispose();
				}
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("ShowSizeChanger");
			}
		}
	}

	[Description("指定每页可以显示多少条")]
	[Category("行为")]
	[DefaultValue(null)]
	public int[]? PageSizeOptions
	{
		get
		{
			return pageSizeOptions;
		}
		set
		{
			if (pageSizeOptions != value)
			{
				pageSizeOptions = value;
				InputSizeChangerDispose();
				if (showSizeChanger)
				{
					ButtonLayout();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("PageSizeOptions");
			}
		}
	}

	[Description("SizeChanger 宽度")]
	[Category("行为")]
	[DefaultValue(0)]
	public int SizeChangerWidth
	{
		get
		{
			return sizeChangerWidth;
		}
		set
		{
			if (sizeChangerWidth != value)
			{
				sizeChangerWidth = value;
				if (showSizeChanger)
				{
					InputSizeChangerDispose();
					ButtonLayout();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("SizeChangerWidth");
			}
		}
	}

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, 0, 0, pyr, 0, borderWidth / 2f * Config.Dpi);

	[Description("颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Fill
	{
		get
		{
			return fill;
		}
		set
		{
			fill = value;
			if (input_SizeChanger != null)
			{
				input_SizeChanger.BorderColor = value;
			}
			((Control)this).Invalidate();
			OnPropertyChanged("Fill");
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
				if (input_SizeChanger != null)
				{
					input_SizeChanger.Radius = value;
				}
				((Control)this).Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BorderWidth");
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
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("RightToLeft");
			}
		}
	}

	[Description("主动显示内容")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? TextDesc
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationTextDesc, textdesc);
		}
		set
		{
			if (!(textdesc == value))
			{
				textdesc = value;
				ButtonLayout();
				((Control)this).Invalidate();
				OnPropertyChanged("TextDesc");
			}
		}
	}

	[Description("主动显示内容")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTextDesc { get; set; }

	[Description("Value 属性值更改时发生")]
	[Category("行为")]
	public event PageValueEventHandler? ValueChanged;

	[Description("用于显示数据总量")]
	[Category("行为")]
	public event PageValueRtEventHandler? ShowTotalChanged;

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Expected O, but got Unknown
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Expected O, but got Unknown
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Expected O, but got Unknown
		if (buttons.Length < 2)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		float num = borderWidth * Config.Dpi;
		float num2 = (float)radius * Config.Dpi;
		if (base.Enabled)
		{
			Color color = Colour.Text.Get("Pagination");
			Color color2 = fill ?? Colour.Primary.Get("Pagination");
			SolidBrush val = new SolidBrush(Colour.FillSecondary.Get("Pagination"));
			try
			{
				ButtonLoad buttonLoad = buttons[0];
				if (buttonLoad.hover)
				{
					GraphicsPath val2 = buttonLoad.rect.RoundPath(num2);
					try
					{
						canvas.Fill((Brush)(object)val, val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				Pen val3 = new Pen(buttonLoad.enabled ? color : Colour.TextQuaternary.Get("Pagination"), num);
				try
				{
					canvas.DrawLines(val3, TAlignMini.Left.TriangleLines(buttonLoad.rect));
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				ButtonLoad buttonLoad2 = buttons[1];
				if (buttonLoad2.hover)
				{
					GraphicsPath val4 = buttonLoad2.rect.RoundPath(num2);
					try
					{
						canvas.Fill((Brush)(object)val, val4);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				Pen val5 = new Pen(buttonLoad2.enabled ? color : Colour.TextQuaternary.Get("Pagination"), num);
				try
				{
					canvas.DrawLines(val5, TAlignMini.Right.TriangleLines(buttonLoad2.rect));
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
				SolidBrush val6 = new SolidBrush(color);
				try
				{
					if (showTotal != null)
					{
						canvas.String(showTotal, ((Control)this).Font, (Brush)(object)val6, rect_text, s_f);
					}
					for (int i = 2; i < buttons.Length; i++)
					{
						ButtonLoad buttonLoad3 = buttons[i];
						if (buttonLoad3.hover)
						{
							GraphicsPath val7 = buttonLoad3.rect.RoundPath(num2);
							try
							{
								canvas.Fill((Brush)(object)val, val7);
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
							}
						}
						if (buttonLoad3.prog > 0)
						{
							SolidBrush val8 = new SolidBrush(Colour.TextQuaternary.Get("Pagination"));
							try
							{
								canvas.String("•••", ((Control)this).Font, (Brush)(object)val8, buttonLoad3.rect, s_f);
							}
							finally
							{
								((IDisposable)val8)?.Dispose();
							}
							continue;
						}
						if (current == buttonLoad3.num)
						{
							GraphicsPath val9 = buttonLoad3.rect.RoundPath(num2);
							try
							{
								canvas.Draw(color2, num, val9);
							}
							finally
							{
								((IDisposable)val9)?.Dispose();
							}
						}
						canvas.String(buttonLoad3.key, ((Control)this).Font, (Brush)(object)val6, buttonLoad3.rect, s_f);
					}
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			ButtonLoad buttonLoad4 = buttons[0];
			Pen val10 = new Pen(Colour.TextQuaternary.Get("Pagination"), num);
			try
			{
				canvas.DrawLines(val10, TAlignMini.Left.TriangleLines(buttonLoad4.rect));
			}
			finally
			{
				((IDisposable)val10)?.Dispose();
			}
			ButtonLoad buttonLoad5 = buttons[1];
			Pen val11 = new Pen(Colour.TextQuaternary.Get("Pagination"), num);
			try
			{
				canvas.DrawLines(val11, TAlignMini.Right.TriangleLines(buttonLoad5.rect));
			}
			finally
			{
				((IDisposable)val11)?.Dispose();
			}
			SolidBrush val12 = new SolidBrush(Colour.TextQuaternary.Get("Pagination"));
			try
			{
				if (showTotal != null)
				{
					canvas.String(showTotal, ((Control)this).Font, (Brush)(object)val12, rect_text, s_f);
				}
				for (int j = 2; j < buttons.Length; j++)
				{
					ButtonLoad buttonLoad6 = buttons[j];
					if (buttonLoad6.prog > 0)
					{
						canvas.String("•••", ((Control)this).Font, (Brush)(object)val12, buttonLoad6.rect, s_f);
						continue;
					}
					if (current == buttonLoad6.num)
					{
						GraphicsPath val13 = buttonLoad6.rect.RoundPath(num2);
						try
						{
							canvas.Fill(Colour.Fill.Get("Pagination"), val13);
						}
						finally
						{
							((IDisposable)val13)?.Dispose();
						}
					}
					canvas.String(buttonLoad6.key, ((Control)this).Font, (Brush)(object)val12, buttonLoad6.rect, s_f);
				}
			}
			finally
			{
				((IDisposable)val12)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (buttons.Length != 0)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < buttons.Length; i++)
			{
				ButtonLoad buttonLoad = buttons[i];
				bool flag = false;
				if (buttonLoad.enabled)
				{
					flag = buttonLoad.rect.Contains(e.Location);
				}
				else if ((i == 0 || i == 1) && buttonLoad.rect.Contains(e.Location))
				{
					num2++;
				}
				if (buttonLoad.hover != flag)
				{
					buttonLoad.hover = flag;
					num++;
				}
				if (buttonLoad.hover)
				{
					num3++;
				}
			}
			if (num2 > 0)
			{
				SetCursor(CursorType.No);
			}
			else
			{
				SetCursor(num3 > 0);
			}
			if (num > 0)
			{
				((Control)this).Invalidate();
			}
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		SetCursor(val: false);
		if (buttons.Length != 0)
		{
			int num = 0;
			ButtonLoad[] array = buttons;
			foreach (ButtonLoad buttonLoad in array)
			{
				if (buttonLoad.hover)
				{
					buttonLoad.hover = false;
					num++;
				}
			}
			if (num > 0)
			{
				((Control)this).Invalidate();
			}
		}
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (buttons.Length != 0)
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				ButtonLoad buttonLoad = buttons[i];
				if (!buttonLoad.enabled || !buttonLoad.rect.Contains(e.Location))
				{
					continue;
				}
				switch (i)
				{
				case 0:
					Current = current - 1;
					break;
				case 1:
					Current = current + 1;
					break;
				default:
					if (buttonLoad.prog > 0)
					{
						if (buttonLoad.prog == 2)
						{
							Current = current - 5;
						}
						else
						{
							Current = current + 5;
						}
					}
					else
					{
						Current = buttonLoad.num;
					}
					break;
				}
				((Control)this).Focus();
				return;
			}
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		ButtonLayout();
		((Control)this).OnSizeChanged(e);
	}

	private void InputSizeChangerDispose()
	{
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)new Action(InputSizeChangerDispose));
			return;
		}
		((Component)(object)input_SizeChanger)?.Dispose();
		input_SizeChanger = null;
	}

	private void ButtonLayout()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Rectangle rect = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, borderWidth / 2f * Config.Dpi);
		if (showSizeChanger)
		{
			int num = (int)(4f * Config.Dpi);
			rect.Y += num;
			rect.Height -= num * 2;
		}
		bool sizeChanger = ShowSizeChanger;
		int t_Width = rect.Width;
		int _SizeChangerWidth = 0;
		if (t_Width <= 1)
		{
			return;
		}
		if (sizeChanger)
		{
			_SizeChangerWidth = InitSizeChanger(rect);
			t_Width -= _SizeChangerWidth;
		}
		int gap = (int)((float)_gap * Config.Dpi);
		int total_button = t_Width / (rect.Height + gap);
		if (total_button < 3)
		{
			buttons = new ButtonLoad[0];
			return;
		}
		int total_page = (int)Math.Ceiling((double)total * 1.0 / (double)pageSize);
		if (total_page == 0)
		{
			total_page = 1;
		}
		if (TextDesc == null)
		{
			showTotal = this.ShowTotalChanged?.Invoke(this, new PagePageEventArgs(current, total, pageSize, total_page));
		}
		else
		{
			showTotal = TextDesc;
		}
		int num2 = Helper.GDI(delegate(Canvas g)
		{
			//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d8: Invalid comparison between Unknown and I4
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			int num3 = 100;
			int num4 = rect.Height;
			for (int i = 0; i <= total_page; i++)
			{
				if (i == num3)
				{
					num3 *= 10;
					Size size = g.MeasureString((i + 1).ToString(), ((Control)this).Font);
					if (size.Width > rect.Height)
					{
						num4 = size.Width;
						dictionary.Add(i.ToString().Length, size.Width);
					}
				}
			}
			int num5 = 0;
			if (showTotal != null)
			{
				num5 = g.MeasureString(showTotal, ((Control)this).Font).Width;
				rect_text = new Rectangle(rect.X, rect.Y, num5, rect.Height);
			}
			total_button = (int)Math.Floor((double)(t_Width - num5) / ((double)num4 * 1.0 + (double)gap));
			if (total_button < 3)
			{
				buttons = new ButtonLoad[0];
				return 0;
			}
			if (MaxPageTotal > 0 && total_button > MaxPageTotal + 2)
			{
				total_button = MaxPageTotal + 2;
			}
			int num6 = total_button - 2;
			PageTotal = total_page;
			bool enable = current > 1;
			bool enable2 = total_page > current;
			List<ButtonLoad> list = new List<ButtonLoad>(total_button)
			{
				new ButtonLoad(new Rectangle(num5 + rect.X, rect.Y, rect.Height, rect.Height), enable)
			};
			int num7 = list[0].rect.Right + gap;
			if (total_page > num6)
			{
				int num8 = num6;
				int num9 = (int)Math.Ceiling((float)num6 / 2f);
				if (current <= num9)
				{
					for (int j = 0; j < num8 - 2; j++)
					{
						num7 += AddButs(ref list, dictionary, new ButtonLoad(j + 1, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true)) + gap;
					}
					num7 += AddButs(ref list, dictionary, new ButtonLoad(num8 - 2, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true, 1)) + gap;
					num7 += AddButs(ref list, dictionary, new ButtonLoad(total_page, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true));
				}
				else if (current > total_page - num9)
				{
					num7 += AddButs(ref list, dictionary, new ButtonLoad(1, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true)) + gap;
					int num10 = total_page - (num8 - 3);
					num7 += AddButs(ref list, dictionary, new ButtonLoad(num10, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true, 2)) + gap;
					for (int k = 0; k < num8 - 2; k++)
					{
						num7 += AddButs(ref list, dictionary, new ButtonLoad(num10 + k, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true)) + gap;
					}
				}
				else
				{
					int num11 = total_page - (num8 - 3);
					num7 += AddButs(ref list, dictionary, new ButtonLoad(1, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true)) + gap;
					num7 += AddButs(ref list, dictionary, new ButtonLoad(1, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true, 2)) + gap;
					int num12 = num8 - 4;
					int num13 = current - num12 / 2;
					for (int l = 0; l < num12; l++)
					{
						num7 += AddButs(ref list, dictionary, new ButtonLoad(num13 + l, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true)) + gap;
					}
					num7 += AddButs(ref list, dictionary, new ButtonLoad(num11, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true, 1)) + gap;
					num7 += AddButs(ref list, dictionary, new ButtonLoad(total_page, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true));
				}
			}
			else
			{
				for (int m = 0; m < total_page; m++)
				{
					num7 += AddButs(ref list, dictionary, new ButtonLoad(m + 1, new Rectangle(num7, rect.Y, rect.Height, rect.Height), enable: true)) + gap;
				}
			}
			list.Insert(1, new ButtonLoad(new Rectangle(list[list.Count - 1].rect.Right + gap, rect.Y, rect.Height, rect.Height), enable2));
			int result = 0;
			if ((int)((Control)this).RightToLeft == 1)
			{
				int num14 = rect.Right - list[1].rect.Right;
				if (sizeChanger)
				{
					num14 -= _SizeChangerWidth;
				}
				foreach (ButtonLoad item in list)
				{
					item._rect.Offset(num14, 0);
				}
				rect_text.Offset(num14, 0);
			}
			else if (sizeChanger)
			{
				result = rect.Right - list[1].rect.Right - _SizeChangerWidth;
			}
			buttons = list.ToArray();
			return result;
		});
		if (pyr != num2)
		{
			pyr = num2;
			IOnSizeChanged();
		}
	}

	private int InitSizeChanger(Rectangle rect)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		if (input_SizeChanger == null)
		{
			string text = Localization.Get("ItemsPerPage", "条/页");
			string text2 = pageSize + " " + text;
			_ = rightToLeft;
			int num = GetSizeChangerWidth(text2);
			if (pageSizeOptions == null || pageSizeOptions.Length == 0)
			{
				Input obj = new Input
				{
					Radius = radius,
					PlaceholderText = text2
				};
				((Control)obj).Size = new Size(num, rect.Height);
				((Control)obj).Dock = (DockStyle)4;
				((Control)obj).Font = ((Control)this).Font;
				obj.BorderColor = fill;
				Input input = obj;
				input_SizeChanger = input;
			}
			else
			{
				Select obj2 = new Select
				{
					Radius = radius,
					PlaceholderText = text2,
					ListAutoWidth = true,
					DropDownArrow = true,
					Placement = TAlignFrom.Top
				};
				((Control)obj2).Size = new Size(num, rect.Height);
				((Control)obj2).Dock = (DockStyle)4;
				((Control)obj2).Font = ((Control)this).Font;
				obj2.BorderColor = fill;
				Select select = obj2;
				int[] array = pageSizeOptions;
				foreach (int num2 in array)
				{
					select.Items.Add(num2);
				}
				select.SelectedValue = pageSize;
				((Control)select).Text = "";
				select.SelectedValueChanged += delegate(object a, ObjectNEventArgs b)
				{
					if (b.Value is int num3)
					{
						PageSize = num3;
					}
				};
				input_SizeChanger = select;
			}
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Control)this).Controls.Add((Control)(object)input_SizeChanger);
				});
			}
			else
			{
				((Control)this).Controls.Add((Control)(object)input_SizeChanger);
			}
			((Control)input_SizeChanger).KeyPress += new KeyPressEventHandler(Input_SizeChanger_KeyPress);
			return num;
		}
		if (sizeChangerWidth <= 0)
		{
			string text3 = Localization.Get("ItemsPerPage", "条/页");
			string placeholder = pageSize + " " + text3;
			int width = GetSizeChangerWidth(placeholder);
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Control)input_SizeChanger).Width = width;
				});
			}
			else
			{
				((Control)input_SizeChanger).Width = width;
			}
			return width;
		}
		return ((Control)input_SizeChanger).Width;
	}

	private int GetSizeChangerWidth(string placeholder)
	{
		string placeholder2 = placeholder;
		if (sizeChangerWidth > 0)
		{
			return (int)((float)sizeChangerWidth * Config.Dpi);
		}
		int wsize = (int)(4f * Config.Dpi) * 2;
		if (pageSizeOptions == null || pageSizeOptions.Length == 0)
		{
			return Helper.GDI(delegate(Canvas g)
			{
				Size size2 = g.MeasureString(placeholder2, ((Control)this).Font);
				return size2.Width + wsize + (int)Math.Ceiling((float)size2.Height * 0.6f);
			});
		}
		return Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString(placeholder2, ((Control)this).Font);
			return size.Width + wsize + (int)Math.Ceiling((float)size.Height * 1.32f);
		});
	}

	protected override void OnFontChanged(EventArgs e)
	{
		((Control)this).OnForeColorChanged(e);
		if (input_SizeChanger != null)
		{
			((Control)input_SizeChanger).Font = ((Control)this).Font;
		}
	}

	private int AddButs(ref List<ButtonLoad> buttons, Dictionary<int, int> dir, ButtonLoad button)
	{
		if (button.key.Length > 1 && dir.TryGetValue(button.key.Length, out var value))
		{
			button._rect.Width = value;
		}
		buttons.Add(button);
		return button.rect.Width;
	}

	private void Input_SizeChanger_KeyPress(object? sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r' && sender is Input input)
		{
			e.Handled = true;
			if (int.TryParse(((Control)input).Text, out var result))
			{
				PageSize = result;
			}
		}
	}

	public bool ProcessCmdKey(Keys keyData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if ((int)keyData == 37)
		{
			int num = current - 1;
			if (num < 1 || num > PageTotal)
			{
				return false;
			}
			Current = num;
			return true;
		}
		if ((int)keyData == 39)
		{
			int num2 = current + 1;
			if (num2 < 1 || num2 > PageTotal)
			{
				return false;
			}
			Current = num2;
			return true;
		}
		return false;
	}

	public void InitData(int Current = 1, int PageSize = 10)
	{
		current = Current;
		pageSize = PageSize;
	}
}
