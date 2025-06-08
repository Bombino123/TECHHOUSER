using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace AntdUI;

[Description("DatePickerRange 日期范围选择框")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class DatePickerRange : Input, SubLayeredForm
{
	private bool showS = true;

	private bool showE = true;

	private string? placeholderS;

	private string? placeholderE;

	private string dateFormat = "yyyy-MM-dd";

	private bool ShowTime;

	private DateTime[]? _value;

	private string? swapSvg;

	public Func<DateTime[], List<DateBadge>?>? BadgeAction;

	private BaseCollection? items;

	private bool showicon = true;

	private bool expandDrop;

	private ILayeredForm? subForm;

	private bool AnimationBar;

	private RectangleF AnimationBarValue = RectangleF.Empty;

	private ITask? ThreadBar;

	private string StartEndFocusedTmp = "00";

	private bool StartFocused;

	private bool EndFocused;

	[Description("显示的水印文本S")]
	[Category("行为")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? PlaceholderStart
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationPlaceholderStart, placeholderS);
		}
		set
		{
			if (!(placeholderS == value))
			{
				placeholderS = value;
				Invalidate();
			}
		}
	}

	[Description("显示的水印文本S")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPlaceholderStart { get; set; }

	[Description("显示的水印文本E")]
	[Category("行为")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? PlaceholderEnd
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationPlaceholderEnd, placeholderE);
		}
		set
		{
			if (!(placeholderE == value))
			{
				placeholderE = value;
				Invalidate();
			}
		}
	}

	[Description("显示的水印文本E")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPlaceholderEnd { get; set; }

	[Browsable(false)]
	[Description("水印文本")]
	[Category("行为")]
	[DefaultValue(null)]
	public override string? PlaceholderText => null;

	[Description("格式化")]
	[Category("行为")]
	[DefaultValue("yyyy-MM-dd")]
	public string Format
	{
		get
		{
			return dateFormat;
		}
		set
		{
			if (!(dateFormat == value))
			{
				dateFormat = value;
				ShowTime = dateFormat.Contains("H");
				if (_value == null)
				{
					((Control)this).Text = "";
				}
				else
				{
					((Control)this).Text = _value[0].ToString(dateFormat) + "\t" + _value[1].ToString(dateFormat);
				}
			}
		}
	}

	[Description("控件当前日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime[]? Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				this.ValueChanged?.Invoke(this, new DateTimesEventArgs(value));
				if (value == null)
				{
					((Control)this).Text = "";
				}
				else
				{
					((Control)this).Text = value[0].ToString(Format) + "\t" + value[1].ToString(Format);
				}
				OnPropertyChanged("Value");
			}
		}
	}

	[Description("最小日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? MinDate { get; set; }

	[Description("最大日期")]
	[Category("数据")]
	[DefaultValue(null)]
	public DateTime? MaxDate { get; set; }

	[Description("时间值水平对齐")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ValueTimeHorizontal { get; set; }

	[Description("交换图标SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? SwapSvg
	{
		get
		{
			return swapSvg;
		}
		set
		{
			if (!(swapSvg == value))
			{
				swapSvg = value;
				Invalidate();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor", typeof(UITypeEditor))]
	[Description("预置")]
	[Category("数据")]
	[DefaultValue(null)]
	public BaseCollection Presets
	{
		get
		{
			if (items == null)
			{
				items = new BaseCollection();
			}
			return items;
		}
		set
		{
			items = value;
		}
	}

	[Description("菜单弹出位置")]
	[Category("行为")]
	[DefaultValue(TAlignFrom.BL)]
	public TAlignFrom Placement { get; set; } = TAlignFrom.BL;


	[Description("下拉箭头是否显示")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool DropDownArrow { get; set; } = true;


	[Description("是否显示图标")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool ShowIcon
	{
		get
		{
			return showicon;
		}
		set
		{
			if (showicon != value)
			{
				showicon = value;
				CalculateRect();
				Invalidate();
			}
		}
	}

	public override bool HasSuffix => showicon;

	private bool ExpandDrop
	{
		get
		{
			return expandDrop;
		}
		set
		{
			if (expandDrop == value)
			{
				return;
			}
			expandDrop = value;
			if (!base.ReadOnly && value)
			{
				if (subForm != null)
				{
					return;
				}
				if (ShowTime)
				{
					subForm = new LayeredFormCalendarTimeRange(this, ReadRectangle, _value, delegate(DateTime[] date)
					{
						Value = date;
					}, delegate(object btn)
					{
						this.PresetsClickChanged?.Invoke(this, new ObjectNEventArgs(btn));
					}, BadgeAction);
					((Component)(object)subForm).Disposed += delegate
					{
						subForm = null;
						ExpandDrop = false;
					};
					((Form)subForm).Show((IWin32Window)(object)this);
				}
				else
				{
					subForm = new LayeredFormCalendarRange(this, ReadRectangle, _value, delegate(DateTime[] date)
					{
						Value = date;
					}, delegate(object btn)
					{
						this.PresetsClickChanged?.Invoke(this, new ObjectNEventArgs(btn));
					}, BadgeAction);
					((Component)(object)subForm).Disposed += delegate
					{
						subForm = null;
						ExpandDrop = false;
					};
					((Form)subForm).Show((IWin32Window)(object)this);
				}
			}
			else
			{
				subForm?.IClose();
			}
		}
	}

	protected override bool ModeRange => true;

	public event DateTimesEventHandler? ValueChanged;

	[Description("预置点击时发生")]
	[Category("行为")]
	public event ObjectNEventHandler? PresetsClickChanged;

	protected override void OnTextChanged(EventArgs e)
	{
		if (isempty)
		{
			showS = (showE = true);
		}
		else
		{
			string text = ((Control)this).Text;
			int num = text.IndexOf("\t");
			if (num > -1)
			{
				showS = num == 0;
				showE = string.IsNullOrEmpty(text.Substring(num + 1));
			}
			else
			{
				showS = (showE = false);
			}
		}
		((Control)this).OnTextChanged(e);
	}

	protected override bool Verify(char key, out string? change)
	{
		if (StartFocused)
		{
			int num = ((Control)this).Text.IndexOf("\t");
			if (num != -1 && base.SelectionStart > num)
			{
				base.SelectionStart = num;
			}
		}
		else if (EndFocused && ((Control)this).Text.IndexOf("\t") == -1)
		{
			change = "\t" + key;
			return true;
		}
		return base.Verify(key, out change);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		if (_value != null)
		{
			((Control)this).Text = _value[0].ToString(Format) + "\t" + _value[1].ToString(Format);
		}
		base.OnHandleCreated(e);
	}

	protected override void PaintRIcon(Canvas g, Rectangle rect_r)
	{
		if (!showicon)
		{
			return;
		}
		Bitmap val = SvgDb.IcoDate.SvgToBmp(rect_r.Width, rect_r.Height, Colour.TextQuaternary.Get("DatePicker"));
		try
		{
			if (val != null)
			{
				g.Image(val, rect_r);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (!StartFocused && !EndFocused)
		{
			StartFocused = true;
		}
		StartEndFocused();
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		ExpandDrop = (StartFocused = (EndFocused = false));
		StartEndFocused();
		AnimationBarValue = RectangleF.Empty;
		if (!((Control)this).IsHandleCreated)
		{
			return;
		}
		string text = ((Control)this).Text;
		int num = text.IndexOf("\t");
		if (num > 0)
		{
			string s = text.Substring(0, num);
			string s2 = text.Substring(num + 1);
			if (DateTime.TryParse(s, out var result) && DateTime.TryParse(s2, out var result2))
			{
				Value = new DateTime[2] { result, result2 };
			}
			else if (_value == null)
			{
				((Control)this).Text = "";
			}
			else
			{
				((Control)this).Text = _value[0].ToString(Format) + "\t" + _value[1].ToString(Format);
			}
		}
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	protected override void OnClearValue()
	{
		Value = null;
	}

	protected override void OnClickContent()
	{
		if (base.HasFocus)
		{
			if (!expandDrop)
			{
				ExpandDrop = !expandDrop;
			}
		}
		else
		{
			((Control)this).Focus();
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		bool result = base.ProcessCmdKey(ref msg, keyData);
		if ((int)keyData == 27 && subForm != null)
		{
			subForm.IClose();
			return true;
		}
		if ((int)keyData == 40 && subForm == null)
		{
			ExpandDrop = true;
			return true;
		}
		if ((int)keyData == 13 && (StartFocused || EndFocused))
		{
			string text = ((Control)this).Text;
			int num = text.IndexOf("\t");
			if (StartFocused)
			{
				string s = ((num != -1) ? text.Substring(0, num) : text);
				if (DateTime.TryParse(s, out var result2))
				{
					if (num == -1)
					{
						((Control)this).Text = result2.ToString(Format) + "\t";
						base.SelectionStart = ((Control)this).Text.Length;
					}
					else
					{
						string text2 = text.Substring(num + 1);
						if (DateTime.TryParse(text2, out var result3))
						{
							((Control)this).Text = result2.ToString(Format) + "\t" + result3.ToString(Format);
						}
						else
						{
							((Control)this).Text = result2.ToString(Format) + "\t" + text2;
						}
					}
					if (subForm is LayeredFormCalendarRange layeredFormCalendarRange)
					{
						layeredFormCalendarRange.Date = result2;
						layeredFormCalendarRange.SetDateS(result2);
						layeredFormCalendarRange.Print();
					}
					else if (subForm is LayeredFormCalendarTimeRange layeredFormCalendarTimeRange)
					{
						layeredFormCalendarTimeRange.IClose();
					}
					StartFocused = false;
					EndFocused = true;
					StartEndFocused();
					SetCaretPostion();
				}
			}
			else
			{
				string s2 = ((num != -1) ? text.Substring(num + 1) : text);
				if (DateTime.TryParse(s2, out var result4))
				{
					DateTime result5;
					if (num == -1)
					{
						((Control)this).Text = "\t" + result4.ToString(Format);
					}
					else if (DateTime.TryParse(text.Substring(0, num), out result5))
					{
						((Control)this).Text = result5.ToString(Format) + "\t" + result4.ToString(Format);
						if (subForm is LayeredFormCalendarRange layeredFormCalendarRange2)
						{
							layeredFormCalendarRange2.Date = result4;
							layeredFormCalendarRange2.SetDateE(result5, result4);
							layeredFormCalendarRange2.Print();
						}
						else if (subForm is LayeredFormCalendarTimeRange layeredFormCalendarTimeRange2)
						{
							layeredFormCalendarTimeRange2.IClose();
						}
					}
					else
					{
						((Control)this).Text = text.Substring(0, num) + "\t" + result4.ToString(Format);
					}
				}
			}
			return true;
		}
		return result;
	}

	protected override bool IMouseDown(Point e)
	{
		if (rect_d_l.Contains(e) || rect_d_ico.Contains(e))
		{
			EndFocused = false;
			StartFocused = true;
			StartEndFocused();
		}
		else if (rect_d_r.Contains(e))
		{
			StartFocused = false;
			EndFocused = true;
			StartEndFocused();
		}
		return false;
	}

	protected override void ModeRangeCaretPostion(bool Null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		if (Null)
		{
			if ((int)base.TextAlign == 2)
			{
				if (StartFocused)
				{
					CaretInfo.X = rect_d_l.X + rect_d_l.Width / 2;
				}
				else if (EndFocused)
				{
					CaretInfo.X = rect_d_r.X + rect_d_r.Width / 2;
				}
			}
			else if ((int)base.TextAlign == 1)
			{
				if (StartFocused)
				{
					CaretInfo.X = rect_d_l.Right;
				}
				else if (EndFocused)
				{
					CaretInfo.X = rect_d_r.Right;
				}
			}
			else if (StartFocused)
			{
				CaretInfo.X = rect_d_l.X;
			}
			else if (EndFocused)
			{
				CaretInfo.X = rect_d_r.X;
			}
		}
		else if (StartFocused)
		{
			if (!rect_d_l.Contains(CaretInfo.Rect))
			{
				ModeRangeCaretPostion(Null: true);
			}
		}
		else if (EndFocused && !rect_d_r.Contains(CaretInfo.Rect))
		{
			ModeRangeCaretPostion(Null: true);
		}
	}

	protected override void PaintOtherBor(Canvas g, RectangleF rect_read, float radius, Color back, Color borderColor, Color borderActive)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		string placeholderStart = PlaceholderStart;
		string placeholderEnd = PlaceholderEnd;
		if ((showS && placeholderStart != null) || (showE && placeholderEnd != null))
		{
			SolidBrush val = new SolidBrush(Colour.TextQuaternary.Get("DatePicker"));
			try
			{
				if (showS && placeholderStart != null)
				{
					g.String(placeholderStart, ((Control)this).Font, (Brush)(object)val, rect_d_l, sf_placeholder);
				}
				if (showE && placeholderEnd != null)
				{
					g.String(placeholderEnd, ((Control)this).Font, (Brush)(object)val, rect_d_r, sf_placeholder);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (AnimationBar)
		{
			float num = (float)rect_text.Height * 0.14f;
			Color color = base.BorderActive ?? Colour.Primary.Get("DatePicker");
			g.Fill(color, new RectangleF(AnimationBarValue.X, rect_read.Bottom - num, AnimationBarValue.Width, num));
		}
		else if (StartFocused || EndFocused)
		{
			float num2 = (float)rect_text.Height * 0.14f;
			SolidBrush val2 = new SolidBrush(base.BorderActive ?? Colour.Primary.Get("DatePicker"));
			try
			{
				if (StartFocused)
				{
					g.Fill((Brush)(object)val2, new RectangleF(rect_d_l.X, rect_read.Bottom - num2, rect_d_l.Width, num2));
				}
				else
				{
					g.Fill((Brush)(object)val2, new RectangleF(rect_d_r.X, rect_read.Bottom - num2, rect_d_r.Width, num2));
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		g.GetImgExtend(swapSvg ?? SvgDb.IcoSwap, rect_d_ico, Colour.TextQuaternary.Get("DatePicker"));
	}

	private void StartEndFocused()
	{
		bool startFocused = StartFocused;
		bool endFocused = EndFocused;
		string text = (startFocused ? 1 : 0).ToString() + (endFocused ? 1 : 0);
		if (StartEndFocusedTmp == text)
		{
			return;
		}
		StartEndFocusedTmp = text;
		if (Config.Animation && (startFocused || endFocused))
		{
			RectangleF NewValue;
			if (startFocused)
			{
				NewValue = rect_d_l;
			}
			else
			{
				NewValue = rect_d_r;
			}
			if (AnimationBarValue == RectangleF.Empty)
			{
				AnimationBarValue = new RectangleF(NewValue.X - 10f, NewValue.Y, 0f, NewValue.Height);
			}
			float p_val = Math.Abs(NewValue.X - AnimationBarValue.X) * 0.09f;
			if (p_val > 0f)
			{
				float p_val2 = (NewValue.X - AnimationBarValue.X) * 0.5f;
				float p_w_val = Math.Abs(NewValue.Width - AnimationBarValue.Width) * 0.1f;
				AnimationBar = true;
				bool left = NewValue.X > AnimationBarValue.X;
				ThreadBar?.Dispose();
				ThreadBar = new ITask((Control)(object)this, delegate
				{
					if (AnimationBarValue.Width != NewValue.Width)
					{
						AnimationBarValue.Width += p_w_val;
						if (AnimationBarValue.Width > NewValue.Width)
						{
							AnimationBarValue.Width = NewValue.Width;
						}
					}
					if (left)
					{
						if (AnimationBarValue.X > p_val2)
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
							Invalidate();
							return false;
						}
					}
					else
					{
						AnimationBarValue.X -= p_val;
						if (AnimationBarValue.X < NewValue.X)
						{
							AnimationBarValue.X = NewValue.X;
							Invalidate();
							return false;
						}
					}
					if (subForm is LayeredFormCalendarRange layeredFormCalendarRange2)
					{
						if (Placement == TAlignFrom.TR || Placement == TAlignFrom.BR)
						{
							layeredFormCalendarRange2.SetArrow(AnimationBarValue.X - (float)rect_d_r.X);
						}
						else
						{
							layeredFormCalendarRange2.SetArrow(AnimationBarValue.X);
						}
					}
					Invalidate();
					return true;
				}, 10, delegate
				{
					if (subForm is LayeredFormCalendarRange layeredFormCalendarRange)
					{
						if (Placement == TAlignFrom.TR || Placement == TAlignFrom.BR)
						{
							layeredFormCalendarRange.SetArrow(NewValue.X - (float)rect_d_r.X);
						}
						else
						{
							layeredFormCalendarRange.SetArrow(NewValue.X);
						}
					}
					AnimationBarValue = NewValue;
					AnimationBar = false;
					Invalidate();
				});
				return;
			}
		}
		if (startFocused)
		{
			AnimationBarValue = rect_d_l;
		}
		else
		{
			AnimationBarValue = rect_d_r;
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadBar?.Dispose();
		base.Dispose(disposing);
	}
}
