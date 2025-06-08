using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

[Description("LabelTime 时间文本")]
[ToolboxItem(true)]
[DefaultProperty("ShowTime")]
public class LabelTime : IControl
{
	private bool showTime = true;

	private string show_tmp = "";

	private readonly StringFormat s_f_l = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)2);

	private readonly StringFormat s_f_r1 = Helper.SF_NoWrap((StringAlignment)2, (StringAlignment)0);

	private readonly StringFormat s_f_r2 = Helper.SF_NoWrap((StringAlignment)0, (StringAlignment)0);

	[Description("外观")]
	[Category("是否显示秒")]
	[DefaultValue(true)]
	public bool ShowTime
	{
		get
		{
			return showTime;
		}
		set
		{
			if (showTime != value)
			{
				showTime = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("自动宽度")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool AutoWidth { get; set; }

	public LabelTime()
	{
		Thread thread = new Thread(TaskLong);
		thread.IsBackground = true;
		thread.Start();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		Canvas canvas = e.Graphics.High();
		string[] array = GTime();
		show_tmp = string.Join("", array);
		SolidBrush val = new SolidBrush(((Control)this).ForeColor.rgba(0.8f));
		try
		{
			Font val2 = new Font(((Control)this).Font.FontFamily, (float)clientRectangle.Height * 0.72f, (FontStyle)1, (GraphicsUnit)2);
			try
			{
				Size size = canvas.MeasureString(array[0], val2, 10000, s_f_l);
				Rectangle rect = new Rectangle(clientRectangle.X, clientRectangle.Y, size.Width, clientRectangle.Height);
				canvas.String(array[1], val2, ((Control)this).ForeColor, rect, s_f_l);
				int num = rect.Height / 2;
				int num2 = rect.Width + (int)((float)size.Height * 0.24f);
				int width = clientRectangle.Width - num2;
				if (AutoWidth)
				{
					Font val3 = new Font(((Control)this).Font.FontFamily, val2.Size * 0.36f, (GraphicsUnit)2);
					try
					{
						Size size2 = canvas.MeasureString(array[2], val3);
						Size size3 = canvas.MeasureString(array[3], val3);
						canvas.String(array[2], val3, (Brush)(object)val, new Rectangle(clientRectangle.X + num2, clientRectangle.Y, width, num), s_f_r1);
						canvas.String(array[3], val3, (Brush)(object)val, new Rectangle(clientRectangle.X + num2, clientRectangle.Y + num, width, num), s_f_r2);
						((Control)this).Width = num2 + ((size2.Width > size3.Width) ? size2.Width : size3.Width);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				else
				{
					Font val4 = new Font(((Control)this).Font.FontFamily, val2.Size * 0.36f, (GraphicsUnit)2);
					try
					{
						canvas.String(array[2], val4, (Brush)(object)val, new Rectangle(clientRectangle.X + num2, clientRectangle.Y, width, num), s_f_r1);
						canvas.String(array[3], val4, (Brush)(object)val, new Rectangle(clientRectangle.X + num2, clientRectangle.Y + num, width, num), s_f_r2);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
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
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void TaskLong()
	{
		while (!((Control)this).IsDisposed)
		{
			Thread.Sleep(ShowTime ? 1000 : 10000);
			if (!((Control)this).IsDisposed)
			{
				string[] value = GTime();
				if (string.Join("", value) != show_tmp)
				{
					((Control)this).Invalidate();
				}
				continue;
			}
			break;
		}
	}

	private string[] GTime()
	{
		DateTime now = DateTime.Now;
		CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
		string text = (currentUICulture.Name.StartsWith("zh") ? now.ToString("dddd", currentUICulture) : now.ToString("ddd", currentUICulture));
		if (!ShowTime)
		{
			return new string[4]
			{
				"24:59",
				now.ToString("HH:mm"),
				now.ToString("MM-dd"),
				text
			};
		}
		return new string[4]
		{
			"24:59:59",
			now.ToString("HH:mm:ss"),
			now.ToString("MM-dd"),
			text
		};
	}
}
