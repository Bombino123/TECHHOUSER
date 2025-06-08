using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Input 输入框")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[DefaultEvent("TextChanged")]
public class Input : IControl
{
	internal class ICaret
	{
		private Input control;

		public Rectangle Rect = new Rectangle(-1, -1000, (int)Config.Dpi, 0);

		public bool Place;

		public bool FirstRet;

		public bool ReadShow;

		private bool show;

		internal bool flag;

		private ITask? CaretPrint;

		public int X
		{
			get
			{
				return Rect.X;
			}
			set
			{
				if (Rect.X != value || !flag)
				{
					Rect.X = value;
					flag = true;
					control.Invalidate();
				}
			}
		}

		public int Y => Rect.Y;

		public int Width => Rect.Width;

		public int Height
		{
			get
			{
				return Rect.Height;
			}
			set
			{
				Rect.Height = value;
			}
		}

		public bool Show
		{
			get
			{
				return show;
			}
			set
			{
				if (show == value)
				{
					return;
				}
				show = value;
				CaretPrint?.Dispose();
				if (!((Control)control).IsHandleCreated)
				{
					return;
				}
				if (show)
				{
					flag = true;
					if (ReadShow)
					{
						show = false;
						return;
					}
					CaretPrint = new ITask((Control)(object)control, delegate
					{
						Flag = !flag;
						return show;
					}, control.CaretSpeed, null, control.CaretSpeed);
					control.SetCaretPostion();
				}
				else
				{
					CaretPrint = null;
				}
				control.Invalidate();
			}
		}

		public bool Flag
		{
			get
			{
				return flag;
			}
			set
			{
				if (flag != value)
				{
					flag = value;
					if (show)
					{
						control.Invalidate();
					}
				}
			}
		}

		public ICaret(Input input)
		{
			control = input;
		}

		public Rectangle SetXY(CacheFont[] cache_font, int i)
		{
			if (FirstRet)
			{
				Rectangle? rectangle = ((i >= cache_font.Length) ? cache_font[^1].rect2 : ((i <= 0) ? cache_font[i].rect2 : cache_font[i - 1].rect2));
				if (rectangle.HasValue)
				{
					SetXY(rectangle.Value.X, rectangle.Value.Y);
					return rectangle.Value;
				}
			}
			if (i >= cache_font.Length)
			{
				Rectangle rect = cache_font[^1].rect;
				SetXY(rect.Right, rect.Y);
				return rect;
			}
			if (Place && i > 0)
			{
				Rectangle rect2 = cache_font[i - 1].rect;
				SetXY(rect2.Right, rect2.Y);
				return rect2;
			}
			Rectangle rect3 = cache_font[i].rect;
			SetXY(rect3.X, rect3.Y);
			return rect3;
		}

		public void SetXY(int x, int y)
		{
			if (Rect.X != x || Rect.Y != y || !flag)
			{
				Rect.X = x;
				Rect.Y = y;
				flag = true;
				control.Invalidate();
			}
		}

		public void Dispose()
		{
			CaretPrint?.Dispose();
		}
	}

	internal class TextHistoryRecord
	{
		public int SelectionStart { get; set; }

		public int SelectionLength { get; set; }

		public string Text { get; set; }

		public TextHistoryRecord(Input input)
		{
			SelectionStart = input.SelectionStart;
			SelectionLength = input.SelectionLength;
			Text = ((Control)input).Text;
		}
	}

	internal class CacheFont
	{
		public int i { get; set; }

		public int line { get; set; }

		public string text { get; set; }

		public Rectangle rect { get; set; }

		public Rectangle? rect2 { get; set; }

		public bool ret { get; set; }

		public bool emoji { get; set; }

		public int width { get; set; }

		internal bool show { get; set; }

		public CacheFont(string _text, bool _emoji, int _width)
		{
			text = _text;
			emoji = _emoji;
			width = _width;
		}

		public override string ToString()
		{
			return text;
		}
	}

	internal Color? fore;

	private Color? back;

	private string? backExtend;

	private Image? backImage;

	private TFit backFit;

	private Color selection = Color.FromArgb(102, 0, 127, 255);

	internal float borderWidth = 1f;

	internal Color? borderColor;

	internal int radius = 6;

	internal bool round;

	private TType status;

	private float iconratio = 0.7f;

	private float icongap = 0.25f;

	private Image? prefix;

	private string? prefixSvg;

	private string? prefixText;

	private Color? prefixFore;

	private Image? suffix;

	private string? suffixSvg;

	private string? suffixText;

	private Color? suffixFore;

	private bool allowclear;

	private bool is_clear;

	private bool is_clear_down;

	private bool is_prefix_down;

	private bool is_suffix_down;

	private bool autoscroll;

	internal bool isempty = true;

	private string _text = "";

	private ImeMode imeMode;

	private int selectionStart;

	private int selectionStartTemp;

	private int selectionLength;

	private bool readOnly;

	private bool multiline;

	private int lineheight;

	private HorizontalAlignment textalign;

	private string? placeholderText;

	private Color? placeholderColor;

	private string? placeholderColorExtend;

	private bool IsPassWord;

	private string PassWordChar = "●";

	private bool useSystemPasswordChar;

	private char passwordChar;

	private bool AnimationFocus;

	private int AnimationFocusValue;

	private IntPtr m_hIMC;

	internal ICaret CaretInfo;

	private int CurrentPosIndex;

	private bool SpeedScrollTo;

	private ITask? ThreadHover;

	private ITask? ThreadFocus;

	private int history_I = -1;

	private List<TextHistoryRecord> history_Log = new List<TextHistoryRecord>();

	private int tmpUp;

	private CacheFont[]? cache_font;

	private bool HasEmoji;

	internal Rectangle rect_text;

	internal Rectangle rect_l;

	internal Rectangle rect_r;

	internal Rectangle rect_d_ico;

	internal Rectangle rect_d_l;

	internal Rectangle rect_d_r;

	internal Rectangle? RECTDIV;

	private bool mDown;

	private bool mDownMove;

	private Point mDownLocation;

	private bool hover_clear;

	private List<string> sptext = new List<string>
	{
		"，", ",", "。", ".", "；", ";", " ", "/", "\\", "\r",
		"\t", "\n", "\r\n"
	};

	internal bool _mouseDown;

	internal int AnimationHoverValue;

	internal bool AnimationHover;

	internal bool _mouseHover;

	[CompilerGenerated]
	private MouseEventHandler? m_PrefixClick;

	[CompilerGenerated]
	private MouseEventHandler? m_SuffixClick;

	private StringFormat sf_font = Helper.SF_MEASURE_FONT();

	internal StringFormat sf_center = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	internal StringFormat sf_placeholder = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	internal Action? TakePaint;

	private Rectangle ScrollRect;

	private RectangleF ScrollSlider;

	private bool scrollhover;

	private int scrollx;

	private int scrolly;

	private int ScrollXMin;

	private int ScrollXMax;

	private int ScrollYMax;

	private bool ScrollXShow;

	private bool ScrollYShow;

