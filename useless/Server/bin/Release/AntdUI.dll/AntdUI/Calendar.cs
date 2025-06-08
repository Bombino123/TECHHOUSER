using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using ChineseCalendar;

namespace AntdUI;

[Description("Calendar 日历")]
[ToolboxItem(true)]
[DefaultProperty("Date")]
[DefaultEvent("DateChanged")]
public class Calendar : IControl
{
	private int radius = 6;

	private bool full;

	private bool chinese;

	private bool showButtonToDay = true;

	private Dictionary<string, DateBadge> badge_list = new Dictionary<string, DateBadge>();

	public Func<DateTime[], List<DateBadge>?>? BadgeAction;

	private List<Calendari>? calendar_year;

	private List<Calendari>? calendar_month;

	private List<Calendari>? calendar_day;

	private DateTime _value = DateTime.Now;

	private DateTime? minDate;

	private DateTime? maxDate;

	private DateTime _Date;

	private DateTime DateNow = DateTime.Now;

	private string year_str = "";

	private CultureInfo Culture;

	private string CultureID = Localization.Get("ID", "zh-CN");

	private string button_text = Localization.Get("ToDay", "今天");

	private string YearFormat;

	private string MonthFormat;

	private string MondayButton;

	private string TuesdayButton;

	private string WednesdayButton;

	private string ThursdayButton;

	private string FridayButton;

	private string SaturdayButton;

	private string SundayButton;

	private bool YDR;

	private StringFormat s_f = Helper.SF((StringAlignment)1, (StringAlignment)1);

	private StringFormat s_f_L;

	private StringFormat s_f_R;

	private Rectangle rect_year_l;

	private Rectangle rect_month_l;

	private RectangleF rect_day_split1;

	private RectangleF rect_day_split2;

	private Rectangle[]? rect_day_s;

	private ITaskOpacity hover_button;

	private ITaskOpacity hover_lefts;

	private ITaskOpacity hover_left;

	private ITaskOpacity hover_rights;

	private ITaskOpacity hover_right;

	private ITaskOpacity hover_year;

	private ITaskOpacity hover_month;

	private Rectangle rect_button = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_lefts = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_left = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_rights = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_right = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year2 = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_month = new Rectangle(-20, -20, 10, 10);

	private int showType;

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

