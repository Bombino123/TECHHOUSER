using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

[Description("InputNumber 数字输入框")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class InputNumber : Input
{
	private decimal? minimum;

	private decimal? maximum;

	private decimal currentValue;

	private bool showcontrol = true;

	private int decimalPlaces;

	private bool thousandsSeparator;

	private bool hexadecimal;

	private ITaskOpacity hover_button;

	private ITaskOpacity hover_button_up;

	private ITaskOpacity hover_button_bottom;

	private RectangleF rect_button;

	private RectangleF rect_button_up;

	private RectangleF rect_button_bottom;

	private static NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;

	private static string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;

	private static string groupSeparator = numberFormatInfo.NumberGroupSeparator;

	private static string negativeSign = numberFormatInfo.NegativeSign;

	private bool isdownup;

	private bool isdowndown;

	private int downid;

	private int temp_old;

	[Browsable(false)]
	[Description("支持清除")]
	[Category("行为")]
	[DefaultValue(false)]
	public new bool AllowClear
	{
		get
		{
			return false;
		}
		set
		{
			base.AllowClear = false;
		}
	}

	[Description("最小值")]
	[Category("数据")]
	[DefaultValue(null)]
	public decimal? Minimum
	{
		get
		{
			return minimum;
		}
		set
		{
			minimum = value;
			if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
			{
				maximum = minimum.Value;
			}
			Value = Constrain(currentValue);
		}
	}

	[Description("最大值")]
	[Category("数据")]
	[DefaultValue(null)]
	public decimal? Maximum
	{
		get
		{
			return maximum;
		}
		set
		{
			maximum = value;
			if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
			{
				minimum = maximum.Value;
			}
			Value = Constrain(currentValue);
		}
	}

	[Description("当前值")]
	[Category("数据")]
	[DefaultValue(typeof(decimal), "0")]
	public decimal Value
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (!(currentValue == value))
			{
				currentValue = Constrain(value);
				((Control)this).Text = GetNumberText(currentValue);
				this.ValueChanged?.Invoke(this, new DecimalEventArgs(currentValue));
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("显示控制器")]
	[Category("交互")]
	[DefaultValue(true)]
	public bool ShowControl
	{
		get
		{
			return showcontrol;
		}
		set
		{
			if (showcontrol != value)
			{
				showcontrol = value;
				Invalidate();
			}
		}
	}

	[Description("显示的小数点位数")]
	[Category("数据")]
	[DefaultValue(0)]
	public int DecimalPlaces
	{
		get
		{
			return decimalPlaces;
		}
		set
		{
			if (decimalPlaces != value)
			{
				decimalPlaces = value;
				((Control)this).Text = GetNumberText(currentValue);
			}
		}
	}

	[Description("是否显示千分隔符")]
	[Category("数据")]
	[DefaultValue(false)]
	public bool ThousandsSeparator
	{
		get
		{
			return thousandsSeparator;
		}
		set
		{
			if (thousandsSeparator != value)
			{
				thousandsSeparator = value;
				((Control)this).Text = GetNumberText(currentValue);
			}
		}
	}

	[Description("值是否应以十六进制显示")]
	[Category("数据")]
	[DefaultValue(false)]
	public bool Hexadecimal
	{
		get
		{
			return hexadecimal;
		}
		set
		{
			if (hexadecimal != value)
			{
				hexadecimal = value;
				((Control)this).Text = GetNumberText(currentValue);
			}
		}
	}

	[Description("当按下箭头键时，是否持续增加/减少")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool InterceptArrowKeys { get; set; } = true;


	[Description("每次单击箭头键时增加/减少的数量")]
	[Category("数据")]
	[DefaultValue(typeof(decimal), "1")]
	public decimal Increment { get; set; } = 1m;


	[Description("Value 属性值更改时发生")]
	[Category("行为")]
	public event DecimalEventHandler? ValueChanged;

	private decimal Constrain(decimal value)
	{
		if (minimum.HasValue && value < minimum.Value)
		{
			value = minimum.Value;
		}
		if (maximum.HasValue && value > maximum.Value)
		{
			value = maximum.Value;
		}
		return value;
	}

	private string GetNumberText(decimal num)
	{
		if (Hexadecimal)
		{
			return ((long)num).ToString("X", CultureInfo.InvariantCulture);
		}
		return num.ToString((ThousandsSeparator ? "N" : "F") + DecimalPlaces.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).Text = GetNumberText(currentValue);
		base.OnHandleCreated(e);
	}

	public InputNumber()
	{
		hover_button = new ITaskOpacity((IControl)this);
		hover_button_up = new ITaskOpacity((IControl)this);
		hover_button_bottom = new ITaskOpacity((IControl)this);
	}

	protected override void Dispose(bool disposing)
	{
		hover_button.Dispose();
		hover_button_up.Dispose();
		hover_button_bottom.Dispose();
		base.Dispose(disposing);
	}

	protected override bool Verify(char key, out string? change)
	{
		change = null;
		string text = key.ToString();
		if (char.IsDigit(key))
		{
			return true;
		}
		if (text.Equals(decimalSeparator) || text.Equals(groupSeparator) || text.Equals(negativeSign))
		{
			return true;
		}
		if (key == '\b')
		{
			return true;
		}
		if (Hexadecimal && ((key >= 'a' && key <= 'f') || (key >= 'A' && key <= 'F')))
		{
			return true;
		}
		return false;
	}

	protected override void PaintOtherBor(Canvas g, RectangleF rect_read, float _radius, Color back, Color borColor, Color borderActive)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Expected O, but got Unknown
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Expected O, but got Unknown
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Expected O, but got Unknown
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Expected O, but got Unknown
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Expected O, but got Unknown
		if (!hover_button.Animation && !hover_button.Switch)
		{
			return;
		}
		float num = (round ? (rect_read.Height / 2f) : _radius);
		int num2 = (int)(22f * Config.Dpi);
		rect_button = new RectangleF(rect_read.Right - (float)num2, rect_read.Y, num2, rect_read.Height);
		rect_button_up = new RectangleF(rect_button.X, rect_button.Y, rect_button.Width, rect_button.Height / 2f);
		rect_button_bottom = new RectangleF(rect_button.X, rect_button_up.Bottom, rect_button.Width, rect_button_up.Height);
		GraphicsPath val = rect_button.RoundPath(num, TL: false, TR: true, BR: true, BL: false);
		try
		{
			g.Fill(back, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (hover_button.Animation)
		{
			Pen val2 = new Pen(Helper.ToColor(hover_button.Value, borColor), Config.Dpi);
			try
			{
				GraphicsPath val3 = rect_button_up.RoundPath(num, TL: false, TR: true, BR: false, BL: false);
				try
				{
					g.Draw(val2, val3);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				GraphicsPath val4 = rect_button_bottom.RoundPath(num, TL: false, TR: false, BR: true, BL: false);
				try
				{
					g.Draw(val2, val4);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		else if (hover_button.Switch)
		{
			Pen val5 = new Pen(borColor, Config.Dpi);
			try
			{
				GraphicsPath val6 = rect_button_up.RoundPath(num, TL: false, TR: true, BR: false, BL: false);
				try
				{
					g.Draw(val5, val6);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
				GraphicsPath val7 = rect_button_bottom.RoundPath(num, TL: false, TR: false, BR: true, BL: false);
				try
				{
					g.Draw(val5, val7);
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
		if (hover_button_up.Animation)
		{
			Pen val8 = new Pen(borColor.BlendColors(hover_button_up.Value, borderActive), Config.Dpi);
			try
			{
				g.DrawLines(val8, TAlignMini.Top.TriangleLines(rect_button_up));
			}
			finally
			{
				((IDisposable)val8)?.Dispose();
			}
		}
		else if (hover_button_up.Switch)
		{
			Pen val9 = new Pen(borderActive, Config.Dpi);
			try
			{
				g.DrawLines(val9, TAlignMini.Top.TriangleLines(rect_button_up));
			}
			finally
			{
				((IDisposable)val9)?.Dispose();
			}
		}
		else
		{
			Pen val10 = new Pen(borColor, Config.Dpi);
			try
			{
				g.DrawLines(val10, TAlignMini.Top.TriangleLines(rect_button_up));
			}
			finally
			{
				((IDisposable)val10)?.Dispose();
			}
		}
		if (hover_button_bottom.Animation)
		{
			Pen val11 = new Pen(borColor.BlendColors(hover_button_bottom.Value, borderActive), Config.Dpi);
			try
			{
				g.DrawLines(val11, TAlignMini.Bottom.TriangleLines(rect_button_bottom));
				return;
			}
			finally
			{
				((IDisposable)val11)?.Dispose();
			}
		}
		if (hover_button_bottom.Switch)
		{
			Pen val12 = new Pen(borderActive, Config.Dpi);
			try
			{
				g.DrawLines(val12, TAlignMini.Bottom.TriangleLines(rect_button_bottom));
				return;
			}
			finally
			{
				((IDisposable)val12)?.Dispose();
			}
		}
		Pen val13 = new Pen(borColor, Config.Dpi);
		try
		{
			g.DrawLines(val13, TAlignMini.Bottom.TriangleLines(rect_button_bottom));
		}
		finally
		{
			((IDisposable)val13)?.Dispose();
		}
	}

	protected override void ChangeMouseHover(bool Hover, bool Focus)
	{
		hover_button.Switch = showcontrol && !base.ReadOnly && (Hover || Focus);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (showcontrol)
		{
			if (!base.ReadOnly && rect_button.Contains(e.Location))
			{
				if (rect_button_up.Contains(e.Location))
				{
					hover_button_bottom.Switch = false;
					hover_button_up.Switch = true;
				}
				else
				{
					hover_button_up.Switch = false;
					hover_button_bottom.Switch = true;
				}
				SetCursor(val: true);
				return;
			}
			ITaskOpacity taskOpacity = hover_button_up;
			bool @switch = (hover_button_bottom.Switch = false);
			taskOpacity.Switch = @switch;
			SetCursor(val: false);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (showcontrol && !base.ReadOnly && rect_button.Contains(e.Location))
		{
			if (decimal.TryParse(((Control)this).Text, out var result))
			{
				Value = result;
			}
			if (rect_button_up.Contains(e.Location))
			{
				Value = currentValue + Increment;
				isdownup = true;
			}
			else
			{
				Value = currentValue - Increment;
				isdowndown = true;
			}
			if ((isdownup || isdowndown) && InterceptArrowKeys)
			{
				int _downid = (downid = temp_old);
				temp_old++;
				if (temp_old > 9999)
				{
					temp_old = 0;
				}
				ITask.Run(delegate
				{
					Thread.Sleep(500);
					while (isdownup || (isdowndown && _downid == downid))
					{
						decimal num = currentValue;
						((Control)this).Invoke((Delegate)(Action)delegate
						{
							if (isdownup)
							{
								Value = currentValue + Increment;
							}
							else if (isdowndown)
							{
								Value = currentValue - Increment;
							}
						});
						if (num == currentValue)
						{
							break;
						}
						Thread.Sleep(200);
					}
				});
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		isdownup = (isdowndown = false);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)keyData == 13 && decimal.TryParse(((Control)this).Text, out var result))
		{
			Value = result;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		if (((Control)this).IsHandleCreated)
		{
			if (decimal.TryParse(((Control)this).Text, out var result))
			{
				Value = result;
			}
			else
			{
				((Control)this).Text = GetNumberText(currentValue);
			}
		}
		base.OnLostFocus(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (!base.ReadOnly)
		{
			if (e.Delta > 0)
			{
				Value = currentValue + Increment;
			}
			else
			{
				Value = currentValue - Increment;
			}
		}
	}
}
