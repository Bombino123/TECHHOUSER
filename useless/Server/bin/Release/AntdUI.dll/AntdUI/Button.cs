using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Button 按钮")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
public class Button : IControl, IButtonControl, IEventListener
{
	private Color? fore;

	private Color? back;

	private string? backExtend;

	private Image? backImage;

	private TFit backFit;

	private Color? defaultback;

	private Color? defaultbordercolor;

	private float borderWidth;

	private int radius = 6;

	private TShape shape;

	private TTypeMini type;

	private bool ghost;

	private bool showArrow;

	private bool isLink;

	private bool textLine;

	private string? text;

	private StringFormat stringFormat = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private ContentAlignment textAlign = (ContentAlignment)32;

	private bool textCenterHasIcon;

	private bool autoEllipsis;

	private bool textMultiLine;

	private float iconratio = 0.7f;

	private float icongap = 0.25f;

	private Image? icon;

	private string? iconSvg;

	private TAlignMini iconPosition = TAlignMini.Left;

	private bool AnimationIconToggle;

	private float AnimationIconToggleValue;

	private bool toggle;

	private Image? iconToggle;

	private string? iconSvgToggle;

	private Color? foreToggle;

	private TTypeMini? typeToggle;

	private Color? backToggle;

	private string? backExtendToggle;

	private bool loading;

	private int AnimationLoadingValue;

	private int AnimationLoadingWaveValue;

	private bool joinLeft;

	private bool joinRight;

	private ITask? ThreadHover;

	private ITask? ThreadIconHover;

	private ITask? ThreadIconToggle;

	private ITask? ThreadClick;

	private ITask? ThreadLoading;

	private bool AnimationClick;

	private float AnimationClickValue;

	private bool _mouseDown;

	private int AnimationHoverValue;

	private bool AnimationHover;

	private bool AnimationIconHover;

	private float AnimationIconHoverValue;

	private bool _mouseHover;

	private Color? colorBlink;

	private ITask? ThreadAnimateBlink;

	public bool AnimationBlinkState;

	private bool init;

	private TAutoSize autoSize;

	private bool hasFocus;

	[Description("原装背景颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "Transparent")]
	public Color OriginalBackColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			((Control)this).BackColor = value;
		}
	}

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
	public Color? BackColor
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
				OnPropertyChanged("BackColor");
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

