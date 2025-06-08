using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Progress 进度条")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
public class Progress : IControl
{
	private Color? fore;

	private Color? back;

	private Color? fill;

	private int radius;

	private TShapeProgress shape = TShapeProgress.Round;

	private string? text;

	private string? textUnit = "%";

	private bool useSystemText;

	private TType state;

	private float iconratio = 0.7f;

	private float valueratio = 0.4f;

	private float _value;

	private float _value_show;

	private bool loading;

	private float AnimationLoadingValue;

	private int steps = 3;

	private int stepSize = 14;

	private int stepGap = 2;

	private ITask? ThreadLoading;

	private ITask? ThreadValue;

	private ContainerControl? ownerForm;

	private bool showInTaskbar;

	private bool canTaskbar;

	private ulong old_value;

	private ThumbnailProgressState old_state;

	private readonly StringFormat s_c = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_r = Helper.SF_ALL((StringAlignment)1, (StringAlignment)2);

	private readonly StringFormat s_l = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

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

	[Description("背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? Back
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
				OnPropertyChanged("Back");
			}
		}
	}

	[Description("进度条颜色")]
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
			if (!(fill == value))
			{
				fill = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Fill");
			}
		}
	}

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(0)]
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

	[Description("形状")]
	[Category("外观")]
	[DefaultValue(TShapeProgress.Round)]
	public TShapeProgress Shape
	{
		get
		{
			return shape;
		}
		set
		{
			if (shape != value)
			{
				shape = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Shape");
			}
		}
	}

	[Description("文本")]
	[Category("外观")]
	[DefaultValue(null)]
	public override string? Text
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationText, text);
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				if (useSystemText)
				{
					((Control)this).Invalidate();
				}
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("单位文本")]
	[Category("外观")]
	[DefaultValue("%")]
	[Localizable(true)]
	public string? TextUnit
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationTextUnit, textUnit);
		}
		set
		{
			if (!(textUnit == value))
			{
				textUnit = value;
				((Control)this).Invalidate();
				OnPropertyChanged("TextUnit");
			}
		}
	}

	[Description("单位文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTextUnit { get; set; }

	[Description("使用系统文本")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool UseSystemText
	{
		get
		{
			return useSystemText;
		}
		set
		{
			if (useSystemText != value)
			{
				useSystemText = value;
				((Control)this).Invalidate();
				OnPropertyChanged("UseSystemText");
			}
		}
	}

	[Description("显示进度文本小数点位数")]
	[Category("外观")]
	[DefaultValue(0)]
	public int ShowTextDot { get; set; }

	[Description("状态")]
	[Category("外观")]
	[DefaultValue(TType.None)]
	public TType State
	{
		get
		{
			return state;
		}
		set
		{
			if (state != value)
			{
				state = value;
				((Control)this).Invalidate();
				if (showInTaskbar)
				{
					ShowTaskbar();
				}
				OnPropertyChanged("State");
			}
		}
	}

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(0.7f)]
	public float IconRatio
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
				((Control)this).Invalidate();
				OnPropertyChanged("IconRatio");
			}
		}
	}

	[Description("进度条比例")]
	[Category("外观")]
	[DefaultValue(0.4f)]
	public float ValueRatio
	{
		get
		{
			return valueratio;
		}
		set
		{
			if (valueratio != value)
			{
				valueratio = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ValueRatio");
			}
		}
	}

	[Description("进度条 0F-1F")]
	[Category("数据")]
	[DefaultValue(0f)]
	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value == value)
			{
				return;
			}
			if (value < 0f)
			{
				value = 0f;
			}
			else if (value > 1f)
			{
				value = 1f;
			}
			_value = value;
			ThreadValue?.Dispose();
			ThreadValue = null;
			if (Config.Animation && ((Control)this).IsHandleCreated && Animation > 0)
			{
				int t = AntdUI.Animation.TotalFrames(10, Animation);
				if (_value > _value_show)
				{
					float s2 = _value_show;
					float v2 = Math.Abs(_value - s2);
					ThreadValue = new ITask(delegate(int i)
					{
						_value_show = s2 + AntdUI.Animation.Animate(i, t, v2, AnimationType.Ball);
						((Control)this).Invalidate();
						return true;
					}, 10, t, delegate
					{
						_value_show = _value;
						((Control)this).Invalidate();
					});
				}
				else
				{
					float s = _value_show;
					float v = Math.Abs(_value_show - _value);
					ThreadValue = new ITask(delegate(int i)
					{
						_value_show = s - AntdUI.Animation.Animate(i, t, v, AnimationType.Ball);
						((Control)this).Invalidate();
						return true;
					}, 10, t, delegate
					{
						_value_show = _value;
						((Control)this).Invalidate();
					});
				}
			}
			else
			{
				_value_show = _value;
				((Control)this).Invalidate();
			}
			if (showInTaskbar)
			{
				ShowTaskbar();
			}
			OnPropertyChanged("Value");
		}
	}

	[Description("加载状态")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Loading
	{
		get
		{
			return loading;
		}
		set
		{
			if (loading == value)
			{
				return;
			}
			loading = value;
			ThreadLoading?.Dispose();
			if (showInTaskbar)
			{
				ShowTaskbar(!loading);
			}
			if (loading)
			{
				ThreadLoading = new ITask((Control)(object)this, delegate
				{
					AnimationLoadingValue = AnimationLoadingValue.Calculate(0.01f);
					if (AnimationLoadingValue > 1f)
					{
						AnimationLoadingValue = 0f;
						((Control)this).Invalidate();
						Thread.Sleep(1000);
					}
					((Control)this).Invalidate();
					return loading;
				}, 10, delegate
				{
					((Control)this).Invalidate();
				});
			}
			else
			{
				((Control)this).Invalidate();
			}
			OnPropertyChanged("Loading");
		}
	}

	[Description("动画铺满")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool LoadingFull { get; set; }

	[Description("动画时长")]
	[Category("外观")]
	[DefaultValue(200)]
	public int Animation { get; set; } = 200;


	[Description("进度条总共步数")]
	[Category("外观")]
	[DefaultValue(3)]
	public int Steps
	{
		get
		{
			return steps;
		}
		set
		{
			if (steps != value)
			{
				steps = value;
				if (shape == TShapeProgress.Steps)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Steps");
			}
		}
	}

	[Description("步数大小")]
	[Category("外观")]
	[DefaultValue(14)]
	public int StepSize
	{
		get
		{
			return stepSize;
		}
		set
		{
			if (stepSize != value)
			{
				stepSize = value;
				if (shape == TShapeProgress.Steps)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("StepSize");
			}
		}
	}

	[Description("步数间隔")]
	[Category("外观")]
	[DefaultValue(2)]
	public int StepGap
	{
		get
		{
			return stepGap;
		}
		set
		{
			if (stepGap != value)
			{
				stepGap = value;
				if (shape == TShapeProgress.Steps)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("StepGap");
			}
		}
	}

	[Description("窗口对象")]
	[Category("任务栏")]
	[DefaultValue(null)]
	public ContainerControl? ContainerControl
	{
		get
		{
			return ownerForm;
		}
		set
		{
			ownerForm = value;
		}
	}

	[Description("任务栏中显示进度")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ShowInTaskbar
	{
		get
		{
			return showInTaskbar;
		}
		set
		{
			if (showInTaskbar == value)
			{
				return;
			}
			showInTaskbar = value;
			if (canTaskbar)
			{
				if (showInTaskbar)
				{
					ShowTaskbar();
				}
				else if (ownerForm != null)
				{
					TaskbarProgressState(ownerForm, ThumbnailProgressState.NoProgress);
				}
			}
			OnPropertyChanged("ShowInTaskbar");
		}
	}

	[Description("Value格式化时发生")]
	[Category("行为")]
	public event ProgressFormatEventHandler? ValueFormatChanged;

	protected override void Dispose(bool disposing)
	{
		ThreadLoading?.Dispose();
		ThreadValue?.Dispose();
		base.Dispose(disposing);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		if (ownerForm == null)
		{
			ownerForm = (ContainerControl?)(object)((Control)this).Parent.FindPARENT();
		}
		ITask.Run(delegate
		{
			Thread.Sleep(100);
			canTaskbar = true;
			if (showInTaskbar)
			{
				ShowTaskbar();
			}
		});
	}

	private void ShowTaskbar(bool sl = false)
	{
		if (!canTaskbar || ownerForm == null)
		{
			return;
		}
		if (state == TType.None)
		{
			if (_value == 0f && loading)
			{
				TaskbarProgressValue(ownerForm, 0uL);
				TaskbarProgressState(ownerForm, ThumbnailProgressState.Indeterminate);
				return;
			}
			if (sl && old_state == ThumbnailProgressState.Indeterminate)
			{
				TaskbarProgressState(ownerForm, ThumbnailProgressState.NoProgress);
			}
			TaskbarProgressState(ownerForm, ThumbnailProgressState.Normal);
			TaskbarProgressValue(ownerForm, (ulong)(_value * 100f));
			return;
		}
		switch (state)
		{
		case TType.Error:
			TaskbarProgressState(ownerForm, ThumbnailProgressState.Error);
			break;
		case TType.Warn:
			TaskbarProgressState(ownerForm, ThumbnailProgressState.Paused);
			break;
		default:
			TaskbarProgressState(ownerForm, ThumbnailProgressState.Normal);
			break;
		}
		TaskbarProgressValue(ownerForm, (ulong)(_value * 100f));
	}

	private void TaskbarProgressState(ContainerControl hwnd, ThumbnailProgressState state)
	{
		ContainerControl hwnd2 = hwnd;
		if (old_state == state)
		{
			return;
		}
		old_state = state;
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				Windows7Taskbar.SetProgressState(((Control)hwnd2).Handle, state);
			});
		}
		else
		{
			Windows7Taskbar.SetProgressState(((Control)hwnd2).Handle, state);
		}
	}

	private void TaskbarProgressValue(ContainerControl hwnd, ulong value)
	{
		ContainerControl hwnd2 = hwnd;
		if (old_value == value)
		{
			return;
		}
		old_value = value;
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				Windows7Taskbar.SetProgressValue(((Control)hwnd2).Handle, value);
			});
		}
		else
		{
			Windows7Taskbar.SetProgressValue(((Control)hwnd2).Handle, value);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		Rectangle rect = clientRectangle.PaddingRect(((Control)this).Padding);
		if (rect.Width != 0 && rect.Height != 0)
		{
			Canvas g = e.Graphics.High();
			Color color = state switch
			{
				TType.Success => fill ?? Colour.Success.Get("Progress"), 
				TType.Info => fill ?? Colour.Info.Get("Progress"), 
				TType.Warn => fill ?? Colour.Warning.Get("Progress"), 
				TType.Error => fill ?? Colour.Error.Get("Progress"), 
				_ => fill ?? Colour.Primary.Get("Progress"), 
			};
			switch (shape)
			{
			case TShapeProgress.Circle:
				PaintShapeCircle(g, rect, color);
				break;
			case TShapeProgress.Mini:
				PaintShapeMini(g, rect, color);
				break;
			case TShapeProgress.Steps:
				PaintShapeSteps(g, clientRectangle, rect, color);
				break;
			case TShapeProgress.Round:
				PaintShapeRound(g, clientRectangle, rect, color, round: true);
				break;
			default:
				PaintShapeRound(g, clientRectangle, rect, color, round: false);
				break;
			}
			this.PaintBadge(g);
			((Control)this).OnPaint(e);
		}
	}

	private void PaintShapeMini(Canvas g, Rectangle rect, Color color)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Expected O, but got Unknown
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Expected O, but got Unknown
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		Color color2 = back ?? Color.FromArgb(40, color);
		rect.IconRectL(g.MeasureString("100" + TextUnit, ((Control)this).Font).Height, out var icon_rect, out var text_rect, iconratio);
		if (icon_rect.Width == 0 || icon_rect.Height == 0)
		{
			return;
		}
		SolidBrush val = new SolidBrush(fore ?? Colour.Text.Get("Progress"));
		try
		{
			string text = this.ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (((Control)this).Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
			g.String(text, ((Control)this).Font, (Brush)(object)val, new Rectangle(text_rect.X + 8, text_rect.Y, text_rect.Width - 8, text_rect.Height), s_l);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		int num = ((radius == 0) ? ((int)Math.Round((float)icon_rect.Width * 0.2f)) : ((int)((float)radius * Config.Dpi)));
		g.DrawEllipse(color2, num, icon_rect);
		int num2 = 0;
		if (_value_show > 0f)
		{
			num2 = (int)Math.Round(360f * _value_show);
			Pen val2 = new Pen(color, (float)num);
			try
			{
				LineCap startCap = (LineCap)2;
				val2.EndCap = (LineCap)2;
				val2.StartCap = startCap;
				g.DrawArc(val2, icon_rect, -90f, num2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (!loading || !(AnimationLoadingValue > 0f))
		{
			return;
		}
		if (_value_show > 0f)
		{
			Pen val3 = new Pen(Helper.ToColor(60f * (1f - AnimationLoadingValue), Colour.BgBase.Get("Progress")), (float)num);
			try
			{
				LineCap startCap = (LineCap)2;
				val3.EndCap = (LineCap)2;
				val3.StartCap = startCap;
				g.DrawArc(val3, icon_rect, -90f, (int)((float)num2 * AnimationLoadingValue));
				return;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		if (!LoadingFull)
		{
			return;
		}
		num2 = 360;
		Pen val4 = new Pen(Helper.ToColor(80f * (1f - AnimationLoadingValue), Colour.BgBase.Get("Progress")), (float)num);
		try
		{
			LineCap startCap = (LineCap)2;
			val4.EndCap = (LineCap)2;
			val4.StartCap = startCap;
			g.DrawArc(val4, icon_rect, -90f, (int)((float)num2 * AnimationLoadingValue));
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	private void PaintShapeSteps(Canvas g, Rectangle rect_t, Rectangle rect, Color color)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Expected O, but got Unknown
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Expected O, but got Unknown
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Expected O, but got Unknown
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Expected O, but got Unknown
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Expected O, but got Unknown
		Color obj = back ?? Colour.FillSecondary.Get("Progress");
		Size size = g.MeasureString("100" + TextUnit, ((Control)this).Font);
		int num = (int)((float)stepGap * Config.Dpi);
		int num2 = (int)((float)size.Height * valueratio);
		float num3 = (int)((float)stepSize * Config.Dpi);
		float num4 = 0f;
		int num5 = rect.Y + (rect.Height - num2) / 2;
		float num6 = (float)steps * _value_show;
		SolidBrush val = new SolidBrush(obj);
		try
		{
			SolidBrush val2 = new SolidBrush(color);
			try
			{
				if (num3 <= 0f)
				{
					float num7 = rect.Width;
					if (state == TType.None)
					{
						string text = this.ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (((Control)this).Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
						num7 -= (float)(g.MeasureString(text, ((Control)this).Font).Width + num2 / 2);
					}
					else
					{
						int num8 = (int)((float)size.Height * (iconratio + 0.1f));
						num7 -= (float)(num8 + num2 * 2 + num2 / 2);
					}
					num3 = (num7 - (float)num * ((float)steps - 1f)) / (float)steps;
				}
				if (num3 > 0f)
				{
					List<RectangleF> list = new List<RectangleF>(steps);
					for (int i = 0; i < steps; i++)
					{
						list.Add(new RectangleF((float)rect.X + num4, num5, num3, num2));
						num4 += num3 + (float)num;
					}
					if (num6 > 0f)
					{
						float num9 = (LoadingFull ? rect.Width : 0);
						for (int j = 0; j < steps; j++)
						{
							if (num6 > (float)j)
							{
								g.Fill((Brush)(object)val2, list[j]);
								continue;
							}
							g.Fill((Brush)(object)val, list[j]);
							num9 = list[j].Right;
						}
						if (loading && AnimationLoadingValue > 0f && num9 > 0f)
						{
							GraphicsPath val3 = new GraphicsPath();
							try
							{
								foreach (RectangleF item in list)
								{
									val3.AddRectangle(item);
								}
								SolidBrush val4 = new SolidBrush(Helper.ToColor(60f * (1f - AnimationLoadingValue), Colour.TextBase.Get("Progress")));
								try
								{
									GraphicsState val5 = g.Save();
									g.SetClip(new RectangleF(rect.X, rect.Y, num9 * _value_show * AnimationLoadingValue, rect.Height));
									g.Fill((Brush)(object)val4, val3);
									g.Restore(val5);
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
					}
					else
					{
						if (loading && LoadingFull)
						{
							GraphicsPath val6 = new GraphicsPath();
							try
							{
								foreach (RectangleF item2 in list)
								{
									val6.AddRectangle(item2);
								}
								SolidBrush val7 = new SolidBrush(Helper.ToColor(80f * (1f - AnimationLoadingValue), Colour.TextBase.Get("Progress")));
								try
								{
									GraphicsState val8 = g.Save();
									g.SetClip(new RectangleF(rect.X, rect.Y, (float)rect.Width * AnimationLoadingValue, rect.Height));
									g.Fill((Brush)(object)val7, val6);
									g.Restore(val8);
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
						for (int k = 0; k < steps; k++)
						{
							g.Fill((Brush)(object)val, list[k]);
						}
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
		if (state == TType.None)
		{
			int num10 = (int)Math.Ceiling(num4 + (float)(num2 / 2));
			SolidBrush val9 = new SolidBrush(fore ?? Colour.Text.Get("Progress"));
			try
			{
				string text2 = this.ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (((Control)this).Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
				g.String(text2, ((Control)this).Font, (Brush)(object)val9, new Rectangle(rect.X + num10, rect.Y, rect.Width - num10, rect.Height), s_l);
				return;
			}
			finally
			{
				((IDisposable)val9)?.Dispose();
			}
		}
		int num11 = (int)Math.Ceiling(num4);
		int num12 = (int)((float)size.Height * (iconratio + 0.1f));
		int num13 = num2 + num12;
		g.PaintIcons(state, new Rectangle(rect.X + num11 + num13 - num12, rect_t.Y + (rect_t.Height - num12) / 2, num12, num12), "Progress");
	}

	private void PaintShapeRound(Canvas g, Rectangle rect_t, Rectangle rect, Color color, bool round)
	{
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Expected O, but got Unknown
		Color color2 = back ?? Colour.FillSecondary.Get("Progress");
		float num = (float)radius * Config.Dpi;
		if (round)
		{
			num = rect.Height;
		}
		if (state == TType.None)
		{
			string text2;
			string text;
			if (this.ValueFormatChanged == null)
			{
				if (useSystemText)
				{
					text2 = (text = ((Control)this).Text);
				}
				else
				{
					string text3 = (_value_show * 100f).ToString("F" + ShowTextDot);
					char[] array = new char[text3.Length];
					array[0] = text3[0];
					for (int i = 1; i < text3.Length; i++)
					{
						if (text3[i] == '.')
						{
							array[i] = '.';
						}
						else
						{
							array[i] = '0';
						}
					}
					text2 = string.Join("", array) + TextUnit;
					text = text3 + TextUnit;
				}
			}
			else
			{
				text2 = (text = this.ValueFormatChanged(this, new FloatEventArgs(_value_show)));
			}
			if (text2 != null || text != null)
			{
				Size size = g.MeasureString(text2, ((Control)this).Font);
				int num2 = (int)((float)size.Height * valueratio);
				int num3 = (int)Math.Ceiling((float)size.Width + (float)size.Height * 0.2f);
				Rectangle rect2 = new Rectangle(rect.Right - num3, rect_t.Y, num3, rect_t.Height);
				rect.Y += (rect.Height - num2) / 2;
				rect.Height = num2;
				rect.Width -= num3;
				PaintProgress(g, num, rect, color2, color);
				SolidBrush val = new SolidBrush(fore ?? Colour.Text.Get("Progress"));
				try
				{
					g.String(text, ((Control)this).Font, (Brush)(object)val, rect2, s_r);
					return;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			int num4 = (int)((float)g.MeasureString("龍Qq", ((Control)this).Font).Height * valueratio);
			rect.Y += (rect.Height - num4) / 2;
			rect.Height = num4;
			PaintProgress(g, num, rect, color2, color);
		}
		else
		{
			string text4 = (_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit;
			Size size2 = g.MeasureString(text4, ((Control)this).Font);
			int num5 = (int)((float)size2.Height * valueratio);
			int num6 = (int)((float)size2.Height * (iconratio + 0.1f));
			int num7 = num5 + num6;
			Rectangle rectangle = new Rectangle(rect.Right - num7, rect_t.Y, num7, rect_t.Height);
			rect.Y += (rect.Height - num5) / 2;
			rect.Height = num5;
			rect.Width -= num7;
			PaintProgress(g, num, rect, color2, color);
			g.PaintIcons(state, new Rectangle(rectangle.Right - num6, rectangle.Y + (rectangle.Height - num6) / 2, num6, num6), "Progress");
		}
	}

	private void PaintShapeCircle(Canvas g, Rectangle rect, Color color)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Expected O, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Expected O, but got Unknown
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Expected O, but got Unknown
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		Color color2 = back ?? Colour.FillSecondary.Get("Progress");
		int num = ((rect.Width == rect.Height) ? rect.Width : ((rect.Width <= rect.Height) ? rect.Width : rect.Height));
		int num2 = ((radius == 0) ? ((int)Math.Round((float)num * 0.04f)) : ((int)((float)radius * Config.Dpi)));
		num -= num2;
		Rectangle rectangle = new Rectangle(rect.X + (rect.Width - num) / 2, rect.Y + (rect.Height - num) / 2, num, num);
		g.DrawEllipse(color2, num2, rectangle);
		int num3 = 0;
		if (_value_show > 0f)
		{
			num3 = (int)Math.Round(360f * _value_show);
			Pen val = new Pen(color, (float)num2);
			try
			{
				LineCap startCap = (LineCap)2;
				val.EndCap = (LineCap)2;
				val.StartCap = startCap;
				g.DrawArc(val, rectangle, -90f, num3);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (loading && AnimationLoadingValue > 0f)
		{
			if (_value_show > 0f)
			{
				Pen val2 = new Pen(Helper.ToColor(60f * (1f - AnimationLoadingValue), Colour.BgBase.Get("Progress")), (float)num2);
				try
				{
					LineCap startCap = (LineCap)2;
					val2.EndCap = (LineCap)2;
					val2.StartCap = startCap;
					g.DrawArc(val2, rectangle, -90f, (int)((float)num3 * AnimationLoadingValue));
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			else if (LoadingFull)
			{
				num3 = 360;
				Pen val3 = new Pen(Helper.ToColor(80f * (1f - AnimationLoadingValue), Colour.BgBase.Get("Progress")), (float)num2);
				try
				{
					LineCap startCap = (LineCap)2;
					val3.EndCap = (LineCap)2;
					val3.StartCap = startCap;
					g.DrawArc(val3, rectangle, -90f, (int)((float)num3 * AnimationLoadingValue));
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}
		if (_value_show > 0f)
		{
			if (state == TType.None)
			{
				SolidBrush val4 = new SolidBrush(fore ?? Colour.Text.Get("Progress"));
				try
				{
					string text = this.ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (((Control)this).Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
					g.String(text, ((Control)this).Font, (Brush)(object)val4, rect, s_c);
					return;
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			int num4 = (int)((float)rectangle.Width * 0.26f);
			g.PaintIconGhosts(state, new Rectangle(rect.X + (rect.Width - num4) / 2, rect.Y + (rect.Height - num4) / 2, num4, num4), color);
			return;
		}
		SolidBrush val5 = new SolidBrush(fore ?? Colour.Text.Get("Progress"));
		try
		{
			string text2 = this.ValueFormatChanged?.Invoke(this, new FloatEventArgs(_value_show)) ?? (useSystemText ? (((Control)this).Text ?? "") : ((_value_show * 100f).ToString("F" + ShowTextDot) + TextUnit));
			g.String(text2, ((Control)this).Font, (Brush)(object)val5, rect, s_c);
		}
		finally
		{
			((IDisposable)val5)?.Dispose();
		}
	}

	private void PaintProgress(Canvas g, float radius, Rectangle rect, Color back, Color color)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		GraphicsPath val = rect.RoundPath(radius);
		try
		{
			g.Fill(back, val);
			bool flag = true;
			if (_value_show > 0f)
			{
				float num = (float)rect.Width * _value_show;
				if (num > radius)
				{
					GraphicsPath val2 = new RectangleF(rect.X, rect.Y, num, rect.Height).RoundPath(radius);
					try
					{
						g.Fill(color, val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
					if (loading && AnimationLoadingValue > 0f)
					{
						flag = false;
						float alpha = 60f * (1f - AnimationLoadingValue);
						GraphicsPath val3 = new RectangleF(rect.X, rect.Y, num * AnimationLoadingValue, rect.Height).RoundPath(radius);
						try
						{
							g.Fill(Helper.ToColor(alpha, Colour.BgBase.Get("Progress")), val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
				}
				else
				{
					Bitmap val4 = new Bitmap(rect.Width, rect.Height);
					try
					{
						using (Canvas canvas = Graphics.FromImage((Image)(object)val4).High())
						{
							GraphicsPath val5 = new RectangleF(0f - num, 0f, num * 2f, rect.Height).RoundPath(radius);
							try
							{
								canvas.Fill(color, val5);
							}
							finally
							{
								((IDisposable)val5)?.Dispose();
							}
							if (loading && AnimationLoadingValue > 0f)
							{
								flag = false;
								float alpha2 = 60f * (1f - AnimationLoadingValue);
								GraphicsPath val6 = new RectangleF(0f - num, 0f, num * 2f * AnimationLoadingValue, rect.Height).RoundPath(radius);
								try
								{
									canvas.Fill(Helper.ToColor(alpha2, Colour.BgBase.Get("Progress")), val6);
								}
								finally
								{
									((IDisposable)val6)?.Dispose();
								}
							}
						}
						TextureBrush val7 = new TextureBrush((Image)(object)val4, (WrapMode)4);
						try
						{
							val7.TranslateTransform((float)rect.X, (float)rect.Y);
							g.Fill((Brush)(object)val7, val);
						}
						finally
						{
							((IDisposable)val7)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
			}
			if (!(loading && AnimationLoadingValue > 0f && flag) || !LoadingFull)
			{
				return;
			}
			SolidBrush val8 = new SolidBrush(Helper.ToColor(80f * (1f - AnimationLoadingValue), Colour.BgBase.Get("Progress")));
			try
			{
				GraphicsPath val9 = new RectangleF(rect.X, rect.Y, (float)rect.Width * AnimationLoadingValue, rect.Height).RoundPath(radius);
				try
				{
					g.Fill((Brush)(object)val8, val9);
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
			((IDisposable)val)?.Dispose();
		}
	}
}
