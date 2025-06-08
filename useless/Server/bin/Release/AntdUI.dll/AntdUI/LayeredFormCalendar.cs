using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace AntdUI;

public class LayeredFormCalendar : ILayeredFormOpacityDown
{
	private DateTime? minDate;

	private DateTime? maxDate;

	private bool ValueTimeHorizontal;

	private IControl control;

	private bool ShowTime;

	private float Radius = 6f;

	private int t_width = 288;

	private int t_h;

	private int t_one_width = 288;

	private int t_x;

	private int left_button = 120;

	private int t_top = 34;

	private int t_button = 38;

	private int t_time = 56;

	private int t_time_height = 30;

	private int year_width = 60;

	private int year2_width = 90;

	private int month_width = 60;

	private TAlign ArrowAlign;

	private int ArrowSize = 8;

	private CultureInfo Culture;

	private string CultureID = Localization.Get("ID", "zh-CN");

	private string button_text = Localization.Get("ToDay", "今天");

	private string OKButton = Localization.Get("OK", "确定");

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

	private ScrollY scrollY_left;

	private ScrollY scrollY_h;

	private ScrollY scrollY_m;

	private ScrollY scrollY_s;

	private List<CalendarButton>? left_buttons;

	private Action<DateTime> action;

	private Action<object> action_btns;

	private Func<DateTime[], List<DateBadge>?>? badge_action;

	private Dictionary<string, DateBadge> badge_list = new Dictionary<string, DateBadge>();

	public DateTime? SelDate;

	private DateTime _Date;

	private DateTime DateNow = DateTime.Now;

	private List<Calendari>? calendar_year;

	private List<Calendari>? calendar_month;

	private List<Calendari>? calendar_day;

	private List<CalendarT>? calendar_time;

	private string year_str = "";

	private bool sizeday = true;

	private bool size_month = true;

	private bool size_year = true;

	private StringFormat s_f = Helper.SF((StringAlignment)1, (StringAlignment)1);

