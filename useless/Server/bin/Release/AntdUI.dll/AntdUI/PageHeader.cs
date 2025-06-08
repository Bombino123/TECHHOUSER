using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;
using Vanara.PInvoke;

namespace AntdUI;

[Description("PageHeader 页头")]
[ToolboxItem(true)]
[Designer(typeof(IControlDesigner))]
public class PageHeader : IControl, IEventListener
{
	private TAMode mode;

	private string? text;

	private string? desc;

	private Font? descFont;

	private string? description;

	private int? gap;

	private int subGap = 6;

	private bool useSystemStyleColor;

	private bool cancelButton;

	private bool showicon;

	private Image? icon;

	private string? iconSvg;

	private bool loading;

	private int AnimationLoadingValue;

	private ITask? ThreadLoading;

	private bool AnimationBack;

	private float AnimationBackValue;

	private bool showback;

	private bool showButton;

	private bool fullBox;

	private bool maximizeBox = true;

	private bool minimizeBox = true;

	private bool isMax;

	private bool isfull;

	private bool showDivider;

	private Color? dividerColor;

	private float dividerthickness = 1f;

	private int dividerMargin;

	private string? backExtend;

	private StringFormat stringLeft = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private StringFormat stringCenter = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private Bitmap? temp_logo;

	private Bitmap? temp_back;

	private Bitmap? temp_back_hover;

	private Bitmap? temp_back_down;

	private Bitmap? temp_full;

	private Bitmap? temp_full_restore;

	private Bitmap? temp_min;

	private Bitmap? temp_max;

	private Bitmap? temp_restore;

	private Bitmap? temp_close;

	private Bitmap? temp_close_hover;

	private int hasr;

	private ITask? ThreadBack;

	private ITaskOpacity hove_back;

	private ITaskOpacity hove_close;

	private ITaskOpacity hove_full;

	private ITaskOpacity hove_max;

	private ITaskOpacity hove_min;

	private Rectangle rect_back;

	private Rectangle rect_close;

	private Rectangle rect_full;

	private Rectangle rect_max;

	private Rectangle rect_min;

