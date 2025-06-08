using System;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI;

public static class FloatButton
{
	public class Config
	{
		public Form Form { get; set; }

		public Font? Font { get; set; }

		public Control? Control { get; set; }

		public TAlign Align { get; set; } = TAlign.BR;


		public bool Vertical { get; set; } = true;


		public bool TopMost { get; set; }

		public int Size { get; set; } = 40;


		public int MarginX { get; set; } = 24;


		public int MarginY { get; set; } = 24;


		public int Gap { get; set; } = 40;


		public ConfigBtn[] Btns { get; set; }

		public Action<ConfigBtn> Call { get; set; }

		public Config(Form form, ConfigBtn[] btns, Action<ConfigBtn> call)
		{
			Form = form;
			Btns = btns;
			Call = call;
		}
	}

	public class ConfigBtn : NotifyProperty, BadgeConfig
	{
		private bool enabled = true;

		private bool loading;

		internal int AnimationLoadingValue;

		private Color? fore;

		private Bitmap? icon;

		private string? iconSvg;

		private Size? iconSize;

		private string? text;

		private TTypeMini type;

		private bool round = true;

		private string? badge;

		private string? badgeSvg;

		private TAlignFrom badgeAlign = TAlignFrom.TR;

		private bool badgeMode;

		internal bool hover;

		internal Rectangle rect;

		internal Rectangle rect_read;

		internal Rectangle rect_icon;

		internal Bitmap? shadow_temp;

		public string Name { get; set; }

		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (enabled != value)
				{
					enabled = value;
					OnPropertyChanged("Enabled");
				}
			}
		}

		public bool Loading
		{
			get
			{
				return loading;
			}
			set
			{
				if (loading != value)
				{
					loading = value;
					OnPropertyChanged("Loading");
				}
			}
		}

		public float LoadingValue { get; set; } = 0.3f;


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
					OnPropertyChanged("Fore");
				}
			}
		}

		public Bitmap? Icon
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
					OnPropertyChanged("Icon");
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
					OnPropertyChanged("IconSvg");
				}
			}
		}

		public Size? IconSize
		{
			get
			{
				return iconSize;
			}
			set
			{
				if (!(iconSize == value))
				{
					iconSize = value;
					OnPropertyChanged("IconSize");
				}
			}
		}

		public string? Text
		{
			get
			{
				return Localization.GetLangI(LocalizationText, text, new string[2] { "{id}", Name });
			}
			set
			{
				if (!(text == value))
				{
					text = value;
					OnPropertyChanged("Text");
				}
			}
		}

		public string? LocalizationText { get; set; }

		public string? Tooltip { get; set; }

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
					OnPropertyChanged("Type");
				}
			}
		}

		public int Radius { get; set; } = 6;


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
					OnPropertyChanged("Round");
				}
			}
		}

		public string? Badge
		{
			get
			{
				return badge;
			}
			set
			{
				if (!(badge == value))
				{
					badge = value;
					OnPropertyChanged("Badge");
				}
			}
		}

		public string? BadgeSvg
		{
			get
			{
				return badgeSvg;
			}
			set
			{
				if (!(badgeSvg == value))
				{
					badgeSvg = value;
					OnPropertyChanged("Badge");
				}
			}
		}

		public TAlignFrom BadgeAlign
		{
			get
			{
				return badgeAlign;
			}
			set
			{
				if (badgeAlign != value)
				{
					badgeAlign = value;
					OnPropertyChanged("Badge");
				}
			}
		}

		public float BadgeSize { get; set; } = 0.6f;


		public bool BadgeMode
		{
			get
			{
				return badgeMode;
			}
			set
			{
				if (badgeMode != value)
				{
					badgeMode = value;
					OnPropertyChanged("BadgeMode");
				}
			}
		}

		public int BadgeOffsetX { get; set; }

		public int BadgeOffsetY { get; set; }

		public Color? BadgeBack { get; set; }

		public ConfigBtn(string name)
		{
			Name = name;
		}

		public ConfigBtn(string name, Bitmap icon)
		{
			Name = name;
			Icon = icon;
		}

		public ConfigBtn(string name, string text, bool isSVG = false)
		{
			Name = name;
			if (isSVG)
			{
				IconSvg = text;
			}
			else
			{
				Text = text;
			}
		}
	}

	public static FormFloatButton? open(Form form, ConfigBtn[] btns, Action<ConfigBtn> call)
	{
		return new Config(form, btns, call).open();
	}

	public static FormFloatButton? open(Form form, Control content, ConfigBtn[] btns, Action<ConfigBtn> call)
	{
		return new Config(form, btns, call)
		{
			Control = content
		}.open();
	}

	public static Config config(Form form, ConfigBtn[] btns, Action<ConfigBtn> call)
	{
		return new Config(form, btns, call);
	}

	public static Config config(Form form, Control content, ConfigBtn[] btns, Action<ConfigBtn> call)
	{
		return new Config(form, btns, call)
		{
			Control = content
		};
	}

	public static FormFloatButton? open(this Config config)
	{
		Config config2 = config;
		if (((Control)config2.Form).IsHandleCreated)
		{
			if (((Control)config2.Form).InvokeRequired)
			{
				FormFloatButton form = null;
				((Control)config2.Form).Invoke((Delegate)(Action)delegate
				{
					form = config2.open();
				});
				return form;
			}
			LayeredFormFloatButton layeredFormFloatButton = new LayeredFormFloatButton(config2);
			((Form)layeredFormFloatButton).Show((IWin32Window)(object)config2.Form);
			return layeredFormFloatButton;
		}
		return null;
	}
}
