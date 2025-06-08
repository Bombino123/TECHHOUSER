using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

public class LayeredFormCalendarTime : ILayeredFormOpacityDown
{
	private bool ShowSecond = true;

	private bool ValueTimeHorizontal;

	internal TimeSpan SelDate;

	private IControl control;

	private float Radius = 6f;

	private int t_width;

	private int t_button = 38;

	private int t_time = 56;

	private int t_height = 210;

	private int t_time_height = 30;

	private TAlign ArrowAlign;

	private int ArrowSize = 8;

	private ScrollY scrollY_h;

	private ScrollY scrollY_m;

	private ScrollY scrollY_s;

	private Action<TimeSpan> action;

	private DateTime DateNow = DateTime.Now;

	private List<CalendarT> calendar_time;

	private string button_text = Localization.Get("Now", "此刻");

	private string OKButton = Localization.Get("OK", "确定");

	private StringFormat s_f = Helper.SF((StringAlignment)1, (StringAlignment)1);

	private Rectangle rect_read_h;

	private Rectangle rect_read_m;

	private Rectangle rect_read_s;

	private Bitmap? shadow_temp;

	private ITaskOpacity hover_button;

	private ITaskOpacity hover_buttonok;

	private Rectangle rect_button = new Rectangle(-20, -20, 10, 10);

	private Rectangle rect_buttonok = new Rectangle(-20, -20, 10, 10);

	public LayeredFormCalendarTime(TimePicker _control, Rectangle rect_read, TimeSpan date, Action<TimeSpan> _action)
	{
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		((Control)_control).Parent.SetTopMost(((Control)this).Handle);
		control = _control;
		ValueTimeHorizontal = _control.ValueTimeHorizontal;
		ShowSecond = _control.Format.Contains("s");
		PARENT = (Control?)(object)_control;
		action = _action;
		scrollY_h = new ScrollY((ILayeredForm)this);
		scrollY_m = new ScrollY((ILayeredForm)this);
		scrollY_s = new ScrollY((ILayeredForm)this);
		hover_button = new ITaskOpacity((ILayeredFormOpacityDown)this);
		hover_buttonok = new ITaskOpacity((ILayeredFormOpacityDown)this);
		calendar_time = new List<CalendarT>(144);
		for (int i = 0; i < 24; i++)
		{
			calendar_time.Add(new CalendarT(0, i, i));
		}
		for (int j = 0; j < 60; j++)
		{
			calendar_time.Add(new CalendarT(1, j, j));
		}
		if (ShowSecond)
		{
			for (int k = 0; k < 60; k++)
			{
				calendar_time.Add(new CalendarT(2, k, k));
			}
		}
		float dpi = Config.Dpi;
		if (dpi != 1f)
		{
			Radius = (float)_control.Radius * dpi;
			t_time = (int)((float)t_time * dpi);
			t_time_height = (int)((float)t_time_height * dpi);
			t_button = (int)((float)t_button * dpi);
			t_height = t_time_height * 7;
		}
		else
		{
			Radius = _control.Radius;
		}
		t_width = t_time * (ShowSecond ? 3 : 2);
		((Control)this).Font = new Font(((Control)_control).Font.FontFamily, 11.2f);
		SelDate = date;
		Point point = ((Control)_control).PointToScreen(Point.Empty);
		int num = t_width + 20;
		int num2 = t_height + t_button + 20;
		SetSize(num, num2);
		int num3 = (int)((float)t_time * 0.857f);
		int num4 = (int)((float)t_time_height * 0.93f);
		Rectangle rect = new Rectangle(10, 18, t_time, t_height - 8);
		rect_read_h = new Rectangle(rect.Right - t_time, rect.Y, t_time, rect.Height);
		rect_read_m = new Rectangle(rect.Right, rect.Y, t_time, rect.Height);
		rect_read_s = new Rectangle(rect.Right + t_time, rect.Y, t_time, rect.Height);
		scrollY_h.SizeChange(rect);
		rect.Width += t_time;
		scrollY_m.SizeChange(rect);
		rect.Width += t_time;
		scrollY_s.SizeChange(rect);
		int height = t_height - (t_time_height - num4);
		if (ValueTimeHorizontal)
		{
			int num5 = 6;
			scrollY_h.SetVrSize(t_time_height * (24 + num5) - 4, height);
			scrollY_m.SetVrSize(t_time_height * (60 + num5) - 4, height);
			scrollY_s.SetVrSize(t_time_height * (60 + num5) - 4, height);
		}
		else
		{
			scrollY_h.SetVrSize(t_time_height * 24, height);
			scrollY_m.SetVrSize(t_time_height * 60, height);
			scrollY_s.SetVrSize(t_time_height * 60, height);
		}
		int num6 = (t_time - num3) / 2;
		int num7 = (t_time_height - num4) / 2;
		foreach (CalendarT item in calendar_time)
		{
			item.rect = new Rectangle(10 + t_time * item.x, 14 + t_time_height * item.y, t_time, t_time_height);
			item.rect_read = new Rectangle(item.rect.X + num6, item.rect.Y + num7, num3, num4);
		}
		ScrollYTime();
		rect_button = new Rectangle(10, 10 + t_height, t_width / 2, t_button);
		rect_buttonok = new Rectangle(rect_button.Right, rect_button.Top, rect_button.Width, rect_button.Height);
		CLocation(point, _control.Placement, _control.DropDownArrow, 10, num, num2, rect_read, ref Inverted, ref ArrowAlign);
	}