	[Description("悬停背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackHover { get; set; }

	[Description("激活背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BackActive { get; set; }

	[Description("背景图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? BackgroundImage
	{
		get
		{
			return backImage;
		}
		set
		{
			if (backImage != value)
			{
				backImage = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackgroundImage");
			}
		}
	}

	[Description("背景图片布局")]
	[Category("外观")]
	[DefaultValue(TFit.Fill)]
	public TFit BackgroundImageLayout
	{
		get
		{
			return backFit;
		}
		set
		{
			if (backFit != value)
			{
				backFit = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BackgroundImageLayout");
			}
		}
	}

	[Description("Default模式背景颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? DefaultBack
	{
		get
		{
			return defaultback;
		}
		set
		{
			if (!(defaultback == value))
			{
				defaultback = value;
				if (type == TTypeMini.Default)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("DefaultBack");
			}
		}
	}

	[Description("Default模式边框颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? DefaultBorderColor
	{
		get
		{
			return defaultbordercolor;
		}
		set
		{
			if (!(defaultbordercolor == value))
			{
				defaultbordercolor = value;
				if (type == TTypeMini.Default)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("DefaultBorderColor");
			}
		}
	}

	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(0f)]
	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	[Description("波浪大小")]
	[Category("外观")]
	[DefaultValue(4)]
	public int WaveSize { get; set; } = 4;


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
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("形状")]
	[Category("外观")]
	[DefaultValue(TShape.Default)]
	public TShape Shape
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
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Shape");
			}
		}
	}

	[Description("类型")]
	[Category("外观")]
	[DefaultValue(TTypeMini.Default)]
	public TTypeMini Type
	{
		get
		{
			return type;
		}
		set
		{
			if (type != value)
			{
				type = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Type");
			}
		}
	}

	[Description("幽灵属性，使按钮背景透明")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Ghost
	{
		get
		{
			return ghost;
		}
		set
		{
			if (ghost != value)
			{
				ghost = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Ghost");
			}
		}
	}

	[Description("响应真实区域")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool RespondRealAreas { get; set; }

	[Description("显示箭头")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ShowArrow
	{
		get
		{
			return showArrow;
		}
		set
		{
			if (showArrow != value)
			{
				showArrow = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ShowArrow");
			}
		}
	}

	[Description("箭头链接样式")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool IsLink
	{
		get
		{
			return isLink;
		}
		set
		{
			if (isLink != value)
			{
				isLink = value;
				((Control)this).Invalidate();
				OnPropertyChanged("IsLink");
			}
		}
	}

	[Browsable(false)]
	[Description("箭头角度")]
	[Category("外观")]
	[DefaultValue(-1f)]
	public float ArrowProg { get; set; } = -1f;


	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
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
			if (string.IsNullOrEmpty(value))
			{
				value = null;
			}
			if (!(text == value))
			{
				text = value;
				if (text == null)
				{
					textLine = false;
				}
				else
				{
					textLine = text.Contains(Environment.NewLine);
				}
				if (BeforeAutoSize())
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

	[Description("文本位置")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public ContentAlignment TextAlign
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return textAlign;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			if (textAlign == value)
			{
				return;
			}
			if ((loading && LoadingValue > -1f) || HasIcon || showArrow)
			{
				value = (ContentAlignment)32;
				if (textAlign == value)
				{
					return;
				}
			}
			textAlign = value;
			textAlign.SetAlignment(ref stringFormat);
			((Control)this).Invalidate();
			OnPropertyChanged("TextAlign");
		}
	}

	[Description("文本居中显示(包含图标后)")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool TextCenterHasIcon
	{
		get
		{
			return textCenterHasIcon;
		}
		set
		{
			if (textCenterHasIcon != value)
			{
				textCenterHasIcon = value;
				if (HasIcon)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("TextCenterHasIcon");
			}
		}
	}

	[Description("文本超出自动处理")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AutoEllipsis
	{
		get
		{
			return autoEllipsis;
		}
		set
		{
			if (autoEllipsis != value)
			{
				autoEllipsis = value;
				stringFormat.Trimming = (StringTrimming)(value ? 3 : 0);
				OnPropertyChanged("AutoEllipsis");
			}
		}
	}

	[Description("是否多行")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool TextMultiLine
	{
		get
		{
			return textMultiLine;
		}
		set
		{
			if (textMultiLine != value)
			{
				textMultiLine = value;
				stringFormat.FormatFlags = (StringFormatFlags)((!value) ? 4096 : 0);
				((Control)this).Invalidate();
				OnPropertyChanged("TextMultiLine");
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
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("IconRatio");
			}
		}
	}

	[Description("图标与文字间距比例")]
	[Category("外观")]
	[DefaultValue(0.25f)]
	public float IconGap
	{
		get
		{
			return icongap;
		}
		set
		{
			if (icongap != value)
			{
				icongap = value;
				((Control)this).Invalidate();
				OnPropertyChanged("IconGap");
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
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
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
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("IconSvg");
			}
		}
	}

	public bool HasIcon
	{
		get
		{
			if (iconSvg == null)
			{
				return icon != null;
			}
			return true;
		}
	}

	[Description("图标大小")]
	[Category("外观")]
	[DefaultValue(typeof(Size), "0, 0")]
	public Size IconSize { get; set; } = new Size(0, 0);


	[Description("悬停图标")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? IconHover { get; set; }

	[Description("悬停图标SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? IconHoverSvg { get; set; }

	[Description("悬停图标动画时长")]
	[Category("外观")]
	[DefaultValue(200)]
	public int IconHoverAnimation { get; set; } = 200;


	[Description("按钮图标组件的位置")]
	[Category("外观")]
	[DefaultValue(TAlignMini.Left)]
	public TAlignMini IconPosition
	{
		get
		{
			return iconPosition;
		}
		set
		{
			if (iconPosition != value)
			{
				iconPosition = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("IconPosition");
			}
		}
	}

	[Description("选中状态")]
	[Category("切换")]
	[DefaultValue(false)]
	public bool Toggle
	{
		get
		{
			return toggle;
		}
		set
		{
			if (value == toggle)
			{
				return;
			}
			toggle = value;
			if (Config.Animation)
			{
				if (IconToggleAnimation > 0 && HasIcon && HasToggleIcon)
				{
					ThreadIconHover?.Dispose();
					ThreadIconHover = null;
					AnimationIconHover = false;
					ThreadIconToggle?.Dispose();
					AnimationIconToggle = true;
					int t = Animation.TotalFrames(10, IconToggleAnimation);
					if (value)
					{
						ThreadIconToggle = new ITask(delegate(int i)
						{
							AnimationIconToggleValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)this).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationIconToggleValue = 1f;
							AnimationIconToggle = false;
							((Control)this).Invalidate();
						});
					}
					else
					{
						ThreadIconToggle = new ITask(delegate(int i)
						{
							AnimationIconToggleValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)this).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationIconToggleValue = 0f;
							AnimationIconToggle = false;
							((Control)this).Invalidate();
						});
					}
				}
				else
				{
					((Control)this).Invalidate();
				}
			}
			else
			{
				((Control)this).Invalidate();
			}
			OnPropertyChanged("Toggle");
		}
	}

	[Description("切换图标")]
	[Category("切换")]
	[DefaultValue(null)]
	public Image? ToggleIcon
	{
		get
		{
			return iconToggle;
		}
		set
		{
			if (iconToggle != value)
			{
				iconToggle = value;
				if (toggle)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ToggleIcon");
			}
		}
	}

	[Description("切换图标SVG")]
	[Category("切换")]
	[DefaultValue(null)]
	public string? ToggleIconSvg
	{
		get
		{
			return iconSvgToggle;
		}
		set
		{
			if (!(iconSvgToggle == value))
			{
				iconSvgToggle = value;
				if (toggle)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ToggleIconSvg");
			}
		}
	}

	public bool HasToggleIcon
	{
		get
		{
			if (iconSvgToggle == null)
			{
				return iconToggle != null;
			}
			return true;
		}
	}

	[Description("切换悬停图标")]
	[Category("切换")]
	[DefaultValue(null)]
	public Image? ToggleIconHover { get; set; }

	[Description("切换悬停图标SVG")]
	[Category("切换")]
	[DefaultValue(null)]
	public string? ToggleIconHoverSvg { get; set; }

	[Description("图标切换动画时长")]
	[Category("切换")]
	[DefaultValue(200)]
	public int IconToggleAnimation { get; set; } = 200;


	[Description("切换文字颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleFore
	{
		get
		{
			return foreToggle;
		}
		set
		{
			if (foreToggle == value)
			{
				foreToggle = value;
			}
			foreToggle = value;
			if (toggle)
			{
				((Control)this).Invalidate();
			}
			OnPropertyChanged("ToggleFore");
		}
	}

	[Description("切换类型")]
	[Category("切换")]
	[DefaultValue(null)]
	public TTypeMini? ToggleType
	{
		get
		{
			return typeToggle;
		}
		set
		{
			if (typeToggle != value)
			{
				typeToggle = value;
				if (toggle)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ToggleType");
			}
		}
	}

	[Description("切换背景颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleBack
	{
		get
		{
			return backToggle;
		}
		set
		{
			if (!(backToggle == value))
			{
				backToggle = value;
				if (toggle)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ToggleBack");
			}
		}
	}

	[Description("切换背景渐变色")]
	[Category("切换")]
	[DefaultValue(null)]
	public string? ToggleBackExtend
	{
		get
		{
			return backExtendToggle;
		}
		set
		{
			if (!(backExtendToggle == value))
			{
				backExtendToggle = value;
				if (toggle)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("ToggleBackExtend");
			}
		}
	}

	[Description("切换悬停背景颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleBackHover { get; set; }

	[Description("切换激活背景颜色")]
	[Category("切换")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ToggleBackActive { get; set; }

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
			SetCursor(_mouseHover && base.Enabled && !value);
			BeforeAutoSize();
			ThreadLoading?.Dispose();
			if (loading)
			{
				AnimationClickValue = 0f;
				ThreadLoading = new ITask((Control)(object)this, delegate(int i)
				{
					AnimationLoadingWaveValue++;
					if (AnimationLoadingWaveValue > 100)
					{
						AnimationLoadingWaveValue = 0;
					}
					AnimationLoadingValue = i;
					((Control)this).Invalidate();
					return loading;
				}, 10, 360, 6, delegate
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

	[Description("加载响应点击")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool LoadingRespondClick { get; set; }

	[Description("加载进度")]
	[Category("加载")]
	[DefaultValue(0.3f)]
	public float LoadingValue { get; set; } = 0.3f;


	[Description("水波进度")]
	[Category("加载")]
	[DefaultValue(0f)]
	public float LoadingWaveValue { get; set; }

	[Description("水波颜色")]
	[Category("加载")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? LoadingWaveColor { get; set; }

	[Description("水波是否垂直")]
	[Category("加载")]
	[DefaultValue(false)]
	public bool LoadingWaveVertical { get; set; }

	[Description("水波大小")]
	[Category("加载")]
	[DefaultValue(2)]
	public int LoadingWaveSize { get; set; } = 2;


	[Description("水波数量")]
	[Category("加载")]
	[DefaultValue(1)]
	public int LoadingWaveCount { get; set; } = 1;


	[Description("连接左边")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool JoinLeft
	{
		get
		{
			return joinLeft;
		}
		set
		{
			if (joinLeft != value)
			{
				joinLeft = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("JoinLeft");
			}
		}
	}

	[Description("连接右边")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool JoinRight
	{
		get
		{
			return joinRight;
		}
		set
		{
			if (joinRight != value)
			{
				joinRight = value;
				if (BeforeAutoSize())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("JoinRight");
			}
		}
	}

	private bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown != value)
			{
				_mouseDown = value;
				((Control)this).Invalidate();
			}
		}
	}

	private bool ExtraMouseHover
	{
		get
		{
			return _mouseHover;
		}
		set
		{
			if (_mouseHover == value)
			{
				return;
			}
			_mouseHover = value;
			bool enabled = base.Enabled;
			SetCursor(value && enabled && !loading);
			if (!enabled)
			{
				return;
			}
			int alpha = GetColorO().A;
			if (Config.Animation)
			{
				if (IconHoverAnimation > 0 && ((toggle && HasToggleIcon && (ToggleIconHoverSvg != null || ToggleIconHover != null)) || (HasIcon && (IconHoverSvg != null || IconHover != null))))
				{
					ThreadIconHover?.Dispose();
					AnimationIconHover = true;
					int t = Animation.TotalFrames(10, IconHoverAnimation);
					if (value)
					{
						ThreadIconHover = new ITask(delegate(int i)
						{
							AnimationIconHoverValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)this).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationIconHoverValue = 1f;
							AnimationIconHover = false;
							((Control)this).Invalidate();
						});
					}
					else
					{
						ThreadIconHover = new ITask(delegate(int i)
						{
							AnimationIconHoverValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)this).Invalidate();
							return true;
						}, 10, t, delegate
						{
							AnimationIconHoverValue = 0f;
							AnimationIconHover = false;
							((Control)this).Invalidate();
						});
					}
				}
				if (alpha > 0)
				{
					int addvalue = alpha / 12;
					ThreadHover?.Dispose();
					AnimationHover = true;
					if (value)
					{
						ThreadHover = new ITask((Control)(object)this, delegate
						{
							AnimationHoverValue += addvalue;
							if (AnimationHoverValue > alpha)
							{
								AnimationHoverValue = alpha;
								return false;
							}
							((Control)this).Invalidate();
							return true;
						}, 10, delegate
						{
							AnimationHover = false;
							((Control)this).Invalidate();
						});
					}
					else
					{
						ThreadHover = new ITask((Control)(object)this, delegate
						{
							if (AnimationHoverValue > alpha)
							{
								AnimationHoverValue = alpha;
							}
							else
							{
								AnimationHoverValue -= addvalue;
							}
							if (AnimationHoverValue < 1)
							{
								AnimationHoverValue = 0;
								return false;
							}
							((Control)this).Invalidate();
							return true;
						}, 10, delegate
						{
							AnimationHover = false;
							((Control)this).Invalidate();
						});
					}
				}
				else
				{
					AnimationHoverValue = alpha;
					((Control)this).Invalidate();
				}
			}
			else
			{
				AnimationHoverValue = alpha;
			}
			((Control)this).Invalidate();
		}
	}

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Config.Dpi, shape, joinLeft, joinRight);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			return Path(readRectangle, (shape == TShape.Round || shape == TShape.Circle) ? ((float)readRectangle.Height) : ((float)radius * Config.Dpi));
		}
	}

	[Browsable(true)]
	[Description("自动大小")]
	[Category("外观")]
	[DefaultValue(false)]
	public override bool AutoSize
	{
		get
		{
			return ((Control)this).AutoSize;
		}
		set
		{
			if (((Control)this).AutoSize == value)
			{
				return;
			}
			((Control)this).AutoSize = value;
			if (value)
			{
				if (autoSize == TAutoSize.None)
				{
					autoSize = TAutoSize.Auto;
				}
			}
			else
			{
				autoSize = TAutoSize.None;
			}
			BeforeAutoSize();
		}
	}

	[Description("自动大小模式")]
	[Category("外观")]
	[DefaultValue(TAutoSize.None)]
	public TAutoSize AutoSizeMode
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
				((Control)this).AutoSize = autoSize != TAutoSize.None;
				BeforeAutoSize();
			}
		}
	}

	private Size PSize => Helper.GDI(delegate(Canvas g)
	{
		Size size = g.MeasureString(((Control)this).Text ?? "龍Qq", ((Control)this).Font);
		int num = (int)(20f * Config.Dpi);
		int num2 = (int)((float)WaveSize * Config.Dpi);
		if (Shape == TShape.Circle || string.IsNullOrEmpty(((Control)this).Text))
		{
			int num3 = size.Height + num2 + num;
			return new Size(num3, num3);
		}
		int num4 = num2 * 2;
		if (joinLeft || joinRight)
		{
			num4 = 0;
		}
		bool flag = (loading && LoadingValue > -1f) || HasIcon;
		if (flag || showArrow)
		{
			if (flag && (IconPosition == TAlignMini.Top || IconPosition == TAlignMini.Bottom))
			{
				int num5 = (int)Math.Ceiling((float)size.Height * 1.2f);
				return new Size(size.Width + num4 + num + num5, size.Height + num2 + num + num5);
			}
			int height = size.Height + num2 + num;
			if (flag && showArrow)
			{
				return new Size(size.Width + num4 + num + size.Height * 2, height);
			}
			if (flag)
			{
				return new Size(size.Width + num4 + num + (int)Math.Ceiling((float)size.Height * 1.2f), height);
			}
			return new Size(size.Width + num4 + num + (int)Math.Ceiling((float)size.Height * 0.8f), height);
		}
		return new Size(size.Width + num4 + num, size.Height + num2 + num);
	});

	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public DialogResult DialogResult { get; set; }

	[Browsable(false)]
	[Description("是否存在焦点")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool HasFocus
	{
		get
		{
			return hasFocus;
		}
		private set
		{
			if (value && (_mouseDown || _mouseHover))
			{
				value = false;
			}
			if (hasFocus != value)
			{
				hasFocus = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public event EventHandler? DoubleClick
	{
		add
		{
			((Control)this).DoubleClick += value;
		}
		remove
		{
			((Control)this).DoubleClick -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public event MouseEventHandler MouseDoubleClick
	{
		add
		{
			((Control)this).MouseDoubleClick += value;
		}
		remove
		{
			((Control)this).MouseDoubleClick -= value;
		}
	}

	public Button()
		: base(ControlType.Button)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).BackColor = Color.Transparent;
	}

	public void ClickAnimation()
	{
		if (WaveSize <= 0 || !Config.Animation)
		{
			return;
		}
		ThreadClick?.Dispose();
		AnimationClickValue = 0f;
		AnimationClick = true;
		ThreadClick = new ITask((Control)(object)this, delegate
		{
			if ((double)AnimationClickValue > 0.6)
			{
				AnimationClickValue = AnimationClickValue.Calculate(0.04f);
			}
			else
			{
				AnimationClickValue += (AnimationClickValue = AnimationClickValue.Calculate(0.1f));
			}
			if (AnimationClickValue > 1f)
			{
				AnimationClickValue = 0f;
				return false;
			}
			((Control)this).Invalidate();
			return true;
		}, 50, delegate
		{
			AnimationClick = false;
			((Control)this).Invalidate();
		});
	}

	public void AnimationBlink(int interval, params Color[] colors)
	{
		Color[] colors2 = colors;
		ThreadAnimateBlink?.Dispose();
		if (colors2.Length <= 1)
		{
			return;
		}
		AnimationBlinkState = true;
		if (!AnimationBlinkState)
		{
			return;
		}
		int index = 0;
		int len = colors2.Length;
		ThreadAnimateBlink = new ITask((Control)(object)this, delegate
		{
			colorBlink = colors2[index];
			index++;
			if (index > len - 1)
			{
				index = 0;
			}
			((Control)this).Invalidate();
			return AnimationBlinkState;
		}, interval, delegate
		{
			((Control)this).Invalidate();
		});
	}

	public void StopAnimationBlink()
	{
		AnimationBlinkState = false;
		ThreadAnimateBlink?.Dispose();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		init = true;
		Canvas canvas = e.Graphics.High();
		Rectangle rectangle = ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding);
		Rectangle readRectangle = ReadRectangle;
		float num = ((shape == TShape.Round || shape == TShape.Circle) ? ((float)readRectangle.Height) : ((float)radius * Config.Dpi));
		if (backImage != null)
		{
			canvas.Image(readRectangle, backImage, backFit, num, shape);
		}
		bool flag = type == TTypeMini.Default;
		bool enabled = base.Enabled;
		if (toggle && typeToggle.HasValue)
		{
			flag = typeToggle.Value == TTypeMini.Default;
		}
		if (flag)
		{
			GetDefaultColorConfig(out var Fore, out var Color, out var backHover, out var backActive);
			GraphicsPath val = Path(readRectangle, num);
			try
			{
				if (AnimationClick)
				{
					float num2 = (float)readRectangle.Width + (float)(rectangle.Width - readRectangle.Width) * AnimationClickValue;
					float num3 = (float)readRectangle.Height + (float)(rectangle.Height - readRectangle.Height) * AnimationClickValue;
					if (shape == TShape.Circle)
					{
						if (num2 > num3)
						{
							num2 = num3;
						}
						else
						{
							num3 = num2;
						}
					}
					float alpha = 100f * (1f - AnimationClickValue);
					GraphicsPath val2 = new RectangleF((float)rectangle.X + ((float)rectangle.Width - num2) / 2f, (float)rectangle.Y + ((float)rectangle.Height - num3) / 2f, num2, num3).RoundPath(num, shape);
					try
					{
						val2.AddPath(val, false);
						canvas.Fill(Helper.ToColor(alpha, Color), val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				if (enabled)
				{
					if (!ghost)
					{
						if (WaveSize > 0)
						{
							PaintShadow(canvas, readRectangle, val, Colour.FillQuaternary.Get("Button"), num);
						}
						canvas.Fill(defaultback ?? Colour.DefaultBg.Get("Button"), val);
					}
					if (borderWidth > 0f)
					{
						PaintLoadingWave(canvas, val, readRectangle);
						float width = borderWidth * Config.Dpi;
						if (ExtraMouseDown)
						{
							canvas.Draw(backActive, width, val);
							PaintTextLoading(canvas, ((Control)this).Text, backActive, readRectangle, enabled, num);
						}
						else if (AnimationHover)
						{
							Color overlay = Helper.ToColor(AnimationHoverValue, backHover);
							canvas.Draw(Colour.DefaultBorder.Get("Button").BlendColors(overlay), width, val);
							PaintTextLoading(canvas, ((Control)this).Text, Fore.BlendColors(overlay), readRectangle, enabled, num);
						}
						else if (ExtraMouseHover)
						{
							canvas.Draw(backHover, width, val);
							PaintTextLoading(canvas, ((Control)this).Text, backHover, readRectangle, enabled, num);
						}
						else
						{
							if (AnimationBlinkState && colorBlink.HasValue)
							{
								canvas.Draw(colorBlink.Value, width, val);
							}
							else
							{
								canvas.Draw(defaultbordercolor ?? Colour.DefaultBorder.Get("Button"), width, val);
							}
							PaintTextLoading(canvas, ((Control)this).Text, Fore, readRectangle, enabled, num);
						}
					}
					else
					{
						if (ExtraMouseDown)
						{
							canvas.Fill(backActive, val);
						}
						else if (AnimationHover)
						{
							canvas.Fill(Helper.ToColor(AnimationHoverValue, backHover), val);
						}
						else if (ExtraMouseHover)
						{
							canvas.Fill(backHover, val);
						}
						PaintLoadingWave(canvas, val, readRectangle);
						PaintTextLoading(canvas, ((Control)this).Text, Fore, readRectangle, enabled, num);
					}
				}
				else
				{
					PaintLoadingWave(canvas, val, readRectangle);
					if (!ghost)
					{
						canvas.Fill(Colour.FillTertiary.Get("Button"), val);
					}
					PaintTextLoading(canvas, ((Control)this).Text, Colour.TextQuaternary.Get("Button"), readRectangle, enabled, num);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			GetColorConfig(out var Fore2, out var Back, out var backHover2, out var backActive2);
			GraphicsPath val3 = Path(readRectangle, num);
			try
			{
				if (AnimationClick)
				{
					float num4 = (float)readRectangle.Width + (float)(rectangle.Width - readRectangle.Width) * AnimationClickValue;
					float num5 = (float)readRectangle.Height + (float)(rectangle.Height - readRectangle.Height) * AnimationClickValue;
					if (shape == TShape.Circle)
					{
						if (num4 > num5)
						{
							num4 = num5;
						}
						else
						{
							num5 = num4;
						}
					}
					float alpha2 = 100f * (1f - AnimationClickValue);
					GraphicsPath val4 = new RectangleF((float)rectangle.X + ((float)rectangle.Width - num4) / 2f, (float)rectangle.Y + ((float)rectangle.Height - num5) / 2f, num4, num5).RoundPath(num, shape);
					try
					{
						val4.AddPath(val3, false);
						canvas.Fill(Helper.ToColor(alpha2, Back), val4);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				if (ghost)
				{
					PaintLoadingWave(canvas, val3, readRectangle);
					if (borderWidth > 0f)
					{
						float width2 = borderWidth * Config.Dpi;
						if (ExtraMouseDown)
						{
							canvas.Draw(backActive2, width2, val3);
							PaintTextLoading(canvas, ((Control)this).Text, backActive2, readRectangle, enabled, num);
						}
						else if (AnimationHover)
						{
							Color overlay2 = Helper.ToColor(AnimationHoverValue, backHover2);
							canvas.Draw((enabled ? Back : Colour.FillTertiary.Get("Button")).BlendColors(overlay2), width2, val3);
							PaintTextLoading(canvas, ((Control)this).Text, Back.BlendColors(overlay2), readRectangle, enabled, num);
						}
						else if (ExtraMouseHover)
						{
							canvas.Draw(backHover2, width2, val3);
							PaintTextLoading(canvas, ((Control)this).Text, backHover2, readRectangle, enabled, num);
						}
						else
						{
							if (enabled)
							{
								if (toggle)
								{
									Brush val5 = backExtendToggle.BrushEx(readRectangle, Back);
									try
									{
										canvas.Draw(val5, width2, val3);
									}
									finally
									{
										((IDisposable)val5)?.Dispose();
									}
								}
								else
								{
									Brush val6 = backExtend.BrushEx(readRectangle, Back);
									try
									{
										canvas.Draw(val6, width2, val3);
									}
									finally
									{
										((IDisposable)val6)?.Dispose();
									}
								}
							}
							else
							{
								canvas.Draw(Colour.FillTertiary.Get("Button"), width2, val3);
							}
							PaintTextLoading(canvas, ((Control)this).Text, enabled ? Back : Colour.TextQuaternary.Get("Button"), readRectangle, enabled, num);
						}
					}
					else
					{
						PaintTextLoading(canvas, ((Control)this).Text, enabled ? Back : Colour.TextQuaternary.Get("Button"), readRectangle, enabled, num);
					}
				}
				else
				{
					if (enabled && WaveSize > 0)
					{
						PaintShadow(canvas, readRectangle, val3, Back.rgba((Config.Mode == TMode.Dark) ? 0.15f : 0.1f), num);
					}
					if (enabled)
					{
						if (toggle)
						{
							Brush val7 = backExtendToggle.BrushEx(readRectangle, Back);
							try
							{
								canvas.Fill(val7, val3);
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
							}
						}
						else
						{
							Brush val8 = backExtend.BrushEx(readRectangle, Back);
							try
							{
								canvas.Fill(val8, val3);
							}
							finally
							{
								((IDisposable)val8)?.Dispose();
							}
						}
					}
					else
					{
						canvas.Fill(Colour.FillTertiary.Get("Button"), val3);
					}
					if (ExtraMouseDown)
					{
						canvas.Fill(backActive2, val3);
					}
					else if (AnimationHover)
					{
						canvas.Fill(Helper.ToColor(AnimationHoverValue, backHover2), val3);
					}
					else if (ExtraMouseHover)
					{
						canvas.Fill(backHover2, val3);
					}
					PaintLoadingWave(canvas, val3, readRectangle);
					PaintTextLoading(canvas, ((Control)this).Text, enabled ? Fore2 : Colour.TextQuaternary.Get("Button"), readRectangle, enabled, num);
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintShadow(Canvas g, Rectangle rect, GraphicsPath path, Color color, float radius)
	{
		float num = (float)WaveSize * Config.Dpi / 2f;
		GraphicsPath val = new RectangleF(rect.X, (float)rect.Y + num, rect.Width, rect.Height).RoundPath(radius);
		try
		{
			val.AddPath(path, false);
			g.Fill(color, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintLoadingWave(Canvas g, GraphicsPath path, Rectangle rect)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		if (!loading || !(LoadingWaveValue > 0f))
		{
			return;
		}
		SolidBrush val = new SolidBrush(LoadingWaveColor ?? Colour.Fill.Get("Button"));
		try
		{
			if (LoadingWaveValue >= 1f)
			{
				g.Fill((Brush)(object)val, path);
			}
			else if (LoadingWaveCount > 0)
			{
				GraphicsState state = g.Save();
				g.SetClip(path);
				g.ResetTransform();
				int num = (int)((float)LoadingWaveSize * Config.Dpi);
				int num2 = LoadingWaveCount * 2 + 2;
				if (num2 < 6)
				{
					num2 = 6;
				}
				if (LoadingWaveVertical)
				{
					int num3 = (int)((float)rect.Height * LoadingWaveValue);
					if (num3 > 0)
					{
						num3 = rect.Height - num3 + rect.Y;
						int num4 = rect.Width / LoadingWaveCount;
						int num5 = num4 * 2;
						int num6 = num3 - num;
						int num7 = rect.X + num4 * num2;
						GraphicsPath val2 = new GraphicsPath();
						try
						{
							g.TranslateTransform(0f - ((float)num4 + (float)num5 * ((float)AnimationLoadingWaveValue / 100f)), 0f);
							val2.AddLine(num7, num3, num7, rect.Bottom);
							val2.AddLine(num7, rect.Bottom, rect.X, rect.Bottom);
							val2.AddLine(rect.X, rect.Bottom, rect.X, num3);
							bool flag = true;
							List<PointF> list = new List<PointF>(num2);
							for (int i = 0; i < num2 + 1; i++)
							{
								list.Add(new PointF(rect.X + num4 * i, flag ? num3 : num6));
								flag = !flag;
							}
							val2.AddCurve(list.ToArray());
							g.Fill((Brush)(object)val, val2);
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
				}
				else
				{
					int num8 = (int)((float)rect.Width * LoadingWaveValue);
					if (num8 > 0)
					{
						num8 += rect.X;
						int num9 = rect.Height / LoadingWaveCount;
						int num10 = num9 * 2;
						int num11 = num8 + num;
						int num12 = rect.Y + num9 * num2;
						GraphicsPath val3 = new GraphicsPath();
						try
						{
							g.TranslateTransform(0f, 0f - ((float)num9 + (float)num10 * ((float)AnimationLoadingWaveValue / 100f)));
							val3.AddLine(num8, num12, rect.X, num12);
							val3.AddLine(rect.X, num12, rect.X, rect.Y);
							val3.AddLine(rect.X, rect.Y, num8, rect.Y);
							bool flag2 = true;
							List<PointF> list2 = new List<PointF>(num2);
							for (int j = 0; j < num2 + 1; j++)
							{
								list2.Add(new PointF(flag2 ? num8 : num11, rect.Y + num9 * j));
								flag2 = !flag2;
							}
							val3.AddCurve(list2.ToArray());
							g.Fill((Brush)(object)val, val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
				}
				g.Restore(state);
			}
			else if (LoadingWaveVertical)
			{
				int num13 = (int)((float)rect.Height * LoadingWaveValue);
				if (num13 > 0)
				{
					GraphicsState state2 = g.Save();
					g.SetClip(new Rectangle(rect.X, rect.Y + rect.Height - num13, rect.Width, num13));
					g.Fill((Brush)(object)val, path);
					g.Restore(state2);
				}
			}
			else
			{
				int num14 = (int)((float)rect.Width * LoadingWaveValue);
				if (num14 > 0)
				{
					GraphicsState state3 = g.Save();
					g.SetClip(new Rectangle(rect.X, rect.Y, num14, rect.Height));
					g.Fill((Brush)(object)val, path);
					g.Restore(state3);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintTextLoading(Canvas g, string? text, Color color, Rectangle rect_read, bool enabled, float radius)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		if (enabled && hasFocus && WaveSize > 0)
		{
			float num = (float)WaveSize * Config.Dpi / 2f;
			float num2 = num * 2f;
			GraphicsPath val = new RectangleF((float)rect_read.X - num, (float)rect_read.Y - num, (float)rect_read.Width + num2, (float)rect_read.Height + num2).RoundPath(radius + num);
			try
			{
				g.Draw(Colour.PrimaryBorder.Get("Button"), num, val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		bool flag = loading && LoadingValue > -1f;
		Size font_size = g.MeasureString(text ?? "龍Qq", ((Control)this).Font);
		if (text == null)
		{
			Rectangle iconRectCenter = GetIconRectCenter(font_size, rect_read);
			if (flag)
			{
				float num3 = (float)rect_read.Height * 0.06f;
				Pen val2 = new Pen(color, num3);
				try
				{
					LineCap startCap = (LineCap)2;
					val2.EndCap = (LineCap)2;
					val2.StartCap = startCap;
					g.DrawArc(val2, iconRectCenter, AnimationLoadingValue, LoadingValue * 360f);
					return;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			if (PaintIcon(g, color, iconRectCenter, hastxt: false, enabled) && showArrow)
			{
				int num4 = (int)((float)font_size.Height * IconRatio);
				Rectangle rect = new Rectangle(rect_read.X + (rect_read.Width - num4) / 2, rect_read.Y + (rect_read.Height - num4) / 2, num4, num4);
				PaintTextArrow(g, rect, color);
			}
			return;
		}
		bool flag2 = flag || HasIcon;
		bool flag3 = showArrow;
		Rectangle rect_text;
		if (flag2 || flag3)
		{
			if (flag2 && flag3)
			{
				rect_text = RectAlignLR(g, textLine, ((Control)this).Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_l, out var rect_r);
				if (flag)
				{
					float num5 = (float)rect_l.Height * 0.14f;
					Pen val3 = new Pen(color, num5);
					try
					{
						LineCap startCap = (LineCap)2;
						val3.EndCap = (LineCap)2;
						val3.StartCap = startCap;
						g.DrawArc(val3, rect_l, AnimationLoadingValue, LoadingValue * 360f);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				else
				{
					PaintIcon(g, color, rect_l, hastxt: true, enabled);
				}
				PaintTextArrow(g, rect_r, color);
			}
			else if (flag2)
			{
				rect_text = RectAlignL(g, textLine, textCenterHasIcon, ((Control)this).Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_l2);
				if (flag)
				{
					float num6 = (float)rect_l2.Height * 0.14f;
					Pen val4 = new Pen(color, num6);
					try
					{
						LineCap startCap = (LineCap)2;
						val4.EndCap = (LineCap)2;
						val4.StartCap = startCap;
						g.DrawArc(val4, rect_l2, AnimationLoadingValue, LoadingValue * 360f);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				else
				{
					PaintIcon(g, color, rect_l2, hastxt: true, enabled);
				}
			}
			else
			{
				rect_text = RectAlignR(g, textLine, ((Control)this).Font, iconPosition, iconratio, icongap, font_size, rect_read, out var rect_r2);
				PaintTextArrow(g, rect_r2, color);
			}
		}
		else
		{
			int num7 = (int)((float)font_size.Height * 0.4f);
			int num8 = num7 * 2;
			rect_text = new Rectangle(rect_read.X + num7, rect_read.Y + num7, rect_read.Width - num8, rect_read.Height - num8);
			PaintTextAlign(rect_read, ref rect_text);
		}
		g.String(text, ((Control)this).Font, color, rect_text, stringFormat);
	}

	private void PaintTextArrow(Canvas g, Rectangle rect, Color color)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Pen val = new Pen(color, 2f * Config.Dpi);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			if (isLink)
			{
				float num = (float)rect.Width / 2f;
				g.TranslateTransform((float)rect.X + num, (float)rect.Y + num);
				g.RotateTransform(-90f);
				g.DrawLines(val, new RectangleF(0f - num, 0f - num, rect.Width, rect.Height).TriangleLines(ArrowProg));
				g.ResetTransform();
			}
			else
			{
				g.DrawLines(val, rect.TriangleLines(ArrowProg));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal static Rectangle RectAlignL(Canvas g, bool textLine, bool textCenter, Font font, TAlignMini iconPosition, float iconratio, float icongap, Size font_size, Rectangle rect_read, out Rectangle rect_l)
	{
		int height = font_size.Height;
		if (textLine && (iconPosition == TAlignMini.Top || iconPosition == TAlignMini.Bottom))
		{
			height = g.MeasureString("龍Qq", font).Height;
		}
		int num = (int)((float)height * iconratio);
		int num2 = (int)((float)height * icongap);
		Rectangle result;
		if (textCenter)
		{
			switch (iconPosition)
			{
			case TAlignMini.Top:
			{
				int num6 = rect_read.Y + (rect_read.Height - font_size.Height) / 2;
				result = new Rectangle(rect_read.X, num6, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num6 - num - num2, num, num);
				break;
			}
			case TAlignMini.Bottom:
			{
				int num5 = rect_read.Y + (rect_read.Height - font_size.Height) / 2;
				result = new Rectangle(rect_read.X, num5, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num5 + font_size.Height + num2, num, num);
				break;
			}
			case TAlignMini.Right:
			{
				int num4 = rect_read.X + (rect_read.Width - font_size.Width) / 2;
				result = new Rectangle(num4, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num4 + font_size.Width + num2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			default:
			{
				int num3 = rect_read.X + (rect_read.Width - font_size.Width) / 2;
				result = new Rectangle(num3, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num3 - num - num2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			}
		}
		else
		{
			switch (iconPosition)
			{
			case TAlignMini.Top:
			{
				int num10 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
				result = new Rectangle(rect_read.X, num10 + num + num2, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num10, num, num);
				break;
			}
			case TAlignMini.Bottom:
			{
				int num9 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
				result = new Rectangle(rect_read.X, num9, rect_read.Width, font_size.Height);
				rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num9 + font_size.Height + num2, num, num);
				break;
			}
			case TAlignMini.Right:
			{
				int num8 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2)) / 2;
				result = new Rectangle(num8, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num8 + font_size.Width + num2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			default:
			{
				int num7 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2)) / 2;
				result = new Rectangle(num7 + num + num2, rect_read.Y, font_size.Width, rect_read.Height);
				rect_l = new Rectangle(num7, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				break;
			}
			}
		}
		return result;
	}

	internal static Rectangle RectAlignLR(Canvas g, bool textLine, Font font, TAlignMini iconPosition, float iconratio, float icongap, Size font_size, Rectangle rect_read, out Rectangle rect_l, out Rectangle rect_r)
	{
		int height = font_size.Height;
		if (textLine && (iconPosition == TAlignMini.Top || iconPosition == TAlignMini.Bottom))
		{
			height = g.MeasureString("龍Qq", font).Height;
		}
		int num = (int)((float)height * iconratio);
		int num2 = (int)((float)height * icongap);
		int num3 = (int)((float)font_size.Height * 0.4f);
		Rectangle result;
		switch (iconPosition)
		{
		case TAlignMini.Top:
		{
			int num7 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
			result = new Rectangle(rect_read.X, num7 + num + num2, rect_read.Width, font_size.Height);
			rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num7, num, num);
			rect_r = new Rectangle(rect_read.Right - num - num3, result.Y + (result.Height - num) / 2, num, num);
			break;
		}
		case TAlignMini.Bottom:
		{
			int num6 = rect_read.Y + (rect_read.Height - (font_size.Height + num + num2)) / 2;
			result = new Rectangle(rect_read.X, num6, rect_read.Width, font_size.Height);
			rect_l = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, num6 + font_size.Height + num2, num, num);
			rect_r = new Rectangle(rect_read.Right - num - num3, result.Y + (result.Height - num) / 2, num, num);
			break;
		}
		case TAlignMini.Right:
		{
			int num5 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2 + num3)) / 2;
			int y2 = rect_read.Y + (rect_read.Height - num) / 2;
			result = new Rectangle(num5, rect_read.Y, font_size.Width, rect_read.Height);
			rect_l = new Rectangle(num5 + font_size.Width + num2, y2, num, num);
			rect_r = new Rectangle(rect_read.X + num3, y2, num, num);
			break;
		}
		default:
		{
			int num4 = rect_read.X + (rect_read.Width - (font_size.Width + num + num2 + num3)) / 2;
			int y = rect_read.Y + (rect_read.Height - num) / 2;
			result = new Rectangle(num4 + num + num2, rect_read.Y, font_size.Width, rect_read.Height);
			rect_l = new Rectangle(num4, y, num, num);
			rect_r = new Rectangle(rect_read.Right - num - num3, y, num, num);
			break;
		}
		}
		return result;
	}

	internal static Rectangle RectAlignR(Canvas g, bool textLine, Font font, TAlignMini iconPosition, float iconratio, float icongap, Size font_size, Rectangle rect_read, out Rectangle rect_r)
	{
		int height = font_size.Height;
		if (textLine && (iconPosition == TAlignMini.Top || iconPosition == TAlignMini.Bottom))
		{
			height = g.MeasureString("龍Qq", font).Height;
		}
		int num = (int)((float)height * iconratio);
		int num2 = (int)((float)height * icongap);
		int num3 = (int)((float)font_size.Height * 0.4f);
		int num4 = num + num2;
		Rectangle result;
		switch (iconPosition)
		{
		case TAlignMini.Right:
		case TAlignMini.Bottom:
			result = new Rectangle(rect_read.X + num4, rect_read.Y, rect_read.Width - num4, rect_read.Height);
			rect_r = new Rectangle(rect_read.X + num3, rect_read.Y + (rect_read.Height - num) / 2, num, num);
			break;
		default:
			result = new Rectangle(rect_read.X, rect_read.Y, rect_read.Width - num4, rect_read.Height);
			rect_r = new Rectangle(rect_read.Right - num - num3, rect_read.Y + (rect_read.Height - num) / 2, num, num);
			break;
		}
		return result;
	}

	private void PaintTextAlign(Rectangle rect_read, ref Rectangle rect_text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		ContentAlignment val = textAlign;
		if ((int)val <= 4)
		{
			if (val - 1 <= 1 || (int)val == 4)
			{
				rect_text.Height = rect_read.Height - rect_text.Y;
			}
		}
		else if ((int)val == 16 || (int)val == 32 || (int)val == 64)
		{
			rect_text.Y = rect_read.Y;
			rect_text.Height = rect_read.Height;
		}
	}

	private Rectangle GetIconRectCenter(Size font_size, Rectangle rect_read)
	{
		if (IconSize.Width > 0 && IconSize.Height > 0)
		{
			int num = (int)((float)IconSize.Width * Config.Dpi);
			int num2 = (int)((float)IconSize.Height * Config.Dpi);
			return new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num2) / 2, num, num2);
		}
		int num3 = (int)((float)font_size.Height * IconRatio * 1.125f);
		return new Rectangle(rect_read.X + (rect_read.Width - num3) / 2, rect_read.Y + (rect_read.Height - num3) / 2, num3, num3);
	}

	private bool PaintIcon(Canvas g, Color? color, Rectangle rect_o, bool hastxt, bool enabled)
	{
		Rectangle rect = (hastxt ? GetIconRect(rect_o) : rect_o);
		if (AnimationIconHover)
		{
			PaintCoreIcon(g, rect, color, 1f - AnimationIconHoverValue);
			PaintCoreIconHover(g, rect, color, AnimationIconHoverValue);
			return false;
		}
		if (AnimationIconToggle)
		{
			float opacity = 1f - AnimationIconToggleValue;
			if (ExtraMouseHover)
			{
				if (!PaintCoreIcon(g, IconHover, IconHoverSvg, rect, color, opacity))
				{
					PaintCoreIcon(g, icon, iconSvg, rect, color, opacity);
				}
				if (!PaintCoreIcon(g, ToggleIconHover, ToggleIconHoverSvg, rect, color, AnimationIconToggleValue))
				{
					PaintCoreIcon(g, ToggleIcon, ToggleIconSvg, rect, color, AnimationIconToggleValue);
				}
			}
			else
			{
				PaintCoreIcon(g, icon, iconSvg, rect, color, opacity);
				PaintCoreIcon(g, iconToggle, iconSvgToggle, rect, color, AnimationIconToggleValue);
			}
			return false;
		}
		if (enabled)
		{
			if (ExtraMouseHover && PaintCoreIconHover(g, rect, color))
			{
				return false;
			}
			if (PaintCoreIcon(g, rect, color))
			{
				return false;
			}
		}
		else
		{
			if (ExtraMouseHover && PaintCoreIconHover(g, rect, color, 0.3f))
			{
				return false;
			}
			if (PaintCoreIcon(g, rect, color, 0.3f))
			{
				return false;
			}
		}
		return true;
	}

	private Rectangle GetIconRect(Rectangle rectl)
	{
		if (IconSize.Width > 0 && IconSize.Height > 0)
		{
			int num = (int)((float)IconSize.Width * Config.Dpi);
			int num2 = (int)((float)IconSize.Height * Config.Dpi);
			return new Rectangle(rectl.X + (rectl.Width - num) / 2, rectl.Y + (rectl.Height - num2) / 2, num, num2);
		}
		return rectl;
	}

	private bool PaintCoreIcon(Canvas g, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (!toggle)
		{
			return PaintCoreIcon(g, icon, iconSvg, rect, color, opacity);
		}
		return PaintCoreIcon(g, iconToggle, iconSvgToggle, rect, color, opacity);
	}

	private bool PaintCoreIconHover(Canvas g, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (!toggle)
		{
			return PaintCoreIcon(g, IconHover, IconHoverSvg, rect, color, opacity);
		}
		return PaintCoreIcon(g, ToggleIconHover, ToggleIconHoverSvg, rect, color, opacity);
	}

	private bool PaintCoreIcon(Canvas g, Image? icon, string? iconSvg, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (iconSvg != null)
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(iconSvg, rect, color);
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect, opacity);
					return true;
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		else if (icon != null)
		{
			g.Image(icon, rect, opacity);
			return true;
		}
		return false;
	}

	public GraphicsPath Path(RectangleF rect, float radius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (shape == TShape.Circle)
		{
			GraphicsPath val = new GraphicsPath();
			val.AddEllipse(rect);
			return val;
		}
		if (joinLeft && joinRight)
		{
			return rect.RoundPath(0f);
		}
		if (joinRight)
		{
			return rect.RoundPath(radius, TL: true, TR: false, BR: false, BL: true);
		}
		if (joinLeft)
		{
			return rect.RoundPath(radius, TL: false, TR: true, BR: true, BL: false);
		}
		return rect.RoundPath(radius);
	}

	private Color GetColorO()
	{
		if (toggle)
		{
			if (typeToggle.HasValue)
			{
				return GetColorO(typeToggle.Value);
			}
			return GetColorO(type);
		}
		return GetColorO(type);
	}

	private Color GetColorO(TTypeMini type)
	{
		Color result = type switch
		{
			TTypeMini.Default => (!(borderWidth > 0f)) ? Colour.FillSecondary.Get("Button") : Colour.PrimaryHover.Get("Button"), 
			TTypeMini.Success => Colour.SuccessHover.Get("Button"), 
			TTypeMini.Error => Colour.ErrorHover.Get("Button"), 
			TTypeMini.Info => Colour.InfoHover.Get("Button"), 
			TTypeMini.Warn => Colour.WarningHover.Get("Button"), 
			_ => Colour.PrimaryHover.Get("Button"), 
		};
		if (BackHover.HasValue)
		{
			result = BackHover.Value;
		}
		return result;
	}

	private void GetDefaultColorConfig(out Color Fore, out Color Color, out Color backHover, out Color backActive)
	{
		Fore = Colour.DefaultColor.Get("Button");
		Color = Colour.Primary.Get("Button");
		if (borderWidth > 0f)
		{
			backHover = Colour.PrimaryHover.Get("Button");
			backActive = Colour.PrimaryActive.Get("Button");
		}
		else
		{
			backHover = Colour.FillSecondary.Get("Button");
			backActive = Colour.Fill.Get("Button");
		}
		if (toggle)
		{
			if (foreToggle.HasValue)
			{
				Fore = foreToggle.Value;
			}
			if (ToggleBackHover.HasValue)
			{
				backHover = ToggleBackHover.Value;
			}
			if (ToggleBackActive.HasValue)
			{
				backActive = ToggleBackActive.Value;
			}
		}
		else
		{
			if (fore.HasValue)
			{
				Fore = fore.Value;
			}
			if (BackHover.HasValue)
			{
				backHover = BackHover.Value;
			}
			if (BackActive.HasValue)
			{
				backActive = BackActive.Value;
			}
		}
		if (AnimationBlinkState && colorBlink.HasValue)
		{
			Color = colorBlink.Value;
		}
		if (loading && LoadingValue > -1f)
		{
			Fore = Color.FromArgb(165, Fore);
			Color = Color.FromArgb(165, Color);
		}
	}

	private void GetColorConfig(out Color Fore, out Color Back, out Color backHover, out Color backActive)
	{
		if (toggle)
		{
			if (typeToggle.HasValue)
			{
				GetColorConfig(typeToggle.Value, out Fore, out Back, out backHover, out backActive);
			}
			else
			{
				GetColorConfig(type, out Fore, out Back, out backHover, out backActive);
			}
			if (foreToggle.HasValue)
			{
				Fore = foreToggle.Value;
			}
			if (backToggle.HasValue)
			{
				Back = backToggle.Value;
			}
			if (ToggleBackHover.HasValue)
			{
				backHover = ToggleBackHover.Value;
			}
			if (ToggleBackActive.HasValue)
			{
				backActive = ToggleBackActive.Value;
			}
			if (loading && LoadingValue > -1f)
			{
				Back = Color.FromArgb(165, Back);
			}
			return;
		}
		GetColorConfig(type, out Fore, out Back, out backHover, out backActive);
		if (fore.HasValue)
		{
			Fore = fore.Value;
		}
		if (back.HasValue)
		{
			Back = back.Value;
		}
		if (BackHover.HasValue)
		{
			backHover = BackHover.Value;
		}
		if (BackActive.HasValue)
		{
			backActive = BackActive.Value;
		}
		if (AnimationBlinkState && colorBlink.HasValue)
		{
			back = colorBlink.Value;
		}
		if (loading && LoadingValue > -1f)
		{
			Back = Color.FromArgb(165, Back);
		}
	}

	private void GetColorConfig(TTypeMini type, out Color Fore, out Color Back, out Color backHover, out Color backActive)
	{
		switch (type)
		{
		case TTypeMini.Error:
			Back = Colour.Error.Get("Button");
			Fore = Colour.ErrorColor.Get("Button");
			backHover = Colour.ErrorHover.Get("Button");
			backActive = Colour.ErrorActive.Get("Button");
			break;
		case TTypeMini.Success:
			Back = Colour.Success.Get("Button");
			Fore = Colour.SuccessColor.Get("Button");
			backHover = Colour.SuccessHover.Get("Button");
			backActive = Colour.SuccessActive.Get("Button");
			break;
		case TTypeMini.Info:
			Back = Colour.Info.Get("Button");
			Fore = Colour.InfoColor.Get("Button");
			backHover = Colour.InfoHover.Get("Button");
			backActive = Colour.InfoActive.Get("Button");
			break;
		case TTypeMini.Warn:
			Back = Colour.Warning.Get("Button");
			Fore = Colour.WarningColor.Get("Button");
			backHover = Colour.WarningHover.Get("Button");
			backActive = Colour.WarningActive.Get("Button");
			break;
		default:
			Back = Colour.Primary.Get("Button");
			Fore = Colour.PrimaryColor.Get("Button");
			backHover = Colour.PrimaryHover.Get("Button");
			backActive = Colour.PrimaryActive.Get("Button");
			break;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RespondRealAreas)
		{
			Rectangle readRectangle = ReadRectangle;
			GraphicsPath val = Path(readRectangle, (float)radius * Config.Dpi);
			try
			{
				ExtraMouseHover = val.IsVisible(e.Location);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		((Control)this).OnMouseEnter(e);
		if (!RespondRealAreas)
		{
			ExtraMouseHover = true;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ExtraMouseHover = false;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (CanClick(e.Location))
		{
			init = false;
			((Control)this).Focus();
			((Control)this).OnMouseDown(e);
			ExtraMouseDown = true;
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		if (ExtraMouseDown)
		{
			if (CanClick(e.Location))
			{
				((Control)this).OnMouseUp(e);
				if ((int)e.Button == 1048576)
				{
					ClickAnimation();
					((Control)this).OnClick((EventArgs)(object)e);
				}
				((Control)this).OnMouseClick(e);
			}
			ExtraMouseDown = false;
		}
		else
		{
			((Control)this).OnMouseUp(e);
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		BeforeAutoSize();
		((Control)this).OnFontChanged(e);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (autoSize == TAutoSize.None)
		{
			return ((Control)this).GetPreferredSize(proposedSize);
		}
		if (autoSize == TAutoSize.Width)
		{
			return new Size(PSize.Width, ((Control)this).GetPreferredSize(proposedSize).Height);
		}
		if (autoSize == TAutoSize.Height)
		{
			return new Size(((Control)this).GetPreferredSize(proposedSize).Width, PSize.Height);
		}
		return PSize;
	}

	protected override void OnResize(EventArgs e)
	{
		BeforeAutoSize();
		((Control)this).OnResize(e);
	}

	private bool BeforeAutoSize()
	{
		if (autoSize == TAutoSize.None)
		{
			return true;
		}
		if (((Control)this).InvokeRequired)
		{
			bool flag = false;
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				flag = BeforeAutoSize();
			});
			return flag;
		}
		Size pSize = PSize;
		switch (autoSize)
		{
		case TAutoSize.Width:
			if (((Control)this).Width == pSize.Width)
			{
				return true;
			}
			((Control)this).Width = pSize.Width;
			break;
		case TAutoSize.Height:
			if (((Control)this).Height == pSize.Height)
			{
				return true;
			}
			((Control)this).Height = pSize.Height;
			break;
		default:
			if (((Control)this).Width == pSize.Width && ((Control)this).Height == pSize.Height)
			{
				return true;
			}
			((Control)this).Size = pSize;
			break;
		}
		return false;
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		((Control)this).OnKeyUp(e);
		if ((int)e.KeyCode == 32 && CanClick())
		{
			ClickAnimation();
			((Control)this).OnClick(EventArgs.Empty);
			e.Handled = true;
		}
	}

	public void NotifyDefault(bool value)
	{
	}

	public void PerformClick()
	{
		if (((Control)this).CanSelect && CanClick())
		{
			ClickAnimation();
			((Control)this).OnClick(EventArgs.Empty);
		}
	}

	private bool CanClick(Point e)
	{
		if (loading)
		{
			return LoadingRespondClick;
		}
		if (RespondRealAreas)
		{
			Rectangle readRectangle = ReadRectangle;
			GraphicsPath val = Path(readRectangle, (float)radius * Config.Dpi);
			try
			{
				return val.IsVisible(e);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return ((Control)this).ClientRectangle.Contains(e);
	}

	private bool CanClick()
	{
		if (loading)
		{
			return LoadingRespondClick;
		}
		return true;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		((Control)this).OnGotFocus(e);
		if (init)
		{
			HasFocus = true;
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		((Control)this).OnLostFocus(e);
		HasFocus = false;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG)
		{
			BeforeAutoSize();
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadClick?.Dispose();
		ThreadHover?.Dispose();
		ThreadIconHover?.Dispose();
		ThreadIconToggle?.Dispose();
		ThreadLoading?.Dispose();
		ThreadAnimateBlink?.Dispose();
		base.Dispose(disposing);
	}
}
