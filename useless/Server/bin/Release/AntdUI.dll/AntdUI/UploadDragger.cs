using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("UploadDragger 拖拽上传")]
[ToolboxItem(true)]
[DefaultProperty("Text")]
[Designer(typeof(IControlDesigner))]
public class UploadDragger : IControl
{
	[Flags]
	public enum FilterType
	{
		ALL = 1,
		Img = 2,
		Imgs = 3,
		Video = 4
	}

	private int radius = 8;

	private string? text;

	private string? textDesc;

	private Color? fore;

	private string? filter;

	private float iconratio = 1.92f;

	private Image? icon;

	private string? iconSvg = "InboxOutlined";

	private Color? back;

	private Image? backImage;

	private TFit backFit;

	private float borderWidth = 1f;

	private Color? borderColor;

	private DashStyle borderStyle = (DashStyle)1;

	private readonly StringFormat s_f = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)1);

	private int AnimationHoverValue;

	private bool AnimationHover;

	private bool _mouseHover;

	private ITask? ThreadHover;

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(8)]
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
				((Control)this).Invalidate();
				((Control)this).OnTextChanged(EventArgs.Empty);
				OnPropertyChanged("Text");
			}
		}
	}

	[Description("文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationText { get; set; }

	[Description("文本描述")]
	[Category("外观")]
	[DefaultValue(null)]
	[Localizable(true)]
	public string? TextDesc
	{
		get
		{
			return textDesc;
		}
		set
		{
			if (!(textDesc == value))
			{
				textDesc = value;
				((Control)this).Invalidate();
				OnPropertyChanged("TextDesc");
			}
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

	[Description("点击上传")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool ClickHand { get; set; } = true;


	[Description("多个文件")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool Multiselect { get; set; } = true;


	[Description("文件名筛选器字符串")]
	[Category("行为")]
	[DefaultValue(null)]
	public string? Filter
	{
		get
		{
			return filter;
		}
		set
		{
			if (filter == value)
			{
				return;
			}
			filter = value;
			if (value == null)
			{
				ONDRAG = null;
				return;
			}
			ONDRAG = delegate(string[] files)
			{
				if (!Multiselect && files.Length > 1)
				{
					files = new string[1] { files[0] };
				}
				if (filter == null)
				{
					return files;
				}
				string[] array = filter.Split(new char[1] { '|' });
				if (array.Length > 1)
				{
					List<string> list = new List<string>(files.Length);
					string[] array2 = files;
					foreach (string text in array2)
					{
						string extension = Path.GetExtension(text);
						if (HandFilter(extension, array))
						{
							list.Add(text);
						}
					}
					if (list.Count > 0)
					{
						return list.ToArray();
					}
				}
				return (string[]?)null;
			};
		}
	}

	[Description("图标比例")]
	[Category("外观")]
	[DefaultValue(1.92f)]
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
	[DefaultValue("InboxOutlined")]
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
				((Control)this).Invalidate();
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
				if (borderWidth > 0f)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BorderColor");
			}
		}
	}

	[Description("边框样式")]
	[Category("边框")]
	[DefaultValue(/*Could not decode attribute arguments.*/)]
	public DashStyle BorderStyle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return borderStyle;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (borderStyle != value)
			{
				borderStyle = value;
				if (borderWidth > 0f)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("BorderStyle");
			}
		}
	}

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Padding, borderWidth / 2f * Config.Dpi);

	public override Rectangle ReadRectangle => ((Control)this).DisplayRectangle;

	public override GraphicsPath RenderRegion => ((Control)this).DisplayRectangle.RoundPath((float)radius * Config.Dpi);

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
			if (!base.Enabled)
			{
				return;
			}
			if (Config.Animation)
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
						AnimationHoverValue -= 20;
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
				AnimationHoverValue = 255;
			}
			((Control)this).Invalidate();
		}
	}

	public UploadDragger()
		: base(ControlType.Select)
	{
	}//IL_0038: Unknown result type (might be due to invalid IL or missing references)


	private bool HandFilter(string fileExtension, string[] filters)
	{
		string fileExtension2 = fileExtension;
		for (int i = 1; i < filters.Length; i += 2)
		{
			if (filters[i] == "*.*")
			{
				return true;
			}
			if (Array.Exists(filters[i].Split(new char[1] { ';' }), (string ext) => ext.Equals("*" + fileExtension2, StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Expected O, but got Unknown
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Expected O, but got Unknown
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Expected O, but got Unknown
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Expected O, but got Unknown
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		Rectangle displayRectangle = ((Control)this).DisplayRectangle;
		Canvas canvas = e.Graphics.High();
		float num = (float)radius * Config.Dpi;
		GraphicsPath val = displayRectangle.RoundPath(num);
		try
		{
			canvas.Fill(back ?? Colour.FillQuaternary.Get("UploadDragger"), val);
			if (backImage != null)
			{
				canvas.Image(displayRectangle, backImage, backFit, num, round: false);
			}
			Size size = canvas.MeasureString("龍Qq", ((Control)this).Font);
			int num2 = (int)(4f * Config.Dpi);
			int num3 = (int)(16f * Config.Dpi);
			int num4 = num3 * 2;
			int num5 = (int)((float)size.Height * iconratio);
			if (string.IsNullOrWhiteSpace(iconSvg) && icon == null)
			{
				if (string.IsNullOrWhiteSpace(TextDesc))
				{
					int y = displayRectangle.Y + (displayRectangle.Height - size.Height) / 2;
					Rectangle rect = new Rectangle(displayRectangle.X + num3, y, displayRectangle.Width - num4, size.Height);
					SolidBrush val2 = new SolidBrush(fore ?? Colour.Text.Get("UploadDragger"));
					try
					{
						canvas.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val2, rect, s_f);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				else
				{
					Font val3 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.875f);
					try
					{
						Size size2 = canvas.MeasureString(TextDesc, val3, displayRectangle.Width - num4);
						int num6 = num2 + size.Height + size2.Height;
						int y2 = displayRectangle.Y + (displayRectangle.Height - num6) / 2;
						Rectangle rect2 = new Rectangle(displayRectangle.X + num3, y2, displayRectangle.Width - num4, size.Height);
						Rectangle rect3 = new Rectangle(rect2.X, rect2.Bottom + num2, rect2.Width, size2.Height);
						SolidBrush val4 = new SolidBrush(fore ?? Colour.Text.Get("UploadDragger"));
						try
						{
							canvas.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val4, rect2, s_f);
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
						SolidBrush val5 = new SolidBrush(Colour.TextTertiary.Get("UploadDragger"));
						try
						{
							canvas.String(TextDesc, val3, (Brush)(object)val5, rect3, s_f);
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
			}
			else if (string.IsNullOrWhiteSpace(TextDesc))
			{
				int num7 = num3 + num5 + size.Height;
				int num8 = displayRectangle.Y + (displayRectangle.Height - num7) / 2;
				Rectangle rect4 = new Rectangle(displayRectangle.X + (displayRectangle.Width - num5) / 2, num8, num5, num5);
				Rectangle rect5 = new Rectangle(displayRectangle.X + num3, num8 + num5 + num3, displayRectangle.Width - num4, size.Height);
				if (iconSvg != null)
				{
					canvas.GetImgExtend(iconSvg, rect4, Colour.Primary.Get("UploadDragger"));
				}
				if (icon != null)
				{
					canvas.Image(icon, rect4);
				}
				SolidBrush val6 = new SolidBrush(fore ?? Colour.Text.Get("UploadDragger"));
				try
				{
					canvas.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val6, rect5, s_f);
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
				}
			}
			else
			{
				Font val7 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 0.875f);
				try
				{
					Size size3 = canvas.MeasureString(TextDesc, val7, displayRectangle.Width - num4);
					int num9 = num2 + num3 + num5 + size.Height + size3.Height;
					int num10 = displayRectangle.Y + (displayRectangle.Height - num9) / 2;
					Rectangle rect6 = new Rectangle(displayRectangle.X + (displayRectangle.Width - num5) / 2, num10, num5, num5);
					Rectangle rect7 = new Rectangle(displayRectangle.X + num3, num10 + num5 + num3, displayRectangle.Width - num4, size.Height);
					Rectangle rect8 = new Rectangle(rect7.X, rect7.Bottom + num2, rect7.Width, size3.Height);
					if (iconSvg != null)
					{
						canvas.GetImgExtend(iconSvg, rect6, Colour.Primary.Get("UploadDragger"));
					}
					if (icon != null)
					{
						canvas.Image(icon, rect6);
					}
					SolidBrush val8 = new SolidBrush(fore ?? Colour.Text.Get("UploadDragger"));
					try
					{
						canvas.String(((Control)this).Text, ((Control)this).Font, (Brush)(object)val8, rect7, s_f);
					}
					finally
					{
						((IDisposable)val8)?.Dispose();
					}
					SolidBrush val9 = new SolidBrush(Colour.TextTertiary.Get("UploadDragger"));
					try
					{
						canvas.String(TextDesc, val7, (Brush)(object)val9, rect8, s_f);
					}
					finally
					{
						((IDisposable)val9)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			if (borderWidth > 0f)
			{
				float width = borderWidth * Config.Dpi;
				if (AnimationHover)
				{
					canvas.Draw((borderColor ?? Colour.BorderColor.Get("UploadDragger")).BlendColors(AnimationHoverValue, Colour.PrimaryHover.Get("UploadDragger")), width, val);
				}
				else if (ExtraMouseHover)
				{
					canvas.Draw(Colour.PrimaryHover.Get("UploadDragger"), width, borderStyle, val);
				}
				else
				{
					canvas.Draw(borderColor ?? Colour.BorderColor.Get("UploadDragger"), width, borderStyle, val);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	protected override void OnDragEnter()
	{
		ExtraMouseHover = true;
	}

	protected override void OnDragLeave()
	{
		ExtraMouseHover = false;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).AllowDrop = true;
		((Control)this).OnHandleCreated(e);
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

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		((Control)this).OnMouseClick(e);
		if (ClickHand && (int)e.Button == 1048576)
		{
			ManualSelection();
		}
	}

	public void ManualSelection()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		OpenFileDialog val = new OpenFileDialog
		{
			Multiselect = Multiselect,
			Filter = (Filter ?? (Localization.Get("All Files", "所有文件") + "|*.*"))
		};
		try
		{
			if ((int)((CommonDialog)val).ShowDialog() == 1)
			{
				OnDragChanged(((FileDialog)val).FileNames);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SetFilter(FilterType filterType)
	{
		bool flag = filterType.HasFlag(FilterType.ALL);
		bool flag2 = filterType.HasFlag(FilterType.Video);
		bool flag3 = filterType.HasFlag(FilterType.Imgs);
		bool flag4 = filterType.HasFlag(FilterType.Img);
		if (flag2 || flag3 || flag4)
		{
			List<string> list = new List<string>(2);
			if (flag2)
			{
				list.Add(Localization.Get("Video Files", "视频文件") + "|*.mp4;*.avi;*.rm;*.rmvb;*.flv;*.xr;*.mpg;*.vcd;*.svcd;*.dvd;*.vob;*.asf;*.wmv;*.mov;*.qt;*.3gp;*.sdp;*.yuv;*.mkv;*.dat;*.torrent;*.mp3;*.3g2;*.3gp2;*.3gpp;*.aac;*.ac3;*.aif;*.aifc;*.aiff;*.amr;*.amv;*.ape;*.asp;*.bik;*.csf;*.divx;*.evo;*.f4v;*.hlv;*.ifo;*.ivm;*.m1v;*.m2p;*.m2t;*.m2ts;*.m2v;*.m4b;*.m4p;*.m4v;*.mag;*.mid;*.mod;*.movie;*.mp2v;*.mp2;*.mpa;*.mpeg;*.mpeg4;*.mpv2;*.mts;*.ogg;*.ogm;*.pmp;*.pss;*.pva;*.qt;*.ram;*.rp;*.rpm;*.rt;*.scm;*.smi;*.smil;*.svx;*.swf;*.tga;*.tod;*.tp;*.tpr;*.ts;*.voc;*.vp6;*.wav;*.webm;*.wma;*.wm;*.wmp;*.xlmv;*.xv;*.xvid");
			}
			if (flag3)
			{
				list.Add(Localization.Get("Picture Files", "图片文件") + "|*.png;*.gif;*.jpg;*.jpeg;*.bmp");
			}
			if (flag4)
			{
				list.Add(Localization.Get("Picture Files", "图片文件") + "|*.jpg;*.jpeg;*.png;*.bmp");
			}
			if (flag)
			{
				list.Add(Localization.Get("All Files", "所有文件") + "|*.*");
			}
			Filter = string.Join("|", list);
		}
		else
		{
			Filter = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		ThreadHover?.Dispose();
		base.Dispose(disposing);
	}
}
