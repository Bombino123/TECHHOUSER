using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace AntdUI;

public class LayeredFormCalendarRange : ILayeredFormOpacityDown
{
	private DateTime? minDate;

	private DateTime? maxDate;

	private IControl control;

	private int Radius = 6;

	private int t_one_width = 288;

	private int t_width = 288;

	private int t_h;

	private int t_x;

	private int left_button = 120;

	private int t_top = 34;

	private int t_time = 56;

	private int t_time_height = 30;

	private int year_width = 60;

	private int year2_width = 90;

	private int month_width = 60;

	private TAlignFrom Placement = TAlignFrom.BL;

	private TAlign ArrowAlign;

	private int ArrowSize = 8;

	private List<CalendarButton>? left_buttons;

	private ScrollY scrollY_left;

	private CultureInfo Culture;

	private string CultureID = Localization.Get("ID", "zh-CN");

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

	private Action<DateTime[]> action;

	private Action<object> action_btns;

	private Func<DateTime[], List<DateBadge>?>? badge_action;

	private Dictionary<string, DateBadge> badge_list = new Dictionary<string, DateBadge>();

	public DateTime[]? SelDate;

	private DateTime _Date;

	private DateTime _Date_R;

	private DateTime DateNow = DateTime.Now;

	private List<Calendari>? calendar_year;

	private List<Calendari>? calendar_month;

	private List<Calendari>? calendar_day;

	private List<Calendari>? calendar_day2;

	private string year_str = "";

	private bool sizeday = true;

	private bool size_month = true;

	private bool size_year = true;

	private ITaskOpacity hover_lefts;

	private ITaskOpacity hover_left;

	private ITaskOpacity hover_rights;

	private ITaskOpacity hover_right;

	private ITaskOpacity hover_year;

	private ITaskOpacity hover_month;

	private ITaskOpacity hover_year_r;

	private ITaskOpacity hover_month_r;

	private Rectangle rect_lefts = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_left = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_rights = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_right = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year2 = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_month = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_year_r = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_month_r = new Rectangle(-20, -20, 10, 10);

	private int showType;

	private bool isEnd;

	private DateTime? oldTime;

	private DateTime? oldTimeHover;

	private float AnimationBarValue;

	private StringFormat s_f = Helper.SF((StringAlignment)1, (StringAlignment)1);

