using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace AntdUI;

[Description("Rate 评分")]
[ToolboxItem(true)]
[DefaultProperty("Value")]
[DefaultEvent("ValueChanged")]
public class Rate : IControl
{
	private class RectStar
	{
		private Rate rate;

		internal bool hover;

		internal bool active;

		internal bool half;

		internal float AnimationActiveValueO;

		internal float AnimationActiveValueS;

		internal bool AnimationActive;

		private ITask? ThreadActive;

		public Rectangle rect_mouse { get; set; }

		public Rectangle rect { get; set; }

		public Rectangle rect_i { get; set; }

		public RectStar(Rate _rate, Rectangle _rect_mouse, Rectangle _rect, int msize, int msize2)
		{
			rate = _rate;
			rect_mouse = _rect_mouse;
			rect = _rect;
			rect_i = new Rectangle(_rect.X + msize2, _rect.Y + msize2, _rect.Width - msize, _rect.Height - msize);
		}

		internal bool Animatio(bool _active, bool _hover, bool _half)
		{
			if (active == _active && hover == _hover)
			{
				if (half != _half)
				{
					half = _half;
				}
				((Control)rate).Invalidate();
				return false;
			}
			bool flag = active;
			bool flag2 = hover;
			active = _active;
			hover = _hover;
			half = _half;
			if (Config.Animation)
			{
				ThreadActive?.Dispose();
				AnimationActive = true;
				int t = Animation.TotalFrames(10, 100);
				if (active || hover)
				{
					if (active && hover)
					{
						ThreadActive = new ITask(delegate(int i)
						{
							AnimationActiveValueS = (AnimationActiveValueO = Animation.Animate(i, t, 1f, AnimationType.Ball));
							((Control)rate).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationActive = false;
							AnimationActiveValueS = (AnimationActiveValueO = 1f);
							((Control)rate).Invalidate();
						});
					}
					else if (flag && flag2)
					{
						ThreadActive = new ITask(delegate(int i)
						{
							AnimationActiveValueS = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)rate).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationActive = false;
							AnimationActiveValueS = 0f;
							AnimationActiveValueO = 1f;
							((Control)rate).Invalidate();
						});
					}
					else
					{
						ThreadActive = new ITask(delegate(int i)
						{
							AnimationActiveValueO = Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)rate).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationActive = false;
							AnimationActiveValueS = 0f;
							AnimationActiveValueO = 1f;
							((Control)rate).Invalidate();
						});
					}
				}
				else if (flag && !flag2)
				{
					ThreadActive = new ITask(delegate(int i)
					{
						AnimationActiveValueO = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
						((Control)rate).Invalidate();
						return true;
					}, 10, t, delegate
					{
						AnimationActive = false;
						AnimationActiveValueS = (AnimationActiveValueO = 0f);
						((Control)rate).Invalidate();
					});
				}
				else
				{
					ThreadActive = new ITask(delegate(int i)
					{
						AnimationActiveValueS = (AnimationActiveValueO = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball));
						((Control)rate).Invalidate();
						return true;
					}, 10, t, delegate
					{
						AnimationActive = false;
						AnimationActiveValueS = (AnimationActiveValueO = 0f);
						((Control)rate).Invalidate();
					});
				}
			}
			else
			{
				((Control)rate).Invalidate();
			}
			return true;
		}
	}

	private Color fill = Color.FromArgb(250, 219, 20);

	private int count = 5;

	private float _value;

	private string? character;

	private Bitmap? icon;

	private Bitmap? icon_active;

	private RectStar[] rect_stars = new RectStar[0];

	private bool autoSize;

	private TooltipForm? tooltipForm;

	private string? tooltipText;

	private bool setvalue;

	[Description("颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "250, 219, 20")]
	public Color Fill
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
				Bitmap? obj = icon_active;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				icon_active = null;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("支持清除")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AllowClear { get; set; }

	[Description("是否允许半选")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AllowHalf { get; set; }

	[Description("Star 总数")]
	[Category("外观")]
	[DefaultValue(5)]
	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			if (count != value)
			{
				count = value;
				IOnSizeChanged();
				((Control)this).Invalidate();
			}
		}
	}

	[Description("当前值")]
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
			_value = value;
			if (rect_stars.Length != 0)
			{
				int num = (int)_value;
				for (int i = 0; i < rect_stars.Length; i++)
				{
					bool flag = num > i;
					bool flag2 = _value > (float)i;
					rect_stars[i].Animatio(flag2, rect_stars[i].hover, flag2 && !flag);
				}
			}
			((Control)this).Invalidate();
			this.ValueChanged?.Invoke(this, new FloatEventArgs(_value));
			OnPropertyChanged("Value");
		}
	}

	[Description("自定义每项的提示信息")]
	[Category("数据")]
	[DefaultValue(null)]
	public string[]? Tooltips { get; set; }

	[Description("自定义字符")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Character
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationCharacter, character);
		}
		set
		{
			if (!(character == value))
			{
				character = value;
				Bitmap? obj = icon;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				Bitmap? obj2 = icon_active;
				if (obj2 != null)
				{
					((Image)obj2).Dispose();
				}
				icon = (icon_active = null);
				((Control)this).Invalidate();
			}
		}
	}

	[Description("自定义字符")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationCharacter { get; set; }

	[Browsable(true)]
	[Description("自动宽度")]
	[Category("外观")]
	[DefaultValue(false)]
	public override bool AutoSize
	{
		get
		{
			return autoSize;
		}
		set
		{
			if (autoSize != value)
			{
				autoSize = value;
				if (value)
				{
					IOnSizeChanged();
				}
			}
		}
	}

	[Description("Value 属性值更改时发生")]
	[Category("行为")]
	public event FloatEventHandler? ValueChanged;

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Expected O, but got Unknown
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected O, but got Unknown
		Rectangle rectangle = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		if (rectangle.Width == 0 || rectangle.Height == 0)
		{
			return;
		}
		if (count < 1)
		{
			((Control)this).OnPaint(e);
			return;
		}
		int height = rectangle.Height;
		Canvas canvas = e.Graphics.High();
		string text = Character;
		if (icon == null || ((Image)icon).Width != height)
		{
			Bitmap? obj = icon;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			icon = (text ?? SvgDb.IcoStar).SvgToBmp(height, height, Colour.FillSecondary.Get("Rate"));
		}
		if (icon_active == null || ((Image)icon_active).Width != height)
		{
			Bitmap? obj2 = icon_active;
			if (obj2 != null)
			{
				((Image)obj2).Dispose();
			}
			icon_active = (text ?? SvgDb.IcoStar).SvgToBmp(height, height, fill);
		}
		if (icon == null || icon_active == null)
		{
			icon = new Bitmap(height, height);
			icon_active = new Bitmap(height, height);
			Font val = new Font(((Control)this).Font.FontFamily, (float)height, ((Control)this).Font.Style);
			try
			{
				Size size = canvas.MeasureString(text, val);
				int num = ((size.Width > size.Height) ? size.Width : size.Height);
				Bitmap val2 = new Bitmap(num, num);
				try
				{
					Bitmap val3 = new Bitmap(num, num);
					try
					{
						Rectangle rect = new Rectangle(0, 0, num, num);
						Rectangle rect2 = new Rectangle(0, 0, height, height);
						StringFormat val4 = Helper.SF((StringAlignment)1, (StringAlignment)1);
						try
						{
							using (Canvas canvas2 = Graphics.FromImage((Image)(object)val2).HighLay(text: true))
							{
								SolidBrush val5 = new SolidBrush(Colour.FillSecondary.Get("Rate"));
								try
								{
									canvas2.String(text, val, (Brush)(object)val5, rect, val4);
								}
								finally
								{
									((IDisposable)val5)?.Dispose();
								}
							}
							using Canvas canvas3 = Graphics.FromImage((Image)(object)val3).HighLay(text: true);
							SolidBrush val6 = new SolidBrush(fill);
							try
							{
								canvas3.String(text, val, (Brush)(object)val6, rect, val4);
							}
							finally
							{
								((IDisposable)val6)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
						using (Canvas canvas4 = Graphics.FromImage((Image)(object)icon).High())
						{
							canvas4.Image(val2, rect2);
						}
						using Canvas canvas5 = Graphics.FromImage((Image)(object)icon_active).High();
						canvas5.Image(val3, rect2);
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
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		for (int i = 0; i < rect_stars.Length; i++)
		{
			RectStar rectStar = rect_stars[i];
			if (rectStar.AnimationActive)
			{
				int num2 = (int)((float)(rectStar.rect.Width - rectStar.rect_i.Width) * rectStar.AnimationActiveValueS);
				int num3 = rectStar.rect_i.Width + num2;
				int num4 = num2 / 2;
				Rectangle rectangle2 = new Rectangle(rectStar.rect_i.X - num4, rectStar.rect_i.Y - num4, num3, num3);
				canvas.Image(icon, rectangle2);
				ImageAttributes val7 = new ImageAttributes();
				try
				{
					ColorMatrix val8 = new ColorMatrix
					{
						Matrix33 = rectStar.AnimationActiveValueO
					};
					val7.SetColorMatrix(val8, (ColorMatrixFlag)0, (ColorAdjustType)1);
					if (rectStar.half)
					{
						canvas.Image((Image)(object)icon_active, new Rectangle(rectangle2.X, rectangle2.Y, rectangle2.Width / 2, rectangle2.Height), 0, 0, ((Image)icon_active).Width / 2, ((Image)icon_active).Height, (GraphicsUnit)2, val7);
					}
					else
					{
						canvas.Image((Image)(object)icon_active, rectangle2, 0, 0, ((Image)icon_active).Width, ((Image)icon_active).Height, (GraphicsUnit)2, val7);
					}
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			else if (rectStar.hover)
			{
				if (rectStar.half)
				{
					canvas.Image(icon, rectStar.rect);
					canvas.Image((Image)(object)icon_active, new Rectangle(rectStar.rect.X, rectStar.rect.Y, rectStar.rect.Width / 2, rectStar.rect.Height), new Rectangle(0, 0, ((Image)icon_active).Width / 2, ((Image)icon_active).Height), (GraphicsUnit)2);
				}
				else
				{
					canvas.Image(icon_active, rectStar.rect);
				}
			}
			else if (rectStar.active)
			{
				if (rectStar.half)
				{
					canvas.Image(icon, rectStar.rect_i);
					canvas.Image((Image)(object)icon_active, new Rectangle(rectStar.rect_i.X, rectStar.rect_i.Y, rectStar.rect_i.Width / 2, rectStar.rect_i.Height), new Rectangle(0, 0, ((Image)icon_active).Width / 2, ((Image)icon_active).Height), (GraphicsUnit)2);
				}
				else
				{
					canvas.Image(icon_active, rectStar.rect_i);
				}
			}
			else
			{
				canvas.Image(icon, rectStar.rect_i);
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).OnSizeChanged(e);
		Rectangle rectangle = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		if (rectangle.Width == 0 || rectangle.Height == 0 || count < 1)
		{
			return;
		}
		int height = rectangle.Height;
		int num = height - (int)((float)height * 0.8f);
		int msize = num / 2;
		int num2 = (int)(8f * Config.Dpi);
		int num3 = height + num2;
		List<RectStar> list = new List<RectStar>(count);
		int num4 = (int)_value;
		for (int i = 0; i < count; i++)
		{
			bool flag = num4 > i;
			bool flag2 = _value > (float)i;
			RectStar rectStar = new RectStar(this, new Rectangle(rectangle.X + num3 * i, rectangle.Y, num3, height), new Rectangle(rectangle.X + num3 * i, rectangle.Y, height, height), num, msize)
			{
				active = flag2
			};
			if (flag2 && !flag)
			{
				rectStar.half = true;
			}
			list.Add(rectStar);
		}
		rect_stars = list.ToArray();
		if (!autoSize)
		{
			return;
		}
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				((Control)this).Width = list[list.Count - 1].rect.Right;
			});
		}
		else
		{
			((Control)this).Width = list[list.Count - 1].rect.Right;
		}
	}

	private void ShowTips(Rectangle dot_rect, string text)
	{
		if (!(text == tooltipText) || tooltipForm == null)
		{
			tooltipText = text;
			Rectangle rectangle = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
			Rectangle rect = new Rectangle(rectangle.X + dot_rect.X, rectangle.Y + dot_rect.Y, dot_rect.Width, dot_rect.Height);
			if (tooltipForm == null)
			{
				tooltipForm = new TooltipForm((Control)(object)this, rect, tooltipText, new TooltipConfig
				{
					Font = ((Control)this).Font,
					ArrowAlign = TAlign.Top
				});
				((Form)tooltipForm).Show((IWin32Window)(object)this);
			}
			else
			{
				tooltipForm.SetText(rect, tooltipText);
			}
		}
	}

	private void CloseTips()
	{
		tooltipForm?.IClose();
		tooltipForm = null;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (setvalue)
		{
			setvalue = false;
			return;
		}
		for (int i = 0; i < rect_stars.Length; i++)
		{
			RectStar rectStar = rect_stars[i];
			if (!rectStar.rect_mouse.Contains(e.Location))
			{
				continue;
			}
			bool half = false;
			if (AllowHalf)
			{
				half = new Rectangle(rectStar.rect.X, rectStar.rect.Y, rectStar.rect.Width / 2, rectStar.rect.Height).Contains(e.Location);
			}
			rectStar.Animatio(_active: true, _hover: true, half);
			for (int j = 0; j < rect_stars.Length; j++)
			{
				if (i != j)
				{
					rect_stars[j].Animatio(j < i, _hover: false, _half: false);
				}
			}
			if (Tooltips != null && Tooltips.Length > i)
			{
				ShowTips(rectStar.rect, Tooltips[i]);
			}
			else
			{
				CloseTips();
			}
			return;
		}
		_Leave();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		_Leave();
	}

	private void _Leave()
	{
		CloseTips();
		int num = (int)_value;
		for (int i = 0; i < rect_stars.Length; i++)
		{
			bool flag = num > i;
			bool flag2 = _value > (float)i;
			rect_stars[i].Animatio(flag2, _hover: false, flag2 && !flag);
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			for (int i = 0; i < rect_stars.Length; i++)
			{
				if (!rect_stars[i].rect_mouse.Contains(e.Location))
				{
					continue;
				}
				float num = (AllowClear ? _value : (-10f));
				RectStar rectStar = rect_stars[i];
				if (AllowHalf && new Rectangle(rectStar.rect.X, rectStar.rect.Y, rectStar.rect.Width / 2, rectStar.rect.Height).Contains(e.Location))
				{
					float num2 = (float)i + 0.5f;
					if (num2 == num)
					{
						Value = 0f;
						setvalue = true;
						_Leave();
					}
					else
					{
						Value = num2;
					}
				}
				else
				{
					int num3 = i + 1;
					if ((float)num3 == num)
					{
						Value = 0f;
						setvalue = true;
						_Leave();
					}
					else
					{
						Value = num3;
					}
				}
				return;
			}
		}
		((Control)this).OnMouseClick(e);
	}

	protected override void Dispose(bool disposing)
	{
		Bitmap? obj = icon;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		Bitmap? obj2 = icon_active;
		if (obj2 != null)
		{
			((Image)obj2).Dispose();
		}
		base.Dispose(disposing);
	}
}