	private StringFormat s_f_LE = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)0);

	private StringFormat s_f_L;

	private StringFormat s_f_R;

	private Rectangle rect_read_left;

	private Rectangle rect_read_h;

	private Rectangle rect_read_m;

	private Rectangle rect_read_s;

	private Bitmap? shadow_temp;

	private ITaskOpacity hover_button;

	private ITaskOpacity hover_buttonok;

	private ITaskOpacity hover_lefts;

	private ITaskOpacity hover_left;

	private ITaskOpacity hover_rights;

	private ITaskOpacity hover_right;

	private ITaskOpacity hover_year;

	private ITaskOpacity hover_month;

	private Rectangle rect_button = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_buttonok = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_lefts = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_left = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_rights = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_right = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year2 = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_month = new Rectangle(-20, -20, 10, 10);

	private int showType;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DateTime Date
	{
		get
		{
			return _Date;
		}
		set
		{
			_Date = value;
			sizeday = (size_month = (size_year = true));
			calendar_day = GetCalendar(value);
			if (ShowTime && calendar_time == null)
			{
				calendar_time = new List<CalendarT>(144);
				for (int i = 0; i < 24; i++)
				{
					calendar_time.Add(new CalendarT(0, i, i));
				}
				for (int j = 0; j < 60; j++)
				{
					calendar_time.Add(new CalendarT(1, j, j));
				}
				for (int k = 0; k < 60; k++)
				{
					calendar_time.Add(new CalendarT(2, k, k));
				}
			}
			List<Calendari> list = new List<Calendari>(12);
			int num = 0;
			int num2 = 0;
			for (int l = 0; l < 12; l++)
			{
				DateTime date = new DateTime(value.Year, l + 1, 1);
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
			for (int m = 0; m < 12; m++)
			{
				DateTime date2 = new DateTime(num3 + m, value.Month, 1);
				list2.Add(new Calendari((m != 0) ? 1 : 0, num4, num5, date2.ToString("yyyy"), date2, date2.ToString("yyyy"), minDate, maxDate));
				num4++;
				if (num4 > 2)
				{
					num5++;
					num4 = 0;
				}
			}
			year_str = list2[1].date_str + "-" + list2[list2.Count - 2].date_str;
			calendar_year = list2;
			if (badge_action != null)
			{
				DateTime oldval = value;
				ITask.Run(delegate
				{
					List<DateBadge> list3 = badge_action(new DateTime[2]
					{
						calendar_day[0].date,
						calendar_day[calendar_day.Count - 1].date
					});
					if (_Date == oldval)
					{
						badge_list.Clear();
						if (list3 == null)
						{
							if (RunAnimation)
							{
								DisposeTmp();
							}
							else
							{
								Print();
							}
						}
						else
						{
							foreach (DateBadge item in list3)
							{
								badge_list.Add(item.Date, item);
							}
							if (RunAnimation)
							{
								DisposeTmp();
							}
							else
							{
								Print();
							}
						}
					}
				});
			}
			hover_left.Enable = Helper.DateExceedMonth(value.AddMonths(-1), minDate, maxDate);
			hover_right.Enable = Helper.DateExceedMonth(value.AddMonths(1), minDate, maxDate);
			hover_lefts.Enable = Helper.DateExceedYear(value.AddYears(-1), minDate, maxDate);
			hover_rights.Enable = Helper.DateExceedYear(value.AddYears(1), minDate, maxDate);
		}
	}

	public LayeredFormCalendar(DatePicker _control, Rectangle rect_read, DateTime? date, Action<DateTime> _action, Action<object> _action_btns, Func<DateTime[], List<DateBadge>?>? _badge_action = null)
	{
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Expected O, but got Unknown
		((Control)_control).Parent.SetTopMost(((Control)this).Handle);
		control = _control;
		minDate = _control.MinDate;
		maxDate = _control.MaxDate;
		badge_action = _badge_action;
		PARENT = (Control?)(object)_control;
		action = _action;
		action_btns = _action_btns;
		ShowTime = _control.ShowTime;
		ValueTimeHorizontal = _control.ValueTimeHorizontal;
		hover_lefts = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_left = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_rights = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_right = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_year = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_month = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_button = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_buttonok = new ITaskOpacity((ILayeredFormOpacityDown)this);
		scrollY_left = new ScrollY((ILayeredForm)this);
		scrollY_h = new ScrollY((ILayeredForm)this);
		scrollY_m = new ScrollY((ILayeredForm)this);
		scrollY_s = new ScrollY((ILayeredForm)this);
		Culture = new CultureInfo(CultureID);
		YDR = CultureID.StartsWith("en");
		float dpi = Config.Dpi;
		if (dpi != 1f)
		{
			Radius = (float)_control.Radius * dpi;
			t_one_width = (int)((float)t_one_width * dpi);
			t_top = (int)((float)t_top * dpi);
			t_time = (int)((float)t_time * dpi);
			t_time_height = (int)((float)t_time_height * dpi);
			t_button = (int)((float)t_button * dpi);
			left_button = (int)((float)left_button * dpi);
			year_width = (int)((float)year_width * dpi);
			year2_width = (int)((float)year2_width * dpi);
			month_width = (int)((float)month_width * dpi);
		}
		else
		{
			Radius = _control.Radius;
		}
		if (_control.Presets.Count > 0)
		{
			left_buttons = new List<CalendarButton>(_control.Presets.Count);
			int num = 0;
			foreach (object preset in _control.Presets)
			{
				left_buttons.Add(new CalendarButton(num, preset));
				num++;
			}
			t_x = left_button;
		}
		if (ShowTime)
		{
			t_width = t_x + t_one_width + t_time * 3;
			button_text = Localization.Get("Now", "此刻");
		}
		else
		{
			t_width = t_x + t_one_width;
		}
		rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
		rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
		rect_rights = new Rectangle(t_x + t_one_width + 10 - t_top, 10, t_top, t_top);
		rect_right = new Rectangle(t_x + t_one_width + 10 - t_top * 2, 10, t_top, t_top);
		int num2 = (int)(4f * dpi);
		int num3 = t_one_width / 2;
		rect_year2 = new Rectangle(t_x + 10 + (t_one_width - year2_width) / 2, 10, year2_width, t_top);
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
			rect_month = new Rectangle(t_x + 10 + num3 - year_width - num2, 10, year_width, t_top);
			rect_year = new Rectangle(t_x + 10 + num3 + num2, 10, month_width, t_top);
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
			rect_year = new Rectangle(t_x + 10 + num3 - year_width - num2, 10, year_width, t_top);
			rect_month = new Rectangle(t_x + 10 + num3 + num2, 10, month_width, t_top);
			s_f_L = Helper.SF((StringAlignment)1, (StringAlignment)2);
			s_f_R = Helper.SF((StringAlignment)1, (StringAlignment)0);
		}
		((Control)this).Font = new Font(((Control)_control).Font.FontFamily, 11.2f);
		SelDate = date;
		Date = date ?? DateNow;
		Point point = ((Control)_control).PointToScreen(Point.Empty);
		int num4 = t_width + 20;
		int num5 = ((calendar_day != null) ? (t_top + t_button + 24 + (int)Math.Ceiling((float)((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16)) / 7f) + 20) : 368);
		SetSize(num4, num5);
		rect_button = new Rectangle(t_x + 10 + (t_one_width - year_width) / 2, num5 - t_button - 12, year_width, t_button);
		if (ShowTime)
		{
			int width = t_time * 3;
			rect_buttonok = new Rectangle(t_x + 10 + t_one_width, rect_button.Y, width, t_button);
		}
		CLocation(point, _control.Placement, _control.DropDownArrow, 10, num4, num5, rect_read, ref Inverted, ref ArrowAlign);
		t_h = num5;
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

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Expected O, but got Unknown
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f5: Expected O, but got Unknown
		//IL_07ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d5: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rectangle = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
		{
			GraphicsPath val2 = rectangle.RoundPath(Radius);
			try
			{
				DrawShadow(canvas, targetRectXY);
				SolidBrush val3 = new SolidBrush(Colour.BgElevated.Get("DatePicker"));
				try
				{
					canvas.Fill((Brush)(object)val3, val2);
					if (ArrowAlign != 0)
					{
						canvas.FillPolygon((Brush)(object)val3, ArrowAlign.AlignLines(ArrowSize, targetRectXY, rectangle));
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
			Pen val4 = new Pen(Colour.TextTertiary.Get("DatePicker"), 1.6f * Config.Dpi);
			try
			{
				Pen val5 = new Pen(Colour.Text.Get("DatePicker"), val4.Width);
				try
				{
					Pen val6 = new Pen(Colour.FillSecondary.Get("DatePicker"), val4.Width);
					try
					{
						if (hover_lefts.Animation)
						{
							Pen val7 = new Pen(val4.Color.BlendColors(hover_lefts.Value, val5.Color), val5.Width);
							try
							{
								canvas.DrawLines(val7, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
								canvas.DrawLines(val7, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
							}
						}
						else if (hover_lefts.Switch)
						{
							canvas.DrawLines(val5, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
							canvas.DrawLines(val5, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						}
						else if (hover_lefts.Enable)
						{
							canvas.DrawLines(val4, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
							canvas.DrawLines(val4, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						}
						else
						{
							canvas.DrawLines(val6, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
							canvas.DrawLines(val6, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26f));
						}
						if (hover_rights.Animation)
						{
							Pen val8 = new Pen(val4.Color.BlendColors(hover_rights.Value, val5.Color), val5.Width);
							try
							{
								canvas.DrawLines(val8, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
								canvas.DrawLines(val8, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
							}
							finally
							{
								((IDisposable)val8)?.Dispose();
							}
						}
						else if (hover_rights.Switch)
						{
							canvas.DrawLines(val5, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
							canvas.DrawLines(val5, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						}
						else if (hover_rights.Enable)
						{
							canvas.DrawLines(val4, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
							canvas.DrawLines(val4, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						}
						else
						{
							canvas.DrawLines(val6, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
							canvas.DrawLines(val6, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26f));
						}
						if (showType == 0)
						{
							if (hover_left.Animation)
							{
								Pen val9 = new Pen(val4.Color.BlendColors(hover_left.Value, val5.Color), val5.Width);
								try
								{
									canvas.DrawLines(val9, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
								}
								finally
								{
									((IDisposable)val9)?.Dispose();
								}
							}
							else if (hover_left.Switch)
							{
								canvas.DrawLines(val5, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
							}
							else if (hover_left.Enable)
							{
								canvas.DrawLines(val4, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
							}
							else
							{
								canvas.DrawLines(val6, TAlignMini.Left.TriangleLines(rect_left, 0.26f));
							}
							if (hover_right.Animation)
							{
								Pen val10 = new Pen(val4.Color.BlendColors(hover_right.Value, val5.Color), val5.Width);
								try
								{
									canvas.DrawLines(val10, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
								}
								finally
								{
									((IDisposable)val10)?.Dispose();
								}
							}
							else if (hover_right.Switch)
							{
								canvas.DrawLines(val5, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
							}
							else if (hover_right.Enable)
							{
								canvas.DrawLines(val4, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
							}
							else
							{
								canvas.DrawLines(val6, TAlignMini.Right.TriangleLines(rect_right, 0.26f));
							}
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
			if (showType == 1 && calendar_month != null)
			{
				PrintMonth(canvas, rectangle, calendar_month);
			}
			else if (showType == 2 && calendar_year != null)
			{
				PrintYear(canvas, rectangle, calendar_year);
			}
			else if (calendar_day != null)
			{
				PrintDay(canvas, rectangle, calendar_day);
			}
		}
		return val;
	}

	private void PrintYear(Canvas g, Rectangle rect_read, List<Calendari> datas)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		Color color = Colour.TextBase.Get("DatePicker");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			Rectangle rect = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width, t_top);
			if (hover_year.Animation)
			{
				g.String(year_str, val, color.BlendColors(hover_year.Value, Colour.Primary.Get("DatePicker")), rect, s_f);
			}
			else if (hover_year.Switch)
			{
				g.String(year_str, val, Colour.Primary.Get("DatePicker"), rect, s_f);
			}
			else
			{
				g.String(year_str, val, color, rect, s_f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		int num = (rect_read.Width - 16) / 3;
		int num2 = (rect_read.Width - 16) / 7 * 2;
		int num3 = rect_read.Y + t_top;
		if (size_year)
		{
			size_year = false;
			foreach (Calendari data in datas)
			{
				data.rect = new Rectangle(rect_read.X + 8 + num * data.x, num3 + num2 * data.y, num, num2);
			}
		}
		SolidBrush val2 = new SolidBrush(Colour.TextQuaternary.Get("DatePicker"));
		try
		{
			SolidBrush val3 = new SolidBrush(Colour.FillTertiary.Get("DatePicker"));
			try
			{
				SolidBrush val4 = new SolidBrush(color);
				try
				{
					foreach (Calendari data2 in datas)
					{
						GraphicsPath val5 = data2.rect_read.RoundPath(Radius);
						try
						{
							if (SelDate.HasValue && SelDate.Value.ToString("yyyy") == data2.date_str)
							{
								g.Fill(Colour.Primary.Get("DatePicker"), val5);
								g.String(data2.v, ((Control)this).Font, Colour.PrimaryColor.Get("DatePicker"), data2.rect, s_f);
							}
							else if (data2.enable)
							{
								if (data2.hover)
								{
									g.Fill(Colour.FillTertiary.Get("DatePicker"), val5);
								}
								if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
								{
									g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val5);
								}
								g.String(data2.v, ((Control)this).Font, (Brush)(object)((data2.t == 1) ? val4 : val2), data2.rect, s_f);
							}
							else
							{
								g.Fill((Brush)(object)val3, new Rectangle(data2.rect.X, data2.rect_read.Y, data2.rect.Width, data2.rect_read.Height));
								if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
								{
									g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val5);
								}
								g.String(data2.v, ((Control)this).Font, (Brush)(object)val2, data2.rect, s_f);
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

	private void PrintMonth(Canvas g, Rectangle rect_read, List<Calendari> datas)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		Color color = Colour.TextBase.Get("DatePicker");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			Rectangle rect = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width, t_top);
			string text = _Date.ToString(YearFormat, Culture);
			if (hover_year.Animation)
			{
				g.String(text, val, color.BlendColors(hover_year.Value, Colour.Primary.Get("DatePicker")), rect, s_f);
			}
			else if (hover_year.Switch)
			{
				g.String(text, val, Colour.Primary.Get("DatePicker"), rect, s_f);
			}
			else
			{
				g.String(text, val, color, rect, s_f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		int num = (rect_read.Width - 16) / 3;
		int num2 = (rect_read.Width - 16) / 7 * 2;
		int num3 = rect_read.Y + t_top;
		if (size_month)
		{
			size_month = false;
			foreach (Calendari data in datas)
			{
				data.rect = new Rectangle(rect_read.X + 8 + num * data.x, num3 + num2 * data.y, num, num2);
			}
		}
		SolidBrush val2 = new SolidBrush(Colour.TextQuaternary.Get("DatePicker"));
		try
		{
			SolidBrush val3 = new SolidBrush(Colour.FillTertiary.Get("DatePicker"));
			try
			{
				SolidBrush val4 = new SolidBrush(color);
				try
				{
					foreach (Calendari data2 in datas)
					{
						GraphicsPath val5 = data2.rect_read.RoundPath(Radius);
						try
						{
							if (SelDate.HasValue && SelDate.Value.ToString("yyyy-MM") == data2.date_str)
							{
								g.Fill(Colour.Primary.Get("DatePicker"), val5);
								g.String(data2.v, ((Control)this).Font, Colour.PrimaryColor.Get("DatePicker"), data2.rect, s_f);
							}
							else if (data2.enable)
							{
								if (data2.hover)
								{
									g.Fill(Colour.FillTertiary.Get("DatePicker"), val5);
								}
								if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
								{
									g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val5);
								}
								g.String(data2.v, ((Control)this).Font, (Brush)(object)val4, data2.rect, s_f);
							}
							else
							{
								g.Fill((Brush)(object)val3, new Rectangle(data2.rect.X, data2.rect_read.Y, data2.rect.Width, data2.rect_read.Height));
								if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
								{
									g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val5);
								}
								g.String(data2.v, ((Control)this).Font, (Brush)(object)val2, data2.rect, s_f);
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

	private void PrintDay(Canvas g, Rectangle rect_read, List<Calendari> datas)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Expected O, but got Unknown
		//IL_0970: Unknown result type (might be due to invalid IL or missing references)
		//IL_0977: Expected O, but got Unknown
		//IL_0983: Unknown result type (might be due to invalid IL or missing references)
		//IL_098a: Expected O, but got Unknown
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_099d: Expected O, but got Unknown
		//IL_09a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09af: Expected O, but got Unknown
		//IL_09ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c1: Expected O, but got Unknown
		//IL_09cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d4: Expected O, but got Unknown
		//IL_0d42: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d49: Expected O, but got Unknown
		//IL_0d55: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5c: Expected O, but got Unknown
		Color color = Colour.TextBase.Get("DatePicker");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			string text = _Date.ToString(YearFormat, Culture);
			string text2 = _Date.ToString(MonthFormat, Culture);
			if (hover_year.Animation)
			{
				g.String(text, val, color.BlendColors(hover_year.Value, Colour.Primary.Get("DatePicker")), rect_year, s_f_L);
			}
			else if (hover_year.Switch)
			{
				g.String(text, val, Colour.Primary.Get("DatePicker"), rect_year, s_f_L);
			}
			else
			{
				g.String(text, val, color, rect_year, s_f_L);
			}
			if (hover_month.Animation)
			{
				g.String(text2, val, color.BlendColors(hover_month.Value, Colour.Primary.Get("DatePicker")), rect_month, s_f_R);
			}
			else if (hover_month.Switch)
			{
				g.String(text2, val, Colour.Primary.Get("DatePicker"), rect_month, s_f_R);
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
		SolidBrush val2 = new SolidBrush(Colour.Split.Get("DatePicker"));
		try
		{
			g.Fill((Brush)(object)val2, new Rectangle(t_x + rect_read.X, rect_read.Y + t_top, t_one_width, 1));
			g.Fill((Brush)(object)val2, new Rectangle(t_x + rect_read.X, rect_button.Y, rect_read.Width - t_x, 1));
			if (ShowTime)
			{
				g.Fill((Brush)(object)val2, new Rectangle(t_x + rect_read.X + t_one_width, rect_read.Y, 1, rect_read.Height));
			}
			if (left_buttons != null)
			{
				g.Fill((Brush)(object)val2, new Rectangle(t_x + rect_read.X, rect_read.Y, 1, rect_read.Height));
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		int num = rect_read.Y + t_top + 12;
		int num2 = (t_one_width - 16) / 7;
		SolidBrush val3 = new SolidBrush(Colour.Text.Get("DatePicker"));
		try
		{
			g.String(MondayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8, num, num2, num2), s_f);
			g.String(TuesdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8 + num2, num, num2, num2), s_f);
			g.String(WednesdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8 + num2 * 2, num, num2, num2), s_f);
			g.String(ThursdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8 + num2 * 3, num, num2, num2), s_f);
			g.String(FridayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8 + num2 * 4, num, num2, num2), s_f);
			g.String(SaturdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8 + num2 * 5, num, num2, num2), s_f);
			g.String(SundayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(t_x + rect_read.X + 8 + num2 * 6, num, num2, num2), s_f);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
		num += num2;
		if (sizeday)
		{
			sizeday = false;
			int gap = (int)((float)num2 * 0.666f);
			foreach (Calendari data in datas)
			{
				data.SetRect(new Rectangle(t_x + rect_read.X + 8 + num2 * data.x, num + num2 * data.y, num2, num2), gap);
			}
			if (calendar_time != null)
			{
				int num3 = (int)((float)t_time * 0.857f);
				int num4 = (int)((float)t_time_height * 0.93f);
				int y = rect_button.Y;
				Rectangle rect = new Rectangle(t_x, rect_read.Y + 8, rect_read.X + t_one_width + t_time, y - rect_read.Y - 8);
				rect_read_h = new Rectangle(rect.Right - t_time, 0, t_time, y);
				rect_read_m = new Rectangle(rect.Right, 0, t_time, y);
				rect_read_s = new Rectangle(rect.Right + t_time, 0, t_time, y);
				scrollY_h.SizeChange(rect);
				rect.Width += t_time;
				scrollY_m.SizeChange(rect);
				rect.Width += t_time;
				scrollY_s.SizeChange(rect);
				int height = y - rect_read.Y * 2 - (t_time_height - num4);
				if (ValueTimeHorizontal)
				{
					int num5 = 10;
					scrollY_h.SetVrSize(t_time_height * (24 + num5), height);
					scrollY_m.SetVrSize(t_time_height * (60 + num5), height);
					scrollY_s.SetVrSize(t_time_height * (60 + num5), height);
				}
				else
				{
					scrollY_h.SetVrSize(t_time_height * 24, height);
					scrollY_m.SetVrSize(t_time_height * 60, height);
					scrollY_s.SetVrSize(t_time_height * 60, height);
				}
				int num6 = (t_time - num3) / 2;
				int num7 = rect_read.Y + (t_time_height - num4) / 2;
				foreach (CalendarT item in calendar_time)
				{
					Rectangle rectangle = new Rectangle(t_time * item.x, t_time_height * item.y, t_time, t_time_height);
					item.rect = new Rectangle(t_x + rect_read.X + t_one_width + rectangle.X, rect_read.Y + rectangle.Y, rectangle.Width, rectangle.Height);
					item.rect_read = new Rectangle(rectangle.X + num6, rectangle.Y + num7, num3, num4);
				}
				if (SelDate.HasValue)
				{
					ScrollYTime(calendar_time, SelDate.Value);
				}
			}
			if (left_buttons != null)
			{
				int num8 = (int)((float)left_button * 0.9f);
				int num9 = (int)((float)t_time_height * 0.93f);
				int num10 = (int)((float)left_button * 0.8f);
				rect_read_left = new Rectangle(rect_read.X, rect_read.Y, t_x, t_h - rect_read.Y * 2);
				scrollY_left.SizeChange(new Rectangle(rect_read.X, rect_read.Y + 8, t_x, t_h - (8 + rect_read.Y) * 2));
				scrollY_left.SetVrSize(t_time_height * left_buttons.Count, t_h - 20 - rect_read.Y * 2);
				int num11 = (left_button - num8) / 2;
				int num12 = (num8 - num10) / 2;
				int num13 = rect_read.Y + (t_time_height - num9) / 2;
				foreach (CalendarButton left_button in left_buttons)
				{
					Rectangle rectangle2 = new Rectangle(0, t_time_height * left_button.y, this.left_button, t_time_height);
					left_button.rect_read = new Rectangle(rectangle2.X + num11, rectangle2.Y + num13, num8, num9);
					left_button.rect = new Rectangle(rect_read.X + rectangle2.X, rect_read.Y + rectangle2.Y, rectangle2.Width, rectangle2.Height);
					left_button.rect_text = new Rectangle(rect_read.X + num12, left_button.rect_read.Y, num10, left_button.rect_read.Height);
				}
			}
		}
		SolidBrush val4 = new SolidBrush(color);
		try
		{
			SolidBrush val5 = new SolidBrush(Colour.TextQuaternary.Get("DatePicker"));
			try
			{
				SolidBrush val6 = new SolidBrush(Colour.FillTertiary.Get("DatePicker"));
				try
				{
					SolidBrush val7 = new SolidBrush(Colour.Primary.Get("DatePicker"));
					try
					{
						SolidBrush val8 = new SolidBrush(Colour.PrimaryColor.Get("DatePicker"));
						try
						{
							SolidBrush val9 = new SolidBrush(Colour.Error.Get("DatePicker"));
							try
							{
								foreach (Calendari data2 in datas)
								{
									if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
									{
										GraphicsPath val10 = data2.rect_read.RoundPath(Radius);
										try
										{
											g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val10);
										}
										finally
										{
											((IDisposable)val10)?.Dispose();
										}
									}
								}
								foreach (Calendari data3 in datas)
								{
									GraphicsPath val11 = data3.rect_read.RoundPath(Radius);
									try
									{
										if (SelDate.HasValue && SelDate.Value.ToString("yyyy-MM-dd") == data3.date_str)
										{
											g.Fill((Brush)(object)val7, val11);
											g.String(data3.v, ((Control)this).Font, (Brush)(object)val8, data3.rect, s_f);
										}
										else if (data3.enable)
										{
											if (data3.hover)
											{
												g.Fill(Colour.FillTertiary.Get("DatePicker"), val11);
											}
											g.String(data3.v, ((Control)this).Font, (Brush)(object)((data3.t == 1) ? val4 : val5), data3.rect, s_f);
										}
										else
										{
											g.Fill((Brush)(object)val6, new Rectangle(data3.rect.X, data3.rect_read.Y, data3.rect.Width, data3.rect_read.Height));
											g.String(data3.v, ((Control)this).Font, (Brush)(object)val5, data3.rect, s_f);
										}
									}
									finally
									{
										((IDisposable)val11)?.Dispose();
									}
								}
								if (rect_read.Height > t_button)
								{
									if (left_buttons != null)
									{
										GraphicsState state = g.Save();
										g.SetClip(new Rectangle(rect_read.X, rect_read.Y, this.left_button, rect_read.Height));
										g.TranslateTransform(rect_read.X, (float)rect_read.Y - scrollY_left.Value);
										foreach (CalendarButton left_button2 in left_buttons)
										{
											GraphicsPath val12 = left_button2.rect_read.RoundPath(Radius);
											try
											{
												if (left_button2.hover)
												{
													g.Fill(Colour.FillTertiary.Get("DatePicker"), val12);
												}
												g.String(left_button2.v, ((Control)this).Font, (Brush)(object)val4, left_button2.rect_text, s_f_LE);
											}
											finally
											{
												((IDisposable)val12)?.Dispose();
											}
										}
										g.Restore(state);
										scrollY_left.Paint(g);
									}
									if (calendar_time != null)
									{
										GraphicsState state2 = g.Save();
										int num14 = t_x + rect_read.X + t_one_width;
										g.SetClip(new Rectangle(num14, rect_read.Y, t_time * 3, rect_button.Y - 10));
										SolidBrush val13 = new SolidBrush(Colour.PrimaryBg.Get("DatePicker"));
										try
										{
											SolidBrush val14 = new SolidBrush(Colour.FillTertiary.Get("DatePicker"));
											try
											{
												g.TranslateTransform(num14, 10f - scrollY_h.Value);
												for (int i = 0; i < calendar_time.Count; i++)
												{
													switch (i)
													{
													case 24:
														g.ResetTransform();
														g.TranslateTransform(num14, 10f - scrollY_m.Value);
														break;
													case 84:
														g.ResetTransform();
														g.TranslateTransform(num14, 10f - scrollY_s.Value);
														break;
													}
													CalendarT calendarT = calendar_time[i];
													bool flag = false;
													if (SelDate.HasValue)
													{
														switch (calendarT.x)
														{
														case 0:
															if (calendarT.t == SelDate.Value.Hour)
															{
																flag = true;
															}
															break;
														case 1:
															if (calendarT.t == SelDate.Value.Minute)
															{
																flag = true;
															}
															break;
														case 2:
															if (calendarT.t == SelDate.Value.Second)
															{
																flag = true;
															}
															break;
														}
													}
													if (flag || calendarT.hover)
													{
														GraphicsPath val15 = calendarT.rect_read.RoundPath(Radius);
														try
														{
															if (flag)
															{
																g.Fill((Brush)(object)val13, val15);
															}
															if (calendarT.hover)
															{
																g.Fill((Brush)(object)val14, val15);
															}
														}
														finally
														{
															((IDisposable)val15)?.Dispose();
														}
													}
													g.String(calendarT.v, ((Control)this).Font, (Brush)(object)val4, calendarT.rect_read, s_f);
												}
											}
											finally
											{
												((IDisposable)val14)?.Dispose();
											}
										}
										finally
										{
											((IDisposable)val13)?.Dispose();
										}
										g.Restore(state2);
										scrollY_h.Paint(g);
										scrollY_m.Paint(g);
										scrollY_s.Paint(g);
										if (hover_buttonok.Animation)
										{
											g.String(OKButton, ((Control)this).Font, val7.Color.BlendColors(hover_buttonok.Value, Colour.PrimaryActive.Get("DatePicker")), rect_buttonok, s_f);
										}
										else if (hover_buttonok.Switch)
										{
											g.String(OKButton, ((Control)this).Font, Colour.PrimaryActive.Get("DatePicker"), rect_buttonok, s_f);
										}
										else
										{
											g.String(OKButton, ((Control)this).Font, (Brush)(object)val7, rect_buttonok, s_f);
										}
									}
								}
								if (hover_button.Animation)
								{
									g.String(button_text, ((Control)this).Font, val7.Color.BlendColors(hover_button.Value, Colour.PrimaryActive.Get("DatePicker")), rect_button, s_f);
								}
								else if (hover_button.Switch)
								{
									g.String(button_text, ((Control)this).Font, Colour.PrimaryActive.Get("DatePicker"), rect_button, s_f);
								}
								else
								{
									g.String(button_text, ((Control)this).Font, (Brush)(object)val7, rect_button, s_f);
								}
								if (badge_list.Count <= 0)
								{
									return;
								}
								foreach (Calendari data4 in datas)
								{
									if (badge_list.TryGetValue(data4.date_str, out DateBadge value))
									{
										control.PaintBadge(value, data4.rect, g);
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

	private void DrawShadow(Canvas g, Rectangle rect)
	{
		if (!Config.ShadowEnabled)
		{
			return;
		}
		if (shadow_temp == null)
		{
			Bitmap? obj = shadow_temp;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			GraphicsPath val = new Rectangle(10, 10, rect.Width - 20, rect.Height - 20).RoundPath(Radius);
			try
			{
				shadow_temp = val.PaintShadow(rect.Width, rect.Height);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Image(shadow_temp, rect, 0.2f);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		((Control)this).OnMouseDown(e);
		if ((left_buttons == null || !rect_read_left.Contains(e.X, e.Y) || scrollY_left.MouseDown(e.Location)) && ShowTime)
		{
			if (rect_read_h.Contains(e.X, e.Y))
			{
				scrollY_h.MouseDown(e.Location);
			}
			else if (rect_read_m.Contains(e.X, e.Y))
			{
				scrollY_m.MouseDown(e.Location);
			}
			else if (rect_read_s.Contains(e.X, e.Y))
			{
				scrollY_s.MouseDown(e.Location);
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		if (scrollY_left.MouseMove(e.Location) && scrollY_h.MouseMove(e.Location) && scrollY_m.MouseMove(e.Location) && scrollY_s.MouseMove(e.Location))
		{
			int num = 0;
			int num2 = 0;
			bool flag = rect_lefts.Contains(e.X, e.Y);
			bool flag2 = rect_rights.Contains(e.X, e.Y);
			bool flag3 = showType == 0 && rect_left.Contains(e.X, e.Y);
			bool flag4 = showType == 0 && rect_right.Contains(e.X, e.Y);
			bool flag5 = showType == 0 && rect_button.Contains(e.X, e.Y);
			bool flag6 = showType == 0 && ShowTime && rect_buttonok.Contains(e.X, e.Y);
			bool flag7 = false;
			bool flag8 = false;
			if (showType != 2)
			{
				flag7 = ((showType == 0) ? rect_year.Contains(e.X, e.Y) : rect_year2.Contains(e.X, e.Y));
				flag8 = rect_month.Contains(e.X, e.Y);
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
			if (flag7 != hover_year.Switch)
			{
				num++;
			}
			if (flag8 != hover_month.Switch)
			{
				num++;
			}
			if (flag5 != hover_button.Switch)
			{
				num++;
			}
			if (flag6 != hover_buttonok.Switch)
			{
				num++;
			}
			hover_lefts.Switch = flag;
			hover_left.Switch = flag3;
			hover_rights.Switch = flag2;
			hover_right.Switch = flag4;
			hover_year.Switch = flag7;
			hover_month.Switch = flag8;
			hover_button.Switch = flag5;
			hover_buttonok.Switch = flag6;
			if (hover_lefts.Switch || hover_left.Switch || hover_rights.Switch || hover_right.Switch || hover_year.Switch || hover_month.Switch || hover_button.Switch || hover_buttonok.Switch)
			{
				num2++;
			}
			else if (showType == 1)
			{
				if (calendar_month != null)
				{
					foreach (Calendari item in calendar_month)
					{
						bool flag9 = item.enable && item.rect.Contains(e.X, e.Y);
						if (item.hover != flag9)
						{
							num++;
						}
						item.hover = flag9;
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
						bool flag10 = item2.enable && item2.rect.Contains(e.X, e.Y);
						if (item2.hover != flag10)
						{
							num++;
						}
						item2.hover = flag10;
						if (item2.hover)
						{
							num2++;
						}
					}
				}
			}
			else
			{
				if (calendar_day != null)
				{
					foreach (Calendari item3 in calendar_day)
					{
						bool flag11 = item3.enable && item3.rect.Contains(e.X, e.Y);
						if (item3.hover != flag11)
						{
							num++;
						}
						item3.hover = flag11;
						if (item3.hover)
						{
							num2++;
						}
					}
				}
				if (left_buttons != null)
				{
					foreach (CalendarButton left_button in left_buttons)
					{
						if (left_button.Contains(e.Location, 0f, scrollY_left.Value, out var change))
						{
							num2++;
						}
						if (change)
						{
							num++;
						}
					}
				}
				if (calendar_time != null)
				{
					foreach (CalendarT item4 in calendar_time)
					{
						switch (item4.x)
						{
						case 1:
						{
							if (item4.Contains(e.Location, 0f, scrollY_m.Value, out var change4))
							{
								num2++;
							}
							if (change4)
							{
								num++;
							}
							break;
						}
						case 2:
						{
							if (item4.Contains(e.Location, 0f, scrollY_s.Value, out var change3))
							{
								num2++;
							}
							if (change3)
							{
								num++;
							}
							break;
						}
						default:
						{
							if (item4.Contains(e.Location, 0f, scrollY_h.Value, out var change2))
							{
								num2++;
							}
							if (change2)
							{
								num++;
							}
							break;
						}
						}
					}
				}
			}
			if (num > 0)
			{
				Print();
			}
			SetCursor(num2 > 0);
		}
		else
		{
			SetCursor(val: false);
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		scrollY_left.Leave();
		scrollY_h.Leave();
		scrollY_m.Leave();
		scrollY_s.Leave();
		hover_lefts.Switch = false;
		hover_left.Switch = false;
		hover_rights.Switch = false;
		hover_right.Switch = false;
		hover_year.Switch = false;
		hover_month.Switch = false;
		hover_button.Switch = false;
		hover_buttonok.Switch = false;
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
		if (calendar_time != null)
		{
			foreach (CalendarT item4 in calendar_time)
			{
				item4.hover = false;
			}
		}
		SetCursor(val: false);
		Print();
		((Control)this).OnMouseLeave(e);
	}

	private void CSize()
	{
		if (left_buttons != null)
		{
			t_x = ((showType == 0) ? left_button : 0);
			rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
			rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
			rect_rights = new Rectangle(t_x + t_one_width + 10 - t_top, 10, t_top, t_top);
			rect_right = new Rectangle(t_x + t_one_width + 10 - t_top * 2, 10, t_top, t_top);
			int num = (int)(4f * Config.Dpi);
			int num2 = t_one_width / 2;
			rect_year2 = new Rectangle(t_x + 10 + (t_one_width - year2_width) / 2, 10, year2_width, t_top);
			if (YDR)
			{
				rect_month = new Rectangle(t_x + 10 + num2 - year_width - num, 10, year_width, t_top);
				rect_year = new Rectangle(t_x + 10 + num2 + num, 10, month_width, t_top);
			}
			else
			{
				rect_year = new Rectangle(t_x + 10 + num2 - year_width - num, 10, year_width, t_top);
				rect_month = new Rectangle(t_x + 10 + num2 + num, 10, month_width, t_top);
			}
		}
		if (showType == 0)
		{
			SetSize(t_width + 20, t_h);
		}
		else
		{
			SetSize(t_one_width + 20, t_h);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		if (RunAnimation)
		{
			return;
		}
		scrollY_left.MouseUp(e.Location);
		scrollY_h.MouseUp(e.Location);
		scrollY_m.MouseUp(e.Location);
		scrollY_s.MouseUp(e.Location);
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
					Print();
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
					Print();
				}
				return;
			}
			if (showType == 0 && rect_left.Contains(e.X, e.Y))
			{
				if (hover_left.Enable)
				{
					Date = _Date.AddMonths(-1);
					Print();
				}
				return;
			}
			if (showType == 0 && rect_right.Contains(e.X, e.Y))
			{
				if (hover_right.Enable)
				{
					Date = _Date.AddMonths(1);
					Print();
				}
				return;
			}
			if ((showType == 0 && rect_year.Contains(e.X, e.Y)) || (showType != 0 && rect_year2.Contains(e.X, e.Y)))
			{
				showType = 2;
				if (ShowTime || left_buttons != null)
				{
					CSize();
				}
				Print();
				return;
			}
			if (showType == 0 && rect_button.Contains(e.X, e.Y))
			{
				DateTime value = (Date = (DateNow = DateTime.Now));
				SelDate = value;
				action(SelDate.Value);
				if (ShowTime && calendar_time != null)
				{
					ScrollYTime(calendar_time, SelDate.Value);
					Print();
				}
				else
				{
					IClose();
				}
				return;
			}
			if (showType == 0 && ShowTime && rect_buttonok.Contains(e.X, e.Y))
			{
				if (SelDate.HasValue)
				{
					action(SelDate.Value);
					IClose();
				}
				return;
			}
			if (rect_month.Contains(e.X, e.Y))
			{
				showType = 1;
				if (ShowTime || left_buttons != null)
				{
					CSize();
				}
				Print();
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
							showType = 0;
							if (ShowTime || left_buttons != null)
							{
								CSize();
							}
							Print();
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
							showType = 1;
							if (ShowTime || left_buttons != null)
							{
								CSize();
							}
							Print();
							return;
						}
					}
				}
			}
			else
			{
				if (calendar_day != null)
				{
					foreach (Calendari item3 in calendar_day)
					{
						if (!item3.enable || !item3.rect.Contains(e.X, e.Y))
						{
							continue;
						}
						if (ShowTime)
						{
							if (SelDate.HasValue)
							{
								SelDate = new DateTime(item3.date.Year, item3.date.Month, item3.date.Day, SelDate.Value.Hour, SelDate.Value.Minute, SelDate.Value.Second);
							}
							else
							{
								DateTime now = DateTime.Now;
								SelDate = new DateTime(item3.date.Year, item3.date.Month, item3.date.Day, now.Hour, now.Minute, now.Second);
							}
							action(SelDate.Value);
							if (calendar_time != null)
							{
								ScrollYTime(calendar_time, SelDate.Value);
							}
							Print();
						}
						else
						{
							SelDate = item3.date;
							action(SelDate.Value);
							IClose();
						}
						return;
					}
				}
				bool change;
				if (left_buttons != null)
				{
					foreach (CalendarButton left_button in left_buttons)
					{
						if (left_button.Contains(e.Location, 0f, scrollY_left.Value, out change))
						{
							action_btns(left_button.Tag);
							IClose();
							return;
						}
					}
				}
				if (calendar_time != null)
				{
					foreach (CalendarT item4 in calendar_time)
					{
						switch (item4.x)
						{
						case 1:
							if (item4.Contains(e.Location, 0f, scrollY_m.Value, out change))
							{
								if (SelDate.HasValue)
								{
									SelDate = new DateTime(SelDate.Value.Year, SelDate.Value.Month, SelDate.Value.Day, SelDate.Value.Hour, item4.t, SelDate.Value.Second);
								}
								if (ValueTimeHorizontal && SelDate.HasValue)
								{
									ScrollYTime(calendar_time, SelDate.Value);
								}
								Print();
								return;
							}
							continue;
						case 2:
							if (item4.Contains(e.Location, 0f, scrollY_s.Value, out change))
							{
								if (SelDate.HasValue)
								{
									SelDate = new DateTime(SelDate.Value.Year, SelDate.Value.Month, SelDate.Value.Day, SelDate.Value.Hour, SelDate.Value.Minute, item4.t);
								}
								if (ValueTimeHorizontal && SelDate.HasValue)
								{
									ScrollYTime(calendar_time, SelDate.Value);
								}
								Print();
								return;
							}
							continue;
						}
						if (item4.Contains(e.Location, 0f, scrollY_h.Value, out change))
						{
							if (SelDate.HasValue)
							{
								SelDate = new DateTime(SelDate.Value.Year, SelDate.Value.Month, SelDate.Value.Day, item4.t, SelDate.Value.Minute, SelDate.Value.Second);
							}
							if (ValueTimeHorizontal && SelDate.HasValue)
							{
								ScrollYTime(calendar_time, SelDate.Value);
							}
							Print();
							return;
						}
					}
				}
			}
		}
		((Control)this).OnMouseUp(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		if (e.Delta != 0)
		{
			if (left_buttons != null && rect_read_left.Contains(e.X, e.Y))
			{
				scrollY_left.MouseWheel(e.Delta);
				Print();
				base.OnMouseWheel(e);
				return;
			}
			if (ShowTime)
			{
				if (rect_read_h.Contains(e.X, e.Y))
				{
					scrollY_h.MouseWheel(e.Delta);
					Print();
				}
				else if (rect_read_m.Contains(e.X, e.Y))
				{
					scrollY_m.MouseWheel(e.Delta);
					Print();
				}
				else if (rect_read_s.Contains(e.X, e.Y))
				{
					scrollY_s.MouseWheel(e.Delta);
					Print();
				}
				else
				{
					MouseWheelDay(e);
				}
			}
			else
			{
				MouseWheelDay(e);
			}
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
			Print();
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
		Print();
	}

	private void ScrollYTime(List<CalendarT> calendar_time, DateTime d)
	{
		CalendarT calendarT = calendar_time.Find((CalendarT a) => a.x == 0 && a.t == d.Hour);
		CalendarT calendarT2 = calendar_time.Find((CalendarT a) => a.x == 1 && a.t == d.Minute);
		CalendarT calendarT3 = calendar_time.Find((CalendarT a) => a.x == 2 && a.t == d.Second);
		int num = 4;
		if (calendarT != null)
		{
			scrollY_h.Value = calendarT.rect.Y - num;
		}
		if (calendarT2 != null)
		{
			scrollY_m.Value = calendarT2.rect.Y - num;
		}
		if (calendarT3 != null)
		{
			scrollY_s.Value = calendarT3.rect.Y - num;
		}
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
		hover_buttonok?.Dispose();
		base.Dispose(disposing);
	}
}