	[Description("是否撑满")]
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
				IOnSizeChanged();
				((Control)this).Invalidate();
				OnPropertyChanged("Full");
			}
		}
	}

	[Description("显示农历")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ShowChinese
	{
		get
		{
			return chinese;
		}
		set
		{
			if (chinese != value)
			{
				chinese = value;
				IOnSizeChanged();
				((Control)this).Invalidate();
				OnPropertyChanged("ShowChinese");
			}
		}
	}

	[Description("显示今天")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool ShowButtonToDay
	{
		get
		{
			return showButtonToDay;
		}
		set
		{
			if (showButtonToDay != value)
			{
				showButtonToDay = value;
				IOnSizeChanged();
				((Control)this).Invalidate();
				OnPropertyChanged("ShowButtonToDay");
			}
		}
	}

	[Description("控件当前日期")]
	[Category("数据")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public DateTime Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (!(_value == value))
			{
				_value = value;
				this.DateChanged?.Invoke(this, new DateTimeEventArgs(_value));
				((Control)this).Invalidate();
				LoadBadge();
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("最小日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? MinDate
	{
		get
		{
			return minDate;
		}
		set
		{
			if (!(minDate == value))
			{
				minDate = value;
				Date = _Date;
				((Control)this).Invalidate();
				OnPropertyChanged("MinDate");
			}
		}
	}

	[Description("最大日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? MaxDate
	{
		get
		{
			return maxDate;
		}
		set
		{
			if (!(maxDate == value))
			{
				maxDate = value;
				Date = _Date;
				((Control)this).Invalidate();
				OnPropertyChanged("MaxDate");
			}
		}
	}

	private DateTime Date
	{
		get
		{
			return _Date;
		}
		set
		{
			_Date = value;
			calendar_day = GetCalendar(value);
			List<Calendari> list = new List<Calendari>(12);
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < 12; i++)
			{
				DateTime date = new DateTime(value.Year, i + 1, 1);
				list.Add(new Calendari(0, num, num2, date.ToString(MonthFormat, Culture), date, date.ToString("yyyy-MM"), minDate, maxDate));
				num++;
				if (num > 2)
				{
					num2++;
					num = 0;
				}
			}
			calendar_month = list;
			int num3 = value.Year - 1;
			if (!value.Year.ToString().EndsWith("0"))
			{
				string text = value.Year.ToString();
				num3 = int.Parse(text.Substring(0, text.Length - 1) + "0") - 1;
			}
			List<Calendari> list2 = new List<Calendari>(12);
			int num4 = 0;
			int num5 = 0;
			if (num3 < 1)
			{
				num3 = 1;
			}
			for (int j = 0; j < 12; j++)
			{
				DateTime date2 = new DateTime(num3 + j, value.Month, 1);
				list2.Add(new Calendari((j != 0) ? 1 : 0, num4, num5, date2.ToString("yyyy"), date2, date2.ToString("yyyy"), minDate, maxDate));
				num4++;
				if (num4 > 2)
				{
					num5++;
					num4 = 0;
				}
			}
			year_str = list2[1].date_str + "-" + list2[list2.Count - 2].date_str;
			calendar_year = list2;
			IOnSizeChanged();
			LoadBadge();
			hover_left.Enable = Helper.DateExceedMonth(value.AddMonths(-1), minDate, maxDate);
			hover_right.Enable = Helper.DateExceedMonth(value.AddMonths(1), minDate, maxDate);
			hover_lefts.Enable = Helper.DateExceedYear(value.AddYears(-1), minDate, maxDate);
			hover_rights.Enable = Helper.DateExceedYear(value.AddYears(1), minDate, maxDate);
		}
	}

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);

	public override GraphicsPath RenderRegion => ReadRectangle.RoundPath((float)radius * Config.Dpi);

	[Description("日期 改变时发生")]
	[Category("行为")]
	public event DateTimeEventHandler? DateChanged;

	public Calendar()
	{
		hover_lefts = new ITaskOpacity((IControl)this);
		hover_left = new ITaskOpacity((IControl)this);
		hover_rights = new ITaskOpacity((IControl)this);
		hover_right = new ITaskOpacity((IControl)this);
		hover_year = new ITaskOpacity((IControl)this);
		hover_month = new ITaskOpacity((IControl)this);
		hover_button = new ITaskOpacity((IControl)this);
		Date = DateNow;
		Culture = new CultureInfo(CultureID);
		YDR = CultureID.StartsWith("en");
		if (YDR)
		{
			YearFormat = "yyyy";
			MonthFormat = "MMM";
			MondayButton = "Mon";
			TuesdayButton = "Tue";
			WednesdayButton = "Wed";
			ThursdayButton = "Thu";
			FridayButton = "Fri";
			SaturdayButton = "Sat";
			SundayButton = "Sun";
			s_f_L = Helper.SF((StringAlignment)1, (StringAlignment)0);
			s_f_R = Helper.SF((StringAlignment)1, (StringAlignment)2);
		}
		else
		{
			YearFormat = "yyyy年";
			MonthFormat = "MM月";
			MondayButton = "一";
			TuesdayButton = "二";
			WednesdayButton = "三";
			ThursdayButton = "四";
			FridayButton = "五";
			SaturdayButton = "六";
			SundayButton = "日";
			s_f_L = Helper.SF((StringAlignment)1, (StringAlignment)2);
			s_f_R = Helper.SF((StringAlignment)1, (StringAlignment)0);
		}
	}

	private List<Calendari> GetCalendar(DateTime now)
	{
		List<Calendari> list = new List<Calendari>(28);
		int num = DateTime.DaysInMonth(now.Year, now.Month);
		DateTime dateTime = new DateTime(now.Year, now.Month, 1);
		int num2 = 0;
		switch (dateTime.DayOfWeek)
		{
		case DayOfWeek.Tuesday:
			num2 = 1;
			break;
		case DayOfWeek.Wednesday:
			num2 = 2;
			break;
		case DayOfWeek.Thursday:
			num2 = 3;
			break;
		case DayOfWeek.Friday:
			num2 = 4;
			break;
		case DayOfWeek.Saturday:
			num2 = 5;
			break;
		case DayOfWeek.Sunday:
			num2 = 6;
			break;
		}
		if (num2 > 0)
		{
			DateTime dateTime2 = now.AddMonths(-1);
			int num3 = DateTime.DaysInMonth(dateTime2.Year, dateTime2.Month);
			for (int i = 0; i < num2; i++)
			{
				int day = num3 - i;
				list.Insert(0, new Calendari(0, num2 - 1 - i, 0, day.ToString(), new DateTime(dateTime2.Year, dateTime2.Month, day), minDate, maxDate));
			}
		}
		int num4 = num2;
		int num5 = 0;
		for (int j = 0; j < num; j++)
		{
			int day2 = j + 1;
			list.Add(new Calendari(1, num4, num5, day2.ToString(), new DateTime(now.Year, now.Month, day2), minDate, maxDate));
			num4++;
			if (num4 > 6)
			{
				num5++;
				num4 = 0;
			}
		}
		if (num4 < 7)
		{
			DateTime dateTime3 = now.AddMonths(1);
			int num6 = 0;
			for (int k = num4; k < 7; k++)
			{
				int day3 = num6 + 1;
				list.Add(new Calendari(2, num4, num5, day3.ToString(), new DateTime(dateTime3.Year, dateTime3.Month, day3), minDate, maxDate));
				num4++;
				num6++;
			}
			if (num5 < 5)
			{
				num5++;
				for (int l = 0; l < 7; l++)
				{
					int day4 = num6 + 1;
					list.Add(new Calendari(2, l, num5, day4.ToString(), new DateTime(dateTime3.Year, dateTime3.Month, day4), minDate, maxDate));
					num6++;
				}
			}
		}
		return list;
	}

	public void LoadBadge()
	{
		if (BadgeAction == null || calendar_day == null)
		{
			return;
		}
		DateTime oldval = _Date;
		ITask.Run(delegate
		{
			List<DateBadge> list = BadgeAction(new DateTime[2]
			{
				calendar_day[0].date,
				calendar_day[calendar_day.Count - 1].date
			});
			if (_Date == oldval)
			{
				badge_list.Clear();
				if (list == null)
				{
					((Control)this).Invalidate();
				}
				else
				{
					foreach (DateBadge item in list)
					{
						badge_list.Add(item.Date, item);
					}
					((Control)this).Invalidate();
				}
			}
		});
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Expected O, but got Unknown
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Expected O, but got Unknown
		//IL_075c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Expected O, but got Unknown
		Canvas canvas = e.Graphics.High();
		_ = ((Control)this).ClientRectangle;
		Rectangle readRectangle = ReadRectangle;
		float num = (float)radius * Config.Dpi;
		GraphicsPath val = readRectangle.RoundPath(num);
		try
		{
			canvas.Fill(Colour.BgElevated.Get("Calendar"), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		Pen val2 = new Pen(Colour.TextTertiary.Get("Calendar"), 1.6f * Config.Dpi);
		try
		{
			Pen val3 = new Pen(Colour.Text.Get("Calendar"), val2.Width);
			try
			{
				Pen val4 = new Pen(Colour.FillSecondary.Get("Calendar"), val2.Width);
				try
				{
					if (hover_lefts.Animation)
					{
						Pen val5 = new Pen(val2.Color.BlendColors(hover_lefts.Value, val3.Color), val3.Width);
						try
						{
							canvas.DrawLines(val5, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
							canvas.DrawLines(val5, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					else if (hover_lefts.Switch)
					{
						canvas.DrawLines(val3, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						canvas.DrawLines(val3, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
					}
					else if (hover_lefts.Enable)
					{
						canvas.DrawLines(val2, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						canvas.DrawLines(val2, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
					}
					else
					{
						canvas.DrawLines(val4, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						canvas.DrawLines(val4, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
					}
					if (hover_rights.Animation)
					{
						Pen val6 = new Pen(val2.Color.BlendColors(hover_rights.Value, val3.Color), val3.Width);
						try
						{
							canvas.DrawLines(val6, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
							canvas.DrawLines(val6, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					else if (hover_rights.Switch)
					{
						canvas.DrawLines(val3, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						canvas.DrawLines(val3, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
					}
					else if (hover_rights.Enable)
					{
						canvas.DrawLines(val2, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						canvas.DrawLines(val2, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
					}
					else
					{
						canvas.DrawLines(val4, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						canvas.DrawLines(val4, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
					}
					if (showType == 0)
					{
						if (hover_left.Animation)
						{
							Pen val7 = new Pen(val2.Color.BlendColors(hover_left.Value, val3.Color), val3.Width);
							try
							{
								canvas.DrawLines(val7, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
							}
						}
						else if (hover_left.Switch)
						{
							canvas.DrawLines(val3, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
						}
						else if (hover_left.Enable)
						{
							canvas.DrawLines(val2, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
						}
						else
						{
							canvas.DrawLines(val4, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
						}
						if (hover_right.Animation)
						{
							Pen val8 = new Pen(val2.Color.BlendColors(hover_right.Value, val3.Color), val3.Width);
							try
							{
								canvas.DrawLines(val8, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
							}
							finally
							{
								((IDisposable)val8)?.Dispose();
							}
						}
						else if (hover_right.Switch)
						{
							canvas.DrawLines(val3, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
						}
						else if (hover_right.Enable)
						{
							canvas.DrawLines(val2, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
						}
						else
						{
							canvas.DrawLines(val4, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
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
		if (showType == 1 && calendar_month != null)
		{
			PrintMonth(canvas, readRectangle, num, calendar_month);
		}
		else if (showType == 2 && calendar_year != null)
		{
			PrintYear(canvas, readRectangle, num, calendar_year);
		}
		else if (calendar_day != null)
		{
			PrintDay(canvas, readRectangle, num, calendar_day);
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PrintYear(Canvas g, Rectangle rect_read, float radius, List<Calendari> datas)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		Color color = Colour.TextBase.Get("Calendar");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			if (hover_year.Animation)
			{
				g.String(year_str, val, color.BlendColors(hover_year.Value, Colour.Primary.Get("Calendar")), rect_year_l, s_f);
			}
			else if (hover_year.Switch)
			{
				g.String(year_str, val, Colour.Primary.Get("Calendar"), rect_year_l, s_f);
			}
			else
			{
				g.String(year_str, val, color, rect_year_l, s_f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SolidBrush val2 = new SolidBrush(Colour.TextQuaternary.Get("Calendar"));
		try
		{
			SolidBrush val3 = new SolidBrush(Colour.FillTertiary.Get("Calendar"));
			try
			{
				SolidBrush val4 = new SolidBrush(color);
				try
				{
					foreach (Calendari data in datas)
					{
						GraphicsPath val5 = data.rect_read.RoundPath(radius);
						try
						{
							if (_value.ToString("yyyy") == data.date_str)
							{
								g.Fill(Colour.Primary.Get("Calendar"), val5);
								g.String(data.v, ((Control)this).Font, Colour.PrimaryColor.Get("Calendar"), data.rect, s_f);
							}
							else if (data.enable)
							{
								if (data.hover)
								{
									g.Fill(Colour.FillTertiary.Get("Calendar"), val5);
								}
								if (DateNow.ToString("yyyy-MM-dd") == data.date_str)
								{
									g.Draw(Colour.Primary.Get("Calendar"), Config.Dpi, val5);
								}
								g.String(data.v, ((Control)this).Font, (Brush)(object)((data.t == 1) ? val4 : val2), data.rect, s_f);
							}
							else
							{
								g.Fill((Brush)(object)val3, new Rectangle(data.rect.X, data.rect_read.Y, data.rect.Width, data.rect_read.Height));
								if (DateNow.ToString("yyyy-MM-dd") == data.date_str)
								{
									g.Draw(Colour.Primary.Get("Calendar"), Config.Dpi, val5);
								}
								g.String(data.v, ((Control)this).Font, (Brush)(object)val2, data.rect, s_f);
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
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
	}

	private void PrintMonth(Canvas g, Rectangle rect_read, float radius, List<Calendari> datas)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		Color color = Colour.TextBase.Get("Calendar");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			string text = _Date.ToString(YearFormat, Culture);
			if (hover_year.Animation)
			{
				g.String(text, val, color.BlendColors(hover_year.Value, Colour.Primary.Get("Calendar")), rect_month_l, s_f);
			}
			else if (hover_year.Switch)
			{
				g.String(text, val, Colour.Primary.Get("Calendar"), rect_month_l, s_f);
			}
			else
			{
				g.String(text, val, color, rect_month_l, s_f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SolidBrush val2 = new SolidBrush(Colour.TextQuaternary.Get("Calendar"));
		try
		{
			SolidBrush val3 = new SolidBrush(Colour.FillTertiary.Get("Calendar"));
			try
			{
				SolidBrush val4 = new SolidBrush(color);
				try
				{
					foreach (Calendari data in datas)
					{
						GraphicsPath val5 = data.rect_read.RoundPath(radius);
						try
						{
							if (_value.ToString("yyyy-MM") == data.date_str)
							{
								g.Fill(Colour.Primary.Get("Calendar"), val5);
								g.String(data.v, ((Control)this).Font, Colour.PrimaryColor.Get("Calendar"), data.rect, s_f);
							}
							else if (data.enable)
							{
								if (data.hover)
								{
									g.Fill(Colour.FillTertiary.Get("Calendar"), val5);
								}
								if (DateNow.ToString("yyyy-MM-dd") == data.date_str)
								{
									g.Draw(Colour.Primary.Get("Calendar"), Config.Dpi, val5);
								}
								g.String(data.v, ((Control)this).Font, (Brush)(object)val4, data.rect, s_f);
							}
							else
							{
								g.Fill((Brush)(object)val3, new Rectangle(data.rect.X, data.rect_read.Y, data.rect.Width, data.rect_read.Height));
								if (DateNow.ToString("yyyy-MM-dd") == data.date_str)
								{
									g.Draw(Colour.Primary.Get("Calendar"), Config.Dpi, val5);
								}
								g.String(data.v, ((Control)this).Font, (Brush)(object)val2, data.rect, s_f);
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
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
	}

	private void PrintDay(Canvas g, Rectangle rect_read, float radius, List<Calendari> datas)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Expected O, but got Unknown
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Expected O, but got Unknown
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Expected O, but got Unknown
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Expected O, but got Unknown
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Expected O, but got Unknown
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Expected O, but got Unknown
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Expected O, but got Unknown
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Expected O, but got Unknown
		if (rect_day_s == null)
		{
			return;
		}
		Color color = Colour.TextBase.Get("Calendar");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			string text = _Date.ToString(YearFormat, Culture);
			string text2 = _Date.ToString(MonthFormat, Culture);
			if (hover_year.Animation)
			{
				g.String(text, val, color.BlendColors(hover_year.Value, Colour.Primary.Get("Calendar")), rect_year, s_f_L);
			}
			else if (hover_year.Switch)
			{
				g.String(text, val, Colour.Primary.Get("Calendar"), rect_year, s_f_L);
			}
			else
			{
				g.String(text, val, color, rect_year, s_f_L);
			}
			if (hover_month.Animation)
			{
				g.String(text2, val, color.BlendColors(hover_month.Value, Colour.Primary.Get("Calendar")), rect_month, s_f_R);
			}
			else if (hover_month.Switch)
			{
				g.String(text2, val, Colour.Primary.Get("Calendar"), rect_month, s_f_R);
			}
			else
			{
				g.String(text2, val, color, rect_month, s_f_R);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SolidBrush val2 = new SolidBrush(Colour.Split.Get("Calendar"));
		try
		{
			g.Fill((Brush)(object)val2, rect_day_split1);
			if (showButtonToDay)
			{
				g.Fill((Brush)(object)val2, rect_day_split2);
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		SolidBrush val3 = new SolidBrush(Colour.Text.Get("Calendar"));
		try
		{
			g.String(MondayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[0], s_f);
			g.String(TuesdayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[1], s_f);
			g.String(WednesdayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[2], s_f);
			g.String(ThursdayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[3], s_f);
			g.String(FridayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[4], s_f);
			g.String(SaturdayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[5], s_f);
			g.String(SundayButton, ((Control)this).Font, (Brush)(object)val3, rect_day_s[6], s_f);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		SolidBrush val4 = new SolidBrush(color);
		try
		{
			SolidBrush val5 = new SolidBrush(Colour.TextQuaternary.Get("Calendar"));
			try
			{
				SolidBrush val6 = new SolidBrush(Colour.FillTertiary.Get("Calendar"));
				try
				{
					SolidBrush val7 = new SolidBrush(Colour.Primary.Get("Calendar"));
					try
					{
						SolidBrush val8 = new SolidBrush(Colour.PrimaryColor.Get("Calendar"));
						try
						{
							SolidBrush val9 = new SolidBrush(Colour.Error.Get("Calendar"));
							try
							{
								PaintToDayFrame(g, datas, DateNow.ToString("yyyy-MM-dd"), radius);
								if (chinese)
								{
									Font val10 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.76f, ((Control)this).Font.Style);
									try
									{
										SolidBrush val11 = new SolidBrush(Colour.TextSecondary.Get("Calendar"));
										try
										{
											foreach (Calendari data in datas)
											{
												GraphicsPath val12 = data.rect_read.RoundPath(radius);
												try
												{
													ChineseDate chineseDate = ChineseDate.From(data.date);
													if (_value.ToString("yyyy-MM-dd") == data.date_str)
													{
														g.Fill((Brush)(object)val7, val12);
														g.String(chineseDate.DayString, val10, (Brush)(object)val8, data.rect_l, s_f);
														g.String(data.v, ((Control)this).Font, (Brush)(object)val8, data.rect_f, s_f);
													}
													else if (data.enable)
													{
														if (data.hover)
														{
															g.Fill(Colour.FillTertiary.Get("Calendar"), val12);
														}
														g.String(chineseDate.DayString, val10, (Brush)(object)val11, data.rect_l, s_f);
														g.String(data.v, ((Control)this).Font, (Brush)(object)((data.t == 1) ? val4 : val5), data.rect_f, s_f);
													}
													else
													{
														g.Fill((Brush)(object)val6, new Rectangle(data.rect.X, data.rect_read.Y, data.rect.Width, data.rect_read.Height));
														g.String(chineseDate.DayString, val10, (Brush)(object)val5, data.rect_l, s_f);
														g.String(data.v, ((Control)this).Font, (Brush)(object)val5, data.rect_f, s_f);
													}
												}
												finally
												{
													((IDisposable)val12)?.Dispose();
												}
											}
										}
										finally
										{
											((IDisposable)val11)?.Dispose();
										}
									}
									finally
									{
										((IDisposable)val10)?.Dispose();
									}
								}
								else
								{
									foreach (Calendari data2 in datas)
									{
										GraphicsPath val13 = data2.rect_read.RoundPath(radius);
										try
										{
											if (_value.ToString("yyyy-MM-dd") == data2.date_str)
											{
												g.Fill((Brush)(object)val7, val13);
												g.String(data2.v, ((Control)this).Font, (Brush)(object)val8, data2.rect, s_f);
											}
											else if (data2.enable)
											{
												if (data2.hover)
												{
													g.Fill(Colour.FillTertiary.Get("Calendar"), val13);
												}
												g.String(data2.v, ((Control)this).Font, (Brush)(object)((data2.t == 1) ? val4 : val5), data2.rect, s_f);
											}
											else
											{
												g.Fill((Brush)(object)val6, new Rectangle(data2.rect.X, data2.rect_read.Y, data2.rect.Width, data2.rect_read.Height));
												g.String(data2.v, ((Control)this).Font, (Brush)(object)val5, data2.rect, s_f);
											}
										}
										finally
										{
											((IDisposable)val13)?.Dispose();
										}
									}
								}
								if (showButtonToDay)
								{
									if (hover_button.Animation)
									{
										g.String(button_text, ((Control)this).Font, val7.Color.BlendColors(hover_button.Value, Colour.PrimaryActive.Get("Calendar")), rect_button, s_f);
									}
									else if (hover_button.Switch)
									{
										g.String(button_text, ((Control)this).Font, Colour.PrimaryActive.Get("Calendar"), rect_button, s_f);
									}
									else
									{
										g.String(button_text, ((Control)this).Font, (Brush)(object)val7, rect_button, s_f);
									}
								}
								if (badge_list.Count <= 0)
								{
									return;
								}
								foreach (Calendari data3 in datas)
								{
									if (badge_list.TryGetValue(data3.date_str, out DateBadge value))
									{
										this.PaintBadge(value, data3.rect, g);
									}
								}
							}
							finally
							{
								((IDisposable)val9)?.Dispose();
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
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
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

	internal static void PaintToDayFrame(Canvas g, IList<Calendari> datas, string dateNow, float radius)
	{
		foreach (Calendari data in datas)
		{
			if (dateNow == data.date_str)
			{
				GraphicsPath val = data.rect_read.RoundPath(radius);
				try
				{
					g.Draw(Colour.Primary.Get("Calendar"), Config.Dpi, val);
					break;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int num = 0;
		int num2 = 0;
		bool flag = rect_lefts.Contains(e.X, e.Y);
		bool flag2 = rect_rights.Contains(e.X, e.Y);
		bool flag3 = showType == 0 && rect_left.Contains(e.X, e.Y);
		bool flag4 = showType == 0 && rect_right.Contains(e.X, e.Y);
		bool flag5 = showType == 0 && showButtonToDay && rect_button.Contains(e.X, e.Y);
		bool flag6 = false;
		bool flag7 = false;
		if (showType != 2)
		{
			flag6 = ((showType == 0) ? rect_year.Contains(e.X, e.Y) : rect_year2.Contains(e.X, e.Y));
			flag7 = rect_month.Contains(e.X, e.Y);
		}
		if (flag != hover_lefts.Switch)
		{
			num++;
		}
		if (flag3 != hover_left.Switch)
		{
			num++;
		}
		if (flag2 != hover_rights.Switch)
		{
			num++;
		}
		if (flag4 != hover_right.Switch)
		{
			num++;
		}
		if (flag6 != hover_year.Switch)
		{
			num++;
		}
		if (flag7 != hover_month.Switch)
		{
			num++;
		}
		if (flag5 != hover_button.Switch)
		{
			num++;
		}
		hover_lefts.Switch = flag;
		hover_left.Switch = flag3;
		hover_rights.Switch = flag2;
		hover_right.Switch = flag4;
		hover_year.Switch = flag6;
		hover_month.Switch = flag7;
		hover_button.Switch = flag5;
		if (hover_lefts.Switch || hover_left.Switch || hover_rights.Switch || hover_right.Switch || hover_year.Switch || hover_month.Switch || hover_button.Switch)
		{
			num2++;
		}
		else if (showType == 1)
		{
			if (calendar_month != null)
			{
				foreach (Calendari item in calendar_month)
				{
					bool flag8 = item.enable && item.rect.Contains(e.X, e.Y);
					if (item.hover != flag8)
					{
						num++;
					}
					item.hover = flag8;
					if (item.hover)
					{
						num2++;
					}
				}
			}
		}
		else if (showType == 2)
		{
			if (calendar_year != null)
			{
				foreach (Calendari item2 in calendar_year)
				{
					bool flag9 = item2.enable && item2.rect.Contains(e.X, e.Y);
					if (item2.hover != flag9)
					{
						num++;
					}
					item2.hover = flag9;
					if (item2.hover)
					{
						num2++;
					}
				}
			}
		}
		else if (calendar_day != null)
		{
			foreach (Calendari item3 in calendar_day)
			{
				bool flag10 = item3.enable && item3.rect.Contains(e.X, e.Y);
				if (item3.hover != flag10)
				{
					num++;
				}
				item3.hover = flag10;
				if (item3.hover)
				{
					num2++;
				}
			}
		}
		if (num > 0)
		{
			((Control)this).Invalidate();
		}
		SetCursor(num2 > 0);
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hover_lefts.Switch = false;
		hover_left.Switch = false;
		hover_rights.Switch = false;
		hover_right.Switch = false;
		hover_year.Switch = false;
		hover_month.Switch = false;
		hover_button.Switch = false;
		if (calendar_year != null)
		{
			foreach (Calendari item in calendar_year)
			{
				item.hover = false;
			}
		}
		if (calendar_month != null)
		{
			foreach (Calendari item2 in calendar_month)
			{
				item2.hover = false;
			}
		}
		if (calendar_day != null)
		{
			foreach (Calendari item3 in calendar_day)
			{
				item3.hover = false;
			}
		}
		SetCursor(val: false);
		((Control)this).Invalidate();
		((Control)this).OnMouseLeave(e);
	}

	private void ChangeType(int type)
	{
		if (type != showType)
		{
			showType = type;
			IOnSizeChanged();
			((Control)this).Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			if (rect_lefts.Contains(e.X, e.Y))
			{
				if (hover_lefts.Enable)
				{
					if (showType == 2)
					{
						Date = _Date.AddYears(-10);
					}
					else
					{
						Date = _Date.AddYears(-1);
					}
					((Control)this).Invalidate();
				}
				return;
			}
			if (rect_rights.Contains(e.X, e.Y))
			{
				if (hover_rights.Enable)
				{
					if (showType == 2)
					{
						Date = _Date.AddYears(10);
					}
					else
					{
						Date = _Date.AddYears(1);
					}
					((Control)this).Invalidate();
				}
				return;
			}
			if (showType == 0 && rect_left.Contains(e.X, e.Y))
			{
				if (hover_left.Enable)
				{
					Date = _Date.AddMonths(-1);
					((Control)this).Invalidate();
				}
				return;
			}
			if (showType == 0 && rect_right.Contains(e.X, e.Y))
			{
				if (hover_right.Enable)
				{
					Date = _Date.AddMonths(1);
					((Control)this).Invalidate();
				}
				return;
			}
			if ((showType == 0 && rect_year.Contains(e.X, e.Y)) || (showType != 0 && rect_year2.Contains(e.X, e.Y)))
			{
				ChangeType(2);
				return;
			}
			if (showType == 0 && showButtonToDay && rect_button.Contains(e.X, e.Y))
			{
				DateTime value = (Date = (DateNow = DateTime.Now));
				Value = value;
				return;
			}
			if (rect_month.Contains(e.X, e.Y))
			{
				ChangeType(1);
				return;
			}
			if (showType == 1)
			{
				if (calendar_month != null)
				{
					foreach (Calendari item in calendar_month)
					{
						if (item.enable && item.rect.Contains(e.X, e.Y))
						{
							Date = item.date;
							ChangeType(0);
							return;
						}
					}
				}
			}
			else if (showType == 2)
			{
				if (calendar_year != null)
				{
					foreach (Calendari item2 in calendar_year)
					{
						if (item2.enable && item2.rect.Contains(e.X, e.Y))
						{
							Date = item2.date;
							ChangeType(1);
							return;
						}
					}
				}
			}
			else if (calendar_day != null)
			{
				foreach (Calendari item3 in calendar_day)
				{
					if (item3.enable && item3.rect.Contains(e.X, e.Y))
					{
						Value = item3.date;
						return;
					}
				}
			}
		}
		((Control)this).OnMouseUp(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (e.Delta != 0)
		{
			MouseWheelDay(e);
		}
		base.OnMouseWheel(e);
	}

	private void MouseWheelDay(MouseEventArgs e)
	{
		if (e.Delta > 0)
		{
			if (showType == 1)
			{
				if (!hover_lefts.Enable)
				{
					return;
				}
				Date = _Date.AddYears(-1);
			}
			else if (showType == 2)
			{
				if (!hover_lefts.Enable)
				{
					return;
				}
				Date = _Date.AddYears(-10);
			}
			else
			{
				if (!hover_left.Enable)
				{
					return;
				}
				Date = _Date.AddMonths(-1);
			}
			((Control)this).Invalidate();
			return;
		}
		if (showType == 1)
		{
			if (!hover_rights.Enable)
			{
				return;
			}
			Date = _Date.AddYears(1);
		}
		else if (showType == 2)
		{
			if (!hover_rights.Enable)
			{
				return;
			}
			Date = _Date.AddYears(10);
		}
		else
		{
			if (!hover_right.Enable)
			{
				return;
			}
			Date = _Date.AddMonths(1);
		}
		((Control)this).Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		float dpi = Config.Dpi;
		Rectangle readRectangle = ReadRectangle;
		int num = 34;
		int num2 = (showButtonToDay ? 38 : 0);
		int num3 = 60;
		int num4 = 88;
		int num5 = 40;
		if (dpi != 1f)
		{
			num = (int)((float)num * dpi);
			if (showButtonToDay)
			{
				num2 = (int)((float)num2 * dpi);
			}
			num3 = (int)((float)num3 * dpi);
			num4 = (int)((float)num4 * dpi);
			num5 = (int)((float)num5 * dpi);
		}
		rect_lefts = new Rectangle(readRectangle.X, readRectangle.Y, num, num);
		rect_left = new Rectangle(readRectangle.X + num, readRectangle.Y, num, num);
		rect_rights = new Rectangle(readRectangle.X + readRectangle.Width - num, readRectangle.Y, num, num);
		rect_right = new Rectangle(readRectangle.X + readRectangle.Width - num * 2, readRectangle.Y, num, num);
		int num6 = (int)(4f * Config.Dpi);
		int num7 = readRectangle.Width / 2;
		rect_year2 = new Rectangle(readRectangle.X + (readRectangle.Width - num4) / 2, readRectangle.Y, num4, num);
		rect_button = new Rectangle(readRectangle.X, readRectangle.Bottom - num2, readRectangle.Width, num2);
		if (YDR)
		{
			rect_month = new Rectangle(readRectangle.X + num7 - num3 - num6, readRectangle.Y, num3, num);
			rect_year = new Rectangle(readRectangle.X + num7 + num6, readRectangle.Y, num5, num);
		}
		else
		{
			rect_year = new Rectangle(readRectangle.X + num7 - num3 - num6, readRectangle.Y, num3, num);
			rect_month = new Rectangle(readRectangle.X + num7 + num6, readRectangle.Y, num5, num);
		}
		int num8 = (int)(8f * dpi);
		int num9 = num8 * 2;
		if (showType == 1)
		{
			rect_month_l = new Rectangle(readRectangle.X, readRectangle.Y, readRectangle.Width, num);
			int num10 = readRectangle.Y + num;
			int num11 = (readRectangle.Width - num9) / 3;
			int num12 = (readRectangle.Height - num - num9) / 4;
			if (calendar_month != null)
			{
				foreach (Calendari item in calendar_month)
				{
					item.rect = new Rectangle(readRectangle.X + num8 + num11 * item.x, num10 + num8 + num12 * item.y, num11, num12);
				}
			}
		}
		else if (showType == 2)
		{
			rect_year_l = new Rectangle(readRectangle.X, readRectangle.Y, readRectangle.Width, num);
			int num13 = readRectangle.Y + num;
			int num14 = (readRectangle.Width - num9) / 3;
			int num15 = (readRectangle.Height - num - num9) / 4;
			if (calendar_year != null)
			{
				foreach (Calendari item2 in calendar_year)
				{
					item2.rect = new Rectangle(readRectangle.X + num8 + num14 * item2.x, num13 + num8 + num15 * item2.y, num14, num15);
				}
			}
		}
		else if (chinese)
		{
			int num16 = readRectangle.Y + num + 12;
			int num17 = (readRectangle.Width - num9) / 7;
			int num18 = (readRectangle.Height - num - num2 - num9) / 7;
			rect_day_split1 = new RectangleF(readRectangle.X, readRectangle.Y + num, readRectangle.Width, Config.Dpi);
			if (showButtonToDay)
			{
				rect_day_split2 = new RectangleF(readRectangle.X, (float)rect_button.Y - 0.5f, readRectangle.Width, Config.Dpi);
			}
			rect_day_s = new Rectangle[7]
			{
				new Rectangle(readRectangle.X + num8, num16, num17, num18),
				new Rectangle(readRectangle.X + num8 + num17, num16, num17, num18),
				new Rectangle(readRectangle.X + num8 + num17 * 2, num16, num17, num18),
				new Rectangle(readRectangle.X + num8 + num17 * 3, num16, num17, num18),
				new Rectangle(readRectangle.X + num8 + num17 * 4, num16, num17, num18),
				new Rectangle(readRectangle.X + num8 + num17 * 5, num16, num17, num18),
				new Rectangle(readRectangle.X + num8 + num17 * 6, num16, num17, num18)
			};
			num16 += num18;
			if (calendar_day != null)
			{
				foreach (Calendari item3 in calendar_day)
				{
					item3.SetRectG(new Rectangle(readRectangle.X + num8 + num17 * item3.x, num16 + num18 * item3.y, num17, num18), 0.92f);
					item3.rect_f = new Rectangle(item3.rect_read.X, item3.rect_read.Y, item3.rect_read.Width, item3.rect_read.Height - item3.rect_read.Height / 4);
					item3.rect_l = new Rectangle(item3.rect_read.X, item3.rect_read.Y + item3.rect_read.Height / 2, item3.rect_read.Width, item3.rect_read.Height / 2);
				}
			}
		}
		else if (full)
		{
			int num19 = readRectangle.Y + num + 12;
			int num20 = (readRectangle.Width - num9) / 7;
			int num21 = (readRectangle.Height - num - num2 - num9) / 7;
			rect_day_split1 = new RectangleF(readRectangle.X, readRectangle.Y + num, readRectangle.Width, Config.Dpi);
			if (showButtonToDay)
			{
				rect_day_split2 = new RectangleF(readRectangle.X, (float)rect_button.Y - 0.5f, readRectangle.Width, Config.Dpi);
			}
			rect_day_s = new Rectangle[7]
			{
				new Rectangle(readRectangle.X + num8, num19, num20, num21),
				new Rectangle(readRectangle.X + num8 + num20, num19, num20, num21),
				new Rectangle(readRectangle.X + num8 + num20 * 2, num19, num20, num21),
				new Rectangle(readRectangle.X + num8 + num20 * 3, num19, num20, num21),
				new Rectangle(readRectangle.X + num8 + num20 * 4, num19, num20, num21),
				new Rectangle(readRectangle.X + num8 + num20 * 5, num19, num20, num21),
				new Rectangle(readRectangle.X + num8 + num20 * 6, num19, num20, num21)
			};
			num19 += num21;
			if (calendar_day != null)
			{
				foreach (Calendari item4 in calendar_day)
				{
					item4.SetRectG(new Rectangle(readRectangle.X + num8 + num20 * item4.x, num19 + num21 * item4.y, num20, num21), 0.92f);
				}
			}
		}
		else
		{
			int num22 = readRectangle.Y + num + 12;
			int num23 = (readRectangle.Width - num9) / 7;
			int num24 = (readRectangle.Height - num - num2 - num9) / 7;
			int num25 = num23;
			if (num23 > num24)
			{
				num25 = num24;
				num9 = readRectangle.Width - num25 * 7;
				num8 = num9 / 2;
			}
			rect_day_split1 = new RectangleF(readRectangle.X, readRectangle.Y + num, readRectangle.Width, Config.Dpi);
			if (showButtonToDay)
			{
				rect_day_split2 = new RectangleF(readRectangle.X, (float)rect_button.Y - 0.5f, readRectangle.Width, Config.Dpi);
			}
			rect_day_s = new Rectangle[7]
			{
				new Rectangle(readRectangle.X + num8, num22, num25, num25),
				new Rectangle(readRectangle.X + num8 + num25, num22, num25, num25),
				new Rectangle(readRectangle.X + num8 + num25 * 2, num22, num25, num25),
				new Rectangle(readRectangle.X + num8 + num25 * 3, num22, num25, num25),
				new Rectangle(readRectangle.X + num8 + num25 * 4, num22, num25, num25),
				new Rectangle(readRectangle.X + num8 + num25 * 5, num22, num25, num25),
				new Rectangle(readRectangle.X + num8 + num25 * 6, num22, num25, num25)
			};
			num22 += num25;
			if (calendar_day != null)
			{
				int gap = (int)((float)num25 * 0.666f);
				foreach (Calendari item5 in calendar_day)
				{
					item5.SetRect(new Rectangle(readRectangle.X + num8 + num25 * item5.x, num22 + num25 * item5.y, num25, num25), gap);
				}
			}
		}
		((Control)this).OnSizeChanged(e);
	}

	protected override void Dispose(bool disposing)
	{
		hover_lefts?.Dispose();
		hover_left?.Dispose();
		hover_rights?.Dispose();
		hover_right?.Dispose();
		hover_year?.Dispose();
		hover_month?.Dispose();
		hover_button?.Dispose();
		base.Dispose(disposing);
	}
}