	private StringFormat s_f_LE = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)0);

	private StringFormat s_f_L;

	private StringFormat s_f_R;

	private Rectangle rect_read_left;

	private Bitmap? shadow_temp;

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
			_Date_R = value.AddMonths(1);
			sizeday = (size_month = (size_year = true));
			calendar_day = GetCalendar(value);
			calendar_day2 = GetCalendar(_Date_R);
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

	public LayeredFormCalendarRange(DatePickerRange _control, Rectangle rect_read, DateTime[]? date, Action<DateTime[]> _action, Action<object> _action_btns, Func<DateTime[], List<DateBadge>?>? _badge_action = null)
	{
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b1: Expected O, but got Unknown
		((Control)_control).Parent.SetTopMost(((Control)this).Handle);
		control = _control;
		minDate = _control.MinDate;
		maxDate = _control.MaxDate;
		badge_action = _badge_action;
		PARENT = (Control?)(object)_control;
		action = _action;
		action_btns = _action_btns;
		hover_lefts = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_left = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_rights = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_right = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_year = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_month = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_year_r = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_month_r = new ITaskOpacity((ILayeredFormOpacityDown)this);
		scrollY_left = new ScrollY((ILayeredForm)this);
		Culture = new CultureInfo(CultureID);
		YDR = CultureID.StartsWith("en");
		float dpi = Config.Dpi;
		if (dpi != 1f)
		{
			t_one_width = (int)((float)t_one_width * dpi);
			t_top = (int)((float)t_top * dpi);
			t_time = (int)((float)t_time * dpi);
			t_time_height = (int)((float)t_time_height * dpi);
			left_button = (int)((float)left_button * dpi);
			year_width = (int)((float)year_width * dpi);
			year2_width = (int)((float)year2_width * dpi);
			month_width = (int)((float)month_width * dpi);
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
		t_width = t_x + t_one_width * 2;
		rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
		rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
		rect_rights = new Rectangle(t_width + 10 - t_top, 10, t_top, t_top);
		rect_right = new Rectangle(t_width + 10 - t_top * 2, 10, t_top, t_top);
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
		rect_year_r = new Rectangle(rect_year.Left + t_one_width, rect_year.Y, rect_year.Width, rect_year.Height);
		rect_month_r = new Rectangle(rect_month.Left + t_one_width, rect_month.Y, rect_month.Width, rect_month.Height);
		((Control)this).Font = new Font(((Control)_control).Font.FontFamily, 11.2f);
		SelDate = date;
		Date = ((date == null) ? DateNow : date[0]);
		Point point = ((Control)_control).PointToScreen(Point.Empty);
		int num4 = t_width + 20;
		int num5 = ((calendar_day != null) ? (t_top + 24 + (int)Math.Ceiling((float)((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16)) / 7f) + 20) : 368);
		SetSize(num4, num5);
		t_h = num5;
		Placement = _control.Placement;
		CLocation(point, _control.Placement, _control.DropDownArrow, 10, num4, num5, rect_read, ref Inverted, ref ArrowAlign);
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

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (!RunAnimation)
		{
			((Control)this).OnMouseDown(e);
			if (left_buttons != null && rect_read_left.Contains(e.X, e.Y))
			{
				scrollY_left.MouseDown(e.Location);
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		if (scrollY_left.MouseMove(e.Location))
		{
			int num = 0;
			int num2 = 0;
			bool flag = rect_lefts.Contains(e.X, e.Y);
			bool flag2 = rect_rights.Contains(e.X, e.Y);
			bool flag3 = showType == 0 && rect_left.Contains(e.X, e.Y);
			bool flag4 = showType == 0 && rect_right.Contains(e.X, e.Y);
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			if (showType != 2)
			{
				flag5 = ((showType == 0) ? rect_year.Contains(e.X, e.Y) : rect_year2.Contains(e.X, e.Y));
				flag6 = rect_month.Contains(e.X, e.Y);
				flag7 = rect_year_r.Contains(e.X, e.Y);
				flag8 = rect_month_r.Contains(e.X, e.Y);
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
			if (flag5 != hover_year.Switch)
			{
				num++;
			}
			if (flag6 != hover_month.Switch)
			{
				num++;
			}
			if (flag7 != hover_year_r.Switch)
			{
				num++;
			}
			if (flag8 != hover_month_r.Switch)
			{
				num++;
			}
			hover_lefts.Switch = flag;
			hover_left.Switch = flag3;
			hover_rights.Switch = flag2;
			hover_right.Switch = flag4;
			hover_year.Switch = flag5;
			hover_month.Switch = flag6;
			hover_year_r.Switch = flag7;
			hover_month_r.Switch = flag8;
			if (hover_lefts.Switch || hover_left.Switch || hover_rights.Switch || hover_right.Switch || hover_year.Switch || hover_month.Switch || hover_year_r.Switch || hover_month_r.Switch)
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
							if (isEnd)
							{
								oldTimeHover = item3.date;
							}
							num2++;
						}
					}
				}
				if (calendar_day2 != null)
				{
					foreach (Calendari item4 in calendar_day2)
					{
						bool flag12 = item4.enable && item4.rect.Contains(e.X, e.Y);
						if (item4.hover != flag12)
						{
							num++;
						}
						item4.hover = flag12;
						if (item4.hover)
						{
							if (isEnd)
							{
								oldTimeHover = item4.date;
							}
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
		hover_lefts.Switch = false;
		hover_left.Switch = false;
		hover_rights.Switch = false;
		hover_right.Switch = false;
		hover_year.Switch = false;
		hover_month.Switch = false;
		hover_year_r.Switch = false;
		hover_month_r.Switch = false;
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
		if (calendar_day2 != null)
		{
			foreach (Calendari item4 in calendar_day2)
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
		}
		int h;
		if (showType == 0)
		{
			t_width = t_x + t_one_width * 2;
			h = ((calendar_day != null) ? (t_top * 2 + 24 + (int)Math.Ceiling((float)((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16)) / 7f) + 20) : 368);
		}
		else
		{
			t_width = t_x + t_one_width;
			h = ((calendar_day != null) ? (t_top * 2 + 24 + (int)Math.Ceiling((float)((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16)) / 7f) + 20) : 368);
		}
		SetSize(t_width + 20, h);
		if (showType == 0)
		{
			rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
			rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
			rect_rights = new Rectangle(t_width + 10 - t_top, 10, t_top, t_top);
			rect_right = new Rectangle(t_width + 10 - t_top * 2, 10, t_top, t_top);
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
			rect_year_r = new Rectangle(rect_year.Left + t_one_width, rect_year.Y, rect_year.Width, rect_year.Height);
			rect_month_r = new Rectangle(rect_month.Left + t_one_width, rect_month.Y, rect_month.Width, rect_month.Height);
		}
		else
		{
			rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
			rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
			rect_rights = new Rectangle(t_one_width + 10 - t_top, 10, t_top, t_top);
			rect_right = new Rectangle(t_one_width + 10 - t_top * 2, 10, t_top, t_top);
			rect_year = new Rectangle(t_x + 10 + t_one_width / 2 - year_width, 10, year_width, t_top);
			rect_year2 = new Rectangle(t_x + 10 + (t_one_width - year2_width) / 2, 10, year2_width, t_top);
			rect_month = new Rectangle(t_x + 10 + t_one_width / 2, 10, month_width, t_top);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (RunAnimation)
		{
			return;
		}
		scrollY_left.MouseUp(e.Location);
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
			if ((showType == 0 && (rect_year.Contains(e.X, e.Y) || rect_year_r.Contains(e.X, e.Y))) || (showType != 0 && rect_year2.Contains(e.X, e.Y)))
			{
				showType = 2;
				CSize();
				Print();
				return;
			}
			if (rect_month.Contains(e.X, e.Y) || rect_month_r.Contains(e.X, e.Y))
			{
				showType = 1;
				CSize();
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
							CSize();
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
							CSize();
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
						if (item3.enable && item3.rect.Contains(e.X, e.Y))
						{
							if (!SetDate(item3))
							{
								IClose();
							}
							return;
						}
					}
				}
				if (calendar_day2 != null)
				{
					foreach (Calendari item4 in calendar_day2)
					{
						if (item4.enable && item4.rect.Contains(e.X, e.Y))
						{
							if (!SetDate(item4))
							{
								IClose();
							}
							return;
						}
					}
				}
				if (left_buttons != null)
				{
					foreach (CalendarButton left_button in left_buttons)
					{
						if (left_button.Contains(e.Location, 0f, scrollY_left.Value, out var _))
						{
							action_btns(left_button.Tag);
							IClose();
							return;
						}
					}
				}
			}
		}
		((Control)this).OnMouseUp(e);
	}

	private bool SetDate(Calendari item)
	{
		if (isEnd && oldTime.HasValue)
		{
			SetDateE(oldTime.Value, item.date);
			return false;
		}
		SetDateS(item.date);
		Print();
		return true;
	}

	public void SetDateS(DateTime date)
	{
		SelDate = null;
		oldTimeHover = (oldTime = date);
		isEnd = true;
	}

	public void SetDateE(DateTime sdate, DateTime edate)
	{
		if (sdate == edate)
		{
			SelDate = new DateTime[2] { edate, edate };
		}
		else if (sdate < edate)
		{
			SelDate = new DateTime[2] { sdate, edate };
		}
		else
		{
			SelDate = new DateTime[2] { edate, sdate };
		}
		action(SelDate);
		isEnd = false;
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

	public override void LoadOK()
	{
		CanLoadMessage = true;
		LoadMessage();
	}

	public void SetArrow(float x)
	{
		if (AnimationBarValue != x)
		{
			AnimationBarValue = x;
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

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Expected O, but got Unknown
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0754: Expected O, but got Unknown
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0834: Expected O, but got Unknown
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
						if (AnimationBarValue != 0f)
						{
							canvas.FillPolygon((Brush)(object)val3, ArrowAlign.AlignLines(ArrowSize, targetRectXY, new RectangleF((float)rectangle.X + AnimationBarValue, rectangle.Y, rectangle.Width, rectangle.Height)));
						}
						else
						{
							canvas.FillPolygon((Brush)(object)val3, ArrowAlign.AlignLines(ArrowSize, targetRectXY, rectangle));
						}
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
			else if (calendar_day != null && calendar_day2 != null)
			{
				PrintDay(canvas, rectangle, calendar_day, calendar_day2);
			}
		}
		return val;
	}

	private void PrintYear(Canvas g, Rectangle rect_read, List<Calendari> datas)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		Color color = Colour.TextQuaternary.Get("DatePicker");
		Color color2 = Colour.FillTertiary.Get("DatePicker");
		Color color3 = Colour.TextBase.Get("DatePicker");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			Rectangle rect = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width, t_top);
			if (hover_year.Animation)
			{
				g.String(year_str, val, color3.BlendColors(hover_year.Value, Colour.Primary.Get("DatePicker")), rect, s_f);
			}
			else if (hover_year.Switch)
			{
				g.String(year_str, val, Colour.Primary.Get("DatePicker"), rect, s_f);
			}
			else
			{
				g.String(year_str, val, color3, rect, s_f);
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
		foreach (Calendari data2 in datas)
		{
			GraphicsPath val2 = data2.rect_read.RoundPath(Radius);
			try
			{
				if (SelDate != null && (SelDate[0].ToString("yyyy") == data2.date_str || (SelDate.Length > 1 && SelDate[1].ToString("yyyy") == data2.date_str)))
				{
					g.Fill(Colour.Primary.Get("DatePicker"), val2);
					g.String(data2.v, ((Control)this).Font, Colour.PrimaryColor.Get("DatePicker"), data2.rect, s_f);
				}
				else if (data2.enable)
				{
					if (data2.hover)
					{
						g.Fill(Colour.FillTertiary.Get("DatePicker"), val2);
					}
					if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
					{
						g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val2);
					}
					g.String(data2.v, ((Control)this).Font, (data2.t == 1) ? color3 : color, data2.rect, s_f);
				}
				else
				{
					g.Fill(color2, new Rectangle(data2.rect.X, data2.rect_read.Y, data2.rect.Width, data2.rect_read.Height));
					if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
					{
						g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val2);
					}
					g.String(data2.v, ((Control)this).Font, color, data2.rect, s_f);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	private void PrintMonth(Canvas g, Rectangle rect_read, List<Calendari> datas)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		Color color = Colour.TextQuaternary.Get("DatePicker");
		Color color2 = Colour.FillTertiary.Get("DatePicker");
		Color color3 = Colour.TextBase.Get("DatePicker");
		Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
		try
		{
			Rectangle rect = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width, t_top);
			string text = _Date.ToString(YearFormat, Culture);
			if (hover_year.Animation)
			{
				g.String(text, val, color3.BlendColors(hover_year.Value, Colour.Primary.Get("DatePicker")), rect, s_f);
			}
			else if (hover_year.Switch)
			{
				g.String(text, val, Colour.Primary.Get("DatePicker"), rect, s_f);
			}
			else
			{
				g.String(text, val, color3, rect, s_f);
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
		foreach (Calendari data2 in datas)
		{
			GraphicsPath val2 = data2.rect_read.RoundPath(Radius);
			try
			{
				if (SelDate != null && (SelDate[0].ToString("yyyy-MM") == data2.date_str || (SelDate.Length > 1 && SelDate[1].ToString("yyyy-MM") == data2.date_str)))
				{
					g.Fill(Colour.Primary.Get("DatePicker"), val2);
					g.String(data2.v, ((Control)this).Font, Colour.PrimaryColor.Get("DatePicker"), data2.rect, s_f);
				}
				else if (data2.enable)
				{
					if (data2.hover)
					{
						g.Fill(Colour.FillTertiary.Get("DatePicker"), val2);
					}
					if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
					{
						g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val2);
					}
					g.String(data2.v, ((Control)this).Font, color3, data2.rect, s_f);
				}
				else
				{
					g.Fill(color2, new Rectangle(data2.rect.X, data2.rect_read.Y, data2.rect.Width, data2.rect_read.Height));
					if (DateNow.ToString("yyyy-MM-dd") == data2.date_str)
					{
						g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val2);
					}
					g.String(data2.v, ((Control)this).Font, color, data2.rect, s_f);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	private void PrintDay(Canvas g, Rectangle rect_read, List<Calendari> datas, List<Calendari> datas2)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Expected O, but got Unknown
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
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
			string text3 = _Date_R.ToString(YearFormat, Culture);
			string text4 = _Date_R.ToString(MonthFormat, Culture);
			if (hover_year_r.Animation)
			{
				g.String(text3, val, color.BlendColors(hover_year_r.Value, Colour.Primary.Get("DatePicker")), rect_year_r, s_f_L);
			}
			else if (hover_year_r.Switch)
			{
				g.String(text3, val, Colour.Primary.Get("DatePicker"), rect_year_r, s_f_L);
			}
			else
			{
				g.String(text3, val, color, rect_year_r, s_f_L);
			}
			if (hover_month_r.Animation)
			{
				g.String(text4, val, color.BlendColors(hover_month_r.Value, Colour.Primary.Get("DatePicker")), rect_month_r, s_f_R);
			}
			else if (hover_month_r.Switch)
			{
				g.String(text4, val, Colour.Primary.Get("DatePicker"), rect_month_r, s_f_R);
			}
			else
			{
				g.String(text4, val, color, rect_month_r, s_f_R);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		SolidBrush val2 = new SolidBrush(Colour.Split.Get("DatePicker"));
		try
		{
			g.Fill((Brush)(object)val2, new RectangleF(t_x + rect_read.X, rect_read.Y + t_top, t_width - t_x, Config.Dpi));
			if (left_buttons != null)
			{
				g.Fill((Brush)(object)val2, new RectangleF(t_x + rect_read.X, rect_read.Y, 1f, rect_read.Height));
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		int num = rect_read.Y + t_top + 12;
		int num2 = (t_one_width - 16) / 7;
		int num3 = t_x + rect_read.X + 8;
		int num4 = t_x + rect_read.X + t_one_width + 8;
		SolidBrush val3 = new SolidBrush(Colour.Text.Get("DatePicker"));
		try
		{
			g.String(MondayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3, num, num2, num2), s_f);
			g.String(TuesdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3 + num2, num, num2, num2), s_f);
			g.String(WednesdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3 + num2 * 2, num, num2, num2), s_f);
			g.String(ThursdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3 + num2 * 3, num, num2, num2), s_f);
			g.String(FridayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3 + num2 * 4, num, num2, num2), s_f);
			g.String(SaturdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3 + num2 * 5, num, num2, num2), s_f);
			g.String(SundayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num3 + num2 * 6, num, num2, num2), s_f);
			g.String(MondayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4, num, num2, num2), s_f);
			g.String(TuesdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4 + num2, num, num2, num2), s_f);
			g.String(WednesdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4 + num2 * 2, num, num2, num2), s_f);
			g.String(ThursdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4 + num2 * 3, num, num2, num2), s_f);
			g.String(FridayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4 + num2 * 4, num, num2, num2), s_f);
			g.String(SaturdayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4 + num2 * 5, num, num2, num2), s_f);
			g.String(SundayButton, ((Control)this).Font, (Brush)(object)val3, new Rectangle(num4 + num2 * 6, num, num2, num2), s_f);
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
			foreach (Calendari item in datas2)
			{
				item.SetRect(new Rectangle(t_x + rect_read.X + t_one_width + 8 + num2 * item.x, num + num2 * item.y, num2, num2), gap);
			}
			if (left_buttons != null)
			{
				int num5 = (int)((float)left_button * 0.9f);
				int num6 = (int)((float)t_time_height * 0.93f);
				int num7 = (int)((float)left_button * 0.8f);
				rect_read_left = new Rectangle(rect_read.X, rect_read.Y, t_x, t_h - rect_read.Y * 2);
				scrollY_left.SizeChange(new Rectangle(rect_read.X, rect_read.Y + 8, t_x, t_h - (8 + rect_read.Y) * 2));
				scrollY_left.SetVrSize(t_time_height * left_buttons.Count, t_h - 20 - rect_read.Y * 2);
				int num8 = (left_button - num5) / 2;
				int num9 = (num5 - num7) / 2;
				int num10 = rect_read.Y + (t_time_height - num6) / 2;
				foreach (CalendarButton left_button in left_buttons)
				{
					Rectangle rectangle = new Rectangle(0, t_time_height * left_button.y, this.left_button, t_time_height);
					left_button.rect_read = new Rectangle(rectangle.X + num8, rectangle.Y + num10, num5, num6);
					left_button.rect = new Rectangle(rect_read.X + rectangle.X, rect_read.Y + rectangle.Y, rectangle.Width, rectangle.Height);
					left_button.rect_text = new Rectangle(rect_read.X + num9, left_button.rect_read.Y, num7, left_button.rect_read.Height);
				}
			}
		}
		Color color_fore_disable = Colour.TextQuaternary.Get("DatePicker");
		Color color_bg_disable = Colour.FillTertiary.Get("DatePicker");
		Color color2 = Colour.Primary.Get("DatePicker");
		Color color3 = Colour.PrimaryBg.Get("DatePicker");
		Color color_fore_active = Colour.PrimaryColor.Get("DatePicker");
		if (oldTimeHover.HasValue && oldTime.HasValue)
		{
			if (oldTimeHover.Value != oldTime.Value && oldTimeHover.Value > oldTime.Value)
			{
				PrintCalendarMutual(g, oldTime.Value, oldTimeHover.Value, color2, color3, datas);
				PrintCalendarMutual(g, oldTime.Value, oldTimeHover.Value, color2, color3, datas2);
			}
			else
			{
				foreach (Calendari data2 in datas)
				{
					if (data2.t == 1 && data2.date == oldTime.Value)
					{
						GraphicsPath val4 = data2.rect_read.RoundPath(Radius, TL: true, TR: false, BR: false, BL: true);
						try
						{
							g.Fill(color2, val4);
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
				}
				foreach (Calendari item2 in datas2)
				{
					if (item2.t == 1 && item2.date == oldTime.Value)
					{
						GraphicsPath val5 = item2.rect_read.RoundPath(Radius, TL: true, TR: false, BR: false, BL: true);
						try
						{
							g.Fill(color2, val5);
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
				}
			}
		}
		PrintCalendar(g, color, color_fore_disable, color_bg_disable, color2, color3, color_fore_active, datas);
		PrintCalendar(g, color, color_fore_disable, color_bg_disable, color2, color3, color_fore_active, datas2);
		if (rect_read.Height <= t_time_height || left_buttons == null)
		{
			return;
		}
		GraphicsState state = g.Save();
		g.SetClip(new Rectangle(rect_read.X, rect_read.Y, this.left_button, rect_read.Height));
		g.TranslateTransform(rect_read.X, (float)rect_read.Y - scrollY_left.Value);
		foreach (CalendarButton left_button2 in left_buttons)
		{
			GraphicsPath val6 = left_button2.rect_read.RoundPath(Radius);
			try
			{
				if (left_button2.hover)
				{
					g.Fill(Colour.FillTertiary.Get("DatePicker"), val6);
				}
				g.String(left_button2.v, ((Control)this).Font, color, left_button2.rect_text, s_f_LE);
			}
			finally
			{
				((IDisposable)val6)?.Dispose();
			}
		}
		g.Restore(state);
		scrollY_left.Paint(g);
	}

	private void PrintCalendarMutual(Canvas g, DateTime oldTime, DateTime oldTimeHover, Color brush_bg_active, Color brush_bg_activebg, List<Calendari> datas)
	{
		foreach (Calendari data in datas)
		{
			if (data.t != 1)
			{
				continue;
			}
			if (data.date > oldTime && data.date < oldTimeHover)
			{
				g.Fill(brush_bg_activebg, new RectangleF((float)data.rect.X - 1f, data.rect_read.Y, (float)data.rect.Width + 2f, data.rect_read.Height));
			}
			else if (data.date == oldTime)
			{
				g.Fill(brush_bg_activebg, new RectangleF(data.rect_read.Right, data.rect_read.Y, data.rect.Width - data.rect_read.Width, data.rect_read.Height));
				GraphicsPath val = data.rect_read.RoundPath(Radius, TL: true, TR: false, BR: false, BL: true);
				try
				{
					g.Fill(brush_bg_active, val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else if (data.date == oldTimeHover)
			{
				g.Fill(brush_bg_activebg, new RectangleF(data.rect.X, data.rect_read.Y, data.rect_read.Width, data.rect_read.Height));
				GraphicsPath val2 = data.rect_read.RoundPath(Radius, TL: false, TR: true, BR: true, BL: false);
				try
				{
					g.Fill(brush_bg_active, val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}
	}

	private void PrintCalendar(Canvas g, Color color_fore, Color color_fore_disable, Color color_bg_disable, Color color_bg_active, Color color_bg_activebg, Color color_fore_active, List<Calendari> datas)
	{
		foreach (Calendari data in datas)
		{
			GraphicsPath val = data.rect_read.RoundPath(Radius);
			try
			{
				bool flag = true;
				if (data.t == 1 && SelDate != null)
				{
					if (SelDate.Length > 1)
					{
						if (SelDate[0] == SelDate[1])
						{
							if (SelDate[0].ToString("yyyy-MM-dd") == data.date_str)
							{
								g.Fill(color_bg_active, val);
								g.String(data.v, ((Control)this).Font, color_fore_active, data.rect, s_f);
								flag = false;
							}
						}
						else if (SelDate[0] <= data.date && SelDate[1] >= data.date)
						{
							if (SelDate[0].ToString("yyyy-MM-dd") == data.date_str)
							{
								g.Fill(color_bg_activebg, new RectangleF(data.rect_read.Right, data.rect_read.Y, data.rect.Width - data.rect_read.Width, data.rect_read.Height));
								GraphicsPath val2 = data.rect_read.RoundPath(Radius, TL: true, TR: false, BR: false, BL: true);
								try
								{
									g.Fill(color_bg_active, val2);
								}
								finally
								{
									((IDisposable)val2)?.Dispose();
								}
								g.String(data.v, ((Control)this).Font, color_fore_active, data.rect, s_f);
							}
							else if (SelDate[1].ToString("yyyy-MM-dd") == data.date_str)
							{
								g.Fill(color_bg_activebg, new RectangleF(data.rect.X, data.rect_read.Y, data.rect_read.Width, data.rect_read.Height));
								GraphicsPath val3 = data.rect_read.RoundPath(Radius, TL: false, TR: true, BR: true, BL: false);
								try
								{
									g.Fill(color_bg_active, val3);
								}
								finally
								{
									((IDisposable)val3)?.Dispose();
								}
								g.String(data.v, ((Control)this).Font, color_fore_active, data.rect, s_f);
							}
							else
							{
								g.Fill(color_bg_activebg, new RectangleF((float)data.rect.X - 1f, data.rect_read.Y, (float)data.rect.Width + 2f, data.rect_read.Height));
								g.String(data.v, ((Control)this).Font, color_fore, data.rect, s_f);
							}
							flag = false;
						}
					}
					else if (SelDate[0].ToString("yyyy-MM-dd") == data.date_str)
					{
						g.Fill(color_bg_active, val);
						g.String(data.v, ((Control)this).Font, color_fore_active, data.rect, s_f);
						flag = false;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (oldTimeHover.HasValue && oldTime.HasValue && data.date < oldTime.Value)
				{
					g.Fill(color_bg_disable, new RectangleF(data.rect.X, data.rect_read.Y, data.rect.Width, data.rect_read.Height));
					g.String(data.v, ((Control)this).Font, color_fore_disable, data.rect, s_f);
				}
				else if (oldTimeHover.HasValue && oldTime.HasValue && data.t == 1 && (data.date == oldTime.Value || data.date == oldTimeHover.Value))
				{
					g.String(data.v, ((Control)this).Font, color_fore_active, data.rect, s_f);
				}
				else if (data.enable)
				{
					if (data.hover)
					{
						g.Fill(color_bg_disable, val);
					}
					g.String(data.v, ((Control)this).Font, (data.t == 1) ? color_fore : color_fore_disable, data.rect, s_f);
				}
				else
				{
					g.Fill(color_bg_disable, new Rectangle(data.rect.X, data.rect_read.Y, data.rect.Width, data.rect_read.Height));
					g.String(data.v, ((Control)this).Font, color_fore_disable, data.rect, s_f);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (badge_list.Count > 0)
		{
			foreach (Calendari data2 in datas)
			{
				if (badge_list.TryGetValue(data2.date_str, out DateBadge value))
				{
					control.PaintBadge(value, data2.rect, g);
				}
			}
		}
		string text = DateNow.ToString("yyyy-MM-dd");
		if (oldTimeHover.HasValue && oldTime.HasValue && (oldTime.Value.ToString("yyyy-MM-dd") == text || oldTimeHover.Value.ToString("yyyy-MM-dd") == text))
		{
			return;
		}
		if (SelDate != null && SelDate.Length != 0)
		{
			if (SelDate.Length > 1)
			{
				if (SelDate[1].ToString("yyyy-MM-dd") == text)
				{
					return;
				}
			}
			else if (SelDate[0].ToString("yyyy-MM-dd") == text)
			{
				return;
			}
		}
		foreach (Calendari data3 in datas)
		{
			if (text == data3.date_str)
			{
				GraphicsPath val4 = data3.rect_read.RoundPath(Radius);
				try
				{
					g.Draw(Colour.Primary.Get("DatePicker"), Config.Dpi, val4);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
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

	protected override void Dispose(bool disposing)
	{
		hover_lefts?.Dispose();
		hover_left?.Dispose();
		hover_rights?.Dispose();
		hover_right?.Dispose();
		hover_year?.Dispose();
		hover_month?.Dispose();
		hover_year_r?.Dispose();
		hover_month_r?.Dispose();
		base.Dispose(disposing);
	}
}