	private bool ScrollYDown;

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
				Invalidate();
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
				Invalidate();
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
				Invalidate();
				OnPropertyChanged("BackExtend");
			}
		}
	}

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
				Invalidate();
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
				Invalidate();
				OnPropertyChanged("BackgroundImageLayout");
			}
		}
	}

	[Description("选中颜色")]
	[Category("外观")]
	[DefaultValue(typeof(Color), "102, 0, 127, 255")]
	public Color SelectionColor
	{
		get
		{
			return selection;
		}
		set
		{
			if (!(selection == value))
			{
				selection = value;
				Invalidate();
				OnPropertyChanged("SelectionColor");
			}
		}
	}

	[Description("光标颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? CaretColor { get; set; }

	[Description("光标速度")]
	[Category("外观")]
	[DefaultValue(1000)]
	public int CaretSpeed { get; set; } = 1000;


	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
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
				Invalidate();
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	[Description("边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			if (!(borderColor == value))
			{
				borderColor = value;
				Invalidate();
				OnPropertyChanged("BorderColor");
			}
		}
	}

	[Description("悬停边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderHover { get; set; }

	[Description("激活边框颜色")]
	[Category("边框")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderActive { get; set; }

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
				Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("圆角样式")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Round
	{
		get
		{
			return round;
		}
		set
		{
			if (round != value)
			{
				round = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Round");
			}
		}
	}

	[Description("设置校验状态")]
	[Category("外观")]
	[DefaultValue(TType.None)]
	public TType Status
	{
		get
		{
			return status;
		}
		set
		{
			if (status != value)
			{
				status = value;
				Invalidate();
				OnPropertyChanged("Status");
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
				CalculateRect();
				Invalidate();
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
				CalculateRect();
				Invalidate();
				OnPropertyChanged("IconGap");
			}
		}
	}

	[Description("前缀")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Prefix
	{
		get
		{
			return prefix;
		}
		set
		{
			if (prefix != value)
			{
				prefix = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Prefix");
			}
		}
	}

	[Description("前缀SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? PrefixSvg
	{
		get
		{
			return prefixSvg;
		}
		set
		{
			if (!(prefixSvg == value))
			{
				prefixSvg = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("PrefixSvg");
			}
		}
	}

	[Description("前缀文本")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? PrefixText
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationPrefixText, prefixText);
		}
		set
		{
			if (!(prefixText == value))
			{
				prefixText = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("PrefixText");
			}
		}
	}

	[Description("前缀文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPrefixText { get; set; }

	[Description("前缀前景色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? PrefixFore
	{
		get
		{
			return prefixFore;
		}
		set
		{
			if (!(prefixFore == value))
			{
				prefixFore = value;
				if (HasPrefix)
				{
					Invalidate();
				}
				OnPropertyChanged("PrefixFore");
			}
		}
	}

	public virtual bool HasPrefix
	{
		get
		{
			if (prefixSvg == null)
			{
				return prefix != null;
			}
			return true;
		}
	}

	[Description("后缀")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? Suffix
	{
		get
		{
			return suffix;
		}
		set
		{
			if (suffix != value)
			{
				suffix = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Suffix");
			}
		}
	}

	[Description("后缀SVG")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? SuffixSvg
	{
		get
		{
			return suffixSvg;
		}
		set
		{
			if (!(suffixSvg == value))
			{
				suffixSvg = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("SuffixSvg");
			}
		}
	}

	[Description("后缀文本")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? SuffixText
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationSuffixText, suffixText);
		}
		set
		{
			if (!(suffixText == value))
			{
				suffixText = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("SuffixText");
			}
		}
	}

	[Description("后缀文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationSuffixText { get; set; }

	[Description("后缀前景色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? SuffixFore
	{
		get
		{
			return suffixFore;
		}
		set
		{
			if (!(suffixFore == value))
			{
				suffixFore = value;
				if (HasSuffix)
				{
					Invalidate();
				}
				OnPropertyChanged("SuffixFore");
			}
		}
	}

	public virtual bool HasSuffix
	{
		get
		{
			if (suffixSvg == null)
			{
				return suffix != null;
			}
			return true;
		}
	}

	[Description("连接左边")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool JoinLeft { get; set; }

	[Description("连接右边")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool JoinRight { get; set; }

	[Description("支持清除")]
	[Category("行为")]
	[DefaultValue(false)]
	public virtual bool AllowClear
	{
		get
		{
			return allowclear;
		}
		set
		{
			if (allowclear != value)
			{
				allowclear = value;
				OnAllowClear();
				OnPropertyChanged("AllowClear");
			}
		}
	}

	[Description("是否显示滚动条")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool AutoScroll
	{
		get
		{
			return autoscroll;
		}
		set
		{
			if (autoscroll != value)
			{
				autoscroll = value;
				Invalidate();
				OnPropertyChanged("AutoScroll");
			}
		}
	}

	[Description("文本")]
	[Category("外观")]
	[DefaultValue("")]
	[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
	public override string Text
	{
		get
		{
			return ((Control)(object)this).GetLangIN(LocalizationText, _text);
		}
		set
		{
			if (value == null)
			{
				value = "";
			}
			if (_text == value)
			{
				return;
			}
			_text = value;
			isempty = string.IsNullOrEmpty(_text);
			FixFontWidth();
			OnAllowClear();
			if (isempty)
			{
				if (selectionStart > 0)
				{
					SelectionStart = 0;
				}
			}
			else if (cache_font != null && selectionStart > cache_font.Length)
			{
				SelectionStart = cache_font.Length;
			}
			Invalidate();
			((Control)this).OnTextChanged(EventArgs.Empty);
			OnPropertyChanged("Text");
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("Emoji字体")]
	[Category("外观")]
	[DefaultValue("Segoe UI Emoji")]
	public string EmojiFont { get; set; } = "Segoe UI Emoji";


	[Description("IME(输入法编辑器)状态")]
	[Category("行为")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public ImeMode ImeMode
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return imeMode;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (imeMode != value)
			{
				imeMode = value;
				SetImeMode(value);
				OnPropertyChanged("ImeMode");
			}
		}
	}

	[Browsable(false)]
	[DefaultValue(0)]
	public int SelectionStart
	{
		get
		{
			return selectionStart;
		}
		set
		{
			SpeedScrollTo = true;
			SetSelectionStart(value);
			SpeedScrollTo = false;
		}
	}

	[Browsable(false)]
	[DefaultValue(0)]
	public int SelectionLength
	{
		get
		{
			return selectionLength;
		}
		set
		{
			if (selectionLength != value)
			{
				selectionLength = value;
				Invalidate();
				OnPropertyChanged("SelectionLength");
			}
		}
	}

	[Description("多行编辑是否允许输入制表符")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool AcceptsTab { get; set; }

	[Description("焦点离开清空选中")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool LostFocusClearSelection { get; set; } = true;


	[Description("只读")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (readOnly != value)
			{
				readOnly = value;
				SetImeMode((ImeMode)(value ? 3 : ((int)imeMode)));
				OnPropertyChanged("ReadOnly");
			}
		}
	}

	[Description("多行文本")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool Multiline
	{
		get
		{
			return multiline;
		}
		set
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			if (multiline != value)
			{
				multiline = value;
				if (multiline)
				{
					StringFormat obj = sf_placeholder;
					obj.FormatFlags = (StringFormatFlags)(obj.FormatFlags & -4097);
					sf_placeholder.LineAlignment = (StringAlignment)0;
				}
				else
				{
					StringFormat obj2 = sf_placeholder;
					obj2.FormatFlags = (StringFormatFlags)(obj2.FormatFlags | 0x1000);
					sf_placeholder.LineAlignment = (StringAlignment)1;
				}
				CalculateRect();
				Invalidate();
				OnPropertyChanged("Multiline");
			}
		}
	}

	[Description("多行行高")]
	[Category("行为")]
	[DefaultValue(0)]
	public int LineHeight
	{
		get
		{
			return lineheight;
		}
		set
		{
			if (lineheight != value)
			{
				lineheight = value;
				CalculateRect();
				Invalidate();
				OnPropertyChanged("LineHeight");
			}
		}
	}

	[Description("文本对齐方向")]
	[Category("外观")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public HorizontalAlignment TextAlign
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return textalign;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (textalign != value)
			{
				textalign = value;
				textalign.SetAlignment(ref sf_placeholder);
				CalculateRect();
				Invalidate();
				OnPropertyChanged("TextAlign");
			}
		}
	}

	[Description("水印文本")]
	[Category("行为")]
	[DefaultValue(null)]
	[Localizable(true)]
	public virtual string? PlaceholderText
	{
		get
		{
			return ((Control)(object)this).GetLangI(LocalizationPlaceholderText, placeholderText);
		}
		set
		{
			if (!(placeholderText == value))
			{
				placeholderText = value;
				if (isempty && ShowPlaceholder)
				{
					Invalidate();
				}
				OnPropertyChanged("PlaceholderText");
			}
		}
	}

	[Description("水印文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationPlaceholderText { get; set; }

	[Description("水印颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? PlaceholderColor
	{
		get
		{
			return placeholderColor;
		}
		set
		{
			if (!(placeholderColor == value))
			{
				placeholderColor = value;
				if (isempty && ShowPlaceholder)
				{
					Invalidate();
				}
				OnPropertyChanged("PlaceholderColor");
			}
		}
	}

	[Description("水印渐变色")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? PlaceholderColorExtend
	{
		get
		{
			return placeholderColorExtend;
		}
		set
		{
			if (!(placeholderColorExtend == value))
			{
				placeholderColorExtend = value;
				if (isempty && ShowPlaceholder)
				{
					Invalidate();
				}
				OnPropertyChanged("PlaceholderColorExtend");
			}
		}
	}

	[Description("文本最大长度")]
	[Category("行为")]
	[DefaultValue(32767)]
	public int MaxLength { get; set; } = 32767;


	[Description("使用密码框")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool UseSystemPasswordChar
	{
		get
		{
			return useSystemPasswordChar;
		}
		set
		{
			if (useSystemPasswordChar != value)
			{
				useSystemPasswordChar = value;
				SetPassWord();
				OnPropertyChanged("UseSystemPasswordChar");
			}
		}
	}

	[Description("自定义密码字符")]
	[Category("行为")]
	[DefaultValue('\0')]
	public char PasswordChar
	{
		get
		{
			return passwordChar;
		}
		set
		{
			if (passwordChar != value)
			{
				passwordChar = value;
				SetPassWord();
				OnPropertyChanged("PasswordChar");
			}
		}
	}

	[Description("密码可以复制")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool PasswordCopy { get; set; }

	[Description("密码可以粘贴")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool PasswordPaste { get; set; } = true;


	[Browsable(false)]
	[Description("是否存在焦点")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool HasFocus { get; private set; }

	protected virtual bool BanInput => false;

	protected virtual bool HasValue => false;

	protected virtual bool ModeRange => false;

	protected virtual bool ShowPlaceholder => true;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseDown
	{
		get
		{
			return _mouseDown;
		}
		set
		{
			if (_mouseDown == value)
			{
				return;
			}
			_mouseDown = value;
			ChangeMouseHover(_mouseHover, value);
			if (Config.Animation && WaveSize > 0)
			{
				ThreadFocus?.Dispose();
				AnimationFocus = true;
				if (value)
				{
					ThreadFocus = new ITask((Control)(object)this, delegate
					{
						AnimationFocusValue += 4;
						if (AnimationFocusValue > 30)
						{
							return false;
						}
						Invalidate();
						return true;
					}, 20, delegate
					{
						AnimationFocus = false;
						Invalidate();
					});
					return;
				}
				ThreadFocus = new ITask((Control)(object)this, delegate
				{
					AnimationFocusValue -= 4;
					if (AnimationFocusValue < 1)
					{
						return false;
					}
					Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationFocus = false;
					Invalidate();
				});
			}
			else
			{
				Invalidate();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal bool ExtraMouseHover
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
			ChangeMouseHover(value, _mouseDown);
			if (!base.Enabled)
			{
				return;
			}
			OnAllowClear();
			if (Config.Animation && !ExtraMouseDown)
			{
				ThreadHover?.Dispose();
				AnimationHover = true;
				if (value)
				{
					ThreadHover = new ITask((Control)(object)this, delegate
					{
						AnimationHoverValue += 20;
						if (AnimationHoverValue > 255)
						{
							AnimationHoverValue = 255;
							return false;
						}
						Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationHover = false;
						Invalidate();
					});
				}
				else
				{
					ThreadHover = new ITask((Control)(object)this, delegate
					{
						AnimationHoverValue -= 20;
						if (AnimationHoverValue < 1)
						{
							AnimationHoverValue = 0;
							return false;
						}
						Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationHover = false;
						Invalidate();
					});
				}
			}
			else
			{
				AnimationHoverValue = 255;
			}
			Invalidate();
		}
	}

	public override Rectangle ReadRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Config.Dpi, JoinLeft, JoinRight);

	public override GraphicsPath RenderRegion
	{
		get
		{
			Rectangle readRectangle = ReadRectangle;
			float num = (round ? ((float)readRectangle.Height) : ((float)radius * Config.Dpi));
			return Path(readRectangle, num);
		}
	}

	private bool ScrollHover
	{
		get
		{
			return scrollhover;
		}
		set
		{
			if (scrollhover != value)
			{
				scrollhover = value;
				Invalidate();
			}
		}
	}

	private int ScrollX
	{
		get
		{
			return scrollx;
		}
		set
		{
			if (value > ScrollXMax)
			{
				value = ScrollXMax;
			}
			if (value < ScrollXMin)
			{
				value = ScrollXMin;
			}
			if (scrollx != value)
			{
				scrollx = value;
				Invalidate();
			}
		}
	}

	private int ScrollY
	{
		get
		{
			return scrolly;
		}
		set
		{
			if (value > ScrollYMax)
			{
				value = ScrollYMax;
			}
			if (value < 0)
			{
				value = 0;
			}
			if (scrolly != value)
			{
				scrolly = value;
				CaretInfo.flag = true;
				Invalidate();
			}
		}
	}

	[Description("前缀 点击时发生")]
	[Category("行为")]
	public event MouseEventHandler PrefixClick
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			MouseEventHandler val = this.m_PrefixClick;
			MouseEventHandler val2;
			do
			{
				val2 = val;
				MouseEventHandler value2 = (MouseEventHandler)Delegate.Combine((Delegate?)(object)val2, (Delegate?)(object)value);
				val = Interlocked.CompareExchange(ref this.m_PrefixClick, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			MouseEventHandler val = this.m_PrefixClick;
			MouseEventHandler val2;
			do
			{
				val2 = val;
				MouseEventHandler value2 = (MouseEventHandler)Delegate.Remove((Delegate?)(object)val2, (Delegate?)(object)value);
				val = Interlocked.CompareExchange(ref this.m_PrefixClick, value2, val2);
			}
			while (val != val2);
		}
	}

	[Description("后缀 点击时发生")]
	[Category("行为")]
	public event MouseEventHandler SuffixClick
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			MouseEventHandler val = this.m_SuffixClick;
			MouseEventHandler val2;
			do
			{
				val2 = val;
				MouseEventHandler value2 = (MouseEventHandler)Delegate.Combine((Delegate?)(object)val2, (Delegate?)(object)value);
				val = Interlocked.CompareExchange(ref this.m_SuffixClick, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			MouseEventHandler val = this.m_SuffixClick;
			MouseEventHandler val2;
			do
			{
				val2 = val;
				MouseEventHandler value2 = (MouseEventHandler)Delegate.Remove((Delegate?)(object)val2, (Delegate?)(object)value);
				val = Interlocked.CompareExchange(ref this.m_SuffixClick, value2, val2);
			}
			while (val != val2);
		}
	}

	public Input()
		: base(ControlType.Select)
	{
		((Control)this).BackColor = Color.Transparent;
		CaretInfo = new ICaret(this);
	}

	private void OnAllowClear()
	{
		bool flag = !ReadOnly && allowclear && _mouseHover && (!isempty || HasValue);
		if (is_clear != flag)
		{
			is_clear = flag;
			CalculateRect();
		}
	}

	private void SetImeMode(ImeMode value)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (((Control)this).InvokeRequired)
		{
			((Control)this).Invoke((Delegate)(Action)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				SetImeMode(value);
			});
		}
		else
		{
			((Control)this).ImeMode = value;
		}
	}

	private void SetSelectionStart(int value)
	{
		if (value < 0)
		{
			value = 0;
		}
		else if (value > 0)
		{
			if (cache_font == null)
			{
				value = 0;
			}
			else if (value > cache_font.Length)
			{
				value = cache_font.Length;
			}
		}
		if (selectionStart != value)
		{
			selectionStart = (selectionStartTemp = value);
			SetCaretPostion(value);
			OnPropertyChanged("SelectionStart");
		}
	}

	private void SetPassWord()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (passwordChar != 0)
		{
			PassWordChar = passwordChar.ToString();
			IsPassWord = true;
		}
		else if (useSystemPasswordChar)
		{
			PassWordChar = "●";
			IsPassWord = true;
		}
		else
		{
			IsPassWord = false;
		}
		SetImeMode((ImeMode)(IsPassWord ? 3 : ((int)imeMode)));
		FixFontWidth(force: true);
		Invalidate();
	}

	public void AppendText(string text)
	{
		string text3 = (((Control)this).Text = _text + text);
		CurrentPosIndex = text3.Length;
	}

	public void Clear()
	{
		((Control)this).Text = "";
	}

	public void ClearUndo()
	{
		history_Log.Clear();
	}

	public void Copy()
	{
		if (!IsPassWord || PasswordCopy)
		{
			string selectionText = GetSelectionText();
			if (!string.IsNullOrEmpty(selectionText))
			{
				((Control)(object)this).ClipboardSetText(selectionText);
			}
		}
	}

	public void Cut()
	{
		if (!IsPassWord || PasswordCopy)
		{
			string selectionText = GetSelectionText();
			if (!string.IsNullOrEmpty(selectionText))
			{
				((Control)(object)this).ClipboardSetText(selectionText);
				ProcessBackSpaceKey();
			}
		}
	}

	public void Paste()
	{
		if (IsPassWord && !PasswordPaste)
		{
			return;
		}
		string text = ((Control)(object)this).ClipboardGetText();
		if (text == null || string.IsNullOrEmpty(text))
		{
			return;
		}
		List<string> list = new List<string>(text.Length);
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			char key = text2[i];
			if (Verify(key, out string change))
			{
				list.Add(change ?? key.ToString());
			}
		}
		if (list.Count > 0)
		{
			EnterText(string.Join("", list), ismax: false);
		}
	}

	public void DeselectAll()
	{
		SelectionLength = 0;
	}

	public void Undo()
	{
		if ((!IsPassWord || PasswordCopy) && history_Log.Count > 0)
		{
			int num;
			if (history_I == -1)
			{
				num = history_Log.Count - 1;
				AddHistoryRecord();
			}
			else
			{
				num = history_I - 1;
			}
			if (num > -1)
			{
				TextHistoryRecord textHistoryRecord = history_Log[num];
				history_I = num;
				((Control)this).Text = textHistoryRecord.Text;
				SelectionStart = textHistoryRecord.SelectionStart;
				SelectionLength = textHistoryRecord.SelectionLength;
			}
		}
	}

	public void Redo()
	{
		if ((!IsPassWord || PasswordCopy) && history_Log.Count > 0 && history_I > -1)
		{
			int num = history_I + 1;
			if (history_Log.Count > num)
			{
				TextHistoryRecord textHistoryRecord = history_Log[num];
				history_I = num;
				((Control)this).Text = textHistoryRecord.Text;
				SelectionStart = textHistoryRecord.SelectionStart;
				SelectionLength = textHistoryRecord.SelectionLength;
			}
		}
	}

	public void Select(int start, int length)
	{
		SelectionStart = start;
		SelectionLength = length;
	}

	public void SelectAll()
	{
		if (cache_font != null)
		{
			SelectionStart = 0;
			SelectionLength = cache_font.Length;
		}
	}

	private void EnterText(string text, bool ismax = true)
	{
		if (ReadOnly || BanInput)
		{
			return;
		}
		AddHistoryRecord();
		int len = 0;
		GraphemeSplitter.Each(text, 0, delegate
		{
			len++;
			return true;
		});
		if (cache_font == null)
		{
			if (ismax && text.Length > MaxLength)
			{
				text = text.Substring(0, MaxLength);
			}
			((Control)this).Text = text;
			SetSelectionStart(len);
		}
		else if (selectionLength > 0)
		{
			int num = selectionStartTemp;
			int num2 = selectionLength;
			AddHistoryRecord();
			int num3 = num + num2;
			List<string> list = new List<string>(num2);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont in array)
			{
				if (cacheFont.i < num || cacheFont.i >= num3)
				{
					list.Add(cacheFont.text);
				}
			}
			list.Insert(num, text);
			string text2 = string.Join("", list);
			if (ismax && text2.Length > MaxLength)
			{
				text2 = text2.Substring(0, MaxLength);
			}
			((Control)this).Text = text2;
			SelectionLength = 0;
			SetSelectionStart(num + len);
		}
		else
		{
			int num4 = selectionStart - 1;
			List<string> list2 = new List<string>(cache_font.Length);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont2 in array)
			{
				list2.Add(cacheFont2.text);
			}
			list2.Insert(num4 + 1, text);
			string text3 = string.Join("", list2);
			if (ismax && text3.Length > MaxLength)
			{
				text3 = text3.Substring(0, MaxLength);
			}
			((Control)this).Text = text3;
			if (CaretInfo.FirstRet)
			{
				CaretInfo.Place = true;
				CaretInfo.FirstRet = false;
			}
			SetSelectionStart(num4 + 1 + len);
		}
	}

	private string? GetSelectionText()
	{
		if (cache_font == null)
		{
			return null;
		}
		if (selectionLength > 0)
		{
			int num = selectionStartTemp;
			int num2 = selectionLength;
			int num3 = num + num2;
			if (num3 > cache_font.Length)
			{
				num3 = cache_font.Length;
			}
			List<string> list = new List<string>(num2);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont in array)
			{
				if (cacheFont.i >= num && num3 > cacheFont.i)
				{
					list.Add(cacheFont.text);
				}
			}
			return string.Join("", list);
		}
		return null;
	}

	public void ScrollToCaret()
	{
		if (cache_font != null)
		{
			ScrollY = ((CurrentPosIndex < cache_font.Length) ? cache_font[CurrentPosIndex].rect : cache_font[cache_font.Length - 1].rect).Bottom;
		}
	}

	public void ScrollToEnd()
	{
		ScrollY = ScrollYMax;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		((Control)this).OnFontChanged(e);
		FixFontWidth(force: true);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		((Control)this).OnGotFocus(e);
		HasFocus = true;
		CaretInfo.Show = true;
		ExtraMouseDown = true;
	}

	protected override void OnLostFocus(EventArgs e)
	{
		((Control)this).OnLostFocus(e);
		HasFocus = false;
		CaretInfo.Show = false;
		if (LostFocusClearSelection)
		{
			SelectionLength = 0;
		}
		ExtraMouseDown = false;
	}

	protected override void WndProc(ref Message m)
	{
		switch (((Message)(ref m)).Msg)
		{
		case 269:
			m_hIMC = Win32.ImmGetContext(((Control)this).Handle);
			OnImeStartPrivate(m_hIMC);
			break;
		case 270:
			Win32.ImmReleaseContext(((Control)this).Handle, m_hIMC);
			break;
		case 271:
			if (((int)((Message)(ref m)).LParam & 0x800) == 2048)
			{
				((Message)(ref m)).Result = (IntPtr)1;
				OnImeResultStrPrivate(m_hIMC, Win32.ImmGetCompositionString(m_hIMC, 2048));
				return;
			}
			break;
		case 135:
			m_hIMC = Win32.ImmGetContext(((Control)this).Handle);
			OnImeStartPrivate(m_hIMC);
			if (multiline)
			{
				((Message)(ref m)).Result = (IntPtr)133;
			}
			else
			{
				((Message)(ref m)).Result = (IntPtr)129;
			}
			return;
		}
		base.WndProc(ref m);
	}

	protected virtual void ModeRangeCaretPostion(bool Null)
	{
	}

	protected virtual bool HasLeft()
	{
		return false;
	}

	protected virtual int[] UseLeft(Rectangle rect, bool delgap)
	{
		return new int[2];
	}

	protected virtual void UseLeftAutoHeight(int height, int gap, int y)
	{
	}

	protected virtual void IBackSpaceKey()
	{
	}

	protected virtual bool IMouseDown(Point e)
	{
		return false;
	}

	protected virtual bool IMouseMove(Point e)
	{
		return false;
	}

	protected virtual bool IMouseUp(Point e)
	{
		return false;
	}

	protected virtual void OnClearValue()
	{
		((Control)this).Text = "";
	}

	protected virtual void OnClickContent()
	{
	}

	protected virtual void ChangeMouseHover(bool Hover, bool Focus)
	{
	}

	private int GetCaretPostion(int x, int y)
	{
		CaretInfo.Place = false;
		if (cache_font == null)
		{
			return 0;
		}
		CacheFont[] array = cache_font;
		foreach (CacheFont cacheFont in array)
		{
			if (HasRect(cacheFont.rect, x, y))
			{
				if (x > cacheFont.rect.X + cacheFont.rect.Width / 2)
				{
					int num = cacheFont.i + 1;
					if (num > cache_font.Length - 1)
					{
						return num;
					}
					if (cache_font[cacheFont.i].rect.Y != cache_font[num].rect.Y)
					{
						CaretInfo.Place = true;
					}
					return num;
				}
				return cacheFont.i;
			}
			if (cacheFont.rect2.HasValue && HasRect(cacheFont.rect2.Value, x, y))
			{
				CaretInfo.Place = false;
				return cacheFont.i;
			}
		}
		bool two;
		CacheFont cacheFont2 = FindNearestFont(x, y, cache_font, out two);
		CaretInfo.FirstRet = two;
		if (cacheFont2 == null)
		{
			if (x > cache_font[cache_font.Length - 1].rect.Right)
			{
				return cache_font.Length;
			}
			return 0;
		}
		if (two)
		{
			return cacheFont2.i;
		}
		if (x > cacheFont2.rect.X + cacheFont2.rect.Width / 2)
		{
			int num2 = cacheFont2.i + 1;
			if (num2 > cache_font.Length - 1)
			{
				return num2;
			}
			if (cache_font[cacheFont2.i].rect.Y != cache_font[num2].rect.Y)
			{
				CaretInfo.Place = true;
			}
			return num2;
		}
		return cacheFont2.i;
	}

	private CacheFont? FindNearestFont(int x, int y, CacheFont[] cache_font, out bool two)
	{
		two = false;
		CacheFont cacheFont = cache_font[0];
		CacheFont cacheFont2 = cache_font[^1];
		if (x < cacheFont.rect.X && y < cacheFont.rect.Y)
		{
			return cacheFont;
		}
		if (x > cacheFont2.rect.X && y > cacheFont2.rect.Y)
		{
			return cacheFont2;
		}
		CacheFont cacheFont3 = FindNearestFontY(y, cache_font, out two);
		CacheFont result = null;
		if (cacheFont3 == null)
		{
			double num = 2147483647.0;
			foreach (CacheFont cacheFont4 in cache_font)
			{
				int num2 = Math.Abs(x - (cacheFont4.rect.X + cacheFont4.rect.Width / 2));
				int num3 = Math.Abs(y - (cacheFont4.rect.Y + cacheFont4.rect.Height / 2));
				double num4 = new int[2] { num2, num3 }.Average();
				if (num4 < num)
				{
					num = num4;
					result = cacheFont4;
				}
			}
		}
		else if (two && cacheFont3.rect2.HasValue)
		{
			int y2 = cacheFont3.rect2.Value.Y;
			int num5 = int.MaxValue;
			foreach (CacheFont cacheFont5 in cache_font)
			{
				if (cacheFont5.rect2.HasValue && cacheFont5.rect2.Value.Y == y2)
				{
					int num6 = Math.Abs(x - (cacheFont5.rect2.Value.X + cacheFont5.rect2.Value.Width / 2));
					if (num6 < num5)
					{
						num5 = num6;
						result = cacheFont5;
					}
				}
				else if (cacheFont5.rect.Y == y2)
				{
					int num7 = Math.Abs(x - (cacheFont5.rect.X + cacheFont5.rect.Width / 2));
					if (num7 < num5)
					{
						num5 = num7;
						result = cacheFont5;
					}
				}
			}
		}
		else
		{
			int y3 = cacheFont3.rect.Y;
			int num8 = int.MaxValue;
			foreach (CacheFont cacheFont6 in cache_font)
			{
				if (cacheFont6.rect.Y == y3)
				{
					int num9 = Math.Abs(x - (cacheFont6.rect.X + cacheFont6.rect.Width / 2));
					if (num9 < num8)
					{
						num8 = num9;
						result = cacheFont6;
					}
				}
			}
		}
		return result;
	}

	private CacheFont? FindNearestFontY(int y, CacheFont[] cache_font, out bool two)
	{
		two = false;
		int num = int.MaxValue;
		CacheFont result = null;
		foreach (CacheFont cacheFont in cache_font)
		{
			int num2 = Math.Abs(y - (cacheFont.rect.Y + cacheFont.rect.Height / 2));
			if (num2 < num)
			{
				two = false;
				num = num2;
				result = cacheFont;
			}
			if (cacheFont.rect2.HasValue)
			{
				num2 = Math.Abs(y - (cacheFont.rect2.Value.Y + cacheFont.rect2.Value.Height / 2));
				if (num2 < num)
				{
					two = true;
					num = num2;
					result = cacheFont;
				}
			}
		}
		return result;
	}

	private bool HasRect(Rectangle rect, int x, int y)
	{
		if (rect.X <= x && rect.Right >= x && rect.Y <= y)
		{
			return rect.Bottom >= y;
		}
		return false;
	}

	internal void SetCaretPostion()
	{
		SetCaretPostion(CurrentPosIndex);
	}

	internal void SetCaretPostion(int PosIndex)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		CurrentPosIndex = PosIndex;
		if (!CaretInfo.Show)
		{
			return;
		}
		if (cache_font == null)
		{
			if (ModeRange)
			{
				ModeRangeCaretPostion(Null: true);
			}
			else if ((int)textalign == 2)
			{
				CaretInfo.X = rect_text.X + rect_text.Width / 2;
			}
			else if ((int)textalign == 1)
			{
				CaretInfo.X = rect_text.Right;
			}
		}
		else
		{
			Rectangle r = CaretInfo.SetXY(cache_font, PosIndex);
			if (ModeRange)
			{
				ModeRangeCaretPostion(Null: false);
			}
			ScrollIFTo(r);
		}
		CaretInfo.flag = true;
		Invalidate();
	}

	private void OnImeStartPrivate(IntPtr hIMC)
	{
		Point location = CaretInfo.Rect.Location;
		location.Offset(0, -scrolly);
		Win32.CANDIDATEFORM cANDIDATEFORM = default(Win32.CANDIDATEFORM);
		cANDIDATEFORM.dwStyle = 64;
		cANDIDATEFORM.ptCurrentPos = location;
		Win32.CANDIDATEFORM fuck = cANDIDATEFORM;
		Win32.ImmSetCandidateWindow(hIMC, ref fuck);
		Win32.COMPOSITIONFORM cOMPOSITIONFORM = default(Win32.COMPOSITIONFORM);
		cOMPOSITIONFORM.dwStyle = 32;
		cOMPOSITIONFORM.ptCurrentPos = location;
		Win32.COMPOSITIONFORM lpCompForm = cOMPOSITIONFORM;
		Win32.ImmSetCompositionWindow(hIMC, ref lpCompForm);
		Win32.LOGFONT lOGFONT = default(Win32.LOGFONT);
		lOGFONT.lfHeight = CaretInfo.Rect.Height;
		lOGFONT.lfFaceName = ((Control)this).Font.Name + "\0";
		Win32.LOGFONT logFont = lOGFONT;
		Win32.ImmSetCompositionFont(hIMC, ref logFont);
	}

	private void OnImeResultStrPrivate(IntPtr hIMC, string? strResult)
	{
		Win32.COMPOSITIONFORM cOMPOSITIONFORM = default(Win32.COMPOSITIONFORM);
		cOMPOSITIONFORM.dwStyle = 32;
		cOMPOSITIONFORM.ptCurrentPos = CaretInfo.Rect.Location;
		Win32.COMPOSITIONFORM lpCompForm = cOMPOSITIONFORM;
		Win32.ImmSetCompositionWindow(hIMC, ref lpCompForm);
		if (strResult == null || string.IsNullOrEmpty(strResult))
		{
			return;
		}
		List<string> list = new List<string>(strResult.Length);
		for (int i = 0; i < strResult.Length; i++)
		{
			char key = strResult[i];
			if (Verify(key, out string change))
			{
				list.Add(change ?? key.ToString());
			}
		}
		if (list.Count > 0)
		{
			EnterText(string.Join("", list));
		}
	}

	protected virtual bool Verify(char key, out string? change)
	{
		change = null;
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		ThreadFocus?.Dispose();
		ThreadHover?.Dispose();
		CaretInfo.Dispose();
		base.Dispose(disposing);
	}

	private void AddHistoryRecord()
	{
		if (history_I > -1)
		{
			history_Log.RemoveRange(history_I + 1, history_Log.Count - (history_I + 1));
			history_I = -1;
		}
		if (!IsPassWord || PasswordCopy)
		{
			history_Log.Add(new TextHistoryRecord(this));
		}
	}

	public void IProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).ProcessCmdKey(ref msg, keyData);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected I4, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected I4, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I4
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected I4, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Invalid comparison between Unknown and I4
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		bool result = ((Control)this).ProcessCmdKey(ref msg, keyData);
		if ((int)keyData <= 131107)
		{
			if ((int)keyData <= 13)
			{
				if ((int)keyData == 8)
				{
					ProcessBackSpaceKey();
					return true;
				}
				if ((int)keyData != 9)
				{
					if ((int)keyData == 13 && multiline)
					{
						EnterText(Environment.NewLine);
						return true;
					}
				}
				else if (multiline && AcceptsTab)
				{
					EnterText("\t");
					return true;
				}
			}
			else
			{
				switch (keyData - 33)
				{
				default:
					switch (keyData - 65571)
					{
					case 2:
						ProcessLeftKey(shift: true);
						return true;
					case 3:
						if (multiline)
						{
							ProcessUpKey(shift: true);
						}
						else
						{
							ProcessLeftKey(shift: false);
						}
						return multiline;
					case 4:
						ProcessRightKey(shift: true);
						return true;
					case 5:
						if (multiline)
						{
							ProcessDownKey(shift: true);
						}
						else
						{
							ProcessRightKey(shift: false);
						}
						return multiline;
					case 1:
						SpeedScrollTo = true;
						ProcessHomeKey(ctrl: false, shift: true);
						SpeedScrollTo = false;
						return true;
					case 0:
						SpeedScrollTo = true;
						ProcessEndKey(ctrl: false, shift: true);
						SpeedScrollTo = false;
						return true;
					}
					if ((int)keyData != 131107)
					{
						break;
					}
					SpeedScrollTo = true;
					ProcessEndKey(ctrl: true, shift: false);
					SpeedScrollTo = false;
					return true;
				case 13:
					ProcessDelete();
					return true;
				case 4:
					ProcessLeftKey(shift: false);
					return true;
				case 5:
					if (multiline)
					{
						ProcessUpKey(shift: false);
					}
					else
					{
						ProcessLeftKey(shift: false);
					}
					return multiline;
				case 6:
					ProcessRightKey(shift: false);
					return true;
				case 7:
					if (multiline)
					{
						ProcessDownKey(shift: false);
					}
					else
					{
						ProcessRightKey(shift: false);
					}
					return multiline;
				case 3:
					SpeedScrollTo = true;
					ProcessHomeKey(ctrl: false, shift: false);
					SpeedScrollTo = false;
					return true;
				case 2:
					SpeedScrollTo = true;
					ProcessEndKey(ctrl: false, shift: false);
					SpeedScrollTo = false;
					return true;
				case 0:
					if (ScrollYShow && cache_font != null)
					{
						SpeedScrollTo = true;
						SelectionLength = 0;
						int caretPostion2 = GetCaretPostion(CaretInfo.Rect.X, CaretInfo.Rect.Y - (rect_text.Height - cache_font[0].rect.Height));
						SetSelectionStart(caretPostion2);
						SpeedScrollTo = false;
						return true;
					}
					break;
				case 1:
					if (ScrollYShow && cache_font != null)
					{
						SpeedScrollTo = true;
						SelectionLength = 0;
						int caretPostion = GetCaretPostion(CaretInfo.Rect.X, CaretInfo.Rect.Y + (rect_text.Height - cache_font[0].rect.Height));
						SetSelectionStart(caretPostion);
						SpeedScrollTo = false;
						return true;
					}
					break;
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
					break;
				}
			}
		}
		else if ((int)keyData <= 131139)
		{
			if ((int)keyData == 131108)
			{
				SpeedScrollTo = true;
				ProcessHomeKey(ctrl: true, shift: false);
				SpeedScrollTo = false;
				return true;
			}
			if ((int)keyData == 131137)
			{
				SelectAll();
				return true;
			}
			if ((int)keyData == 131139)
			{
				Copy();
				return true;
			}
		}
		else
		{
			switch (keyData - 131158)
			{
			default:
				if ((int)keyData != 196643)
				{
					if ((int)keyData != 196644)
					{
						break;
					}
					SpeedScrollTo = true;
					ProcessHomeKey(ctrl: true, shift: true);
					SpeedScrollTo = false;
					return true;
				}
				SpeedScrollTo = true;
				ProcessEndKey(ctrl: true, shift: true);
				SpeedScrollTo = false;
				return true;
			case 2:
				Cut();
				return true;
			case 0:
				Paste();
				return true;
			case 4:
				Undo();
				return true;
			case 3:
				Redo();
				return true;
			case 1:
				break;
			}
		}
		return result;
	}

	internal void IKeyPress(KeyPressEventArgs e)
	{
		((Control)this).OnKeyPress(e);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		string change;
		if (e.KeyChar < ' ')
		{
			((Control)this).OnKeyPress(e);
		}
		else if (Verify(e.KeyChar, out change))
		{
			EnterText(change ?? e.KeyChar.ToString());
			((Control)this).OnKeyPress(e);
		}
		else
		{
			e.Handled = true;
		}
	}

	private void ProcessBackSpaceKey()
	{
		if (ReadOnly || BanInput)
		{
			return;
		}
		if (cache_font == null)
		{
			IBackSpaceKey();
		}
		else if (selectionLength > 0)
		{
			int num = selectionStartTemp;
			int num2 = selectionLength;
			AddHistoryRecord();
			int num3 = num + num2;
			List<string> list = new List<string>(num2);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont in array)
			{
				if (cacheFont.i < num || cacheFont.i >= num3)
				{
					list.Add(cacheFont.text);
				}
			}
			((Control)this).Text = string.Join("", list);
			SelectionLength = 0;
			SetSelectionStart(num);
		}
		else
		{
			if (selectionStart <= 0)
			{
				return;
			}
			int num4 = selectionStart - 1;
			if (num4 == 0)
			{
				CaretInfo.FirstRet = true;
			}
			List<string> list2 = new List<string>(cache_font.Length);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont2 in array)
			{
				if (num4 != cacheFont2.i)
				{
					list2.Add(cacheFont2.text);
				}
			}
			((Control)this).Text = string.Join("", list2);
			SetSelectionStart(num4);
		}
	}

	private void ProcessDelete()
	{
		if (cache_font == null || ReadOnly || BanInput)
		{
			return;
		}
		if (selectionLength > 0)
		{
			int num = selectionStartTemp;
			int num2 = selectionLength;
			AddHistoryRecord();
			int num3 = num + num2;
			List<string> list = new List<string>(num2);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont in array)
			{
				if (cacheFont.i < num || cacheFont.i >= num3)
				{
					list.Add(cacheFont.text);
				}
			}
			((Control)this).Text = string.Join("", list);
			SelectionLength = 0;
			SetSelectionStart(num);
		}
		else
		{
			if (selectionStart >= cache_font.Length)
			{
				return;
			}
			int num4 = selectionStart;
			List<string> list2 = new List<string>(cache_font.Length);
			CacheFont[] array = cache_font;
			foreach (CacheFont cacheFont2 in array)
			{
				if (num4 != cacheFont2.i)
				{
					list2.Add(cacheFont2.text);
				}
			}
			((Control)this).Text = string.Join("", list2);
			SetSelectionStart(num4);
		}
	}

	private void ProcessLeftKey(bool shift)
	{
		tmpUp = 0;
		if (shift)
		{
			int num = selectionStartTemp;
			if (selectionStartTemp == selectionStart || selectionStartTemp < selectionStart)
			{
				selectionStartTemp--;
			}
			if (selectionStartTemp < 0)
			{
				selectionStartTemp = 0;
			}
			if (num != selectionStartTemp)
			{
				SelectionLength++;
				CurrentPosIndex = selectionStartTemp;
				SetCaretPostion();
			}
		}
		else if (SelectionLength > 0)
		{
			if (selectionStartTemp < selectionStart)
			{
				SetSelectionStart(selectionStartTemp);
			}
			else
			{
				int num2 = selectionStart;
				selectionStart--;
				SetSelectionStart(num2);
			}
			SelectionLength = 0;
		}
		else
		{
			SelectionLength = 0;
			if (selectionStart == 1)
			{
				CaretInfo.FirstRet = true;
			}
			SetSelectionStart(selectionStart - 1);
		}
	}

	private void ProcessRightKey(bool shift)
	{
		tmpUp = 0;
		if (CaretInfo.FirstRet)
		{
			CaretInfo.FirstRet = false;
			CaretInfo.Place = true;
		}
		if (shift)
		{
			if (selectionStart > selectionStartTemp)
			{
				selectionStartTemp++;
				SelectionLength--;
				CurrentPosIndex = selectionStartTemp + selectionLength;
			}
			else
			{
				SelectionLength++;
				CurrentPosIndex = selectionStart + selectionLength;
			}
			SetCaretPostion();
		}
		else if (SelectionLength > 0)
		{
			if (selectionStartTemp > selectionStart)
			{
				SetSelectionStart(selectionStartTemp + selectionLength);
			}
			else
			{
				int num = selectionStart;
				selectionStart--;
				SetSelectionStart(num + selectionLength);
			}
			SelectionLength = 0;
		}
		else
		{
			SelectionLength = 0;
			SetSelectionStart(selectionStart + 1);
		}
	}

	private void ProcessUpKey(bool shift)
	{
		if (shift)
		{
			if (cache_font == null)
			{
				SelectionLength = 0;
				SetSelectionStart(selectionStart - 1);
				return;
			}
			int num = selectionStartTemp;
			int num2 = cache_font.Length - 1;
			if (num > num2)
			{
				num = num2;
			}
			CacheFont cacheFont = cache_font[num];
			bool two;
			CacheFont cacheFont2 = FindNearestFont(cacheFont.rect.X + cacheFont.rect.Width / 2, cacheFont.rect.Y - cacheFont.rect.Height / 2, cache_font, out two);
			CaretInfo.FirstRet = two;
			if (cacheFont2 == null || cacheFont2.i == selectionStartTemp)
			{
				SetSelectionStart(num - 1);
				SelectionLength++;
			}
			else
			{
				SetSelectionStart(cacheFont2.i);
				SelectionLength += num - cacheFont2.i + ((num >= num2) ? 1 : 0);
			}
			return;
		}
		SelectionLength = 0;
		if (cache_font == null)
		{
			SetSelectionStart(selectionStart - 1);
			return;
		}
		int num3 = SelectionStart;
		if (num3 > cache_font.Length - 1)
		{
			num3 = cache_font.Length - 1;
		}
		CacheFont cacheFont3 = cache_font[num3];
		bool two2;
		CacheFont cacheFont4 = FindNearestFont(cacheFont3.rect.X + cacheFont3.rect.Width / 2, cacheFont3.rect.Y - cacheFont3.rect.Height / 2, cache_font, out two2);
		CaretInfo.FirstRet = two2;
		if (cacheFont4 == null)
		{
			SetSelectionStart(selectionStart - 1);
			return;
		}
		if (cacheFont4.i == 0)
		{
			if (tmpUp > 0)
			{
				CaretInfo.FirstRet = true;
				tmpUp = 0;
				SetCaretPostion(cacheFont4.i);
			}
			else if (!CaretInfo.FirstRet)
			{
				tmpUp++;
			}
		}
		else
		{
			tmpUp = 0;
		}
		if (cacheFont4.i == selectionStart)
		{
			SetSelectionStart(selectionStart - 1);
		}
		else
		{
			SetSelectionStart(cacheFont4.i);
		}
	}

	private void ProcessDownKey(bool shift)
	{
		tmpUp = 0;
		if (CaretInfo.FirstRet)
		{
			CaretInfo.FirstRet = false;
			CaretInfo.Place = true;
			tmpUp++;
		}
		if (shift)
		{
			if (cache_font == null)
			{
				SelectionLength = 0;
				SetSelectionStart(selectionStart + 1);
				return;
			}
			int num = selectionStartTemp + selectionLength;
			if (num <= cache_font.Length - 1)
			{
				CacheFont cacheFont = cache_font[num];
				bool two;
				CacheFont cacheFont2 = FindNearestFont(cacheFont.rect.X + cacheFont.rect.Width / 2, cacheFont.rect.Bottom + cacheFont.rect.Height / 2, cache_font, out two);
				CaretInfo.FirstRet = two;
				if (cacheFont2 == null || cacheFont2.i == num)
				{
					SelectionLength++;
				}
				else
				{
					SelectionLength += cacheFont2.i - num;
				}
				CurrentPosIndex = selectionStart + selectionLength;
				SetCaretPostion();
			}
			return;
		}
		SelectionLength = 0;
		if (cache_font == null)
		{
			SetSelectionStart(selectionStart + 1);
			return;
		}
		int num2 = SelectionStart;
		if (num2 <= cache_font.Length - 1)
		{
			CacheFont cacheFont3 = cache_font[num2];
			bool two2;
			CacheFont cacheFont4 = FindNearestFont(cacheFont3.rect.X + cacheFont3.rect.Width / 2, cacheFont3.rect.Bottom + cacheFont3.rect.Height / 2, cache_font, out two2);
			CaretInfo.FirstRet = two2;
			if (cacheFont4 == null || cacheFont4.i == selectionStart)
			{
				SetSelectionStart(selectionStart + 1);
			}
			else
			{
				SetSelectionStart(cacheFont4.i);
			}
		}
	}

	private void ProcessHomeKey(bool ctrl, bool shift)
	{
		if (ctrl && shift)
		{
			int num = selectionStartTemp;
			if (num != 0)
			{
				if (ScrollYShow)
				{
					ScrollY = 0;
				}
				SetSelectionStart(0);
				SelectionLength += num;
			}
			return;
		}
		SelectionLength = 0;
		if (ctrl)
		{
			if (ScrollYShow)
			{
				ScrollY = 0;
			}
			SetSelectionStart(0);
		}
		else if (multiline)
		{
			if (cache_font == null)
			{
				return;
			}
			int num2 = selectionStartTemp;
			if (num2 > 0)
			{
				int num3 = FindStartY(cache_font, num2 - 1);
				if (num3 != num2)
				{
					CaretInfo.Place = false;
					SetSelectionStart(num3);
				}
			}
		}
		else
		{
			if (ScrollYShow)
			{
				ScrollY = 0;
			}
			SetSelectionStart(0);
		}
	}

	private void ProcessEndKey(bool ctrl, bool shift)
	{
		if (cache_font == null)
		{
			return;
		}
		if (ctrl && shift)
		{
			if (selectionStartTemp + selectionLength <= cache_font.Length - 1)
			{
				if (ScrollYShow)
				{
					ScrollY = ScrollYMax;
				}
				SelectionLength += cache_font.Length - selectionStartTemp;
			}
		}
		else if (ctrl)
		{
			if (ScrollYShow)
			{
				ScrollY = ScrollYMax;
			}
			SelectionLength = 0;
			SetSelectionStart(cache_font.Length);
		}
		else if (multiline)
		{
			int num = selectionStartTemp + selectionLength;
			if (num <= cache_font.Length - 1)
			{
				int num2 = FindEndY(cache_font, num) + 1;
				if (num2 != num)
				{
					CaretInfo.Place = true;
					SetSelectionStart(num2);
				}
			}
		}
		else
		{
			if (ScrollYShow)
			{
				ScrollY = ScrollYMax;
			}
			SelectionLength = 0;
			SetSelectionStart(cache_font.Length);
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		FixFontWidth(force: true);
		((Control)this).OnHandleCreated(e);
	}

	private void FixFontWidth(bool force = false)
	{
		HasEmoji = false;
		string text = ((Control)this).Text;
		if (force)
		{
			Helper.GDI(delegate(Canvas g)
			{
				//IL_018f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0196: Expected O, but got Unknown
				Canvas g3 = g;
				_ = Config.Dpi;
				int font_height2 = g3.MeasureString("龍Qq", ((Control)this).Font, 10000, sf_font).Height;
				if (isempty)
				{
					Input input = this;
					int scrollX2 = (ScrollY = 0);
					input.ScrollX = scrollX2;
					cache_font = null;
				}
				else
				{
					List<CacheFont> font_widths2 = new List<CacheFont>(text.Length);
					if (IsPassWord)
					{
						Size size6 = g3.MeasureString(PassWordChar, ((Control)this).Font, 10000, sf_font);
						int width2 = size6.Width;
						if (font_height2 < size6.Height)
						{
							font_height2 = size6.Height;
						}
						string text4 = text;
						foreach (char c2 in text4)
						{
							font_widths2.Add(new CacheFont(c2.ToString(), _emoji: false, width2));
						}
					}
					else
					{
						GraphemeSplitter.Each(text, 0, delegate(string str, int nStart, int nLen)
						{
							string text5 = str.Substring(nStart, nLen);
							UnicodeCategory unicodeCategory2 = CharUnicodeInfo.GetUnicodeCategory(text5[0]);
							if (IsEmoji(unicodeCategory2))
							{
								HasEmoji = true;
								font_widths2.Add(new CacheFont(text5, _emoji: true, 0));
							}
							else
							{
								switch (text5)
								{
								case "\t":
								{
									Size size9 = g3.MeasureString(" ", ((Control)this).Font, 10000, sf_font);
									if (font_height2 < size9.Height)
									{
										font_height2 = size9.Height;
									}
									font_widths2.Add(new CacheFont(text5, _emoji: false, (int)Math.Ceiling((float)size9.Width * 8f)));
									break;
								}
								case "\n":
								case "\r\n":
								{
									Size size10 = g3.MeasureString(" ", ((Control)this).Font, 10000, sf_font);
									if (font_height2 < size10.Height)
									{
										font_height2 = size10.Height;
									}
									font_widths2.Add(new CacheFont(text5, _emoji: false, size10.Width));
									break;
								}
								default:
								{
									Size size8 = g3.MeasureString(text5, ((Control)this).Font, 10000, sf_font);
									if (font_height2 < size8.Height)
									{
										font_height2 = size8.Height;
									}
									font_widths2.Add(new CacheFont(text5, _emoji: false, size8.Width));
									break;
								}
								}
							}
							return true;
						});
						if (HasEmoji)
						{
							Font val2 = new Font(EmojiFont, ((Control)this).Font.Size);
							try
							{
								foreach (CacheFont item in font_widths2)
								{
									if (item.emoji)
									{
										Size size7 = g3.MeasureString(item.text, val2, 10000, sf_font);
										if (font_height2 < size7.Height)
										{
											font_height2 = size7.Height;
										}
										item.width = size7.Width;
									}
								}
							}
							finally
							{
								((IDisposable)val2)?.Dispose();
							}
						}
					}
					for (int k = 0; k < font_widths2.Count; k++)
					{
						font_widths2[k].i = k;
					}
					cache_font = font_widths2.ToArray();
				}
				CaretInfo.Height = font_height2;
				CalculateRect();
			});
			return;
		}
		if (isempty)
		{
			int scrollX = (ScrollY = 0);
			ScrollX = scrollX;
			cache_font = null;
			CalculateRect();
			return;
		}
		Helper.GDI(delegate(Canvas g)
		{
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Expected O, but got Unknown
			Canvas g2 = g;
			int font_height = g2.MeasureString("龍Qq", ((Control)this).Font, 10000, sf_font).Height;
			if (text == null)
			{
				CaretInfo.Height = font_height;
			}
			else
			{
				List<CacheFont> font_widths = new List<CacheFont>(text.Length);
				if (IsPassWord)
				{
					Size size = g2.MeasureString(PassWordChar, ((Control)this).Font, 10000, sf_font);
					int width = size.Width;
					if (font_height < size.Height)
					{
						font_height = size.Height;
					}
					string text2 = text;
					foreach (char c in text2)
					{
						font_widths.Add(new CacheFont(c.ToString(), _emoji: false, width));
					}
				}
				else
				{
					Dictionary<string, CacheFont> font_dir = new Dictionary<string, CacheFont>(font_widths.Count);
					if (cache_font != null)
					{
						CacheFont[] array = cache_font;
						foreach (CacheFont cacheFont in array)
						{
							if (!cacheFont.emoji && !font_dir.ContainsKey(cacheFont.text))
							{
								font_dir.Add(cacheFont.text, cacheFont);
							}
						}
					}
					GraphemeSplitter.Each(text, 0, delegate(string str, int nStart, int nLen)
					{
						string text3 = str.Substring(nStart, nLen);
						UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(text3[0]);
						CacheFont value;
						if (IsEmoji(unicodeCategory))
						{
							HasEmoji = true;
							font_widths.Add(new CacheFont(text3, _emoji: true, 0));
						}
						else if (font_dir.TryGetValue(text3, out value))
						{
							if (font_height < value.rect.Height)
							{
								font_height = value.rect.Height;
							}
							font_widths.Add(new CacheFont(text3, _emoji: false, value.width));
						}
						else
						{
							switch (text3)
							{
							case "\t":
							{
								Size size4 = g2.MeasureString(" ", ((Control)this).Font, 10000, sf_font);
								if (font_height < size4.Height)
								{
									font_height = size4.Height;
								}
								font_widths.Add(new CacheFont(text3, _emoji: false, (int)Math.Ceiling((float)size4.Width * 8f)));
								break;
							}
							case "\n":
							case "\r\n":
							{
								Size size5 = g2.MeasureString(" ", ((Control)this).Font, 10000, sf_font);
								if (font_height < size5.Height)
								{
									font_height = size5.Height;
								}
								font_widths.Add(new CacheFont(text3, _emoji: false, size5.Width));
								break;
							}
							default:
							{
								Size size3 = g2.MeasureString(text3, ((Control)this).Font, 10000, sf_font);
								if (font_height < size3.Height)
								{
									font_height = size3.Height;
								}
								font_widths.Add(new CacheFont(text3, _emoji: false, size3.Width));
								break;
							}
							}
						}
						return true;
					});
					if (HasEmoji)
					{
						Font val = new Font(EmojiFont, ((Control)this).Font.Size);
						try
						{
							foreach (CacheFont item2 in font_widths)
							{
								if (item2.emoji)
								{
									Size size2 = g2.MeasureString(item2.text, val, 10000, sf_font);
									if (font_height < size2.Height)
									{
										font_height = size2.Height;
									}
									item2.width = size2.Width;
								}
							}
						}
						finally
						{
							((IDisposable)val)?.Dispose();
						}
					}
				}
				for (int j = 0; j < font_widths.Count; j++)
				{
					font_widths[j].i = j;
				}
				cache_font = font_widths.ToArray();
				CaretInfo.Height = font_height;
				CalculateRect();
			}
		});
	}

	private bool IsEmoji(UnicodeCategory unicodeInfo)
	{
		if (unicodeInfo != UnicodeCategory.Surrogate && unicodeInfo != UnicodeCategory.OtherSymbol && unicodeInfo != UnicodeCategory.MathSymbol && unicodeInfo != UnicodeCategory.EnclosingMark && unicodeInfo != UnicodeCategory.NonSpacingMark)
		{
			return unicodeInfo == UnicodeCategory.ModifierLetter;
		}
		return true;
	}

	internal void CalculateRect()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bc: Invalid comparison between Unknown and I4
		//IL_0a58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5e: Invalid comparison between Unknown and I4
		//IL_0b2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b34: Invalid comparison between Unknown and I4
		//IL_06eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f1: Invalid comparison between Unknown and I4
		//IL_0b36: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b39: Invalid comparison between Unknown and I4
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_081e: Invalid comparison between Unknown and I4
		//IL_0c3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Invalid comparison between Unknown and I4
		Rectangle rect = (RECTDIV.HasValue ? RECTDIV.Value.PaddingRect(((Control)this).Padding).ReadRect(((float)WaveSize + borderWidth / 2f) * Config.Dpi, JoinLeft, JoinRight) : ReadRectangle);
		int num = (int)((float)CaretInfo.Height * 0.4f);
		int sps = num * 2;
		RectAuto(rect, num, sps);
		if (cache_font == null)
		{
			if (ModeRange)
			{
				int num2 = rect_text.Width / 2;
				int num3 = CaretInfo.Height / 2;
				rect_d_ico = new Rectangle(rect_text.X + num2 - num3, rect_text.Y + (rect_text.Height - CaretInfo.Height) / 2, CaretInfo.Height, CaretInfo.Height);
				rect_d_l = new Rectangle(rect_text.X, rect_text.Y, num2 - num3, rect_text.Height);
				rect_d_r = new Rectangle(rect_d_l.Right + CaretInfo.Height, rect_text.Y, rect_d_l.Width, rect_text.Height);
			}
			CaretInfo.Place = false;
			CaretInfo.SetXY(rect_text.X, rect_text.Y);
		}
		else
		{
			if (multiline)
			{
				Rectangle rectangle = rect_text;
				if (ScrollYShow)
				{
					rectangle.Width -= 16;
				}
				int num4 = CaretInfo.Height + ((lineheight > 0) ? ((int)((float)lineheight * Config.Dpi)) : 0);
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				CacheFont[] array = cache_font;
				foreach (CacheFont cacheFont in array)
				{
					cacheFont.show = false;
					if (cacheFont.text == "\r")
					{
						cacheFont.rect = new Rectangle(rectangle.X + num5, rectangle.Y + num6, cacheFont.width, CaretInfo.Height);
					}
					else if (cacheFont.text == "\n" || cacheFont.text == "\r\n")
					{
						cacheFont.ret = true;
						cacheFont.line = num7;
						num7++;
						if (num5 == 0 && num6 == 0)
						{
							cacheFont.rect2 = new Rectangle(rectangle.X + num5, rectangle.Y + num6, 0, CaretInfo.Height);
						}
						num6 += num4;
						num5 = 0;
						cacheFont.rect = new Rectangle(rectangle.X + num5, rectangle.Y + num6, 0, CaretInfo.Height);
					}
					else
					{
						if (num5 + cacheFont.width > rectangle.Width)
						{
							num7++;
							num6 += num4;
							num5 = 0;
						}
						cacheFont.line = num7;
						cacheFont.rect = new Rectangle(rectangle.X + num5, rectangle.Y + num6, cacheFont.width, CaretInfo.Height);
						num5 += cacheFont.width;
					}
				}
			}
			else
			{
				int num8 = 0;
				if (ModeRange)
				{
					int num9 = rect_text.Width / 2;
					int num10 = CaretInfo.Height / 2;
					rect_d_ico = new Rectangle(rect_text.X + num9 - num10, rect_text.Y + (rect_text.Height - CaretInfo.Height) / 2, CaretInfo.Height, CaretInfo.Height);
					rect_d_l = new Rectangle(rect_text.X, rect_text.Y, num9 - num10, rect_text.Height);
					rect_d_r = new Rectangle(rect_d_l.Right + CaretInfo.Height, rect_text.Y, rect_d_l.Width, rect_text.Height);
					int num11 = GetTabIndex();
					List<int> list3 = new List<int>(cache_font.Length);
					List<int> list4 = new List<int>(list3.Count);
					if (num11 == -1)
					{
						for (int j = 0; j < cache_font.Length; j++)
						{
							CacheFont cacheFont2 = cache_font[j];
							cacheFont2.show = true;
							cacheFont2.rect = new Rectangle(rect_d_l.X + num8, rect_text.Y, cacheFont2.width, CaretInfo.Height);
							num8 += cacheFont2.width;
							list3.Add(j);
						}
					}
					else if (num11 > 0)
					{
						for (int k = 0; k < num11; k++)
						{
							CacheFont cacheFont3 = cache_font[k];
							cacheFont3.show = true;
							cacheFont3.rect = new Rectangle(rect_d_l.X + num8, rect_text.Y, cacheFont3.width, CaretInfo.Height);
							num8 += cacheFont3.width;
							list3.Add(k);
						}
						Rectangle rect2 = cache_font[num11 - 1].rect;
						cache_font[num11].rect = new Rectangle(rect2.Right, rect2.Y, 0, rect2.Height);
						int num12 = 0;
						for (int l = num11 + 1; l < cache_font.Length; l++)
						{
							CacheFont cacheFont4 = cache_font[l];
							cacheFont4.show = true;
							cacheFont4.rect = new Rectangle(rect_d_r.X + num12, rect_text.Y, cacheFont4.width, CaretInfo.Height);
							num12 += cacheFont4.width;
							list4.Add(l);
						}
					}
					else
					{
						int num13 = 0;
						for (int m = num11 + 1; m < cache_font.Length; m++)
						{
							CacheFont cacheFont5 = cache_font[m];
							cacheFont5.show = true;
							cacheFont5.rect = new Rectangle(rect_d_r.X + num13, rect_text.Y, cacheFont5.width, CaretInfo.Height);
							num13 += cacheFont5.width;
							list4.Add(m);
						}
					}
					if ((int)textalign == 1)
					{
						if (list3.Count > 0)
						{
							int x = rect_d_l.Right - cache_font[list3[list3.Count - 1]].rect.Right;
							foreach (int item in list3)
							{
								CacheFont obj = cache_font[item];
								Rectangle rect3 = obj.rect;
								rect3.Offset(x, 0);
								obj.rect = rect3;
							}
						}
						if (list4.Count > 0)
						{
							int x2 = rect_d_r.Right - cache_font[list4[list4.Count - 1]].rect.Right;
							foreach (int item2 in list4)
							{
								CacheFont obj2 = cache_font[item2];
								Rectangle rect4 = obj2.rect;
								rect4.Offset(x2, 0);
								obj2.rect = rect4;
							}
						}
					}
					else if ((int)textalign == 2)
					{
						if (list3.Count > 0)
						{
							int x3 = (rect_d_l.Right - cache_font[list3[list3.Count - 1]].rect.Right) / 2;
							foreach (int item3 in list3)
							{
								CacheFont obj3 = cache_font[item3];
								Rectangle rect5 = obj3.rect;
								rect5.Offset(x3, 0);
								obj3.rect = rect5;
							}
						}
						if (list4.Count > 0)
						{
							int x4 = (rect_d_r.Right - cache_font[list4[list4.Count - 1]].rect.Right) / 2;
							foreach (int item4 in list4)
							{
								CacheFont obj4 = cache_font[item4];
								Rectangle rect6 = obj4.rect;
								rect6.Offset(x4, 0);
								obj4.rect = rect6;
							}
						}
					}
				}
				else
				{
					CacheFont[] array = cache_font;
					foreach (CacheFont cacheFont6 in array)
					{
						cacheFont6.show = true;
						cacheFont6.rect = new Rectangle(rect_text.X + num8, rect_text.Y, cacheFont6.width, CaretInfo.Height);
						num8 += cacheFont6.width;
					}
					if ((int)textalign == 1)
					{
						int num14 = -1;
						List<CacheFont> list2 = new List<CacheFont>();
						Action action = delegate
						{
							if (list2.Count > 0)
							{
								int x6 = rect_text.Right - list2[list2.Count - 1].rect.Right;
								foreach (CacheFont item5 in list2)
								{
									Rectangle rect8 = item5.rect;
									rect8.Offset(x6, 0);
									item5.rect = rect8;
								}
								list2.Clear();
							}
						};
						array = cache_font;
						foreach (CacheFont cacheFont7 in array)
						{
							if (cacheFont7.rect.Y != num14)
							{
								num14 = cacheFont7.rect.Y;
								action();
							}
							list2.Add(cacheFont7);
						}
						action();
					}
					else if ((int)textalign == 2)
					{
						int num15 = -1;
						List<CacheFont> list = new List<CacheFont>();
						Action action2 = delegate
						{
							if (list.Count > 0)
							{
								int x5 = (rect_text.Right - list[list.Count - 1].rect.Right) / 2;
								foreach (CacheFont item6 in list)
								{
									Rectangle rect7 = item6.rect;
									rect7.Offset(x5, 0);
									item6.rect = rect7;
								}
								list.Clear();
							}
						};
						array = cache_font;
						foreach (CacheFont cacheFont8 in array)
						{
							if (cacheFont8.rect.Y != num15)
							{
								num15 = cacheFont8.rect.Y;
								action2();
							}
							list.Add(cacheFont8);
						}
						action2();
					}
				}
			}
			CacheFont cacheFont9 = cache_font[cache_font.Length - 1];
			ScrollXMax = cacheFont9.rect.Right - rect_text.Right;
			HorizontalAlignment val = textalign;
			if ((int)val != 1)
			{
				if ((int)val == 2)
				{
					if (ScrollXMax > 0)
					{
						ScrollXMin = -ScrollXMax;
					}
					else
					{
						ScrollXMin = ScrollXMax;
						ScrollXMax = -ScrollXMax;
					}
				}
				else
				{
					ScrollXMin = 0;
				}
			}
			else
			{
				ScrollXMin = cache_font[0].rect.Right - rect.Right + num;
				ScrollXMax = 0;
			}
			ScrollYMax = cacheFont9.rect.Bottom - rect.Height + num;
			if (multiline)
			{
				ScrollX = 0;
				ScrollXShow = false;
				ScrollYShow = cacheFont9.rect.Bottom > rect.Bottom;
				if (ScrollYShow)
				{
					if (ScrollY > ScrollYMax)
					{
						ScrollY = ScrollYMax;
					}
				}
				else
				{
					ScrollY = 0;
				}
			}
			else
			{
				ScrollYShow = false;
				ScrollY = 0;
				if ((int)textalign == 1)
				{
					ScrollXShow = cacheFont9.rect.Right < rect.Right;
				}
				else
				{
					ScrollXShow = cacheFont9.rect.Right > rect_text.Right;
				}
				if (ScrollXShow)
				{
					if (ScrollX > ScrollXMax)
					{
						ScrollX = ScrollXMax;
					}
				}
				else
				{
					ScrollX = 0;
				}
			}
		}
		SetCaretPostion();
		int GetTabIndex()
		{
			CacheFont[] array2 = cache_font;
			foreach (CacheFont cacheFont10 in array2)
			{
				if (cacheFont10.text == "\t")
				{
					return cacheFont10.i;
				}
			}
			return -1;
		}
	}

	private void RectAuto(Rectangle rect, int sps, int sps2)
	{
		int read_height = CaretInfo.Height;
		string prefixText = PrefixText;
		string suffixText = SuffixText;
		bool has_prefixText = prefixText != null;
		bool has_suffixText = suffixText != null;
		bool has_prefix = HasPrefix;
		bool has_suffix = HasSuffix;
		if (is_clear)
		{
			int icon_size = (int)((float)read_height * iconratio);
			if (has_prefixText)
			{
				Helper.GDI(delegate(Canvas g)
				{
					RectLR(rect, read_height, sps, sps2, g.MeasureString(prefixText, ((Control)this).Font).Width, read_height, icon_size, icon_size);
				});
			}
			else if (has_prefix)
			{
				RectLR(rect, read_height, sps, sps2, icon_size, icon_size, icon_size, icon_size);
			}
			else
			{
				RectR(rect, read_height, sps, sps2, icon_size, icon_size);
			}
			return;
		}
		if (has_prefixText || has_suffixText || has_prefix || has_suffix)
		{
			if (has_prefixText || has_suffixText)
			{
				Helper.GDI(delegate(Canvas g)
				{
					if (has_prefixText && has_suffixText)
					{
						RectLR(rect, read_height, sps, sps2, g.MeasureString(prefixText, ((Control)this).Font).Width, read_height, g.MeasureString(suffixText, ((Control)this).Font).Width, read_height);
					}
					else if (has_prefix || has_suffix)
					{
						if (has_prefixText)
						{
							if (has_suffix)
							{
								int num6 = (int)((float)read_height * iconratio);
								RectLR(rect, read_height, sps, sps2, g.MeasureString(prefixText, ((Control)this).Font).Width, read_height, num6, num6);
							}
							else
							{
								RectL(rect, read_height, sps, sps2, g.MeasureString(prefixText, ((Control)this).Font).Width);
							}
						}
						else if (has_prefix)
						{
							int num7 = (int)((float)read_height * iconratio);
							RectLR(rect, read_height, sps, sps2, num7, num7, g.MeasureString(suffixText, ((Control)this).Font).Width, read_height);
						}
						else
						{
							RectR(rect, read_height, sps, sps2, g.MeasureString(suffixText, ((Control)this).Font).Width);
						}
					}
					else if (has_prefixText)
					{
						RectL(rect, read_height, sps, sps2, g.MeasureString(prefixText, ((Control)this).Font).Width);
					}
					else
					{
						RectR(rect, read_height, sps, sps2, g.MeasureString(suffixText, ((Control)this).Font).Width);
					}
				});
			}
			else if (has_prefix || has_suffix)
			{
				int num = (int)((float)read_height * iconratio);
				if (has_prefix && has_suffix)
				{
					RectLR(rect, read_height, sps, sps2, num, num, num, num);
				}
				else if (has_prefix)
				{
					RectL(rect, read_height, sps, sps2, num, num);
				}
				else
				{
					RectR(rect, read_height, sps, sps2, num, num);
				}
			}
			return;
		}
		int num2 = -1;
		if (HasLeft())
		{
			int[] array = UseLeft(rect, delgap: false);
			if (array[0] > 0 || array[1] > 0)
			{
				int num3 = array[0];
				int num4 = array[1];
				rect.X += num3;
				rect.Width -= num3;
				rect.Y += num4;
				rect.Height -= num4;
				num2 = num4;
			}
		}
		ref Rectangle reference = ref rect_l;
		int width = (rect_r.Width = 0);
		reference.Width = width;
		if (multiline)
		{
			rect_text = new Rectangle(rect.X + sps, rect.Y + sps, rect.Width - sps2, rect.Height - sps2);
		}
		else
		{
			rect_text = new Rectangle(rect.X + sps, rect.Y + (rect.Height - read_height) / 2, rect.Width - sps2, read_height);
		}
		if (num2 > -1)
		{
			UseLeftAutoHeight(read_height, sps2, num2);
		}
	}

	private void RectLR(Rectangle rect, int read_height, int sps, int sps2, int w_L, int h_L, int w_R, int h_R)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		int num3 = sps + w_L + num2;
		int num4 = w_L + w_R + (sps + num2) * 2;
		int[] array = (HasLeft() ? UseLeft(new Rectangle(rect.X + num3, rect.Y, rect.Width - num4, rect.Height), delgap: true) : new int[2]);
		if (multiline)
		{
			rect_l = new Rectangle(rect.X + sps, rect.Y + sps + (read_height - h_L) / 2, w_L, h_L);
			if (array[0] > 0 || array[1] > 0)
			{
				int num5 = array[0];
				int num6 = array[1];
				rect_text = new Rectangle(rect.X + num3 + num5, rect.Y + sps + num6, rect.Width - num4 - num5, rect.Height - sps2 - num6);
				num = num6;
			}
			else
			{
				rect_text = new Rectangle(rect.X + num3, rect.Y + sps, rect.Width - w_L - w_R - (sps + num2) * 2, rect.Height - sps2);
			}
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + sps + (read_height - h_R) / 2, w_R, h_R);
		}
		else
		{
			rect_l = new Rectangle(rect.X + sps, rect.Y + (rect.Height - h_L) / 2, w_L, h_L);
			if (array[0] > 0 || array[1] > 0)
			{
				int num7 = array[0];
				int num8 = array[1];
				rect_text = new Rectangle(rect.X + num3 + num7, rect.Y + (rect.Height - read_height) / 2 + num8, rect.Width - num4 - num7, read_height - num8);
				num = num8;
			}
			else
			{
				rect_text = new Rectangle(rect.X + num3, rect.Y + (rect.Height - read_height) / 2, rect.Width - num4, read_height);
			}
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + (rect.Height - h_R) / 2, w_R, h_R);
		}
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, sps2, num);
		}
	}

	private void RectL(Rectangle rect, int read_height, int sps, int sps2, int w)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		int num3 = sps + w + num2;
		int num4 = sps2 + w + num2;
		int[] array = (HasLeft() ? UseLeft(new Rectangle(rect.X + num3, rect.Y, rect.Width - num3, rect.Height), delgap: true) : new int[2]);
		if (multiline)
		{
			int num5 = rect.Y + sps;
			int num6 = rect.Height - sps2;
			rect_l = new Rectangle(rect.X + sps, num5, w, read_height);
			if (array[0] > 0 || array[1] > 0)
			{
				int num7 = array[0];
				int num8 = array[1];
				rect_text = new Rectangle(rect.X + num3 + num7, num5 + num8, rect.Width - num4 - num7, num6 - num8);
				num = num8;
			}
			else
			{
				rect_text = new Rectangle(rect.X + num3, num5, rect.Width - num4, num6);
			}
		}
		else
		{
			int num9 = rect.Y + (rect.Height - read_height) / 2;
			rect_l = new Rectangle(rect.X + sps, num9, w, read_height);
			if (array[0] > 0 || array[1] > 0)
			{
				int num10 = array[0];
				int num11 = array[1];
				rect_text = new Rectangle(rect.X + num3 + num10, num9 + num11, rect.Width - num4 - num10, read_height - num11);
				num = num11;
			}
			else
			{
				rect_text = new Rectangle(rect.X + num3, num9, rect.Width - num4, read_height);
			}
		}
		rect_r.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, sps2, num);
		}
	}

	private void RectL(Rectangle rect, int read_height, int sps, int sps2, int w, int h)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		int num3 = sps + w + num2;
		int num4 = sps2 + w + num2;
		int[] array = (HasLeft() ? UseLeft(new Rectangle(rect.X + num3, rect.Y, rect.Width - num3, rect.Height), delgap: true) : new int[2]);
		if (multiline)
		{
			rect_l = new Rectangle(rect.X + sps, rect.Y + sps + (read_height - h) / 2, w, h);
			if (array[0] > 0 || array[1] > 0)
			{
				int num5 = array[0];
				int num6 = array[1];
				rect_text = new Rectangle(rect.X + num3 + num5, rect.Y + sps + num6, rect.Width - num4 - num5, rect.Height - sps2 - num6);
				num = num6;
			}
			else
			{
				rect_text = new Rectangle(rect.X + num3, rect.Y + sps, rect.Width - num4, rect.Height - sps2);
			}
		}
		else
		{
			rect_l = new Rectangle(rect.X + sps, rect.Y + (rect.Height - h) / 2, w, h);
			if (array[0] > 0 || array[1] > 0)
			{
				int num7 = array[0];
				int num8 = array[1];
				rect_text = new Rectangle(rect.X + num3 + num7, rect.Y + (rect.Height - read_height) / 2 + num8, rect.Width - num4 - num7, read_height - num8);
				num = num8;
			}
			else
			{
				rect_text = new Rectangle(rect.X + num3, rect.Y + (rect.Height - read_height) / 2, rect.Width - num4, read_height);
			}
		}
		rect_r.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, sps2, num);
		}
	}

	private void RectR(Rectangle rect, int read_height, int sps, int sps2, int w)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		if (HasLeft())
		{
			int[] array = UseLeft(new Rectangle(rect.X, rect.Y, rect.Width - num2, rect.Height), delgap: false);
			if (array[0] > 0 || array[1] > 0)
			{
				int num3 = array[0];
				int num4 = array[1];
				rect.X += num3;
				rect.Width -= num3;
				rect.Y += num4;
				rect.Height -= num4;
				num = num4;
			}
		}
		if (multiline)
		{
			rect_text = new Rectangle(rect.X + sps, rect.Y + sps, rect.Width - sps2 - w - num2, rect.Height - sps2);
			rect_r = new Rectangle(rect_text.Right + num2, rect_text.Y, w, read_height);
		}
		else
		{
			rect_text = new Rectangle(rect.X + sps, rect.Y + (rect.Height - read_height) / 2, rect.Width - sps2 - w - num2, read_height);
			rect_r = new Rectangle(rect_text.Right + num2, rect_text.Y, w, rect_text.Height);
		}
		rect_l.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, sps2, num);
		}
	}

	private void RectR(Rectangle rect, int read_height, int sps, int sps2, int w, int h)
	{
		int num = -1;
		int num2 = (int)((float)read_height * icongap);
		if (HasLeft())
		{
			int[] array = UseLeft(new Rectangle(rect.X, rect.Y, rect.Width - num2, rect.Height), delgap: false);
			if (array[0] > 0 || array[1] > 0)
			{
				int num3 = array[0];
				int num4 = array[1];
				rect.X += num3;
				rect.Width -= num3;
				rect.Y += num4;
				rect.Height -= num4;
				num = num4;
			}
		}
		if (multiline)
		{
			rect_text = new Rectangle(rect.X + sps, rect.Y + sps, rect.Width - sps2 - w - num2, rect.Height - sps2);
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + sps + (read_height - h) / 2, w, h);
		}
		else
		{
			rect_text = new Rectangle(rect.X + sps, rect.Y + (rect.Height - read_height) / 2, rect.Width - sps2 - w - num2, read_height);
			rect_r = new Rectangle(rect_text.Right + num2, rect.Y + (rect.Height - h) / 2, w, h);
		}
		rect_l.Width = 0;
		if (num > -1)
		{
			UseLeftAutoHeight(read_height, sps2, num);
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		CalculateRect();
		((Control)this).OnSizeChanged(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).OnMouseDown(e);
		((Control)this).Focus();
		((Control)this).Select();
		is_prefix_down = (is_suffix_down = false);
		if ((int)e.Button != 1048576)
		{
			return;
		}
		if (cache_font != null && e.Clicks > 1 && !BanInput)
		{
			mDownMove = (mDown = false);
			int caretPostion = GetCaretPostion(e.X + scrollx, e.Y + scrolly);
			int num = 0;
			if (caretPostion > 0)
			{
				num = FindStart(cache_font, caretPostion - 2);
			}
			int num2 = ((caretPostion < cache_font.Length) ? FindEnd(cache_font, caretPostion) : cache_font.Length);
			SetSelectionStart(num);
			SelectionLength = num2 - num;
		}
		else if (is_clear && rect_r.Contains(e.Location))
		{
			is_clear_down = true;
		}
		else if (HasPrefix && rect_l.Contains(e.Location) && this.PrefixClick != null)
		{
			is_prefix_down = true;
		}
		else if (HasSuffix && rect_r.Contains(e.Location) && this.SuffixClick != null)
		{
			is_suffix_down = true;
		}
		else
		{
			if (IMouseDown(e.Location))
			{
				return;
			}
			if (ScrollYShow && autoscroll && ScrollHover)
			{
				float num3 = ((float)e.Y - ScrollSlider.Height / 2f) / (float)ScrollRect.Height;
				float num4 = ScrollYMax + ScrollRect.Height;
				ScrollY = (int)(num3 * num4);
				ScrollYDown = true;
				return;
			}
			mDownMove = false;
			mDownLocation = e.Location;
			if (BanInput)
			{
				return;
			}
			int caretPostion2 = GetCaretPostion(e.X + scrollx, e.Y + scrolly);
			if (((Enum)Control.ModifierKeys).HasFlag((Enum)(object)(Keys)65536))
			{
				if (caretPostion2 > selectionStartTemp)
				{
					if (selectionStart != selectionStartTemp)
					{
						SetSelectionStart(selectionStartTemp);
					}
					SelectionLength = caretPostion2 - selectionStartTemp;
				}
				else
				{
					int num5 = selectionStartTemp - caretPostion2;
					SetSelectionStart(caretPostion2);
					SelectionLength = num5;
				}
			}
			else
			{
				SetSelectionStart(caretPostion2);
				SelectionLength = 0;
				SetCaretPostion(selectionStart);
			}
			if (cache_font != null)
			{
				mDown = true;
			}
			else if (ModeRange)
			{
				SetCaretPostion();
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (ScrollYDown)
		{
			float num = ((float)e.Y - ScrollSlider.Height / 2f) / (float)ScrollRect.Height;
			float num2 = ScrollYMax + ScrollRect.Height;
			ScrollY = (int)(num * num2);
			return;
		}
		if (mDown && cache_font != null)
		{
			mDownMove = true;
			SetCursor(CursorType.IBeam);
			int caretPostion = GetCaretPostion(mDownLocation.X + scrollx + (e.X - mDownLocation.X), mDownLocation.Y + scrolly + (e.Y - mDownLocation.Y));
			SelectionLength = Math.Abs(caretPostion - selectionStart);
			if (caretPostion > selectionStart)
			{
				selectionStartTemp = selectionStart;
			}
			else
			{
				selectionStartTemp = caretPostion;
			}
			SetCaretPostion(caretPostion);
			return;
		}
		if (ScrollYShow && autoscroll)
		{
			ScrollHover = ScrollRect.Contains(e.Location);
		}
		if (is_clear)
		{
			bool flag = rect_r.Contains(e.Location);
			if (hover_clear != flag)
			{
				hover_clear = flag;
				Invalidate();
			}
			if (flag)
			{
				SetCursor(val: true);
				return;
			}
		}
		if ((HasPrefix && rect_l.Contains(e.Location) && this.PrefixClick != null) || (HasSuffix && rect_r.Contains(e.Location) && this.SuffixClick != null))
		{
			SetCursor(val: true);
		}
		else if (IMouseMove(e.Location))
		{
			SetCursor(val: true);
		}
		else if (CaretInfo.ReadShow)
		{
			if (rect_text.Contains(e.Location))
			{
				SetCursor(val: true);
			}
			else
			{
				SetCursor(val: false);
			}
		}
		else if (rect_text.Contains(e.Location))
		{
			SetCursor(CursorType.IBeam);
		}
		else
		{
			SetCursor(val: false);
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (ScrollYShow && autoscroll && e.Delta != 0)
		{
			ScrollY -= e.Delta;
		}
		base.OnMouseWheel(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		bool flag = mDown;
		mDown = false;
		ScrollYDown = false;
		if (is_clear_down)
		{
			if (rect_r.Contains(e.Location))
			{
				OnClearValue();
			}
			is_clear_down = false;
		}
		else if (is_prefix_down && this.PrefixClick != null)
		{
			this.PrefixClick.Invoke((object)this, e);
			is_prefix_down = (is_suffix_down = false);
		}
		else if (is_suffix_down && this.SuffixClick != null)
		{
			this.SuffixClick.Invoke((object)this, e);
			is_prefix_down = (is_suffix_down = false);
		}
		else
		{
			if (IMouseUp(e.Location))
			{
				return;
			}
			if (flag && mDownMove && mDownLocation != e.Location && cache_font != null)
			{
				int caretPostion = GetCaretPostion(e.X + scrollx, e.Y + scrolly);
				if (selectionStart == caretPostion)
				{
					SelectionLength = 0;
					return;
				}
				if (caretPostion > selectionStart)
				{
					SelectionLength = Math.Abs(caretPostion - selectionStart);
					SetSelectionStart(selectionStart);
					return;
				}
				int scrollX = scrollx;
				SelectionLength = Math.Abs(caretPostion - selectionStart);
				SetSelectionStart(caretPostion);
				ScrollX = scrollX;
			}
			else
			{
				OnClickContent();
			}
		}
	}

	private int FindStart(CacheFont[] cache_font, int index)
	{
		for (int num = index; num >= 0; num--)
		{
			if (sptext.Contains(cache_font[num].text))
			{
				return num + 1;
			}
		}
		return 0;
	}

	private int FindEnd(CacheFont[] cache_font, int index)
	{
		int num = cache_font.Length;
		for (int i = index; i < num; i++)
		{
			if (sptext.Contains(cache_font[i].text))
			{
				return i;
			}
		}
		return num;
	}

	private int FindStartY(CacheFont[] cache_font, int index)
	{
		int line = cache_font[index].line;
		int result = 0;
		int num = index;
		while (num >= 0)
		{
			if (cache_font[num].line == line)
			{
				result = num;
				num--;
				continue;
			}
			return result;
		}
		return 0;
	}

	private int FindEndY(CacheFont[] cache_font, int index)
	{
		int line = cache_font[index].line;
		int num = 0;
		int num2 = cache_font.Length;
		for (int i = index + 1; i < num2; i++)
		{
			if (cache_font[i].line == line)
			{
				num = i;
				continue;
			}
			if (num != 0)
			{
				return num;
			}
			return index;
		}
		return num2;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		ExtraMouseHover = true;
		((Control)this).OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		ExtraMouseHover = false;
		((Control)this).OnMouseLeave(e);
	}

	public void Invalidate()
	{
		if (TakePaint == null)
		{
			((Control)this).Invalidate();
		}
		else
		{
			TakePaint();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width > 0 && clientRectangle.Height > 0)
		{
			Canvas g = e.Graphics.High();
			Rectangle rect = clientRectangle.PaddingRect(((Control)this).Padding);
			Rectangle rect_read = rect.ReadRect(((float)WaveSize + borderWidth / 2f) * Config.Dpi, JoinLeft, JoinRight);
			IPaint(g, rect, rect_read);
			this.PaintBadge(g);
			((Control)this).OnPaint(e);
		}
	}

	internal void IPaint(Canvas g, Rectangle rect, Rectangle rect_read)
	{
		float num = (round ? ((float)rect_read.Height) : ((float)radius * Config.Dpi));
		if (backImage != null)
		{
			g.Image(rect_read, backImage, backFit, num, round: false);
		}
		GraphicsPath val = Path(rect_read, num);
		try
		{
			Color def = back ?? Colour.BgContainer.Get("Input");
			Color color = fore ?? Colour.Text.Get("Input");
			Color color2 = borderColor ?? Colour.BorderColor.Get("Input");
			Color color3 = BorderHover ?? Colour.PrimaryHover.Get("Input");
			Color color4 = BorderActive ?? Colour.Primary.Get("Input");
			switch (status)
			{
			case TType.Success:
				color2 = Colour.SuccessBorder.Get("Input");
				color3 = Colour.SuccessHover.Get("Input");
				color4 = Colour.Success.Get("Input");
				break;
			case TType.Error:
				color2 = Colour.ErrorBorder.Get("Input");
				color3 = Colour.ErrorHover.Get("Input");
				color4 = Colour.Error.Get("Input");
				break;
			case TType.Warn:
				color2 = Colour.WarningBorder.Get("Input");
				color3 = Colour.WarningHover.Get("Input");
				color4 = Colour.Warning.Get("Input");
				break;
			}
			PaintClick(g, val, rect, color4, num);
			if (base.Enabled)
			{
				Brush val2 = backExtend.BrushEx(rect_read, def);
				try
				{
					g.Fill(val2, val);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				PaintIcon(g, color);
				PaintText(g, color, rect_read.Right, rect_read.Bottom);
				PaintOtherBor(g, rect_read, num, def, color2, color4);
				PaintScroll(g, rect_read, num);
				if (borderWidth > 0f)
				{
					float width = borderWidth * Config.Dpi;
					if (AnimationHover)
					{
						g.Draw(color2.BlendColors(AnimationHoverValue, color3), width, val);
					}
					else if (ExtraMouseDown)
					{
						g.Draw(color4, width, val);
					}
					else if (ExtraMouseHover)
					{
						g.Draw(color3, width, val);
					}
					else
					{
						g.Draw(color2, width, val);
					}
				}
			}
			else
			{
				g.Fill(Colour.FillTertiary.Get("Input"), val);
				PaintIcon(g, Colour.TextQuaternary.Get("Input"));
				PaintText(g, Colour.TextQuaternary.Get("Input"), rect_read.Right, rect_read.Bottom);
				PaintOtherBor(g, rect_read, num, def, color2, color4);
				PaintScroll(g, rect_read, num);
				if (borderWidth > 0f)
				{
					g.Draw(color2, borderWidth * Config.Dpi, val);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintScroll(Canvas g, Rectangle rect_read, float _radius)
	{
		if (!ScrollYShow || !autoscroll)
		{
			return;
		}
		int num = 20;
		ScrollRect = new Rectangle(rect_read.Right - num, rect_read.Y, num, rect_read.Height);
		if (IsPaintScroll())
		{
			Color color = Color.FromArgb(10, Colour.TextBase.Get("Input"));
			if (JoinRight)
			{
				g.Fill(color, ScrollRect);
			}
			else
			{
				GraphicsPath val = ScrollRect.RoundPath(_radius, TL: false, TR: true, BR: true, BL: false);
				try
				{
					g.Fill(color, val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		float num2 = scrolly;
		float num3 = ScrollYMax + ScrollRect.Height;
		float num4 = (float)ScrollRect.Height / num3 * (float)ScrollRect.Height - 20f;
		if (num4 < (float)num)
		{
			num4 = num;
		}
		float num5 = ((num2 == 0f) ? 0f : (num2 / (num3 - (float)ScrollRect.Height) * ((float)ScrollRect.Height - num4)));
		if (ScrollHover)
		{
			ScrollSlider = new RectangleF(ScrollRect.X + 6, (float)ScrollRect.Y + num5, 8f, num4);
		}
		else
		{
			ScrollSlider = new RectangleF(ScrollRect.X + 7, (float)ScrollRect.Y + num5, 6f, num4);
		}
		if (ScrollSlider.Y < 10f)
		{
			ScrollSlider.Y = 10f;
		}
		else if (ScrollSlider.Y > (float)ScrollRect.Height - num4 - 6f)
		{
			ScrollSlider.Y = (float)ScrollRect.Height - num4 - 6f;
		}
		GraphicsPath val2 = ScrollSlider.RoundPath(ScrollSlider.Width);
		try
		{
			g.Fill(Color.FromArgb(141, Colour.TextBase.Get("Input")), val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private bool IsPaintScroll()
	{
		if (Config.ScrollBarHide)
		{
			return ExtraMouseHover;
		}
		return true;
	}

	private void PaintIcon(Canvas g, Color _fore)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		string text = PrefixText;
		string text2 = SuffixText;
		if (text != null)
		{
			SolidBrush val = new SolidBrush(prefixFore.GetValueOrDefault(_fore));
			try
			{
				g.String(text, ((Control)this).Font, (Brush)(object)val, rect_l, sf_center);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else if (prefixSvg != null)
		{
			g.GetImgExtend(prefixSvg, rect_l, prefixFore ?? fore ?? Colour.Text.Get("Input"));
		}
		else if (prefix != null)
		{
			g.Image(prefix, rect_l);
		}
		if (is_clear)
		{
			g.GetImgExtend(SvgDb.IcoError, rect_r, hover_clear ? Colour.TextTertiary.Get("Input") : Colour.TextQuaternary.Get("Input"));
			return;
		}
		if (text2 != null)
		{
			SolidBrush val2 = new SolidBrush(suffixFore.GetValueOrDefault(_fore));
			try
			{
				g.String(text2, ((Control)this).Font, (Brush)(object)val2, rect_r, sf_center);
				return;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (suffixSvg != null)
		{
			g.GetImgExtend(suffixSvg, rect_r, suffixFore ?? fore ?? Colour.Text.Get("Input"));
		}
		else if (suffix != null)
		{
			g.Image(suffix, rect_r);
		}
		else
		{
			PaintRIcon(g, rect_r);
		}
	}

	private void PaintText(Canvas g, Color _fore, int w, int h)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Expected O, but got Unknown
		if (multiline)
		{
			g.SetClip(rect_text);
		}
		else if (RECTDIV.HasValue)
		{
			g.SetClip(RECTDIV.Value);
		}
		else
		{
			g.SetClip(new Rectangle(rect_text.X, 0, rect_text.Width, ((Control)this).Height));
		}
		if (cache_font != null)
		{
			g.TranslateTransform(-ScrollX, -ScrollY);
			PaintTextSelected(g, cache_font);
			SolidBrush val = new SolidBrush(_fore);
			try
			{
				if (HasEmoji)
				{
					Font val2 = new Font(EmojiFont, ((Control)this).Font.Size);
					try
					{
						CacheFont[] array = cache_font;
						foreach (CacheFont cacheFont in array)
						{
							cacheFont.show = cacheFont.rect.Y > ScrollY - cacheFont.rect.Height && cacheFont.rect.Bottom < ScrollY + h + cacheFont.rect.Height;
							if (cacheFont.show)
							{
								if (IsPassWord)
								{
									g.String(PassWordChar, ((Control)this).Font, (Brush)(object)val, cacheFont.rect, sf_font);
								}
								else if (cacheFont.emoji)
								{
									g.String(cacheFont.text, val2, (Brush)(object)val, cacheFont.rect, sf_font);
								}
								else
								{
									g.String(cacheFont.text, ((Control)this).Font, (Brush)(object)val, cacheFont.rect, sf_font);
								}
							}
						}
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				else
				{
					CacheFont[] array = cache_font;
					foreach (CacheFont cacheFont2 in array)
					{
						cacheFont2.show = cacheFont2.rect.Y > ScrollY - cacheFont2.rect.Height && cacheFont2.rect.Bottom < ScrollY + h + cacheFont2.rect.Height;
						if (cacheFont2.show && !cacheFont2.ret)
						{
							if (IsPassWord)
							{
								g.String(PassWordChar, ((Control)this).Font, (Brush)(object)val, cacheFont2.rect, sf_font);
							}
							else
							{
								g.String(cacheFont2.text, ((Control)this).Font, (Brush)(object)val, cacheFont2.rect, sf_font);
							}
						}
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			g.ResetTransform();
		}
		else if (PlaceholderText != null && ShowPlaceholder)
		{
			Brush val3 = placeholderColorExtend.BrushEx(rect_text, placeholderColor ?? Colour.TextQuaternary.Get("Input"));
			try
			{
				g.String(PlaceholderText, ((Control)this).Font, val3, rect_text, sf_placeholder);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		g.ResetClip();
		if (CaretInfo.Show && CaretInfo.Flag)
		{
			g.TranslateTransform(-ScrollX, -ScrollY);
			SolidBrush val4 = new SolidBrush(CaretColor.GetValueOrDefault(_fore));
			try
			{
				g.Fill((Brush)(object)val4, CaretInfo.Rect);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			g.ResetTransform();
		}
	}

	private void PaintTextSelected(Canvas g, CacheFont[] cache_font)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if (selectionLength <= 0 || cache_font.Length <= selectionStartTemp || BanInput)
		{
			return;
		}
		try
		{
			int num = selectionStartTemp;
			int num2 = num + selectionLength - 1;
			if (num2 > cache_font.Length - 1)
			{
				num2 = cache_font.Length - 1;
			}
			_ = cache_font[num];
			SolidBrush val = new SolidBrush(selection);
			try
			{
				Dictionary<int, CacheFont> dictionary = new Dictionary<int, CacheFont>(6);
				for (int i = num; i <= num2; i++)
				{
					CacheFont cacheFont = cache_font[i];
					if (cacheFont.ret)
					{
						dictionary.Add(cacheFont.line + 1, cacheFont);
						continue;
					}
					if (dictionary.ContainsKey(cacheFont.line))
					{
						dictionary.Remove(cacheFont.line);
					}
					g.Fill((Brush)(object)val, cacheFont.rect);
				}
				foreach (KeyValuePair<int, CacheFont> item in dictionary)
				{
					g.Fill((Brush)(object)val, item.Value.rect.X, item.Value.rect.Y, item.Value.width, item.Value.rect.Height);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
	}

	protected virtual void PaintRIcon(Canvas g, Rectangle rect)
	{
	}

	protected virtual void PaintOtherBor(Canvas g, RectangleF rect_read, float radius, Color back, Color borderColor, Color borderActive)
	{
	}

	internal void PaintClick(Canvas g, GraphicsPath path, Rectangle rect, Color color, float radius)
	{
		if (AnimationFocus)
		{
			if (AnimationFocusValue > 0)
			{
				GraphicsPath val = rect.RoundPath(radius, round);
				try
				{
					val.AddPath(path, false);
					g.Fill(Helper.ToColor(AnimationFocusValue, color), val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		else if (ExtraMouseDown && WaveSize > 0)
		{
			GraphicsPath val2 = rect.RoundPath(radius, round);
			try
			{
				val2.AddPath(path, false);
				g.Fill(Color.FromArgb(30, color), val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	internal GraphicsPath Path(RectangleF rect_read, float _radius)
	{
		if (JoinLeft && JoinRight)
		{
			return rect_read.RoundPath(0f);
		}
		if (JoinRight)
		{
			return rect_read.RoundPath(_radius, TL: true, TR: false, BR: false, BL: true);
		}
		if (JoinLeft)
		{
			return rect_read.RoundPath(_radius, TL: false, TR: true, BR: true, BL: false);
		}
		return rect_read.RoundPath(_radius);
	}

	private void ScrollIFTo(Rectangle r)
	{
		if (SpeedScrollTo)
		{
			if (ScrollYShow)
			{
				int num = CaretInfo.Y - scrolly;
				if (num < rect_text.Y)
				{
					ScrollY = r.Y;
				}
				else if (num + CaretInfo.Height > rect_text.Height)
				{
					ScrollY = r.Bottom;
				}
			}
			else if (ScrollXShow)
			{
				int num2 = CaretInfo.X - scrollx;
				if (num2 < rect_text.X)
				{
					ScrollX = r.X;
				}
				else if (num2 + CaretInfo.Width > rect_text.Width)
				{
					ScrollX = r.Right;
				}
			}
			else
			{
				int scrollX = (ScrollY = 0);
				ScrollX = scrollX;
			}
		}
		ITask.Run(delegate
		{
			ScrollTo(r);
		});
	}

	private void ScrollTo(Rectangle r)
	{
		if (ScrollYShow)
		{
			int height = CaretInfo.Height;
			int num = 0;
			List<int> list = new List<int>(2);
			while (true)
			{
				int num2 = CaretInfo.Y - scrolly;
				list.Add(num2);
				if (list.Count > 1)
				{
					if (list.Contains(num2))
					{
						break;
					}
					list.Clear();
				}
				if (num2 < rect_text.Y)
				{
					int num3 = (ScrollY -= height);
					if (ScrollY != num3)
					{
						break;
					}
					num++;
					if (num < 4)
					{
						Thread.Sleep(50);
					}
					continue;
				}
				if (num2 + CaretInfo.Height <= rect_text.Height)
				{
					break;
				}
				int num4 = (ScrollY += height);
				if (ScrollY != num4)
				{
					break;
				}
				num++;
				if (num < 4)
				{
					Thread.Sleep(50);
				}
			}
		}
		else if (ScrollXShow)
		{
			int width = r.Width;
			int num5 = 0;
			List<int> list2 = new List<int>(2);
			while (true)
			{
				int num6 = CaretInfo.X - scrollx;
				list2.Add(num6);
				if (list2.Count > 1)
				{
					if (list2.Contains(num6))
					{
						break;
					}
					list2.Clear();
				}
				if (num6 < rect_text.X)
				{
					int num7 = (ScrollX -= width);
					if (ScrollX != num7)
					{
						break;
					}
					num5++;
					if (num5 < 5)
					{
						Thread.Sleep(50);
					}
					continue;
				}
				if (num6 + CaretInfo.Width <= rect_text.Width)
				{
					break;
				}
				int num8 = (ScrollX += width);
				if (ScrollX != num8)
				{
					break;
				}
				num5++;
				if (num5 < 5)
				{
					Thread.Sleep(50);
				}
			}
		}
		else
		{
			int scrollX = (ScrollY = 0);
			ScrollX = scrollX;
		}
	}
}