	private void ScrollYTime()
	{
		CalendarT calendarT = calendar_time.Find((CalendarT a) => a.x == 0 && a.t == SelDate.Hours);
		CalendarT calendarT2 = calendar_time.Find((CalendarT a) => a.x == 1 && a.t == SelDate.Minutes);
		CalendarT calendarT3 = calendar_time.Find((CalendarT a) => a.x == 2 && a.t == SelDate.Seconds);
		int num = 14;
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

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
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
			SolidBrush val4 = new SolidBrush(Colour.TextBase.Get("DatePicker"));
			try
			{
				GraphicsState state = canvas.Save();
				canvas.SetClip(new Rectangle(0, 10, t_width + 20, t_height));
				SolidBrush val5 = new SolidBrush(Colour.PrimaryBg.Get("DatePicker"));
				try
				{
					canvas.TranslateTransform(0f, 0f - scrollY_h.Value);
					for (int i = 0; i < calendar_time.Count; i++)
					{
						switch (i)
						{
						case 24:
							canvas.ResetTransform();
							canvas.TranslateTransform(0f, 0f - scrollY_m.Value);
							break;
						case 84:
							canvas.ResetTransform();
							canvas.TranslateTransform(0f, 0f - scrollY_s.Value);
							break;
						}
						CalendarT calendarT = calendar_time[i];
						GraphicsPath val6 = calendarT.rect_read.RoundPath(Radius);
						try
						{
							switch (calendarT.x)
							{
							case 0:
								if (calendarT.t == SelDate.Hours)
								{
									canvas.Fill((Brush)(object)val5, val6);
								}
								break;
							case 1:
								if (calendarT.t == SelDate.Minutes)
								{
									canvas.Fill((Brush)(object)val5, val6);
								}
								break;
							case 2:
								if (calendarT.t == SelDate.Seconds)
								{
									canvas.Fill((Brush)(object)val5, val6);
								}
								break;
							}
							if (calendarT.hover)
							{
								canvas.Fill(Colour.FillTertiary.Get("DatePicker"), val6);
							}
							canvas.String(calendarT.v, ((Control)this).Font, (Brush)(object)val4, calendarT.rect_read, s_f);
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
				canvas.Restore(state);
				scrollY_h.Paint(canvas);
				scrollY_m.Paint(canvas);
				scrollY_s.Paint(canvas);
				Color color = Colour.Primary.Get("DatePicker");
				if (hover_button.Animation)
				{
					canvas.String(button_text, ((Control)this).Font, color.BlendColors(hover_button.Value, Colour.PrimaryActive.Get("DatePicker")), rect_button, s_f);
				}
				else if (hover_button.Switch)
				{
					canvas.String(button_text, ((Control)this).Font, Colour.PrimaryActive.Get("DatePicker"), rect_button, s_f);
				}
				else
				{
					canvas.String(button_text, ((Control)this).Font, color, rect_button, s_f);
				}
				if (hover_buttonok.Animation)
				{
					canvas.String(OKButton, ((Control)this).Font, color.BlendColors(hover_buttonok.Value, Colour.PrimaryActive.Get("DatePicker")), rect_buttonok, s_f);
				}
				else if (hover_buttonok.Switch)
				{
					canvas.String(OKButton, ((Control)this).Font, Colour.PrimaryActive.Get("DatePicker"), rect_buttonok, s_f);
				}
				else
				{
					canvas.String(OKButton, ((Control)this).Font, color, rect_buttonok, s_f);
				}
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		return val;
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
		if (!RunAnimation)
		{
			((Control)this).OnMouseDown(e);
			if (rect_read_h.Contains(e.Location))
			{
				scrollY_h.MouseDown(e.Location);
			}
			else if (rect_read_m.Contains(e.Location))
			{
				scrollY_m.MouseDown(e.Location);
			}
			else if (rect_read_s.Contains(e.Location))
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
		if (scrollY_h.MouseMove(e.Location) && scrollY_m.MouseMove(e.Location) && scrollY_s.MouseMove(e.Location))
		{
			int num = 0;
			int num2 = 0;
			bool flag = rect_button.Contains(e.Location);
			bool flag2 = rect_buttonok.Contains(e.Location);
			if (flag != hover_button.Switch)
			{
				num++;
			}
			if (flag2 != hover_buttonok.Switch)
			{
				num++;
			}
			hover_button.Switch = flag;
			hover_buttonok.Switch = flag2;
			if (hover_button.Switch || hover_buttonok.Switch)
			{
				num2++;
			}
			else
			{
				foreach (CalendarT item in calendar_time)
				{
					switch (item.x)
					{
					case 1:
					{
						if (item.Contains(e.Location, 0f, scrollY_m.Value, out var change3))
						{
							num2++;
						}
						if (change3)
						{
							num++;
						}
						break;
					}
					case 2:
					{
						if (item.Contains(e.Location, 0f, scrollY_s.Value, out var change2))
						{
							num2++;
						}
						if (change2)
						{
							num++;
						}
						break;
					}
					default:
					{
						if (item.Contains(e.Location, 0f, scrollY_h.Value, out var change))
						{
							num2++;
						}
						if (change)
						{
							num++;
						}
						break;
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
		scrollY_h.Leave();
		scrollY_m.Leave();
		scrollY_s.Leave();
		foreach (CalendarT item in calendar_time)
		{
			item.hover = false;
		}
		SetCursor(val: false);
		Print();
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		if (RunAnimation)
		{
			return;
		}
		scrollY_h.MouseUp(e.Location);
		scrollY_m.MouseUp(e.Location);
		scrollY_s.MouseUp(e.Location);
		if ((int)e.Button == 1048576)
		{
			if (rect_button.Contains(e.Location))
			{
				DateNow = DateTime.Now;
				SelDate = new TimeSpan(DateNow.Hour, DateNow.Minute, DateNow.Second);
				action(SelDate);
				ScrollYTime();
				Print();
				return;
			}
			if (rect_buttonok.Contains(e.Location))
			{
				action(SelDate);
				IClose();
				return;
			}
			foreach (CalendarT item in calendar_time)
			{
				bool change;
				switch (item.x)
				{
				case 1:
					if (item.Contains(e.Location, 0f, scrollY_m.Value, out change))
					{
						SelDate = new TimeSpan(SelDate.Hours, item.t, SelDate.Seconds);
						if (ValueTimeHorizontal)
						{
							ScrollYTime();
						}
						Print();
						return;
					}
					continue;
				case 2:
					if (item.Contains(e.Location, 0f, scrollY_s.Value, out change))
					{
						SelDate = new TimeSpan(SelDate.Hours, SelDate.Minutes, item.t);
						if (ValueTimeHorizontal)
						{
							ScrollYTime();
						}
						Print();
						return;
					}
					continue;
				}
				if (item.Contains(e.Location, 0f, scrollY_h.Value, out change))
				{
					SelDate = new TimeSpan(item.t, SelDate.Minutes, SelDate.Seconds);
					if (ValueTimeHorizontal)
					{
						ScrollYTime();
					}
					Print();
					return;
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
			if (rect_read_h.Contains(e.Location))
			{
				scrollY_h.MouseWheel(e.Delta);
				Print();
			}
			else if (rect_read_m.Contains(e.Location))
			{
				scrollY_m.MouseWheel(e.Delta);
				Print();
			}
			else if (rect_read_s.Contains(e.Location))
			{
				scrollY_s.MouseWheel(e.Delta);
				Print();
			}
		}
		base.OnMouseWheel(e);
	}
}