	[Description("色彩模式")]
	[Category("外观")]
	[DefaultValue(TAMode.Auto)]
	public TAMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				DisposeBmp();
				((Control)this).Invalidate();
				OnPropertyChanged("Mode");
			}
		}
	}

	[Description("文字")]
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
				((Control)this).Invalidate();
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文字")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("使用标题大小")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool UseTitleFont { get; set; }

	[Description("标题使用粗体")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool UseTextBold { get; set; } = true;


	[Description("副标题居中")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool UseSubCenter { get; set; }

	[Description("副标题")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? SubText
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationSubText, desc);
		}
		set
		{
			if (!(desc == value))
			{
				desc = value;
				((Control)this).Invalidate();
				OnPropertyChanged("SubText");
			}
		}
	}

	[Description("副标题字体")]
	[Category("外观")]
	[DefaultValue(null)]
	public Font? SubFont
	{
		get
		{
			return descFont;
		}
		set
		{
			if (descFont != value)
			{
				descFont = value;
				((Control)this).Invalidate();
				OnPropertyChanged("SubFont");
			}
		}
	}

	[Description("副标题")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSubText { get; set; }

	[Description("描述文本")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? Description
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationDescription, description);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
			if (!(description == value))
			{
				description = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Description");
			}
		}
	}

	[Description("描述文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationDescription { get; set; }

	[Description("间隔")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? Gap
	{
		get
		{
			return gap;
		}
		set
		{
			if (gap != value)
			{
				gap = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("副标题与标题间隔")]
	[Category("外观")]
	[DefaultValue(6)]
	public int SubGap
	{
		get
		{
			return subGap;
		}
		set
		{
			if (subGap != value)
			{
				subGap = value;
				((Control)this).Invalidate();
				OnPropertyChanged("SubGap");
			}
		}
	}

	[Description("使用系统颜色")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool UseSystemStyleColor
	{
		get
		{
			return useSystemStyleColor;
		}
		set
		{
			if (useSystemStyleColor != value)
			{
				useSystemStyleColor = value;
				DisposeBmp();
				((Control)this).Invalidate();
				OnPropertyChanged("UseSystemStyleColor");
			}
		}
	}

	[Description("点击退出关闭")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool CancelButton
	{
		get
		{
			return cancelButton;
		}
		set
		{
			if (cancelButton != value)
			{
				cancelButton = value;
				if (((Control)this).IsHandleCreated)
				{
					HandCancelButton(value);
				}
			}
		}
	}

	[Description("是否显示图标")]
	[Category("外观")]
	[DefaultValue(false)]
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
				((Control)this).Invalidate();
				OnPropertyChanged("ShowIcon");
			}
		}
	}

	[Description("图标")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Icon
	{
		get
		{
			return icon;
		}
		set
		{
			if (icon != value)
			{
				icon = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Icon");
			}
		}
	}

	[Description("图标SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? IconSvg
	{
		get
		{
			return iconSvg;
		}
		set
		{
			if (!(iconSvg == value))
			{
				iconSvg = value;
				((Control)this).Invalidate();
				OnPropertyChanged("IconSvg");
			}
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
			if (loading)
			{
				ThreadLoading = new ITask((Control)(object)this, delegate
				{
					AnimationLoadingValue += 6;
					if (AnimationLoadingValue > 360)
					{
						AnimationLoadingValue = 0;
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

	[Description("是否显示返回按钮")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ShowBack
	{
		get
		{
			return showback;
		}
		set
		{
			if (showback == value)
			{
				return;
			}
			showback = value;
			if (Config.Animation && ((Control)this).IsHandleCreated)
			{
				ThreadBack?.Dispose();
				AnimationBack = true;
				int t = Animation.TotalFrames(10, 200);
				Rectangle clientRectangle = ((Control)this).ClientRectangle;
				Rectangle rect = new Rectangle(clientRectangle.X, clientRectangle.Y, clientRectangle.Width - hasr, clientRectangle.Height);
				if (value)
				{
					ThreadBack = new ITask(delegate(int i)
					{
						AnimationBackValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
						((Control)this).Invalidate(rect);
						return true;
					}, 10, t, delegate
					{
						AnimationBackValue = 1f;
						AnimationBack = false;
						((Control)this).Invalidate();
					});
				}
				else
				{
					ThreadBack = new ITask(delegate(int i)
					{
						AnimationBackValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
						((Control)this).Invalidate(rect);
						return true;
					}, 10, t, delegate
					{
						AnimationBackValue = 0f;
						AnimationBack = false;
						((Control)this).Invalidate();
					});
				}
			}
			else
			{
				AnimationBackValue = (value ? 1f : 0f);
				((Control)this).Invalidate();
			}
			OnPropertyChanged("ShowBack");
		}
	}

	[Description("是否显示标题栏按钮")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool ShowButton
	{
		get
		{
			return showButton;
		}
		set
		{
			if (showButton != value)
			{
				showButton = value;
				IOnSizeChanged();
				((Control)this).Invalidate();
				OnPropertyChanged("ShowButton");
			}
		}
	}

	[Description("是否显示全屏按钮")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool FullBox
	{
		get
		{
			return fullBox;
		}
		set
		{
			if (fullBox != value)
			{
				fullBox = value;
				if (showButton)
				{
					IOnSizeChanged();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("FullBox");
			}
		}
	}

	[Description("是否显示最大化按钮")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool MaximizeBox
	{
		get
		{
			return maximizeBox;
		}
		set
		{
			if (maximizeBox != value)
			{
				maximizeBox = value;
				if (showButton)
				{
					IOnSizeChanged();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("MaximizeBox");
			}
		}
	}

	[Description("是否显示最小化按钮")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool MinimizeBox
	{
		get
		{
			return minimizeBox;
		}
		set
		{
			if (minimizeBox != value)
			{
				minimizeBox = value;
				if (showButton)
				{
					IOnSizeChanged();
					((Control)this).Invalidate();
				}
				OnPropertyChanged("MinimizeBox");
			}
		}
	}

	[Description("是否最大化")]
	[Category("外观")]
	[DefaultValue(false)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsMax
	{
		get
		{
			return isMax;
		}
		set
		{
			if (isMax != value)
			{
				isMax = value;
				if (showButton)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("是否全屏")]
	[Category("外观")]
	[DefaultValue(false)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsFull
	{
		get
		{
			return isfull;
		}
		set
		{
			if (isfull != value)
			{
				isfull = value;
				if (showButton)
				{
					((Control)this).Invalidate();
				}
			}
		}
	}

	[Description("是否可以拖动位置")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool DragMove { get; set; } = true;


	[Description("关闭按钮大小")]
	[Category("行为")]
	[DefaultValue(48)]
	public int CloseSize { get; set; } = 48;


	[Description("显示线")]
	[Category("线")]
	[DefaultValue(false)]
	public bool DividerShow
	{
		get
		{
			return showDivider;
		}
		set
		{
			if (showDivider != value)
			{
				showDivider = value;
				((Control)this).Invalidate();
				OnPropertyChanged("DividerShow");
			}
		}
	}

	[Description("线颜色")]
	[Category("线")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? DividerColor
	{
		get
		{
			return dividerColor;
		}
		set
		{
			if (!(dividerColor == value))
			{
				dividerColor = value;
				if (showDivider)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("DividerColor");
			}
		}
	}

	[Description("线厚度")]
	[Category("线")]
	[DefaultValue(1f)]
	public float DividerThickness
	{
		get
		{
			return dividerthickness;
		}
		set
		{
			if (dividerthickness != value)
			{
				dividerthickness = value;
				if (showDivider)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("DividerThickness");
			}
		}
	}

	[Description("线边距")]
	[Category("线")]
	[DefaultValue(0)]
	public int DividerMargin
	{
		get
		{
			return dividerMargin;
		}
		set
		{
			if (dividerMargin != value)
			{
				dividerMargin = value;
				if (showDivider)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("DividerMargin");
			}
		}
	}

	[Description("背景渐变色")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? BackExtend
	{
		get
		{
			return backExtend;
		}
		set
		{
			if (!(backExtend == value))
			{
				backExtend = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackExtend");
			}
		}
	}

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, 0, 0, hasr, 0);

	public event EventHandler? BackClick;

	private void HandCancelButton(bool value)
	{
		Form val = ((Control)this).Parent.FindPARENT();
		BaseForm formb = val as BaseForm;
		if (formb == null)
		{
			return;
		}
		if (value)
		{
			formb.ONESC = delegate
			{
				if (showback && this.BackClick != null)
				{
					this.BackClick(this, EventArgs.Empty);
				}
				else
				{
					((Form)formb).Close();
				}
			};
		}
		else
		{
			formb.ONESC = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadBack?.Dispose();
		hove_back.Dispose();
		hove_close.Dispose();
		hove_full.Dispose();
		hove_max.Dispose();
		hove_min.Dispose();
		ThreadLoading?.Dispose();
		Bitmap? obj = temp_logo;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		Bitmap? obj2 = temp_back;
		if (obj2 != null)
		{
			((Image)obj2).Dispose();
		}
		Bitmap? obj3 = temp_back_hover;
		if (obj3 != null)
		{
			((Image)obj3).Dispose();
		}
		Bitmap? obj4 = temp_back_down;
		if (obj4 != null)
		{
			((Image)obj4).Dispose();
		}
		Bitmap? obj5 = temp_full;
		if (obj5 != null)
		{
			((Image)obj5).Dispose();
		}
		Bitmap? obj6 = temp_full_restore;
		if (obj6 != null)
		{
			((Image)obj6).Dispose();
		}
		Bitmap? obj7 = temp_min;
		if (obj7 != null)
		{
			((Image)obj7).Dispose();
		}
		Bitmap? obj8 = temp_max;
		if (obj8 != null)
		{
			((Image)obj8).Dispose();
		}
		Bitmap? obj9 = temp_restore;
		if (obj9 != null)
		{
			((Image)obj9).Dispose();
		}
		Bitmap? obj10 = temp_close;
		if (obj10 != null)
		{
			((Image)obj10).Dispose();
		}
		Bitmap? obj11 = temp_close_hover;
		if (obj11 != null)
		{
			((Image)obj11).Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width == 0 || clientRectangle.Height == 0)
		{
			return;
		}
		Rectangle rect_real = clientRectangle.PaddingRect(((Control)this).Padding, 0, 0, hasr, 0);
		Canvas canvas = e.Graphics.High();
		backExtend.BrushEx(clientRectangle, canvas);
		Color fore = Colour.Text.Get("PageHeader", mode);
		Color forebase = Colour.TextBase.Get("PageHeader", mode);
		Color foreSecondary = Colour.TextSecondary.Get("PageHeader", mode);
		Color fillsecondary = Colour.FillSecondary.Get("PageHeader", mode);
		if (useSystemStyleColor)
		{
			forebase = ((Control)this).ForeColor;
		}
		if (UseTitleFont)
		{
			Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 1.44f, (FontStyle)(UseTextBold ? 1 : ((int)((Control)this).Font.Style)));
			try
			{
				IPaint(canvas, clientRectangle, rect_real, canvas.MeasureString("龍Qq", ((Control)this).Font), 1.36f, val, fore, forebase, foreSecondary, fillsecondary);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			IPaint(canvas, clientRectangle, rect_real, canvas.MeasureString(((Control)this).Text ?? "龍Qq", ((Control)this).Font), 1f, null, fore, forebase, foreSecondary, fillsecondary);
		}
		this.PaintBadge(canvas);
		if (showDivider)
		{
			int num = (int)(dividerthickness * Config.Dpi);
			int num2 = (int)((float)dividerMargin * Config.Dpi);
			SolidBrush val2 = dividerColor.Brush(Colour.Split.Get("PageHeader"));
			try
			{
				canvas.Fill((Brush)(object)val2, new Rectangle(clientRectangle.X + num2, clientRectangle.Bottom - num, clientRectangle.Width - num2 * 2, num));
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		((Control)this).OnPaint(e);
	}

	private void IPaint(Canvas g, Rectangle rect, Rectangle rect_real, Size size, float ratio, Font? fontTitle, Color fore, Color forebase, Color foreSecondary, Color fillsecondary)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		bool flag = false;
		int num = rect_real.Height;
		if (Description != null)
		{
			flag = true;
			num = rect_real.Height / 3;
			rect_real = new Rectangle(rect_real.X, rect_real.Y, rect_real.Width, rect_real.Height - num);
		}
		int num2 = IPaint(g, rect_real, fore, size.Height, ratio);
		rect_real.X += num2;
		rect_real.Width -= num2;
		SolidBrush val = new SolidBrush(forebase);
		try
		{
			int width = size.Width;
			if (fontTitle == null)
			{
				g.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val, rect_real, stringLeft);
			}
			else
			{
				Size size2 = g.MeasureString(((Control)this).Text, fontTitle);
				g.String(((Control)this).Text, fontTitle, (Brush)(object)val, rect_real, stringLeft);
				width = size2.Width;
			}
			if (SubText != null)
			{
				int num3 = width + (int)((float)subGap * Config.Dpi);
				SolidBrush val2 = new SolidBrush(foreSecondary);
				try
				{
					if (UseSubCenter)
					{
						g.String(SubText, descFont ?? ((Control)this).Font, (Brush)(object)val2, rect, stringCenter);
					}
					else
					{
						g.String(SubText, descFont ?? ((Control)this).Font, (Brush)(object)val2, new Rectangle(rect_real.X + num3, rect_real.Y, rect_real.Width - num3, rect_real.Height), stringLeft);
					}
					if (flag)
					{
						g.String(Description, ((Control)this).Font, (Brush)(object)val2, new Rectangle(rect_real.X, rect_real.Bottom, rect_real.Width, num), stringLeft);
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			else if (flag)
			{
				SolidBrush val3 = new SolidBrush(foreSecondary);
				try
				{
					g.String(Description, ((Control)this).Font, (Brush)(object)val3, new Rectangle(rect_real.X, rect_real.Bottom, rect_real.Width, num), stringLeft);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (showButton)
		{
			IPaintButton(g, rect_real, fore, fillsecondary, size);
		}
	}

	public Rectangle GetTitleRect(Canvas g)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		Rectangle rect = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, 0, 0, hasr, 0);
		Size size = g.MeasureString(((Control)this).Text ?? "龍Qq", ((Control)this).Font);
		if (UseTitleFont)
		{
			Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 1.44f, (FontStyle)(UseTextBold ? 1 : ((int)((Control)this).Font.Style)));
			try
			{
				Size size2 = g.MeasureString(((Control)this).Text, val);
				rect.X += IPaintS(g, rect, size.Height, 1.36f) / 2;
				return new Rectangle(rect.X, rect.Y + (rect.Height - size2.Height) / 2, size2.Width, size2.Height);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		rect.X += IPaintS(g, rect, size.Height, 1f) / 2;
		return new Rectangle(rect.X, rect.Y + (rect.Height - size.Height) / 2, size.Width, size.Height);
	}

	private int IPaintS(Canvas g, Rectangle rect, int sHeight, float icon_ratio)
	{
		int num = 0;
		int num2 = (int)(gap.HasValue ? ((float)gap.Value * Config.Dpi) : ((float)sHeight * 0.6f));
		int num3 = (int)Math.Round((float)sHeight * 0.72f);
		if (showback || AnimationBack)
		{
			int num4 = num3 + num2;
			if (AnimationBack)
			{
				num4 = (int)((float)num4 * AnimationBackValue);
			}
			num += num4;
		}
		if (loading)
		{
			num3 = sHeight;
			num += num3 + num2;
		}
		else if (showicon)
		{
			num3 = ((icon_ratio == 1f) ? sHeight : ((int)Math.Round((float)sHeight * icon_ratio)));
			num += num3 + num2;
		}
		return num + num2;
	}

	private int IPaint(Canvas g, Rectangle rect, Color fore, int sHeight, float icon_ratio)
	{
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = (int)(gap.HasValue ? ((float)gap.Value * Config.Dpi) : ((float)sHeight * 0.6f));
		int num3 = (int)Math.Round((float)sHeight * 0.72f);
		if (showback || AnimationBack)
		{
			int num4 = num3 + num2;
			if (AnimationBack)
			{
				num4 = (int)((float)num4 * AnimationBackValue);
			}
			if (showback)
			{
				rect_back = new Rectangle(rect.X + num, rect.Y, num4 + num2, rect.Height);
				Rectangle rect_icon = new Rectangle(rect.X + num + num2, rect.Y + (rect.Height - num3) / 2, num3, num3);
				if (hove_back.Down)
				{
					PrintBackDown(g, rect_icon);
				}
				else if (hove_back.Animation)
				{
					PrintBackHover(g, fore, rect_icon);
				}
				else if (hove_back.Switch)
				{
					PrintBackHover(g, rect_icon);
				}
				else
				{
					PrintBack(g, fore, rect_icon);
				}
			}
			num += num4;
		}
		if (loading)
		{
			num3 = sHeight;
			Rectangle rect2 = new Rectangle(rect.X + num + num2, rect.Y + (rect.Height - num3) / 2, num3, num3);
			Pen val = new Pen(Colour.Fill.Get("PageHeader"), (float)sHeight * 0.14f);
			try
			{
				Pen val2 = new Pen(Color.FromArgb(170, fore), val.Width);
				try
				{
					g.DrawEllipse(val, rect2);
					LineCap startCap = (LineCap)2;
					val2.EndCap = (LineCap)2;
					val2.StartCap = startCap;
					g.DrawArc(val2, rect2, AnimationLoadingValue, 100f);
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
			num += num3 + num2;
		}
		else if (showicon)
		{
			num3 = ((icon_ratio == 1f) ? sHeight : ((int)Math.Round((float)sHeight * icon_ratio)));
			Rectangle rectangle = new Rectangle(rect.X + num + num2, rect.Y + (rect.Height - num3) / 2, num3, num3);
			bool flag = false;
			if (iconSvg != null && PrintLogo(g, iconSvg, fore, rectangle))
			{
				flag = true;
			}
			if (!flag)
			{
				if (icon != null)
				{
					g.Image(icon, rectangle);
					flag = true;
				}
				else
				{
					Form val3 = ((Control)this).Parent.FindPARENT();
					if (val3 != null && val3.Icon != null)
					{
						g.Icon(val3.Icon, rectangle);
						flag = true;
					}
				}
			}
			num += num3 + num2;
		}
		return num + num2;
	}

	private void IPaintButton(Canvas g, Rectangle rect, Color fore, Color fillsecondary, Size size)
	{
		int num = (int)((float)size.Height * 1.2f);
		int num2 = (rect_close.Width - num) / 2;
		int num3 = (rect_close.Height - num) / 2;
		Rectangle rectangle = new Rectangle(rect_close.X + num2, rect_close.Y + num3, num, num);
		if (hove_close.Down)
		{
			g.Fill(Colour.ErrorActive.Get("PageHeader"), rect_close);
			PrintCloseHover(g, rectangle);
		}
		else if (hove_close.Animation)
		{
			g.Fill(Helper.ToColor(hove_close.Value, Colour.Error.Get("PageHeader")), rect_close);
			PrintClose(g, fore, rectangle);
			g.GetImgExtend(SvgDb.IcoAppClose, rectangle, Helper.ToColor(hove_close.Value, Colour.ErrorColor.Get("PageHeader")));
		}
		else if (hove_close.Switch)
		{
			g.Fill(Colour.Error.Get("PageHeader"), rect_close);
			PrintCloseHover(g, rectangle);
		}
		else
		{
			PrintClose(g, fore, rectangle);
		}
		if (fullBox)
		{
			Rectangle rect_icon = new Rectangle(rect_full.X + num2, rect_full.Y + num3, num, num);
			if (hove_full.Animation)
			{
				g.Fill(Helper.ToColor(hove_full.Value, fillsecondary), rect_full);
			}
			else if (hove_full.Switch)
			{
				g.Fill(fillsecondary, rect_full);
			}
			if (hove_full.Down)
			{
				g.Fill(fillsecondary, rect_full);
			}
			if (IsFull)
			{
				PrintFullRestore(g, fore, rect_icon);
			}
			else
			{
				PrintFull(g, fore, rect_icon);
			}
		}
		if (maximizeBox)
		{
			Rectangle rect_icon2 = new Rectangle(rect_max.X + num2, rect_max.Y + num3, num, num);
			if (hove_max.Animation)
			{
				g.Fill(Helper.ToColor(hove_max.Value, fillsecondary), rect_max);
			}
			else if (hove_max.Switch)
			{
				g.Fill(fillsecondary, rect_max);
			}
			if (hove_max.Down)
			{
				g.Fill(fillsecondary, rect_max);
			}
			if (IsMax)
			{
				PrintRestore(g, fore, rect_icon2);
			}
			else
			{
				PrintMax(g, fore, rect_icon2);
			}
		}
		if (minimizeBox)
		{
			Rectangle rect_icon3 = new Rectangle(rect_min.X + num2, rect_min.Y + num3, num, num);
			if (hove_min.Animation)
			{
				g.Fill(Helper.ToColor(hove_min.Value, fillsecondary), rect_min);
			}
			else if (hove_min.Switch)
			{
				g.Fill(fillsecondary, rect_min);
			}
			if (hove_min.Down)
			{
				g.Fill(fillsecondary, rect_min);
			}
			PrintMin(g, fore, rect_icon3);
		}
	}

	private void PrintBack(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_back == null || ((Image)temp_back).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_back;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_back = SvgExtend.GetImgExtend("ArrowLeftOutlined", rect_icon, color);
		}
		if (temp_back != null)
		{
			g.Image(temp_back, rect_icon);
		}
	}

	private void PrintBackHover(Canvas g, Color color, Rectangle rect_icon)
	{
		PrintBack(g, color, rect_icon);
		g.GetImgExtend("ArrowLeftOutlined", rect_icon, Helper.ToColor(hove_back.Value, Colour.Primary.Get("PageHeader")));
	}

	private void PrintBackHover(Canvas g, Rectangle rect_icon)
	{
		if (temp_back_hover == null || ((Image)temp_back_hover).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_back_hover;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_back_hover = SvgExtend.GetImgExtend("ArrowLeftOutlined", rect_icon, Colour.Primary.Get("PageHeader"));
		}
		if (temp_back_hover != null)
		{
			g.Image(temp_back_hover, rect_icon);
		}
	}

	private void PrintBackDown(Canvas g, Rectangle rect_icon)
	{
		if (temp_back_down == null || ((Image)temp_back_down).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_back_down;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_back_down = SvgExtend.GetImgExtend("ArrowLeftOutlined", rect_icon, Colour.PrimaryActive.Get("PageHeader"));
		}
		if (temp_back_down != null)
		{
			g.Image(temp_back_down, rect_icon);
		}
	}

	private void PrintClose(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_close == null || ((Image)temp_close).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_close;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_close = SvgExtend.GetImgExtend(SvgDb.IcoAppClose, rect_icon, color);
		}
		if (temp_close != null)
		{
			g.Image(temp_close, rect_icon);
		}
	}

	private void PrintCloseHover(Canvas g, Rectangle rect_icon)
	{
		if (temp_close_hover == null || ((Image)temp_close_hover).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_close_hover;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_close_hover = SvgExtend.GetImgExtend(SvgDb.IcoAppClose, rect_icon, Colour.ErrorColor.Get("PageHeader"));
		}
		if (temp_close_hover != null)
		{
			g.Image(temp_close_hover, rect_icon);
		}
	}

	private void PrintFull(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_full == null || ((Image)temp_full).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_full;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_full = SvgExtend.GetImgExtend(SvgDb.IcoAppFull, rect_icon, color);
		}
		if (temp_full != null)
		{
			g.Image(temp_full, rect_icon);
		}
	}

	private void PrintFullRestore(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_full_restore == null || ((Image)temp_full_restore).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_full_restore;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_full_restore = SvgExtend.GetImgExtend(SvgDb.IcoAppFullRestore, rect_icon, color);
		}
		if (temp_full_restore != null)
		{
			g.Image(temp_full_restore, rect_icon);
		}
	}

	private void PrintMax(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_max == null || ((Image)temp_max).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_max;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_max = SvgExtend.GetImgExtend(SvgDb.IcoAppMax, rect_icon, color);
		}
		if (temp_max != null)
		{
			g.Image(temp_max, rect_icon);
		}
	}

	private void PrintRestore(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_restore == null || ((Image)temp_restore).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_restore;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_restore = SvgExtend.GetImgExtend(SvgDb.IcoAppRestore, rect_icon, color);
		}
		if (temp_restore != null)
		{
			g.Image(temp_restore, rect_icon);
		}
	}

	private void PrintMin(Canvas g, Color color, Rectangle rect_icon)
	{
		if (temp_min == null || ((Image)temp_min).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_min;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_min = SvgExtend.GetImgExtend(SvgDb.IcoAppMin, rect_icon, color);
		}
		if (temp_min != null)
		{
			g.Image(temp_min, rect_icon);
		}
	}

	private bool PrintLogo(Canvas g, string svg, Color color, Rectangle rect_icon)
	{
		if (temp_logo == null || ((Image)temp_logo).Width != rect_icon.Width)
		{
			Bitmap? obj = temp_logo;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			temp_logo = SvgExtend.GetImgExtend(svg, rect_icon, color);
		}
		if (temp_logo != null)
		{
			g.Image(temp_logo, rect_icon);
			return true;
		}
		return false;
	}

	private void DisposeBmp()
	{
		Bitmap? obj = temp_logo;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		Bitmap? obj2 = temp_back;
		if (obj2 != null)
		{
			((Image)obj2).Dispose();
		}
		Bitmap? obj3 = temp_back_hover;
		if (obj3 != null)
		{
			((Image)obj3).Dispose();
		}
		Bitmap? obj4 = temp_back_down;
		if (obj4 != null)
		{
			((Image)obj4).Dispose();
		}
		Bitmap? obj5 = temp_full;
		if (obj5 != null)
		{
			((Image)obj5).Dispose();
		}
		Bitmap? obj6 = temp_full_restore;
		if (obj6 != null)
		{
			((Image)obj6).Dispose();
		}
		Bitmap? obj7 = temp_min;
		if (obj7 != null)
		{
			((Image)obj7).Dispose();
		}
		Bitmap? obj8 = temp_max;
		if (obj8 != null)
		{
			((Image)obj8).Dispose();
		}
		Bitmap? obj9 = temp_restore;
		if (obj9 != null)
		{
			((Image)obj9).Dispose();
		}
		Bitmap? obj10 = temp_close;
		if (obj10 != null)
		{
			((Image)obj10).Dispose();
		}
		temp_logo = null;
		temp_back = (temp_back_hover = (temp_back_down = null));
		temp_full = null;
		temp_full_restore = null;
		temp_min = null;
		temp_max = null;
		temp_restore = null;
		temp_close = null;
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Invalid comparison between Unknown and I4
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Invalid comparison between Unknown and I4
		Rectangle rectangle = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		if (CloseSize > 0 && showButton)
		{
			int num = ((fullBox || maximizeBox || minimizeBox) ? ((int)Math.Round((float)CloseSize * Config.Dpi)) : ((int)Math.Round((float)(CloseSize - 8) * Config.Dpi)));
			rect_close = new Rectangle(rectangle.Right - num, rectangle.Y, num, rectangle.Height);
			hasr = num;
			int num2 = rect_close.Left;
			if (fullBox)
			{
				rect_full = new Rectangle(num2 - num, rectangle.Y, num, rectangle.Height);
				num2 -= num;
				hasr += num;
			}
			if (maximizeBox)
			{
				rect_max = new Rectangle(num2 - num, rectangle.Y, num, rectangle.Height);
				num2 -= num;
				hasr += num;
			}
			if (minimizeBox)
			{
				rect_min = new Rectangle(num2 - num, rectangle.Y, num, rectangle.Height);
				hasr += num;
			}
		}
		else
		{
			hasr = 0;
		}
		if (DragMove)
		{
			Form val = ((Control)this).Parent.FindPARENT();
			if (val != null)
			{
				if (val is LayeredFormDrawer)
				{
					return;
				}
				if (val is BaseForm baseForm)
				{
					IsMax = baseForm.IsMax;
					IsFull = baseForm.IsFull;
				}
				else
				{
					IsMax = (int)val.WindowState == 2;
					if (IsMax)
					{
						IsFull = (int)val.FormBorderStyle == 0;
					}
					else
					{
						IsFull = false;
					}
				}
			}
		}
		((Control)this).OnSizeChanged(e);
	}

	public PageHeader()
	{
		hove_back = new ITaskOpacity((IControl)this);
		hove_close = new ITaskOpacity((IControl)this);
		hove_full = new ITaskOpacity((IControl)this);
		hove_max = new ITaskOpacity((IControl)this);
		hove_min = new ITaskOpacity((IControl)this);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (showButton)
		{
			bool flag = rect_close.Contains(e.Location);
			bool flag2 = rect_full.Contains(e.Location);
			bool flag3 = rect_max.Contains(e.Location);
			bool flag4 = rect_min.Contains(e.Location);
			if (flag != hove_close.Switch || flag2 != hove_full.Switch || flag3 != hove_max.Switch || flag4 != hove_min.Switch)
			{
				Color color = Colour.FillSecondary.Get("PageHeader", mode);
				ITaskOpacity taskOpacity = hove_max;
				ITaskOpacity taskOpacity2 = hove_min;
				int num = (hove_full.MaxValue = color.A);
				int maxValue = (taskOpacity2.MaxValue = num);
				taskOpacity.MaxValue = maxValue;
				hove_close.Switch = flag;
				hove_full.Switch = flag2;
				hove_max.Switch = flag3;
				hove_min.Switch = flag4;
			}
		}
		if (showback)
		{
			hove_back.Switch = rect_back.Contains(e.Location);
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		ITaskOpacity taskOpacity = hove_back;
		ITaskOpacity taskOpacity2 = hove_close;
		ITaskOpacity taskOpacity3 = hove_full;
		ITaskOpacity taskOpacity4 = hove_max;
		bool flag2 = (hove_min.Switch = false);
		bool flag4 = (taskOpacity4.Switch = flag2);
		bool flag6 = (taskOpacity3.Switch = flag4);
		bool @switch = (taskOpacity2.Switch = flag6);
		taskOpacity.Switch = @switch;
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			if (showButton)
			{
				hove_close.Down = rect_close.Contains(e.Location);
				hove_full.Down = rect_full.Contains(e.Location);
				hove_max.Down = rect_max.Contains(e.Location);
				hove_min.Down = rect_min.Contains(e.Location);
				if (hove_close.Down || hove_full.Down || hove_max.Down || hove_min.Down)
				{
					return;
				}
			}
			if (showback)
			{
				hove_back.Down = rect_back.Contains(e.Location);
				if (hove_back.Down)
				{
					return;
				}
			}
			if (DragMove)
			{
				Form val = ((Control)this).Parent.FindPARENT();
				if (val != null)
				{
					if (val is LayeredFormDrawer)
					{
						return;
					}
					if (e.Clicks > 1)
					{
						if (maximizeBox)
						{
							isfull = false;
							if (val is BaseForm baseForm)
							{
								IsMax = baseForm.MaxRestore();
							}
							else if ((int)val.WindowState == 2)
							{
								IsMax = false;
								val.WindowState = (FormWindowState)0;
							}
							else
							{
								IsMax = true;
								val.WindowState = (FormWindowState)2;
							}
							return;
						}
					}
					else if (val is BaseForm baseForm2)
					{
						baseForm2.DraggableMouseDown();
					}
					else
					{
						User32.ReleaseCapture();
						User32.SendMessage(((Control)val).Handle, 274, 61458, IntPtr.Zero);
					}
				}
			}
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Invalid comparison between Unknown and I4
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Invalid comparison between Unknown and I4
		if (showButton)
		{
			if (hove_close.Down && rect_close.Contains(e.Location))
			{
				Form? obj = ((Control)this).Parent.FindPARENT();
				if (obj != null)
				{
					obj.Close();
				}
			}
			else if (hove_full.Down && rect_full.Contains(e.Location))
			{
				Form val = ((Control)this).Parent.FindPARENT();
				if (val != null)
				{
					if (val is LayeredFormDrawer)
					{
						return;
					}
					if (val is BaseForm baseForm)
					{
						IsFull = baseForm.FullRestore();
					}
					else if ((int)val.WindowState == 2)
					{
						IsFull = false;
						val.FormBorderStyle = (FormBorderStyle)4;
						val.WindowState = (FormWindowState)0;
					}
					else
					{
						IsFull = true;
						val.FormBorderStyle = (FormBorderStyle)0;
						val.WindowState = (FormWindowState)2;
					}
				}
			}
			else if (hove_max.Down && rect_max.Contains(e.Location))
			{
				Form val2 = ((Control)this).Parent.FindPARENT();
				if (val2 != null)
				{
					if (val2 is LayeredFormDrawer)
					{
						return;
					}
					if (val2 is BaseForm baseForm2)
					{
						IsMax = baseForm2.MaxRestore();
					}
					else if ((int)val2.WindowState == 2)
					{
						IsMax = false;
						val2.WindowState = (FormWindowState)0;
					}
					else
					{
						IsMax = true;
						val2.WindowState = (FormWindowState)2;
					}
				}
			}
			else if (hove_min.Down && rect_min.Contains(e.Location))
			{
				Form val3 = ((Control)this).Parent.FindPARENT();
				if (val3 != null)
				{
					if (val3 is LayeredFormDrawer)
					{
						return;
					}
					if (val3 is BaseForm baseForm3)
					{
						baseForm3.Min();
					}
					else
					{
						val3.WindowState = (FormWindowState)1;
					}
				}
			}
		}
		if (showback && hove_back.Down && rect_back.Contains(e.Location))
		{
			this.BackClick?.Invoke(this, EventArgs.Empty);
		}
		ITaskOpacity taskOpacity = hove_back;
		ITaskOpacity taskOpacity2 = hove_close;
		ITaskOpacity taskOpacity3 = hove_full;
		ITaskOpacity taskOpacity4 = hove_max;
		bool flag2 = (hove_min.Down = false);
		bool flag4 = (taskOpacity4.Down = flag2);
		bool flag6 = (taskOpacity3.Down = flag4);
		bool down = (taskOpacity2.Down = flag6);
		taskOpacity.Down = down;
		((Control)this).OnMouseUp(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		if (cancelButton)
		{
			HandCancelButton(cancelButton);
		}
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		switch (id)
		{
		case EventType.THEME:
			DisposeBmp();
			((Control)this).Invalidate();
			break;
		case EventType.WINDOW_STATE:
			if (tag is bool flag)
			{
				IsMax = flag;
			}
			break;
		}
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if (CancelButton && (keyData & 0x60000) == 0 && (keyData & 0xFFFF) == 27)
		{
			if (showback && this.BackClick != null)
			{
				this.BackClick(this, EventArgs.Empty);
			}
			else
			{
				Form? obj = ((Control)this).Parent.FindPARENT();
				if (obj != null)
				{
					obj.Close();
				}
			}
			return true;
		}
		return ((Control)this).ProcessDialogKey(keyData);
	}
}
