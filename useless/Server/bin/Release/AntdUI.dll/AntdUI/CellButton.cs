using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public class CellButton : CellLink
{
	private Color? fore;

	private Color? back;

	private string? backExtend;

	private Color? defaultback;

	private Color? defaultbordercolor;

	internal float borderWidth;

	private float iconratio = 0.7f;

	private float icongap = 0.25f;

	private Image? icon;

	private string? iconSvg;

	private TAlignMini iconPosition = TAlignMini.Left;

	private int radius = 6;

	private TShape shape;

	private TTypeMini type;

	private bool ghost;

	internal float ArrowProg = -1f;

	private bool showArrow;

	private bool isLink;

	public Color? Fore
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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

	public Color? BackHover { get; set; }

	public Color? BackActive { get; set; }

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
				OnPropertyChanged();
			}
		}
	}

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
					OnPropertyChanged();
				}
			}
		}
	}

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
					OnPropertyChanged();
				}
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged(layout: true);
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

	public Image? IconHover { get; set; }

	public string? IconHoverSvg { get; set; }

	public int IconHoverAnimation { get; set; } = 200;


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
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged();
			}
		}
	}

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
				OnPropertyChanged(layout: true);
			}
		}
	}

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
				if (showArrow)
				{
					OnPropertyChanged();
				}
			}
		}
	}

	internal override bool ExtraMouseHover
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
			Color _back_hover;
			switch (Type)
			{
			case TTypeMini.Default:
				if (BorderWidth > 0f)
				{
					_back_hover = Colour.PrimaryHover.Get("Button");
				}
				else
				{
					_back_hover = Colour.FillSecondary.Get("Button");
				}
				break;
			case TTypeMini.Success:
				_back_hover = Colour.SuccessHover.Get("Button");
				break;
			case TTypeMini.Error:
				_back_hover = Colour.ErrorHover.Get("Button");
				break;
			case TTypeMini.Info:
				_back_hover = Colour.InfoHover.Get("Button");
				break;
			case TTypeMini.Warn:
				_back_hover = Colour.WarningHover.Get("Button");
				break;
			default:
				_back_hover = Colour.PrimaryHover.Get("Button");
				break;
			}
			if (BackHover.HasValue)
			{
				_back_hover = BackHover.Value;
			}
			if (Config.Animation)
			{
				if (IconHoverAnimation > 0 && HasIcon && (IconHoverSvg != null || IconHover != null))
				{
					ThreadImageHover?.Dispose();
					AnimationImageHover = true;
					int t = Animation.TotalFrames(10, IconHoverAnimation);
					if (value)
					{
						ThreadImageHover = new ITask(delegate(int i)
						{
							AnimationImageHoverValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
							OnPropertyChanged();
							return true;
						}, 10, t, delegate
						{
							AnimationImageHoverValue = 1f;
							AnimationImageHover = false;
							OnPropertyChanged();
						});
					}
					else
					{
						ThreadImageHover = new ITask(delegate(int i)
						{
							AnimationImageHoverValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							OnPropertyChanged();
							return true;
						}, 10, t, delegate
						{
							AnimationImageHoverValue = 0f;
							AnimationImageHover = false;
							OnPropertyChanged();
						});
					}
				}
				if (_back_hover.A > 0)
				{
					int addvalue = _back_hover.A / 12;
					ThreadHover?.Dispose();
					AnimationHover = true;
					if (value)
					{
						ThreadHover = new ITask((Control)(object)base.PARENT.PARENT, delegate
						{
							AnimationHoverValue += addvalue;
							if (AnimationHoverValue > _back_hover.A)
							{
								AnimationHoverValue = _back_hover.A;
								return false;
							}
							OnPropertyChanged();
							return true;
						}, 10, delegate
						{
							AnimationHover = false;
							OnPropertyChanged();
						});
					}
					else
					{
						ThreadHover = new ITask((Control)(object)base.PARENT.PARENT, delegate
						{
							AnimationHoverValue -= addvalue;
							if (AnimationHoverValue < 1)
							{
								AnimationHoverValue = 0;
								return false;
							}
							OnPropertyChanged();
							return true;
						}, 10, delegate
						{
							AnimationHover = false;
							OnPropertyChanged();
						});
					}
				}
				else
				{
					AnimationHoverValue = _back_hover.A;
					OnPropertyChanged();
				}
			}
			else
			{
				AnimationHoverValue = _back_hover.A;
			}
			OnPropertyChanged();
		}
	}

	public CellButton(string id, string? text = null)
		: base(id, text)
	{
	}

	public CellButton(string id, string text, TTypeMini _type)
		: base(id, text)
	{
		type = _type;
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		Table.PaintButton(g, font, base.PARENT.PARENT.Gap, base.Rect, this, enable);
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		if (string.IsNullOrEmpty(base.Text))
		{
			int num = g.MeasureString("龍Qq", font).Height + gap;
			return new Size(num, num);
		}
		Size size = g.MeasureString(base.Text ?? "龍Qq", font);
		bool hasIcon = HasIcon;
		if (hasIcon || ShowArrow)
		{
			if (hasIcon && (IconPosition == TAlignMini.Top || IconPosition == TAlignMini.Bottom))
			{
				int num2 = (int)Math.Ceiling((float)size.Height * 1.2f);
				return new Size(size.Width + gap2 * 2 + num2, size.Height + gap + num2);
			}
			int height = size.Height + gap;
			if (hasIcon && ShowArrow)
			{
				return new Size(size.Width + gap2 + size.Height * 2, height);
			}
			if (hasIcon)
			{
				return new Size(size.Width + gap2 + (int)Math.Ceiling((float)size.Height * 1.2f), height);
			}
			return new Size(size.Width + gap2 + (int)Math.Ceiling((float)size.Height * 0.8f), height);
		}
		return new Size(size.Width + gap, size.Height + gap);
	}

	internal override void Click()
	{
		if (!_mouseDown || !Config.Animation)
		{
			return;
		}
		ThreadClick?.Dispose();
		AnimationClickValue = 0f;
		AnimationClick = true;
		ThreadClick = new ITask((Control)(object)base.PARENT.PARENT, delegate
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
			OnPropertyChanged();
			return true;
		}, 50, delegate
		{
			AnimationClick = false;
			OnPropertyChanged();
		});
	}
}
